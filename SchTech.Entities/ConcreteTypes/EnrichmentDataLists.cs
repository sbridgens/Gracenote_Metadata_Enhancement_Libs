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

        public List<GnApiProgramsSchema.externalLinksTypeExternalLink> ExternalLinks { get; private set; }

        public void AddProgramAssetsToList(IEnumerable<GnApiProgramsSchema.assetType> programsList, string apiLevel)
        {
            if (programsList != null && programsList.Count() > 0)
            {
                if (ProgramAssets == null)
                    ProgramAssets = new List<GnApiProgramsSchema.assetType>();

                Log.Debug($"Number of Assets at {apiLevel} Level: {programsList.Count()}");
                foreach (var item in programsList) ProgramAssets.Add(item);
            }
            else
            {
                Log.Warn($"Asset is currently null at the current api level: {apiLevel}, " +
                         "will continue and check next api results for Cast data");
            }
        }

        public void AddCastMembersToList(IEnumerable<GnApiProgramsSchema.castTypeMember> castList, string apiLevel)
        {
            if (castList != null && castList.Count() > 0)
            {
                if (CastMembers == null)
                    CastMembers = new List<GnApiProgramsSchema.castTypeMember>();

                Log.Debug($"Number of Cast Members at {apiLevel} Level: {castList.Count()}");
                foreach (var cast in castList) CastMembers.Add(cast);
            }
            else
            {
                Log.Warn($"Cast info is currently null at the current api level: {apiLevel}, " +
                         "will continue and check next api results for Cast data");
            }
        }

        public void AddCrewMembersToList(IEnumerable<GnApiProgramsSchema.crewTypeMember> crewList, string apiLevel)
        {
            if (crewList != null && crewList.Count() > 0)
            {
                if (CrewMembers == null)
                    CrewMembers = new List<GnApiProgramsSchema.crewTypeMember>();

                Log.Debug($"Number of Crew Members at {apiLevel} Level: {crewList.Count()}");

                foreach (var crew in crewList) CrewMembers.Add(crew);
            }
            else
            {
                Log.Warn($"Crew info is currently null at the current api level: {apiLevel}, " +
                         "will continue and check next api results for Crew data");
            }
        }

        public void AddProgramTitlesToList(GnApiProgramsSchema.programsProgramTitles programTitles, string apiLevel)
        {
            if (programTitles.title != null && programTitles.title.Count() > 0)
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
            if (genres != null && genres.Count() > 0)
            {
                if (GenresList == null)
                    GenresList = new List<GnApiProgramsSchema.programsProgramGenre>();

                Log.Debug($"Number of Genres at {apiLevel} Level: {genres.Count()}");

                foreach (var genre in genres) GenresList.Add(genre);
            }
            else
            {
                Log.Warn($"Genre info is currently null at the current api level: {apiLevel}, " +
                         "will continue and check next api results for genre data");
            }
        }

        public void AddExternalLinksToList(IEnumerable<GnApiProgramsSchema.externalLinksTypeExternalLink> externalLinks)
        {
            if (externalLinks != null)
            {
                Log.Info("Asset has External Links, Storing IMDB Data.");

                if (ExternalLinks == null)
                    ExternalLinks = new List<GnApiProgramsSchema.externalLinksTypeExternalLink>();

                foreach (var link in externalLinks) ExternalLinks.Add(link);
            }
            else
            {
                Log.Warn("No Imdb Data available for the current package.");
            }
        }
    }
}