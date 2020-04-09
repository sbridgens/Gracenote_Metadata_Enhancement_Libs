using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.Business.Manager.Concrete.EntityFramework
{
    public class GnUpdateTrackerManager : IGnUpdateTrackerService
    {
        private readonly IGnUpdateTrackingDal _updateTrackingDal;

        public GnUpdateTrackerManager(IGnUpdateTrackingDal updateTrackingDal)
        {
            _updateTrackingDal = updateTrackingDal;
        }

        public List<GN_UpdateTracking> GetList(Expression<Func<GN_UpdateTracking, bool>> filter = null)
        {
            return _updateTrackingDal.GetList(filter);
        }

        public GN_UpdateTracking Get(Expression<Func<GN_UpdateTracking, bool>> filter)
        {
            return _updateTrackingDal.Get(filter);
        }

        public GN_UpdateTracking Add(GN_UpdateTracking entity)
        {
            return _updateTrackingDal.Add(entity);
        }

        public GN_UpdateTracking Update(GN_UpdateTracking entity)
        {
            return _updateTrackingDal.Update(entity);
        }

        public GN_UpdateTracking Delete(GN_UpdateTracking entity)
        {
            return _updateTrackingDal.Delete(entity);
        }

        public GN_UpdateTracking GetTrackingItemByUid(Guid ingestUuid)
        {
            return _updateTrackingDal.GetTrackingItemByUid(ingestUuid);
        }

        public GN_UpdateTracking GetTrackingItemByPidPaid(string gnProviderId)
        {
            return _updateTrackingDal.GetTrackingItemByPidPaid(gnProviderId);
        }

        public string GetLowestGnMappingDataUpdateId()
        {
            return _updateTrackingDal.GetLowestGnMappingDataUpdateId();
        }

        public string GetLowestTrackerMappingUpdateId()
        {
            return _updateTrackingDal.GetLowestGnMappingDataUpdateId();
        }

        public string GetLowestLayer1UpdateId()
        {
            return _updateTrackingDal.GetLowestLayer1UpdateId();
        }

        public string GetLowestLayer2UpdateId()
        {
            return _updateTrackingDal.GetLowestLayer2UpdateId();
        }
    }
}
