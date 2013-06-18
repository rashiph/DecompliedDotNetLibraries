namespace System.Xaml
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class XamlReaderSettings
    {
        public XamlReaderSettings()
        {
            this.InitializeProvideLineInfo();
        }

        public XamlReaderSettings(XamlReaderSettings settings) : this()
        {
            if (settings != null)
            {
                this.AllowProtectedMembersOnRoot = settings.AllowProtectedMembersOnRoot;
                this.ProvideLineInfo = settings.ProvideLineInfo;
                this.BaseUri = settings.BaseUri;
                this.LocalAssembly = settings.LocalAssembly;
                this.IgnoreUidsOnPropertyElements = settings.IgnoreUidsOnPropertyElements;
                this.ValuesMustBeString = settings.ValuesMustBeString;
            }
        }

        private void InitializeProvideLineInfo()
        {
            if (Debugger.IsAttached)
            {
                this.ProvideLineInfo = true;
            }
            else
            {
                this.ProvideLineInfo = false;
            }
        }

        public bool AllowProtectedMembersOnRoot { get; set; }

        public Uri BaseUri { get; set; }

        public bool IgnoreUidsOnPropertyElements { get; set; }

        public Assembly LocalAssembly { get; set; }

        public bool ProvideLineInfo { get; set; }

        public bool ValuesMustBeString { get; set; }
    }
}

