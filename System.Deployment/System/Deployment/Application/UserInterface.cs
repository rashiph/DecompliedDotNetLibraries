namespace System.Deployment.Application
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    internal class UserInterface : IDisposable
    {
        private ApplicationContext _appctx;
        private ManualResetEvent _appctxExitThreadFinished;
        private bool _disposed;
        private SplashInfo _splashInfo;
        private ManualResetEvent _uiConstructed;
        private UserInterfaceForm _uiForm;
        private ManualResetEvent _uiReady;
        private Thread _uiThread;

        public UserInterface() : this(true)
        {
        }

        public UserInterface(bool wait)
        {
            this._appctxExitThreadFinished = new ManualResetEvent(false);
            this._uiConstructed = new ManualResetEvent(false);
            this._uiReady = new ManualResetEvent(false);
            this._splashInfo = new SplashInfo();
            this._splashInfo.initializedAsWait = wait;
            this._uiThread = new Thread(new ThreadStart(this.UIThread));
            this._uiThread.SetApartmentState(ApartmentState.STA);
            this._uiThread.Name = "UIThread";
            this._uiThread.Start();
        }

        public void Activate()
        {
            this.WaitReady();
            this._uiForm.BeginInvoke(new MethodInvoker(this._uiForm.Activate));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this.WaitReady();
                    this._appctx.ExitThread();
                    this._appctxExitThreadFinished.Set();
                }
                this._disposed = true;
            }
        }

        public static string GetDisplaySite(Uri sourceUri)
        {
            string host = null;
            if (sourceUri.IsUnc)
            {
                try
                {
                    host = Path.GetDirectoryName(sourceUri.LocalPath);
                }
                catch (ArgumentException)
                {
                }
                return host;
            }
            host = sourceUri.Host;
            if (string.IsNullOrEmpty(host))
            {
                try
                {
                    host = Path.GetDirectoryName(sourceUri.LocalPath);
                }
                catch (ArgumentException)
                {
                }
            }
            return host;
        }

        public void Hide()
        {
            this.WaitReady();
            this._uiForm.BeginInvoke(new MethodInvoker(this._uiForm.Hide));
        }

        public static bool IsValidHttpUrl(string url)
        {
            bool flag = false;
            if (((url == null) || (url.Length <= 0)) || (!url.StartsWith(Uri.UriSchemeHttp + Uri.SchemeDelimiter, StringComparison.Ordinal) && !url.StartsWith(Uri.UriSchemeHttps + Uri.SchemeDelimiter, StringComparison.Ordinal)))
            {
                return flag;
            }
            return true;
        }

        public static void LaunchUrlInBrowser(string url)
        {
            try
            {
                Process.Start(DefaultBrowserExePath, url);
            }
            catch (Win32Exception)
            {
            }
        }

        public static string LimitDisplayTextLength(string displayText)
        {
            if (displayText.Length > 50)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(displayText, 0, 0x2f);
                builder.Append("...");
                return builder.ToString();
            }
            return displayText;
        }

        public void ShowError(string title, string message, string logFileLocation, string linkUrl, string linkUrlMessage)
        {
            this.WaitReady();
            ManualResetEvent event2 = new ManualResetEvent(false);
            this._uiForm.BeginInvoke(new UserInterfaceForm.ConstructErrorPieceDelegate(this._uiForm.ConstructErrorPiece), new object[] { title, message, logFileLocation, linkUrl, linkUrlMessage, event2 });
            event2.WaitOne();
        }

        public UserInterfaceModalResult ShowMaintenance(UserInterfaceInfo info, MaintenanceInfo maintenanceInfo)
        {
            this.WaitReady();
            ManualResetEvent event2 = new ManualResetEvent(false);
            MaintenancePiece piece = (MaintenancePiece) this._uiForm.Invoke(new UserInterfaceForm.ConstructMaintenancePieceDelegate(this._uiForm.ConstructMaintenancePiece), new object[] { info, maintenanceInfo, event2 });
            event2.WaitOne();
            return piece.ModalResult;
        }

        public void ShowMessage(string message, string caption)
        {
            this.WaitReady();
            this._uiForm.Invoke(new UserInterfaceForm.ShowSimpleMessageBoxDelegate(this._uiForm.ShowSimpleMessageBox), new object[] { message, caption });
        }

        public void ShowPlatform(string platformDetectionErrorMsg, Uri supportUrl)
        {
            this.WaitReady();
            ManualResetEvent event2 = new ManualResetEvent(false);
            this._uiForm.BeginInvoke(new UserInterfaceForm.ConstructPlatformPieceDelegate(this._uiForm.ConstructPlatformPiece), new object[] { platformDetectionErrorMsg, supportUrl, event2 });
            event2.WaitOne();
        }

        public ProgressPiece ShowProgress(UserInterfaceInfo info)
        {
            this.WaitReady();
            return (ProgressPiece) this._uiForm.Invoke(new UserInterfaceForm.ConstructProgressPieceDelegate(this._uiForm.ConstructProgressPiece), new object[] { info });
        }

        public UserInterfaceModalResult ShowUpdate(UserInterfaceInfo info)
        {
            this.WaitReady();
            ManualResetEvent event2 = new ManualResetEvent(false);
            UpdatePiece piece = (UpdatePiece) this._uiForm.Invoke(new UserInterfaceForm.ConstructUpdatePieceDelegate(this._uiForm.ConstructUpdatePiece), new object[] { info, event2 });
            event2.WaitOne();
            return piece.ModalResult;
        }

        public bool SplashCancelled()
        {
            return this._splashInfo.cancelled;
        }

        private void UIThread()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (this._uiForm = new UserInterfaceForm(this._uiReady, this._splashInfo))
            {
                this._uiConstructed.Set();
                this._appctx = new ApplicationContext(this._uiForm);
                Application.Run(this._appctx);
                this._appctxExitThreadFinished.WaitOne();
                Application.ExitThread();
            }
        }

        private void WaitReady()
        {
            this._uiConstructed.WaitOne();
            this._uiReady.WaitOne();
            this._splashInfo.pieceReady.WaitOne();
        }

        private static string DefaultBrowserExePath
        {
            get
            {
                string str = null;
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command");
                if (key == null)
                {
                    return str;
                }
                string str2 = (string) key.GetValue(string.Empty);
                if (str2 == null)
                {
                    return str;
                }
                str2 = str2.Trim();
                if (str2.Length == 0)
                {
                    return str;
                }
                if (str2[0] == '"')
                {
                    int num = str2.IndexOf('"', 1);
                    if (num != -1)
                    {
                        str = str2.Substring(1, num - 1);
                    }
                    return str;
                }
                int index = str2.IndexOf(' ');
                if (index != -1)
                {
                    return str2.Substring(0, index);
                }
                return str2;
            }
        }
    }
}

