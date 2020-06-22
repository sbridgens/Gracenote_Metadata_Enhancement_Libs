using SchTech.Core.Entities;

namespace SchTech.Entities.ConcreteTypes
{
    public class CategoryMapping : IEntity
    {
        public int Id { get; set; }

        public string ProviderId { get; set; }

        public string ProviderName { get; set; }

        public string CategoryValue { get; set; }
    }
}
