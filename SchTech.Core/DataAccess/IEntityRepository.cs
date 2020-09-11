﻿using SchTech.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SchTech.Core.DataAccess
{
    public interface IEntityRepository<T>
        where T : class, IEntity, new()
    {
        List<T> GetList(Expression<Func<T, bool>> filter = null);
        T Get(Expression<Func<T, bool>> filter);
        T Add(T entity);
        T Update(T entity);
        T Delete(T entity);
    }
}