﻿using log4net;
using SchTech.Api.Manager.GracenoteOnApi.Concrete.EqualityComparers;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Business.Manager.Concrete.Validation;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.Entities.ConcreteTypes;
using SchTech.File.Manager.Concrete.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using SchTech.Business.Manager.Concrete.ImageLogic;

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

        public List<GnApiProgramsSchema.programsProgramSeason> SeasonInfo { get; set; }

        private EnrichmentDataLists EnrichmentDataLists { get; set; }

        private EnhancementDataValidator AdiDataValidator { get; }

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

            if (seasonData == null)
                return;
            //Season Asset List
            EnrichmentDataLists.AddProgramAssetsToList(seasonData.assets, "Season");
            //Season Cast List
            EnrichmentDataLists.AddCastMembersToList(seasonData.cast, "Season");
            //Season Crew List
            EnrichmentDataLists.AddCrewMembersToList(seasonData.crew, "Season");
        }

        public List<GnApiProgramsSchema.assetType> ReturnAssetList()
        {
            return EnrichmentDataLists.ProgramAssets;
        }

        private static bool AddTitleMetadataApp_DataNode(string nodeName, string nodeValue)
        {
            try
            {
                if (string.IsNullOrEmpty(nodeValue))
                    return false;

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

        public static bool AddAssetMetadataApp_DataNode(string assetId, string nodeName, string nodeValue)
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

        public static bool SetAdiAssetContentField(string assetClass, string assetFileName)
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

        public static int GetVersionMajor()
        {
            return EnrichmentWorkflowEntities.AdiFile.Metadata.AMS.Version_Major;
        }

        public static int GetVersionMinor()
        {
            return EnrichmentWorkflowEntities.AdiFile.Metadata.AMS.Version_Minor;
        }

        public static string GetProviderId()
        {
            return EnrichmentWorkflowEntities.AdiFile.Metadata.AMS.Provider_ID;
        }

        public static string GetAssetPaid(string assetType)
        {
            return EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                .Where(asset => asset.Metadata.AMS.Asset_Class.Equals(assetType))
                .Select(asset => asset.Metadata.AMS.Asset_ID.ToString()).FirstOrDefault();
        }

        public static string GetLicenceEndData()
        {
            return EnrichmentWorkflowEntities.AdiFile.Asset.Metadata.App_Data
                .FirstOrDefault(l => l.Name.Equals("Licensing_Window_End"))?.Value;
        }

        //////Additional Safety net to ensure its not included.
        //public static bool RemoveMovieContentFromUpdate()
        //{
        //    try
        //    {
        //        var movieAsset = EnrichmentWorkflowEntities.AdiFile.Asset.Asset
        //            .FirstOrDefault(c => c.Metadata.AMS.Asset_Class == "movie");
        //        var previewAsset = EnrichmentWorkflowEntities.AdiFile.Asset.Asset
        //            .FirstOrDefault(c => c.Metadata.AMS.Asset_Class == "preview");

        //        if (movieAsset != null)
        //        {
        //            var newMovie = new ADIAssetAsset
        //            {
        //                Metadata = new ADIAssetAssetMetadata
        //                {
        //                    AMS = new ADIAssetAssetMetadataAMS(),
        //                    App_Data = new List<ADIAssetAssetMetadataApp_Data>()
        //                }
        //            };

        //            newMovie.Metadata.AMS = movieAsset?.Metadata.AMS;
        //            newMovie.Metadata.App_Data = movieAsset?.Metadata.App_Data;
        //            EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Remove(movieAsset);
        //            EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Add(newMovie);
        //        }

        //        if (previewAsset == null)
        //            return true;
        //        EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Remove(previewAsset);
        //        EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Add(previewAsset);


        //        return true;
        //    }
        //    catch (Exception rmcfuEx)
        //    {
        //        Log.Error($"[RemoveMovieContentFromUpdate] Error during Removal of " +
        //                  $"Movie Content section from Update {rmcfuEx.Message}");

        //        if (rmcfuEx.InnerException != null)
        //            Log.Error(
        //                $"[RemoveMovieContentFromUpdate] Inner Exception: {rmcfuEx.InnerException.Message}");
        //        return false;
        //    }
        //}


        // CopyAssetSectionToAdi


        public static bool CopyPreviouslyEnrichedAssetDataToAdi(bool hasPreviewAsset, bool hasPreviousUpdate)
        {
            try
            {
                var enrichedDataHasImages =
                    EnrichmentWorkflowEntities.EnrichedAdi.Asset.Asset.Any(p => p.Metadata.AMS.Asset_Class == "image");

                //no enriched preview data, no preview asset supplied preview metadata included via incoming adi
                if (!hasPreviewAsset && 
                    EnrichmentWorkflowEntities.PackageHasPreviewMetadata && 
                    EnrichmentWorkflowEntities.EnrichedAdi.Asset.Asset.All(p => p.Metadata.AMS.Asset_Class != "preview"))
                {
                    Log.Info($"Incoming asset has preview metadata, No Preview asset supplied and " +
                             $"Enriched data does not contain preview metadata! removing preview data from adi.xml");

                    var previewData = EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                        .FirstOrDefault(c => c.Metadata.AMS.Asset_Class == "preview");
                    EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Remove(previewData);
                }


                foreach (var assetData in EnrichmentWorkflowEntities.EnrichedAdi.Asset.Asset.ToList())
                {
                    if (assetData.Metadata.AMS.Asset_Class == "movie")
                    {
                        //ensure no existing movie data exists in supplied adi.xml
                        if (EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                            .Any(m => m.Metadata.AMS.Asset_Class == "movie"))
                        {
                            var movieAsset = EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                                .FirstOrDefault(c => c.Metadata.AMS.Asset_Class == "movie");
                            EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Remove(movieAsset);
                        }

                        //add without content node
                        var assetSection = new ADIAssetAsset
                        {
                            Metadata = new ADIAssetAssetMetadata
                            {
                                AMS = assetData.Metadata.AMS,
                                App_Data = assetData.Metadata.App_Data
                            }
                        };

                        EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Add(assetSection);
                    }
                    if (assetData.Metadata.AMS.Asset_Class == "preview")
                    {
                        if (!hasPreviewAsset)
                        {
                            //enriched = preview, adi = no preview asset and no preview metadata
                            if (EnrichmentWorkflowEntities.PackageHasPreviewMetadata)
                            {
                                var previewData = EnrichmentWorkflowEntities.AdiFile.Asset.Asset
                                    .FirstOrDefault(c => c.Metadata.AMS.Asset_Class == "preview");
                                EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Remove(previewData);
                            }
                        }
                        else
                        {
                            if (EnrichmentWorkflowEntities.PackageHasPreviewMetadata)
                            {
                                Log.Info("Using supplied preview metadata as the update contains a physical asset plus preview metadata.");
                                continue;
                            }
                        }
                        var previewAsset = new ADIAssetAsset
                        {
                            Metadata = new ADIAssetAssetMetadata
                            {
                                AMS = assetData.Metadata.AMS,
                                App_Data = assetData.Metadata.App_Data
                            }
                        };

                        EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Add(previewAsset);
                    }
                    if (assetData.Metadata.AMS.Asset_Class == "image")
                    {
                        var imageSection = new ADIAssetAsset
                        {
                            Content = new ADIAssetAssetContent
                            {
                                Value = assetData.Content.Value
                            },
                            Metadata = new ADIAssetAssetMetadata
                            {
                                AMS = assetData.Metadata.AMS,
                                App_Data = assetData.Metadata.App_Data
                            }
                        };

                        EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Add(imageSection);
                    }
                }

                if (!enrichedDataHasImages)
                {
                    if (EnrichmentWorkflowEntities.UpdateAdi.Asset.Asset.All(u => u.Metadata.AMS.Asset_Class != "image"))
                    {
                        Log.Warn("We shouldn't be here! No image data found in the Enriched adi data or the Update Adi data?");
                        return true;
                    }

                    Log.Info("Cloning image data from db UpdateAdi data.");

                    foreach (var imageSection in
                        from assetData in EnrichmentWorkflowEntities.UpdateAdi.Asset.Asset.ToList()
                        where assetData.Metadata.AMS.Asset_Class == "image"
                        select new ADIAssetAsset
                        {
                            Content = new ADIAssetAssetContent
                            {
                                Value = assetData.Content.Value
                            },
                            Metadata = new ADIAssetAssetMetadata
                            {
                                AMS = assetData.Metadata.AMS,
                                App_Data = assetData.Metadata.App_Data
                            }
                        })
                    {
                        EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Add(imageSection);
                    }

                }

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


        public static bool UpdateAllVersionMajorValues(int newVersionMajor)
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

        public bool CheckAndRemovePosterSection()
        {
            var hasPoster =
                EnrichmentWorkflowEntities.AdiFile.Asset.Asset.FirstOrDefault(p =>
                    p.Metadata.AMS.Asset_Class == "poster");
            if (hasPoster == null)
                return false;

            Log.Info("Asset contains a Poster, removing from Package");
            EnrichmentWorkflowEntities.AdiFile.Asset.Asset.Remove(hasPoster);
            return true;

        }

        public static void CheckAndAddBlockPlatformData()
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

        public static bool InsertProgramLayerData(string tmsid, string seriesId)
        {
            try
            {
                return AddTitleMetadataApp_DataNode("GN_Layer1_TMSId", tmsid) &&
                       AddTitleMetadataApp_DataNode("GN_Layer2_RootId", seriesId);
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

        public static bool InsertSeriesLayerData(string seriesTmsId, string seriesId)
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
                var producer = 0;
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
                            case "Producer" when producer < 2:
                            case "Executive Producer" when producer < 2:
                                AddTitleMetadataApp_DataNode("Producer", memberName);
                                crewAdded = true;
                                producer++;
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

        public bool InsertTitleData(bool IsMoviePackage)
        {
            try
            {
                var title = EnrichmentDataLists.ProgramTitles.Where(t => t.type == "full")
                    .Select(t => t.Value)
                    .FirstOrDefault();

                var titleAdded = AddTitleMetadataApp_DataNode("Title", title);

                if (IsMoviePackage)
                    return titleAdded;

                var sortTitle = EnrichmentDataLists.ProgramTitles.FirstOrDefault(t => t.type == "sort")?.Value;

                if (string.IsNullOrEmpty(sortTitle))
                    return titleAdded;

                Log.Info("Title contains sort data, adding Show_Title_Sort_Name to ADI.");
                titleAdded = AddTitleMetadataApp_DataNode("Show_Title_Sort_Name", sortTitle);


                return titleAdded;
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
                var desc = EnhancementDataValidator.CheckAndReturnDescriptionData(descriptions);

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
        public static bool InsertEpisodeData(string tmsid, string episodeOrdinalValue, string episodeTitle)
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
                var seriesIdData = !string.IsNullOrWhiteSpace(ADIWF_Config.Prefix_Series_ID_Value)
                    ? $"{ADIWF_Config.Prefix_Series_ID_Value}{seriesId}"
                    : seriesId;

                AddTitleMetadataApp_DataNode("Series_ID", seriesIdData);
                AddTitleMetadataApp_DataNode("Series_Ordinal", seriesOrdinalValue);

                if (seasonId == 0)
                    return false;


                AddTitleMetadataApp_DataNode("Series_Name",
                    $"Season {episodeSeason}");

                //if (SeasonInfo != null && SeasonInfo.Any())
                if (!SeasonInfo.Any())
                    return true;


                var seasonData = SeasonInfo.FirstOrDefault(i => i.seasonId == seasonId.ToString());

                if (seasonData?.totalSeasonEpisodes != "0")
                    AddTitleMetadataApp_DataNode("Series_NumberOfItems",
                        seasonData?.totalSeasonEpisodes);

                if (seasonData?.descriptions != null)
                {
                    AddTitleMetadataApp_DataNode("Series_Summary_Short",
                        seasonData.descriptions?.desc
                            .FirstOrDefault(d => Convert.ToInt32(d.size) == 250 ||
                                                 Convert.ToInt32(d.size) >= 100)
                            ?.Value);
                }
                else
                {
                    Log.Warn("Season Description data not available?");
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
                           EnhancementDataValidator.CheckAndReturnDescriptionData(
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

        private static bool ValidateYear(string year)
        {
            return year.Length == 4;
        }

        public static bool InsertProductionYears(
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
                if (externalLinks.Count <= 0 && IdmbDataInserted)
                    return true;

                var links = externalLinks;
                Log.Info("Adding IMDb_ID data.");
                AddTitleMetadataApp_DataNode("IMDb_ID", links.FirstOrDefault()?.id);

                if (!hasMovieInfo)
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

        public static bool InsertImageData(
            string titlPaid,
            string imageName,
            string imageMapping,
            string contentCheckSum,
            string contentFileSize,
            string encodingType,
            string imageQualifier,
            string imageLookupName,
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
                            Value = ImageSelectionLogic.GetImageName(imageName, imageMapping)
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
                AddAssetMetadataApp_DataNode(paid, "Image_Qualifier", imageLookupName);
                AddAssetMetadataApp_DataNode(paid, "Image_Aspect_Ratio", imageAspectRatio);

                return true;
            }
            catch (Exception spidEx)
            {
                Log.Error($"Error Encountered Setting Image Data: {spidEx.Message}");
                return false;
            }
        }

        public static bool UpdateImageData(
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

                adiObject.Content.Value = ImageSelectionLogic.GetImageName(imageName, imageMapping);
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

        public void SetQamUpdateContent()
        {
            try
            {
                var movieAsset =
                    EnrichmentWorkflowEntities.AdiFile.Asset.Asset.FirstOrDefault(c =>
                        c.Metadata.AMS.Asset_Class == "movie");
                if (movieAsset == null)
                    throw new Exception("Error retrieving previously Enriched movie section for QAM Update.");
                movieAsset.Content = new ADIAssetAssetContent { Value = MovieContent };

            }
            catch (Exception squcex)
            {
                Log.Error($"Error Encountered Setting QAM Update content field: {squcex.Message}");
            }
        }
    }
}