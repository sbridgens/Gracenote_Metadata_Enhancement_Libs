﻿using System;
using System.Collections.Generic;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface IMappingsUpdateTrackingDal : IEntityRepository<MappingsUpdateTracking>
    {
        MappingsUpdateTracking GetTrackingItemByUid(Guid ingestUuid);

        MappingsUpdateTracking GetTrackingItemByPidPaid(string gnProviderId);

        List<MappingsUpdateTracking> GetPackagesRequiringEnrichment();

        long GetLastUpdateIdFromLatestUpdateIds();

        string GetLowestUpdateIdFromMappingTable();

        string GetLowestUpdateIdFromMappingTrackingTable();

        void UpdateMappingData(Guid uuid, GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping mappingData, string nextUpdateId, string maxUpdateId);
    }
}
