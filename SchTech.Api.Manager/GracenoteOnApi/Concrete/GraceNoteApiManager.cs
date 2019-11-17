using System;
using System.Collections.Generic;
using System.Linq;
using SchTech.Api.Manager.GracenoteOnApi.Abstract;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using SchTech.Configuration.Manager.Schema.ADIWFE;

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


            if (MovieEpisodeProgramData?.externalLinks != null)
            {
                foreach (var link in MovieEpisodeProgramData.externalLinks)
                {
                    externalLinks.Add(link);
                }
            }

            if (ShowSeriesSeasonProgramData?.externalLinks != null)
            {
                foreach (var link in ShowSeriesSeasonProgramData.externalLinks)
                {
                    externalLinks.Add(link);
                }
            }

            return externalLinks;
        }


        public string GetConnectorId()
        {
            return MovieEpisodeProgramData.connectorId;
        }


        public string GetEpisodeTitle()
        {
            return MovieEpisodeProgramData.episodeInfo?
                       .title != null
                ? MovieEpisodeProgramData.episodeInfo?.title.Value
                : (MovieEpisodeProgramData.episodeInfo?.number != null
                    ? $"Episode {MovieEpisodeProgramData.episodeInfo?.number}"
                    : $"Episode {GetSeriesTitle() ?? MovieEpisodeProgramData.partNumber}"
                );
        }

        public string GetSeriesTitle()
        {
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
            return ShowSeriesSeasonProgramData.seriesId;
        }

        public int GetSeasonId()
        {
            var sId = Convert.ToInt32(MovieEpisodeProgramData.seasonId);
            return sId > 0 ? sId : 0;
        }


        public string GetEpisodeOrdinalValue()
        {
            var num = MovieEpisodeProgramData.episodeInfo?.number;

            return Convert.ToInt32(num) != 0
                ? num
                : "100001";
        }

        public string GetSeriesOrdinalValue()
        {
            return Convert.ToInt32(MovieEpisodeProgramData.seasonId) == 0
                ? "100000"
                : MovieEpisodeProgramData.episodeInfo?.season;
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
            return Convert.ToInt32(SeasonData.seasonId);
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
            return !string.IsNullOrWhiteSpace(ADIWF_Config.Prefix_Show_ID_Value.ToString())
                ? $"{ADIWF_Config.Prefix_Show_ID_Value}{ShowSeriesSeasonProgramData.seriesId}"
                : ShowSeriesSeasonProgramData.seriesId;
        }

        public string GetFirstSeasonId()
        {
            return ShowSeriesSeasonProgramData.seasons.FirstOrDefault(s => s.seasonId != null)?.seasonId;
        }

        public string GetLastSeasonId()
        {
            return ShowSeriesSeasonProgramData.seasons.LastOrDefault(s => s.seasonId != null)?.seasonId;
        }

        public GnApiProgramsSchema.programsProgramEpisodeInfo GetEpisodeInfo()
        {
            return MovieEpisodeProgramData.episodeInfo;
        }

        public List<GnApiProgramsSchema.programsProgramSeason> GetSeasonInfo()
        {
            return ShowSeriesSeasonProgramData.seasons ?? MovieEpisodeProgramData.seasons;
        }
    }
}
