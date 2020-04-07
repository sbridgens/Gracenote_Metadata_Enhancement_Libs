using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Core.Entities;

namespace SchTech.Entities.ConcreteTypes
{
    public class GN_UpdateTracking : IEntity
    {
        public int id { get; set; }
        public Guid IngestUUID { get; set; }
        public string GN_ProviderId { get; set; }

        public string Mapping_UpdateId { get; set; }
        public DateTime Mapping_UpdateDate { get; set; }
        public string Mapping_NextUpdateId { get; set; }
        public string Mapping_MaxUpdateId { get; set; }
        public string Mapping_RootId { get; set; }

        public string Layer1_UpdateId { get; set; }
        public DateTime Layer1_UpdateDate { get; set; }
        public string Layer1_NextUpdateId { get; set; }
        public string Layer1_MaxUpdateId { get; set; }
        public string Layer1_RootId { get; set; }

        public string Layer2_UpdateId { get; set; }
        public DateTime Layer2_UpdateDate { get; set; }
        public string Layer2_NextUpdateId { get; set; }
        public string Layer2_MaxUpdateId { get; set; }
        public string Layer2_RootId { get; set; }

        public DateTime UpdatesChecked { get; set; }
        public bool RequiresEnrichment { get; set; }
    }
}
