using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Api.Manager.GracenoteOnApi.Concrete;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Api.Manager.Serialization;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.Business.Manager.Concrete.EntityFramework;
using SchTech.DataAccess.Concrete.EntityFramework;
using SchTech.Entities.ConcreteTypes;

namespace GracenoteUpdateManager
{
    internal class GnMappingComparer
    {
        private GraceNoteApiManager ApiManager { get; set; }
        private GN_Mapping_Data CoreGnMappingData { get; set; }

        private GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping programMapping { get; set; }

        private readonly IGnMappingDataService _gnMappingDataService;

        private readonly IGnApiLookupService _gnApiLookupService;

        internal GnMappingComparer()
        {
            _gnMappingDataService = new GnMappingDataManager(new EfGnMappingDataDal());
            CoreGnMappingData = new GN_Mapping_Data();
            _gnApiLookupService = new GnApiLookupManager(new EfGnApiLookupDal());
        }

        internal bool MappingDataChanged(Guid ingestUUID)
        {
            try
            {
                var mappingData = _gnApiLookupService.Get(m => m.IngestUUID == ingestUUID);

                var serializeMappingData = new UpdateTrackerSerializationHelper<GnOnApiProgramMappingSchema.@on>();
                // taken from the GnApiLookup table
                ApiManager.CoreGnMappingData = serializeMappingData.Read(mappingData.GnMapData);
                if (ApiManager.CoreGnMappingData.programMappings.programMapping == null)
                {
                    return false;
                }


                //retrieve existing mapping data for comparison
                CoreGnMappingData = _gnMappingDataService.ReturnMapData(ingestUUID);
                GetGnMappingData();
                checkIdTypes();
                CheckLinkTypes();
                CheckAvailability();

                if (programMapping != null)
                {
                    //do something
                }


                return true;

            }
            catch (Exception e)
            {

                return false;
            }
        }


        // <status>Mapped</status>
        private void GetGnMappingData()
        {
            programMapping = ApiManager.CoreGnMappingData
                    .programMappings
                    .programMapping
                    .FirstOrDefault(m => m.status == GnOnApiProgramMappingSchema.onProgramMappingsProgramMappingStatus.Mapped);
        }


        // <id type="TMSId">EP030510850002</id>
        // <id type="rootId">15959485</id>
        private bool checkIdTypes()
        {

            //get previous api tmsid
            var previousTmsId = programMapping.id.FirstOrDefault(t => t.type.ToLower() == "tmsid")?.Value;
            //get previous api rootid
            var previousRootId = programMapping.id.FirstOrDefault(r => r.type.ToLower() == "rootid")?.Value;


            if (previousTmsId == CoreGnMappingData.GN_TMSID)
            {
                //do something
            }

            if (previousRootId == CoreGnMappingData.GN_RootID)
            {
                //do something
            }

            return true;
        }


        // <link idType="ProviderId">ITVTITL0100000000012082</link>
        // <link idType="PAID">TITL0100000000012082</link>
        // <link idType="PID">ITV</link>

        private bool CheckLinkTypes()
        {
            //get api providerid
            var previousProviderId = programMapping.link.FirstOrDefault(p => p.idType.ToLower() == "providerid")?.Value;
            //get api paid
            var previousPaid = programMapping.link.FirstOrDefault(p => p.idType.ToLower() == "paid")?.Value;
            //get api pid
            var previousPid = programMapping.link.FirstOrDefault(p => p.idType.ToLower() == "pid")?.Value;

            if (previousProviderId == CoreGnMappingData.GN_ProviderId)
            {
                //do something
            }

            if (previousPaid == CoreGnMappingData.GN_Paid)
            {
                //do something
            }
            if (previousPid == CoreGnMappingData.GN_Pid)
            {
                //do something
            }

            return true;
        }

        // <availability>
        //   <start>2019-12-23T10:09:30Z</start>
        //   <end>2021-04-01T00:01:01Z</end>
        // </availability>
        private bool CheckAvailability()
        {
            if (programMapping.availability == null)
                return true;

            var apiAvailabilityStart = programMapping.availability?.start.ToString("yyyy-MM-dd");
            var apiAvailabilityEnd = programMapping.availability?.end.ToString("yyyy-MM-dd");

            var enrichedAvailabilityStart = CoreGnMappingData.GN_Availability_Start?.ToString("yyyy-MM-dd");
            var enrichedAvailabilityEnd = CoreGnMappingData.GN_Availability_End?.ToString("yyyy-MM-dd");

            if (apiAvailabilityStart.Equals(enrichedAvailabilityStart))
            {
                //do something
            }
            if (apiAvailabilityEnd.Equals(enrichedAvailabilityEnd))
            {
                //do something
            }
            return true;
        }
    }
}
