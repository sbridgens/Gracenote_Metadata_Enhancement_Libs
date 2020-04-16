using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.Business.Manager.Concrete.EntityFramework
{
    public class Layer1UpdateTrackingManager : ILayer1UpdateTrackingService
    {
        private readonly ILayer1UpdateTrackingDal _layer1TrackingDal;

        public Layer1UpdateTrackingManager(ILayer1UpdateTrackingDal layer1UpdateTrackingDal)
        {
            _layer1TrackingDal = layer1UpdateTrackingDal;
        }

        public List<Layer1UpdateTracking> GetList(Expression<Func<Layer1UpdateTracking, bool>> filter = null)
        {
            return _layer1TrackingDal.GetList(filter);
        }

        public Layer1UpdateTracking Get(Expression<Func<Layer1UpdateTracking, bool>> filter)
        {
            return _layer1TrackingDal.Get(filter);
        }

        public Layer1UpdateTracking Add(Layer1UpdateTracking entity)
        {
            return _layer1TrackingDal.Add(entity);
        }

        public Layer1UpdateTracking Update(Layer1UpdateTracking entity)
        {
            return _layer1TrackingDal.Update(entity);
        }

        public Layer1UpdateTracking Delete(Layer1UpdateTracking entity)
        {
            return _layer1TrackingDal.Delete(entity);
        }

        public Layer1UpdateTracking GetTrackingItemByUid(Guid ingestUuid)
        {
            return _layer1TrackingDal.GetTrackingItemByUid(ingestUuid);
        }

        public Layer1UpdateTracking GetTrackingItemByTmsId(string tmsId)
        {
            return _layer1TrackingDal.GetTrackingItemByTmsId(tmsId);
        }

        public string GetLowestLayer1UpdateId()
        {
            return _layer1TrackingDal.GetLowestLayer1UpdateId();
        }
        
        public string GetLowestTrackerLayer1UpdateId()
        {
            return _layer1TrackingDal.GetLowestTrackerLayer1UpdateId();
        }
        
        
    }
}
