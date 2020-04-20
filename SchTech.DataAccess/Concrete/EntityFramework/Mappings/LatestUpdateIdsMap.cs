using System.Data.Entity.ModelConfiguration;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework.Mappings
{
    public class LatestUpdateIdsMap : EntityTypeConfiguration<LatestUpdateIds>
    {
        public LatestUpdateIdsMap()
        {
            //mapped to EntityTypeConfiguration LatestUpdateIds
            ToTable(@"LatestUpdateIds", @"dbo");

            //Primary key is set here
            HasKey(x => x.id);
            Property(x => x.LastMappingUpdateIdChecked).HasColumnName("LastMappingUpdateIdChecked");
            Property(x => x.LastLayer1UpdateIdChecked).HasColumnName("LastLayer1UpdateIdChecked");
            Property(x => x.LastLayer2UpdateIdChecked).HasColumnName("LastLayer2UpdateIdChecked");
        }
    }
}