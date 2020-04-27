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
    public class EfLayer1UpdateTrackingDal : EfEntityRepositoryBase<Layer1UpdateTracking, ADI_EnrichmentContext>, ILayer1UpdateTrackingDal
    {
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
                var rowData = GetList(t => t.GN_TMSID == tmsId && t.Layer1_RootId == rootId && t.RequiresEnrichment == false);

                if (rowData.Count == 0)
                    return null;

                foreach (var row in rowData
                    .Select(row => new
                    {
                        row,
                        mapdata = mapContext.MappingsUpdateTracking.FirstOrDefault(m =>
                            m.IngestUUID == row.IngestUUID && m.RequiresEnrichment == false)
                    })
                    .Select(@t => new
                    {
                        @t,
                        l1data = mapContext.Layer2UpdateTracking.FirstOrDefault(l2 =>
                            l2.IngestUUID == @t.row.IngestUUID && l2.RequiresEnrichment == false)
                    })
                    .Where(@t => @t.@t.mapdata != null && @t.l1data != null)
                    .Select(@t => @t.@t.row))
                {
                    SetLayer1RequiresUpdate(row, true);
                }

                return rowData;

            }
        }

        public List<Layer1UpdateTracking> GetPackagesRequiringEnrichment()
        {
            return GetList(r => r.RequiresEnrichment);
        }

        public long GetLastUpdateIdFromLatestUpdateIds()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var val = mapContext.LatestUpdateIds.FirstOrDefault();
                return val?.LastLayer1UpdateIdChecked ?? 0;
            }
        }

        public string GetLowestUpdateIdFromLayer1UpdateTrackingTable()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.Layer1UpdateTracking.OrderBy(u => u.Layer1_UpdateId).First();
                return minVal.Layer1_UpdateId;
            }
        }

        public string GetLowestUpdateIdFromMappingTrackingTable()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.GN_Mapping_Data.OrderBy(u => u.GN_updateId).First();
                return minVal.GN_updateId;
            }
        }

        public void UpdateLayer1Data(Guid uuid, GnApiProgramsSchema.programsProgram programData, string nextUpdateId, string maxUpdateId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var rowData = Get(l1 => l1.IngestUUID == uuid);
                rowData.Layer1_UpdateId = programData.updateId;
                rowData.Layer1_NextUpdateId = nextUpdateId;
                rowData.Layer1_MaxUpdateId = maxUpdateId;
                rowData.UpdatesChecked = DateTime.Now;
                rowData.RequiresEnrichment = true;
                Update(rowData);
            }
        }
    }
}
