﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
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
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProdisAdiContentManager));

        private readonly List<string> _adiNodesToRemove = new List<string>
        {
            //look at the set or update method to add if not exists
            //or update if it does, need a bool for crew/cast data.
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
            "Writer"
        };

        public ProdisAdiContentManager()
        {
            AdiDataValidator = new EnhancementDataValidator();
        }

        private EnrichmentDataLists EnrichmentDataLists { get; set; }

        private EnhancementDataValidator AdiDataValidator { get; }

        public List<GnApiProgramsSchema.programsProgramSeason> SeasonInfo { get; set; }

        private bool IdmbDataInserted { get; set; }

        public string MovieContent { get; set; }

        public void InitialiseAndSeedObjectLists(GnApiProgramsSchema.programsProgram episodeMovieData, string seasonId)
        {
            //Instantiate List Entities
            EnrichmentDataLists = new EnrichmentDataLists();

            UpdateListData(episodeMovieData, seasonId);
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
                EnrichmentDataLists.AddProgramAssetsToList(seasonData.assets, "Season");
                //Season Cast List
                EnrichmentDataLists.AddCastMembersToList(seasonData.cast, "Season");
                //Season Crew List
                EnrichmentDataLists.AddCrewMembersToList(seasonData.crew, "Season");
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
            catch (Exception atmadnEx)
            {
                Log.Error("[AddTitleMetadataApp_DataNode] " +
                          $"Error Setting Metadata for Node {nodeName}:" +
                          $" {atmadnEx.Message}");
                if (atmadnEx.InnerException != null)
                    Log.Error("[AddTitleMetadataApp_DataNode] Inner Exception:" +
                              $" {atmadnEx.InnerException.Message}");
                return false;
            }
        }

        public bool AddAssetMetadataApp_DataNode(string assetId, string nodeName, string nodeValue)
        {
            try
            {
                var nodeExists = EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                    .FirstOrDefault(a => a.Metadata.AMS.Asset_ID == assetId)
                    ?.Metadata.App_Data.FirstOrDefault(n => n.Name == nodeName);


                if (nodeExists != null)
                {
                    nodeExists.Value = nodeValue;
                }
                else
                {
                    var newAppData = new ADIAssetAssetMetadataApp_Data
                    {
                        App = "VOD",
                        Name = nodeName,
                        Value = nodeValue
                    };

                    EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                        .FirstOrDefault(a => a.Metadata.AMS.Asset_ID == assetId)
                        ?.Metadata.App_Data.Add(newAppData);
                }

                return true;
            }
            catch (Exception aamdadnEx)
            {
                Log.Error("[AddAssetMetadataApp_DataNode] " +
                          $"Error Setting Metadata for Node {nodeName}:" +
                          $" {aamdadnEx.Message}");
                if (aamdadnEx.InnerException != null)
                    Log.Error("[AddAssetMetadataApp_DataNode] Inner Exception:" +
                              $" {aamdadnEx.InnerException.Message}");
                return false;
            }
        }


        public bool SetAdiAssetContentField(string assetClass, string assetFileName)
        {
            var contentFile = EnrichmentWorkflowEntities.AdiFile
                .Asset.Asset
                .FirstOrDefault(asset => $"{assetClass}".Equals(asset.Metadata.AMS.Asset_Class));

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
                var newAsset = new ADIAssetAsset
                {
                    Content = assetData.Content,
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
            return EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                .Where(asset => asset.Metadata.AMS.Asset_Class.Equals(assetType))
                .Select(asset => asset.Metadata.AMS.Asset_ID.ToString()).FirstOrDefault();
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
                    EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Add(assetData);

                return true;
            }
            catch (Exception cpeadtaEx)
            {
                Log.Error("[CopyPreviouslyEnrichedAssetDataToAdi] Error during Copy of " +
                          $"previously enriched asset data: {cpeadtaEx.Message}");

                if (cpeadtaEx.InnerException != null)
                    Log.Error(
                        $"[CopyPreviouslyEnrichedAssetDataToAdi] Inner Exception: {cpeadtaEx.InnerException.Message}");
                return false;
            }
        }

        public bool UpdateAllVersionMajorValues(int newVersionMajor)
        {
            try
            {
                foreach (var item in EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                    .Where(item => item.Metadata.AMS.Version_Major != newVersionMajor))
                    item.Metadata.AMS.Version_Major = newVersionMajor;

                return true;
            }
            catch (Exception uavmvEx)
            {
                Log.Error("[UpdateAllVersionMajorValues] Error during update of version Major" +
                          $": {uavmvEx.Message}");

                if (uavmvEx.InnerException != null)
                    Log.Error($"[UpdateAllVersionMajorValues] Inner Exception: {uavmvEx.InnerException.Message}");
                return false;
            }
        }

        public void RemoveDefaultAdiNodes()
        {
            foreach (var adiNode in _adiNodesToRemove.SelectMany(item => EnrichmentWorkflowEntities.AdiFile
                .Asset.Metadata.App_Data.Where(attr => attr.Name == item).ToList()))
                EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.App_Data.Remove(adiNode);
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
                        Block_Platform.BlockPlatformValue);
                }
            }
            catch (Exception caabpdEx)
            {
                Log.Error("[CheckAndAddBlockPlatformData] Error Setting Block platform data" +
                          $": {caabpdEx.Message}");

                if (caabpdEx.InnerException != null)
                    Log.Error("[CheckAndAddBlockPlatformData] Inner Exception: " +
                              $"{caabpdEx.InnerException.Message}");
            }
        }

        public bool InsertProgramLayerData(string tmsid, string rootid)
        {
            try
            {
                return AddTitleMetadataApp_DataNode("GN_Layer1_TMSId", tmsid) &&
                       AddTitleMetadataApp_DataNode("GN_Layer2_RootId", rootid);
            }
            catch (Exception ipldEx)
            {
                Log.Error("[InsertProgramLayerData] Error Setting Program Layer data" +
                          $": {ipldEx.Message}");

                if (ipldEx.InnerException != null)
                    Log.Error("[InsertProgramLayerData] Inner Exception: " +
                              $"{ipldEx.InnerException.Message}");
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
            catch (Exception isldEx)
            {
                Log.Error("[InsertSeriesLayerData] Error Setting Program Layer data" +
                          $": {isldEx.Message}");

                if (isldEx.InnerException != null)
                    Log.Error("[InsertSeriesLayerData] Inner Exception: " +
                              $"{isldEx.InnerException.Message}");
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
            catch (Exception iadEx)
            {
                Log.Error("[InsertActorData] Error Setting Actor data" +
                          $": {iadEx.Message}");

                if (iadEx.InnerException != null)
                    Log.Error("[InsertActorData] Inner Exception: " +
                              $"{iadEx.InnerException.Message}");
                return false;
            }
        }

        public bool InsertCrewData()
        {
            try
            {
                var crewAdded = false;
                if (EnrichmentDataLists.CrewMembers != null)
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

                if (crewAdded)
                    Log.Info("Crew data successfully added");
                else
                    Log.Warn("No crew data found?");
                //non mandatory
                return true;
            }
            catch (Exception icdEx)
            {
                Log.Error("[InsertCrewData] Error Setting Crew data" +
                          $": {icdEx.Message}");

                if (icdEx.InnerException != null)
                    Log.Error("[InsertCrewData] Inner Exception: " +
                              $"{icdEx.InnerException.Message}");
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
            catch (Exception itdEx)
            {
                Log.Error("[InsertTitleData] Error Setting Title data" +
                          $": {itdEx.Message}");

                if (itdEx.InnerException != null)
                    Log.Error("[InsertTitleData] Inner Exception: " +
                              $"{itdEx.InnerException.Message}");
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
            catch (Exception iddEx)
            {
                Log.Error("[InsertDescriptionData] Error Setting Description data" +
                          $": {iddEx.Message}");

                if (iddEx.InnerException != null)
                    Log.Error("[InsertDescriptionData] Inner Exception: " +
                              $"{iddEx.InnerException.Message}");
                return false;
            }
        }

        public bool InsertYearData(DateTime airDate, GnApiProgramsSchema.programsProgramMovieInfo movieInfo)
        {
            try
            {
                if (AdiDataValidator.HasYearData(airDate, movieInfo))
                {
                    var nodeToRemove =
                        EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.App_Data.FirstOrDefault(n =>
                            n.Name.ToLower().Equals("year"));
                    EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.App_Data.Remove(nodeToRemove);

                    AddTitleMetadataApp_DataNode("Year", AdiDataValidator.AdiYearValue);
                }
                else
                {
                    Log.Warn("No Year data found in API, Updated Year data will be omitted.");
                }

                //return true as non mandatory
                return true;
            }
            catch (Exception iydEx)
            {
                Log.Error("[InsertYearData] Error Setting Year data" +
                          $": {iydEx.Message}");

                if (iydEx.InnerException != null)
                    Log.Error("[InsertYearData] Inner Exception: " +
                              $"{iydEx.InnerException.Message}");
                return false;
            }
        }

        /// <summary>
        ///     Required if not a movie asset and requires a zero value for specials episode number.
        /// </summary>
        /// <param name="episodeOrdinalValue"></param>
        /// <param name="episodeTitle"></param>
        /// <param name="tmsid"></param>
        /// <returns></returns>
        public bool InsertEpisodeData(string tmsid, string episodeOrdinalValue, string episodeTitle)
        {
            try
            {
                return AddTitleMetadataApp_DataNode("Episode_ID", tmsid) &&
                       AddTitleMetadataApp_DataNode("Episode_Name", episodeTitle) &&
                       AddTitleMetadataApp_DataNode("Episode_Ordinal", episodeOrdinalValue);
            }
            catch (Exception iedEx)
            {
                Log.Error("[InsertEpisodeData] Error Setting Episode data" +
                          $": {iedEx.Message}");

                if (iedEx.InnerException != null)
                    Log.Error("[InsertCrewData] Inner Exception: " +
                              $"{iedEx.InnerException.Message}");
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
            catch (Exception igdEx)
            {
                Log.Error("[InsertGenreData] Error Setting Genre data" +
                          $": {igdEx.Message}");

                if (igdEx.InnerException != null)
                    Log.Error("[InsertGenreData] Inner Exception: " +
                              $"{igdEx.InnerException.Message}");
                return false;
            }
        }

        public bool InsertSeriesData(string seriesId, string seriesOrdinalValue, int seasonId, string episodeSeason)
        {
            try
            {
                AddTitleMetadataApp_DataNode("Series_ID", seriesId);
                AddTitleMetadataApp_DataNode("Series_Ordinal", seriesOrdinalValue);

                if (seasonId == 0)
                    return false;


                AddTitleMetadataApp_DataNode("Series_Name",
                    $"Season {episodeSeason}");


                if (SeasonInfo != null && SeasonInfo.Any())
                {
                    var seasonData = SeasonInfo.FirstOrDefault(i => i.seasonId == seasonId.ToString());

                    if (seasonData != null)
                    {
                        AddTitleMetadataApp_DataNode("Series_Summary_Short",
                            seasonData.descriptions.desc
                                .FirstOrDefault(d => Convert.ToInt32(d.size) == 250 ||
                                                     Convert.ToInt32(d.size) >= 100)
                                ?.Value);

                        if (seasonData.totalSeasonEpisodes != "0")
                            AddTitleMetadataApp_DataNode("Series_NumberOfItems",
                                seasonData.totalSeasonEpisodes);
                    }
                }


                return true;
            }
            catch (Exception isdEx)
            {
                Log.Error("[InsertSeriesdata] Error Setting Series data" +
                          $": {isdEx.Message}");

                if (isdEx.InnerException != null)
                    Log.Error("[InsertSeriesdata] Inner Exception: " +
                              $"{isdEx.InnerException.Message}");
                return false;
            }
        }

        public bool InsertShowData(string showId, string showName, int totalSeasons,
            GnApiProgramsSchema.programsProgramDescriptions descriptions)
        {
            try
            {
                if (SeasonInfo != null) AddTitleMetadataApp_DataNode("Show_NumberOfItems", totalSeasons.ToString());


                return AddTitleMetadataApp_DataNode("Show_ID", showId) &&
                       AddTitleMetadataApp_DataNode("Show_Name", showName) &&
                       AddTitleMetadataApp_DataNode(
                           "Show_Summary_Short",
                           AdiDataValidator.CheckAndReturnDescriptionData(
                               descriptions,
                               true));
            }
            catch (Exception shDEx)
            {
                Log.Error("[InsertShowData] Error Setting Show data" +
                          $": {shDEx.Message}");

                if (shDEx.InnerException != null)
                    Log.Error("[InsertShowData] Inner Exception: " +
                              $"{shDEx.InnerException.Message}");
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
                        AddTitleMetadataApp_DataNode("Show_Genre", genre.Value);
                        AddTitleMetadataApp_DataNode("Show_GenreID", genre.genreId);
                        hasGenres = true;
                    }

                    gId = genre.genreId;
                }

                return hasGenres;
            }
            catch (Exception isgdEx)
            {
                Log.Error("[InsertSeriesGenreData] Error Setting Show Genre data" +
                          $": {isgdEx.Message}");

                if (isgdEx.InnerException != null)
                    Log.Error("[InsertSeriesGenreData] Inner Exception: " +
                              $"{isgdEx.InnerException.Message}");
                return false;
            }
        }

        private bool ValidateYear(string year)
        {
            return year.Length == 4;
        }

        public bool InsertProductionYears(
            DateTime? seriesPremiere,
            DateTime? seasonPremiere,
            DateTime? seriesFinale,
            DateTime? seasonFinale)
        {
            try
            {
                string productionYears;
                var sPremiere = ValidateYear(seriesPremiere?.Year.ToString())
                    ? seriesPremiere?.Year.ToString()
                    : seasonPremiere?.Year.ToString();

                if (sPremiere != null && sPremiere.Length != 4)
                    return false;

                Log.Info($"Premiere year: {sPremiere}");

                var sFinale = ValidateYear(seriesFinale?.Year.ToString())
                    ? seriesFinale?.Year.ToString()
                    : seasonFinale?.Year.ToString();

                if (sFinale != null && sFinale.Length == 4)
                {
                    Log.Info($"Finale year: {sFinale}");
                    productionYears = $"{sPremiere}-{sFinale}";
                }
                else
                {
                    productionYears = sPremiere;
                    Log.Info("No Finale year, using Premiere year only");
                }

                AddTitleMetadataApp_DataNode("Production_Years", productionYears);

                return true;
            }
            catch (Exception ipyEx)
            {
                Log.Error("[InsertProductionYears] Error Setting Production Years Data: " +
                          $": {ipyEx.Message}");

                if (ipyEx.InnerException != null)
                    Log.Error("[InsertProductionYears] Inner Exception: " +
                              $"{ipyEx.InnerException.Message}");
                return false;
            }
        }

        public bool InsertIdmbData(List<GnApiProgramsSchema.externalLinksTypeExternalLink> externalLinks,
            bool hasMovieInfo)
        {
            try
            {
                if (externalLinks != null && IdmbDataInserted == false)
                {
                    var links = externalLinks;
                    Log.Info("Adding IMDb_ID data.");
                    AddTitleMetadataApp_DataNode("IMDb_ID", links.FirstOrDefault()?.id);

                    if (hasMovieInfo)
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
            catch (Exception iidEx)
            {
                Log.Error("[InsertIdmbData] Error Setting IDMB Data: " +
                          $": {iidEx.Message}");

                if (iidEx.InnerException != null)
                    Log.Error("[InsertIdmbData] Inner Exception: " +
                              $"{iidEx.InnerException.Message}");
                return false;
            }
        }

        private string GetImageName(string imageUri, string imageMapping)
        {
            return imageUri.Replace(imageUri, $"{imageMapping}_{imageUri}");
        }

        public bool InsertImageData(
            string titlPaid,
            string imageName,
            string imageMapping,
            string contentCheckSum,
            string contentFileSize,
            string encodingType,
            string imageQualifier,
            string imageAspectRatio
        )
        {
            try
            {
                var paid = $"{imageQualifier}{titlPaid.Replace("TITL", "")}";
                var adiObject = EnrichmentWorkflowEntities.AdiFile.Asset.Asset.FirstOrDefault();

                if (adiObject != null)
                    EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Add(new ADIAssetAsset
                    {
                        Content = new ADIAssetAssetContent
                        {
                            Value = GetImageName(imageName, imageMapping)
                        },
                        Metadata = new ADIAssetAssetMetadata
                        {
                            AMS = new ADIAssetAssetMetadataAMS
                            {
                                Asset_Class = "image",
                                Asset_ID = paid,
                                Asset_Name = adiObject.Metadata.AMS.Asset_Name,
                                Creation_Date = adiObject.Metadata.AMS.Creation_Date,
                                Description = adiObject.Metadata.AMS.Description,
                                Product = "",
                                Provider = adiObject.Metadata.AMS.Provider,
                                Provider_ID = adiObject.Metadata.AMS.Provider_ID,
                                Verb = adiObject.Metadata.AMS.Verb,
                                Version_Major = adiObject.Metadata.AMS.Version_Major,
                                Version_Minor = adiObject.Metadata.AMS.Version_Minor
                            },
                            App_Data = new List<ADIAssetAssetMetadataApp_Data>()
                        }
                    });

                AddAssetMetadataApp_DataNode(paid, "Content_CheckSum", contentCheckSum);
                AddAssetMetadataApp_DataNode(paid, "Content_FileSize", contentFileSize);
                AddAssetMetadataApp_DataNode(paid, "Encoding_Type", encodingType);
                AddAssetMetadataApp_DataNode(paid, "Image_Qualifier", imageQualifier);
                AddAssetMetadataApp_DataNode(paid, "Image_Aspect_Ratio", imageAspectRatio);

                return true;
            }
            catch (Exception spidEx)
            {
                Log.Error($"Error Encountered Setting Image Data: {spidEx.Message}");
                return false;
            }
        }

        public bool UpdateImageData(
            string imageQualifier, 
            string titlPaid, 
            string imageName,
            string imageMapping,
            string imageAspectRatio,
            string checksum,
            string filesize)
        {
            try
            {
                var paid = $"{imageQualifier}{titlPaid.Replace("TITL", "")}";
                var adiObject = EnrichmentWorkflowEntities.AdiFile.Asset.Asset.FirstOrDefault(i => i.Metadata.AMS.Asset_ID == paid);

                if (adiObject == null)
                    throw new Exception($"Error retrieving ADI data for image: {paid}");

                adiObject.Content.Value = GetImageName(imageName, imageMapping);
                var cSum = adiObject.Metadata.App_Data.FirstOrDefault(c => c.Name == "Content_CheckSum");
                var fSize = adiObject.Metadata.App_Data.FirstOrDefault(s => s.Name == "Content_FileSize");
                var aRatio = adiObject.Metadata.App_Data.FirstOrDefault(a => a.Name == "Image_Aspect_Ratio");

                if (cSum != null) cSum.Value = checksum;
                if (fSize != null) fSize.Value = filesize;
                if (aRatio != null) aRatio.Value = imageAspectRatio;

                return true;
            }
            catch (Exception uidex)
            {
                Log.Error($"Error Encountered Updating Image Data: {uidex.Message}");
                return false;
            }
        }

        public bool SetQamUpdateContent()
        {
            try
            {
                var movieAsset =
                    EnrichmentWorkflowEntities.AdiFile.Asset.Asset.FirstOrDefault(c =>
                        c.Metadata.AMS.Asset_Class == "movie");
                if (movieAsset == null)
                    throw new Exception("Error retrieving previously Enriched movie section for QAM Update.");
                movieAsset.Content = new ADIAssetAssetContent {Value = MovieContent};

                return true;
            }
            catch (Exception squcex)
            {
                Log.Error($"Error Encountered Setting QAM Update content field: {squcex.Message}");
                return false;
            }
        }
    }
}