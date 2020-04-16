using System;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface IMappingsUpdateTrackingDal : IEntityRepository<MappingsUpdateTracking>
    {
        MappingsUpdateTracking GetTrackingItemByUid(Guid ingestUuid);

        MappingsUpdateTracking GetTrackingItemByPidPaid(string gnProviderId);

        string GetLowestGnMappingDataUpdateId();

        string GetLowestTrackerMappingUpdateId();
        
    }
}
