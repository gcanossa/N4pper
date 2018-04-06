using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    internal class CustomDisposable : IDisposable
    {
        private Action _dispose;
        public CustomDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose?.Invoke();
        }
    }
}
