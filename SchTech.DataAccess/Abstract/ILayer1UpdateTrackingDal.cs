using System;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
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

        long GetLastUpdateIdFromLatestUpdateIds();

        string GetLowestUpdateIdFromLayer1UpdateTrackingTable();

        string GetLowestUpdateIdFromMappingTrackingTable();

        void UpdateLayer1Data(Guid uuid, GnApiProgramsSchema.programsProgram programData,
            string nextUpdateId, string maxUpdateId);
    }
}
