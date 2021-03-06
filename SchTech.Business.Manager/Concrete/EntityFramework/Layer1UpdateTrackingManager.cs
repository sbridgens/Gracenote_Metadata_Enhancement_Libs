﻿using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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

        public void SetLayer1RequiresUpdate(Layer1UpdateTracking rowData, bool updateValue)
        {
            _layer1TrackingDal.SetLayer1RequiresUpdate(rowData, updateValue);
        }

        public Layer1UpdateTracking GetTrackingItemByUid(Guid ingestUuid)
        {
            return _layer1TrackingDal.GetTrackingItemByUid(ingestUuid);
        }

        public Layer1UpdateTracking GetTrackingItemByTmsId(string tmsId)
        {
            return _layer1TrackingDal.GetTrackingItemByTmsId(tmsId);
        }

        public List<Layer1UpdateTracking> GetTrackingItemByTmsIdAndRootId(string tmsId, string rootId)
        {
            return _layer1TrackingDal.GetTrackingItemByTmsIdAndRootId(tmsId, rootId);
        }

        public List<Layer1UpdateTracking> GetPackagesRequiringEnrichment()
        {
            return _layer1TrackingDal.GetPackagesRequiringEnrichment();
        }

        public long GetLastUpdateIdFromLatestUpdateIds()
        {
            return _layer1TrackingDal.GetLastUpdateIdFromLatestUpdateIds();
        }

        public string GetLowestUpdateIdFromLayer1UpdateTrackingTable()
        {
            return _layer1TrackingDal.GetLowestUpdateIdFromLayer1UpdateTrackingTable();
        }

        public string GetLowestUpdateIdFromMappingTrackingTable()
        {
            return _layer1TrackingDal.GetLowestUpdateIdFromMappingTrackingTable();
        }

        public void UpdateLayer1Data(Guid uuid, GnApiProgramsSchema.programsProgram programData, string nextUpdateId, string maxUpdateId)
        {
            _layer1TrackingDal.UpdateLayer1Data(uuid, programData, nextUpdateId, maxUpdateId);
        }
    }
}
