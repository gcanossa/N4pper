using System;
using System.Collections.Generic;
using System.Text;
using AsIKnow.Graph;
using Neo4j.Driver.V1;

namespace N4pper
{
    public class GraphManagedStatementRunner : StatementRunnerDecorator, IGraphManagedStatementRunner
    {
        public GraphManager Manager { get; protected set; }
        public GraphManagedStatementRunner(IStatementRunner runner, GraphManager manager) : base(runner)
        {
            Manager = manager;
        }
    }
}
