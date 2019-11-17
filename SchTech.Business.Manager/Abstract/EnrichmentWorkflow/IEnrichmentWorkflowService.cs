using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.File.Manager.Concrete.Serialization;
using SchTech.Queue.Manager.Concrete;

namespace SchTech.Business.Manager.Abstract.EnrichmentWorkflow
{
    public interface IEnrichmentWorkflowService
    {
        FileInfo PrimaryAsset { get; set; }

        FileInfo PreviewAsset { get; set; }

        void LogError(string functionName, string message, Exception ex);

        bool CheckAndCleanOrphanedData();

        bool AvailableDriveSpace();

        void BuildWorkQueue();

        bool ObtainAndParseAdiFile(FileInfo adiPackageInfo);

        bool CallAndParseGnMappingData();

        bool ValidatePackageIsUnique();

        bool SeedGnMappingData();

        bool ExtractPackageMedia();

        bool SeedAdiData();

        bool GetGracenoteMovieEpisodeData();

        bool GetSeriesSeasonSpecialsData();

        bool SetAdiEpisodeMetadata();

        bool SetAdiMovieMetadata();

        bool SetAdiSeriesData();

        bool SetAdiSeasonData();

        bool CheckAndAddPreviewData();

        bool ImageSelectionLogic();

        bool RemoveDerivedFromAsset();

        bool FinalisePackageData();

        bool PackageEnrichedAsset();

        bool DeliverEnrichedAsset();

        bool ProcessFailedPackage();
    }
}
