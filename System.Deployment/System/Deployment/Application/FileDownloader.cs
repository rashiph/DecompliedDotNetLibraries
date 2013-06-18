namespace System.Deployment.Application
{
    using Microsoft.Internal.Performance;
    using System;
    using System.Collections;
    using System.Deployment.Application.Manifest;
    using System.Deployment.Internal.Isolation;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    internal abstract class FileDownloader
    {
        protected long _accumulatedBytesTotal;
        protected byte[] _buffer = new byte[0x1000];
        protected System.Deployment.Application.ComponentVerifier _componentVerifier = new System.Deployment.Application.ComponentVerifier();
        protected ArrayList _downloadResults = new ArrayList();
        protected DownloadEventArgs _eventArgs = new DownloadEventArgs();
        protected long _expectedBytesTotal;
        protected bool _fCancelPending;
        protected Queue _fileQueue = new Queue();
        protected DownloadOptions _options = new DownloadOptions();

        public event DownloadCompletedEventHandler DownloadCompleted;

        public event DownloadModifiedEventHandler DownloadModified;

        protected FileDownloader()
        {
        }

        public void AddFile(Uri sourceUri, string targetFilePath)
        {
            this.AddFile(sourceUri, targetFilePath, null, null);
        }

        public void AddFile(Uri sourceUri, string targetFilePath, int maxFileSize)
        {
            this.AddFile(sourceUri, targetFilePath, null, null, maxFileSize);
        }

        public void AddFile(Uri sourceUri, string targetFilePath, object cookie, HashCollection hashCollection)
        {
            this.AddFile(sourceUri, targetFilePath, cookie, hashCollection, -1);
        }

        public void AddFile(Uri sourceUri, string targetFilePath, object cookie, HashCollection hashCollection, int maxFileSize)
        {
            UriHelper.ValidateSupportedScheme(sourceUri);
            DownloadQueueItem item = new DownloadQueueItem {
                _sourceUri = sourceUri,
                _targetPath = targetFilePath,
                _cookie = cookie,
                _hashCollection = hashCollection,
                _maxFileSize = maxFileSize
            };
            lock (this._fileQueue)
            {
                this._fileQueue.Enqueue(item);
                this._eventArgs._filesTotal++;
            }
        }

        private static void AddFilesInHashtable(Hashtable hashtable, AssemblyManifest applicationManifest, string applicationFolder)
        {
            Logger.AddMethodCall("AddFilesInHashtable called.");
            Logger.AddInternalState("applicationFolder=" + applicationFolder);
            string location = null;
            foreach (System.Deployment.Application.Manifest.File file in applicationManifest.Files)
            {
                location = Path.Combine(applicationFolder, file.NameFS);
                try
                {
                    AddSingleFileInHashtable(hashtable, file.HashCollection, location);
                }
                catch (IOException exception)
                {
                    Logger.AddErrorInformation(exception, Resources.GetString("Ex_PatchDependencyFailed"), new object[] { Path.GetFileName(location) });
                    Logger.AddInternalState("Exception thrown : " + exception.GetType().ToString() + ":" + exception.Message);
                }
            }
            foreach (DependentAssembly assembly in applicationManifest.DependentAssemblies)
            {
                if (!assembly.IsPreRequisite)
                {
                    location = Path.Combine(applicationFolder, assembly.Codebase);
                    try
                    {
                        if (AddSingleFileInHashtable(hashtable, assembly.HashCollection, location))
                        {
                            AssemblyManifest manifest = new AssemblyManifest(location);
                            System.Deployment.Application.Manifest.File[] files = manifest.Files;
                            for (int i = 0; i < files.Length; i++)
                            {
                                string str2 = Path.Combine(Path.GetDirectoryName(location), files[i].NameFS);
                                AddSingleFileInHashtable(hashtable, files[i].HashCollection, str2);
                            }
                        }
                    }
                    catch (InvalidDeploymentException exception2)
                    {
                        Logger.AddErrorInformation(exception2, Resources.GetString("Ex_PatchDependencyFailed"), new object[] { Path.GetFileName(location) });
                        Logger.AddInternalState("Exception thrown : " + exception2.GetType().ToString() + ":" + exception2.Message);
                    }
                    catch (IOException exception3)
                    {
                        Logger.AddErrorInformation(exception3, Resources.GetString("Ex_PatchDependencyFailed"), new object[] { Path.GetFileName(location) });
                        Logger.AddInternalState("Exception thrown : " + exception3.GetType().ToString() + ":" + exception3.Message);
                    }
                }
            }
        }

        public void AddNotification(IDownloadNotification notification)
        {
            this.DownloadCompleted += new DownloadCompletedEventHandler(notification.DownloadCompleted);
            this.DownloadModified += new DownloadModifiedEventHandler(notification.DownloadModified);
        }

        private static bool AddSingleFileInHashtable(Hashtable hashtable, HashCollection hashCollection, string location)
        {
            bool flag = false;
            if (System.IO.File.Exists(location))
            {
                using (HashCollection.HashEnumerator enumerator = hashCollection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        string compositString = enumerator.Current.CompositString;
                        if (!hashtable.Contains(compositString))
                        {
                            hashtable.Add(compositString, location);
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }

        public virtual void Cancel()
        {
            this._fCancelPending = true;
        }

        internal void CheckForSizeLimit(ulong bytesDownloaded, bool addToSize)
        {
            if ((this._options != null) && this._options.EnforceSizeLimit)
            {
                ulong num = (this._options.SizeLimit > this._options.Size) ? (this._options.SizeLimit - this._options.Size) : ((ulong) 0L);
                if (bytesDownloaded > num)
                {
                    throw new DeploymentDownloadException(ExceptionTypes.SizeLimitForPartialTrustOnlineAppExceeded, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_OnlineSemiTrustAppSizeLimitExceeded"), new object[] { this._options.SizeLimit }));
                }
                if (addToSize && (bytesDownloaded > 0L))
                {
                    this._options.Size += bytesDownloaded;
                }
            }
        }

        public static FileDownloader Create()
        {
            return new SystemNetDownloader();
        }

        public void Download(SubscriptionState subState)
        {
            try
            {
                this.OnModified();
                if (subState != null)
                {
                    CodeMarker_Singleton.Instance.CodeMarker(CodeMarkerEvent.perfCopyBegin);
                    this.PatchFiles(subState);
                    CodeMarker_Singleton.Instance.CodeMarker(CodeMarkerEvent.perfCopyEnd);
                }
                this.DownloadAllFiles();
            }
            finally
            {
                this.OnCompleted();
            }
        }

        protected abstract void DownloadAllFiles();
        private static bool FileHashVerified(HashCollection hashCollection, string location)
        {
            try
            {
                System.Deployment.Application.ComponentVerifier.VerifyFileHash(location, hashCollection);
            }
            catch (InvalidDeploymentException exception)
            {
                if (exception.SubType != ExceptionTypes.HashValidation)
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        private static FileStream GetPatchSourceStream(string filePath)
        {
            Logger.AddMethodCall("GetPatchSourceStream(" + filePath + ") called.");
            FileStream stream = null;
            try
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (IOException exception)
            {
                Logger.AddErrorInformation(exception, Resources.GetString("Ex_PatchSourceOpenFailed"), new object[] { Path.GetFileName(filePath) });
            }
            catch (UnauthorizedAccessException exception2)
            {
                Logger.AddErrorInformation(exception2, Resources.GetString("Ex_PatchSourceOpenFailed"), new object[] { Path.GetFileName(filePath) });
            }
            return stream;
        }

        private static FileStream GetPatchTargetStream(string filePath)
        {
            return new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        }

        protected void OnCompleted()
        {
            if (this.DownloadCompleted != null)
            {
                this.DownloadCompleted(this, this._eventArgs);
            }
        }

        protected void OnModified()
        {
            if (this.DownloadModified != null)
            {
                this.DownloadModified(this, this._eventArgs);
            }
        }

        protected void OnModifiedWithThrottle(ref int lastTick)
        {
            int tickCount = Environment.TickCount;
            int num2 = tickCount - lastTick;
            if (num2 < 0)
            {
                num2 += 0x7fffffff;
            }
            if (num2 >= 100)
            {
                this.OnModified();
                lastTick = tickCount;
            }
        }

        private void PatchFiles(SubscriptionState subState)
        {
            if (!subState.IsInstalled)
            {
                Logger.AddInternalState("Subscription is not installed. No patching.");
            }
            else
            {
                System.Deployment.Internal.Isolation.Store.IPathLock @lock = null;
                System.Deployment.Internal.Isolation.Store.IPathLock lock2 = null;
                using (subState.SubscriptionStore.AcquireSubscriptionReaderLock(subState))
                {
                    if (!subState.IsInstalled)
                    {
                        Logger.AddInternalState("Subscription is not installed. No patching.");
                        return;
                    }
                    Hashtable hashtable = new Hashtable();
                    try
                    {
                        @lock = subState.SubscriptionStore.LockApplicationPath(subState.CurrentBind);
                        AddFilesInHashtable(hashtable, subState.CurrentApplicationManifest, @lock.Path);
                        try
                        {
                            if (subState.PreviousBind != null)
                            {
                                lock2 = subState.SubscriptionStore.LockApplicationPath(subState.PreviousBind);
                                AddFilesInHashtable(hashtable, subState.PreviousApplicationManifest, lock2.Path);
                            }
                            Queue queue = new Queue();
                            do
                            {
                                DownloadQueueItem item = null;
                                lock (this._fileQueue)
                                {
                                    if (this._fileQueue.Count > 0)
                                    {
                                        item = (DownloadQueueItem) this._fileQueue.Dequeue();
                                    }
                                }
                                if (item == null)
                                {
                                    break;
                                }
                                if (!this.PatchSingleFile(item, hashtable))
                                {
                                    queue.Enqueue(item);
                                }
                            }
                            while (!this._fCancelPending);
                            lock (this._fileQueue)
                            {
                                while (this._fileQueue.Count > 0)
                                {
                                    queue.Enqueue(this._fileQueue.Dequeue());
                                }
                                this._fileQueue = queue;
                            }
                        }
                        finally
                        {
                            if (lock2 != null)
                            {
                                lock2.Dispose();
                            }
                        }
                    }
                    finally
                    {
                        if (@lock != null)
                        {
                            @lock.Dispose();
                        }
                    }
                }
                if (this._fCancelPending)
                {
                    throw new DownloadCancelledException();
                }
            }
        }

        private bool PatchSingleFile(DownloadQueueItem item, Hashtable dependencyTable)
        {
            if (item._hashCollection == null)
            {
                return false;
            }
            string location = null;
            using (HashCollection.HashEnumerator enumerator = item._hashCollection.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string compositString = enumerator.Current.CompositString;
                    if (dependencyTable.Contains(compositString))
                    {
                        location = (string) dependencyTable[compositString];
                        goto Label_0062;
                    }
                }
            }
        Label_0062:
            if (location == null)
            {
                return false;
            }
            if (this._fCancelPending)
            {
                return false;
            }
            if (!FileHashVerified(item._hashCollection, location))
            {
                Logger.AddInternalState("Hash verify failed for " + location + ", not using it for file patching.");
                return false;
            }
            FileStream patchSourceStream = null;
            FileStream patchTargetStream = null;
            try
            {
                patchSourceStream = GetPatchSourceStream(location);
                if (patchSourceStream == null)
                {
                    return false;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(item._targetPath));
                patchTargetStream = GetPatchTargetStream(item._targetPath);
                if (patchTargetStream == null)
                {
                    return false;
                }
                this._eventArgs._fileSourceUri = item._sourceUri;
                this._eventArgs.FileLocalPath = item._targetPath;
                this._eventArgs.Cookie = null;
                this._eventArgs._fileResponseUri = null;
                this.CheckForSizeLimit((ulong) patchSourceStream.Length, true);
                this._accumulatedBytesTotal += patchSourceStream.Length;
                this.SetBytesTotal();
                this.OnModified();
                int count = 0;
                int tickCount = Environment.TickCount;
                patchTargetStream.SetLength(patchSourceStream.Length);
                patchTargetStream.Position = 0L;
                do
                {
                    if (this._fCancelPending)
                    {
                        return false;
                    }
                    count = patchSourceStream.Read(this._buffer, 0, this._buffer.Length);
                    if (count > 0)
                    {
                        patchTargetStream.Write(this._buffer, 0, count);
                    }
                    this._eventArgs._bytesCompleted += count;
                    this._eventArgs._progress = (int) ((this._eventArgs._bytesCompleted * 100L) / this._eventArgs._bytesTotal);
                    this.OnModifiedWithThrottle(ref tickCount);
                }
                while (count > 0);
            }
            finally
            {
                if (patchSourceStream != null)
                {
                    patchSourceStream.Close();
                }
                if (patchTargetStream != null)
                {
                    patchTargetStream.Close();
                }
            }
            this._eventArgs.Cookie = item._cookie;
            this._eventArgs._filesCompleted++;
            this.OnModified();
            DownloadResult result = new DownloadResult {
                ResponseUri = null
            };
            this._downloadResults.Add(result);
            Logger.AddInternalState(item._targetPath + " is patched from store.");
            return true;
        }

        public void RemoveNotification(IDownloadNotification notification)
        {
            this.DownloadModified -= new DownloadModifiedEventHandler(notification.DownloadModified);
            this.DownloadCompleted -= new DownloadCompletedEventHandler(notification.DownloadCompleted);
        }

        protected void SetBytesTotal()
        {
            if (this._expectedBytesTotal < this._accumulatedBytesTotal)
            {
                this._eventArgs._bytesTotal = this._accumulatedBytesTotal;
            }
            else
            {
                this._eventArgs._bytesTotal = this._expectedBytesTotal;
            }
        }

        public void SetExpectedBytesTotal(long total)
        {
            this._expectedBytesTotal = total;
        }

        public System.Deployment.Application.ComponentVerifier ComponentVerifier
        {
            get
            {
                return this._componentVerifier;
            }
        }

        public DownloadResult[] DownloadResults
        {
            get
            {
                return (DownloadResult[]) this._downloadResults.ToArray(typeof(DownloadResult));
            }
        }

        public DownloadOptions Options
        {
            set
            {
                this._options = value;
            }
        }

        public delegate void DownloadCompletedEventHandler(object sender, DownloadEventArgs e);

        public delegate void DownloadModifiedEventHandler(object sender, DownloadEventArgs e);

        protected class DownloadQueueItem
        {
            public object _cookie;
            public HashCollection _hashCollection;
            public int _maxFileSize;
            public Uri _sourceUri;
            public string _targetPath;
            public const int FileOfAnySize = -1;

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(" _sourceUri = " + this._sourceUri);
                builder.Append(",  _targetPath = " + this._targetPath);
                return builder.ToString();
            }
        }
    }
}

