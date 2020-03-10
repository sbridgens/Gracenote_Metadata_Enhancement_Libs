using log4net;
using SchTech.Api.Manager.GracenoteOnApi.Concrete;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Api.Manager.Serialization;
using SchTech.Business.Manager.Abstract.EnrichmentWorkflow;
using SchTech.Business.Manager.Abstract.EntityFramework;
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


namespace SchTech.Business.Manager.Concrete.CustomerBusinessLogic.VirginMedia
{
    public class EnrichmentWorkflowManager : IEnrichmentWorkflowService
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(EnrichmentWorkflowManager));

        private IAdiEnrichmentService _adiDataService;

        private readonly IGnImageLookupService _gnImageLookupService;

        private readonly IGnMappingDataService _gnMappingDataService;

        private EnrichmentWorkflowEntities WorkflowEntities { get; }
        private ProdisAdiContentManager AdiContentManager { get; }
        private GN_Mapping_Data GnMappingData { get; set; }
        private GraceNoteApiManager ApiManager { get; }
        private string DeliveryPackage { get; set; }
        private bool IsPackageAnUpdate { get; set; }
        private ZipHandler ZipHandler { get; set; }
        public FileInfo PrimaryAsset { get; set; }
        public FileInfo PreviewAsset { get; set; }
        private bool IsTvodPackage { get; set; }
        private bool InsertSuccess { get; set; }
        private bool FailedToMap { get; set; }
        private Adi_Data AdiData { get; set; }
        private bool HasPoster { get; set; }

        public EnrichmentWorkflowManager()
        {
            ApiManager = new GraceNoteApiManager();
            WorkflowEntities = new EnrichmentWorkflowEntities();
            AdiContentManager = new ProdisAdiContentManager();
            _adiDataService = new AdiEnrichmentManager(new EfAdiEnrichmentDal());
            _gnImageLookupService = new GnImageLookupManager(new EfGnImageLookupDal());
            _gnMappingDataService = new GnMappingDataManager(new EfGnMappingDataDal());
            GnMappingData = new GN_Mapping_Data();
        }

        private static void LogError(string functionName, string message, Exception ex)
        {
            Log.Error($"[{functionName}] {message}: {ex.Message}");
            if (ex.InnerException != null)
                Log.Error($"[{functionName}] Inner Exception:" +
                          $" {ex.InnerException.Message}");
        }

        public bool CheckAndCleanOrphanedData(bool timerElapsed)
        {
            try
            {
                Log.Info("Checking for orphaned db data, this may take time dependent on db size; please be patient");
                if (_adiDataService == null)
                    _adiDataService = new AdiEnrichmentManager(new EfAdiEnrichmentDal());
                return _adiDataService.CleanAdiDataWithNoMapping(timerElapsed) &&
                       _gnMappingDataService.CleanMappingDataWithNoAdi();
            }
            catch (Exception e)
            {
                LogError("CheckAndCleanOrphanedData", "Error Cleaning DB Orphans", e);
                return false;
            }
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

                var adiValidation = new AdiXmlValidator();

                if (!ZipHandler.ExtractItemFromArchive(
                    WorkflowEntities.CurrentPackage.FullName,
                    WorkflowEntities.CurrentWorkingDirectory, false, true))
                    return false;


                Log.Info("Validating ADI XML is well formed");
                if (!WorkflowEntities.SerializeAdiFile(false))
                    return false;

                Log.Info("XML well formed, Retrieving PAID Value from ADI to use in Gracenote Mapping Lookup");
                WorkflowEntities.TitlPaidValue =
                    EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.AMS.Asset_ID;

                WorkflowEntities.OnapiProviderid =
                    adiValidation.ValidatePaidValue(WorkflowEntities.TitlPaidValue);
                if (!string.IsNullOrEmpty(adiValidation.NewTitlPaid))
                    WorkflowEntities.TitlPaidValue = adiValidation.NewTitlPaid;

                WorkflowEntities.IsQamAsset = adiValidation.IsQamAsset;

                IsPackageAnUpdate = ZipHandler.IsUpdatePackage;

                WorkflowEntities.CheckSetSdPackage(IsPackageAnUpdate);

                IsTvodPackage = EnrichmentWorkflowEntities.CheckIfTvodAsset();
                ZipHandler.IsTvod = IsTvodPackage;
                WorkflowEntities.CheckIfAssetContainsPreviewMetadata();


                HasPoster = AdiContentManager.CheckAndRemovePosterSection();

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
            var adiData = _adiDataService.Get(i => i.TitlPaid == WorkflowEntities.TitlPaidValue);
            if(adiData?.VersionMajor != null)
                IsPackageAnUpdate = EnhancementDataValidator.ValidateVersionMajor(adiData.VersionMajor, IsTvodPackage);

            if (IsPackageAnUpdate && adiData != null)
            {
                Log.Info("Package is confirmed as a valid Update Package");
                IsPackageAnUpdate = true;
                return true;
            }

            if (IsPackageAnUpdate && adiData == null)
                Log.Error($"No Parent Package exists in the database for update package with paid: {WorkflowEntities.TitlPaidValue}, Failing ingest");

            if (!IsPackageAnUpdate && adiData != null)
            {
                Log.Error($"Package with Paid: {WorkflowEntities.TitlPaidValue} Exists in the database, failing Ingest.");
                return false;
            }

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
                    if (mapdata.availability?.start != null && mapdata.availability?.start.Year != 1)
                        availableDateTime = Convert.ToDateTime(mapdata.availability?.start);
                    break;
                }
                case "end":
                {
                    if (mapdata.availability?.end != null && mapdata.availability?.end.Year != 1)
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
                if (IsPackageAnUpdate)
                    return UpdateGnMappingData();

                var mapData = GetGnMappingData();

                var data = new GN_Mapping_Data
                {
                    GN_TMSID = WorkflowEntities.GraceNoteTmsId,
                    GN_Paid = mapData?.link.Where(i => i.idType.Equals("PAID"))
                        .Select(r => r.Value)
                        .FirstOrDefault(),

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
                GnMappingData = _gnMappingDataService.ReturnMapData(WorkflowEntities.TitlPaidValue);

                if (GnMappingData.GN_TMSID != WorkflowEntities.GraceNoteTmsId)
                {
                    Log.Info($"TMSID Mismatch updating db with new value.");
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
                //if (!IsPackageAnUpdate || IsTvodPackage)
                if (IsPackageAnUpdate && !IsTvodPackage)
                    return SetInitialUpdateData();

                Log.Info("Extracting Media from Package");

                //Extract remaining items from package
                ZipHandler.ExtractItemFromArchive(WorkflowEntities.CurrentPackage.FullName,
                    WorkflowEntities.CurrentWorkingDirectory,
                    true,
                    false);

                if (HasPoster)
                {
                    var patterns = new[] { ".jpg", ".jpeg", ".gif", ".png", ".bmp" };

                    var files = Directory
                        .GetFiles(WorkflowEntities.CurrentWorkingDirectory)
                        .Where(file => patterns.Any(file.ToLower().EndsWith))
                        .ToList();

                    if (files.Any())
                    {
                        foreach (var f in files)
                        {
                            System.IO.File.Delete(f);
                        }
                    }
                }

                if (!IsPackageAnUpdate)
                {
                    //Update content adi movie value with ts file name
                    ProdisAdiContentManager.SetAdiAssetContentField("movie", ZipHandler.ExtractedMovieAsset.Name);

                    PrimaryAsset = ZipHandler.ExtractedMovieAsset;
                }

                if (!IsPackageAnUpdate && !ZipHandler.HasPreviewAsset)
                    return SeedAdiData();


                if (!ZipHandler.HasPreviewAsset)
                    return !IsPackageAnUpdate ? SeedAdiData() : SetInitialUpdateData();

                if(ZipHandler.HasPreviewAsset)
                {
                    //Update content adi preview value with ts file name
                    ProdisAdiContentManager.SetAdiAssetContentField("preview", ZipHandler.ExtractedPreview.Name);
                    PreviewAsset = ZipHandler.ExtractedPreview;
                }
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

        public bool SeedAdiData()
        {
            try
            {
                var isMapped = _adiDataService.Get(p => p.TitlPaid == WorkflowEntities.TitlPaidValue) != null;


                if (!isMapped && !IsPackageAnUpdate)
                {
                    Log.Info("Seeding Adi Data to the database");

                    WorkflowEntities.MovieFileSize =
                        FileDirectoryManager.GetFileSize(ZipHandler.ExtractedMovieAsset.FullName);
                    WorkflowEntities.MovieChecksum =
                        FileDirectoryManager.GetFileHash(ZipHandler.ExtractedMovieAsset.FullName);

                    WorkflowEntities.PreviewFileSize = ZipHandler.HasPreviewAsset
                        ? FileDirectoryManager.GetFileSize(ZipHandler.ExtractedPreview.FullName)
                        : string.Empty;
                    WorkflowEntities.PreviewCheckSum = ZipHandler.HasPreviewAsset
                        ? FileDirectoryManager.GetFileHash(ZipHandler.ExtractedPreview.FullName)
                        : string.Empty;

                    AdiData = new Adi_Data
                    {
                        TitlPaid = WorkflowEntities.TitlPaidValue,
                        OriginalAdi = FileDirectoryManager.ReturnAdiAsAString(ZipHandler.ExtractedAdiFile.FullName),
                        VersionMajor = ProdisAdiContentManager.GetVersionMajor(),
                        VersionMinor = ProdisAdiContentManager.GetVersionMinor(),
                        ProviderId = ProdisAdiContentManager.GetProviderId(),
                        TmsId = WorkflowEntities.GraceNoteTmsId,
                        Licensing_Window_End = WorkflowEntities.IsDateTime(
                            ProdisAdiContentManager.GetLicenceEndData()
                        )
                            ? ProdisAdiContentManager.GetLicenceEndData()
                            : throw new Exception("Licensing_Window_End Is not a valid DateTime Format," +
                                                  " Rejecting Ingest"),
                        ProcessedDateTime = DateTime.Now,
                        ContentTsFile = ZipHandler.ExtractedMovieAsset.Name,
                        ContentTsFilePaid = ProdisAdiContentManager.GetAssetPaid("movie"),
                        ContentTsFileSize = WorkflowEntities.MovieFileSize,
                        ContentTsFileChecksum = WorkflowEntities.MovieChecksum,
                        PreviewFile = ZipHandler.HasPreviewAsset ? ZipHandler.ExtractedPreview.Name : "",
                        PreviewFilePaid = ZipHandler.HasPreviewAsset ? ProdisAdiContentManager.GetAssetPaid("preview") : "",
                        PreviewFileSize = WorkflowEntities.PreviewFileSize,
                        PreviewFileChecksum = WorkflowEntities.PreviewCheckSum
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
                WorkflowEntities.GraceNoteUpdateId = ApiManager.GetUpdateId();
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

        public bool SetAdiMovieEpisodeMetadata()
        {
            try
            {
                AdiContentManager.RemoveDefaultAdiNodes();

                if (!EnrichmentWorkflowEntities.IsMoviePackage)
                {
                    ProdisAdiContentManager.InsertEpisodeData(
                        WorkflowEntities.GraceNoteTmsId,
                        episodeOrdinalValue: ApiManager.GetEpisodeOrdinalValue(),
                        episodeTitle: ApiManager.GetEpisodeTitle()
                        );

                }
                
                return
                    //Get and add GN Program Data
                    _gnMappingDataService.AddGraceNoteProgramData(
                        paid: WorkflowEntities.TitlPaidValue,
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
                        descriptions: ApiManager.MovieEpisodeProgramData.descriptions
                        ) &&

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
            }
            catch (Exception ex)
            {
                LogError(
                    "SetAdiMovieEpisodeMetadata",
                    "Error Setting Title Metadata", ex);
                return false;
            }
        }

        public bool SetAdiMovieMetadata()
        {
            try
            {
                if (!IsPackageAnUpdate)
                {
                    Log.Info("Setting ADI Content Metadata");
                    var paid = WorkflowEntities.TitlPaidValue.Replace("TITL", "ASST");
                    ProdisAdiContentManager.AddAssetMetadataApp_DataNode(
                        paid,
                        "Content_CheckSum",
                        WorkflowEntities.MovieChecksum
                    );

                    ProdisAdiContentManager.AddAssetMetadataApp_DataNode(
                        paid,
                        "Content_FileSize",
                        WorkflowEntities.MovieFileSize
                    );

                    ProdisAdiContentManager.SetAdiAssetContentField(
                        "movie",
                        PrimaryAsset.Name);

                }

                if (!ZipHandler.HasPreviewAsset)
                    return true;


                Log.Info("Adding Preview metadata");
                return CheckAndAddPreviewData();
            }
            catch (Exception ex)
            {
                LogError(
                    "SetAdiMovieMetadata",
                    "Error Setting Title Movie metadata", ex);
                return false;
            }
        }

        public bool CheckAndAddPreviewData()
        {
            try
            {
                if (!EnrichmentWorkflowEntities.PackageHasPreviewMetadata)
                    return true;

                var previewpaid = WorkflowEntities.TitlPaidValue.Replace("TITL", "PREV");
                var checksum = FileDirectoryManager.GetFileHash(PreviewAsset.FullName);
                var previewSize = FileDirectoryManager.GetFileSize(PreviewAsset.FullName);


                AdiData.PreviewFile = PreviewAsset.Name;
                AdiData.PreviewFilePaid = previewpaid;
                AdiData.PreviewFileChecksum = checksum;
                AdiData.PreviewFileSize = previewSize;
                _adiDataService.Update(AdiData);

                ProdisAdiContentManager.AddAssetMetadataApp_DataNode(
                    previewpaid,
                    "Content_CheckSum",
                    checksum
                );


                ProdisAdiContentManager.AddAssetMetadataApp_DataNode(
                    previewpaid,
                    "Content_FileSize",
                    previewSize
                    
                );

                ProdisAdiContentManager.SetAdiAssetContentField(
                    "preview",
                    PreviewAsset.Name);


                return true;
            }
            catch (Exception ex)
            {
                LogError(
                    "CheckAndAddPreviewData",
                    "Error Setting Preview metadata", ex);
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
                       ProdisAdiContentManager.InsertSeriesLayerData(
                           ApiManager.ShowSeriesSeasonProgramData.connectorId,
                           ApiManager.GetSeriesId()
                           ) &&

                       //Insert the TITL Show Data
                       AdiContentManager.InsertShowData(
                           showId: ApiManager.GetShowId(),
                           showName: ApiManager.GetShowName(),
                           totalSeasons: ApiManager.GetNumberOfSeasons(),
                           descriptions: ApiManager.ShowSeriesSeasonProgramData.descriptions
                           ) &&

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
                return ProdisAdiContentManager.InsertProductionYears(
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
                if (EnrichmentWorkflowEntities.IsMoviePackage && configLookup.Image_Mapping.ToLower().Contains("_series_") ||
                    EnrichmentWorkflowEntities.IsMoviePackage && configLookup.Image_Mapping.ToLower().Contains("_show_"))
                    continue;

                //prevent duplicate processing
                if (string.IsNullOrEmpty(currentProgramType) ||
                    configLookup.Image_Mapping == currentImage)
                    continue;


                var isl = new ImageSelectionLogic
                {
                    ImageMapping = mappingData,
                    CurrentMappingData = _gnMappingDataService.ReturnMapData(WorkflowEntities.TitlPaidValue),
                    IsUpdate = IsPackageAnUpdate,
                    ConfigImageCategories = mappingData.ImageCategory,
                    ApiAssetList = AdiContentManager.ReturnAssetList()
                };

                isl.DbImagesForAsset = _gnMappingDataService.ReturnDbImagesForAsset(
                    WorkflowEntities.TitlPaidValue,
                    isl.CurrentMappingData.Id
                );

                var imageUri = isl.GetGracenoteImage(
                    configLookup.Image_Lookup, 
                    currentProgramType,
                    WorkflowEntities.TitlPaidValue, 
                    WorkflowEntities.SeasonId);

                if (string.IsNullOrEmpty(imageUri))
                    continue;

                if (!isl.DownloadImageRequired)
                    continue;

                var localImage = GetImageName(imageUri, configLookup.Image_Mapping);
                isl.DownloadImage(imageUri, localImage);

                if (IsPackageAnUpdate && !string.IsNullOrEmpty(isl.DbImages))
                {

                    InsertSuccess = ProdisAdiContentManager.UpdateImageData(
                        isl.ImageQualifier,
                        WorkflowEntities.TitlPaidValue,
                        imageUri.Replace("assets/", ""),
                        configLookup.Image_Mapping,
                        isl.GetFileAspectRatio(localImage),
                        FileDirectoryManager.GetFileHash(localImage),
                        FileDirectoryManager.GetFileSize(localImage)
                    );

                    //update image data in db and adi
                    UpdateDbImages(isl.DbImages);
                    isl.DbImages = null;
                }
                else
                {
                    //download and insert image
                    InsertSuccess = ProdisAdiContentManager.InsertImageData
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
                    UpdateDbImages(isl.DbImages);
                }

                if (InsertSuccess)
                    currentImage = configLookup.Image_Mapping;
            }

            return InsertSuccess;
        }

        public bool RemoveDerivedFromAsset()
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
                ProdisAdiContentManager.InsertProgramLayerData(
                    WorkflowEntities.GraceNoteTmsId,
                    programRootId: ApiManager.MovieEpisodeProgramData?.rootId,
                    shoDataRootId: ApiManager.ShowSeriesSeasonProgramData?.rootId);

                ProdisAdiContentManager.CheckAndAddBlockPlatformData();
                    if (WorkflowEntities.IsQamAsset && IsPackageAnUpdate)
                        AdiContentManager.SetQamUpdateContent();
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

        public bool PackageEnrichedAsset()
        {
            try
            {
                DeliveryPackage = Path.Combine(ADIWF_Config.TempWorkingDirectory,
                    $"{Path.GetFileNameWithoutExtension(WorkflowEntities.CurrentPackage.Name)}.zip");

                if (System.IO.File.Exists(DeliveryPackage))
                    System.IO.File.Delete(DeliveryPackage);

                return ZipHandler.CreateArchive(WorkflowEntities.CurrentWorkingDirectory,
                    DeliveryPackage);
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

                var tmpPackage = IsTvodPackage
                    ? Path.Combine(ADIWF_Config.TVOD_Delivery_Directory, $"{fInfo.Name}.tmp")
                    : Path.Combine(ADIWF_Config.IngestDirectory, $"{fInfo.Name}.tmp");

                var finalPackage = IsTvodPackage
                    ? Path.Combine(ADIWF_Config.TVOD_Delivery_Directory, fInfo.Name)
                    : Path.Combine(ADIWF_Config.IngestDirectory, fInfo.Name);

                Log.Info($"Moving Temp Package to ingest: {DeliveryPackage} to {tmpPackage}");
                System.IO.File.Move(DeliveryPackage, tmpPackage);
                if (System.IO.File.Exists(tmpPackage))
                {
                    Log.Info("Temp package successfully moved");
                    Log.Info($"Moving Temp Package: {tmpPackage} to Ingest package {finalPackage}");
                    System.IO.File.Move(tmpPackage, finalPackage);
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
                AdiData = _adiDataService.GetAdiData(WorkflowEntities.TitlPaidValue);
                if (AdiData.EnrichedAdi == null)
                    throw new Exception($"Previously Enriched ADI data for Paid: " +
                                        $"{WorkflowEntities.TitlPaidValue} was not found in the database?");

                //Serialize previously enriched Adi File to obtain Asset data
                WorkflowEntities.SerializeAdiFile(true, AdiData.EnrichedAdi);
                AdiData.TmsId = WorkflowEntities.GraceNoteTmsId;
                AdiData.VersionMajor = ProdisAdiContentManager.GetVersionMajor();
                AdiData.VersionMinor = ProdisAdiContentManager.GetVersionMinor();

                AdiData.Licensing_Window_End = WorkflowEntities.IsDateTime(
                    ProdisAdiContentManager.GetLicenceEndData()
                )
                    ? ProdisAdiContentManager.GetLicenceEndData()
                    : throw new Exception("Licensing_Window_End Is not a valid DateTime Format, Rejecting Ingest");


                //if tvod remove enhanced movie section
                if (IsTvodPackage && AdiData.UpdateAdi != null)
                {
                    //var movieData = EnrichmentWorkflowEntities.EnrichedAdi.Asset.Asset.FirstOrDefault(m => m.Metadata.AMS.Asset_Class == "movie");
                    //AdiContentManager.MovieContent = movieData?.Content.Value;
                    //EnrichmentWorkflowEntities.EnrichedAdi.Asset.Asset.Remove(movieData);
                    //Load the update adi ready for preview data checks
                    WorkflowEntities.SerializeAdiFile(true, AdiData.UpdateAdi, true);
                }

                //ProdisAdiContentManager.RemoveMovieContentFromUpdate();
                //Get original asset data and modify new adi.
                if (!ProdisAdiContentManager.CopyPreviouslyEnrichedAssetDataToAdi(
                    ZipHandler.HasPreviewAsset, 
                    AdiData.UpdateAdi != null))
                    return false;

                if (EnrichmentWorkflowEntities.PackageHasPreviewMetadata && AdiData.PreviewFileChecksum != null)
                {
                    var previewAsset =
                        EnrichmentWorkflowEntities.AdiFile.Asset.Asset.FirstOrDefault(p =>
                            p.Metadata.AMS.Asset_Class == "preview");

                    if (previewAsset?.Metadata.App_Data != null)
                    {
                        foreach (var appdata in (previewAsset.Metadata.App_Data))
                        {
                            if (appdata.Name.ToLower() == "content_checksum")
                            {
                                appdata.Value = AdiData.PreviewFileChecksum;
                            }

                            if (appdata.Name.ToLower() == "content_filesize")
                            {
                                appdata.Value = AdiData.PreviewFileSize;
                            }
                        }
                    }

                }

                //Update all version major values to correct value.
                if (!ProdisAdiContentManager.UpdateAllVersionMajorValues(WorkflowEntities.AdiVersionMajor))
                    return false;



                _adiDataService.Update(AdiData);
                Log.Info("Adi data updated in the database.");

                //nullify updateadi data
                if(EnrichmentWorkflowEntities.UpdateAdi != null)
                    EnrichmentWorkflowEntities.UpdateAdi = null;

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
            var baseImage = imageUri.Replace("?trim=true", "");
            var originalFileName = Path.GetFileNameWithoutExtension(baseImage);

            var newFileName = originalFileName.Replace(originalFileName,
                $"{imageMapping}_{originalFileName}{Path.GetExtension(baseImage)}");
            return Path.Combine(WorkflowEntities.CurrentWorkingDirectory, newFileName);
        }

        private void UpdateDbImages(string dbImages)
        {
            var gnMappingRow = _gnMappingDataService.ReturnMapData(WorkflowEntities.TitlPaidValue);
            gnMappingRow.GN_Images = dbImages;
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
                    AdiData = _adiDataService.GetAdiData(WorkflowEntities.TitlPaidValue);

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

                if (System.IO.File.Exists(destination))
                    System.IO.File.Delete(destination);

                if (FailedToMap)
                {
                    Log.Info($"Moving Package: {packageFile} to Failed to map directory: " +
                             $"{ADIWF_Config.MoveNonMappedDirectory}");
                    Log.Info($"This package will be retried for: {ADIWF_Config.FailedToMap_Max_Retry_Days}" +
                             $" before it is failed completely.");

                    var dt = DateTime.Now.AddDays(-Convert.ToInt32(ADIWF_Config.FailedToMap_Max_Retry_Days));

                    if (dt >= packageFile.LastWriteTime.Date)
                    {
                        Log.Warn($"Ingest file has passed the time for allowed mapping and will deleted!");
                        System.IO.File.Delete(packageFile.FullName);
                        return;
                    }
                }

                if (IsPackageAnUpdate)
                {
                    Log.Info($"Moving Package: {packageFile} to Updates Failed directory: " +
                             $"{ADIWF_Config.UpdatesFailedDirectory}");
                    System.IO.File.Move(source, destination);
                }
                else
                {
                    Log.Info($"Moving Package: {packageFile} to Failed directory: " +
                             $"{ADIWF_Config.FailedDirectory}");

                    System.IO.File.Move(source, destination);
                }


                if (System.IO.File.Exists(destination))
                    Log.Info("Move to failed directory successful.");
                
                FileDirectoryManager.RemoveExistingTempDirectory(WorkflowEntities.CurrentWorkingDirectory);
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
                System.IO.File.Delete(packageFile.FullName);
                if (!System.IO.File.Exists(packageFile.FullName))
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