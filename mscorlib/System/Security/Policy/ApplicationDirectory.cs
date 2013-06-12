namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class ApplicationDirectory : EvidenceBase
    {
        private URLString m_appDirectory;

        private ApplicationDirectory(URLString appDirectory)
        {
            this.m_appDirectory = appDirectory;
        }

        public ApplicationDirectory(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.m_appDirectory = new URLString(name);
        }

        public override EvidenceBase Clone()
        {
            return new ApplicationDirectory(this.m_appDirectory);
        }

        public object Copy()
        {
            return this.Clone();
        }

        public override bool Equals(object o)
        {
            ApplicationDirectory directory = o as ApplicationDirectory;
            if (directory == null)
            {
                return false;
            }
            return this.m_appDirectory.Equals(directory.m_appDirectory);
        }

        public override int GetHashCode()
        {
            return this.m_appDirectory.GetHashCode();
        }

        public override string ToString()
        {
            return this.ToXml().ToString();
        }

        internal SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("System.Security.Policy.ApplicationDirectory");
            element.AddAttribute("version", "1");
            if (this.m_appDirectory != null)
            {
                element.AddChild(new SecurityElement("Directory", this.m_appDirectory.ToString()));
            }
            return element;
        }

        public string Directory
        {
            get
            {
                return this.m_appDirectory.ToString();
            }
        }
    }
}

