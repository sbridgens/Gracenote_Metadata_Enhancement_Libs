using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.Core.DataAccess;
using SchTech.DataAccess.Abstract;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.Business.Manager.Concrete.EntityFramework
{
    public class GnMappingDataManager : IGnMappingDataService
    {
        private readonly IGnMappingDataDal _gnMappingDataDal;

        public GnMappingDataManager(IGnMappingDataDal gnMappingDataDal)
        {
            _gnMappingDataDal = gnMappingDataDal;
        }

        public List<GN_Mapping_Data> GetList(Expression<Func<GN_Mapping_Data, bool>> filter = null)
        {
            return _gnMappingDataDal.GetList(filter);
        }

        public GN_Mapping_Data Get(Expression<Func<GN_Mapping_Data, bool>> filter)
        {
            return _gnMappingDataDal.Get(filter);
        }

        public GN_Mapping_Data Add(GN_Mapping_Data entity)
        {
            return _gnMappingDataDal.Add(entity);
        }

        public GN_Mapping_Data Update(GN_Mapping_Data entity)
        {
            return _gnMappingDataDal.Update(entity);
        }

        public GN_Mapping_Data Delete(GN_Mapping_Data entity)
        {
            return _gnMappingDataDal.Delete(entity);
        }

        public bool CleanMappingDataWithNoAdi()
        {
            return _gnMappingDataDal.CleanMappingDataWithNoAdi();
        }

        public bool AddGraceNoteProgramData(string paid, string seriesTitle, string episodeTitle, GnApiProgramsSchema.programsProgram programDatas)
        {
            return _gnMappingDataDal.AddGraceNoteProgramData(paid, seriesTitle, episodeTitle, programDatas);
        }

        public Dictionary<string, string> ReturnDbImagesForAsset(string paidValue, int rowId)
        {
            return _gnMappingDataDal.ReturnDbImagesForAsset(paidValue, rowId);
        }

        public GN_Mapping_Data ReturnMapData(string paid)
        {
            return _gnMappingDataDal.ReturnMapData(paid);
        }

        public void UpdateGNImages(string NewGnImages)
        {
            _gnMappingDataDal.UpdateGNImages(NewGnImages);
        }

    }
}
