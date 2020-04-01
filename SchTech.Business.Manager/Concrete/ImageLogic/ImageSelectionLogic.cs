using log4net;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.DataAccess.Concrete;
using SchTech.Entities.ConcreteTypes;
using SchTech.Web.Manager.Concrete;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SchTech.DataAccess.Concrete.EntityFramework;

namespace SchTech.Business.Manager.Concrete.ImageLogic
{
    public class ImageSelectionLogic
    {
        /// <summary>
        ///     Initialize log4net
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger(typeof(ImageSelectionLogic));
        private List<GnApiProgramsSchema.assetType> ApiAssetSortedList { get; set; }
        private string IdentifierType { get; set; }
        private List<string> AssetTier { get; set; }
        private string IdentifierId { get; set; }
        private bool IsLandscape { get; set; }
        private bool IsSquare { get; set; }


        public List<GnApiProgramsSchema.assetType> ApiAssetList { get; set; }
        public List<Image_Category> ConfigImageCategories { get; set; }
        public Dictionary<string, string> DbImagesForAsset { get; set; }
        public GN_Mapping_Data CurrentMappingData { get; set; }
        public bool DownloadImageRequired { get; private set; }
        public ImageMapping ImageMapping { get; set; }
        public string ImageQualifier { get; private set; }
        public static string DbImages { get; set; }
        public bool IsUpdate { get; set; }

        /// <summary>
        ///     Downloads the image from the configured image url and the image path found in the api
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="destinationImage"></param>
        /// <returns></returns>
        public void DownloadImage(string sourceImage, string destinationImage)
        {
            try
            {
                using (var webClient = new WebClientManager())
                {
                    var downloadUrl = $"{ADIWF_Config.MediaCloud}/{sourceImage}";

                    webClient.DownloadWebBasedFile(
                        downloadUrl,
                        false,
                        destinationImage);

                    _log.Info($"Successfully Downloaded Image: {sourceImage} as {destinationImage}");
                }
            }
            catch (Exception diEx)
            {
                _log.Error($"[DownloadImage] Error Downloading Image: {diEx.Message}");
                if (diEx.InnerException != null)
                    _log.Error($"[DownloadImage] Inner Exception: {diEx.InnerException.Message}");

            }
        }

        private static string ImageTrim(string imageUrl)
        {
            var cleanString = imageUrl.TrimStart();
            return cleanString.TrimEnd();
        }

        /// <summary>
        ///     Used during update packages to detect if an Image is required for download
        ///     As updates may need the image adi section but not the image if no changes are required
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <param name="imageTypeRequired"></param>
        /// <param name="currentImageUri"></param>
        /// <returns></returns>
        private static bool HasAsset(Dictionary<string, string> keyValuePairs, string imageTypeRequired, string currentImageUri)
        {
            return (from item in keyValuePairs
                    let assetKey = ImageTrim(item.Key)
                    let assetValue = ImageTrim(item.Value)
                    where imageTypeRequired.Equals(assetKey) &&
                          currentImageUri.Equals(assetValue)
                    select assetKey).Any();
        }

        private void SortAssets()
        {
            ApiAssetSortedList = new List<GnApiProgramsSchema.assetType>();

            ApiAssetSortedList = IsLandscape
                ? ApiAssetList.OrderByDescending(i => i.identifiers.Any()).ThenBy(w => w.width).ThenBy(h => h.height)
                    .ToList()
                : ApiAssetList.OrderByDescending(i => i.identifiers.Any()).ThenBy(h => h.height).ThenBy(w => w.width)
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
                _log.Error($"[GetFileAspectRatio] Error Getting Image File Aspect Ratio: {ex.Message}");
                if (ex.InnerException != null)
                    _log.Error($"[GetFileAspectRatio] Inner Exception: {ex.InnerException.Message}");

                return null;
            }
        }

        public static string GetImageName(string imageUri, string imageMapping)
        {
            return imageUri.Replace(imageUri, $"{imageMapping}_{imageUri.Replace("?trim=true", "")}");
        }

        private void UpdateCategoryList(List<Image_Category> imageCategories)
        {
            ConfigImageCategories = imageCategories.Where(t => AssetTier.Contains(t.ImageTier.ToString()))
                .OrderBy(p => p.PriorityOrder)
                .ToList();
        }


        private bool MatchIdentifier(
            IEnumerable<GnApiProgramsSchema.identifierType> identifiers,
            string imageName
        )
        {
            foreach (var idArr in identifiers)
            {
                foreach (var confIdentifier in ImageMapping.ImageIdentifier.Where(
                    confIdentifier => idArr != null &&
                                      idArr.type == confIdentifier.Type &&
                                      idArr.id.FirstOrDefault() == confIdentifier.Id))
                {
                    IdentifierId = idArr.id.FirstOrDefault();
                    IdentifierType = idArr.type;
                    _log.Debug($"Image: {imageName} - Identifier Type: {IdentifierType} " +
                               $"Identifier ID: {IdentifierId}" +
                               $" matches Config value: {confIdentifier.Id}");
                    return true;
                }
            }


            _log.Debug($"Image: {imageName} - No matching identifier present for image, " +
                      "no identifier rules applied.");

            return false;
        }



        /// <summary>
        ///     Image logic rules, this is processed in order and can be updated/detracted from dependent on
        ///     image requirement changes
        /// </summary>
        /// <param name="imageCategory"></param>
        /// <param name="image"></param>
        /// <param name="configTier"></param>
        /// <param name="imageTier"></param>
        /// <param name="isLandscape"></param>
        /// <returns></returns>
        private bool PassesImageLogic(Image_Category imageCategory, GnApiProgramsSchema.assetType image,
            string configTier, string imageTier, bool isLandscape)
        {
            if (imageTier == null)
                imageTier = "";

            return imageCategory.AllowedAspects.Aspect.Any(
                       a => a.AspectHeight == image.height &&
                            a.AspectWidth == image.width) &&
                   configTier == imageTier &&
                   IsLandscape == isLandscape &&
                   !IsSquare;
        }

        private void SetDbImages(string imageTypeRequired, string uri)
        {
            var gnimages = CurrentMappingData.GN_Images;


            DbImages = String.IsNullOrEmpty(gnimages)
                ? $"{imageTypeRequired}: {uri}"
                : $"{gnimages}, {imageTypeRequired}: {uri}";
        }


        public string GetGracenoteImage(string imageTypeRequired)
        {
            try
            {
                _log.Info($"Processing Image: {imageTypeRequired}");

                if (ApiAssetList != null)
                {
                    DownloadImageRequired = true;
                    SortAssets();
                    int logged;
                    //iterate the db config for images
                    foreach (var category in ConfigImageCategories)
                    {
                        logged = 0;
                        //Iterate each image category based on asset tier
                        foreach (var imageTier in category.ImageTier)
                        {
                            // Populate asset list based on Tier
                            UpdateAssetList(imageTier);
                            UpdateCategoryList(ConfigImageCategories);

                            //iterate each image inside the sorted api asset list
                            foreach (var image in ApiAssetSortedList)
                            {
                                ImageAspect(image.width, image.height);

                                //validate the image category is a match with the config
                                //and that the image is not flagged as expired on the api
                                if (image.category != category.CategoryName &&
                                    !String.IsNullOrEmpty(image.expiredDate.ToLongDateString()))
                                    continue;

                                //Check if the images contain identifiers
                                if (image.identifiers.Any())
                                {
                                    logged = 1;
                                    //Check the identifiers match the config.
                                    if (!MatchIdentifier(image.identifiers, image.assetId))
                                    {
                                        continue;
                                    }
                                }
                                else if (logged == 0)
                                {
                                    _log.Debug($"No Identifier config found for current image Type -" +
                                               $" {imageTypeRequired}");
                                    logged++;
                                }

                                //Validate the current image can be used for ingest.
                                if (!PassesImageLogic(category, image, imageTier, image.tier, IsLandscape))
                                    continue;


                                //LogIdentifierLogic(image.identifiers.Count(), image.assetId);

                                _log.Info($"Image {image.assetId} for {imageTypeRequired} passed Image logic");

                                if(imageTypeRequired == "TitleTreatment")
                                    _log.Info("");
                                //check if the image is flagged as a requires trimming in config
                                var requiresTrim = Convert.ToBoolean(category.AllowedAspects.Aspect
                                    .Select(r => r.TrimImage).FirstOrDefault());

                                //if requires trimming then append the trim=true to the image uri
                                var imageUri = requiresTrim
                                    ? $"{image.URI}?trim=true"
                                    : image.URI;

                                //gets any existing images
                                var gnimages = CurrentMappingData.GN_Images;

                                //IMGA, IMGB etc
                                ImageQualifier = ImageMapping.ImageQualifier;

                                //Is new ingest or update?
                                if (!IsUpdate || gnimages == null)
                                {
                                    _log.Info($"Updating Database with Image {imageTypeRequired}: {image.URI}");
                                    SetDbImages(imageTypeRequired, image.URI);
                                    _log.Info(
                                        $"Image URI: {image.URI} for: {imageTypeRequired} and Image Priority: {category.PriorityOrder}");

                                    return imageUri;
                                }

                                _log.Debug("Retrieved images for update package from db");

                                //if this is an update get the db images and validate if the image matches 
                                //or has been updated, if there is no match download else return false as the image matches
                                if (!HasAsset(DbImagesForAsset, imageTypeRequired, image.URI))
                                {
                                    var imageExtension = Path.GetExtension(image.URI);
                                    Match match = Regex.Match(CurrentMappingData.GN_Images, $"(?m){imageTypeRequired}:.*?{imageExtension}");
                                    //if "" then the image doesnt exist in the db so grab it.
                                    if (match.Success || match.Value == "")
                                    {
                                        //New image required so update the db and set the uri ready for download.
                                        string newUri = $"{imageTypeRequired}: {image.URI}";

                                        _log.Debug($"Update package detected a new image, updating db for {imageTypeRequired} with {image.URI}");
                                        //Added this check in to ensure images are updated if missing or changed.
                                        CurrentMappingData.GN_Images = match.Value == ""
                                            ? CurrentMappingData.GN_Images = $"{CurrentMappingData.GN_Images}, {imageTypeRequired}: {image.URI}"
                                            : CurrentMappingData.GN_Images = CurrentMappingData.GN_Images.Replace(match.Value, newUri);

                                        
                                        //update DbImages list, this will be saved by calling class.
                                        DbImages = CurrentMappingData.GN_Images;

                                        _log.Info($"Image URI: {image.URI} for: {imageTypeRequired} and Image Priority: {category.PriorityOrder}");
                                    }
                                }
                                else
                                {
                                    _log.Info("Update Package - image is up to date not required for download.");
                                    DownloadImageRequired = false;
                                }

                                return imageUri;
                            }
                        }
                    }
                }
            }
            catch (Exception ggiEx)
            {
                _log.Error($"[GetGracenoteImage] Error Getting Gracenote Image: {ggiEx.Message}");
                if (ggiEx.InnerException != null)
                    _log.Error($"[GetGracenoteImage] Inner Exception: {ggiEx.InnerException.Message}");

                return null;
            }


            _log.Warn($"No Matching images found for: {imageTypeRequired}");

            return null;
        }
    }
}