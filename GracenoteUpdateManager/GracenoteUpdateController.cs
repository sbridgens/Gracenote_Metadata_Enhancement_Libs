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

        public string GetLowestMappingValue(bool checkMapping, bool checkLayer1)
        {
            try
            {
                return checkMapping
                    ? _mappingsTrackerService.GetLowestGnMappingDataUpdateId()
                    : (checkLayer1
                        ? _layer1TrackingService.GetLowestLayer1UpdateId()
                        : _layer2TrackingService.GetLowestLayer2UpdateId());
            }
            catch (Exception gmvex)
            {
                LogError("GetLowestMappingValue",
                    "Error Encountered Obtaining lowest db mapping value", gmvex);
                return string.Empty;
            }
        }

        public bool GetGracenoteMappingData(string dbUpdateId)
        {
            try
            {
                MappingsRequiringUpdate = new List<GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping>();
                


                if (!WorkflowEntities.GetGraceNoteUpdates(dbUpdateId, "ProgramMappings","1000"))
                {
                    Log.Error("Package Mapping data is Null cannot process package!");
                    return false;
                }

                var serializeMappingData = new XmlApiSerializationHelper<GnOnApiProgramMappingSchema.@on>();
                ApiManager.CoreGnMappingData = serializeMappingData.Read(WorkflowEntities.GraceNoteUpdateData);

                var nextUpdateId = ApiManager.CoreGnMappingData?.header.streamData.nextUpdateId;
                var maxUpdateId = ApiManager.CoreGnMappingData?.header.streamData.maxUpdateId;

                ApiManager.UpdateMappingsData = 
                    (from mapping in ApiManager.CoreGnMappingData?.programMappings.programMapping
                        let paid = mapping.link.FirstOrDefault(t => t.idType.ToLower().Equals("paid"))
                        where paid != null
                        where paid.Value.ToLower().StartsWith("titl")
                        select mapping).ToList();
                
                ApiManager.CoreGnMappingData = null;

                //check if any of the above are in the db?
                foreach (var programMapping in ApiManager.UpdateMappingsData)
                {
                    var providerId = programMapping.link.FirstOrDefault(p => p.idType.ToLower().Equals("providerid"))?.Value;

                    if (string.IsNullOrEmpty(providerId))
                        continue;
                    var exists = _mappingsTrackerService?.GetTrackingItemByPidPaid(providerId);

                    if (exists == null)
                        continue;

                    Log.Info($"Mapping PIDPAID: {providerId} EXISTS IN THE DB Requires Update, Update id: {programMapping.updateId}");
                    MappingsRequiringUpdate.Add(programMapping);

                    Log.Info($"Updating MappingsUpdateTracking Table with new mapping data for IngestUUID: {exists.IngestUUID} and PIDPAID: {exists.GN_ProviderId}");
                    _mappingsTrackerService.UpdateMappingData(exists.IngestUUID, programMapping, nextUpdateId.ToString(), maxUpdateId.ToString());
                }

                //mappings requiring updates calculated and can be used to generate adi updates
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
                
                ApiManager.UpdateProgramData =
                    (from programs in ApiManager.CoreProgramData.programs
                     let tmsId = programs.TMSId
                     where tmsId != null
                     select programs).ToList();

                ApiManager.CoreProgramData = null;
                

                //check if any of the above are in the db?
                foreach (var programMapping in from programMapping 
                        in ApiManager.UpdateProgramData
                        let layer1Exists = _layer1TrackingService.GetTrackingItemByTmsIdAndRootId(programMapping.TMSId, programMapping.rootId)
                        where layer1Exists != null
                        select programMapping)
                {
                    Log.Info($"Layer1 TMSID: {programMapping.TMSId} with RootId: {programMapping.rootId} EXISTS IN THE DB Requires Update, Update id: {programMapping.updateId}");
                    Layer1DataUpdatesRequiredList.Add(programMapping);
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

                ApiManager.UpdateProgramData =
                    (from programs in ApiManager.CoreProgramData.programs
                     let tmsId = programs.TMSId
                     where tmsId != null
                     select programs).ToList();

                ApiManager.CoreProgramData = null;


                //check if any of the above are in the db?
                foreach (var programMapping in from programMapping
                        in ApiManager.UpdateProgramData
                                               let layer2Exists = _layer2TrackingService.GetTrackingItemByConnectorIdAndRootId(programMapping.connectorId, programMapping.rootId)
                                               where layer2Exists != null
                                               select programMapping)
                {
                    Log.Info($"Layer2 ConnectorId: {programMapping.connectorId} with RootId: {programMapping.rootId} EXISTS IN THE DB Requires Update, Update id: {programMapping.updateId}");
                    Layer2DataUpdatesRequiredList.Add(programMapping);
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
