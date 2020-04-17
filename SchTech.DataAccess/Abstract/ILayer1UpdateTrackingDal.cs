using System;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface ILayer1UpdateTrackingDal : IEntityRepository<Layer1UpdateTracking>
    {
        void SetLayer1RequiresUpdate(Layer1UpdateTracking rowData, bool updateValue);

        Layer1UpdateTracking GetTrackingItemByUid(Guid ingestUuid);

        Layer1UpdateTracking GetTrackingItemByTmsId(string tmsId);

        Layer1UpdateTracking GetTrackingItemByTmsIdAndRootId(string tmsId, string rootId);

        string GetLowestLayer1UpdateId();

        string GetLowestTrackerLayer1UpdateId();
        
    }
}
