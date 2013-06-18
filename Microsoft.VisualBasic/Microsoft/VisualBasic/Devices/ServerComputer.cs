namespace Microsoft.VisualBasic.Devices
{
    using Microsoft.VisualBasic.MyServices;
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class ServerComputer
    {
        private static Microsoft.VisualBasic.Devices.Clock m_Clock;
        private ComputerInfo m_ComputerInfo;
        private FileSystemProxy m_FileIO;
        private Microsoft.VisualBasic.Devices.Network m_Network;
        private RegistryProxy m_RegistryInstance;

        public Microsoft.VisualBasic.Devices.Clock Clock
        {
            get
            {
                if (m_Clock == null)
                {
                    m_Clock = new Microsoft.VisualBasic.Devices.Clock();
                }
                return m_Clock;
            }
        }

        public FileSystemProxy FileSystem
        {
            get
            {
                if (this.m_FileIO == null)
                {
                    this.m_FileIO = new FileSystemProxy();
                }
                return this.m_FileIO;
            }
        }

        public ComputerInfo Info
        {
            get
            {
                if (this.m_ComputerInfo == null)
                {
                    this.m_ComputerInfo = new ComputerInfo();
                }
                return this.m_ComputerInfo;
            }
        }

        public string Name
        {
            get
            {
                return Environment.MachineName;
            }
        }

        public Microsoft.VisualBasic.Devices.Network Network
        {
            get
            {
                if (this.m_Network == null)
                {
                    this.m_Network = new Microsoft.VisualBasic.Devices.Network();
                }
                return this.m_Network;
            }
        }

        public RegistryProxy Registry
        {
            get
            {
                if (this.m_RegistryInstance == null)
                {
                    this.m_RegistryInstance = new RegistryProxy();
                }
                return this.m_RegistryInstance;
            }
        }
    }
}

