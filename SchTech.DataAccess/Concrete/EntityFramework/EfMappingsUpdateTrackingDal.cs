using System;
using System.Collections.Generic;
using System.Linq;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfMappingsUpdateTrackingDal : EfEntityRepositoryBase<MappingsUpdateTracking, ADI_EnrichmentContext>, IMappingsUpdateTrackingDal
    {
        public MappingsUpdateTracking GetTrackingItemByUid(Guid ingestUuid)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return Get(i => i.IngestUUID == ingestUuid);
            }
        }

        public MappingsUpdateTracking GetTrackingItemByPidPaid(string gnProviderId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return Get(t => t.GN_ProviderId == gnProviderId && t.RequiresEnrichment == false);
            }
        }

        public List<MappingsUpdateTracking> GetPackagesRequiringEnrichment()
        {
            return GetList(r => r.RequiresEnrichment);
        }

        public long GetLastUpdateIdFromLatestUpdateIds()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var val = mapContext.LatestUpdateIds.FirstOrDefault();
                return val?.LastMappingUpdateIdChecked ?? 0;
            }
        }

        public string GetLowestUpdateIdFromMappingTrackingTable()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.MappingsUpdateTracking.OrderBy(u => u.Mapping_UpdateId).First();
                return minVal.Mapping_UpdateId;
            }
        }

        public string GetLowestUpdateIdFromMappingTable()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.GN_Mapping_Data.OrderBy(u => u.GN_updateId).First();
                return minVal.GN_updateId;
            }
        }


        public void UpdateMappingData(Guid uuid, GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping mappingData, string nextUpdateId, string maxUpdateId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var rowdata = Get(i => i.IngestUUID == uuid);
                rowdata.Mapping_UpdateId = mappingData.updateId;
                rowdata.Mapping_UpdateDate = DateTime.Now;
                rowdata.Mapping_NextUpdateId = nextUpdateId;
                rowdata.Mapping_MaxUpdateId = maxUpdateId;
                rowdata.UpdatesChecked = DateTime.Now;
                rowdata.RequiresEnrichment = true;
                Update(rowdata);
            }
        }
    }
}
