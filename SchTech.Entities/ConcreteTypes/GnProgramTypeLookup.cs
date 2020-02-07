using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Core.Entities;

namespace SchTech.Entities.ConcreteTypes
{
    public class GnProgramTypeLookup : IEntity
    {
        public int Id { get; set; }

        public string GnProgramType { get; set; }

        public string GnProgramSubType { get; set; }

        public string LgiProgramType { get; set; }

        public int LgiProgramTypeId { get; set; }
    }
}
