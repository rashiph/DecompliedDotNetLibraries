namespace System.Web
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Web.Util;

    internal sealed class FileMonitor
    {
        private byte[] _dacl;
        private bool _exists;
        private FileAttributesData _fad;
        private string _fileNameLong;
        private string _fileNameShort;
        private FileAction _lastAction;
        private HybridDictionary _targets;
        private DateTime _utcLastCompletion;
        internal readonly HybridDictionary Aliases;
        internal readonly System.Web.DirectoryMonitor DirectoryMonitor;

        internal FileMonitor(System.Web.DirectoryMonitor dirMon, string fileNameLong, string fileNameShort, bool exists, FileAttributesData fad, byte[] dacl)
        {
            this.DirectoryMonitor = dirMon;
            this._fileNameLong = fileNameLong;
            this._fileNameShort = fileNameShort;
            this._exists = exists;
            this._fad = fad;
            this._dacl = dacl;
            this._targets = new HybridDictionary();
            this.Aliases = new HybridDictionary(true);
        }

        internal void AddTarget(FileChangeEventHandler callback, string alias, bool newAlias)
        {
            FileMonitorTarget target = (FileMonitorTarget) this._targets[callback.Target];
            if (target != null)
            {
                target.AddRef();
            }
            else
            {
                this._targets.Add(callback.Target, new FileMonitorTarget(callback, alias));
            }
            if (newAlias)
            {
                this.Aliases[alias] = alias;
            }
        }

        internal void MakeExist(FindFileData ffd, byte[] dacl)
        {
            this._fileNameLong = ffd.FileNameLong;
            this._fileNameShort = ffd.FileNameShort;
            this._fad = ffd.FileAttributesData;
            this._dacl = dacl;
            this._exists = true;
        }

        internal void MakeExtinct()
        {
            this._fad = null;
            this._dacl = null;
            this._exists = false;
        }

        internal void RemoveFileNameShort()
        {
            this._fileNameShort = null;
        }

        internal int RemoveTarget(object callbackTarget)
        {
            FileMonitorTarget target = (FileMonitorTarget) this._targets[callbackTarget];
            if ((target != null) && (target.Release() == 0))
            {
                this._targets.Remove(callbackTarget);
            }
            return this._targets.Count;
        }

        internal void ResetCachedAttributes()
        {
            this._fad = null;
            this._dacl = null;
        }

        internal void UpdateCachedAttributes()
        {
            string path = Path.Combine(this.DirectoryMonitor.Directory, this.FileNameLong);
            FileAttributesData.GetFileAttributes(path, out this._fad);
            this._dacl = FileSecurity.GetDacl(path);
        }

        internal FileAttributesData Attributes
        {
            get
            {
                return this._fad;
            }
        }

        internal byte[] Dacl
        {
            get
            {
                return this._dacl;
            }
        }

        internal bool Exists
        {
            get
            {
                return this._exists;
            }
        }

        internal string FileNameLong
        {
            get
            {
                return this._fileNameLong;
            }
        }

        internal string FileNameShort
        {
            get
            {
                return this._fileNameShort;
            }
        }

        internal bool IsDirectory
        {
            get
            {
                return (this.FileNameLong == null);
            }
        }

        internal FileAction LastAction
        {
            get
            {
                return this._lastAction;
            }
            set
            {
                this._lastAction = value;
            }
        }

        internal ICollection Targets
        {
            get
            {
                return this._targets.Values;
            }
        }

        internal DateTime UtcLastCompletion
        {
            get
            {
                return this._utcLastCompletion;
            }
            set
            {
                this._utcLastCompletion = value;
            }
        }
    }
}

