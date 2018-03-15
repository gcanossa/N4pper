using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    internal class SelectQueryTranslator : QueryPartTranslatorBase
    {
        internal SelectQueryTranslator()
        {
        }

        protected override void ValidateChain()
        {
            if (_callChain.Count(p => p.Method.Name == "Select") > 1)
                throw new ArgumentOutOfRangeException("expression", "Only one call to 'Select' is allowed");
            if (_callChain.Count(p => p.Method.Name == "Distinct") > 1)
                throw new ArgumentOutOfRangeException("expression", "Only one call to 'Distinct' is allowed");

            _builder.Append(" RETURN ");

            if (_callChain.Count(p => p.Method.Name == "Distinct") == 1)
                _builder.Append("DISTINCT ");

            if (_callChain.Count(p => p.Method.Name == "Select") == 0)
                _builder.Append("* ");
        }

        protected override bool MatchCall(MethodCallExpression expression)
        {
            string[] names = new[] { "Select", "Distinct" };
            return names.Contains(expression.Method.Name);
        }

        private bool HandleVisitBody(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression tmp = lambda.Body as MemberExpression;

                if (_typeResult.GetProperty(tmp.Member.Name) != null)
                {
                    Visit(lambda.Body);

                    return true;
                }
            }
            else if (lambda.Body.NodeType == ExpressionType.New)
            {
                NewExpression tmp = lambda.Body as NewExpression;

                if (tmp.Type.GetProperties().All(p => _typeResult.GetProperty(p.Name) != null))
                {
                    Visit(lambda.Body);

                    return true;
                }
            }

            return false;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                if (m.Method.Name == "Select")
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    if(!HandleVisitBody(lambda))
                        throw new ArgumentOutOfRangeException(nameof(m), $"The Select method can only be used to narrow the type '{_typeResult.FullName}'");

                }
                else if(m.Method.Name=="Distinct")
                { }
                else
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");

                return m;
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }
    }
}
