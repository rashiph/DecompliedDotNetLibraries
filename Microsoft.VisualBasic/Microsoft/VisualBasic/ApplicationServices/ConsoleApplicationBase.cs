namespace Microsoft.VisualBasic.ApplicationServices
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Deployment.Application;
    using System.Runtime;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class ConsoleApplicationBase : ApplicationBase
    {
        private ReadOnlyCollection<string> m_CommandLineArgs;

        public ReadOnlyCollection<string> CommandLineArgs
        {
            get
            {
                if (this.m_CommandLineArgs == null)
                {
                    string[] commandLineArgs = Environment.GetCommandLineArgs();
                    if (commandLineArgs.GetLength(0) >= 2)
                    {
                        string[] destinationArray = new string[(commandLineArgs.GetLength(0) - 2) + 1];
                        Array.Copy(commandLineArgs, 1, destinationArray, 0, commandLineArgs.GetLength(0) - 1);
                        this.m_CommandLineArgs = new ReadOnlyCollection<string>(destinationArray);
                    }
                    else
                    {
                        this.m_CommandLineArgs = new ReadOnlyCollection<string>(new string[0]);
                    }
                }
                return this.m_CommandLineArgs;
            }
        }

        public ApplicationDeployment Deployment
        {
            get
            {
                return ApplicationDeployment.CurrentDeployment;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected ReadOnlyCollection<string> InternalCommandLine
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_CommandLineArgs = value;
            }
        }

        public bool IsNetworkDeployed
        {
            get
            {
                return ApplicationDeployment.IsNetworkDeployed;
            }
        }
    }
}

