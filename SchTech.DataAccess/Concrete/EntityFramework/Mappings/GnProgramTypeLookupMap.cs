using SchTech.Entities.ConcreteTypes;
using System.Data.Entity.ModelConfiguration;

namespace SchTech.DataAccess.Concrete.EntityFramework.Mappings
{
    public class GnProgramTypeLookupMap : EntityTypeConfiguration<GnProgramTypeLookup>
    {
        public GnProgramTypeLookupMap()
        {
            //mapped to EntityTypeConfiguration Adi_data
            ToTable(@"GnProgramTypeLookup", @"dbo");
            //Primary key is set here
            HasKey(x => x.Id);

            //EntityTypeConfiguration properties are now mapped
            //to the db tables matching the property names.
            Property(x => x.GnProgramType).HasColumnName("GnProgramType");
            Property(x => x.GnProgramSubType).HasColumnName("GnProgramSubType");
            Property(x => x.LgiProgramType).HasColumnName("LgiProgramType");
            Property(x => x.LgiProgramTypeId).HasColumnName("LgiProgramTypeId");
        }
    }
}

