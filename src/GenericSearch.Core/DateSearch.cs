using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace GenericSearch.Core
{
    public class DateSearch : AbstractSearch
    {
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? SearchTerm { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? SearchTerm2 { get; set; }

        public DateComparators Comparator { get; set; }

        protected override Expression BuildFilterExpression(Expression property)
        {
            Expression searchExpression1 = null;
            Expression searchExpression2 = null;

            if (SearchTerm.HasValue)
            {
                searchExpression1 = GetFilterExpression(property);
            }

            if (Comparator == DateComparators.InRange && SearchTerm2.HasValue)
            {
                searchExpression2 = Expression.LessThanOrEqual(property, Expression.Constant(SearchTerm2.Value));
            }

            if (searchExpression1 == null && searchExpression2 == null)
            {
                return null;
            }

            if (searchExpression1 != null && searchExpression2 != null)
            {
                var combinedExpression = Expression.AndAlso(searchExpression1, searchExpression2);
                return combinedExpression;
            }

            if (searchExpression1 != null)
            {
                return searchExpression1;
            }

            return searchExpression2;
        }

        private Expression GetFilterExpression([NotNull] Expression property)
        {
            switch (Comparator)
            {
                case DateComparators.Less:
                    return Expression.LessThan(property, Expression.Constant(SearchTerm.Value));
                case DateComparators.LessOrEqual:
                    return Expression.LessThanOrEqual(property, Expression.Constant(SearchTerm.Value));
                case DateComparators.Equal:
                    return Expression.Equal(property, Expression.Constant(SearchTerm.Value));
                case DateComparators.GreaterOrEqual:
                case DateComparators.InRange:
                    return Expression.GreaterThanOrEqual(property, Expression.Constant(SearchTerm.Value));
                case DateComparators.Greater:
                    return Expression.GreaterThan(property, Expression.Constant(SearchTerm.Value));
                default:
                    throw new InvalidOperationException("Comparator not supported.");
            }
        }
    }
}