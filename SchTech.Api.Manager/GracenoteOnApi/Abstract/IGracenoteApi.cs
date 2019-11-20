using SchTech.Api.Manager.GracenoteOnApi.Schema.GNMappingSchema;
using SchTech.Api.Manager.GracenoteOnApi.Schema.GNProgramSchema;
using System.Collections.Generic;

namespace SchTech.Api.Manager.GracenoteOnApi.Abstract
{
    public interface IGracenoteApi
    {
        GnOnApiProgramMappingSchema.@on CoreGnMappingData { get; set; }
        GnApiProgramsSchema.@on CoreProgramData { get; set; }
        GnApiProgramsSchema.@on CoreSeriesData { get; set; }
        GnOnApiProgramMappingSchema.onProgramMappingsProgramMapping GraceNoteMappingData { get; set; }
        GnApiProgramsSchema.programsProgram MovieEpisodeProgramData { get; set; }
        GnApiProgramsSchema.programsProgram ShowSeriesSeasonProgramData { get; set; }
        GnApiProgramsSchema.programsProgramSeason SeasonData { get; set; }
        List<GnApiProgramsSchema.externalLinksTypeExternalLink> ExternalLinks();

        string GetConnectorId();

        string GetEpisodeTitle();

        string GetSeriesTitle();

        string GetSeriesId();

        int GetSeasonId();
        string GetEpisodeOrdinalValue();

        string GetSeriesOrdinalValue();

        int GetNumberOfSeasons();

        void SetSeasonData();

        int SetSeasonId();

        string GetShowName();

        string GetShowId();
    }
}