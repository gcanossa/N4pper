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

        public ParameterNameRewriter(string name)
        {
            Name = string.IsNullOrEmpty(name) ? "p" : name;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return Expression.Parameter(node.Type, Name);
        }
    }
}
