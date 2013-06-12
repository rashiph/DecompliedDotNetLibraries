namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class RunInstallerAttribute : Attribute
    {
        public static readonly RunInstallerAttribute Default = No;
        public static readonly RunInstallerAttribute No = new RunInstallerAttribute(false);
        private bool runInstaller;
        public static readonly RunInstallerAttribute Yes = new RunInstallerAttribute(true);

        public RunInstallerAttribute(bool runInstaller)
        {
            this.runInstaller = runInstaller;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            RunInstallerAttribute attribute = obj as RunInstallerAttribute;
            return ((attribute != null) && (attribute.RunInstaller == this.runInstaller));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public bool RunInstaller
        {
            get
            {
                return this.runInstaller;
            }
        }
    }
}

