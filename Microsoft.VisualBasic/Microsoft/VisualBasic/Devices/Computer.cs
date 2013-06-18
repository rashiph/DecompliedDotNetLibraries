namespace Microsoft.VisualBasic.Devices
{
    using Microsoft.VisualBasic.MyServices;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class Computer : ServerComputer
    {
        private Microsoft.VisualBasic.Devices.Audio m_Audio;
        private static ClipboardProxy m_Clipboard;
        private static Microsoft.VisualBasic.Devices.Keyboard m_KeyboardInstance;
        private static Microsoft.VisualBasic.Devices.Mouse m_Mouse;
        private Microsoft.VisualBasic.Devices.Ports m_Ports;

        public Microsoft.VisualBasic.Devices.Audio Audio
        {
            get
            {
                if (this.m_Audio == null)
                {
                    this.m_Audio = new Microsoft.VisualBasic.Devices.Audio();
                }
                return this.m_Audio;
            }
        }

        public ClipboardProxy Clipboard
        {
            get
            {
                if (m_Clipboard == null)
                {
                    m_Clipboard = new ClipboardProxy();
                }
                return m_Clipboard;
            }
        }

        public Microsoft.VisualBasic.Devices.Keyboard Keyboard
        {
            get
            {
                if (m_KeyboardInstance == null)
                {
                    m_KeyboardInstance = new Microsoft.VisualBasic.Devices.Keyboard();
                }
                return m_KeyboardInstance;
            }
        }

        public Microsoft.VisualBasic.Devices.Mouse Mouse
        {
            get
            {
                if (m_Mouse == null)
                {
                    m_Mouse = new Microsoft.VisualBasic.Devices.Mouse();
                }
                return m_Mouse;
            }
        }

        public Microsoft.VisualBasic.Devices.Ports Ports
        {
            get
            {
                if (this.m_Ports == null)
                {
                    this.m_Ports = new Microsoft.VisualBasic.Devices.Ports();
                }
                return this.m_Ports;
            }
        }

        public System.Windows.Forms.Screen Screen
        {
            get
            {
                return System.Windows.Forms.Screen.PrimaryScreen;
            }
        }
    }
}

