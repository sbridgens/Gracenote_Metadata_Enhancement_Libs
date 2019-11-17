using System;
using System.Collections.Generic;
using System.Linq;
using SchTech.Api.Manager.GracenoteOnApi.Concrete.EqualityComparers;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Business.Manager.Concrete.Validation;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.Entities.ConcreteTypes;
using SchTech.File.Manager.Concrete.Serialization;

namespace SchTech.Business.Manager.Concrete.CustomerBusinessLogic.VirginMedia
{
    public class ProdisAdiContentManager
    {
        /// <summary>
        /// Initialize Log4net
        /// </summary>
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ProdisAdiContentManager));

        private EnrichmentDataLists EnrichmentDataLists { get; set; }

        private EnhancementDataValidator AdiDataValidator { get; set; }

        public GnApiProgramsSchema.programsProgramMovieInfo MovieInfo { get; set; }

        public GnApiProgramsSchema.programsProgramEpisodeInfo EpisodeInfo { get; set; }

        public List<GnApiProgramsSchema.programsProgramSeason> SeasonInfo { get; set; }

        private bool IdmbDataInserted { get; set; }

        private List<string> AdiNodesToRemove = new List<string>()
        {
            ///look at the set or update method to add if not exists
            ///or update if it does, need a bool for crew/cast data.
            "Actors",
            "Actors_Display",
            "Block_Platform",
            "Director",
            "Episode_ID",
            "Episode_Name",
            "Episode_Ordinal",
            "Executive Producer",
            "ExtraData_1",
            "ExtraData_3",
            "GN_Layer1_TMSId",
            "GN_Layer2_RootId",
            "GN_Layer2_SeriesId",
            "GN_Layer2_TMSId",
            "Genre",
            "GenreID",
            "Producer",
            "Screenwriter",
            "Series_ID",
            "Series_Name",
            "Series_NumberOfItems",
            "Series_Ordinal",
            "Show_ID",
            "Show_Name",
            "Show_NumberOfItems",
            "Show_Summary_Short",
            "Summary_Short",
            "Title",
            "Writer",
            "Year"
        };

        public ProdisAdiContentManager()
        {
            AdiDataValidator = new EnhancementDataValidator();
        }

        public void InitialiseAndSeedObjectLists(GnApiProgramsSchema.programsProgram episodeMovieData,string seasonId)
        {
            //Instantiate List Entities
            EnrichmentDataLists = new EnrichmentDataLists();

            UpdateListData(episodeMovieData,seasonId);
        }


        public void UpdateListData(GnApiProgramsSchema.programsProgram apiData, string seasonId)
        {
            //Build Data Lists
            //Asset List
            
            EnrichmentDataLists.AddProgramAssetsToList(apiData?.assets, "program(series/movie)");
            //Cast List
            EnrichmentDataLists.AddCastMembersToList(apiData?.cast, "program(series/movie)");
            //Crew List
            EnrichmentDataLists.AddCrewMembersToList(apiData?.crew, "program(series/movie)");
            //titles
            EnrichmentDataLists.AddProgramTitlesToList(apiData?.titles, "program(series/movie)");
            //genres
            EnrichmentDataLists.AddGenresToList(apiData?.genres, "program(series/movie)");
            //external Links
            EnrichmentDataLists.AddExternalLinksToList(apiData?.externalLinks);
            //
            var seasonData = apiData?.seasons?.FirstOrDefault(s => s.seasonId == seasonId);

            if (seasonData != null)
            {
                //Season Asset List
                EnrichmentDataLists.AddProgramAssetsToList(seasonData?.assets, "Season");
                //Season Cast List
                EnrichmentDataLists.AddCastMembersToList(seasonData?.cast, "Season");
                //Season Crew List
                EnrichmentDataLists.AddCrewMembersToList(seasonData?.crew, "Season");
            }
        }

        public List<GnApiProgramsSchema.assetType> ReturnAssetList()
        {
            return EnrichmentDataLists.ProgramAssets;
        }

        public bool AddTitleMetadataApp_DataNode(string nodeName, string nodeValue)
        {
            try
            {
                var newAppData = new ADIAssetMetadataApp_Data
                {
                    App = "VOD",
                    Name = nodeName,
                    Value = nodeValue
                };

                EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.App_Data.Add(newAppData);

                return true;
            }
            catch (Exception ATMADN_EX)
            {
                Log.Error("[AddTitleMetadataApp_DataNode] " +
                          $"Error Setting Metadata for Node {nodeName}:" +
                          $" {ATMADN_EX.Message}");
                if (ATMADN_EX.InnerException != null)
                {
                    Log.Error("[AddTitleMetadataApp_DataNode] Inner Exception:" +
                              $" {ATMADN_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool AddAssetMetadataApp_DataNode(string assetId, string nodeName, string nodeValue)
        {
            try
            {
                var newAppData = new ADIAssetAssetMetadataApp_Data()
                {
                    App = "VOD",
                    Name = nodeName,
                    Value = nodeValue
                };

                EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                    .FirstOrDefault(a => a.Metadata.AMS.Asset_ID == assetId)
                    ?.Metadata.App_Data.Add(newAppData);

                return true;
            }
            catch (Exception AAMDADN_EX)
            {
                Log.Error("[AddAssetMetadataApp_DataNode] " +
                          $"Error Setting Metadata for Node {nodeName}:" +
                          $" {AAMDADN_EX.Message}");
                if (AAMDADN_EX.InnerException != null)
                {
                    Log.Error("[AddAssetMetadataApp_DataNode] Inner Exception:" +
                              $" {AAMDADN_EX.InnerException.Message}");
                }
                return false;
            }
        }

       
        public bool SetAdiAssetContentField(string assetClass, string assetFileName)
        {
            var contentFile = EnrichmentWorkflowEntities.AdiFile
                .Asset.Asset
                .FirstOrDefault(asset => $"{assetClass}".Equals(value: asset.Metadata.AMS.Asset_Class));

            if (contentFile == null)
                return false;

            contentFile.Content.Value = assetFileName;
            Log.Info($"Successfully Set Content Value for Asset Type: {assetClass} to {assetFileName}");
            return true;

        }

        public void CloneEnrichedAssetDataToAdi(IEnumerable<ADIAssetAsset> clonedData)
        {
            foreach (var assetData in clonedData)
            {
                var newAsset = new ADIAssetAsset()
                {
                   Content =  assetData.Content,
                   Metadata = assetData.Metadata
                };

                EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Add(newAsset);
            }
            Log.Info("Successfully cloned Enriched asset data to ADI.");
        }

        public int GetVersionMajor()
        {
            return EnrichmentWorkflowEntities.AdiFile.Metadata.AMS.Version_Major;
        }

        public int GetVersionMinor()
        {
            return EnrichmentWorkflowEntities.AdiFile.Metadata.AMS.Version_Minor;
        }

        public string GetProviderId()
        {
            return EnrichmentWorkflowEntities.AdiFile.Metadata.AMS.Provider_ID;
        }
        public string GetAssetPaid(string assetType)
        {
            return (EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                .Where(asset => asset.Metadata.AMS.Asset_Class.Equals(assetType))
                .Select(asset => asset.Metadata.AMS.Asset_ID.ToString())).FirstOrDefault();
        }

        public string GetLicenceEndData()
        {
            return EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.App_Data
                .FirstOrDefault(l => l.Name.Equals("Licensing_Window_End"))?.Value;
        }

        public bool CopyPreviouslyEnrichedAssetDataToAdi()
        {
            try
            {
                foreach (var assetData in EnrichmentWorkflowEntities
                    .EnrichedAdi.Asset.Asset
                    .Select(assetSection => new ADIAssetAsset
                {
                    Content = new ADIAssetAssetContent
                    {
                        Value = assetSection.Content.Value
                    },
                    Metadata = new ADIAssetAssetMetadata
                    {
                        AMS = assetSection.Metadata.AMS,
                        App_Data = assetSection.Metadata.App_Data
                    }
                }))
                {
                    EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Add(assetData);
                }

                return true;
            }
            catch (Exception CPEADTA_EX)
            {
                Log.Error("[CopyPreviouslyEnrichedAssetDataToAdi] Error during Copy of " +
                          $"previously enriched asset data: {CPEADTA_EX.Message}");

                if (CPEADTA_EX.InnerException != null)
                {
                    Log.Error($"[CopyPreviouslyEnrichedAssetDataToAdi] Inner Exception: {CPEADTA_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool UpdateAllVersionMajorValues(int newVersionMajor)
        {
            try
            {
                foreach (var item in EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                    .Where(item => item.Metadata.AMS.Version_Major != newVersionMajor))
                {
                    item.Metadata.AMS.Version_Major = newVersionMajor;
                }

                return true;
            }
            catch (Exception UAVMV_EX)
            {
                Log.Error("[UpdateAllVersionMajorValues] Error during update of version Major" +
                          $": {UAVMV_EX.Message}");

                if (UAVMV_EX.InnerException != null)
                {
                    Log.Error($"[UpdateAllVersionMajorValues] Inner Exception: {UAVMV_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public void RemoveDefaultAdiNodes()
        {
            foreach (var adiNode in AdiNodesToRemove.SelectMany(item => EnrichmentWorkflowEntities.AdiFile
                .Asset.Metadata.App_Data.Where(attr => attr.Name == item).ToList()))
            {
                EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.App_Data.Remove(adiNode);
            }
        }

        public void CheckAndAddBlockPlatformData()
        {
            try
            {
                var providerName = EnrichmentWorkflowEntities.AdiFile.Metadata.AMS.Provider;
                var assetId =
                    EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                        .FirstOrDefault(a =>
                        a.Metadata.AMS.Asset_Class.Equals("movie"))
                        ?.Metadata.AMS.Asset_ID;

                var providerList = Block_Platform.Providers
                    .Split(',')
                    .ToList();

                foreach (var provider in providerList
                    .Where(provider => !string.IsNullOrEmpty(provider) && 
                           provider.ToLower().Trim() == providerName.ToLower().Trim()))
                {
                    Log.Info($"Provider: {provider} matches the Block list, " +
                             "adding the Block_Platform entry with a value of: " +
                             $"{Block_Platform.BlockPlatformValue}" +
                             " for this provider.");

                    AddAssetMetadataApp_DataNode(assetId,
                        "Block_Platform",
                        nodeValue: Block_Platform.BlockPlatformValue);
                }
            }
            catch (Exception CAABPD_EX)
            {
                Log.Error("[CheckAndAddBlockPlatformData] Error Setting Block platform data" +
                          $": {CAABPD_EX.Message}");

                if (CAABPD_EX.InnerException != null)
                {
                    Log.Error($"[CheckAndAddBlockPlatformData] Inner Exception: " +
                              $"{CAABPD_EX.InnerException.Message}");
                }
            }
        }

        public bool InsertProgramLayerData(string tmsid, string rootid)
        {
            try
            {
                return AddTitleMetadataApp_DataNode("GN_Layer1_TMSId", tmsid) &&
                       AddTitleMetadataApp_DataNode("GN_Layer2_RootId", rootid);
            }
            catch (Exception IPLD_EX)
            {
                Log.Error("[InsertProgramLayerData] Error Setting Program Layer data" +
                          $": {IPLD_EX.Message}");

                if (IPLD_EX.InnerException != null)
                {
                    Log.Error($"[InsertProgramLayerData] Inner Exception: " +
                              $"{IPLD_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool InsertSeriesLayerData(string seriesTmsId, string seriesId)
        {
            try
            {
                return AddTitleMetadataApp_DataNode("GN_Layer2_TMSId", seriesTmsId) &&
                       AddTitleMetadataApp_DataNode("GN_Layer2_SeriesId", seriesId);
            }
            catch (Exception ISLD_EX)
            {
                Log.Error("[InsertSeriesLayerData] Error Setting Program Layer data" +
                          $": {ISLD_EX.Message}");

                if (ISLD_EX.InnerException != null)
                {
                    Log.Error($"[InsertSeriesLayerData] Inner Exception: " +
                              $"{ISLD_EX.InnerException.Message}");
                }
                return false;
            }
        }


        public bool InsertActorData()
        {
            try
            {
                if (EnrichmentDataLists.CastMembers != null)
                {
                    var counter = 0;
                    var actorsDisplay = "";

                    foreach (var member in EnrichmentDataLists.CastMembers.Distinct(new CastComparer())
                        .ToList()
                        .Where(member => member.role.Equals("Actor") ||
                                         member.role.Equals("Voice")))
                    {
                        if (counter == 5)
                        {
                            AddTitleMetadataApp_DataNode("Actors_Display",
                                actorsDisplay.TrimEnd(','));
                            break;
                        }
                        var actorName = $"{member.name.first} {member.name.last}";
                        AddTitleMetadataApp_DataNode("Actors", actorName);
                        actorsDisplay += $"{actorName},";
                        counter++;
                    }

                    Log.Info("Actors data successfully added.");
                    return true;
                }

                Log.Warn("No Actors data available.");
                return true;
            }
            catch (Exception IAD_EX)
            {
                Log.Error("[InsertActorData] Error Setting Actor data" +
                          $": {IAD_EX.Message}");

                if (IAD_EX.InnerException != null)
                {
                    Log.Error($"[InsertActorData] Inner Exception: " +
                              $"{IAD_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool InsertCrewData()
        {
            try
            {
                var crewAdded = false;
                if (EnrichmentDataLists.CrewMembers != null)
                {
                    foreach (var member in EnrichmentDataLists.CrewMembers.Distinct(new CrewComparer()).ToList())
                    {
                        var memberName = $"{member.name.first} {member.name.last}";
                        switch (member.role)
                        {
                            case "Director":
                                AddTitleMetadataApp_DataNode("Director", memberName);
                                crewAdded = true;
                                break;
                            case "Producer":
                            case "Executive Producer":
                                AddTitleMetadataApp_DataNode("Producer", memberName);
                                crewAdded = true;
                                break;
                            case "Writer":
                            case "Screenwriter":
                                AddTitleMetadataApp_DataNode("Writer", memberName);
                                crewAdded = true;
                                break;
                        }
                    }
                }
                if (crewAdded)
                {
                    Log.Info("Crew data successfully added");
                }
                else
                {
                    Log.Warn("No crew data found?");
                }
                //non mandatory
                return true;
            }
            catch (Exception ICD_EX)
            {
                Log.Error("[InsertCrewData] Error Setting Crew data" +
                          $": {ICD_EX.Message}");

                if (ICD_EX.InnerException != null)
                {
                    Log.Error("[InsertCrewData] Inner Exception: " +
                              $"{ICD_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool InsertTitleData()
        {
            try
            {
                var title = EnrichmentDataLists.ProgramTitles.Where(t => t.type == "full")
                    .Select(t => t.Value)
                    .FirstOrDefault();

                return AddTitleMetadataApp_DataNode("Title", title);
            }
            catch (Exception ITD_EX)
            {
                Log.Error("[InsertTitleData] Error Setting Title data" +
                          $": {ITD_EX.Message}");

                if (ITD_EX.InnerException != null)
                {
                    Log.Error("[InsertTitleData] Inner Exception: " +
                              $"{ITD_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool InsertDescriptionData(GnApiProgramsSchema.programsProgramDescriptions descriptions)
        {
            try
            {
                var desc = AdiDataValidator.CheckAndReturnDescriptionData(descriptions);

                if (!string.IsNullOrEmpty(desc))
                {
                    AddTitleMetadataApp_DataNode("Summary_Short", desc);
                    Log.Info("Description Data successfully added");
                }
                else
                {
                    Log.Warn("No description Data added");
                }
                //return true as not mandatory
                return true;
            }
            catch (Exception IDD_EX)
            {
                Log.Error("[InsertDescriptionData] Error Setting Description data" +
                          $": {IDD_EX.Message}");

                if (IDD_EX.InnerException != null)
                {
                    Log.Error("[InsertDescriptionData] Inner Exception: " +
                              $"{IDD_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool InsertYearData(DateTime airDate, GnApiProgramsSchema.programsProgramMovieInfo movieInfo)
        {
            try
            {
                if (AdiDataValidator.HasYearData(airDate,movieInfo))
                {
                    AddTitleMetadataApp_DataNode("Year", AdiDataValidator.AdiYearValue);
                }
                else
                {
                    Log.Warn("No Year data found, Year data will be omitted.");
                }
                //return true as non mandatory
                return true;
            }
            catch (Exception IYD_EX)
            {
                Log.Error("[InsertYearData] Error Setting Year data" +
                          $": {IYD_EX.Message}");

                if (IYD_EX.InnerException != null)
                {
                    Log.Error("[InsertYearData] Inner Exception: " +
                              $"{IYD_EX.InnerException.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Required if not a movie asset and requires a zero value for specials episode number.
        /// </summary>
        /// <param name="episodeOrdinalValue"></param>
        /// <param name="episodeTitle"></param>
        /// <param name="tmsid"></param>
        /// <returns></returns>
        public bool InsertEpisodeData(string tmsid, string episodeOrdinalValue, string episodeTitle)
        {
            try
            {
                return AddTitleMetadataApp_DataNode("Episode_ID", tmsid)&&
                       AddTitleMetadataApp_DataNode("Episode_Name", episodeTitle)&&
                       AddTitleMetadataApp_DataNode("Episode_Ordinal", nodeValue: episodeOrdinalValue);

            }
            catch (Exception IED_EX)
            {
                Log.Error("[InsertEpisodeData] Error Setting Episode data" +
                          $": {IED_EX.Message}");

                if (IED_EX.InnerException != null)
                {
                    Log.Error("[InsertCrewData] Inner Exception: " +
                              $"{IED_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool InsertGenreData()
        {
            try
            {
                var currentId = "-9";

                foreach (var genre in EnrichmentDataLists.GenresList.Distinct(new GenreComparer()).ToList())
                {
                    if (currentId != genre.genreId)
                    {
                        AddTitleMetadataApp_DataNode("Genre", genre.Value);
                        AddTitleMetadataApp_DataNode("GenreID", genre.genreId);
                    }

                    currentId = genre.genreId;
                }

                return true;
            }
            catch (Exception IGD_EX)
            {
                Log.Error("[InsertGenreData] Error Setting Genre data" +
                          $": {IGD_EX.Message}");

                if (IGD_EX.InnerException != null)
                {
                    Log.Error("[InsertGenreData] Inner Exception: " +
                              $"{IGD_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool InsertSeriesData(string seriesId, string seriesOrdinalValue, int seasonId)
        {
            try
            {
                AddTitleMetadataApp_DataNode("Series_ID", seriesId);
                AddTitleMetadataApp_DataNode("Series_Ordinal", seriesOrdinalValue);

                if (seasonId == 0)
                    return false;

                
                AddTitleMetadataApp_DataNode("Series_Name", 
                    $"Season {EpisodeInfo?.season}");



                if(SeasonInfo != null && SeasonInfo.Any())
                {
                    var seasonData = SeasonInfo.FirstOrDefault(i => i.seasonId == seasonId.ToString());

                    AddTitleMetadataApp_DataNode("Series_Summary_Short",
                        seasonData.descriptions.desc
                            .FirstOrDefault(d => Convert.ToInt32(d.size) == 250 || 
                                                 Convert.ToInt32(d.size) >= 100)
                            ?.Value);

                    if (seasonData.totalSeasonEpisodes != "0")
                    {
                        AddTitleMetadataApp_DataNode("Series_NumberOfItems",
                            seasonData.totalSeasonEpisodes);
                    }
                }

                

                return true;

            }
            catch (Exception ISD_EX)
            {
                Log.Error("[InsertSeriesdata] Error Setting Series data" +
                          $": {ISD_EX.Message}");

                if (ISD_EX.InnerException != null)
                {
                    Log.Error("[InsertSeriesdata] Inner Exception: " +
                              $"{ISD_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool InsertShowData(string showId, string showName, int totalSeasons, GnApiProgramsSchema.programsProgramDescriptions descriptions)
        {
            try
            {
                if (SeasonInfo != null)
                {
                    AddTitleMetadataApp_DataNode("Show_NumberOfItems",totalSeasons.ToString());
                }



                return AddTitleMetadataApp_DataNode("Show_ID", showId) &&
                       AddTitleMetadataApp_DataNode("Show_Name", showName) &&
                       AddTitleMetadataApp_DataNode(
                            "Show_Summary_Short",
                            AdiDataValidator.CheckAndReturnDescriptionData(
                                programDescriptions: descriptions,
                                true));
            }
            catch (Exception IShD_EX)
            {
                Log.Error("[InsertShowData] Error Setting Show data" +
                          $": {IShD_EX.Message}");

                if (IShD_EX.InnerException != null)
                {
                    Log.Error("[InsertShowData] Inner Exception: " +
                              $"{IShD_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool InsertSeriesGenreData()
        {
            try
            {
                var gId = "";
                var hasGenres = false;
                var genres = EnrichmentDataLists.GenresList.Distinct(new GenreComparer()).ToList();

                foreach (var genre in genres)
                {
                    if (gId != genre.genreId)
                    {
                        AddTitleMetadataApp_DataNode("Show_Genre", nodeValue: genre.Value);
                        AddTitleMetadataApp_DataNode("Show_GenreID", genre.genreId);
                        hasGenres = true;
                    }

                    gId = genre.genreId;
                }

                return hasGenres;
            }
            catch (Exception ISGD_EX)
            {
                Log.Error("[InsertSeriesGenreData] Error Setting Show Genre data" +
                          $": {ISGD_EX.Message}");

                if (ISGD_EX.InnerException != null)
                {
                    Log.Error("[InsertSeriesGenreData] Inner Exception: " +
                              $"{ISGD_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool InsertProductionYears(string firstSeasonId, string lastSeasonId, int seasonId)
        {
            try
            {
                
                if (SeasonInfo != null && SeasonInfo.Any())
                {
                    Log.Info("Setting Production year data");
                    Log.Info($"First available season: {firstSeasonId}, " +
                             $"Last available season: {lastSeasonId}");

                    var premData = Convert.ToDateTime(SeasonInfo.FirstOrDefault(s => s.seasonId == seasonId.ToString()).seasonPremiere);

                    var finaleData =
                        Convert.ToDateTime(SeasonInfo.FirstOrDefault(s => s.seasonId == seasonId.ToString())
                            .seasonFinale);

                    if (AdiDataValidator.HasPremiereData(
                        firstSeasonId,
                        lastSeasonId,
                        premData,
                        finaleData,
                        SeasonInfo.FirstOrDefault(s => s.seasonId == seasonId.ToString())))
                    {
                        AddTitleMetadataApp_DataNode("Production_Years", AdiDataValidator.ProductionYears);

                        return true;
                    }
                }
                

                Log.Warn("No Production years data found!");
                return false;

            }
            catch (Exception IPY_EX)
            {
                Log.Error("[InsertProductionYears] Error Setting Production Years Data: " +
                          $": {IPY_EX.Message}");

                if (IPY_EX.InnerException != null)
                {
                    Log.Error("[InsertProductionYears] Inner Exception: " +
                              $"{IPY_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public  bool InsertIdmbData(List<GnApiProgramsSchema.externalLinksTypeExternalLink> externalLinks)
        {
            try
            {
                if (externalLinks != null && IdmbDataInserted == false)
                {

                    var links = externalLinks;
                    Log.Info("Adding IMDb_ID data.");
                    AddTitleMetadataApp_DataNode("IMDb_ID", links.FirstOrDefault()?.id);

                    if (MovieInfo == null)
                    {
                        Log.Info("Adding Show_IMDb_ID data.");
                        AddTitleMetadataApp_DataNode("Show_IMDb_ID", 
                            links.Any() 
                                ? links.Last().id 
                                : links.FirstOrDefault()?.id);
                    }

                    IdmbDataInserted = true;
                    return true;
                }

                return true;
            }
            catch (Exception IID_EX)
            {
                Log.Error("[InsertIdmbData] Error Setting IDMB Data: " +
                          $": {IID_EX.Message}");

                if (IID_EX.InnerException != null)
                {
                    Log.Error($"[InsertIdmbData] Inner Exception: " +
                              $"{IID_EX.InnerException.Message}");
                }
                return false;
            }
        }

        public bool InsertImageData(
            string titlPaid,
            string imageName,
            string contentCheckSum,
            string contentFileSize,
            string encodingType,
            string imageQualifier,
            string imageAspectRatio
        )
        {
            try
            {
                var _paid = $"{imageQualifier}{titlPaid.Replace("TITL", "ASST")}";
                var adiObject = EnrichmentWorkflowEntities.AdiFile.Asset.Asset.FirstOrDefault();


                if (adiObject != null)
                    EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Add(new ADIAssetAsset
                    {
                        Content = new ADIAssetAssetContent
                        {
                            Value = imageName
                        },
                        Metadata = new ADIAssetAssetMetadata
                        {
                            AMS = new ADIAssetAssetMetadataAMS
                            {
                                Asset_Class = "image",
                                Asset_ID = _paid,
                                Asset_Name = adiObject?.Metadata.AMS.Asset_Name,
                                Creation_Date = adiObject.Metadata.AMS.Creation_Date,
                                Description = adiObject.Metadata.AMS.Description,
                                Product = adiObject.Metadata.AMS.Product,
                                Provider = adiObject.Metadata.AMS.Provider,
                                Provider_ID = adiObject.Metadata.AMS.Provider_ID,
                                Verb = adiObject.Metadata.AMS.Verb,
                                Version_Major = adiObject.Metadata.AMS.Version_Major,
                                Version_Minor = adiObject.Metadata.AMS.Version_Minor
                            },
                            App_Data = new List<ADIAssetAssetMetadataApp_Data>()
                        }
                    });

                AddAssetMetadataApp_DataNode(_paid, "Content_CheckSum", contentCheckSum);
                AddAssetMetadataApp_DataNode(_paid, "Content_FileSize", contentFileSize);
                AddAssetMetadataApp_DataNode(_paid, "Encoding_Type", encodingType);
                AddAssetMetadataApp_DataNode(_paid, "Image_Qualifier", imageQualifier);
                AddAssetMetadataApp_DataNode(_paid, "Image_Aspect_Ratio", imageAspectRatio);

                return true;
            }
            catch (Exception SPID_EX)
            {
                Log.Error($"Error Encountered Setting Image Data: {SPID_EX.Message}");
                return false;
            }
        }
    }
}
