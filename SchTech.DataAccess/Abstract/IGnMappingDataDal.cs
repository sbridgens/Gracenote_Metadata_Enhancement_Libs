using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Collections.Generic;

namespace SchTech.DataAccess.Abstract
{
    public interface IGnMappingDataDal : IEntityRepository<GN_Mapping_Data>
    {
        bool CleanMappingDataWithNoAdi();

        bool AddGraceNoteProgramData(Guid ingestGuid, string seriesTitle, string episodeTitle,
            GnApiProgramsSchema.programsProgram programDatas);

        Dictionary<string, string> ReturnDbImagesForAsset(string paidValue, int rowId);

        GN_Mapping_Data ReturnMapData(Guid ingestGuid);
    }
}