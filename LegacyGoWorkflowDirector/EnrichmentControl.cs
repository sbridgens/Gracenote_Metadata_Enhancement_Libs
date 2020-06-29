using log4net;
using SchTech.Api.Manager.GracenoteOnApi.Concrete;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Api.Manager.Serialization;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.Business.Manager.Concrete;
using SchTech.Business.Manager.Concrete.EntityFramework;
using SchTech.Business.Manager.Concrete.ImageLogic;
using SchTech.Business.Manager.Concrete.Validation;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.DataAccess.Concrete;
using SchTech.DataAccess.Concrete.EntityFramework;
using SchTech.Entities.ConcreteTypes;
using SchTech.File.Manager.Concrete.FileSystem;
using SchTech.File.Manager.Concrete.Serialization;
using SchTech.File.Manager.Concrete.ZipArchive;
using SchTech.Queue.Manager.Concrete;
using System;
using System.IO;
using System.Linq;

namespace LegacyGoWorkflowDirector
{
    public class EnrichmentControl
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(EnrichmentControl));

        private IAdiEnrichmentService _adiDataService;

        private readonly IGnImageLookupService _gnImageLookupService;

        private readonly IGnMappingDataService _gnMappingDataService;

        private readonly ICategoryMappingService _categoryMappingService;

        private EnrichmentWorkflowEntities WorkflowEntities { get; }
        private AdiContentController AdiContentManager { get; }
        private GN_Mapping_Data GnMappingData { get; set; }
        private GraceNoteApiManager ApiManager { get; }
        private string DeliveryPackage { get; set; }
        public bool IsPackageAnUpdate { get; set; }
        private ZipHandler ZipHandler { get; set; }
        private bool InsertSuccess { get; set; }
        private Adi_Data AdiData { get; set; }
        public bool FailedToMap { get; set; }
        public bool NonLegacyGoPackage { get; set; }

        private bool HasPoster { get; set; }

        public EnrichmentControl()
        {
            ApiManager = new GraceNoteApiManager();
            WorkflowEntities = new EnrichmentWorkflowEntities();
            AdiContentManager = new AdiContentController();
            _adiDataService = new AdiEnrichmentManager(new EfAdiEnrichmentDal());
            _gnImageLookupService = new GnImageLookupManager(new EfGnImageLookupDal());
            _gnMappingDataService = new GnMappingDataManager(new EfGnMappingDataDal());
            _categoryMappingService = new CategoryMappingManager(new EfCategoryMappingDal());
            GnMappingData = new GN_Mapping_Data();
        }

        private static void LogError(string functionName, string message, Exception ex)
        {
            Log.Error($"[{functionName}] {message}: {ex.Message}");
            if (ex.InnerException != null)
                Log.Error($"[{functionName}] Inner Exception:" +
                          $" {ex.InnerException.Message}");
        }

        public bool AvailableDriveSpace()
        {
            using (var hardwareInformation = new HardwareInformationManager())
            {
                return hardwareInformation.GetDriveSpace();
            }
        }

        public void BuildWorkQueue()
        {
            var pollController = new AdiEnrichmentPollController();

            pollController.StartPollingOperations(ADIWF_Config.InputDirectory, "*.zip");
            WorkflowEntities.HasPackagesToProcess = pollController.HasFilesToProcess;
        }

        public bool ObtainAndParseAdiFile(FileInfo adiPackageInfo)
        {
            try
            {
                ZipHandler = new ZipHandler();
                WorkflowEntities.CurrentPackage = adiPackageInfo;
                WorkflowEntities.SetCurrentWorkingDirectory();
                if (Directory.Exists(WorkflowEntities.CurrentWorkingDirectory))
                    FileDirectoryManager.RemoveExistingTempDirectory(WorkflowEntities.CurrentWorkingDirectory);


                Directory.CreateDirectory(WorkflowEntities.CurrentWorkingDirectory);

                if (!ZipHandler.ExtractItemFromArchive(
                    WorkflowEntities.CurrentPackage.FullName,
                    WorkflowEntities.CurrentWorkingDirectory, false, true))
                    return false;



                Log.Info("Validating ADI XML is well formed");
                if (!WorkflowEntities.SerializeAdiFile(false))
                    return false;


                if (!AdiContentController.ValidatePackageIsLegacyGo(adiPackageInfo))
                {
                    NonLegacyGoPackage = true;
                    return false;
                }


                Log.Info("XML well formed, Retrieving PAID Value from ADI to use in Gracenote Mapping Lookup");

                WorkflowEntities.TitlPaidValue =
                    EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.AMS.Asset_ID;

                HasPoster = AdiContentController.CheckAndRemovePosterSection();

                var adiValidation = new AdiXmlValidator();


                WorkflowEntities.OnapiProviderid =
                    adiValidation.ValidatePaidValue(WorkflowEntities.TitlPaidValue);
                if (!string.IsNullOrEmpty(adiValidation.NewTitlPaid))
                    WorkflowEntities.TitlPaidValue = adiValidation.NewTitlPaid;

                WorkflowEntities.IsQamAsset = adiValidation.IsQamAsset;

                IsPackageAnUpdate = ZipHandler.IsUpdatePackage;



                return true;
            }
            catch (Exception ex)
            {
                LogError(
                    "ObtainAndParseAdiFile",
                    "Error During Parse of Adi file", ex);
                return false;
            }
        }

        public bool CallAndParseGnMappingData()
        {
            try
            {
                if (!WorkflowEntities.GetGracenoteMappingData())
                {
                    Log.Error("Package Mapping data is Null cannot process package!");
                    return false;
                }

                var serializeMappingData = new XmlApiSerializationHelper<GnOnApiProgramMappingSchema.@on>();
                ApiManager.CoreGnMappingData = serializeMappingData.Read(WorkflowEntities.GracenoteMappingData);

                var gnMappingData = new GraceNoteApiManager
                {
                    GraceNoteMappingData = ApiManager.CoreGnMappingData.programMappings.programMapping.FirstOrDefault
                    (p => p.link.Any
                        (
                            i => i.Value == WorkflowEntities.OnapiProviderid &&
                                 string.Equals
                                 (
                                     p.status.ToString(),
                                     GnOnApiProgramMappingSchema.onProgramMappingsProgramMappingStatus.Mapped
                                         .ToString(),
                                     StringComparison.CurrentCultureIgnoreCase)
                        )
                    )
                };

                if (gnMappingData.GraceNoteMappingData == null)
                {
                    Log.Warn("Package is not yet mapped and will not continue to be processed until a later time.");
                    FailedToMap = true;
                    return false;
                }

                WorkflowEntities.GraceNoteTmsId = gnMappingData
                    .GraceNoteMappingData.id.Where(t =>
                    {
                        if (t == null) throw new ArgumentNullException(nameof(t));
                        return t.type.Equals("TMSId");
                    })
                    .Select(r => r?.Value)
                    .FirstOrDefault();

                WorkflowEntities.GraceNoteRootId = gnMappingData
                    .GraceNoteMappingData.id.Where(t =>
                    {
                        if (t == null) throw new ArgumentNullException(nameof(t));
                        return t.type.Equals("rootId");
                    })
                    .Select(r => r?.Value)
                    .FirstOrDefault();

                Log.Info(
                    $"Mapping data with TmsID: {WorkflowEntities.GraceNoteTmsId} parsed successfully, continuing package processing.");
                return true;
            }
            catch (Exception ex)
            {
                FailedToMap = true;
                LogError(
                    "CallAndParseGnMappingData",
                    "Error During Mapping Gracenote of Data", ex);
                return false;
            }
        }

        public bool ValidatePackageIsUnique()
        {
            var adiMajor = _adiDataService.Get(i => i.TitlPaid == WorkflowEntities.TitlPaidValue);

            if (IsPackageAnUpdate & adiMajor != null)
            {
                if (!EnhancementDataValidator.ValidateVersionMajor(adiMajor.VersionMajor, adiMajor.VersionMinor))
                    return false;

                Log.Info("Package is confirmed as a valid Update Package");
                IsPackageAnUpdate = true;
                return true;
            }

            if (IsPackageAnUpdate & adiMajor == null)
            {
                Log.Error(
                    $"No Parent Package exists in the database for update package with paid: {WorkflowEntities.TitlPaidValue}, Failing ingest");
                return false;
            }
            //if (!IsPackageAnUpdate && adiMajor == null)
            if (IsPackageAnUpdate & adiMajor != null)
                return false;
            Log.Info($"Package with Paid: {WorkflowEntities.TitlPaidValue} " +
                     "confirmed as a unique package, continuing ingest operations.");

            return true;

        }

        private GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping GetGnMappingData()
        {
            return ApiManager.CoreGnMappingData
                .programMappings
                .programMapping
                .FirstOrDefault();
        }

        private static DateTime? GetAvailability(string typeRequired,
            GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping mapdata)
        {
            DateTime? availableDateTime = null;


            switch (typeRequired)
            {
                case "start":
                    {
                        if (mapdata.availability?.start != null & mapdata.availability?.start.Year != 1)
                            availableDateTime = Convert.ToDateTime(mapdata.availability?.start);
                        break;
                    }
                case "end":
                    {
                        if (mapdata.availability?.end != null & mapdata.availability?.end.Year != 1)
                            availableDateTime = Convert.ToDateTime(mapdata.availability?.end);
                        break;
                    }
            }

            return availableDateTime;
        }

        public bool SeedGnMappingData()
        {
            try
            {
                var mapData = GetGnMappingData();
                var gnpaid = mapData?.link.Where(i => i.idType.Equals("PAID"))
                    .Select(r => r.Value)
                    .FirstOrDefault();

                var gnexists =
                    _gnMappingDataService.Get(p => p.GN_Paid == gnpaid);

                if (IsPackageAnUpdate || gnexists != null)
                    return UpdateGnMappingData();

                var data = new GN_Mapping_Data
                {
                    GN_TMSID = WorkflowEntities.GraceNoteTmsId,
                    GN_Paid = gnpaid,

                    GN_RootID = mapData?.id.Where(t => t.type.Equals("rootId"))
                        .Select(r => r.Value)
                        .FirstOrDefault(),

                    GN_Status = mapData?.status.ToString(),
                    GN_ProviderId = mapData?.link.Where(i => i.idType.Equals("ProviderId"))
                        .Select(r => r.Value)
                        .FirstOrDefault(),

                    GN_Pid = mapData?.link.Where(i => i.idType.Equals("PID"))
                        .Select(r => r.Value)
                        .FirstOrDefault(),

                    GN_programMappingId = mapData?.programMappingId,
                    GN_creationDate = mapData?.creationDate != null
                        ? Convert.ToDateTime(mapData.creationDate)
                        : DateTime.Now,
                    GN_updateId = mapData?.updateId,
                    GN_Availability_Start = GetAvailability("start", mapData),
                    GN_Availability_End = GetAvailability("end", mapData)
                };
                GnMappingData = _gnMappingDataService.Add(data);
                Log.Info($"Gracenote Mapping data seeded to the database with Row Id: {GnMappingData.Id}");

                return true;
            }
            catch (Exception ex)
            {
                LogError(
                    "SeedGnMappingData",
                    "Error Seeding Mapping data", ex);
                return false;
            }
        }

        private bool UpdateGnMappingData()
        {
            try
            {
                var mapData = GetGnMappingData();
                GnMappingData = _gnMappingDataService.ReturnMapData(WorkflowEntities.IngestUuid);

                if (GnMappingData.GN_TMSID != WorkflowEntities.GraceNoteTmsId)
                {
                    Log.Info("TMSID Mismatch updating db with new value.");
                    GnMappingData.GN_TMSID = WorkflowEntities.GraceNoteTmsId;
                }

                Log.Info("Updating GN_Mapping_Data table with new gracenote mapping data.");

                GnMappingData.GN_Availability_Start = mapData?.availability?.start;
                GnMappingData.GN_Availability_End = mapData?.availability?.end;
                GnMappingData.GN_updateId = mapData?.updateId;
                _gnMappingDataService.Update(GnMappingData);

                return true;
            }
            catch (Exception ugmdex)
            {
                LogError(
                    "UpdateGnMappingData",
                    "Error updating db Mapping data", ugmdex);
                return false;
            }
        }

        public bool ExtractPackageMedia()
        {
            try
            {
                if (IsPackageAnUpdate)
                    return SetInitialUpdateData();


                Log.Info("Extracting Required Items from Package");

                //Legacy Go Package Flagged here to enable entire unpack
                ZipHandler.IsLegacyGoPackage = true;
                ZipHandler.ExtractItemFromArchive(WorkflowEntities.CurrentPackage.FullName,
                    WorkflowEntities.CurrentWorkingDirectory,
                    true,
                    false);

                if (HasPoster)
                {
                    ZipHandler.DeletePostFromPackage(WorkflowEntities.CurrentPackage.FullName);
                    //var patterns = new[] { ".jpg", ".jpeg", ".gif", ".png", ".bmp" };

                    //var files = Directory
                    //    .GetFiles(WorkflowEntities.CurrentWorkingDirectory)
                    //    .Where(file => patterns.Any(file.ToLower().EndsWith))
                    //    .ToList();

                    //if (files.Any())
                    //{
                    //    foreach (var f in files)
                    //    {
                    //        System.IO.File.Delete(f);
                    //    }
                    //}
                }

                if (!IsPackageAnUpdate)
                    return SeedAdiData();


                //seed adi data if main ingest
                return !IsPackageAnUpdate ? SeedAdiData() : SetInitialUpdateData();

            }
            catch (Exception ex)
            {
                LogError(
                    "PackageHandler",
                    "Error during initial package handling", ex);
                return false;
            }
        }

        private bool SeedAdiData()
        {
            try
            {
                var isMapped = _adiDataService.Get(p => p.TitlPaid == WorkflowEntities.TitlPaidValue) != null;


                if (!isMapped & !IsPackageAnUpdate)
                {
                    Log.Info("Seeding Adi Data to the database");


                    AdiData = new Adi_Data
                    {
                        TitlPaid = WorkflowEntities.TitlPaidValue,
                        OriginalAdi = FileDirectoryManager.ReturnAdiAsAString(ZipHandler.ExtractedAdiFile.FullName),
                        VersionMajor = AdiContentController.GetVersionMajor(),
                        VersionMinor = AdiContentController.GetVersionMinor(),
                        ProviderId = AdiContentController.GetProviderId(),
                        TmsId = WorkflowEntities.GraceNoteTmsId,
                        Licensing_Window_End = WorkflowEntities.IsDateTime(
                            AdiContentController.GetLicenceEndData()
                        )
                            ? AdiContentController.GetLicenceEndData()
                            : throw new Exception("Licensing_Window_End Is not a valid DateTime Format," +
                                                  " Rejecting Ingest"),
                        ProcessedDateTime = DateTime.Now
                    };

                    _adiDataService.Add(AdiData);
                    Log.Info($"Adi data seeded to the database with Id: {AdiData.Id}");

                    return true;
                }

                Log.Error("Failed to seed data" +
                          $" data exists for paid: {WorkflowEntities.TitlPaidValue}" +
                          "Failing Ingest.");
                return false;
            }
            catch (Exception ex)
            {
                LogError(
                    "SeedAdiData",
                    "Error during seed of Adi Data", ex);
                return false;
            }
        }

        public bool GetGracenoteMovieEpisodeData()
        {
            try
            {
                if (!WorkflowEntities.GetGracenoteProgramData())
                    return false;

                var serializeEpisodeMovieData =
                    new XmlApiSerializationHelper<GnApiProgramsSchema.@on>();

                //Serialize GN Api Program data
                ApiManager.CoreProgramData =
                    serializeEpisodeMovieData.Read(WorkflowEntities.GracenoteProgramData);

                ApiManager.MovieEpisodeProgramData = ApiManager.CoreProgramData.programs.FirstOrDefault();

                Log.Info("Successfully serialized Gracenote Episode/Movie data");

                WorkflowEntities.GraceNoteConnectorId = ApiManager.GetConnectorId();
                //Wrong place and call for this!
                //WorkflowEntities.GraceNoteUpdateId = ApiManager.GetUpdateId();
                GnMappingData.GN_connectorId = WorkflowEntities.GraceNoteConnectorId;
                _gnMappingDataService.Update(GnMappingData);

                ProgramTypes.SetProgramType(
                    ApiManager.MovieEpisodeProgramData.progType,
                    ApiManager.MovieEpisodeProgramData.subType);

                //set default vars for workflow
                AdiContentManager.InitialiseAndSeedObjectLists(ApiManager.MovieEpisodeProgramData,
                    ApiManager.GetSeasonId().ToString());
                return true;
            }
            catch (Exception ex)
            {
                LogError(
                    "GetGracenoteProgramEpisodeData",
                    "Error Obtaining / Parsing GN Program Data", ex);
                return false;
            }
        }

        public bool GetSeriesSeasonSpecialsData()
        {
            try
            {
                if (!WorkflowEntities.GetGraceNoteSeriesSeasonSpecialsData())
                    return false;

                var serializeSeriesSeasonSpecialData =
                    new XmlApiSerializationHelper<GnApiProgramsSchema.@on>();

                //Serialize GN Api Program data
                ApiManager.CoreSeriesData =
                    serializeSeriesSeasonSpecialData.Read(WorkflowEntities.GraceNoteSeriesSeasonSpecialsData);
                Log.Info("Successfully serialized Gracenote Series/Season/Specials data");
                ApiManager.ShowSeriesSeasonProgramData = ApiManager.CoreSeriesData.programs.FirstOrDefault();

                var seriesData = ApiManager.ShowSeriesSeasonProgramData;

                WorkflowEntities.SeasonId = Convert.ToInt32(seriesData?.seasonId);

                if (seriesData?.seasons != null && (seriesData?.seasons).Any())
                {
                    ApiManager.SetSeasonData();
                    WorkflowEntities.SeasonId = ApiManager.SetSeasonId();

                    Log.Info($"Program contains Season data for season ID: {WorkflowEntities.SeasonId}");
                }


                //BuildEpisodeMovieDataLists();
                AdiContentManager.UpdateListData(ApiManager.ShowSeriesSeasonProgramData,
                    ApiManager.GetSeasonId().ToString());


                return true;
            }
            catch (Exception ex)
            {
                LogError(
                    "GetSeriesSeasonSpecialsData",
                    " Error Obtaining / Parsing Series/Season Data", ex);
                return false;
            }
        }

        private string GetCategoryValue()
        {
            try
            {
                var provider = AdiContentController.GetProvider();
                return _categoryMappingService.Get(
                    c => c.ProviderName == provider).CategoryValue;
            }
            catch (Exception)
            {
                return null;
            }

        }


        public bool SetAdiMovieEpisodeMetadata()
        {
            try
            {
                AdiContentManager.RemoveDefaultAdiNodes();

                if (!EnrichmentWorkflowEntities.IsMoviePackage)
                {
                    AdiContentController.InsertEpisodeData(
                        WorkflowEntities.GraceNoteTmsId,
                        episodeOrdinalValue: ApiManager.GetEpisodeOrdinalValue(),
                        episodeTitle: ApiManager.GetEpisodeTitle()
                        );

                }

                var catValue = GetCategoryValue();

                //Get and insert Legacy Go Category
                if (!string.IsNullOrEmpty(catValue))
                    return
                        AdiContentManager.InsertCategoryValue(catValue) &&
                        //Get and add GN Program Data
                        _gnMappingDataService.AddGraceNoteProgramData(
                            ingestGuid: WorkflowEntities.IngestUuid,
                            seriesTitle: ApiManager.GetSeriesTitle(),
                            episodeTitle: ApiManager.GetEpisodeTitle(),
                            programDatas: ApiManager.MovieEpisodeProgramData
                        ) &&

                        //Insert Crew Actor Data
                        AdiContentManager.InsertActorData() &&

                        //Insert Support Crew Data
                        AdiContentManager.InsertCrewData() &&

                        //Insert Program Title Data
                        AdiContentManager.InsertTitleData(EnrichmentWorkflowEntities.IsMoviePackage) &&

                        //Add Correct description summaries
                        AdiContentManager.InsertDescriptionData(
                            descriptions: ApiManager.MovieEpisodeProgramData.descriptions) &&

                        //Insert the Year data based on air date
                        AdiContentManager.InsertYearData(
                            airDate: ApiManager.MovieEpisodeProgramData.origAirDate,
                            movieInfo: ApiManager.MovieEpisodeProgramData?.movieInfo
                        ) &&

                        //Insert Program Genres and Genre Id's
                        AdiContentManager.InsertGenreData() &&

                        //Insert required IMDB Data
                        AdiContentManager.InsertIdmbData(
                            externalLinks: ApiManager.ExternalLinks(),
                            hasMovieInfo: HasMovieInfo()
                        );

                Log.Error($"A correct Category Mapping for Provider: {AdiContentController.GetProvider()} was not found, failing ingest.");
                return false;

            }
            catch (Exception ex)
            {
                LogError(
                    "SetAdiMovieEpisodeMetadata",
                    "Error Setting Title Metadata", ex);
                return false;
            }
        }


        public bool SetAdiSeriesData()
        {
            try
            {
                //need to check api calls as The series and season data is missing

                ApiManager.SetSeasonData();
                AdiContentManager.SeasonInfo = ApiManager.GetSeasonInfo();
                //Insert IMDB Data
                return AdiContentManager.InsertIdmbData(
                           ApiManager.ExternalLinks(),
                           HasMovieInfo()
                           ) &&

                       //Insert the TITL Series Layerdata
                       AdiContentController.InsertSeriesLayerData(
                           ApiManager.ShowSeriesSeasonProgramData.connectorId,
                           ApiManager.GetSeriesId()
                           ) &&

                       //Insert the TITL Show Data
                       AdiContentManager.InsertShowData(
                           showId: ApiManager.GetShowId(),
                           showName: ApiManager.GetShowName(),
                           totalSeasons: ApiManager.GetNumberOfSeasons(),
                           descriptions: ApiManager.ShowSeriesSeasonProgramData.descriptions) &&

                       //Insert the TITLE Series Genres
                       AdiContentManager.InsertSeriesGenreData() &&

                       //Insert the Series ID information
                       AdiContentManager.InsertSeriesData(
                           ApiManager.GetGnSeriesId(),
                           seriesOrdinalValue: ApiManager.GetSeriesOrdinalValue(),
                           ApiManager.GetSeasonId(),
                           episodeSeason: ApiManager.GetEpisodeSeason()
                           );
            }
            catch (Exception ex)
            {
                LogError(
                    "SetAdiSeriesData",
                    "Error Setting Series metadata", ex);
                return false;
            }
        }

        public bool SetAdiSeasonData()
        {
            try
            {
                return AdiContentController.InsertProductionYears(
                    ApiManager.GetSeriesPremiere(),
                    ApiManager.GetSeasonPremiere(),
                    ApiManager.GetSeriesFinale(),
                    ApiManager.GetSeasonFinale()
                    );
            }
            catch (Exception ex)
            {
                LogError(
                    "SetAdiSeasonData",
                    "Error Setting Season metadata", ex);
                return false;
            }
        }

        public bool ImageSelectionLogic()
        {
            //initialise as true and allow the workflow to falsify
            //this is needed for update packages as there is not always an updated image.
            InsertSuccess = true;
            var currentImage = "";
            var imageLookups = _gnImageLookupService.GetList().OrderBy(o => Convert.ToInt32(o.Image_AdiOrder));
            var serialization = new XmlSerializationManager<ImageMapping>();

            foreach (var configLookup in imageLookups)
            {
                var mappingData = serialization.Read(configLookup.Mapping_Config);
                var currentProgramType =
                    mappingData.ProgramType.SingleOrDefault(p => p == ApiManager.MovieEpisodeProgramData.progType);

                // Ensure we don't use series or show assets for movies
                if (EnrichmentWorkflowEntities.IsMoviePackage & configLookup.Image_Mapping.ToLower().Contains("_series_") ||
                    EnrichmentWorkflowEntities.IsMoviePackage & configLookup.Image_Mapping.ToLower().Contains("_show_"))
                    continue;

                //prevent duplicate processing
                if (string.IsNullOrEmpty(currentProgramType) ||
                    configLookup.Image_Mapping == currentImage)
                    continue;


                var isl = new ImageSelectionLogic
                {
                    ImageMapping = mappingData,
                    CurrentMappingData = _gnMappingDataService.ReturnMapData(WorkflowEntities.IngestUuid),
                    IsUpdate = IsPackageAnUpdate,
                    ConfigImageCategories = mappingData.ImageCategory,
                    ApiAssetList = AdiContentManager.ReturnAssetList()
                };

                isl.DbImagesForAsset = _gnMappingDataService.ReturnDbImagesForAsset(
                    WorkflowEntities.GnMappingPaid,
                    isl.CurrentMappingData.Id,
                    false
                );

                var imageUri = isl.GetGracenoteImage(configLookup.Image_Lookup);

                if (string.IsNullOrEmpty(imageUri))
                    continue;

                if (!isl.DownloadImageRequired)
                    continue;

                var localImage = GetImageName(imageUri, configLookup.Image_Mapping);
                isl.DownloadImage(imageUri, localImage);

                if (IsPackageAnUpdate & !string.IsNullOrEmpty(SchTech.Business.Manager.Concrete.ImageLogic.ImageSelectionLogic.DbImages))
                {

                    InsertSuccess = AdiContentController.UpdateImageData(
                        isl.ImageQualifier,
                        WorkflowEntities.TitlPaidValue,
                        imageUri.Replace("assets/", ""),
                        configLookup.Image_Mapping,
                        isl.GetFileAspectRatio(localImage),
                        FileDirectoryManager.GetFileHash(localImage),
                        FileDirectoryManager.GetFileSize(localImage)
                    );

                    //update image data in db and adi
                    UpdateDbImages();
                    SchTech.Business.Manager.Concrete.ImageLogic.ImageSelectionLogic.DbImages = null;
                }
                else
                {
                    //download and insert image
                    InsertSuccess = AdiContentController.InsertImageData
                    (
                        WorkflowEntities.TitlPaidValue,
                        imageUri.Replace("assets/", ""),
                        configLookup.Image_Mapping,
                        FileDirectoryManager.GetFileHash(localImage),
                        FileDirectoryManager.GetFileSize(localImage),
                        Path.GetExtension(localImage),
                        isl.ImageQualifier,
                        configLookup.Image_Lookup,
                        isl.GetFileAspectRatio(localImage)
                    );

                    //update image data in db and adi
                    UpdateDbImages();
                    SchTech.Business.Manager.Concrete.ImageLogic.ImageSelectionLogic.DbImages = null;
                }

                if (InsertSuccess)
                    currentImage = configLookup.Image_Mapping;
            }

            return InsertSuccess;
        }

        public static bool RemoveDerivedFromAsset()
        {
            try
            {
                //<App_Data App="VOD" Name="DeriveFromAsset" Value="ASST0000000001506105" />
                foreach (var asset in from asset in EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                                      let dfa = asset.Metadata.App_Data.FirstOrDefault(d =>
                                              d.Name.ToLower() == "derivefromasset"
                                      )
                                      where dfa != null
                                      select asset)
                {
                    Log.Info("Removing DeriveFromAsset section from ADI.xml");
                    EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Remove(asset);
                    break;
                }


                return true;
            }
            catch (Exception ex)
            {
                LogError(
                    "RemoveDerivedFromAsset",
                    "Error removing DerivedFromAsset", ex);
                return false;
            }
        }

        public bool FinalisePackageData()
        {
            try
            {
                //Insert Layer data for Program Layer
                AdiContentController.InsertProgramLayerData(
                    WorkflowEntities.GraceNoteTmsId,
                    programRootId: ApiManager.MovieEpisodeProgramData?.rootId,
                    shoDataRootId: ApiManager.ShowSeriesSeasonProgramData?.rootId);

                AdiContentController.CheckAndAddBlockPlatformData();
            }
            catch (Exception ex)
            {
                LogError(
                    "FinalisePackageData",
                    "Error Setting Final package Data", ex);
                return false;
            }

            return true;
        }

        public bool PackageEnrichedAsset(FileInfo sourceArchive)
        {
            try
            {
                DeliveryPackage = sourceArchive.FullName;

                return ZipHandler.CreateLegacyGoPackage(sourceArchive.FullName, WorkflowEntities.CurrentWorkingDirectory);
            }
            catch (Exception ex)
            {
                LogError(
                    "PackageEnrichedAsset",
                    "Error Creating Final Package", ex);
                return false;
            }
        }

        public bool DeliverEnrichedAsset()
        {
            try
            {
                var fInfo = new FileInfo(DeliveryPackage);

                var tmpPackage = Path.Combine(ADIWF_Config.IngestDirectory, $"{fInfo.Name}.tmp");

                var finalPackage = Path.Combine(ADIWF_Config.IngestDirectory, fInfo.Name);

                Log.Info($"Moving Temp Package to ingest: {DeliveryPackage} to {tmpPackage}");
                File.Move(DeliveryPackage, tmpPackage);
                if (File.Exists(tmpPackage))
                {
                    Log.Info("Temp package successfully moved");
                    Log.Info($"Moving Temp Package: {tmpPackage} to Ingest package {finalPackage}");
                    File.Move(tmpPackage, finalPackage);
                    Log.Info("Ingest package Delivered successfully.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError(
                    "DeliverEnrichedAsset",
                    "Error Delivering Final Package", ex);
                return false;
            }

            return false;
        }

        /// <summary>
        ///     If update set the required vars for processing an update package.
        ///     TVOD Media Section is handled here also
        /// </summary>
        /// <returns></returns>
        private bool SetInitialUpdateData()
        {
            try
            {
                //Get the correct stored adi data
                AdiData = _adiDataService.GetAdiData(WorkflowEntities.IngestUuid);
                if (AdiData.EnrichedAdi == null)
                    throw new Exception($"Previously Enriched ADI data for Paid: {WorkflowEntities.TitlPaidValue}" +
                                        " was not found in the database?");

                //Serialize previously enriched Adi File to obtain Asset data
                WorkflowEntities.SerializeAdiFile(true, AdiData.EnrichedAdi);
                AdiData.TmsId = WorkflowEntities.GraceNoteTmsId;
                AdiData.VersionMajor = AdiContentController.GetVersionMajor();
                AdiData.VersionMinor = AdiContentController.GetVersionMinor();

                AdiData.Licensing_Window_End = WorkflowEntities.IsDateTime(
                    AdiContentController.GetLicenceEndData()
                )
                    ? AdiContentController.GetLicenceEndData()
                    : throw new Exception("Licensing_Window_End Is not a valid DateTime Format, Rejecting Ingest");


                AdiContentController.RemoveMovieContentFromUpdate();
                //Get original asset data and modify new adi.
                if (!AdiContentController.CopyPreviouslyEnrichedAssetDataToAdi())
                    return false;
                //Update all version major values to correct value.
                if (!AdiContentController.UpdateAllVersionMajorValues(WorkflowEntities.AdiVersionMajor))
                    return false;


                _adiDataService.Update(AdiData);
                Log.Info("Adi data updated in the database.");

                return true;


            }
            catch (Exception ex)
            {
                LogError(
                    "SetInitialUpdateData",
                    "Error Setting initial update data", ex);
                return false;
            }
        }

        private bool HasMovieInfo()
        {
            return ApiManager.MovieEpisodeProgramData?.movieInfo != null;
        }

        private string GetImageName(string imageUri, string imageMapping)
        {
            var imageName = imageUri.Replace("?trim=true", "");
            var originalFileName = Path.GetFileNameWithoutExtension(imageName);

            var newFileName = originalFileName.Replace(originalFileName,
                $"{imageMapping}_{originalFileName}{Path.GetExtension(imageName)}");

            return Path.Combine(WorkflowEntities.CurrentWorkingDirectory, newFileName);
        }

        private void UpdateDbImages()
        {
            var gnMappingRow = _gnMappingDataService.ReturnMapData(WorkflowEntities.IngestUuid);
            gnMappingRow.GN_Images = SchTech.Business.Manager.Concrete.ImageLogic.ImageSelectionLogic.DbImages;
            var rowId = _gnMappingDataService.Update(gnMappingRow);
            Log.Info($"GN Mapping table with row id: {rowId.Id} updated with new image data");
        }

        public bool SaveAdiFile()
        {
            try
            {
                var outputAdi = Path.Combine(WorkflowEntities.CurrentWorkingDirectory, "ADI.xml");
                WorkflowEntities.SaveAdiFile(outputAdi, EnrichmentWorkflowEntities.AdiFile);

                var adiString = FileDirectoryManager.ReturnAdiAsAString(outputAdi);

                if (AdiData == null)
                    AdiData = _adiDataService.GetAdiData(WorkflowEntities.IngestUuid);

                if (!IsPackageAnUpdate)
                {
                    AdiData.EnrichedAdi = adiString;
                    AdiData.Enrichment_DateTime = DateTime.Now;
                }
                else
                {
                    AdiData.UpdateAdi = adiString;
                    AdiData.Update_DateTime = DateTime.Now;
                }

                _adiDataService.Update(AdiData);
                return true;
            }
            catch (Exception safex)
            {
                LogError(
                    "SaveAdiFile",
                    "Error Saving Enriched ADI", safex);
                return false;
            }
        }

        public void ProcessFailedPackage(FileInfo packageFile)
        {
            try
            {
                var source = packageFile.FullName;
                var destination = FailedToMap
                    ? $"{ADIWF_Config.MoveNonMappedDirectory}\\{packageFile.Name}"
                    : $"{ADIWF_Config.FailedDirectory}\\{packageFile.Name}";

                if (File.Exists(destination))
                    File.Delete(destination);

                if (FailedToMap)
                {
                    Log.Info($"Moving Package: {packageFile} to Failed to map directory: " +
                             $"{ADIWF_Config.MoveNonMappedDirectory}");
                    Log.Info($"This package will be retried for: {ADIWF_Config.FailedToMap_Max_Retry_Days}" +
                             " before it is failed completely.");

                    var dt = DateTime.Now.AddDays(-Convert.ToInt32(ADIWF_Config.FailedToMap_Max_Retry_Days));

                    if (dt >= packageFile.LastWriteTime.Date)
                    {
                        Log.Warn("Ingest file has passed the time for allowed mapping and will deleted!");
                        File.Delete(packageFile.FullName);
                        return;
                    }
                }

                if (IsPackageAnUpdate)
                {
                    Log.Info($"Moving Package: {packageFile} to Updates Failed directory: " +
                             $"{ADIWF_Config.UpdatesFailedDirectory}");
                    File.Move(source, destination);
                }
                if (!NonLegacyGoPackage)
                {
                    Log.Info($"Moving Package: {packageFile} to Failed directory: " +
                             $"{ADIWF_Config.FailedDirectory}");

                    File.Move(source, destination);
                }


                if (File.Exists(destination) & !NonLegacyGoPackage)
                    Log.Info("Move to failed directory successful.");

                FileDirectoryManager.RemoveExistingTempDirectory(WorkflowEntities.CurrentWorkingDirectory);

                Log.Info("Removing db entries for Failed Package.");
                _adiDataService.Delete(AdiData);
                _gnMappingDataService.Delete(GnMappingData);
            }
            catch (Exception pfpex)
            {
                LogError(
                    "ProcessFailedPackage",
                    "Error Processing Failed Package", pfpex);
            }
        }

        public void PackageCleanup(FileInfo packageFile)
        {
            try
            {
                Log.Info($"Deleting package file: {packageFile.FullName}");
                File.Delete(packageFile.FullName);
                if (!File.Exists(packageFile.FullName))
                    Log.Info($"Successfully deleted {packageFile.FullName}");

                Log.Info($"Removing working directory: {WorkflowEntities.CurrentWorkingDirectory}");
                Directory.Delete(WorkflowEntities.CurrentWorkingDirectory, true);
                if (!Directory.Exists(WorkflowEntities.CurrentWorkingDirectory))
                    Log.Info($"Successfully deleted Working directory {WorkflowEntities.CurrentWorkingDirectory}");

            }
            catch (Exception pcuex)
            {
                LogError(
                    "PackageCleanup",
                    "Error Cleaning up processed Package", pcuex);
            }
        }
    }
}