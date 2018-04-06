using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace N4pper.Decorators
{
    public abstract class DriverDecoratorBase : IDriver
    {
        public IDriver Driver { get; protected set; }

        public DriverDecoratorBase(IDriver driver)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        #region IDriver

        public virtual Uri Uri => Driver.Uri;

        public virtual void Close()
        {
            Driver.Close();
        }

        public virtual Task CloseAsync()
        {
            return Driver.CloseAsync();
        }

        public virtual void Dispose()
        {
            Driver.Dispose();
        }

        public virtual ISession Session()
        {
            return Driver.Session();
        }

        public virtual ISession Session(AccessMode defaultMode)
        {
            return Driver.Session(defaultMode);
        }

        public virtual ISession Session(string bookmark)
        {
            return Driver.Session(bookmark);
        }

        public virtual ISession Session(AccessMode defaultMode, string bookmark)
        {
            return Driver.Session(defaultMode, bookmark);
        }

        public virtual ISession Session(AccessMode defaultMode, IEnumerable<string> bookmarks)
        {
            return Driver.Session(defaultMode, bookmarks);
        }

        public virtual ISession Session(IEnumerable<string> bookmarks)
        {
            return Driver.Session(bookmarks);
        }

        #endregion
    }
}
