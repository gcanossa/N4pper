using N4pper.Orm.Entities;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace N4pper.Orm.Design
{
    internal class ReverseConnectionBuilder<T, D> : IReverseConnectionBuilder<T, D> where T : class where D : class
    {
        protected Action<IEnumerable<string>> Handler { get; set; }
        public ReverseConnectionBuilder(Action<IEnumerable<string>> handler)
        {
            Handler = handler;
        }
        public void Connected(Expression<Func<T, D>> back)
        {
            back = back ?? throw new ArgumentNullException(nameof(back));

            IEnumerable<string> backP = back.ToPropertyNameCollection();
            if (backP.Count() != 1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(back));

            Handler(backP);
        }

        public void ConnectedMany(Expression<Func<T, IEnumerable<D>>> back)
        {
            back = back ?? throw new ArgumentNullException(nameof(back));

            IEnumerable<string> backP = back.ToPropertyNameCollection();
            if (backP.Count() != 1)
                throw new ArgumentException("Only a single navigation property must be specified", nameof(back));

            Handler(backP);
        }
    }
}
