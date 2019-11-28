using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GenericSearch.Core
{
        public static class SearchExtensions
    {
        public static IQueryable<T> ApplySearchCriteria<T>(this IQueryable<T> query, IEnumerable<AbstractSearch> searchCriterias)
        {
            foreach (var criteria in searchCriterias)
            {
                query = criteria.ApplyToQuery(query);
            }

            return query;
        }

        public static ICollection<AbstractSearch> GetDefaultSearchCriteria(this Type type)
        {
            var properties = type.GetProperties()
                .Where(p => p.CanRead && p.CanWrite)
                .OrderBy(p => p.PropertyType.IsCollectionType())
                .ThenBy(p => p.Name);

            var criteria = properties
                .Select(p => CreateSearchCriterion(type, p.PropertyType, p.Name))
                .Where(s => s != null)
                .ToList();

            return criteria;
        }

        public static ICollection<AbstractSearch> AddCustomSearchCriterion<T>(this ICollection<AbstractSearch> criteria, Expression<Func<T, object>> property)
        {
            var fullPropertyPath = GetPropertyPath(property, out var propertyType);

            var criterion = CreateSearchCriterion(typeof(T), propertyType, fullPropertyPath);

            if (criterion != null)
            {
                criteria.Add(criterion);
            }

            return criteria;
        }

        private static AbstractSearch CreateSearchCriterion(Type targetType, Type propertyType, string property)
        {
            AbstractSearch result = null;

            if (propertyType.IsCollectionType())
            {
                propertyType = propertyType.GetGenericArguments().First();
            }

            if (propertyType == typeof(string))
            {
                result = new TextSearch();
            }
            else if (propertyType == typeof(int) || propertyType == typeof(int?))
            {
                result = new NumericSearch();
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                result = new DateSearch();
            }
            else if (propertyType.IsEnum)
            {
                result = new EnumSearch(propertyType);
            }

            if (result != null)
            {
                result.Property = property;
                result.TargetTypeName = targetType.AssemblyQualifiedName;
            }

            return result;
        }

        private static string GetPropertyPath<T>(Expression<Func<T, object>> expression, out Type targetType)
        {
            if (expression.Body is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Arguments.Count != 2)
                {
                    throw new ArgumentException("Please provide a lambda expression like 'n => n.Collection.Select(c => c.PropertyName)'",
                                          nameof(expression));
                }

                if (!(methodCallExpression.Arguments[0] is MemberExpression memberExpression1) ||
                    !(methodCallExpression.Arguments[1] is LambdaExpression lambdaExpression))
                {
                    throw new ArgumentException("Please provide a lambda expression like 'n => n.Collection.Select(c => c.PropertyName)'",
                                          nameof(expression));
                }

                if (!(lambdaExpression.Body is MemberExpression memberExpression2))
                {
                    throw new ArgumentException("Please provide a lambda expression like 'n => n.Collection.Select(c => c.PropertyName)'",
                                          nameof(expression));
                }

                targetType = memberExpression2.Type;

                return $"{GetPropertyPath(memberExpression1)}.{GetPropertyPath(memberExpression2)}";

            }

            var unaryExpression = expression.Body as UnaryExpression;
            MemberExpression memberExpression;

            if (unaryExpression != null)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = expression.Body as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new ArgumentException("Please provide a lambda expression like 'n => n.PropertyName'", nameof(expression));
            }

            targetType = memberExpression.Type;

            return GetPropertyPath(memberExpression);

        }

        private static string GetPropertyPath(MemberExpression memberExpression)
        {
            var property = memberExpression.ToString();
            return property.Substring(property.IndexOf('.') + 1);
        }
    }
}
