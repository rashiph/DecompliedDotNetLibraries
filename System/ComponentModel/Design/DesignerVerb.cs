namespace System.ComponentModel.Design
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;

    [ComVisible(true), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class DesignerVerb : MenuCommand
    {
        public DesignerVerb(string text, EventHandler handler) : base(handler, StandardCommands.VerbFirst)
        {
            this.Properties["Text"] = (text == null) ? null : Regex.Replace(text, @"\(\&.\)", "");
        }

        public DesignerVerb(string text, EventHandler handler, CommandID startCommandID) : base(handler, startCommandID)
        {
            this.Properties["Text"] = (text == null) ? null : Regex.Replace(text, @"\(\&.\)", "");
        }

        public override string ToString()
        {
            return (this.Text + " : " + base.ToString());
        }

        public string Description
        {
            get
            {
                object obj2 = this.Properties["Description"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.Properties["Description"] = value;
            }
        }

        public string Text
        {
            get
            {
                object obj2 = this.Properties["Text"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
        }
    }
}

