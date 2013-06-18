namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PartialCachingAttribute : Attribute
    {
        private int _duration;
        private string _providerName;
        private bool _shared;
        private string _sqlDependency;
        private string _varyByControls;
        private string _varyByCustom;
        private string _varyByParams;

        public PartialCachingAttribute(int duration)
        {
            this._duration = duration;
        }

        public PartialCachingAttribute(int duration, string varyByParams, string varyByControls, string varyByCustom) : this(duration, varyByParams, varyByControls, varyByCustom, null, false)
        {
        }

        public PartialCachingAttribute(int duration, string varyByParams, string varyByControls, string varyByCustom, bool shared) : this(duration, varyByParams, varyByControls, varyByCustom, null, shared)
        {
        }

        public PartialCachingAttribute(int duration, string varyByParams, string varyByControls, string varyByCustom, string sqlDependency, bool shared)
        {
            this._duration = duration;
            this._varyByParams = varyByParams;
            this._varyByControls = varyByControls;
            this._varyByCustom = varyByCustom;
            this._shared = shared;
            this._sqlDependency = sqlDependency;
        }

        public int Duration
        {
            get
            {
                return this._duration;
            }
            set
            {
                this._duration = value;
            }
        }

        public string ProviderName
        {
            get
            {
                if (this._providerName == null)
                {
                    return "AspNetInternalProvider";
                }
                return this._providerName;
            }
            set
            {
                if (value == "AspNetInternalProvider")
                {
                    value = null;
                }
                this._providerName = value;
            }
        }

        public bool Shared
        {
            get
            {
                return this._shared;
            }
            set
            {
                this._shared = value;
            }
        }

        public string SqlDependency
        {
            get
            {
                return this._sqlDependency;
            }
            set
            {
                this._sqlDependency = value;
            }
        }

        public string VaryByControls
        {
            get
            {
                return this._varyByControls;
            }
            set
            {
                this._varyByControls = value;
            }
        }

        public string VaryByCustom
        {
            get
            {
                return this._varyByCustom;
            }
            set
            {
                this._varyByCustom = value;
            }
        }

        public string VaryByParams
        {
            get
            {
                return this._varyByParams;
            }
            set
            {
                this._varyByParams = value;
            }
        }
    }
}

