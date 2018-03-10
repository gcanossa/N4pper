using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper.Orm
{
    public abstract class StatementBuilder
    {
        //TODO: fai
        
        #region nested types
        
        public class AddOrUpdateNodeStatementBuilder : StatementBuilder
        {
            private const string Template = "MERGE #1# WITH #2# SET....";

            public override string Build()
            {
                throw new NotImplementedException();
            }
        }
        public class DeleteStatementNodeBuilder : StatementBuilder
        {
            private const string Template = "";

            public override string Build()
            {
                throw new NotImplementedException();
            }
        }
        public class QueryStatementNodeBuilder : StatementBuilder
        {
            private const string Template = "";

            public override string Build()
            {
                throw new NotImplementedException();
            }
        }

        public class AddOrUpdateRelStatementBuilder : StatementBuilder
        {
            private const string Template = "";

            public override string Build()
            {
                throw new NotImplementedException();
            }
        }
        public class DeleteStatementRelBuilder : StatementBuilder
        {
            private const string Template = "";

            public override string Build()
            {
                throw new NotImplementedException();
            }
        }
        public class QueryStatementRelBuilder : StatementBuilder
        {
            private const string Template = "";

            public override string Build()
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public abstract string Build();
    }
}
