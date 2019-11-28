﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GenericSearch.Paging
{
    /// <summary>
    /// Extensions for paging and sorting.
    /// </summary>
    public static class PagingExtensions
    {
        /// <summary>
        /// The collection types that should be avoided for the sort column.
        /// </summary>
        private static readonly List<Type> Collections = new List<Type>() { typeof(IEnumerable<>), typeof(IEnumerable) };

        /// <summary>
        /// Gets the paged result.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="paging">The paging.</param>
        /// <returns>The paged result.</returns>
        public static PagedResult<T> GetPagedResult<T>(this IQueryable<T> query, Paging<T> paging)
        {
            int count = query.Count();

            return new PagedResult<T>(query.SortAndPage(paging).ToArray(), count, paging);
        }

        /// <summary>
        /// Gets the paged result.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="paging">The paging.</param>
        /// <returns>The paged result.</returns>
        public static async Task<PagedResult<T>> GetPagedResultAsync<T>(this IQueryable<T> query, Paging<T> paging)
        {
            int count = await query.CountAsync();

            return new PagedResult<T>(await query.SortAndPage(paging).ToListAsync(), count, paging);
        }

        /// <summary>
        /// Extends a query with paging and sorting.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="paging">The <see cref="Paging"/>.</param>
        /// <returns>The extended query.</returns>
        public static IQueryable<T> SortAndPage<T>(this IQueryable<T> query, Paging<T> paging)
        {
            if (paging == null)
            {
                return query;
            }

            // If no sort column is provided use a property of the type, a sort column is required to use the 'Skip' method together with SQL-Server
            if (string.IsNullOrEmpty(paging.SortColumn))
            {
                paging.SortColumn = typeof(T).GetProperties()
                                             .First(p => p.PropertyType == typeof(string)
                                                      || !p.PropertyType.GetInterfaces()
                                                           .Any(i => Collections.Any(c => i == c)))
                                             .Name;
            }

            // Sorting required
            var parameter = Expression.Parameter(typeof(T), "p");

            var command = paging.SortDirection == SortDirection.Descending ? "OrderByDescending" : "OrderBy";

            // If sort column is a nested property like 'CreatedBy.FirstName'
            var parts = paging.SortColumn.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            PropertyInfo property = typeof(T).GetProperty(parts[0]);
            MemberExpression member = Expression.MakeMemberAccess(parameter, property);
            for (int i = 1; i < parts.Length; i++)
            {
                property = property.PropertyType.GetProperty(parts[i]);
                member = Expression.MakeMemberAccess(member, property);
            }

            var orderByExpression = Expression.Lambda(member, parameter);

            Expression resultExpression = Expression.Call(
                typeof(Queryable),
                command,
                new Type[] { typeof(T), property.PropertyType },
                query.Expression,
                Expression.Quote(orderByExpression));

            query = query.Provider.CreateQuery<T>(resultExpression);

            return query.Skip(paging.Skip).Take(paging.Top);
        }
    }
}
