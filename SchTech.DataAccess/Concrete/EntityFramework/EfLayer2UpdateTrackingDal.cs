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
    public class EfLayer2UpdateTrackingDal : EfEntityRepositoryBase<Layer2UpdateTracking, ADI_EnrichmentContext>, ILayer2UpdateTrackingDal
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(EfLayer2UpdateTrackingDal));

        public void SetLayer2RequiresUpdate(Layer2UpdateTracking rowData, bool updateValue)
        {
            rowData.RequiresEnrichment = updateValue;
            Update(rowData);
        }

        public Layer2UpdateTracking GetTrackingItemByUid(Guid ingestUuid)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return Get(i => i.IngestUUID == ingestUuid);
            }
        }

        public Layer2UpdateTracking GetTrackingItemByConnectorId(string connectorId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return Get(t => t.GN_connectorId == connectorId);
            }
        }

        public List<Layer2UpdateTracking> GetTrackingItemByConnectorIdAndRootId(string connectorId, string rootId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var rowData = GetList(t => t.GN_connectorId == connectorId && 
                                            t.Layer2_RootId == rootId && 
                                            t.RequiresEnrichment == false);

                if (rowData.Count == 0)
                    return null;

                foreach (var row in rowData)
                {
                    var mapdata = mapContext.MappingsUpdateTracking.FirstOrDefault(m =>
                        m.IngestUUID == row.IngestUUID);

                    var layer1Data = mapContext.Layer1UpdateTracking.FirstOrDefault(l =>
                        l.IngestUUID == row.IngestUUID);

                    if(mapdata?.RequiresEnrichment == false)
                    {
                        if(layer1Data?.RequiresEnrichment == false)
                        {
                            SetLayer2RequiresUpdate(row, true);
                            //only return rowdata for items not requiring enrichment in the previous tables
                            return rowData;
                        }
                    }
                }

                return null;
            }
        }

        public List<Layer2UpdateTracking> GetPackagesRequiringEnrichment()
        {
            return GetList(r => r.RequiresEnrichment);
        }

        //primary check for last update
        public long GetLastUpdateIdFromLatestUpdateIds()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var val = mapContext.LatestUpdateIds.FirstOrDefault();
                Log.Debug($"[GetLastUpdateIdFromLatestUpdateIds] Returning value {val?.LastLayer2UpdateIdChecked ?? 0} from LatestUpdateIds LastLayer2UpdateIdChecked");
                return val?.LastLayer2UpdateIdChecked ?? 0;
            }
        }

        //fallback 1 for update id
        public string GetLowestUpdateIdFromLayer2UpdateTrackingTable()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.Layer2UpdateTracking.OrderBy(u => u.Layer2_UpdateId).First();
                Log.Debug($"[GetLowestUpdateIdFromLayer2UpdateTrackingTable] Fallback 1: Returning value {minVal.Layer2_UpdateId ?? "0"} from Layer2UpdateTracking");
                return minVal.Layer2_UpdateId ?? "0";
            }
        }

        //final fallback for update id
        public string GetLowestUpdateIdFromMappingTrackingTable()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.MappingsUpdateTracking.OrderBy(u => u.Mapping_UpdateId).First();
                Log.Debug(
                    $"[GetLowestUpdateIdFromMappingTrackingTable] Fallback 2: Returning value {minVal} from MappingsUpdateTracking");
                return minVal.Mapping_UpdateId;
            }
        }
        
        public void UpdateLayer2Data(Guid uuid, GnApiProgramsSchema.programsProgram programData, string nextUpdateId, string maxUpdateId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var rowData = Get(l2 => l2.IngestUUID == uuid);
                Log.Debug($"Updating Layer2 Update id with GN Value: {programData.updateId}");
                rowData.Layer2_UpdateId = programData.updateId;
                Log.Debug($"Updating Layer2 Update Date with GN Value: {programData.updateDate}");
                rowData.Layer2_UpdateDate = programData.updateDate;
                Log.Debug($"Updating Layer2 Next Update Id with GN Value: {nextUpdateId}");
                rowData.Layer2_NextUpdateId = nextUpdateId;
                Log.Debug($"Updating Layer2 Max Update Id with GN Value: {maxUpdateId}");
                rowData.Layer2_MaxUpdateId = maxUpdateId;
                rowData.UpdatesChecked = DateTime.Now;
                rowData.RequiresEnrichment = true;
                Update(rowData);
            }
        }
    }
}
