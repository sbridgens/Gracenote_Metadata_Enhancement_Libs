using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.Business.Manager.Concrete.EntityFramework
{
    public class MappingsUpdateTrackingManager : IMappingsUpdateTrackingService
    {
        private readonly IMappingsUpdateTrackingDal _updateTrackingDal;

        public MappingsUpdateTrackingManager(IMappingsUpdateTrackingDal updateTrackingDal)
        {
            _updateTrackingDal = updateTrackingDal;
        }

        public List<MappingsUpdateTracking> GetList(Expression<Func<MappingsUpdateTracking, bool>> filter = null)
        {
            return _updateTrackingDal.GetList(filter);
        }

        public MappingsUpdateTracking Get(Expression<Func<MappingsUpdateTracking, bool>> filter)
        {
            return _updateTrackingDal.Get(filter);
        }

        public MappingsUpdateTracking Add(MappingsUpdateTracking entity)
        {
            return _updateTrackingDal.Add(entity);
        }

        public MappingsUpdateTracking Update(MappingsUpdateTracking entity)
        {
            return _updateTrackingDal.Update(entity);
        }

        public MappingsUpdateTracking Delete(MappingsUpdateTracking entity)
        {
            return _updateTrackingDal.Delete(entity);
        }

        public MappingsUpdateTracking GetTrackingItemByUid(Guid ingestUuid)
        {
            return _updateTrackingDal.GetTrackingItemByUid(ingestUuid);
        }

        public MappingsUpdateTracking GetTrackingItemByPidPaid(string gnProviderId)
        {
            return _updateTrackingDal.GetTrackingItemByPidPaid(gnProviderId);
        }

        public long GetLastUpdateIdFromLatestUpdateIds()
        {
            return _updateTrackingDal.GetLastUpdateIdFromLatestUpdateIds();
        }

        public string GetLowestUpdateIdFromMappingTable()
        {
            return _updateTrackingDal.GetLowestUpdateIdFromMappingTable();
        }

        public string GetLowestUpdateIdFromMappingTrackingTable()
        {
            return _updateTrackingDal.GetLowestUpdateIdFromMappingTrackingTable();
        }

        public void UpdateMappingData(Guid uuid, GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping mappingData, string nextUpdateId, string maxUpdateId)
        {
            _updateTrackingDal.UpdateMappingData(uuid, mappingData, nextUpdateId, maxUpdateId);
        }
    }
}
