using log4net;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Linq;
using SchTech.File.Manager.Concrete.ZipArchive;

namespace SchTech.Business.Manager.Concrete.Validation
{
    public class EnhancementDataValidator
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(EnhancementDataValidator));

        public string AdiYearValue { get; private set; }

        public static bool UpdateVersionFailure { get; set; }

        public static bool ValidateVersionMajor(int? dbVersionMajor, int? dbVersionMinor, bool isTvod = false)
        {
            var adiVersionMajor = EnrichmentWorkflowEntities.AdiFile.Metadata.AMS.Version_Major;
            var paid = EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.AMS.Asset_ID;
            EnrichmentWorkflowEntities.IsDuplicateIngest = false;
            UpdateVersionFailure = false;

            if (dbVersionMajor > 0)
            {
                Log.Info($"[Version Information] DB Version Major: {dbVersionMajor}, ADI Version Major: {adiVersionMajor}");

                if (adiVersionMajor > dbVersionMajor)
                {
                    if (EnrichmentWorkflowEntities.AdiFile.Asset.Asset?
                            .FirstOrDefault()?.Content == null || isTvod)
                    {
                        Log.Info($"Confirmed that package with PAID: {paid} is an update. ");
                        //ensure this is set for media unpack later in workflow
                        ZipHandler.IsUpdatePackage = true;

                        return true;
                    }

                    Log.Error("Metadata update contains a media section, failing ingest.");
                    return false;
                }
                if (adiVersionMajor == dbVersionMajor)
                {
                    Log.Info($"Package Version Major matches DB Version Major, Checking version Minor.");

                    if (ValidateVersionMinor(dbVersionMinor, isTvod))
                    {
                        Log.Info($"Confirmed that package with PAID: {paid} is an update. ");
                        //ensure this is set for media unpack later in workflow
                        ZipHandler.IsUpdatePackage = true;

                        return true;
                    }
                    EnrichmentWorkflowEntities.IsDuplicateIngest = true;
                    Log.Error($"Package for PAID: {paid} already exists, duplicate ingest detected! Failing Enhancement.");
                    return false;
                }

                UpdateVersionFailure = true;
                Log.Error(
                    $"Package for PAID: {paid} detected as an update but does not have a higher Version Major! Failing Enhancement.");
                return false;

            }

            Log.Error(
                $"Package for PAID: {paid} detected as an update does not have a database entry! Failing Enhancement.");
            return true;
        }

        private static bool ValidateVersionMinor(int? dbVersionMinor, bool isTvod = false)
        {
            var adiVersionMinor = EnrichmentWorkflowEntities.AdiFile.Metadata.AMS.Version_Minor;
            var paid = EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.AMS.Asset_ID;
            EnrichmentWorkflowEntities.IsDuplicateIngest = false;
            UpdateVersionFailure = false;

            Log.Info($"[Version Information] DB Version Minor: {dbVersionMinor}, ADI Version Minor: {adiVersionMinor}");

            if (adiVersionMinor > dbVersionMinor)
            {
                if (EnrichmentWorkflowEntities.AdiFile.Asset.Asset?
                        .FirstOrDefault()?.Content == null || isTvod)
                {
                    //ensure this is set for media unpack later in workflow
                    ZipHandler.IsUpdatePackage = true;

                    return true;
                }

                Log.Error("Metadata update contains a media section, failing ingest.");
                return false;
            }
            if (adiVersionMinor == dbVersionMinor)
            {
                EnrichmentWorkflowEntities.IsDuplicateIngest = true;
                Log.Error($"Package for PAID: {paid} already exists, duplicate ingest detected! Failing Enhancement.");
                return false;
            }

            UpdateVersionFailure = true;
            Log.Error(
                $"Package for PAID: {paid} detected as an update but does not have a higher Version Minor! Failing Enhancement.");
            return false;
        }

        public static bool IsProgramOneOffSpecial(GnApiProgramsSchema.programsProgram programData)
        {
            return programData.progType.ToLower().Contains("special") || programData.holiday != null;
        }

        public static string CheckAndReturnDescriptionData(
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