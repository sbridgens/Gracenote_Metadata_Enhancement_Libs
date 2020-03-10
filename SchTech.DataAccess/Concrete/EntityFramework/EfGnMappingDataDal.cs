using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfGnMappingDataDal : EfEntityRepositoryBase<GN_Mapping_Data, ADI_EnrichmentContext>, IGnMappingDataDal
    {
        public bool CleanMappingDataWithNoAdi()
        {
            using (var mapContext = new ADI_EnrichmentContext())
            {
                try
                {
                    var mappedNoAdi = mapContext.GN_Mapping_Data.Where(map => !mapContext.Adi_Data.Any(
                        adata => EfStaticMethods.GetPaidLastValue(adata.TitlPaid) == EfStaticMethods.GetPaidLastValue(map.GN_Paid))
                    );

                    if (mappedNoAdi.FirstOrDefault() != null)
                    {
                        mapContext.RemoveRange(mappedNoAdi);
                        mapContext.SaveChanges();
                    }
                    else
                    {
                        EfStaticMethods.Log.Info("No orphaned GN mappings found.");
                    }

                    return true;
                }
                catch (SqlException sqlEx)
                {
                    EfStaticMethods.Log.Error($"SQL Exception during database connection: {sqlEx.Message}");
                    if (sqlEx.InnerException != null)
                        EfStaticMethods.Log.Error($"Inner Exception: {sqlEx.InnerException.Message}");

                    return false;
                }
                catch (Exception cmdwndEx)
                {
                    EfStaticMethods.Log.Error($"General Exception during database connection: {cmdwndEx.Message}");
                    if (cmdwndEx.InnerException != null)
                        EfStaticMethods.Log.Error($"Inner Exception: {cmdwndEx.InnerException.Message}");

                    return false;
                }
            }
        }

        public bool AddGraceNoteProgramData(string paid, string seriesTitle, string episodeTitle,
            GnApiProgramsSchema.programsProgram programData)
        {
            EfStaticMethods.Log.Info("Updating Gracenote database Mapping table with Program Data");

            var gnMappingData = ReturnMapData(paid);
            if (gnMappingData == null)
                return false;

            gnMappingData.GN_SeasonId = Convert.ToInt32(programData?.seasonId);
            gnMappingData.GN_SeasonNumber = Convert.ToInt32(programData?.episodeInfo?.season);
            gnMappingData.GN_SeriesId = Convert.ToInt32(programData?.seriesId);
            gnMappingData.GN_EpisodeNumber = Convert.ToInt32(programData?.episodeInfo?.number);
            gnMappingData.GN_EpisodeTitle = episodeTitle;
            gnMappingData.GN_SeriesTitle = seriesTitle;
            Update(gnMappingData);

            EfStaticMethods.Log.Info($"GN Mapping database updated," +
                                     $" where Paid: {paid} & Row ID: {gnMappingData.Id}");



            return true;
        }

        public Dictionary<string, string> ReturnDbImagesForAsset(string paidValue, int rowId)
        {
            var dbImages = new Dictionary<string, string>();

            EfStaticMethods.Log.Debug($"Retrieving image data from the db where GNPAID = {paidValue}");
            try
            {
                using (var db = new ADI_EnrichmentContext())
                {
                    var imgList = db.GN_Mapping_Data.Where(i => i.Id == rowId)
                        .Select(i => i.GN_Images)
                        .FirstOrDefault()
                        ?.Split(',')
                        .Select(k => k.Trim().Split(':'))
                        .ToList();

                    if (imgList == null)
                        return dbImages;

                    foreach (var kv in imgList.Where(kv => !dbImages.ContainsKey(kv[0]))) dbImages.Add(kv[0], kv[1]);
                }

                return dbImages;
            }
            catch (Exception gdbiEx)
            {
                EfStaticMethods.Log.Error(
                    $"Error obtaining DB Images for GN PAID: {paidValue}, ERROR = {gdbiEx.Message}");
                if (gdbiEx.InnerException != null)
                    EfStaticMethods.Log.Error($"Inner Exception: {gdbiEx.InnerException.Message}");
                EfStaticMethods.Log.Info("Continuing with workflow!");

                return dbImages;
            }
        }

        public GN_Mapping_Data ReturnMapData(string paid)
        {
            using (var db = new ADI_EnrichmentContext())
            {
                var gnPaid = paid
                    .Replace("TITL", "")
                    .Replace("i", "")
                    .TrimStart('0');

                return db.GN_Mapping_Data.FirstOrDefault(
                    i => i.GN_Paid.Contains(gnPaid));
            }
        }
    }
}