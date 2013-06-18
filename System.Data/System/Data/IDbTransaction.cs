namespace System.Data
{
    using System;

    public interface IDbTransaction : IDisposable
    {
        void Commit();
        void Rollback();

        IDbConnection Connection { get; }

        System.Data.IsolationLevel IsolationLevel { get; }
    }
}

