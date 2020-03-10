using log4net;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using System.Collections.Generic;
using System.Linq;

namespace SchTech.Entities.ConcreteTypes
{
    public class EnrichmentDataLists
    {
        /// <summary>
        ///     Initialize Log4net
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(EnrichmentDataLists));

        public List<GnApiProgramsSchema.assetType> ProgramAssets { get; private set; }

        public List<GnApiProgramsSchema.castTypeMember> CastMembers { get; private set; }

        public List<GnApiProgramsSchema.crewTypeMember> CrewMembers { get; private set; }

        public List<GnApiProgramsSchema.titleDescType> ProgramTitles { get; private set; }

        public List<GnApiProgramsSchema.programsProgramGenre> GenresList { get; private set; }

        private List<GnApiProgramsSchema.externalLinksTypeExternalLink> ExternalLinks { get; set; }

        public void AddProgramAssetsToList(IEnumerable<GnApiProgramsSchema.assetType> programsList, string apiLevel)
        {
            var assetTypes = programsList.ToList();

            if (assetTypes.Any())
            {
                if (ProgramAssets == null)
                    ProgramAssets = new List<GnApiProgramsSchema.assetType>();

                Log.Debug($"Number of Assets at {apiLevel} Level: {assetTypes.Count()}");
                foreach (var item in assetTypes) ProgramAssets.Add(item);
            }
            else
            {
                Log.Warn($"Asset is currently null at the current api level: {apiLevel}, " +
                         "will continue and check next api results for Cast data");
            }
        }

        public void AddCastMembersToList(IEnumerable<GnApiProgramsSchema.castTypeMember> castList, string apiLevel)
        {
            var castTypeMembers = castList.ToList();
            if (castTypeMembers.Any())
            {
                if (CastMembers == null)
                    CastMembers = new List<GnApiProgramsSchema.castTypeMember>();

                Log.Debug($"Number of Cast Members at {apiLevel} Level: {castTypeMembers.Count()}");
                foreach (var cast in castTypeMembers) CastMembers.Add(cast);
            }
            else
            {
                Log.Warn($"Cast info is currently null at the current api level: {apiLevel}, " +
                         "will continue and check next api results for Cast data");
            }
        }

        public void AddCrewMembersToList(IEnumerable<GnApiProgramsSchema.crewTypeMember> crewList, string apiLevel)
        {
            var crewTypeMembers = crewList.ToList();
            if (crewTypeMembers.Any())
            {
                if (CrewMembers == null)
                    CrewMembers = new List<GnApiProgramsSchema.crewTypeMember>();

                Log.Debug($"Number of Crew Members at {apiLevel} Level: {crewTypeMembers.Count()}");

                foreach (var crew in crewTypeMembers) CrewMembers.Add(crew);
            }
            else
            {
                Log.Warn($"Crew info is currently null at the current api level: {apiLevel}, " +
                         "will continue and check next api results for Crew data");
            }
        }

        public void AddProgramTitlesToList(GnApiProgramsSchema.programsProgramTitles programTitles, string apiLevel)
        {
            if (programTitles.title != null && programTitles.title.Any())
            {
                if (ProgramTitles == null)
                    ProgramTitles = new List<GnApiProgramsSchema.titleDescType>();

                Log.Debug($"Number of Program Titles at {apiLevel} Level: {programTitles.title.Count()}");

                foreach (var title in programTitles.title.ToList()) ProgramTitles.Add(title);
            }
            else
            {
                Log.Warn($"Title info is currently null at the current api level: {apiLevel}, " +
                         "will continue and check next api results for Title data");
            }
        }

        public void AddGenresToList(IEnumerable<GnApiProgramsSchema.programsProgramGenre> genres, string apiLevel)
        {
            var programsProgramGenres = genres.ToList();
            if (programsProgramGenres.Any())
            {
                if (GenresList == null)
                    GenresList = new List<GnApiProgramsSchema.programsProgramGenre>();

                Log.Debug($"Number of Genres at {apiLevel} Level: {programsProgramGenres.Count()}");

                foreach (var genre in programsProgramGenres) GenresList.Add(genre);
            }
            else
            {
                Log.Warn($"Genre info is currently null at the current api level: {apiLevel}, " +
                         "will continue and check next api results for genre data");
            }
        }

        public void AddExternalLinksToList(IEnumerable<GnApiProgramsSchema.externalLinksTypeExternalLink> externalLinks)
        {
            var externalLinksTypeExternalLinks = externalLinks.ToList();

            if (externalLinksTypeExternalLinks.Any())
            {
                Log.Info("Asset has External Links, Storing IMDB Data.");

                if (ExternalLinks == null)
                    ExternalLinks = new List<GnApiProgramsSchema.externalLinksTypeExternalLink>();

                foreach (var link in externalLinksTypeExternalLinks)
                    ExternalLinks.Add(link);
            }
            else
            {
                Log.Warn("No Imdb Data available for the current package.");
            }
        }
    }
}