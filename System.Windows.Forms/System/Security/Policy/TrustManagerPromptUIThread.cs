namespace System.Security.Policy
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Windows.Forms;

    internal class TrustManagerPromptUIThread
    {
        private string m_appName;
        private X509Certificate2 m_certificate;
        private string m_defaultBrowserExePath;
        private string m_deploymentUrl;
        private TrustManagerPromptOptions m_options;
        private string m_publisherName;
        private DialogResult m_ret = DialogResult.No;
        private string m_supportUrl;

        public TrustManagerPromptUIThread(string appName, string defaultBrowserExePath, string supportUrl, string deploymentUrl, string publisherName, X509Certificate2 certificate, TrustManagerPromptOptions options)
        {
            this.m_appName = appName;
            this.m_defaultBrowserExePath = defaultBrowserExePath;
            this.m_supportUrl = supportUrl;
            this.m_deploymentUrl = deploymentUrl;
            this.m_publisherName = publisherName;
            this.m_certificate = certificate;
            this.m_options = options;
        }

        public DialogResult ShowDialog()
        {
            Thread thread = new Thread(new ThreadStart(this.ShowDialogWork));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return this.m_ret;
        }

        private void ShowDialogWork()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using (TrustManagerPromptUI tui = new TrustManagerPromptUI(this.m_appName, this.m_defaultBrowserExePath, this.m_supportUrl, this.m_deploymentUrl, this.m_publisherName, this.m_certificate, this.m_options))
                {
                    this.m_ret = tui.ShowDialog();
                }
            }
            catch
            {
            }
        }
    }
}

