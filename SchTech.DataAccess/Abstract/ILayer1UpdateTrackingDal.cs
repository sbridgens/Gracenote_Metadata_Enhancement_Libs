using System;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface ILayer1UpdateTrackingDal : IEntityRepository<Layer1UpdateTracking>
    {
        Layer1UpdateTracking GetTrackingItemByUid(Guid ingestUuid);

        Layer1UpdateTracking GetTrackingItemByTmsId(string tmsId);

        string GetLowestLayer1UpdateId();

        string GetLowestTrackerLayer1UpdateId();
        
    }
}
