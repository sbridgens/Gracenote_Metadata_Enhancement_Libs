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
        public Int64 LastMappingUpdateIdChecked { get; set; }
        public Int64 LastLayer1UpdateIdChecked { get; set; }
        public Int64 LastLayer2UpdateIdChecked { get; set; }
        public bool InOperation { get; set; }
    }
}
