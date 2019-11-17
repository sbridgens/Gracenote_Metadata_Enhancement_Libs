using System;
using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using SchTech.Configuration.Manager.Parameters;
using SchTech.Configuration.Manager.Schema.ADIWFE;
using SchTech.DataAccess.Concrete.EntityFramework.Mappings;
using SchTech.Entities.ConcreteTypes;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace SchTech.DataAccess.Concrete.EntityFramework.Contexts
{
    public class ADI_EnrichmentContext : DbContext
    {
        private readonly AdiWfeProperties _adiWfeProperties;
        private DbModelBuilder modelBuilder;
        public ADI_EnrichmentContext()
        {
            if (_adiWfeProperties == null)
            {
                _adiWfeProperties = new AdiWfeProperties();
            }
        }


        public ADI_EnrichmentContext(DbContextOptions<ADI_EnrichmentContext> options)
            : base(options)
        {   
        }

        public virtual Microsoft.EntityFrameworkCore.DbSet<Adi_Data> Adi_Data { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<GN_ImageLookup> GN_ImageLookup { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<GN_Mapping_Data> GN_Mapping_Data { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer($"Server={ADIWF_Config.Database_Host},1433;" +
                                            $"Database={ADIWF_Config.Database_Name};" +
                                            $"Trusted_Connection={ADIWF_Config.Integrated_Security};" +
                                            $"MultipleActiveResultSets=True;");

                if (modelBuilder == null)
                {
                    modelBuilder = new DbModelBuilder();
                    modelBuilder.Configurations.Add(new AdiEnrichmentMap());
                    modelBuilder.Configurations.Add(new GnImageLookupMap());
                    modelBuilder.Configurations.Add(new GnMappingDataMap());
                }
               
            }
        }

        /// <summary>
        /// SCH Tech added to allow update of entity based on property name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public void UpdateEntity<T>(int id, string propertyName, string propertyValue) where T : class
        {

            var entity = this.Set<T>().Find(id);
            var prop = typeof(T).GetProperty(propertyName);
            if (prop != null)
            {
                var val = Convert.ChangeType(propertyValue, prop.PropertyType);
                prop.SetValue(entity, val);
            }

            this.SaveChanges();
        }

        /// <summary>
        /// SCH Tech added to allow return of data based on property name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetEntity<T>(int id, string propertyName) where T : class
        {
            var entity = this.Set<T>().Find(id);
            var prop = typeof(T).GetProperty(propertyName);

            if (prop != null)
                return prop.GetValue(entity, null).ToString();
            else
            {
                return string.Empty;
            }
        }

        
    }


}