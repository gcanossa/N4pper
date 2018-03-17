using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using q = System.Linq.Queryable;

namespace N4pper.Queryable
{
    internal class AggregatedQueryTranslator : QueryPartTranslatorBase
    {
        internal AggregatedQueryTranslator(Type typeResult) : base(typeResult)
        {
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                if (m.Method.Name == nameof(q.Count))
                {
                    _builder.Append(" WITH *");
                    if (m.Arguments.Count == 2)
                    {
                        _builder.Append(" WHERE (");
                        LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                        Visit(lambda.Body);
                        _builder.Append(")");
                    }
                    _builder.Append(" RETURN count(*)");
                }
                else if (m.Method.Name == nameof(q.Average))
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    _builder.Append(" RETURN avg(");

                    if (lambda.Body.NodeType == ExpressionType.MemberAccess && ObjectExtensions.IsNumeric(((MemberExpression)lambda.Body).Type))
                        Visit(lambda.Body);
                    else
                        throw new ArgumentException("Lambada must be a numeric memmeber accessor", nameof(m));

                    _builder.Append("), ");
                    Visit(lambda.Parameters[0]);
                }
                else if (m.Method.Name == "Sum")
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    _builder.Append(" RETURN sum(");

                    if (lambda.Body.NodeType == ExpressionType.MemberAccess && ObjectExtensions.IsNumeric(((MemberExpression)lambda.Body).Type))
                        Visit(lambda.Body);
                    else
                        throw new ArgumentException("Lambada must be a numeric memmeber accessor", nameof(m));

                    _builder.Append("), ");
                    Visit(lambda.Parameters[0]);
                }
                else if (m.Method.Name == "Min")
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    _builder.Append(" RETURN min(");

                    if (lambda.Body.NodeType == ExpressionType.MemberAccess)
                        Visit(lambda.Body);
                    else
                        throw new ArgumentException("Lambada must be a numeric memmeber accessor", nameof(m));

                    _builder.Append("), ");
                    Visit(lambda.Parameters[0]);
                }
                else if (m.Method.Name == "Max")
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    _builder.Append(" RETURN max(");

                    if (lambda.Body.NodeType == ExpressionType.MemberAccess)
                        Visit(lambda.Body);
                    else
                        throw new ArgumentException("Lambada must be a numeric memmeber accessor", nameof(m));

                    _builder.Append("), ");
                    Visit(lambda.Parameters[0]);
                }
                else
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");

                return m;
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }
    }
}
