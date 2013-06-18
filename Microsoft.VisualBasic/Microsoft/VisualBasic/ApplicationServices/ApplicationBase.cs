namespace Microsoft.VisualBasic.ApplicationServices
{
    using Microsoft.VisualBasic.CompilerServices;
    using Microsoft.VisualBasic.Logging;
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class ApplicationBase
    {
        private AssemblyInfo m_Info;
        private Microsoft.VisualBasic.Logging.Log m_Log;

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public void ChangeCulture(string cultureName)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
        }

        public void ChangeUICulture(string cultureName)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
        }

        public string GetEnvironmentVariable(string name)
        {
            string environmentVariable = Environment.GetEnvironmentVariable(name);
            if (environmentVariable == null)
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName("name", "EnvVarNotFound_Name", new string[] { name });
            }
            return environmentVariable;
        }

        public CultureInfo Culture
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture;
            }
        }

        public AssemblyInfo Info
        {
            [MethodImpl(MethodImplOptions.NoInlining), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
            get
            {
                if (this.m_Info == null)
                {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly == null)
                    {
                        entryAssembly = Assembly.GetCallingAssembly();
                    }
                    this.m_Info = new AssemblyInfo(entryAssembly);
                }
                return this.m_Info;
            }
        }

        public Microsoft.VisualBasic.Logging.Log Log
        {
            get
            {
                if (this.m_Log == null)
                {
                    this.m_Log = new Microsoft.VisualBasic.Logging.Log();
                }
                return this.m_Log;
            }
        }

        public CultureInfo UICulture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
        }
    }
}

