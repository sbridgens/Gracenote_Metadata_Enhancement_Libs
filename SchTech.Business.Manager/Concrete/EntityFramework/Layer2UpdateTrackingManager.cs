using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.Business.Manager.Concrete.EntityFramework
{
    public class Layer2UpdateTrackingManager : ILayer2UpdateTrackingService
    {
        private readonly ILayer2UpdateTrackingDal _layer2TrackingDal;

        public Layer2UpdateTrackingManager(ILayer2UpdateTrackingDal layer2UpdateTrackingDal)
        {
            _layer2TrackingDal = layer2UpdateTrackingDal;
        }

        public List<Layer2UpdateTracking> GetList(Expression<Func<Layer2UpdateTracking, bool>> filter = null)
        {
            return _layer2TrackingDal.GetList(filter);
        }

        public Layer2UpdateTracking Get(Expression<Func<Layer2UpdateTracking, bool>> filter)
        {
            return _layer2TrackingDal.Get(filter);
        }

        public Layer2UpdateTracking Add(Layer2UpdateTracking entity)
        {
            return _layer2TrackingDal.Add(entity);
        }

        public Layer2UpdateTracking Update(Layer2UpdateTracking entity)
        {
            return _layer2TrackingDal.Update(entity);
        }

        public Layer2UpdateTracking Delete(Layer2UpdateTracking entity)
        {
            return _layer2TrackingDal.Delete(entity);
        }

        public void SetLayer2RequiresUpdate(Layer2UpdateTracking rowData, bool updateValue)
        {
            _layer2TrackingDal.SetLayer2RequiresUpdate(rowData, updateValue);
        }

        public Layer2UpdateTracking GetTrackingItemByUid(Guid ingestUuid)
        {
            return _layer2TrackingDal.GetTrackingItemByUid(ingestUuid);
        }

        public Layer2UpdateTracking GetTrackingItemByConnectorId(string connectorId)
        {
            return _layer2TrackingDal.GetTrackingItemByConnectorId(connectorId);
        }

        public Layer2UpdateTracking GetTrackingItemByConnectorIdAndRootId(string connectorId, string rootId)
        {
            return _layer2TrackingDal.GetTrackingItemByConnectorIdAndRootId(connectorId, rootId);
        }

        public List<Layer2UpdateTracking> GetPackagesRequiringEnrichment()
        {
            return _layer2TrackingDal.GetPackagesRequiringEnrichment();
        }

        public string GetLowestLayer2UpdateId()
        {
            return _layer2TrackingDal.GetLowestLayer2UpdateId();
        }
        
        public string GetLowestTrackerLayer2UpdateId()
        {
            return _layer2TrackingDal.GetLowestTrackerLayer2UpdateId();
        }

        public void UpdateLayer2Data(Guid uuid, GnApiProgramsSchema.programsProgram programData, string nextUpdateId, string maxUpdateId)
        {
            _layer2TrackingDal.UpdateLayer2Data(uuid, programData, nextUpdateId, maxUpdateId);
        }
    }
}
