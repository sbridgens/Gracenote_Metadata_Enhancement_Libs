using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfLayer1UpdateTrackingDal : EfEntityRepositoryBase<Layer1UpdateTracking, ADI_EnrichmentContext>, ILayer1UpdateTrackingDal
    {            
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(EfLayer1UpdateTrackingDal));

        public void SetLayer1RequiresUpdate(Layer1UpdateTracking rowData, bool updateValue)
        {
            rowData.RequiresEnrichment = updateValue;
            Update(rowData);
        }

        public Layer1UpdateTracking GetTrackingItemByUid(Guid ingestUuid)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return Get(i => i.IngestUUID == ingestUuid);
            }
        }

        public Layer1UpdateTracking GetTrackingItemByTmsId(string tmsId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return Get(t => t.GN_TMSID == tmsId);
            }
        }

        public List<Layer1UpdateTracking> GetTrackingItemByTmsIdAndRootId(string tmsId, string rootId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var rowData = GetList(t => t.RequiresEnrichment == false && t.GN_TMSID == tmsId && t.Layer1_RootId == rootId);

                if (rowData.Count == 0)
                    return null;

                foreach (var row in rowData)
                {
                    if(row.RequiresEnrichment)
                        continue;
                    
                    var mapdata = mapContext.MappingsUpdateTracking.FirstOrDefault(m =>
                        m.IngestUUID == row.IngestUUID);

                    var layer2Data = mapContext.Layer2UpdateTracking.FirstOrDefault(l =>
                        l.IngestUUID == row.IngestUUID);

                    if (mapdata?.RequiresEnrichment == false)
                    {
                        if(layer2Data?.RequiresEnrichment == false)
                        {
                            SetLayer1RequiresUpdate(row, true);
                            //only return rowdata for items not requiring enrichment in the previous tables
                            return rowData;
                        }
                    }
                }

                return null;
            }
        }

        public List<Layer1UpdateTracking> GetPackagesRequiringEnrichment()
        {
            return GetList(r => r.RequiresEnrichment);
        }

        //primary check for last update
        public long GetLastUpdateIdFromLatestUpdateIds()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var val = mapContext.LatestUpdateIds.FirstOrDefault();
                Log.Debug($"[GetLastUpdateIdFromLatestUpdateIds] Returning value {val?.LastLayer1UpdateIdChecked ?? 0} from LatestUpdateIds LastLayer1UpdateIdChecked");
                return val?.LastLayer1UpdateIdChecked ?? 0;
            }
        }

        //fallback 1 for update id
        public string GetLowestUpdateIdFromLayer1UpdateTrackingTable()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.Layer1UpdateTracking.OrderBy(u => u.Layer1_UpdateId).First();
                Log.Debug($"[GetLowestUpdateIdFromLayer1UpdateTrackingTable] Fallback 1: Returning value {minVal.Layer1_UpdateId ?? "0"} from Layer1UpdateTracking");
                return minVal?.Layer1_UpdateId ?? "0";
            }
        }

        //final fallback for update id
        public string GetLowestUpdateIdFromMappingTrackingTable()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.MappingsUpdateTracking.OrderBy(u => u.Mapping_UpdateId).First();
                Log.Debug(
                    $"[GetLowestUpdateIdFromMappingTrackingTable] Fallback 2: Returning value {minVal.Mapping_UpdateId} from MappingsUpdateTracking");
                return minVal.Mapping_UpdateId;
            }
        }

        public void UpdateLayer1Data(Guid uuid, GnApiProgramsSchema.programsProgram programData, string nextUpdateId, string maxUpdateId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var rowData = Get(l1 => l1.IngestUUID == uuid);
                Log.Debug($"Updating Layer1 Update id with GN Value: {programData.updateId}");
                rowData.Layer1_UpdateId = programData.updateId;
                Log.Debug($"Updating Layer1 Update Date with GN Value: {programData.updateDate}");
                rowData.Layer1_UpdateDate = programData.updateDate;
                Log.Debug($"Updating Layer1 Next Update Id with GN Value: {nextUpdateId}");
                rowData.Layer1_NextUpdateId = nextUpdateId;
                Log.Debug($"Updating Layer1 Max Update Id with GN Value: {maxUpdateId}");
                rowData.Layer1_MaxUpdateId = maxUpdateId;
                rowData.UpdatesChecked = DateTime.Now;
                rowData.RequiresEnrichment = true;
                Update(rowData);
            }
        }
    }
}
