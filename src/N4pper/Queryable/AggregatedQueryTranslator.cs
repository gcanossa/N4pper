using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Queryable
{
    internal class AggregatedQueryTranslator : QueryPartTranslatorBase
    {
        private bool _distinct;
        internal AggregatedQueryTranslator(bool distinct)
        {
            _distinct = distinct;
        }
        protected override void ValidateChain()
        {
            if (_callChain.Count() > 1)
                throw new ArgumentOutOfRangeException("expression", "Only one terminal call is allowed");
        }
        protected override bool MatchCall(MethodCallExpression m)
        {
            return true;
        }
        private bool IsNumeric(Type type)
        {
            return type.IsPrimitive && type != typeof(char) && type != typeof(bool);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                if (m.Method.Name == "Count" && m.Arguments.Count == 1)
                {
                    _builder.Append(" RETURN ");
                    if(_distinct)
                        _builder.Append("DISTINCT ");
                    _builder.Append("count(*) ");
                }
                else if (m.Method.Name == "Average")
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    _builder.Append(" RETURN ");
                    if (_distinct)
                        _builder.Append("DISTINCT ");
                    _builder.Append("avg(");

                    if (lambda.Body.NodeType == ExpressionType.MemberAccess && IsNumeric(((MemberExpression)lambda.Body).Type))
                        Visit(lambda.Body);
                    else
                        throw new ArgumentException("Lambada must be a numeric memmeber accessor", nameof(m));

                    _builder.Append(") ");
                }
                else if (m.Method.Name == "Sum")
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    _builder.Append(" RETURN ");
                    if (_distinct)
                        _builder.Append("DISTINCT ");
                    _builder.Append("sum(");

                    if (lambda.Body.NodeType == ExpressionType.MemberAccess && IsNumeric(((MemberExpression)lambda.Body).Type))
                        Visit(lambda.Body);
                    else
                        throw new ArgumentException("Lambada must be a numeric memmeber accessor", nameof(m));

                    _builder.Append(") ");
                }
                else if (m.Method.Name == "Min")
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    _builder.Append(" RETURN ");
                    if (_distinct)
                        _builder.Append("DISTINCT ");
                    _builder.Append("min(");

                    if (lambda.Body.NodeType == ExpressionType.MemberAccess)
                        Visit(lambda.Body);
                    else
                        throw new ArgumentException("Lambada must be a numeric memmeber accessor", nameof(m));

                    _builder.Append(") ");
                }
                else if (m.Method.Name == "Max")
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    _builder.Append(" RETURN ");
                    if (_distinct)
                        _builder.Append("DISTINCT ");
                    _builder.Append("max(");

                    if (lambda.Body.NodeType == ExpressionType.MemberAccess)
                        Visit(lambda.Body);
                    else
                        throw new ArgumentException("Lambada must be a numeric memmeber accessor", nameof(m));

                    _builder.Append(") ");
                }
                else
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");

                return m;
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }
    }
}
