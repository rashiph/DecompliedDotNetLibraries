namespace System.Configuration.Internal
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class InternalConfigRoot : IInternalConfigRoot
    {
        private System.Configuration.Configuration _CurrentConfiguration;
        private ReaderWriterLock _hierarchyLock;
        private IInternalConfigHost _host;
        private bool _isDesignTime;
        private BaseConfigurationRecord _rootConfigRecord;

        public event InternalConfigEventHandler ConfigChanged;

        public event InternalConfigEventHandler ConfigRemoved;

        internal InternalConfigRoot()
        {
        }

        internal InternalConfigRoot(System.Configuration.Configuration currentConfiguration)
        {
            this._CurrentConfiguration = currentConfiguration;
        }

        private void AcquireHierarchyLockForRead()
        {
            if (this._hierarchyLock.IsReaderLockHeld)
            {
                throw ExceptionUtil.UnexpectedError("System.Configuration.Internal.InternalConfigRoot::AcquireHierarchyLockForRead - reader lock already held by this thread");
            }
            if (this._hierarchyLock.IsWriterLockHeld)
            {
                throw ExceptionUtil.UnexpectedError("System.Configuration.Internal.InternalConfigRoot::AcquireHierarchyLockForRead - writer lock already held by this thread");
            }
            this._hierarchyLock.AcquireReaderLock(-1);
        }

        private void AcquireHierarchyLockForWrite()
        {
            if (this._hierarchyLock.IsReaderLockHeld)
            {
                throw ExceptionUtil.UnexpectedError("System.Configuration.Internal.InternalConfigRoot::AcquireHierarchyLockForWrite - reader lock already held by this thread");
            }
            if (this._hierarchyLock.IsWriterLockHeld)
            {
                throw ExceptionUtil.UnexpectedError("System.Configuration.Internal.InternalConfigRoot::AcquireHierarchyLockForWrite - writer lock already held by this thread");
            }
            this._hierarchyLock.AcquireWriterLock(-1);
        }

        public void ClearResult(BaseConfigurationRecord configRecord, string configKey, bool forceEvaluation)
        {
            string[] parts = ConfigPathUtility.GetParts(configRecord.ConfigPath);
            try
            {
                int num;
                BaseConfigurationRecord record;
                this.AcquireHierarchyLockForRead();
                this.hlFindConfigRecord(parts, out num, out record);
                if ((num == parts.Length) && object.ReferenceEquals(configRecord, record))
                {
                    record.hlClearResultRecursive(configKey, forceEvaluation);
                }
            }
            finally
            {
                this.ReleaseHierarchyLockForRead();
            }
        }

        internal void FireConfigChanged(string configPath)
        {
            this.OnConfigChanged(new InternalConfigEventArgs(configPath));
        }

        public IInternalConfigRecord GetConfigRecord(string configPath)
        {
            IInternalConfigRecord record4;
            if (!ConfigPathUtility.IsValid(configPath))
            {
                throw ExceptionUtil.ParameterInvalid("configPath");
            }
            string[] parts = ConfigPathUtility.GetParts(configPath);
            try
            {
                int num;
                BaseConfigurationRecord record;
                this.AcquireHierarchyLockForRead();
                this.hlFindConfigRecord(parts, out num, out record);
                if ((num == parts.Length) || !record.hlNeedsChildFor(parts[num]))
                {
                    return record;
                }
            }
            finally
            {
                this.ReleaseHierarchyLockForRead();
            }
            try
            {
                int num2;
                BaseConfigurationRecord record2;
                this.AcquireHierarchyLockForWrite();
                this.hlFindConfigRecord(parts, out num2, out record2);
                if (num2 == parts.Length)
                {
                    return record2;
                }
                string parentConfigPath = string.Join("/", parts, 0, num2);
                while ((num2 < parts.Length) && record2.hlNeedsChildFor(parts[num2]))
                {
                    BaseConfigurationRecord record3;
                    string childConfigPath = parts[num2];
                    parentConfigPath = ConfigPathUtility.Combine(parentConfigPath, childConfigPath);
                    if (this._isDesignTime)
                    {
                        record3 = MgmtConfigurationRecord.Create(this, record2, parentConfigPath, null);
                    }
                    else
                    {
                        record3 = (BaseConfigurationRecord) RuntimeConfigurationRecord.Create(this, record2, parentConfigPath);
                    }
                    record2.hlAddChild(childConfigPath, record3);
                    num2++;
                    record2 = record3;
                }
                record4 = record2;
            }
            finally
            {
                this.ReleaseHierarchyLockForWrite();
            }
            return record4;
        }

        public object GetSection(string section, string configPath)
        {
            BaseConfigurationRecord uniqueConfigRecord = (BaseConfigurationRecord) this.GetUniqueConfigRecord(configPath);
            return uniqueConfigRecord.GetSection(section);
        }

        public string GetUniqueConfigPath(string configPath)
        {
            IInternalConfigRecord uniqueConfigRecord = this.GetUniqueConfigRecord(configPath);
            if (uniqueConfigRecord == null)
            {
                return null;
            }
            return uniqueConfigRecord.ConfigPath;
        }

        public IInternalConfigRecord GetUniqueConfigRecord(string configPath)
        {
            BaseConfigurationRecord configRecord = (BaseConfigurationRecord) this.GetConfigRecord(configPath);
            while (configRecord.IsEmpty)
            {
                BaseConfigurationRecord parent = configRecord.Parent;
                if (parent.IsRootConfig)
                {
                    return configRecord;
                }
                configRecord = parent;
            }
            return configRecord;
        }

        private void hlFindConfigRecord(string[] parts, out int nextIndex, out BaseConfigurationRecord currentRecord)
        {
            currentRecord = this._rootConfigRecord;
            nextIndex = 0;
            while (nextIndex < parts.Length)
            {
                BaseConfigurationRecord record = currentRecord.hlGetChild(parts[nextIndex]);
                if (record == null)
                {
                    return;
                }
                currentRecord = record;
                nextIndex++;
            }
        }

        private void OnConfigChanged(InternalConfigEventArgs e)
        {
            InternalConfigEventHandler configChanged = this.ConfigChanged;
            if (configChanged != null)
            {
                configChanged(this, e);
            }
        }

        private void OnConfigRemoved(InternalConfigEventArgs e)
        {
            InternalConfigEventHandler configRemoved = this.ConfigRemoved;
            if (configRemoved != null)
            {
                configRemoved(this, e);
            }
        }

        private void ReleaseHierarchyLockForRead()
        {
            if (this._hierarchyLock.IsReaderLockHeld)
            {
                this._hierarchyLock.ReleaseReaderLock();
            }
        }

        private void ReleaseHierarchyLockForWrite()
        {
            if (this._hierarchyLock.IsWriterLockHeld)
            {
                this._hierarchyLock.ReleaseWriterLock();
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void RemoveConfig(string configPath)
        {
            this.RemoveConfigImpl(configPath, null);
        }

        private void RemoveConfigImpl(string configPath, BaseConfigurationRecord configRecord)
        {
            BaseConfigurationRecord record;
            if (!ConfigPathUtility.IsValid(configPath))
            {
                throw ExceptionUtil.ParameterInvalid("configPath");
            }
            string[] parts = ConfigPathUtility.GetParts(configPath);
            try
            {
                int num;
                this.AcquireHierarchyLockForWrite();
                this.hlFindConfigRecord(parts, out num, out record);
                if ((num != parts.Length) || ((configRecord != null) && !object.ReferenceEquals(configRecord, record)))
                {
                    return;
                }
                record.Parent.hlRemoveChild(parts[parts.Length - 1]);
            }
            finally
            {
                this.ReleaseHierarchyLockForWrite();
            }
            this.OnConfigRemoved(new InternalConfigEventArgs(configPath));
            record.CloseRecursive();
        }

        public void RemoveConfigRecord(BaseConfigurationRecord configRecord)
        {
            this.RemoveConfigImpl(configRecord.ConfigPath, configRecord);
        }

        void IInternalConfigRoot.Init(IInternalConfigHost host, bool isDesignTime)
        {
            this._host = host;
            this._isDesignTime = isDesignTime;
            this._hierarchyLock = new ReaderWriterLock();
            if (this._isDesignTime)
            {
                this._rootConfigRecord = MgmtConfigurationRecord.Create(this, null, string.Empty, null);
            }
            else
            {
                this._rootConfigRecord = (BaseConfigurationRecord) RuntimeConfigurationRecord.Create(this, null, string.Empty);
            }
        }

        internal System.Configuration.Configuration CurrentConfiguration
        {
            get
            {
                return this._CurrentConfiguration;
            }
        }

        internal IInternalConfigHost Host
        {
            get
            {
                return this._host;
            }
        }

        internal BaseConfigurationRecord RootConfigRecord
        {
            get
            {
                return this._rootConfigRecord;
            }
        }

        bool IInternalConfigRoot.IsDesignTime
        {
            get
            {
                return this._isDesignTime;
            }
        }
    }
}

