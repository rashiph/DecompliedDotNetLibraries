namespace System.Runtime.Caching
{
    using System;
    using System.Data.SqlClient;
    using System.Globalization;

    public sealed class SqlChangeMonitor : ChangeMonitor
    {
        private SqlDependency _sqlDependency;
        private string _uniqueId;

        private SqlChangeMonitor()
        {
        }

        public SqlChangeMonitor(SqlDependency dependency)
        {
            if (dependency == null)
            {
                throw new ArgumentNullException("dependency");
            }
            bool flag = true;
            try
            {
                this._sqlDependency = dependency;
                this._sqlDependency.OnChange += new OnChangeEventHandler(this.OnDependencyChanged);
                this._uniqueId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                flag = false;
            }
            finally
            {
                base.InitializationComplete();
                if (flag)
                {
                    base.Dispose();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
        }

        private void OnDependencyChanged(object sender, SqlNotificationEventArgs e)
        {
            base.OnChanged(null);
        }

        public override string UniqueId
        {
            get
            {
                return this._uniqueId;
            }
        }
    }
}

