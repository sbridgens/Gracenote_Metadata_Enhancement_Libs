using System;
using System.Linq;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfGnUpdateTrackerDal : EfEntityRepositoryBase<GN_UpdateTracking, ADI_EnrichmentContext>, IGnUpdateTrackingDal
    {
        public GN_UpdateTracking GetTrackingItemByUid(Guid ingestUuid)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return mapContext.GN_UpdateTracking.FirstOrDefault(
                    i => i.IngestUUID == ingestUuid);
            }
        }

        public GN_UpdateTracking GetTrackingItemByPidPaid(string gnProviderId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return mapContext.GN_UpdateTracking.FirstOrDefault(
                    t => t.GN_ProviderId == gnProviderId);
            }
        }

        public string GetLowestGnMappingDataUpdateId()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.GN_Mapping_Data.OrderBy(u => u.GN_updateId).First();
                return minVal.GN_updateId;
            }
        }

        public string GetLowestTrackerMappingUpdateId()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.GN_UpdateTracking.OrderBy(u => u.Mapping_UpdateId).First();
                return minVal.Mapping_UpdateId;
            }
        }

        public string GetLowestLayer1UpdateId()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.GN_UpdateTracking.OrderBy(u => u.Layer1_UpdateId).First();
                return minVal.Layer1_UpdateId;
            }
        }

        public string GetLowestLayer2UpdateId()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.GN_UpdateTracking.OrderBy(u => u.Layer2_UpdateId).First();
                return minVal.Layer2_UpdateId;
            }
        }
    }
}
