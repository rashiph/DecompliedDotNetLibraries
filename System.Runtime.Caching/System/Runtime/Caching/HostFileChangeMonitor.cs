namespace System.Runtime.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime.Caching.Hosting;
    using System.Runtime.Caching.Resources;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    public sealed class HostFileChangeMonitor : FileChangeMonitor
    {
        private object _fcnState;
        private ReadOnlyCollection<string> _filePaths;
        private DateTimeOffset _lastModified;
        private string _uniqueId;
        private const int MAX_CHAR_COUNT_OF_LONG_CONVERTED_TO_HEXADECIMAL_STRING = 0x10;
        private static IFileChangeNotificationSystem s_fcn;

        private HostFileChangeMonitor()
        {
        }

        public HostFileChangeMonitor(IList<string> filePaths)
        {
            if (filePaths == null)
            {
                throw new ArgumentNullException("filePaths");
            }
            if (filePaths.Count == 0)
            {
                throw new ArgumentException(RH.Format(R.Empty_collection, new object[] { "filePaths" }));
            }
            foreach (string str in filePaths)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new ArgumentException(RH.Format(R.Collection_contains_null_or_empty_string, new object[] { "filePaths" }));
                }
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, str).Demand();
            }
            List<string> list = new List<string>(filePaths.Count);
            list.AddRange(filePaths);
            this._filePaths = list.AsReadOnly();
            InitFCN();
            this.InitDisposableMembers();
        }

        protected override void Dispose(bool disposing)
        {
            if ((disposing && (s_fcn != null)) && ((this._filePaths != null) && (this._fcnState != null)))
            {
                if (this._filePaths.Count > 1)
                {
                    Hashtable hashtable = this._fcnState as Hashtable;
                    foreach (string str in this._filePaths)
                    {
                        if (str != null)
                        {
                            object state = hashtable[str];
                            if (state != null)
                            {
                                s_fcn.StopMonitoring(str, state);
                            }
                        }
                    }
                }
                else
                {
                    string filePath = this._filePaths[0];
                    if ((filePath != null) && (this._fcnState != null))
                    {
                        s_fcn.StopMonitoring(filePath, this._fcnState);
                    }
                }
            }
        }

        private void InitDisposableMembers()
        {
            bool flag = true;
            try
            {
                string str = null;
                if (this._filePaths.Count == 1)
                {
                    DateTimeOffset offset;
                    long num;
                    string filePath = this._filePaths[0];
                    s_fcn.StartMonitoring(filePath, new OnChangedCallback(this.OnChanged), out this._fcnState, out offset, out num);
                    str = filePath + offset.UtcDateTime.Ticks.ToString("X", CultureInfo.InvariantCulture) + num.ToString("X", CultureInfo.InvariantCulture);
                    this._lastModified = offset;
                }
                else
                {
                    int capacity = 0;
                    foreach (string str3 in this._filePaths)
                    {
                        capacity += str3.Length + 0x20;
                    }
                    Hashtable hashtable = new Hashtable(this._filePaths.Count);
                    this._fcnState = hashtable;
                    StringBuilder builder = new StringBuilder(capacity);
                    foreach (string str4 in this._filePaths)
                    {
                        if (!hashtable.Contains(str4))
                        {
                            DateTimeOffset offset2;
                            long num3;
                            object obj2;
                            s_fcn.StartMonitoring(str4, new OnChangedCallback(this.OnChanged), out obj2, out offset2, out num3);
                            hashtable[str4] = obj2;
                            builder.Append(str4);
                            builder.Append(offset2.UtcDateTime.Ticks.ToString("X", CultureInfo.InvariantCulture));
                            builder.Append(num3.ToString("X", CultureInfo.InvariantCulture));
                            if (offset2 > this._lastModified)
                            {
                                this._lastModified = offset2;
                            }
                        }
                    }
                    str = builder.ToString();
                }
                this._uniqueId = str;
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

        [SecuritySafeCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private static void InitFCN()
        {
            if (s_fcn == null)
            {
                IFileChangeNotificationSystem service = null;
                IServiceProvider host = ObjectCache.Host;
                if (host != null)
                {
                    service = host.GetService(typeof(IFileChangeNotificationSystem)) as IFileChangeNotificationSystem;
                }
                if (service == null)
                {
                    service = new FileChangeNotificationSystem();
                }
                Interlocked.CompareExchange<IFileChangeNotificationSystem>(ref s_fcn, service, null);
            }
        }

        public override ReadOnlyCollection<string> FilePaths
        {
            get
            {
                return this._filePaths;
            }
        }

        public override DateTimeOffset LastModified
        {
            get
            {
                return this._lastModified;
            }
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

