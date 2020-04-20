using System;
using System.Linq;
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

        public Layer2UpdateTracking GetTrackingItemByConnectorIdAndRootId(string connectorId, string rootId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var rowData = Get(t => t.GN_connectorId == connectorId && t.Layer2_RootId == rootId && t.RequiresEnrichment == false);

                if (rowData == null)
                    return null;

                var mappingFalse = mapContext.MappingsUpdateTracking.FirstOrDefault(m =>
                    m.IngestUUID == rowData.IngestUUID &&
                    m.RequiresEnrichment == false);

                var layer1False = mapContext.Layer1UpdateTracking.FirstOrDefault(l1 =>
                    l1.IngestUUID == rowData.IngestUUID &&
                    l1.RequiresEnrichment == false);

                if (layer1False != null)
                {
                    layer1False.RequiresEnrichment = true;
                    mapContext.SaveChanges();
                }

                if (mappingFalse != null)
                {
                    mappingFalse.RequiresEnrichment = true;
                    mapContext.SaveChanges();
                }


                SetLayer2RequiresUpdate(rowData, true);
                return rowData;
            }
        }

        public string GetLowestLayer2UpdateId()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.Layer1UpdateTracking.OrderBy(u => u.Layer1_UpdateId).First();
                return minVal.Layer1_UpdateId;
            }
        }

        public string GetLowestTrackerLayer2UpdateId()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.MappingsUpdateTracking.OrderBy(u => u.Mapping_UpdateId).First();
                return minVal.Mapping_UpdateId;
            }
        }
    }
}
