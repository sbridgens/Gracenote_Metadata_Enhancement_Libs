using log4net;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.File.Manager.Concrete.Serialization;
using SchTech.Web.Manager.Concrete;
using System;
using System.IO;
using System.Linq;
using SchTech.Configuration.Manager.Schema.GNUpdateTracker;

namespace SchTech.Entities.ConcreteTypes
{
    public class EnrichmentWorkflowEntities
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(EnrichmentWorkflowEntities));

        private XmlSerializationManager<ADI> XmlSerializer { get; set; }
        
        public static ADI AdiFile { get; set; }

        public static ADI EnrichedAdi { get; set; }

        public static ADI UpdateAdi { get; set; }

        public Guid IngestUuid { get; set; }

        public string MovieFileSize { get; set; }

        public string PreviewFileSize { get; set; }

        public string MovieChecksum { get; set; }

        public string PreviewCheckSum { get; set; }

        public static bool IsSdContent { get; set; }

        public static bool PackageIsAOneOffSpecial { get; set; }

        public static bool IsMoviePackage { get; set; }

        public static bool IsEpisodeSeries { get; set; }

        public static bool PackageHasPreviewMetadata { get; set; }

        public static bool IsDuplicateIngest { get; set; }

        public bool HasPackagesToProcess { get; set; }

        public FileInfo CurrentPackage { get; set; }

        public string CurrentWorkingDirectory { get; set; }

        public string TitlPaidValue { get; set; }

        public string GnMappingPaid { get; set; }
        
        public bool IsQamAsset { get; set; }

        public string OnapiProviderid { get; set; }

        public string GracenoteMappingData { get; private set; }

        public string GracenoteProgramData { get; private set; }

        public string GraceNoteUpdateData { get; private set; }

        public string GraceNoteSeriesSeasonSpecialsData { get; private set; }

        public string GraceNoteTmsId { get; set; }

        public string GraceNoteRootId { get; set; }

        public string GraceNoteUpdateId { get; set; }

        public string GraceNoteConnectorId { get; set; }

        public int AdiVersionMajor { get; private set; }

        public int AdiVersionMinor { get; private set; }

        public int SeasonId { get; set; }

        public bool IsDateTime(string txtDate) => DateTime.TryParse(txtDate, out _);

        public void SetCurrentWorkingDirectory()
        {
            CurrentWorkingDirectory = Path.Combine(ADIWF_Config.TempWorkingDirectory,
                Path.GetFileNameWithoutExtension(CurrentPackage.Name));
        }

        public bool SerializeAdiFile(bool isUpdate, string adiData = "", bool loadUpdateAdi = false)
        {
            try
            {
                XmlSerializer = new XmlSerializationManager<ADI>();

                if (!isUpdate)
                {
                    AdiFile = new ADI();
                    AdiFile = XmlSerializer.Read(
                        System.IO.File.ReadAllText(Path.Combine(CurrentWorkingDirectory, "ADI.xml")));
                }
                if(isUpdate && !loadUpdateAdi)
                {
                    EnrichedAdi = new ADI();
                    EnrichedAdi = XmlSerializer.Read(adiData);
                }

                if (loadUpdateAdi)
                {
                    Log.Info("Loading DB Update ADI.");
                    UpdateAdi = new ADI();
                    UpdateAdi = XmlSerializer.Read(adiData);
                }
                if (!isUpdate && AdiFile == null)
                    throw new Exception("Adi file is null check namespaces and adi document structure?");

                Log.Info("ADI Loaded correctly and will continue processing.");

                if(AdiFile != null && !isUpdate)
                {
                    AdiVersionMajor = AdiFile.Metadata.AMS.Version_Major;
                    AdiVersionMinor = AdiFile.Metadata.AMS.Version_Minor;
                }
                else if(isUpdate && UpdateAdi != null)
                {
                    AdiVersionMajor = UpdateAdi.Metadata.AMS.Version_Major;
                    AdiVersionMinor = UpdateAdi.Metadata.AMS.Version_Minor;
                }

                Log.Info($"Asset Version Major: {AdiVersionMajor}");
                return true;

            }
            catch (Exception adfEx)
            {
                Log.Error($"[SerializeAdiFile] Error during serialization of ADI file: {adfEx.Message}");
                if (adfEx.InnerException != null)
                    Log.Error($"[SerializeAdiFile] Inner exception: {adfEx.InnerException.Message}");

                return false;
            }
        }

        public void CheckIfAssetContainsPreviewMetadata()
        {
            PackageHasPreviewMetadata = AdiFile.Asset.Asset.Any(e => e.Metadata.AMS.Asset_Class != null &&
                                                                     e.Metadata.AMS.Asset_Class.ToLower()
                                                           .Equals("preview"));
            if (PackageHasPreviewMetadata)
                Log.Info("Package Contains a Preview Metadata.");
        }

        public static bool CheckIfTvodAsset()
        {
            ADIAssetAsset first = null;
            first = AdiFile != null ? AdiFile.Asset.Asset?.FirstOrDefault() : UpdateAdi.Asset.Asset?.FirstOrDefault();
            
            if (first == null || !first.Metadata.AMS.Product.ToLower().Contains("tvod"))
                return false;

            Log.Info("Package Detected as a TVOD Asset.");
            return true;
        }

        public bool CheckSetSdPackage(bool isupdate)
        {
            if (isupdate)
                return true;

            var adiAssetAssetMetadata = AdiFile.Asset.Asset?.FirstOrDefault()?.Metadata;

            if (adiAssetAssetMetadata != null)
                IsSdContent = (adiAssetAssetMetadata.App_Data)
                              .FirstOrDefault(c => c.Name.ToLower() == "hdcontent")
                              ?.Value.ToLower() != "y";

            if (IsSdContent && !Convert.ToBoolean(ADIWF_Config.AllowSDContentIngest))
                throw new InvalidOperationException(
                    $"SD Content Detected, Configuration disallows SD Content from Ingest; Failing ingest for {TitlPaidValue}");

            if(IsSdContent)
                Log.Info("Content is marked as SD Content, Configuration allows SD content for ingest.");

            return IsSdContent;
        }

        public bool GetGracenoteMappingData()
        {
            try
            {
                var mapUrl = $"{ADIWF_Config.OnApi}ProgramMappings?" +
                             $"providerId={OnapiProviderid}&" +
                             $"api_key={ADIWF_Config.ApiKey}";

                GracenoteMappingData = null;

                Log.Info($"Calling On API Mappings url with Provider Value: {OnapiProviderid}");
                var webClient = new WebClientManager();
                GracenoteMappingData = webClient.HttpGetRequest(mapUrl);
                Log.Debug($"RECEIVED MAPPING DATA FROM GRACENOTE: \r\n{GracenoteMappingData}");
                if (GracenoteMappingData != null && webClient.SuccessfulWebRequest) return true;

                throw new Exception($"Gracenote Mapping Data: {GracenoteMappingData}, " +
                                    $"Successful Web request: {webClient.SuccessfulWebRequest}," +
                                    $"Web request response code: {webClient.RequestStatusCode}");
            }
            catch (Exception ggmdEx)
            {
                Log.Error($"[GetGracenoteMappingData] Error obtaining Gracenote mapping data: {ggmdEx.Message}");
                if (ggmdEx.InnerException != null)
                    Log.Error($"[GetGracenoteMappingData] Inner exception: {ggmdEx.InnerException.Message}");
            }

            return false;
        }

        public bool GetGraceNoteUpdates(string updateId, string apiCall, string resultLimit)
        {
            try
            {
                //http://on-api.gracenote.com/v3/ProgramMappings?updateId=10938407543&limit=100&api_key=wgu7uhqcqyzspwxj28mxgy4b
                var updateUrl = $"{GN_UpdateTracker_Config.OnApi}{apiCall}" +
                             $"?updateId={updateId}" +
                             $"&limit={resultLimit}" +
                             $"&api_key={GN_UpdateTracker_Config.ApiKey}";

                GraceNoteUpdateData = null;

                Log.Info($"Calling On API url with Update Value: {updateId} and Limit: {resultLimit}");
                var webClient = new WebClientManager();
                GraceNoteUpdateData = webClient.HttpGetRequest(updateUrl);
                Log.Info("Successfully Called Gracenote OnApi");
                if (GraceNoteUpdateData != null && webClient.SuccessfulWebRequest)
                    return true;

                throw new Exception($"Gracenote Update Data: {GraceNoteUpdateData}, " +
                                    $"Successful Web request: {webClient.SuccessfulWebRequest}," +
                                    $"Web request response code: {webClient.RequestStatusCode}");
            }
            catch (Exception ggnmuex)
            {
                Log.Error($"[GetGraceNoteUpdates] Error obtaining Gracenote Update data: {ggnmuex.Message}");
                if (ggnmuex.InnerException != null)
                    Log.Error($"[GetGraceNoteUpdates] Inner exception: {ggnmuex.InnerException.Message}");
            }

            return false;
        }

        public bool GetGracenoteProgramData()
        {
            try
            {
                Log.Info($"Retrieving MetaData from On API using TMSId: {GraceNoteTmsId}");
                var programUrl = $"{ADIWF_Config.OnApi}Programs?" +
                                 $"tmsId={GraceNoteTmsId}&" +
                                 $"api_key={ADIWF_Config.ApiKey}";

                GracenoteProgramData = null;

                Log.Info($"Calling On API Programs url with TmsId Value: {GraceNoteTmsId}");
                var webClient = new WebClientManager();
                GracenoteProgramData = webClient.HttpGetRequest(programUrl);

                if (GracenoteProgramData != null && webClient.SuccessfulWebRequest) return true;

                throw new Exception("Error during receive of GN Api Program data, " +
                                    $"Web request data: {webClient.SuccessfulWebRequest}," +
                                    $"Web request response code: {webClient.RequestStatusCode}");
            }
            catch (Exception ggpdEx)
            {
                Log.Error("[GetGracenoteProgramData] Error obtaining " +
                          $"Gracenote Program data: {ggpdEx.Message}");

                if (ggpdEx.InnerException != null)
                    Log.Error("[GetGracenoteProgramData] " +
                              $"Inner exception: {ggpdEx.InnerException.Message}");

                return false;
            }
        }

        public bool GetGraceNoteSeriesSeasonSpecialsData()
        {
            try
            {
                var requestUrl = $"{ADIWF_Config.OnApi}Programs?" +
                             $"tmsId={GraceNoteConnectorId}&" +
                             $"api_key={ADIWF_Config.ApiKey}";

                Log.Info($"Retrieving MetaData from On API using Connector ID: {GraceNoteConnectorId}");

                GraceNoteSeriesSeasonSpecialsData = null;

                var webClient = new WebClientManager();
                GraceNoteSeriesSeasonSpecialsData = webClient.HttpGetRequest(requestUrl);

                if (GraceNoteSeriesSeasonSpecialsData != null && webClient.SuccessfulWebRequest) return true;

                throw new Exception("[GetGraceNoteSeriesSeasonSpecialsData] Error during receive of GN Api data, " +
                                    $"Web request data: {webClient.SuccessfulWebRequest}," +
                                    $"Web request response code: {webClient.RequestStatusCode}");
            }
            catch (Exception ggpdEx)
            {
                Log.Error("[GetGraceNoteSeriesSeasonSpecialsData] Error obtaining " +
                          $"Gracenote Api data: {ggpdEx.Message}");

                if (ggpdEx.InnerException != null)
                    Log.Error("[GetGraceNoteSeriesSeasonSpecialsData] " +
                              $"Inner exception: {ggpdEx.InnerException.Message}");

                return false;
            }
        }

        public void GetDbEnrichedAdi(string adidata)
        {
            SerializeAdiFile(true, adidata);
        }

        public void SetTitlPaidWithGnTitlPaid(string paidValue)
        {
            UpdateAdi.Asset.Metadata.AMS.Asset_ID = paidValue;
        }

        public void SaveAdiFile(string filePath, ADI adiFileContent)
        {
            XmlSerializer = new XmlSerializationManager<ADI>();
            XmlSerializer.Save(filePath, adiFileContent);
        }
    }
}