using SchTech.Entities.ConcreteTypes;
using System.Data.Entity.ModelConfiguration;

namespace SchTech.DataAccess.Concrete.EntityFramework.Mappings
{
    public class CategoryMappingMap : EntityTypeConfiguration<CategoryMapping>
    {
        public CategoryMappingMap()
        {
            //mapped to EntityTypeConfiguration Adi_data
            ToTable(@"CategoryMapping", @"dbo");
            //Primary key is set here
            HasKey(x => x.Id);


            //EntityTypeConfiguration properties are now mapped
            //to the db tables matching the property names.
            Property(x => x.ProviderId).HasColumnName("ProviderId");
            Property(x => x.ProviderName).HasColumnName("ProviderName");
            Property(x => x.CategoryValue).HasColumnName("CategoryValye");
        }
    }
}