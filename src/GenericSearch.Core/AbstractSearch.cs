using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GenericSearch.Core
{
    public abstract class AbstractSearch
    {
        public string Property { get; set; }

        public string TargetTypeName { get; set; }

        public string LabelText
        {
            get
            {
                if (Property == null)
                {
                    return null;
                }

                var arg = Expression.Parameter(Type.GetType(TargetTypeName) ?? throw new InvalidOperationException(), "p");
                var propertyInfo = GetPropertyAccess(arg).Member as PropertyInfo;

                if (propertyInfo != null)
                {
                    var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();

                    if (displayAttribute != null)
                    {
                        return displayAttribute.GetName();
                    }
                }

                return Property.Split('.').Last();
            }
        }

        internal IQueryable<T> ApplyToQuery<T>(IQueryable<T> query)
        {
            var parts = Property.Split('.');

            var parameter = Expression.Parameter(typeof(T), "p");

            var filterExpression = BuildFilterExpressionWithNullChecks(null, parameter, null, parts);

            if (filterExpression == null)
            {
                return query;
            }
            else
            {
                var predicate = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
                return query.Where(predicate);
            }
        }

        protected abstract Expression BuildFilterExpression(Expression property);

        private static Expression Combine(Expression first, Expression second)
        {
            if (first == null)
            {
                return second;
            }

            return Expression.AndAlso(first, second);
        }

        private Expression BuildFilterExpressionWithNullChecks(Expression filterExpression, ParameterExpression parameter, Expression property, IReadOnlyList<string> remainingPropertyParts)
        {
            property = Expression.Property(property ?? parameter, remainingPropertyParts[0]);

            BinaryExpression nullCheckExpression;
            if (remainingPropertyParts.Count == 1)
            {
                if (!property.Type.IsValueType || property.Type.IsNullableType())
                {
                    nullCheckExpression = Expression.NotEqual(property, Expression.Constant(null));
                    filterExpression = Combine(filterExpression, nullCheckExpression);
                }

                if (property.Type.IsNullableType())
                {
                    property = Expression.Property(property, "Value");
                }

                Expression searchExpression = null;
                if (property.Type.IsCollectionType())
                {
                    parameter = Expression.Parameter(property.Type.GetGenericArguments().First());
                    searchExpression =
                        ApplySearchExpressionToCollection(parameter, property, BuildFilterExpression(parameter));
                }
                else
                {
                    searchExpression = BuildFilterExpression(property);
                }

                return searchExpression == null ? null : Combine(filterExpression, searchExpression);
            }

            nullCheckExpression = Expression.NotEqual(property, Expression.Constant(null));
            filterExpression = Combine(filterExpression, nullCheckExpression);

            if (property.Type.IsCollectionType())
            {
                parameter = Expression.Parameter(property.Type.GetGenericArguments().First());
                var searchExpression = BuildFilterExpressionWithNullChecks(null, parameter, null, remainingPropertyParts.Skip(1).ToArray());

                if (searchExpression == null)
                {
                    return null;
                }

                searchExpression = ApplySearchExpressionToCollection(
                                                                     parameter,
                                                                     property,
                                                                     searchExpression);

                return Combine(filterExpression, searchExpression);
            }

            return BuildFilterExpressionWithNullChecks(filterExpression, parameter, property, remainingPropertyParts.Skip(1).ToArray());
        }

        private static Expression ApplySearchExpressionToCollection(ParameterExpression parameter, Expression property, Expression searchExpression)
        {
            if (searchExpression == null)
            {
                return null;
            }

            var asQueryable = typeof(Queryable).GetMethods()
                                               .Where(m => m.Name == nameof(Queryable.AsQueryable))
                                               .Single(m => m.IsGenericMethod)
                                               .MakeGenericMethod(property.Type.GetGenericArguments());

            var anyMethod = typeof(Queryable).GetMethods()
                                             .Where(m => m.Name == nameof(Queryable.Any))
                                             .Single(m => m.GetParameters().Length == 2)
                                             .MakeGenericMethod(property.Type.GetGenericArguments());

            return Expression.Call(null, anyMethod, Expression.Call(null, asQueryable, property), Expression.Lambda(searchExpression, parameter));
        }

        private MemberExpression GetPropertyAccess(Expression expression)
        {
            var parts = Property.Split('.');

            var property = Expression.Property(expression, parts[0]);

            for (var i = 1; i < parts.Length; i++)
            {
                if (property.Type.IsCollectionType())
                {
                    property = Expression.Property(Expression.Parameter(property.Type.GetGenericArguments().First()), parts[i]);
                }
                else
                {
                    property = Expression.Property(property, parts[i]);
                }
            }

            return property;
        }
    }
}