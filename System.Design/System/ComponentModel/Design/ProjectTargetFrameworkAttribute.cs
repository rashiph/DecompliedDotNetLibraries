namespace System.ComponentModel.Design
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class ProjectTargetFrameworkAttribute : Attribute
    {
        private string _targetFrameworkMoniker;

        public ProjectTargetFrameworkAttribute(string targetFrameworkMoniker)
        {
            this._targetFrameworkMoniker = targetFrameworkMoniker;
        }

        public string TargetFrameworkMoniker
        {
            get
            {
                return this._targetFrameworkMoniker;
            }
        }
    }
}

