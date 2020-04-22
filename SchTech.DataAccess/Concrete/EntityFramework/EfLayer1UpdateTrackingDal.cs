using System;
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

        public Layer1UpdateTracking GetTrackingItemByTmsIdAndRootId(string tmsId, string rootId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var rowData = Get(t => t.GN_TMSID == tmsId && t.Layer1_RootId == rootId && t.RequiresEnrichment == false);

                if (rowData == null)
                    return null;

                var mappingFalse = mapContext.MappingsUpdateTracking.FirstOrDefault(m =>
                    m.IngestUUID == rowData.IngestUUID &&
                    m.RequiresEnrichment == false);

                var l2data = mapContext.Layer1UpdateTracking.FirstOrDefault(l1 =>
                    l1.IngestUUID == rowData.IngestUUID &&
                    l1.RequiresEnrichment == false);

                if (mappingFalse != null)
                {
                    mappingFalse.RequiresEnrichment = true;
                    mapContext.SaveChanges();
                }

                if (l2data != null)
                {
                    l2data.RequiresEnrichment = true;
                    mapContext.SaveChanges();
                }

                SetLayer1RequiresUpdate(rowData, true);
                return rowData;

            }
        }

        public string GetLowestLayer1UpdateId()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.Layer1UpdateTracking.OrderBy(u => u.Layer1_UpdateId).First();
                return minVal.Layer1_UpdateId;
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
