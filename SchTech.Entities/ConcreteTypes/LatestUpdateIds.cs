using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Core.Entities;

namespace SchTech.Entities.ConcreteTypes
{
    public class LatestUpdateIds : IEntity
    {
        public int id { get; set; }
        public long LastMappingUpdateIdChecked { get; set; }
        public long LastLayer1UpdateIdChecked { get; set; }
        public long LastLayer2UpdateIdChecked { get; set; }
    }
}
