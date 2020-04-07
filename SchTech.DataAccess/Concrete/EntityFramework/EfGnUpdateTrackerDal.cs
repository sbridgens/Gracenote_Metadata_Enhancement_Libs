using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfGnUpdateTrackerDal : EfEntityRepositoryBase<GN_UpdateTracking, ADI_EnrichmentContext>, IGnUpdateTrackingDal
    {
        public GN_UpdateTracking GetTrackingItemByUid(Guid ingestUUID)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return mapContext.GN_UpdateTracking.FirstOrDefault(
                    i => i.TrackingIngestGuid == ingestUUID);
            }
        }

        public GN_UpdateTracking GetTrackingItemByPidPaid(string gnProviderId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return mapContext.GN_UpdateTracking.FirstOrDefault(
                    t => t.TrackingGnProviderId == gnProviderId);
            }
        }
    }
}
