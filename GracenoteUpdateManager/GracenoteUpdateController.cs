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
        public List<GnApiProgramsSchema.programsProgram> ProgramDataUpdatesRequiredList { get; private set; }

        public static long NextMappingUpdateId { get; private set; }
        public static long MaxMappingUpdateId { get; private set; }
        public static long NextLayer1UpdateId { get; private set; }
        public static long MaxLayer1UpdateId { get; private set; }
        public static long NextLayer2UpdateId { get; private set; }
        public static long MaxLayer2UpdateId { get; private set; }
        
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
                //1: Get from lastupdate table
                //2: Get from tracking table
                //3: Use lowest mapping updateid.
                var updateId = _layer1TrackingService.GetLastUpdateIdFromLatestUpdateIds();

                if (updateId == 0)
                    updateId = Convert.ToInt64(_layer1TrackingService.GetLowestUpdateIdFromLayer1UpdateTrackingTable() ??
                                               _layer1TrackingService.GetLowestUpdateIdFromMappingTrackingTable());
                return updateId;
            }
            catch (Exception gmvex)
            {
                LogError("GetLowestLayer1UpdateId",
                    "Error Encountered Obtaining lowest db Layer1 Update ID", gmvex);
                return 0;
            }
        }

        public long GetLowestLayer2UpdateId()
        {
            return 0;
        }



        private void BuildLayer1UpdatesList()
        {
            ApiManager.UpdateProgramData =
                (from programs in ApiManager.CoreProgramData?.programs
                    let tmsId = programs.TMSId
                    where tmsId != null
                    select programs).ToList();
        }

        private void BuildLayer2UpdatesList()
        {
            ApiManager.UpdateProgramData =
                (from programs in ApiManager.CoreProgramData?.programs
                    let connectorId = programs.connectorId
                    where connectorId != null
                    select programs).ToList();
        }

        public bool GetGracenoteMappingData(string dbUpdateId, string apiLimit)
        {
            try
            {
                //Create a new List
                MappingsRequiringUpdate = new List<GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping>();
                

                //Call the Gn api with a limit of 1000 for mapping updates
                if (!WorkflowEntities.GetGraceNoteUpdates(dbUpdateId, "ProgramMappings",apiLimit))
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

        public bool GetGracenoteProgramUpdates(string dbUpdateId, string limit,  int layer)
        {
            try
            {
                //initialise a new programs list
                ProgramDataUpdatesRequiredList = new List<GnApiProgramsSchema.programsProgram>();

                //call the programs api with set limit
                if (!WorkflowEntities.GetGraceNoteUpdates(dbUpdateId, "Programs", limit))
                {
                    //no layer data returned check errors
                    Log.Error($"Package Layer{layer} data is Null cannot process package!");
                    return false;
                }

                //serialise returned gracenote program data
                var serializeMappingData = new XmlApiSerializationHelper<GnApiProgramsSchema.@on>();
                ApiManager.CoreProgramData = serializeMappingData.Read(WorkflowEntities.GraceNoteUpdateData);

                //store maxid as local variable in order to set correct layer data
                var maxid = ApiManager.CoreProgramData?.header.streamData.maxUpdateId ?? 0;
                Log.Info($"Max Update ID for Layer{layer}: {maxid}");
                //store nextid as local variable in order to set correct layer data
                var nextId = ApiManager.CoreProgramData?.header.streamData.nextUpdateId ?? 0;
                Log.Info($"Next Update ID for Layer{layer}: {nextId}");
                

                switch (layer)
                {
                    //build correct program lists based on layer id
                    case 1:
                        MaxLayer1UpdateId = maxid;
                        NextLayer1UpdateId = nextId;
                        if (nextId == 0)
                        {
                            Log.Info($"Workflow for Layer{layer} updates has reached the Maximum Update Id, Setting Next updateid to Maximum Id: {maxid}");
                            NextLayer1UpdateId = maxid;
                        }
                        BuildLayer1UpdatesList();
                        break;
                    default:
                        MaxLayer2UpdateId = maxid;
                        NextLayer2UpdateId = nextId;
                        if (nextId == 0)
                        {
                            Log.Info($"Workflow for Layer{layer} updates has reached the Maximum Update Id, Setting Next updateid to Maximum Id: {maxid}");
                            NextLayer2UpdateId = maxid;
                        }
                        BuildLayer2UpdatesList();
                        break;
                }

                ApiManager.CoreProgramData = null;
                foreach (var programData in ApiManager.UpdateProgramData)
                {
                    switch (layer)
                    {
                        case 1:
                            ParseLayer1Updates(programData);
                            break;
                        default:
                            ParseLayer2Updates(programData);
                            break;
                    }
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

        private void ParseLayer1Updates(GnApiProgramsSchema.programsProgram programData)
        {
            var programExistsInDb =
                _layer1TrackingService.GetTrackingItemByTmsIdAndRootId(programData.TMSId, programData.rootId);
            
            if(programExistsInDb == null)
                return;

            Log.Info($"Layer1 TMSID: {programData.TMSId} with RootId: {programData.rootId} EXISTS IN THE DB Requires Update, Update id: {programData.updateId}");
            ProgramDataUpdatesRequiredList.Add(programData);

            foreach (var row in programExistsInDb)
            {
                Log.Info($"Updating Layer1UpdateTracking Table with new Layer1 data for IngestUUID: { row.IngestUUID} and TmsID: {row.GN_TMSID}");
                _layer1TrackingService.UpdateLayer1Data(row.IngestUUID, programData, NextLayer1UpdateId.ToString(), MaxLayer1UpdateId.ToString());
            }
        }

        private void ParseLayer2Updates(GnApiProgramsSchema.programsProgram programData)
        {
            var programExistsInDb =
                _layer2TrackingService.GetTrackingItemByConnectorIdAndRootId(programData.connectorId,
                    programData.rootId);

            if(programExistsInDb == null)
                return;

            Log.Info($"Layer2 ConnectorId: {programData.connectorId} with RootId: {programData.rootId} EXISTS IN THE DB Requires Update, Update id: {programData.updateId}");
            ProgramDataUpdatesRequiredList.Add(programData);

            Log.Info($"Updating Layer2UpdateTracking Table with new Layer2 data for IngestUUID: { programExistsInDb.IngestUUID} and ConnectorId: {programExistsInDb.GN_connectorId}");
            _layer2TrackingService.UpdateLayer2Data(programExistsInDb.IngestUUID, programData, NextLayer2UpdateId.ToString(), MaxLayer2UpdateId.ToString());
        }

        public static int GetAdiVersionMinor()
        {
            return EnrichmentWorkflowEntities.UpdateAdi.Metadata.AMS.Version_Minor;
        }

        public static bool UpdateAllVersionMinorValues(int newVersionMinor)
        {
            try
            {
                //set main ams version minor
                EnrichmentWorkflowEntities.UpdateAdi.Metadata.AMS.Version_Minor = newVersionMinor;
                //set titl data ams version minor
                EnrichmentWorkflowEntities.UpdateAdi.Asset.Metadata.AMS.Version_Minor = newVersionMinor;

                //iterate any asset sections and update the version minor
                foreach (var item in EnrichmentWorkflowEntities.UpdateAdi.Asset.Asset.ToList()
                    .Where(item => item.Metadata.AMS.Version_Minor != newVersionMinor))
                    item.Metadata.AMS.Version_Minor = newVersionMinor;

                return true;
            }
            catch (Exception uavmvEx)
            {
                Log.Error("[UpdateAllVersionMinorValues] Error during update of version Minor" +
                          $": {uavmvEx.Message}");

                if (uavmvEx.InnerException != null)
                    Log.Error($"[UpdateAllVersionMinorValues] Inner Exception: {uavmvEx.InnerException.Message}");
                return false;
            }
        }
    }
}
