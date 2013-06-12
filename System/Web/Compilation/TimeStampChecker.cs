namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.Remoting.Messaging;
    using System.Web.Hosting;

    internal class TimeStampChecker
    {
        private Hashtable _timeStamps = new Hashtable(StringComparer.OrdinalIgnoreCase);
        internal const string CallContextSlotName = "TSC";

        internal static void AddFile(string virtualPath, string path)
        {
            Current.AddFileInternal(virtualPath, path);
        }

        private void AddFileInternal(string virtualPath, string path)
        {
            DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);
            if (this._timeStamps.Contains(virtualPath))
            {
                DateTime time2 = (DateTime) this._timeStamps[virtualPath];
                if ((time2 != DateTime.MaxValue) && (time2 != lastWriteTimeUtc))
                {
                    this._timeStamps[virtualPath] = DateTime.MaxValue;
                }
            }
            else
            {
                this._timeStamps[virtualPath] = lastWriteTimeUtc;
            }
        }

        internal static bool CheckFilesStillValid(string key, ICollection virtualPaths)
        {
            return ((virtualPaths == null) || Current.CheckFilesStillValidInternal(key, virtualPaths));
        }

        private bool CheckFilesStillValidInternal(string key, ICollection virtualPaths)
        {
            foreach (string str in virtualPaths)
            {
                if (this._timeStamps.Contains(str))
                {
                    DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(HostingEnvironment.MapPath(str));
                    DateTime time2 = (DateTime) this._timeStamps[str];
                    if (lastWriteTimeUtc != time2)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static TimeStampChecker Current
        {
            get
            {
                TimeStampChecker data = (TimeStampChecker) CallContext.GetData("TSC");
                if (data == null)
                {
                    data = new TimeStampChecker();
                    CallContext.SetData("TSC", data);
                }
                return data;
            }
        }
    }
}

