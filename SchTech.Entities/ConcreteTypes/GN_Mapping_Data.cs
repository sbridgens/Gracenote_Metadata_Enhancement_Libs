using System;
using SchTech.Core.Entities;

namespace SchTech.Entities.ConcreteTypes
{
    public class GN_Mapping_Data : IEntity
    {
        public int Id { get; set; }
        public string GN_TMSID { get; set; }
        public string GN_RootID { get; set; }
        public string GN_Status { get; set; }
        public string GN_ProviderId { get; set; }
        public string GN_Paid { get; set; }
        public string GN_Pid { get; set; }
        public string GN_programMappingId { get; set; }
        public DateTime? GN_creationDate { get; set; }
        public string GN_updateId { get; set; }
        public string GN_Images { get; set; }
        public DateTime? GN_Availability_Start { get; set; }
        public DateTime? GN_Availability_End { get; set; }
        public string GN_connectorId { get; set; }
        public int? GN_SeasonId { get; set; }
        public int? GN_SeasonNumber { get; set; }
        public long? GN_SeriesId { get; set; }
        public long? GN_EpisodeNumber { get; set; }
        public string GN_SeriesTitle { get; set; }
        public string GN_EpisodeTitle { get; set; }
    }
}