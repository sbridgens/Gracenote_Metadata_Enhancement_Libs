using SchTech.Core.Entities;
using System;

namespace SchTech.Entities.ConcreteTypes
{
    public class Layer1UpdateTracking : IEntity
    {
        public int Id { get; set; }
        public Guid IngestUUID { get; set; }
        public string GN_Paid { get; set; }
        public string GN_TMSID { get; set; }
        public string Layer1_UpdateId { get; set; }
        public DateTime Layer1_UpdateDate { get; set; }
        public string Layer1_NextUpdateId { get; set; }
        public string Layer1_MaxUpdateId { get; set; }
        public string Layer1_RootId { get; set; }
        public DateTime UpdatesChecked { get; set; }
        public bool RequiresEnrichment { get; set; }
    }
}