namespace System.Data.Common
{
    using System;
    using System.Data;

    public abstract class DbTransaction : MarshalByRefObject, IDbTransaction, IDisposable
    {
        protected DbTransaction()
        {
        }

        public abstract void Commit();
        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public abstract void Rollback();

        public System.Data.Common.DbConnection Connection
        {
            get
            {
                return this.DbConnection;
            }
        }

        protected abstract System.Data.Common.DbConnection DbConnection { get; }

        public abstract System.Data.IsolationLevel IsolationLevel { get; }

        IDbConnection IDbTransaction.Connection
        {
            get
            {
                return this.DbConnection;
            }
        }
    }
}

