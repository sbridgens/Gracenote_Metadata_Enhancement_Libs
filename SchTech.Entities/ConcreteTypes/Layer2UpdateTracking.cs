using SchTech.Core.Entities;
using System;

namespace SchTech.Entities.ConcreteTypes
{
    public class Layer2UpdateTracking : IEntity
    {
        public int Id { get; set; }
        public Guid IngestUUID { get; set; }
        public string GN_Paid { get; set; }
        public string GN_connectorId { get; set; }
        public string Layer2_UpdateId { get; set; }
        public DateTime Layer2_UpdateDate { get; set; }
        public string Layer2_NextUpdateId { get; set; }
        public string Layer2_MaxUpdateId { get; set; }
        public string Layer2_RootId { get; set; }
        public DateTime UpdatesChecked { get; set; }
        public bool RequiresEnrichment { get; set; }
    }
}