using System;
using System.Linq.Expressions;

namespace GenericSearch.Core
{
    public class TextSearch : AbstractSearch
    {
        public string SearchTerm { get; set; }

        public TextComparators Comparator { get; set; }

        protected override Expression BuildFilterExpression(Expression property)
        {
            if (SearchTerm == null)
            {
                return null;
            }

            var searchExpression = Expression.Call(property, typeof(string).GetMethod(Comparator.ToString(), new[] { typeof(string) }) ?? throw new InvalidOperationException(),Expression.Constant(SearchTerm));

            return searchExpression;
        }
    }
}