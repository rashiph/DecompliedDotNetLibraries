namespace Microsoft.VisualBasic.Devices
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class Mouse
    {
        public bool ButtonsSwapped
        {
            get
            {
                if (!SystemInformation.MousePresent)
                {
                    throw ExceptionUtils.GetInvalidOperationException("Mouse_NoMouseIsPresent", new string[0]);
                }
                return SystemInformation.MouseButtonsSwapped;
            }
        }

        public bool WheelExists
        {
            get
            {
                if (!SystemInformation.MousePresent)
                {
                    throw ExceptionUtils.GetInvalidOperationException("Mouse_NoMouseIsPresent", new string[0]);
                }
                return SystemInformation.MouseWheelPresent;
            }
        }

        public int WheelScrollLines
        {
            get
            {
                if (!this.WheelExists)
                {
                    throw ExceptionUtils.GetInvalidOperationException("Mouse_NoWheelIsPresent", new string[0]);
                }
                return SystemInformation.MouseWheelScrollLines;
            }
        }
    }
}

