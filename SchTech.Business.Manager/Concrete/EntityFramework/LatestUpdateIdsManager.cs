using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SchTech.Business.Manager.Abstract.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.Business.Manager.Concrete.EntityFramework
{
    public sealed class LatestUpdateIdsManager : ILatestUpdateIdsService
    {
        private readonly ILatestUpdateIdsDal _latestUpdateIdsDal;

        public LatestUpdateIdsManager(ILatestUpdateIdsDal latestUpdateIdsDal)
        {
            _latestUpdateIdsDal = latestUpdateIdsDal;
        }

        public List<LatestUpdateIds> GetList(Expression<Func<LatestUpdateIds, bool>> filter = null)
        {
            return _latestUpdateIdsDal.GetList(filter);
        }

        public LatestUpdateIds Get(Expression<Func<LatestUpdateIds, bool>> filter)
        {
            return _latestUpdateIdsDal.Get(filter);
        }

        public LatestUpdateIds Add(LatestUpdateIds entity)
        {
            return _latestUpdateIdsDal.Add(entity);
        }

        public LatestUpdateIds Update(LatestUpdateIds entity)
        {
            return _latestUpdateIdsDal.Update(entity);
        }

        public LatestUpdateIds Delete(LatestUpdateIds entity)
        {
            return _latestUpdateIdsDal.Delete(entity);
        }

        public void InUpdateOperation(bool inOperation)
        {
            _latestUpdateIdsDal.InUpdateOperation(inOperation);
        }
    }
}
