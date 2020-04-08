﻿using System;
using System.Runtime.InteropServices;
using log4net;
using SchTech.Api.Manager.GracenoteOnApi.Concrete;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Api.Manager.Serialization;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.Business.Manager.Concrete.EntityFramework;
using SchTech.DataAccess.Concrete.EntityFramework;
using SchTech.Entities.ConcreteTypes;
using SchTech.Web.Manager.Concrete;

namespace GracenoteUpdateManager
{
    public class GracenoteUpdateController
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(GracenoteUpdateController));

        private GN_Mapping_Data GnMappingData { get; set; }
        private EnrichmentWorkflowEntities WorkflowEntities { get; }
        private GraceNoteApiManager ApiManager { get; }

        private readonly IGnUpdateTrackerService _gnUpdateTrackerService;
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
            _gnUpdateTrackerService = new GnUpdateTrackerManager(new EfGnUpdateTrackerDal());
            ApiManager = new GraceNoteApiManager();
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
                    ? _gnUpdateTrackerService.GetLowestMappingUpdateId()
                    : (checkLayer1
                        ? _gnUpdateTrackerService.GetLowestLayer1UpdateId()
                        : _gnUpdateTrackerService.GetLowestLayer2UpdateId());
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


                return true;
            }
            catch (Exception ggmd)
            {
                LogError("GetGracenoteMappingData",
                    $"Error Encountered Obtaining Gracenote Mapping for update Id: {dbUpdateId}", ggmd);
                return false;
            }
        }

    }
}
