namespace System.Management.Instrumentation
{
    using System;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;

    [AttributeUsage(AttributeTargets.Assembly), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class WmiConfigurationAttribute : Attribute
    {
        private string _HostingGroup;
        private ManagementHostingModel _HostingModel;
        private bool _IdentifyLevel = true;
        private string _NamespaceSecurity;
        private string _Scope;
        private string _SecurityRestriction;

        public WmiConfigurationAttribute(string scope)
        {
            string str = scope;
            if (str != null)
            {
                str = str.Replace('/', '\\');
            }
            if ((str == null) || (str.Length == 0))
            {
                str = @"root\default";
            }
            bool flag = true;
            foreach (string str2 in str.Split(new char[] { '\\' }))
            {
                if (((str2.Length != 0) && (!flag || (string.Compare(str2, "root", StringComparison.OrdinalIgnoreCase) == 0))) && (Regex.Match(str2, "^[a-z,A-Z]").Success && !Regex.Match(str2, "_$").Success))
                {
                    bool success = Regex.Match(str2, @"[^a-z,A-Z,0-9,_,\u0080-\uFFFF]").Success;
                }
                flag = false;
            }
            this._Scope = str;
        }

        public string HostingGroup
        {
            get
            {
                return this._HostingGroup;
            }
            set
            {
                this._HostingGroup = value;
            }
        }

        public ManagementHostingModel HostingModel
        {
            get
            {
                return this._HostingModel;
            }
            set
            {
                this._HostingModel = value;
            }
        }

        public bool IdentifyLevel
        {
            get
            {
                return this._IdentifyLevel;
            }
            set
            {
                this._IdentifyLevel = value;
            }
        }

        public string NamespaceSecurity
        {
            get
            {
                return this._NamespaceSecurity;
            }
            set
            {
                this._NamespaceSecurity = value;
            }
        }

        public string Scope
        {
            get
            {
                return this._Scope;
            }
        }

        public string SecurityRestriction
        {
            get
            {
                return this._SecurityRestriction;
            }
            set
            {
                this._SecurityRestriction = value;
            }
        }
    }
}

