using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Remotion.Linq.Clauses.ResultOperators;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfMappingsUpdateTrackingDal : EfEntityRepositoryBase<MappingsUpdateTracking, ADI_EnrichmentContext>, IMappingsUpdateTrackingDal
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(EfMappingsUpdateTrackingDal));

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
                var rowData = Get(t => t.RequiresEnrichment == false && t.GN_ProviderId == gnProviderId);

                if (rowData == null)
                    return null;

                var layer1Data = mapContext.Layer1UpdateTracking.FirstOrDefault(l1 => l1.IngestUUID == rowData.IngestUUID);

                var layer2Data = mapContext.Layer2UpdateTracking.FirstOrDefault(l2 => l2.IngestUUID == rowData.IngestUUID);

                if (layer1Data?.RequiresEnrichment == false)
                {
                    if(layer2Data?.RequiresEnrichment == false)
                    {
                        return rowData;
                    }
                }

                return null;
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
                Log.Debug($"Updating Mapping Update id with GN Value: {mappingData.updateId}");
                rowdata.Mapping_UpdateId = mappingData.updateId;
                Log.Debug($"Updating Mapping Update Date with GN Value: {mappingData.updateDate}");
                rowdata.Mapping_UpdateDate = mappingData.updateDate;
                Log.Debug($"Updating Mapping Next Update Id with GN Value: {nextUpdateId}");
                rowdata.Mapping_NextUpdateId = nextUpdateId;
                Log.Debug($"Updating Mapping Max Update Id with GN Value: {maxUpdateId}");
                rowdata.Mapping_MaxUpdateId = maxUpdateId;
                rowdata.UpdatesChecked = DateTime.Now;
                rowdata.RequiresEnrichment = true;
                Update(rowdata);
            }
        }
    }
}