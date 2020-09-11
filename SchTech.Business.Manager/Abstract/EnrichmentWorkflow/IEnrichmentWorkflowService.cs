using System.IO;

namespace SchTech.Business.Manager.Abstract.EnrichmentWorkflow
{
    public interface IEnrichmentWorkflowService
    {
        FileInfo PrimaryAsset { get; set; }

        FileInfo PreviewAsset { get; set; }

        bool CheckAndCleanOrphanedData(bool timeElapsed);

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

        bool SetAdiMovieEpisodeMetadata();

        bool SetAdiMovieMetadata();
        bool CheckAndAddPreviewData();

        bool SetAdiSeriesData();

        bool SetAdiSeasonData();

        bool ImageSelectionLogic();

        bool RemoveDerivedFromAsset();

        bool FinalisePackageData();

        bool PackageEnrichedAsset();

        bool DeliverEnrichedAsset();

        void ProcessFailedPackage(FileInfo packageFile);

        bool SaveAdiFile();

        void PackageCleanup(FileInfo packageFile);
    }
}