using System;
using System.Linq;
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

                if (mapContext.MappingsUpdateTracking.FirstOrDefault(m => m.Mapping_RootId == rootId && m.RequiresEnrichment == false) == null)
                {
                    return rowData;
                }

                SetLayer1RequiresUpdate(rowData,true);
                return null;
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

        public string GetLowestTrackerLayer1UpdateId()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.MappingsUpdateTracking.OrderBy(u => u.Mapping_UpdateId).First();
                return minVal.Mapping_UpdateId;
            }
        }
    }
}
