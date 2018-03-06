using System;
using System.Collections.Generic;
using System.Text;
using AsIKnow.Graph;
using Neo4j.Driver.V1;

namespace N4pper
{
    public class GraphManagedTransaction : TransactionDecorator, IGraphManagedStatementRunner
    {
        public GraphManager Manager { get; protected set; }
        public GraphManagedTransaction(ITransaction transaction, GraphManager manager) : base(transaction)
        {
            Manager = manager;
        }
    }
}
