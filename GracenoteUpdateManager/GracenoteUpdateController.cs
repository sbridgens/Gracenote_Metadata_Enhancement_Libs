using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using SchTech.Api.Manager.GracenoteOnApi.Concrete;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Api.Manager.Serialization;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.Business.Manager.Concrete.EntityFramework;
using SchTech.DataAccess.Concrete.EntityFramework;
using SchTech.Entities.ConcreteTypes;


namespace GracenoteUpdateManager
{
    public class GracenoteUpdateController
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(GracenoteUpdateController));
        
        private EnrichmentWorkflowEntities WorkflowEntities { get; }
        private GraceNoteApiManager ApiManager { get; }

        private readonly IMappingsUpdateTrackingService _mappingsTrackerService;
        private readonly ILayer1UpdateTrackingService _layer1TrackingService;
        private readonly ILayer2UpdateTrackingService _layer2TrackingService;
        public List<GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping> MappingsRequiringUpdate { get; private set; }
        public List<GnApiProgramsSchema.programsProgram> Layer1DataUpdatesRequiredList { get; private set; }
        public List<GnApiProgramsSchema.programsProgram> Layer2DataUpdatesRequiredList { get; private set; }

        public static long NextMappingUpdateId { get; private set; }
        public static long MaxMappingUpdateId { get; private set; }

        /*TODO
            1: Create db types and mappings = Done
            2: Create logic to parse db and process data from [GN_UpdateTracking] table
            3: Call http://on-api.gracenote.com/v3/ProgramMappings?updateId=10938407543&limit=100&api_key=wgu7uhqcqyzspwxj28mxgy4b with lowest update id from point 2
            4: parse call results and grab all pidpaid items matching platform
            5: check db for pid paid values, if match update the tracker db row for update.
            6: call http://on-api.gracenote.com/v3/Programs?updateId=10938407543&limit=100&api_key=wgu7uhqcqyzspwxj28mxgy4b which returns layer1 & 2 data
            7: Parse call results and check rootids from 4 in tracking table against results
            8: Link episode with sho data for layer 2 calls
            9: any matches update the db row for update if an update has not been flagged
         */

        public GracenoteUpdateController()
        {
            _mappingsTrackerService = new MappingsUpdateTrackingManager(new EfMappingsUpdateTrackingDal());
            _layer1TrackingService = new Layer1UpdateTrackingManager(new EfLayer1UpdateTrackingDal());
            _layer2TrackingService = new Layer2UpdateTrackingManager(new EfLayer2UpdateTrackingDal());

            ApiManager = new GraceNoteApiManager();
            if(WorkflowEntities==null)
                WorkflowEntities = new EnrichmentWorkflowEntities();
            
        }

        private static void LogError(string functionName, string message, Exception ex)
        {
            Log.Error($"[{functionName}] {message}: {ex.Message}");
            if (ex.InnerException != null)
                Log.Error($"[{functionName}] Inner Exception:" +
                          $" {ex.InnerException.Message}");
        }

        public long GetLowestMappingUpdateId()
        {
            try
            {
                //Get from tracking table but fallback to mapping if there is no entry
                var updateId = _mappingsTrackerService.GetLastUpdateIdFromLatestUpdateIds();

                if(updateId == 0)
                       updateId = Convert.ToInt64(_mappingsTrackerService.GetLowestUpdateIdFromMappingTrackingTable() ??
                                                  _mappingsTrackerService.GetLowestUpdateIdFromMappingTable());

                return updateId;
            }
            catch (Exception gmvex)
            {
                LogError("GetLowestMappingValue",
                    "Error Encountered Obtaining lowest db mapping Update ID", gmvex);
                return 0;
            }
        }

        public long GetLowestLayer1UpdateId()
        {
            try
            {
                return Convert.ToInt64(_layer1TrackingService.GetLowestLayer1UpdateId());
            }
            catch (Exception gmvex)
            {
                LogError("GetLowestLayer1UpdateId",
                    "Error Encountered Obtaining lowest db Layer1 Update ID", gmvex);
                return 0;
            }
        }

        public bool GetGracenoteMappingData(string dbUpdateId)
        {
            try
            {
                //Create a new List
                MappingsRequiringUpdate = new List<GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping>();
                

                //Call the Gn api with a limit of 1000 for mapping updates
                if (!WorkflowEntities.GetGraceNoteUpdates(dbUpdateId, "ProgramMappings","1000"))
                {
                    Log.Info($"No Mapping Updates for UpdateId: {dbUpdateId}");
                    return false;
                }

                //Serialize the results set
                var serializeMappingData = new XmlApiSerializationHelper<GnOnApiProgramMappingSchema.@on>();
                ApiManager.CoreGnMappingData = serializeMappingData.Read(WorkflowEntities.GraceNoteUpdateData);

                //Obtain Next and Max update ids used for iteration and max workflow calls
                MaxMappingUpdateId = ApiManager.CoreGnMappingData?.header.streamData.maxUpdateId ?? 0;
                Log.Info($"Max Update ID: {MaxMappingUpdateId}");

                NextMappingUpdateId = ApiManager.CoreGnMappingData?.header.streamData.nextUpdateId ?? 0;
                Log.Info($"Next Update ID: {NextMappingUpdateId}");

                if (NextMappingUpdateId == 0)
                {
                    //No next id so we reached the max.
                    Log.Info($"Workflow for Mapping updates has reached the Maximum Update Id, Setting Next updateid to Maximum Id: {MaxMappingUpdateId}");
                    NextMappingUpdateId = MaxMappingUpdateId;
                }

                //Parse the mapping results and keep only the items that relate to the current ingest platform
                //only valid pid paid values starting with TITL belong to the current platform.
                ApiManager.UpdateMappingsData = 
                    (from mapping in ApiManager.CoreGnMappingData?.programMappings.programMapping
                        let paid = mapping.link.FirstOrDefault(t => t.idType.ToLower().Equals("paid"))
                        where paid != null
                        where paid.Value.ToLower().StartsWith("titl")
                        select mapping).ToList();
                
                //nullify the api results to cleanup resources
                ApiManager.CoreGnMappingData = null;

                //check if any of the filtered results are currently in the db if so they likely require an update
                foreach (var programMapping in ApiManager.UpdateMappingsData)
                {
                    //obtain the provider id value for checks on the db
                    var providerId = programMapping.link.FirstOrDefault(p => p.idType.ToLower().Equals("providerid"))?.Value;
                    //continue if null
                    if (string.IsNullOrEmpty(providerId))
                        continue;
                    //pid paid exists in the db based on providerid value
                    var exists = _mappingsTrackerService?.GetTrackingItemByPidPaid(providerId);
                    //continue if null
                    if (exists == null)
                        continue;

                    Log.Info($"Mapping PIDPAID: {providerId} EXISTS IN THE DB Requires Update, Update id: {programMapping.updateId}");
                    //update the the list for programs requiring adi updates
                    MappingsRequiringUpdate.Add(programMapping);

                    Log.Info($"Updating MappingsUpdateTracking Table with new mapping data for IngestUUID: {exists.IngestUUID} and PIDPAID: {exists.GN_ProviderId}");
                    //set the tracker service to flag the related asset as requiring an update.
                    //this flag will be used to trigger the adi creation service to generate a valid update against the correct ingestuuid.
                    
                    _mappingsTrackerService.UpdateMappingData(exists.IngestUUID, programMapping, NextMappingUpdateId.ToString(), MaxMappingUpdateId.ToString());
                }

                //mappings requiring updates finished being calculated and can now be used to generate adi updates
                return true;
            }
            catch (Exception ggmdex)
            {
                LogError(
                    "GetGracenoteMappingData",
                    "Error During Parsing of GetGracenote Mapping Data", ggmdex);
                return false;
            }
        }

        public bool GetGracenoteLayer1Updates(string dbUpdateId)
        {
            try
            {
                Layer1DataUpdatesRequiredList = new List<GnApiProgramsSchema.programsProgram>();


                if (!WorkflowEntities.GetGraceNoteUpdates(dbUpdateId, "Programs", "100"))
                {
                    Log.Error("Package Layer1 data is Null cannot process package!");
                    return false;
                }

                var serializeMappingData = new XmlApiSerializationHelper<GnApiProgramsSchema.@on>();
                ApiManager.CoreProgramData = serializeMappingData.Read(WorkflowEntities.GraceNoteUpdateData);

                var nextUpdateId = ApiManager.CoreProgramData?.header.streamData.nextUpdateId;
                var maxUpdateId = ApiManager.CoreProgramData?.header.streamData.maxUpdateId;


                ApiManager.UpdateProgramData =
                    (from programs in ApiManager.CoreProgramData?.programs
                     let tmsId = programs.TMSId
                     where tmsId != null
                     select programs).ToList();

                ApiManager.CoreProgramData = null;

                
                foreach (var programMapping in ApiManager.UpdateProgramData)
                {
                    var layer1Exists = _layer1TrackingService.GetTrackingItemByTmsIdAndRootId(programMapping.TMSId, programMapping.rootId);

                    if(layer1Exists == null)
                        continue;

                    Log.Info($"Layer1 TMSID: {programMapping.TMSId} with RootId: {programMapping.rootId} EXISTS IN THE DB Requires Update, Update id: {programMapping.updateId}");
                    Layer1DataUpdatesRequiredList.Add(programMapping);

                    Log.Info($"Updating Layer1UpdateTracking Table with new Layer1 data for IngestUUID: { layer1Exists.IngestUUID} and TmsID: {layer1Exists.GN_TMSID}");
                    _layer1TrackingService.UpdateLayer1Data(layer1Exists.IngestUUID, programMapping, nextUpdateId.ToString(), maxUpdateId.ToString());
                }

                //mappings requiring updates calculated and can be used to generate adi updates
                return true;
            }
            catch (Exception ggl1Uex)
            {
                LogError(
                    "GetGracenoteLayer1Updates",
                    "Error During Parsing of GetGracenote Layer1 Data", ggl1Uex);
                return false;
            }
        }

        public bool GetGracenoteLayer2Updates(string dbUpdateId)
        {
            try
            {
                Layer2DataUpdatesRequiredList = new List<GnApiProgramsSchema.programsProgram>();


                if (!WorkflowEntities.GetGraceNoteUpdates(dbUpdateId, "Programs", "100"))
                {
                    Log.Error("Package Layer2 data is Null cannot process package!");
                    return false;
                }

                var serializeMappingData = new XmlApiSerializationHelper<GnApiProgramsSchema.@on>();
                ApiManager.CoreProgramData = serializeMappingData.Read(WorkflowEntities.GraceNoteUpdateData);

                var nextUpdateId = ApiManager.CoreProgramData?.header.streamData.nextUpdateId;
                var maxUpdateId = ApiManager.CoreProgramData?.header.streamData.maxUpdateId;

                ApiManager.UpdateProgramData =
                    (from programs in ApiManager.CoreProgramData?.programs
                     let tmsId = programs.TMSId
                     where tmsId != null
                     select programs).ToList();

                ApiManager.CoreProgramData = null;


                foreach (var programMapping in ApiManager.UpdateProgramData)
                {
                    var layer2Exists =
                        _layer2TrackingService.GetTrackingItemByConnectorIdAndRootId(programMapping.connectorId, programMapping.rootId);

                    if (layer2Exists == null)
                        continue;

                    Log.Info($"Layer1 TMSID: {programMapping.TMSId} with RootId: {programMapping.rootId} EXISTS IN THE DB Requires Update, Update id: {programMapping.updateId}");
                    Layer2DataUpdatesRequiredList.Add(programMapping);

                    Log.Info($"Updating Layer2UpdateTracking Table with new Layer2 data for IngestUUID: { layer2Exists.IngestUUID} and ConnectorId: {layer2Exists.GN_connectorId}");
                    _layer2TrackingService.UpdateLayer2Data(layer2Exists.IngestUUID, programMapping, nextUpdateId.ToString(), maxUpdateId.ToString());
                }

                //mappings requiring updates calculated and can be used to generate adi updates
                return true;
            }
            catch (Exception ggl2Uex)
            {
                LogError(
                    "GetGracenoteLayer2Updates",
                    "Error During Parsing of GetGracenote Layer2 Data", ggl2Uex);
                return false;
            }
        }
    }
}
