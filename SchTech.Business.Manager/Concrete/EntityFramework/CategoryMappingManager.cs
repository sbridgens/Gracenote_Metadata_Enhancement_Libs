using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.Entities.ConcreteTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SchTech.Business.Manager.Concrete.EntityFramework
{
    public sealed class CategoryMappingManager : ICategoryMappingService
    {
        private readonly ICategoryMappingDal _categoryMappingDal;

        public CategoryMappingManager(ICategoryMappingDal categoryMappingDal)
        {
            _categoryMappingDal = categoryMappingDal;
        }

        public CategoryMapping Add(CategoryMapping entity)
        {
            return _categoryMappingDal.Add(entity);
        }

        public CategoryMapping Delete(CategoryMapping entity)
        {
            return _categoryMappingDal.Delete(entity);
        }

        public CategoryMapping Get(Expression<Func<CategoryMapping, bool>> filter)
        {
            return _categoryMappingDal.Get(filter);
        }

        public List<CategoryMapping> GetList(Expression<Func<CategoryMapping, bool>> filter = null)
        {
            return _categoryMappingDal.GetList(filter);
        }

        public CategoryMapping Update(CategoryMapping entity)
        {
            return _categoryMappingDal.Update(entity);
        }
    }
}