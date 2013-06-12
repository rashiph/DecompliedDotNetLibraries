namespace System.Runtime.Versioning
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false, Inherited=false)]
    public sealed class TargetFrameworkAttribute : Attribute
    {
        private string _frameworkDisplayName;
        private string _frameworkName;

        public TargetFrameworkAttribute(string frameworkName)
        {
            if (frameworkName == null)
            {
                throw new ArgumentNullException("frameworkName");
            }
            this._frameworkName = frameworkName;
        }

        public string FrameworkDisplayName
        {
            get
            {
                return this._frameworkDisplayName;
            }
            set
            {
                this._frameworkDisplayName = value;
            }
        }

        public string FrameworkName
        {
            get
            {
                return this._frameworkName;
            }
        }
    }
}

