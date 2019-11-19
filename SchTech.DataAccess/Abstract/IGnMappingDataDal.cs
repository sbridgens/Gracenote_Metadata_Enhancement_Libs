using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Core.DataAccess;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Abstract
{
    public interface IGnMappingDataDal : IEntityRepository<GN_Mapping_Data>
    {
        bool CleanMappingDataWithNoAdi();

        bool AddGraceNoteProgramData(string paid, string seriesTitle, string episodeTitle, GnApiProgramsSchema.programsProgram programDatas);

        Dictionary<string, string> ReturnDbImagesForAsset(string paidValue, int rowId);

        GN_Mapping_Data ReturnMapData(string paid);

    }
}
