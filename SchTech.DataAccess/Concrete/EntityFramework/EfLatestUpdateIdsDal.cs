using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchTech.Core.DataAccess.EntityFramework;
using SchTech.DataAccess.Abstract;
using SchTech.DataAccess.Concrete.EntityFramework.Contexts;
using SchTech.Entities.ConcreteTypes;

namespace SchTech.DataAccess.Concrete.EntityFramework
{
    public class EfLatestUpdateIdsDal : EfEntityRepositoryBase<LatestUpdateIds, ADI_EnrichmentContext>, ILatestUpdateIdsDal
    {
        public void InUpdateOperation(bool inOperation)
        {
            using (var context = new ADI_EnrichmentContext())
            {
                var row = context.LatestUpdateIds.FirstOrDefault();
                if (row == null)
                    return;
                row.InOperation = inOperation;
                Update(row);
            }
        }
    }
}
