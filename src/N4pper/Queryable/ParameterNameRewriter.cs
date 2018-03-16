using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    internal class ParameterNameRewriter : ExpressionVisitor
    {
        protected string Name { get; set; }
        protected Type TypeResult { get; set; }

        public ParameterNameRewriter(string name, Type typeResult)
        {
            Name = string.IsNullOrEmpty(name) ? "p" : name;
            TypeResult = typeResult ?? throw new ArgumentNullException(nameof(typeResult));
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return Expression.Parameter(node.Type, Name);
        }
    }
}
