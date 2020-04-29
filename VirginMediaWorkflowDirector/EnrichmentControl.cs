using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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



namespace VirginMediaWorkflowDirector
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
        private readonly IMappingsUpdateTrackingService _mappingsUpdateTrackingService;
        private readonly ILayer1UpdateTrackingService _layer1UpdateTrackingService;
        private readonly ILayer2UpdateTrackingService _layer2UpdateTrackingService;

        private EnrichmentWorkflowEntities WorkflowEntities { get; }
        private AdiContentController AdiContentManager { get; }
        private GN_Mapping_Data GnMappingData { get; set; }
        private GraceNoteApiManager ApiManager { get; }
        private string DeliveryPackage { get; set; }
        public bool IsPackageAnUpdate { get; set; }
        private ZipHandler ZipHandler { get; set; }
        public FileInfo PrimaryAsset { get; set; }
        public FileInfo PreviewAsset { get; set; }
        private bool IsTvodPackage { get; set; }
        private bool InsertSuccess { get; set; }
        public bool FailedToMap { get; set; }
        private Adi_Data AdiData { get; set; }
        private bool HasPoster { get; set; }

        public EnrichmentControl()
        {
            ApiManager = new GraceNoteApiManager();
            WorkflowEntities = new EnrichmentWorkflowEntities();
            AdiContentManager = new AdiContentController();
            _adiDataService = new AdiEnrichmentManager(new EfAdiEnrichmentDal());
            _gnImageLookupService = new GnImageLookupManager(new EfGnImageLookupDal());
            _gnMappingDataService = new GnMappingDataManager(new EfGnMappingDataDal());
            _mappingsUpdateTrackingService = new MappingsUpdateTrackingManager(new EfMappingsUpdateTrackingDal());
            _layer1UpdateTrackingService = new Layer1UpdateTrackingManager(new EfLayer1UpdateTrackingDal());
            _layer2UpdateTrackingService = new Layer2UpdateTrackingManager(new EfLayer2UpdateTrackingDal());
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
                ZipHandler.IsUpdatePackage = false;
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

                if (ApiManager.CoreGnMappingData.programMappings.programMapping == null)
                {
                    Log.Warn($"Processing Stopped as mapping data is not ready, package will be retried for {ADIWF_Config.FailedToMap_Max_Retry_Days} days before failing!");
                    FailedToMap = true;
                    return false;
                }

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
                    Log.Warn($"Processing Stopped as mapping data is not ready, package will be retried for {ADIWF_Config.FailedToMap_Max_Retry_Days} days before failing!");
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
                    "Error During Mapping of GracenoteData", ex);
                return false;
            }
        }

        public bool ValidatePackageIsUnique()
        {
            var adiData = _adiDataService.Get(i => i.TitlPaid == WorkflowEntities.TitlPaidValue);

            if (IsPackageAnUpdate && adiData == null)
            {
                Log.Error(
                    $"No Parent Package exists in the database for update package with paid: {WorkflowEntities.TitlPaidValue}, Failing ingest");
                return false;
            }

            if (adiData?.VersionMajor != null)
                IsPackageAnUpdate = EnhancementDataValidator.ValidateVersionMajor(adiData.VersionMajor, adiData.VersionMinor, IsTvodPackage);

            if (IsPackageAnUpdate && adiData != null)
            {
                Log.Info("Package is confirmed as a valid Update Package");

                Log.Info($"IngestUUID: {adiData.IngestUUID} Extracted from the database.");
                IsPackageAnUpdate = true;
                WorkflowEntities.IngestUuid = adiData.IngestUUID;
                return true;
            }


            if (!IsPackageAnUpdate && adiData != null && !EnhancementDataValidator.UpdateVersionFailure)
            {
                Log.Error($"Package with Paid: {WorkflowEntities.TitlPaidValue} Exists in the database, failing Ingest.");
                return false;
            }
            if (!IsPackageAnUpdate && adiData != null && EnhancementDataValidator.UpdateVersionFailure)
            {
                return false;
            }

            Log.Info($"Package with Paid: {WorkflowEntities.TitlPaidValue} " +
                     "confirmed as a unique package, continuing ingest operations.");

            WorkflowEntities.IngestUuid = Guid.NewGuid();
            Log.Info($"New package Identifier Generated: {WorkflowEntities.IngestUuid}");

            return true;

        }

        private GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping GetGnMappingData()
        {
            return ApiManager.CoreGnMappingData
                .programMappings
                .programMapping
                .FirstOrDefault(m => m.status == GnOnApiProgramMappingSchema.onProgramMappingsProgramMappingStatus.Mapped);
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
                var mapData = GetGnMappingData();
                WorkflowEntities.GnMappingPaid = mapData.link.Where(i => i.idType.Equals("PAID"))
                    .Select(r => r.Value)
                    .FirstOrDefault();

                if (IsPackageAnUpdate)
                    return UpdateGnMappingData();

                

                //secondary check
                if (mapData.status == GnOnApiProgramMappingSchema.onProgramMappingsProgramMappingStatus.Mapped)
                {
                    Log.Info($"Asset Mapping status: {mapData.status}, Catalog Name: {mapData.catalogName}");
                    

                    var data = new GN_Mapping_Data
                    {
                        IngestUUID = WorkflowEntities.IngestUuid,
                        GN_TMSID = WorkflowEntities.GraceNoteTmsId,
                        GN_Paid = mapData.link.Where(i => i.idType.Equals("PAID"))
                            .Select(r => r.Value)
                            .FirstOrDefault(),

                        GN_RootID = mapData.id.Where(t => t.type.Equals("rootId"))
                            .Select(r => r.Value)
                            .FirstOrDefault(),

                        GN_Status = mapData.status.ToString(),
                        GN_ProviderId = mapData.link.Where(i => i.idType.Equals("ProviderId"))
                            .Select(r => r.Value)
                            .FirstOrDefault(),

                        GN_Pid = mapData.link.Where(i => i.idType.Equals("PID"))
                            .Select(r => r.Value)
                            .FirstOrDefault(),

                        GN_programMappingId = mapData.programMappingId,
                        GN_creationDate = mapData.creationDate != null
                            ? Convert.ToDateTime(mapData.creationDate)
                            : DateTime.Now,
                        GN_updateId = mapData.updateId,
                        GN_Availability_Start = GetAvailability("start", mapData),
                        GN_Availability_End = GetAvailability("end", mapData)
                    };

                    GnMappingData = _gnMappingDataService.Add(data);
                    Log.Info($"Gracenote Mapping data seeded to the database with Row Id: {GnMappingData.Id}");
                    return true;

                }

                Log.Error($"Package {WorkflowEntities.TitlPaidValue} is not mapped, Status returned: {mapData.status.ToString()}, Catalog Name: {mapData.catalogName}");
                FailedToMap = true;
                return false;


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
                GnMappingData = _gnMappingDataService.ReturnMapData(WorkflowEntities.IngestUuid);

                if (GnMappingData == null)
                    throw new NullReferenceException(
                        $"Failed to update the GN Mapping table no Data received for IngestUuid:{WorkflowEntities.IngestUuid}! Is this Ingest a genuine Update?");


                if (GnMappingData.GN_TMSID != WorkflowEntities.GraceNoteTmsId)
                {
                    Log.Info("TMSID Mismatch updating ADI_Data and Layer1UpdateTracking Table with new value.");
                    GnMappingData.GN_TMSID = WorkflowEntities.GraceNoteTmsId;
                    //Update All TMSID's in the Layer1 tracking table with the tmsid update
                    var layer1Data =
                        _layer1UpdateTrackingService.GetList(t => t.GN_TMSID == GnMappingData.GN_TMSID);

                    foreach (var l1Item in layer1Data.ToList())
                    {
                        Log.Info($"Updating TMSID in the Layer1 table with ingestUUID: {l1Item.IngestUUID} with new TmsID: {WorkflowEntities.GraceNoteTmsId}");
                        l1Item.GN_TMSID = WorkflowEntities.GraceNoteTmsId;
                        _layer1UpdateTrackingService.Update(l1Item);
                    }
                }

                Log.Info("Updating GN_Mapping_Data table with new gracenote mapping data.");
                var mapData = GetGnMappingData();
                GnMappingData.GN_Availability_Start = GetAvailability("start", mapData);
                GnMappingData.GN_Availability_End = GetAvailability("end", mapData);
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
                            File.Delete(f);
                        }
                    }
                }

                if (!IsPackageAnUpdate)
                {
                    //Update content adi movie value with ts file name
                    AdiContentController.SetAdiAssetContentField("movie", ZipHandler.ExtractedMovieAsset.Name);

                    PrimaryAsset = ZipHandler.ExtractedMovieAsset;
                }

                if (!IsPackageAnUpdate && !ZipHandler.HasPreviewAsset)
                    return SeedAdiData();


                if (!ZipHandler.HasPreviewAsset)
                    return !IsPackageAnUpdate ? SeedAdiData() : SetInitialUpdateData();

                if (ZipHandler.HasPreviewAsset)
                {
                    //Update content adi preview value with ts file name
                    AdiContentController.SetAdiAssetContentField("preview", ZipHandler.ExtractedPreview.Name);
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
                        IngestUUID = WorkflowEntities.IngestUuid,
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
                        ProcessedDateTime = DateTime.Now,
                        ContentTsFile = ZipHandler.ExtractedMovieAsset.Name,
                        ContentTsFilePaid = AdiContentController.GetAssetPaid("movie"),
                        ContentTsFileSize = WorkflowEntities.MovieFileSize,
                        ContentTsFileChecksum = WorkflowEntities.MovieChecksum,
                        PreviewFile = ZipHandler.HasPreviewAsset ? ZipHandler.ExtractedPreview.Name : "",
                        PreviewFilePaid = ZipHandler.HasPreviewAsset ? AdiContentController.GetAssetPaid("preview") : "",
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
                //ensure this is updated in the current dataset
                if (AdiContentController.DbImagesNullified)
                    GnMappingData.GN_Images = string.Empty;

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

                Log.Info("[GetGracenoteProgramEpisodeData] Successfully update GN Mapping table.");
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
                    AdiContentController.InsertEpisodeData(
                        WorkflowEntities.GraceNoteTmsId,
                        episodeOrdinalValue: ApiManager.GetEpisodeOrdinalValue(),
                        episodeTitle: ApiManager.GetEpisodeTitle()
                        );

                }

                return
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
                    AdiContentController.AddAssetMetadataApp_DataNode(
                        paid,
                        "Content_CheckSum",
                        WorkflowEntities.MovieChecksum
                    );

                    AdiContentController.AddAssetMetadataApp_DataNode(
                        paid,
                        "Content_FileSize",
                        WorkflowEntities.MovieFileSize
                    );

                    AdiContentController.SetAdiAssetContentField(
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

                AdiContentController.AddAssetMetadataApp_DataNode(
                    previewpaid,
                    "Content_CheckSum",
                    checksum
                );


                AdiContentController.AddAssetMetadataApp_DataNode(
                    previewpaid,
                    "Content_FileSize",
                    previewSize

                );

                AdiContentController.SetAdiAssetContentField(
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
                ApiManager.SetSeasonData();
                AdiContentManager.SeasonInfo = ApiManager.GetSeasonInfo();
                //Insert IMDB Data
                return AdiContentManager.InsertIdmbData(
                           ApiManager.ExternalLinks(),
                           HasMovieInfo()) &&

                       AdiContentController.InsertSeriesLayerData(
                           ApiManager.ShowSeriesSeasonProgramData.connectorId,
                           ApiManager.GetSeriesId()) &&

                       AdiContentManager.InsertShowData(
                           showId: ApiManager.GetShowId(),
                           showName: ApiManager.GetShowName(),
                           totalSeasons: ApiManager.GetNumberOfSeasons(),
                           descriptions: ApiManager.ShowSeriesSeasonProgramData.descriptions) &&

                       AdiContentManager.InsertSeriesGenreData() &&

                       AdiContentManager.InsertSeriesData(
                           ApiManager.GetGnSeriesId(),
                           seriesOrdinalValue: ApiManager.GetSeriesOrdinalValue(),
                           ApiManager.GetSeasonId(),
                           episodeSeason: ApiManager.GetEpisodeSeason());
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
                AdiContentController.InsertProductionYears(
                    ApiManager.GetSeriesPremiere(),
                    ApiManager.GetSeasonPremiere(),
                    ApiManager.GetSeriesFinale(),
                    ApiManager.GetSeasonFinale()
                    );

                //return true here to continue flow and allow for the fact that some items do not require season items
                //returns false on error only
                return true;
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
                    CurrentMappingData = _gnMappingDataService.ReturnMapData(WorkflowEntities.IngestUuid),
                    IsUpdate = IsPackageAnUpdate,
                    ConfigImageCategories = mappingData.ImageCategory,
                    ApiAssetList = AdiContentManager.ReturnAssetList()
                };

                isl.DbImagesForAsset = _gnMappingDataService.ReturnDbImagesForAsset(
                    WorkflowEntities.GnMappingPaid,
                    isl.CurrentMappingData.Id
                );

                var imageUri = isl.GetGracenoteImage(configLookup.Image_Lookup);

                if (string.IsNullOrEmpty(imageUri))
                    continue;

                if (!isl.DownloadImageRequired)
                    continue;

                var localImage = GetImageName(imageUri, configLookup.Image_Mapping);
                isl.DownloadImage(imageUri, localImage);
                var imagepaid = $"{isl.ImageQualifier}{WorkflowEntities.TitlPaidValue.Replace("TITL", "")}";

                var imageExists = EnrichmentWorkflowEntities.AdiFile.Asset.Asset.FirstOrDefault(i => i.Metadata.AMS.Asset_ID == imagepaid);
                if (imageExists != null)
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

                }
                //If its an update but the image is new then we will get a false back
                //therefore update the adi
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
                }


                //update image data in db and adi
                UpdateDbImages();
                SchTech.Business.Manager.Concrete.ImageLogic.ImageSelectionLogic.DbImages = null;

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
                AdiContentController.InsertProgramLayerData(
                    WorkflowEntities.GraceNoteTmsId,
                    programRootId: ApiManager.MovieEpisodeProgramData?.rootId,
                    shoDataRootId: ApiManager.ShowSeriesSeasonProgramData?.rootId);

                AdiContentController.CheckAndAddBlockPlatformData();
                if (WorkflowEntities.IsQamAsset && IsPackageAnUpdate)
                    AdiContentManager.SetQamUpdateContent();

                //Ensure db checksum is correct
                var enrichedChecksum =
                    EnrichmentWorkflowEntities.AdiFile.Asset.Asset.FirstOrDefault(a =>
                        a.Metadata.AMS.Asset_Class == "movie");
                AdiData.ContentTsFileChecksum = enrichedChecksum?.Metadata.App_Data
                    .FirstOrDefault(c => c.Name.ToLower() == "content_checksum")
                    ?.Value;

                AdiData.ContentTsFileSize = enrichedChecksum?.Metadata.App_Data
                    .FirstOrDefault(c => c.Name.ToLower() == "content_filesize")?.Value;

                _adiDataService.Update(AdiData);

                Log.Info("Setting Updates Tracking data.");
                if (AddOrUpdateMappingTrackingData() &&
                    AddOrUpdateLayer1TrackingData() &&
                    AddOrUpdateLayer2TrackingData())
                {
                    Log.Info("Successfully Set Tracking Data.");
                }
                else
                {
                    Log.Error("Failed to Set Tracking Data Check previous log entries.");
                    return false;
                }
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

        private bool AddOrUpdateMappingTrackingData()
        {
            try
            {
                var mapTracking = _mappingsUpdateTrackingService.Get(m => m.IngestUUID == AdiData.IngestUUID);

                if (mapTracking == null)
                {

                    mapTracking = new MappingsUpdateTracking
                    {
                        IngestUUID =  AdiData.IngestUUID,
                        GN_ProviderId = AdiData.ProviderId,
                        Mapping_MaxUpdateId = GnMappingData.GN_updateId,
                        Mapping_NextUpdateId = GnMappingData.GN_updateId,
                        Mapping_RootId = GnMappingData.GN_RootID,
                        Mapping_UpdateDate = DateTime.Now,
                        Mapping_UpdateId = GnMappingData.GN_updateId,
                        UpdatesChecked = DateTime.Now,
                        RequiresEnrichment = false
                    };

                    _mappingsUpdateTrackingService.Add(mapTracking);
                }
                else
                {
                    mapTracking.Mapping_UpdateId = GnMappingData.GN_updateId;
                    mapTracking.UpdatesChecked = DateTime.Now;
                    mapTracking.Mapping_RootId = GnMappingData.GN_RootID;
                    mapTracking.RequiresEnrichment = false;

                    _mappingsUpdateTrackingService.Update(mapTracking);
                }
                return true;
            }
            catch (Exception aoumtdException)
            {
                LogError("AddOrUpdateMappingTrackingData", "Failed to Set Mapping Tracking data.", aoumtdException);
                return false;
            }
        }

        private bool AddOrUpdateLayer1TrackingData()
        {
            try
            {
                var layer1Tracking = _layer1UpdateTrackingService.Get(l => l.IngestUUID == AdiData.IngestUUID);

                if (layer1Tracking == null)
                {

                    layer1Tracking = new Layer1UpdateTracking
                    {
                        IngestUUID = AdiData.IngestUUID,
                        GN_TMSID = GnMappingData.GN_TMSID,
                        GN_Paid = GnMappingData.GN_Paid,
                        Layer1_MaxUpdateId = GnMappingData.GN_updateId,
                        Layer1_NextUpdateId = GnMappingData.GN_updateId,
                        Layer1_RootId = GnMappingData.GN_RootID,
                        Layer1_UpdateDate = DateTime.Now,
                        Layer1_UpdateId = GnMappingData.GN_updateId,
                        RequiresEnrichment = false
                    };

                    _layer1UpdateTrackingService.Add(layer1Tracking);
                }
                else
                {
                    layer1Tracking.GN_Paid = GnMappingData.GN_Paid;
                    layer1Tracking.GN_TMSID = GnMappingData.GN_TMSID;
                    layer1Tracking.Layer1_UpdateId = GnMappingData.GN_updateId;
                    layer1Tracking.Layer1_RootId = GnMappingData.GN_RootID;
                    layer1Tracking.UpdatesChecked = DateTime.Now;
                    layer1Tracking.RequiresEnrichment = false;

                    _layer1UpdateTrackingService.Update(layer1Tracking);
                }
                return true;
            }
            catch (Exception aoul1TdException)
            {
                LogError("AddOrUpdateLayer1TrackingData", "Failed to Set Layer1 Tracking data.", aoul1TdException);
                return false;
            }
        }

        private bool AddOrUpdateLayer2TrackingData()
        {
            try
            {
                var layer2Tracking = _layer2UpdateTrackingService.Get(l => l.IngestUUID == AdiData.IngestUUID);

                if (layer2Tracking == null)
                {

                    layer2Tracking = new Layer2UpdateTracking
                    {
                        Id = 0,
                        IngestUUID = AdiData.IngestUUID,
                        GN_connectorId = GnMappingData.GN_connectorId,
                        GN_Paid = GnMappingData.GN_Paid,
                        Layer2_MaxUpdateId = GnMappingData.GN_updateId,
                        Layer2_NextUpdateId = GnMappingData.GN_updateId,
                        Layer2_RootId = GnMappingData.GN_RootID,
                        Layer2_UpdateDate = DateTime.Now,
                        Layer2_UpdateId = GnMappingData.GN_updateId,
                        RequiresEnrichment = false
                    };

                    _layer2UpdateTrackingService.Add(layer2Tracking);
                }
                else
                {
                    layer2Tracking.GN_Paid = GnMappingData.GN_Paid;
                    layer2Tracking.GN_connectorId = GnMappingData.GN_connectorId;
                    layer2Tracking.Layer2_UpdateId = GnMappingData.GN_updateId;
                    layer2Tracking.Layer2_RootId = GnMappingData.GN_RootID;
                    layer2Tracking.UpdatesChecked = DateTime.Now;
                    layer2Tracking.RequiresEnrichment = false;

                    _layer2UpdateTrackingService.Update(layer2Tracking);
                }
                return true;
            }
            catch (Exception aoul1TdException)
            {
                LogError("AddOrUpdateLayer1TrackingData", "Failed to Set Layer1 Tracking data.", aoul1TdException);
                return false;
            }
        }

        public bool PackageEnrichedAsset()
        {
            try
            {
                DeliveryPackage = Path.Combine(ADIWF_Config.TempWorkingDirectory,
                    $"{Path.GetFileNameWithoutExtension(WorkflowEntities.CurrentPackage.Name)}.zip");

                if (File.Exists(DeliveryPackage))
                    File.Delete(DeliveryPackage);

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
                    throw new Exception($"Previously Enriched ADI data for Paid: " +
                                        $"{WorkflowEntities.TitlPaidValue} was not found in the database?");

                //Serialize previously enriched Adi File to obtain Asset data

                if(AdiData.UpdateAdi != null)
                    WorkflowEntities.SerializeAdiFile(true, AdiData.UpdateAdi, true);
                else
                    WorkflowEntities.SerializeAdiFile(true, AdiData.EnrichedAdi);
                AdiData.TmsId = WorkflowEntities.GraceNoteTmsId;
                AdiData.VersionMajor = AdiContentController.GetVersionMajor();
                AdiData.VersionMinor = AdiContentController.GetVersionMinor();

                AdiData.Licensing_Window_End = WorkflowEntities.IsDateTime(
                    AdiContentController.GetLicenceEndData()
                )
                    ? AdiContentController.GetLicenceEndData()
                    : throw new Exception("Licensing_Window_End Is not a valid DateTime Format, Rejecting Ingest");


                if (IsTvodPackage && AdiData.UpdateAdi != null)
                {
                    WorkflowEntities.SerializeAdiFile(true, AdiData.UpdateAdi, true);
                }

                //AdiContentController.RemoveMovieContentFromUpdate();
                //Get original asset data and modify new adi.
                if (!AdiContentController.CopyPreviouslyEnrichedAssetDataToAdi(AdiData.IngestUUID,
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
                if (!AdiContentController.UpdateAllVersionMajorValues(WorkflowEntities.AdiVersionMajor))
                    return false;



                _adiDataService.Update(AdiData);
                Log.Info("Adi data updated in the database.");

                //nullify updateadi data
                if (EnrichmentWorkflowEntities.UpdateAdi != null)
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

        private string GetFailureDirectory(string packageName)
        {
            if (FailedToMap)
            {
                return $"{ADIWF_Config.MoveNonMappedDirectory}\\{packageName}";
            }

            if (IsPackageAnUpdate || EnhancementDataValidator.UpdateVersionFailure)
            {
                // set here to ensure correct cleanup is run
                IsPackageAnUpdate = true;
                return $"{ADIWF_Config.UpdatesFailedDirectory}\\{packageName}";
            }

            if (EnrichmentWorkflowEntities.IsSdContent)
            {
                return $"{ADIWF_Config.UnrequiredSDContentDirectory}\\{packageName}";
            }



            return $"{ADIWF_Config.FailedDirectory}\\{packageName}";
        }


        public void ProcessFailedPackage(FileInfo packageFile)
        {
            try
            {
                var source = packageFile.FullName;
                var destination = GetFailureDirectory(packageFile.Name);

                if (FailedToMap)
                {
                    Log.Info($"Setting Package: {packageFile} Move Destination to Failed to map directory: " +
                             $"{ADIWF_Config.MoveNonMappedDirectory}");

                    Log.Info($"This package will be retried for: {ADIWF_Config.FailedToMap_Max_Retry_Days}" +
                             $" days before it is failed completely.");

                    var dt = DateTime.Now.AddDays(-Convert.ToInt32(ADIWF_Config.FailedToMap_Max_Retry_Days));

                    if (dt >= packageFile.LastWriteTime.Date)
                    {
                        Log.Warn($"Ingest file has passed the time for allowed mapping and will deleted!");
                        File.Delete(packageFile.FullName);
                        RemoveWorkingDirectory();
                        return;
                    }
                    if (File.Exists(destination))
                    {
                        Log.Info("No package move required for Mapping failure retry.");
                        RemoveWorkingDirectory();
                        return;
                    }
                }

                if (IsPackageAnUpdate)
                {
                    Log.Info($"Setting Package: {packageFile} Move Destination to Updates Failed directory: " +
                             $"{ADIWF_Config.UpdatesFailedDirectory}");
                    File.Move(source, destination);
                }
                else
                {
                    Log.Info(FailedToMap
                        ? $"Moving Package: {packageFile} to Failed directory: {destination}"
                        : $"Moving Package: {packageFile} to Failed to Map directory: {destination}");

                    if (File.Exists(destination) && source != destination)
                        File.Delete(destination);

                    File.Move(source, destination);
                }


                if (File.Exists(destination))
                    Log.Info("Move to failed directory successful.");

                FileDirectoryManager.RemoveExistingTempDirectory(WorkflowEntities.CurrentWorkingDirectory);

                if (IsPackageAnUpdate || EnrichmentWorkflowEntities.IsDuplicateIngest)
                    return;

                Log.Info($"Removing db entries for Failed Package.");
                if (GnMappingData?.GN_Paid != null)
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

                RemoveWorkingDirectory();

            }
            catch (Exception pcuex)
            {
                LogError(
                    "PackageCleanup",
                    "Error Cleaning up processed Package", pcuex);
            }
        }

        private void RemoveWorkingDirectory()
        {
            try
            {
                Log.Info($"Removing working directory: {WorkflowEntities.CurrentWorkingDirectory}");
                
                if (Directory.Exists(WorkflowEntities.CurrentWorkingDirectory))
                    Directory.Delete(WorkflowEntities.CurrentWorkingDirectory, true);

                Log.Info($"Successfully deleted Working directory {WorkflowEntities.CurrentWorkingDirectory}");
            }
            catch (Exception rmwdEx)
            {
                LogError(
                    "RemoveWorkingDirectory",
                    "Error Cleaning up Working Directory", rmwdEx);
            }
        }

        public void CleanStaticReferences()
        {
            try
            {
                var properties = typeof(ZipHandler).GetProperties();
                ClearProperties(properties);

                properties = typeof(EnrichmentWorkflowEntities).GetProperties();
                ClearProperties(properties);
            }
            catch (Exception csrex)
            {
                LogError(
                    "CleanStaticReferences",
                    "Error Cleaning up Static references", csrex);
            }
        }

        private static void ClearProperties(IEnumerable<PropertyInfo> propertyInfos)
        {
            if (propertyInfos == null)
                return;

            foreach (var item in propertyInfos)
            {
                try
                {
                    item.SetValue(null, null);
                }
                catch
                {
                    //do nothing
                }
            }
        }
    }
}
