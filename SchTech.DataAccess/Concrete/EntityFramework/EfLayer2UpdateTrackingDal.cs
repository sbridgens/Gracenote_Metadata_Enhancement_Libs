using System;
using System.Collections.Generic;
using System.Linq;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfLayer2UpdateTrackingDal : EfEntityRepositoryBase<Layer2UpdateTracking, ADI_EnrichmentContext>, ILayer2UpdateTrackingDal
    {
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

                    if(mapdata?.RequiresEnrichment == false && layer1Data?.RequiresEnrichment == false)
                    {
                        SetLayer2RequiresUpdate(row, true);
                        //only return rowdata for items not requiring enrichment in the previous tables
                        return rowData;
                    }
                }

                return null;
            }
        }

        public List<Layer2UpdateTracking> GetPackagesRequiringEnrichment()
        {
            return GetList(r => r.RequiresEnrichment);
        }
        
        public string GetLowestUpdateIdFromLayer2UpdateTrackingTable()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.Layer2UpdateTracking.OrderBy(u => u.Layer2_UpdateId).First();
                return minVal.Layer2_UpdateId;
            }
        }

        public string GetLowestUpdateIdFromMappingTrackingTable()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.MappingsUpdateTracking.OrderBy(u => u.Mapping_UpdateId).First();
                return minVal.Mapping_UpdateId;
            }
        }

        public long GetLastUpdateIdFromLatestUpdateIds()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var val = mapContext.LatestUpdateIds.FirstOrDefault();
                return val?.LastLayer2UpdateIdChecked ?? 0;
            }
        }


        public void UpdateLayer2Data(Guid uuid, GnApiProgramsSchema.programsProgram programData, string nextUpdateId, string maxUpdateId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var rowData = Get(l2 => l2.IngestUUID == uuid);

                rowData.Layer2_UpdateId = programData.updateId;
                rowData.Layer2_NextUpdateId = nextUpdateId;
                rowData.Layer2_MaxUpdateId = maxUpdateId;
                rowData.UpdatesChecked = DateTime.Now;
                rowData.RequiresEnrichment = true;
                Update(rowData);
            }
        }
    }
}
