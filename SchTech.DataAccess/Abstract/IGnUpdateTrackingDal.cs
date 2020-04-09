using System;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface IGnUpdateTrackingDal : IEntityRepository<GN_UpdateTracking>
    {
        GN_UpdateTracking GetTrackingItemByUid(Guid ingestUuid);

        GN_UpdateTracking GetTrackingItemByPidPaid(string gnProviderId);

        string GetLowestGnMappingDataUpdateId();

        string GetLowestTrackerMappingUpdateId();

        string GetLowestLayer1UpdateId();

        string GetLowestLayer2UpdateId();
        
    }
}
