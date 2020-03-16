using SchTech.Api.Manager.GracenoteOnApi.Abstract;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchTech.Api.Manager.GracenoteOnApi.Concrete
{
    public class GraceNoteApiManager : IGracenoteApi
    {
        public GnOnApiProgramMappingSchema.@on CoreGnMappingData { get; set; }
        public GnApiProgramsSchema.@on CoreProgramData { get; set; }
        public GnApiProgramsSchema.@on CoreSeriesData { get; set; }
        public GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping GraceNoteMappingData { get; set; }
        public GnApiProgramsSchema.programsProgram MovieEpisodeProgramData { get; set; }
        public GnApiProgramsSchema.programsProgram ShowSeriesSeasonProgramData { get; set; }
        public GnApiProgramsSchema.programsProgramSeason SeasonData { get; set; }

        public List<GnApiProgramsSchema.externalLinksTypeExternalLink> ExternalLinks()
        {
            var externalLinks = new List<GnApiProgramsSchema.externalLinksTypeExternalLink>();


            if (MovieEpisodeProgramData.externalLinks.Any())
                externalLinks.AddRange(MovieEpisodeProgramData.externalLinks);
            
            if (ShowSeriesSeasonProgramData != null && ShowSeriesSeasonProgramData.externalLinks.Any())
                externalLinks.AddRange(ShowSeriesSeasonProgramData.externalLinks);

            return externalLinks;
        }

        public string GetUpdateId()
        {
            return MovieEpisodeProgramData.updateId;
        }

        public string GetConnectorId()
        {
            return MovieEpisodeProgramData.connectorId;
        }


        public string GetEpisodeTitle()
        {
            if (MovieEpisodeProgramData.movieInfo != null)
                return null;

            return MovieEpisodeProgramData.episodeInfo?
                       .title != null
                ? MovieEpisodeProgramData.episodeInfo?.title.Value
                : MovieEpisodeProgramData.episodeInfo?.number != null
                    ? $"Episode {MovieEpisodeProgramData.episodeInfo?.number}"
                    : $"Episode {GetSeriesTitle() ?? MovieEpisodeProgramData.partNumber}";
        }

        public string GetSeriesTitle()
        {
            var tets = MovieEpisodeProgramData.progType;
            if (MovieEpisodeProgramData.movieInfo != null)
                return null;

            return MovieEpisodeProgramData
                .titles
                .title
                .Where(t => t.subType.Contains("Main")
                    ? t.subType.Equals("Main")
                    : t.subType.Equals("AKA"))
                .Select(r => r.Value)
                .FirstOrDefault();
        }

        public string GetSeriesId()
        {
            return Convert.ToInt32(MovieEpisodeProgramData.seriesId) > 0
                ? MovieEpisodeProgramData.seriesId
                : GetSeasonId() > 0
                    ? GetSeasonId().ToString()
                    : GetConnectorId();
        }

        public int GetSeasonId()
        {
            var sId = Convert.ToInt32(MovieEpisodeProgramData?.seasonId);
            return sId > 0 ? sId : 0;
        }

        public string GetEpisodeOrdinalValue()
        {
            var num = Convert.ToInt32(MovieEpisodeProgramData.episodeInfo?.number);

            return Convert.ToInt32(num) > 0
                ? num.ToString()
                : (num == 0
                    ? "100001"
                    : num.ToString());
        }

        public string GetSeriesOrdinalValue()
        {
            return Convert.ToInt32(MovieEpisodeProgramData.seasonId) == 0
                ? "100000"
                : MovieEpisodeProgramData.episodeInfo?.season;
        }

        public string GetGnSeriesId()
        {
            return GetSeasonId() == 0
                ? GetConnectorId()
                : GetSeasonId().ToString();
        }

        public int GetNumberOfSeasons()
        {
            return ShowSeriesSeasonProgramData.seasons?.Count ?? 0;
        }

        public void SetSeasonData()
        {
            SeasonData = ShowSeriesSeasonProgramData.seasons?
                .FirstOrDefault(s => s.seasonId == MovieEpisodeProgramData.seasonId);
        }

        public int SetSeasonId()
        {
            return Convert.ToInt32(SeasonData?.seasonId);
        }

        public string GetShowName()
        {
            return ShowSeriesSeasonProgramData.titles.title
                .Where(t => t.type == "full" && t.size == "120")
                .Select(t => t.Value)
                .FirstOrDefault()
                ?.ToString();
        }

        public string GetShowId()
        {
            return !string.IsNullOrWhiteSpace(ADIWF_Config.Prefix_Show_ID_Value)
                ? $"{ADIWF_Config.Prefix_Show_ID_Value}{ShowSeriesSeasonProgramData.seriesId}"
                : ShowSeriesSeasonProgramData.seriesId;
        }

        public string GetEpisodeSeason()
        {
            return MovieEpisodeProgramData.episodeInfo?.season;
        }

        public List<GnApiProgramsSchema.programsProgramSeason> GetSeasonInfo()
        {
            return ShowSeriesSeasonProgramData.seasons ?? MovieEpisodeProgramData.seasons;
        }

        public DateTime GetSeriesPremiere()
        {
            return ShowSeriesSeasonProgramData.seriesPremiere;
        }

        public DateTime GetSeriesFinale()
        {
            return ShowSeriesSeasonProgramData.seriesFinale;
        }

        public DateTime? GetSeasonPremiere()
        {
            var first =
                ShowSeriesSeasonProgramData.seasons?.FirstOrDefault();

            return first?.seasonPremiere;
        }

        public DateTime? GetSeasonFinale()
        {
            var last =
                ShowSeriesSeasonProgramData.seasons?.LastOrDefault();
            return last?.seasonFinale;
        }
    }
}