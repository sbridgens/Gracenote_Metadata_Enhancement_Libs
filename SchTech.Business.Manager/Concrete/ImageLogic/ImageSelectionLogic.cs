using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.DataAccess.Concrete;
using SchTech.Entities.ConcreteTypes;
using SchTech.Web.Manager.Concrete;

namespace SchTech.Business.Manager.Concrete.ImageLogic
{
    public class ImageSelectionLogic
    {
        /// <summary>
        ///     Initialize log4net
        /// </summary>
        private readonly ILog Log = LogManager.GetLogger(typeof(ImageSelectionLogic));

        private List<GnApiProgramsSchema.assetType> ApiAssetSortedList { get; set; }
        private string IdentifierType { get; set; }
        private List<string> AssetTier { get; set; }
        private string IdentifierId { get; set; }
        private string AspectRatio { get; set; }
        private bool IsSquare { get; set; }
        public List<GnApiProgramsSchema.assetType> ApiAssetList { get; set; }
        public List<Image_Category> ConfigImageCategories { get; set; }
        public Dictionary<string, string> DbImagesForAsset { get; set; }
        public GN_Mapping_Data CurrentMappingData { get; set; }
        public bool DownloadImageRequired { get; set; }
        public ImageMapping ImageMapping { get; set; }
        public string ConfigProgramType { get; set; }
        public string ImageQualifier { get; set; }
        public bool IsLandscape { get; set; }
        public string DbImages { get; set; }
        public bool IsUpdate { get; set; }
        public int SeasonId { get; set; }

        /// <summary>
        ///     Gets the image encoding type
        /// </summary>
        /// <param name="assetType"></param>
        /// <returns></returns>
        private string GetEncodingType(string assetType)
        {
            string encodingType = null;

            switch (assetType)
            {
                case "image/gif":
                    encodingType = "GIF";
                    break;
                case "image/jpeg":
                    encodingType = "JPG";
                    break;
                case "image/png":
                    encodingType = "PNG";
                    break;
                case "image/svg+xml":
                    encodingType = "SVG";
                    break;
            }

            return encodingType;
        }

        /// <summary>
        ///     Downloads the image from the configured image url and the image path found in the api
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="destinationImage"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public bool DownloadImage(string sourceImage, string destinationImage, int height = 0)
        {
            try
            {
                var downloadUrl = height != 0
                    ? $"{ADIWF_Config.MediaCloud}/{sourceImage}?h={height}"
                    : $"{ADIWF_Config.MediaCloud}/{sourceImage}";

                var webClient = new WebClientManager();
                webClient.DownloadWebBasedFile(downloadUrl, false, destinationImage);

                Log.Info($"Successfully Downloaded Image: {sourceImage} as {destinationImage}");

                return true;
            }
            catch (Exception diEx)
            {
                Log.Error($"[DownloadImage] Error Downloading Image: {diEx.Message}");
                if (diEx.InnerException != null)
                    Log.Error($"[DownloadImage] Inner Exception: {diEx.InnerException.Message}");

                return false;
            }
        }

        /// <summary>
        ///     Used during update packages to detect if an Image is required for download
        ///     As updates may need the image adi section but not the image if no changes are required
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <param name="imageTypeRequired"></param>
        /// <returns></returns>
        private bool HasAsset(Dictionary<string, string> keyValuePairs, string imageTypeRequired)
        {
            return (
                from item in keyValuePairs
                from image in ApiAssetList
                where imageTypeRequired == item.Key.Trim() &&
                      image.URI == item.Value.Trim()
                select item).Any();
        }

        /// <summary>
        ///     Sorts the Image list in order of Landscape or Portrait dependant on the flag taken from the
        ///     flags islandscape in the db
        /// </summary>
        private void SortAssets()
        {
            ApiAssetSortedList = new List<GnApiProgramsSchema.assetType>();

            ApiAssetSortedList = IsLandscape
                ? ApiAssetList.OrderByDescending(w => w.width).ThenBy(h => h.height).ThenBy(i => i.identifiers.Any())
                    .ToList()
                : ApiAssetList.OrderByDescending(h => h.height).ThenBy(w => w.width).ThenBy(i => i.identifiers.Any())
                    .ToList();
        }

        /// <summary>
        ///     Populate the asset list based on tier used later for sorting and returning
        ///     configs based on program type and tier
        /// </summary>
        private void UpdateAssetList(string configTier)
        {
            if (AssetTier == null)
                AssetTier = ApiAssetSortedList.Where(t => t.tier == configTier)
                    .Select(t => t.tier)
                    .Distinct()
                    .ToList();

            //for movies ensure value is "" as the api has no tier but a empty string
            if (!AssetTier.Any())
                AssetTier.Add("");
        }

        /// <summary>
        ///     Future method to detect if the file is portait or landscape
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private void ImageAspect(string width, string height)
        {
            var w = Convert.ToInt32(width);
            var h = Convert.ToInt32(height);

            IsLandscape = w > h;
            IsSquare = w == h;
        }

        /// <summary>
        ///     Returns the WxH aspect ratio taken from the physical image file
        /// </summary>
        /// <param name="fullFileName"></param>
        /// <returns></returns>
        public string GetFileAspectRatio(string fullFileName)
        {
            try
            {
                int height;
                int width;
                using (var img = Image.FromFile(fullFileName))
                {
                    height = img.Height;
                    width = img.Width;
                }

                return $"{width}x{height}";
            }
            catch (Exception ex)
            {
                Log.Error($"[GetFileAspectRatio] Error Getting Image File Aspect Ratio: {ex.Message}");
                if (ex.InnerException != null)
                    Log.Error($"[GetFileAspectRatio] Inner Exception: {ex.InnerException.Message}");

                return null;
            }
        }


        /// <summary>
        ///     Returns the formatted aspect ratio
        /// </summary>
        /// <param name="resizeHeight"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void SetAspect(string width, string height, int resizeHeight = 0)
        {
            AspectRatio = resizeHeight != 0
                ? AspectRatio = $"{width}x{resizeHeight}"
                : AspectRatio = $"{width}x{height}";
        }

        private void UpdateCategoryList(List<Image_Category> imageCategories)
        {
            ConfigImageCategories = imageCategories.Where(t => AssetTier.Contains(t.ImageTier.ToString()))
                .OrderBy(p => p.PriorityOrder)
                .ToList();
        }


        private bool MatchIdentifier(
            IEnumerable<GnApiProgramsSchema.identifierType> identifiers
        )
        {
            if (!ImageMapping.ImageIdentifier.Any())
                return false;

            foreach (var idArr in identifiers)
            foreach (var confIdentifier in ImageMapping.ImageIdentifier
                .Where(confIdentifier => idArr.type == confIdentifier.Type))
            {
                if (idArr.id.FirstOrDefault() == confIdentifier.Id)
                {
                    IdentifierId = idArr.id.FirstOrDefault();
                    return true;
                }

                IdentifierType = idArr.type;
            }

            return true;
        }

        /// <summary>
        ///     Image logic rules, this is processed in order and can be updated/detracted from dependent on
        ///     image requirement changes
        /// </summary>
        /// <param name="imageCategory"></param>
        /// <param name="image"></param>
        /// <param name="configTier"></param>
        /// <param name="imageTier"></param>
        /// <param name="_IsLandscape"></param>
        /// <returns></returns>
        private bool PassesImageLogic(Image_Category imageCategory, GnApiProgramsSchema.assetType image,
            string configTier, string imageTier, bool _IsLandscape)
        {
            //no image tier for movies so ensure the value matches config tier string.empty
            if (imageTier == null)
                imageTier = "";

            return imageCategory.AllowedAspects.Aspect
                       .Any(a =>
                           a.AspectHeight == image.height &&
                           a.AspectWidth == image.width
                       ) &&
                   configTier == imageTier &&
                   IsLandscape == _IsLandscape &&
                   !IsSquare &&
                   !image.identifiers.Any();
        }

        private void SetDbImages(string imageTypeRequired, string URI)
        {
            var gnimages = CurrentMappingData.GN_Images;

            DbImages = string.IsNullOrEmpty(gnimages)
                ? $"{imageTypeRequired}: {URI}"
                : $"{gnimages}, {imageTypeRequired}: {URI}";
        }

        private void LogIdentifierLogic(int identifiersCount, string imageName)
        {
            if (!string.IsNullOrEmpty(IdentifierType))
                Log.Info($"Image: {imageName} - Image Identifier TYPE Match: {IdentifierType} matches Config value");
            if (!string.IsNullOrEmpty(IdentifierId))
                Log.Info($"Image: {imageName} - Image Identifier ID: {IdentifierId} matches Config value");
            else if (identifiersCount == 0) Log.Info("No Identifier config found for current image Type.");
        }


        public string GetGracenoteImage(string ImageTypeRequired, string ProgramType, string PAID, int seasonId = 0)
        {
            try
            {
                Log.Info($"Processing Image: {ImageTypeRequired}");

                if (ApiAssetList != null)
                {
                    DownloadImageRequired = true;
                    SortAssets();

                    foreach (var category in ConfigImageCategories)
                    foreach (var imageTier in category.ImageTier)
                    {
                        // Populate asset list based on Tier
                        UpdateAssetList(imageTier);
                        UpdateCategoryList(ConfigImageCategories);
                        foreach (var image in ApiAssetSortedList)
                        {
                            ImageAspect(image.width, image.height);

                            if (image.category != category.CategoryName &&
                                !string.IsNullOrEmpty(image.expiredDate.ToLongDateString()))
                                continue;

                            if (MatchIdentifier(image.identifiers) &&
                                PassesImageLogic(category, image, imageTier, image.tier, IsLandscape))
                            {
                                LogIdentifierLogic(image.identifiers.Count(), image.assetId);

                                Log.Info($"Image {image.assetId} for {ImageTypeRequired} passed Image logic");

                                SetAspect(image.width, image.height,
                                    Convert.ToInt32(category.AllowedAspects.Aspect.Select(r => r.ResizeHeight)
                                        .FirstOrDefault()));
                                var encodingType = GetEncodingType(image.type);

                                //gets any existing images
                                var gnimages = CurrentMappingData.GN_Images;

                                //IMGA, IMGB etc
                                ImageQualifier = ImageMapping.ImageQualifier;

                                //Is new ingest or update?
                                if (!IsUpdate || gnimages == null)
                                {
                                    Log.Info($"Updating Database with Image {ImageTypeRequired}: {image.URI}");
                                    SetDbImages(ImageTypeRequired, image.URI);
                                    Log.Info(
                                        $"Image URI: {image.URI} for: {ImageTypeRequired} and Image Priority: {category.PriorityOrder}");

                                    return image.URI;
                                }

                                Log.Debug("Retrieved images for update package from db");

                                if (!HasAsset(DbImagesForAsset, ImageTypeRequired))
                                {
                                    var match = Regex.Match(CurrentMappingData.GN_Images,
                                        $"(?m){ImageTypeRequired}:.*?.jpg");
                                    //if "" then the image doesnt exist in the db so grab it.
                                    if (!match.Success || match.Value != "")
                                        continue;

                                    if (string.IsNullOrEmpty(match.Value))
                                        CurrentMappingData.GN_Images =
                                            CurrentMappingData.GN_Images.Replace(match.Value,
                                                $"{ImageTypeRequired}: {image.URI}");

                                    Log.Info(
                                        $"Update package detected a new image, updating db for {ImageTypeRequired} with {image.URI}");

                                    SetDbImages(ImageTypeRequired, image.URI);


                                    Log.Info(
                                        $"Image URI: {image.URI} for: {ImageTypeRequired} and Image Priority: {category.PriorityOrder}");
                                }
                                else
                                {
                                    DownloadImageRequired = false;
                                }

                                return image.URI;
                            }
                        }
                    }
                }
            }
            catch (Exception ggiEx)
            {
                Log.Error($"[GetGracenoteImage] Error Getting Gracenote Image: {ggiEx.Message}");
                if (ggiEx.InnerException != null)
                    Log.Error($"[GetGracenoteImage] Inner Exception: {ggiEx.InnerException.Message}");

                return null;
            }


            Log.Warn($"No Matching images found for: {ImageTypeRequired}");

            return null;
        }
    }
}