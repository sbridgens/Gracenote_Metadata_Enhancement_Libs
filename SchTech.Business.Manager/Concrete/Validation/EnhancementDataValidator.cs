using log4net;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Linq;

namespace SchTech.Business.Manager.Concrete.Validation
{
    public class EnhancementDataValidator
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(EnhancementDataValidator));

        public string AdiYearValue { get; set; }

        public static bool ValidateVersionMajor(int? dbVersionMajor, bool isTvod)
        {
            var adiVersionMajor = EnrichmentWorkflowEntities.AdiFile.Metadata.AMS.Version_Major;
            var paid = EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.AMS.Asset_ID;

            if (dbVersionMajor > 0)
            {
                if (adiVersionMajor > dbVersionMajor)
                {
                    if (EnrichmentWorkflowEntities.AdiFile.Asset.Asset?.FirstOrDefault()?.Content == null
                        ||
                        isTvod)
                    {
                        Log.Info("Confirmed that package with PAID: " +
                                 $"{paid} " +
                                 "is an update. " +
                                 $"Previous Version Major: {dbVersionMajor}, New Version Major: {adiVersionMajor}");

                        return true;
                    }

                    Log.Error("Metadata update contains a media section, failing ingest.");
                    return false;
                }

                Log.Error(
                    $"Package for PAID: {paid} detected as an update but does not have a higher Version Major! Failing Enhancement.");
                return false;
            }

            Log.Error(
                $"Package for PAID: {paid} detected as an update does not have a database entry! Failing Enhancement.");
            return false;
        }

        public static bool IsProgramOneOffSpecial(GnApiProgramsSchema.programsProgram programData)
        {
            return programData.progType.ToLower().Contains("special") || programData.holiday != null;
        }

        public string CheckAndReturnDescriptionData(
            GnApiProgramsSchema.programsProgramDescriptions programDescriptions, bool isSeason = false)
        {
            if (!programDescriptions.desc.Any())
                return string.Empty;

            if (!isSeason)
                return programDescriptions.desc
                    .Where(
                        d => d.type == "plot" && d.size == "250" ||
                             d.type == "plot" && d.size == "100" ||
                             d.type == "generic" && d.size == "100" ||
                             d.size == "250" ||
                             d.size == "100")
                    .Select(t => t.Value)
                    .FirstOrDefault();

            return programDescriptions.desc
                .Where(d => d.size == "250")
                .Select(t => t.Value)
                .FirstOrDefault()
                ?.ToString();
        }

        public bool HasYearData(DateTime airDate, GnApiProgramsSchema.programsProgramMovieInfo movieInfo)
        {
            if (!string.IsNullOrEmpty(airDate.Year.ToString()))
                AdiYearValue = airDate.Year.ToString().Length == 4
                    ? airDate.Year.ToString()
                    : movieInfo?.yearOfRelease?.Length == 4
                        ? movieInfo.yearOfRelease
                        : string.Empty;

            return !string.IsNullOrEmpty(AdiYearValue);
        }
    }
}