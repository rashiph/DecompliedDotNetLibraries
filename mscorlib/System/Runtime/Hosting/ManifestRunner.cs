namespace System.Runtime.Hosting
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    internal sealed class ManifestRunner
    {
        private ApartmentState m_apt;
        private string[] m_args;
        private RuntimeAssembly m_assembly;
        private AppDomain m_domain;
        private string m_path;
        private int m_runResult;

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, Unrestricted=true)]
        internal ManifestRunner(AppDomain domain, ActivationContext activationContext)
        {
            string str;
            string str2;
            this.m_domain = domain;
            CmsUtils.GetEntryPoint(activationContext, out str, out str2);
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NoMain"));
            }
            if (string.IsNullOrEmpty(str2))
            {
                this.m_args = new string[0];
            }
            else
            {
                this.m_args = str2.Split(new char[] { ' ' });
            }
            this.m_apt = ApartmentState.Unknown;
            string applicationDirectory = activationContext.ApplicationDirectory;
            this.m_path = Path.Combine(applicationDirectory, str);
        }

        [SecurityCritical]
        internal int ExecuteAsAssembly()
        {
            if (this.EntryAssembly.EntryPoint.GetCustomAttributes(typeof(STAThreadAttribute), false).Length > 0)
            {
                this.m_apt = ApartmentState.STA;
            }
            if (this.EntryAssembly.EntryPoint.GetCustomAttributes(typeof(MTAThreadAttribute), false).Length > 0)
            {
                if (this.m_apt == ApartmentState.Unknown)
                {
                    this.m_apt = ApartmentState.MTA;
                }
                else
                {
                    this.m_apt = ApartmentState.Unknown;
                }
            }
            return this.Run(true);
        }

        [SecurityCritical]
        private void NewThreadRunner()
        {
            this.m_runResult = this.Run(false);
        }

        [SecurityCritical]
        private int Run(bool checkAptModel)
        {
            if (checkAptModel && (this.m_apt != ApartmentState.Unknown))
            {
                if ((Thread.CurrentThread.GetApartmentState() != ApartmentState.Unknown) && (Thread.CurrentThread.GetApartmentState() != this.m_apt))
                {
                    return this.RunInNewThread();
                }
                Thread.CurrentThread.SetApartmentState(this.m_apt);
            }
            return this.m_domain.nExecuteAssembly(this.EntryAssembly, this.m_args);
        }

        [SecurityCritical]
        private int RunInNewThread()
        {
            Thread thread = new Thread(new ThreadStart(this.NewThreadRunner));
            thread.SetApartmentState(this.m_apt);
            thread.Start();
            thread.Join();
            return this.m_runResult;
        }

        internal RuntimeAssembly EntryAssembly
        {
            [SecurityCritical, SecurityPermission(SecurityAction.Assert, Unrestricted=true), FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
            get
            {
                if (this.m_assembly == null)
                {
                    this.m_assembly = (RuntimeAssembly) Assembly.LoadFrom(this.m_path);
                }
                return this.m_assembly;
            }
        }
    }
}

