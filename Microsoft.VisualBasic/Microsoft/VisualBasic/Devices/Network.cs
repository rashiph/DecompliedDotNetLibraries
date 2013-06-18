namespace Microsoft.VisualBasic.Devices
{
    using Microsoft.VisualBasic.CompilerServices;
    using Microsoft.VisualBasic.FileIO;
    using Microsoft.VisualBasic.MyServices.Internal;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class Network
    {
        private const int BUFFER_SIZE = 0x20;
        private const string DEFAULT_PASSWORD = "";
        private const int DEFAULT_PING_TIMEOUT = 0x3e8;
        private const int DEFAULT_TIMEOUT = 0x186a0;
        private const string DEFAULT_USERNAME = "";
        private bool m_Connected;
        private SendOrPostCallback m_NetworkAvailabilityChangedCallback;
        private ArrayList m_NetworkAvailabilityEventHandlers;
        private byte[] m_PingBuffer;
        private SynchronizationContext m_SynchronizationContext;
        private object m_SyncObject = new object();

        public event NetworkAvailableEventHandler NetworkAvailabilityChanged
        {
            add
            {
                try
                {
                    this.m_Connected = this.IsAvailable;
                }
                catch (SecurityException)
                {
                    return;
                }
                catch (PlatformNotSupportedException)
                {
                    return;
                }
                object syncObject = this.m_SyncObject;
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
                lock (syncObject)
                {
                    if (this.m_NetworkAvailabilityEventHandlers == null)
                    {
                        this.m_NetworkAvailabilityEventHandlers = new ArrayList();
                    }
                    this.m_NetworkAvailabilityEventHandlers.Add(handler);
                    if (this.m_NetworkAvailabilityEventHandlers.Count == 1)
                    {
                        this.m_NetworkAvailabilityChangedCallback = new SendOrPostCallback(this.NetworkAvailabilityChangedHandler);
                        if (AsyncOperationManager.SynchronizationContext != null)
                        {
                            this.m_SynchronizationContext = AsyncOperationManager.SynchronizationContext;
                            try
                            {
                                NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(this.OS_NetworkAvailabilityChangedListener);
                            }
                            catch (PlatformNotSupportedException)
                            {
                            }
                            catch (NetworkInformationException)
                            {
                            }
                        }
                    }
                }
            }
            remove
            {
                if ((this.m_NetworkAvailabilityEventHandlers != null) && (this.m_NetworkAvailabilityEventHandlers.Count > 0))
                {
                    this.m_NetworkAvailabilityEventHandlers.Remove(handler);
                    if (this.m_NetworkAvailabilityEventHandlers.Count == 0)
                    {
                        NetworkChange.NetworkAddressChanged -= new NetworkAddressChangedEventHandler(this.OS_NetworkAvailabilityChangedListener);
                        this.DisconnectListener();
                    }
                }
            }
            raise
            {
                if (this.m_NetworkAvailabilityEventHandlers != null)
                {
                    IEnumerator enumerator;
                    try
                    {
                        enumerator = this.m_NetworkAvailabilityEventHandlers.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            NetworkAvailableEventHandler current = (NetworkAvailableEventHandler) enumerator.Current;
                            if (current != null)
                            {
                                current(sender, e);
                            }
                        }
                    }
                    finally
                    {
                        if (enumerator is IDisposable)
                        {
                            (enumerator as IDisposable).Dispose();
                        }
                    }
                }
            }
        }

        internal void DisconnectListener()
        {
            NetworkChange.NetworkAddressChanged -= new NetworkAddressChangedEventHandler(this.OS_NetworkAvailabilityChangedListener);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void DownloadFile(string address, string destinationFileName)
        {
            this.DownloadFile(address, destinationFileName, "", "", false, 0x186a0, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void DownloadFile(Uri address, string destinationFileName)
        {
            this.DownloadFile(address, destinationFileName, "", "", false, 0x186a0, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void DownloadFile(string address, string destinationFileName, string userName, string password)
        {
            this.DownloadFile(address, destinationFileName, userName, password, false, 0x186a0, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void DownloadFile(Uri address, string destinationFileName, string userName, string password)
        {
            this.DownloadFile(address, destinationFileName, userName, password, false, 0x186a0, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void DownloadFile(Uri address, string destinationFileName, ICredentials networkCredentials, bool showUI, int connectionTimeout, bool overwrite)
        {
            this.DownloadFile(address, destinationFileName, networkCredentials, showUI, connectionTimeout, overwrite, UICancelOption.ThrowException);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void DownloadFile(string address, string destinationFileName, string userName, string password, bool showUI, int connectionTimeout, bool overwrite)
        {
            this.DownloadFile(address, destinationFileName, userName, password, showUI, connectionTimeout, overwrite, UICancelOption.ThrowException);
        }

        [SecuritySafeCritical]
        public void DownloadFile(Uri address, string destinationFileName, ICredentials networkCredentials, bool showUI, int connectionTimeout, bool overwrite, UICancelOption onUserCancel)
        {
            if (connectionTimeout <= 0)
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName("connectionTimeOut", "Network_BadConnectionTimeout", new string[0]);
            }
            if (address == null)
            {
                throw ExceptionUtils.GetArgumentNullException("address");
            }
            using (WebClientExtended extended = new WebClientExtended())
            {
                extended.Timeout = connectionTimeout;
                extended.UseNonPassiveFtp = showUI;
                string path = FileSystem.NormalizeFilePath(destinationFileName, "destinationFileName");
                if (Directory.Exists(path))
                {
                    throw ExceptionUtils.GetInvalidOperationException("Network_DownloadNeedsFilename", new string[0]);
                }
                if (System.IO.File.Exists(path) & !overwrite)
                {
                    throw new IOException(Utils.GetResourceString("IO_FileExists_Path", new string[] { destinationFileName }));
                }
                if (networkCredentials != null)
                {
                    extended.Credentials = networkCredentials;
                }
                ProgressDialog dialog = null;
                if (showUI && Environment.UserInteractive)
                {
                    new UIPermission(UIPermissionWindow.SafeSubWindows).Demand();
                    dialog = new ProgressDialog {
                        Text = Utils.GetResourceString("ProgressDialogDownloadingTitle", new string[] { address.AbsolutePath }),
                        LabelText = Utils.GetResourceString("ProgressDialogDownloadingLabel", new string[] { address.AbsolutePath, path })
                    };
                }
                string directoryName = Path.GetDirectoryName(path);
                if (directoryName == "")
                {
                    throw ExceptionUtils.GetInvalidOperationException("Network_DownloadNeedsFilename", new string[0]);
                }
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                new WebClientCopy(extended, dialog).DownloadFile(address, path);
                if ((showUI && Environment.UserInteractive) && ((onUserCancel == UICancelOption.ThrowException) & dialog.UserCanceledTheDialog))
                {
                    throw new OperationCanceledException();
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void DownloadFile(Uri address, string destinationFileName, string userName, string password, bool showUI, int connectionTimeout, bool overwrite)
        {
            this.DownloadFile(address, destinationFileName, userName, password, showUI, connectionTimeout, overwrite, UICancelOption.ThrowException);
        }

        public void DownloadFile(string address, string destinationFileName, string userName, string password, bool showUI, int connectionTimeout, bool overwrite, UICancelOption onUserCancel)
        {
            if (string.IsNullOrEmpty(address) || (address.Trim() == ""))
            {
                throw ExceptionUtils.GetArgumentNullException("address");
            }
            Uri uri = this.GetUri(address.Trim());
            ICredentials networkCredentials = this.GetNetworkCredentials(userName, password);
            this.DownloadFile(uri, destinationFileName, networkCredentials, showUI, connectionTimeout, overwrite, onUserCancel);
        }

        public void DownloadFile(Uri address, string destinationFileName, string userName, string password, bool showUI, int connectionTimeout, bool overwrite, UICancelOption onUserCancel)
        {
            ICredentials networkCredentials = this.GetNetworkCredentials(userName, password);
            this.DownloadFile(address, destinationFileName, networkCredentials, showUI, connectionTimeout, overwrite, onUserCancel);
        }

        private ICredentials GetNetworkCredentials(string userName, string password)
        {
            if (userName == null)
            {
                userName = "";
            }
            if (password == null)
            {
                password = "";
            }
            if ((userName == "") & (password == ""))
            {
                return null;
            }
            return new NetworkCredential(userName, password);
        }

        private Uri GetUri(string address)
        {
            Uri uri;
            try
            {
                uri = new Uri(address);
            }
            catch (UriFormatException)
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName("address", "Network_InvalidUriString", new string[] { address });
            }
            return uri;
        }

        private void NetworkAvailabilityChangedHandler(object state)
        {
            bool isAvailable = this.IsAvailable;
            if (this.m_Connected != isAvailable)
            {
                this.m_Connected = isAvailable;
                this.raise_NetworkAvailabilityChanged(this, new NetworkAvailableEventArgs(isAvailable));
            }
        }

        private void OS_NetworkAvailabilityChangedListener(object sender, EventArgs e)
        {
            object syncObject = this.m_SyncObject;
            ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
            lock (syncObject)
            {
                this.m_SynchronizationContext.Post(this.m_NetworkAvailabilityChangedCallback, null);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public bool Ping(string hostNameOrAddress)
        {
            return this.Ping(hostNameOrAddress, 0x3e8);
        }

        public bool Ping(Uri address)
        {
            if (address == null)
            {
                throw ExceptionUtils.GetArgumentNullException("address");
            }
            return this.Ping(address.Host, 0x3e8);
        }

        public bool Ping(string hostNameOrAddress, int timeout)
        {
            if (!this.IsAvailable)
            {
                throw ExceptionUtils.GetInvalidOperationException("Network_NetworkNotAvailable", new string[0]);
            }
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            return (ping.Send(hostNameOrAddress, timeout, this.PingBuffer).Status == IPStatus.Success);
        }

        public bool Ping(Uri address, int timeout)
        {
            if (address == null)
            {
                throw ExceptionUtils.GetArgumentNullException("address");
            }
            return this.Ping(address.Host, timeout);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void UploadFile(string sourceFileName, string address)
        {
            this.UploadFile(sourceFileName, address, "", "", false, 0x186a0);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void UploadFile(string sourceFileName, Uri address)
        {
            this.UploadFile(sourceFileName, address, "", "", false, 0x186a0);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void UploadFile(string sourceFileName, string address, string userName, string password)
        {
            this.UploadFile(sourceFileName, address, userName, password, false, 0x186a0);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void UploadFile(string sourceFileName, Uri address, string userName, string password)
        {
            this.UploadFile(sourceFileName, address, userName, password, false, 0x186a0);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void UploadFile(string sourceFileName, Uri address, ICredentials networkCredentials, bool showUI, int connectionTimeout)
        {
            this.UploadFile(sourceFileName, address, networkCredentials, showUI, connectionTimeout, UICancelOption.ThrowException);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void UploadFile(string sourceFileName, string address, string userName, string password, bool showUI, int connectionTimeout)
        {
            this.UploadFile(sourceFileName, address, userName, password, showUI, connectionTimeout, UICancelOption.ThrowException);
        }

        public void UploadFile(string sourceFileName, Uri address, ICredentials networkCredentials, bool showUI, int connectionTimeout, UICancelOption onUserCancel)
        {
            sourceFileName = FileSystem.NormalizeFilePath(sourceFileName, "sourceFileName");
            if (!System.IO.File.Exists(sourceFileName))
            {
                throw new FileNotFoundException(Utils.GetResourceString("IO_FileNotFound_Path", new string[] { sourceFileName }));
            }
            if (connectionTimeout <= 0)
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName("connectionTimeout", "Network_BadConnectionTimeout", new string[0]);
            }
            if (address == null)
            {
                throw ExceptionUtils.GetArgumentNullException("address");
            }
            using (WebClientExtended extended = new WebClientExtended())
            {
                extended.Timeout = connectionTimeout;
                if (networkCredentials != null)
                {
                    extended.Credentials = networkCredentials;
                }
                ProgressDialog dialog = null;
                if (showUI && Environment.UserInteractive)
                {
                    dialog = new ProgressDialog {
                        Text = Utils.GetResourceString("ProgressDialogUploadingTitle", new string[] { sourceFileName }),
                        LabelText = Utils.GetResourceString("ProgressDialogUploadingLabel", new string[] { sourceFileName, address.AbsolutePath })
                    };
                }
                new WebClientCopy(extended, dialog).UploadFile(sourceFileName, address);
                if ((showUI && Environment.UserInteractive) && ((onUserCancel == UICancelOption.ThrowException) & dialog.UserCanceledTheDialog))
                {
                    throw new OperationCanceledException();
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void UploadFile(string sourceFileName, Uri address, string userName, string password, bool showUI, int connectionTimeout)
        {
            this.UploadFile(sourceFileName, address, userName, password, showUI, connectionTimeout, UICancelOption.ThrowException);
        }

        public void UploadFile(string sourceFileName, string address, string userName, string password, bool showUI, int connectionTimeout, UICancelOption onUserCancel)
        {
            if (string.IsNullOrEmpty(address) || (address.Trim() == ""))
            {
                throw ExceptionUtils.GetArgumentNullException("address");
            }
            Uri uri = this.GetUri(address.Trim());
            if (Path.GetFileName(uri.AbsolutePath) == "")
            {
                throw ExceptionUtils.GetInvalidOperationException("Network_UploadAddressNeedsFilename", new string[0]);
            }
            this.UploadFile(sourceFileName, uri, userName, password, showUI, connectionTimeout, onUserCancel);
        }

        public void UploadFile(string sourceFileName, Uri address, string userName, string password, bool showUI, int connectionTimeout, UICancelOption onUserCancel)
        {
            ICredentials networkCredentials = this.GetNetworkCredentials(userName, password);
            this.UploadFile(sourceFileName, address, networkCredentials, showUI, connectionTimeout, onUserCancel);
        }

        public bool IsAvailable
        {
            get
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
        }

        private byte[] PingBuffer
        {
            get
            {
                if (this.m_PingBuffer == null)
                {
                    this.m_PingBuffer = new byte[0x20];
                    int index = 0;
                    do
                    {
                        this.m_PingBuffer[index] = Convert.ToByte(0x61 + (index % 0x17), CultureInfo.InvariantCulture);
                        index++;
                    }
                    while (index <= 0x1f);
                }
                return this.m_PingBuffer;
            }
        }
    }
}

