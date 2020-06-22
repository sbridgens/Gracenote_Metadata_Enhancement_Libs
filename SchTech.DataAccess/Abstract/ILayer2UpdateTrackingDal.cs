using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Collections.Generic;

namespace SchTech.DataAccess.Abstract
{
    public interface ILayer2UpdateTrackingDal : IEntityRepository<Layer2UpdateTracking>
    {
        void SetLayer2RequiresUpdate(Layer2UpdateTracking rowData, bool updateValue);

        Layer2UpdateTracking GetTrackingItemByUid(Guid ingestUuid);

        Layer2UpdateTracking GetTrackingItemByConnectorId(string connectorId);

        List<Layer2UpdateTracking> GetTrackingItemByConnectorIdAndRootId(string connectorId, string rootId);

        List<Layer2UpdateTracking> GetPackagesRequiringEnrichment();

        string GetLowestUpdateIdFromLayer2UpdateTrackingTable();

        string GetLowestUpdateIdFromMappingTrackingTable();

        long GetLastUpdateIdFromLatestUpdateIds();

        void UpdateLayer2Data(Guid uuid, GnApiProgramsSchema.programsProgram programData,
            string nextUpdateId, string maxUpdateId);
    }
}
