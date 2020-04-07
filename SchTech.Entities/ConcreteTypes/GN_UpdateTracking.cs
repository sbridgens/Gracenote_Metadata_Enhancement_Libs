using System;
using SchTech.Core.Entities;

namespace SchTech.Entities.ConcreteTypes
{
    public class GN_UpdateTracking : IEntity
    {
        public int Id { get; set; }
        public Guid TrackingIngestGuid { get; set; }
        public string TrackingGnProviderId { get; set; }

        public string MappingUpdateId { get; set; }
        public DateTime MappingUpdateDate { get; set; }
        public string MappingNextUpdateId { get; set; }
        public string MappingMaxUpdateId { get; set; }
        public string MappingRootId { get; set; }

        public string Layer1UpdateId { get; set; }
        public DateTime Layer1UpdateDate { get; set; }
        public string Layer1NextUpdateId { get; set; }
        public string Layer1MaxUpdateId { get; set; }
        public string Layer1RootId { get; set; }

        public string Layer2UpdateId { get; set; }
        public DateTime Layer2UpdateDate { get; set; }
        public string Layer2NextUpdateId { get; set; }
        public string Layer2MaxUpdateId { get; set; }
        public string Layer2RootId { get; set; }

        public DateTime UpdatesChecked { get; set; }
        public bool RequiresEnrichment { get; set; }
    }
}
