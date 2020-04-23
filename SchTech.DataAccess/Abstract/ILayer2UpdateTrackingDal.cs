using System;
using System.Collections.Generic;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface ILayer2UpdateTrackingDal : IEntityRepository<Layer2UpdateTracking>
    {
        void SetLayer2RequiresUpdate(Layer2UpdateTracking rowData, bool updateValue);

        Layer2UpdateTracking GetTrackingItemByUid(Guid ingestUuid);

        Layer2UpdateTracking GetTrackingItemByConnectorId(string connectorId);

        Layer2UpdateTracking GetTrackingItemByConnectorIdAndRootId(string connectorId, string rootId);

        List<Layer2UpdateTracking> GetPackagesRequiringEnrichment();

        string GetLowestLayer2UpdateId();

        string GetLowestTrackerLayer2UpdateId();

        void UpdateLayer2Data(Guid uuid, GnApiProgramsSchema.programsProgram programData,
            string nextUpdateId, string maxUpdateId);
    }
}
