using System;
using System.Linq.Expressions;

namespace GenericSearch.Core
{
    public class EnumSearch : AbstractSearch
    {
        public EnumSearch()
        {
        }

        public EnumSearch(Type enumType)
        {
            EnumTypeName = enumType.AssemblyQualifiedName;
        }

        public string SearchTerm { get; set; }

        public Type EnumType => Type.GetType(EnumTypeName);

        public string EnumTypeName { get; set; }

        protected override Expression BuildFilterExpression(Expression property)
        {
            if (SearchTerm == null)
            {
                return null;
            }

            var enumValue = Enum.Parse(EnumType, SearchTerm);

            Expression searchExpression = Expression.Equal(property, Expression.Constant(enumValue));

            return searchExpression;
        }
    }
}