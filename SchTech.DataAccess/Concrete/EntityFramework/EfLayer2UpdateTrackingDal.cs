using System;
using System.Collections.Generic;
using System.Linq;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfLayer2UpdateTrackingDal : EfEntityRepositoryBase<Layer2UpdateTracking, ADI_EnrichmentContext>, ILayer2UpdateTrackingDal
    {
        public void SetLayer2RequiresUpdate(Layer2UpdateTracking rowData, bool updateValue)
        {
            rowData.RequiresEnrichment = updateValue;
            Update(rowData);
        }

        public Layer2UpdateTracking GetTrackingItemByUid(Guid ingestUuid)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return Get(i => i.IngestUUID == ingestUuid);
            }
        }

        public Layer2UpdateTracking GetTrackingItemByConnectorId(string connectorId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                return Get(t => t.GN_connectorId == connectorId);
            }
        }

        public Layer2UpdateTracking GetTrackingItemByConnectorIdAndRootId(string connectorId, string rootId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var rowData = Get(t => t.GN_connectorId == connectorId && t.Layer2_RootId == rootId && t.RequiresEnrichment == false);

                if (rowData == null)
                    return null;

                var mappingFalse = mapContext.MappingsUpdateTracking.FirstOrDefault(m =>
                    m.IngestUUID == rowData.IngestUUID &&
                    m.RequiresEnrichment == false);

                var layer1False = mapContext.Layer1UpdateTracking.FirstOrDefault(l1 =>
                    l1.IngestUUID == rowData.IngestUUID &&
                    l1.RequiresEnrichment == false);

                if (mappingFalse != null && layer1False != null)
                {
                    SetLayer2RequiresUpdate(rowData, true);
                }

                return rowData;
            }
        }

        public List<Layer2UpdateTracking> GetPackagesRequiringEnrichment()
        {
            return GetList(r => r.RequiresEnrichment);
        }

        public string GetLowestLayer2UpdateId()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.Layer1UpdateTracking.OrderBy(u => u.Layer1_UpdateId).First();
                return minVal.Layer1_UpdateId;
            }
        }

        public string GetLowestTrackerLayer2UpdateId()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var minVal = mapContext.MappingsUpdateTracking.OrderBy(u => u.Mapping_UpdateId).First();
                return minVal.Mapping_UpdateId;
            }
        }

        public void UpdateLayer2Data(Guid uuid, GnApiProgramsSchema.programsProgram programData, string nextUpdateId, string maxUpdateId)
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                var rowData = Get(l2 => l2.IngestUUID == uuid);

                rowData.Layer2_UpdateId = programData.updateId;
                rowData.Layer2_NextUpdateId = nextUpdateId;
                rowData.Layer2_MaxUpdateId = maxUpdateId;
                rowData.UpdatesChecked = DateTime.Now;
                rowData.RequiresEnrichment = true;
                Update(rowData);
            }
        }
    }
}
