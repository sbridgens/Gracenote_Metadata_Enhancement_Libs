using SchTech.Core.Entities;
using System;

namespace SchTech.Entities.ConcreteTypes
{
    //getters and setters for tables
    public class MappingsUpdateTracking : IEntity
    {
        public int Id { get; set; }
        public Guid IngestUUID { get; set; }

        public string GN_ProviderId { get; set; }
        public string Mapping_UpdateId { get; set; }

        public DateTime Mapping_UpdateDate { get; set; }

        public string Mapping_NextUpdateId { get; set; }
        public string Mapping_MaxUpdateId { get; set; }
        public string Mapping_RootId { get; set; }

        public DateTime UpdatesChecked { get; set; }

        public bool RequiresEnrichment { get; set; }
    }
}
