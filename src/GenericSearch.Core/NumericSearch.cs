using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace GenericSearch.Core
{
    public class NumericSearch : AbstractSearch
    {
        public int? SearchTerm { get; set; }

        public NumericComparators Comparator { get; set; }

        protected override Expression BuildFilterExpression(Expression property)
        {
            if (!SearchTerm.HasValue)
            {
                return null;
            }

            var searchExpression = GetFilterExpression(property);

            return searchExpression;
        }

        private Expression GetFilterExpression([NotNull] Expression property)
        {
            switch (Comparator)
            {
                case NumericComparators.Less:
                    return Expression.LessThan(property, Expression.Constant(SearchTerm.Value));
                case NumericComparators.LessOrEqual:
                    return Expression.LessThanOrEqual(property, Expression.Constant(SearchTerm.Value));
                case NumericComparators.Equal:
                    return Expression.Equal(property, Expression.Constant(SearchTerm.Value));
                case NumericComparators.GreaterOrEqual:
                    return Expression.GreaterThanOrEqual(property, Expression.Constant(SearchTerm.Value));
                case NumericComparators.Greater:
                    return Expression.GreaterThan(property, Expression.Constant(SearchTerm.Value));
                default:
                    throw new InvalidOperationException("Comparator not supported.");
            }
        }
    }
}