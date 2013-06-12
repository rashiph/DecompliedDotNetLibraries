namespace System.Web.UI
{
    using System;
    using System.Web.Util;

    public sealed class OutputCacheParameters
    {
        private string _cacheProfile;
        private int _duration;
        private bool _enabled = true;
        private SimpleBitVector32 _flags;
        private OutputCacheLocation _location;
        private bool _noStore;
        private string _sqlDependency;
        private string _varyByContentEncoding;
        private string _varyByControl;
        private string _varyByCustom;
        private string _varyByHeader;
        private string _varyByParam;

        internal bool IsParameterSet(OutputCacheParameter value)
        {
            return this._flags[(int) value];
        }

        public string CacheProfile
        {
            get
            {
                return this._cacheProfile;
            }
            set
            {
                this._flags[1] = true;
                this._cacheProfile = value;
            }
        }

        public int Duration
        {
            get
            {
                return this._duration;
            }
            set
            {
                this._flags[2] = true;
                this._duration = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return this._enabled;
            }
            set
            {
                this._flags[4] = true;
                this._enabled = value;
            }
        }

        public OutputCacheLocation Location
        {
            get
            {
                return this._location;
            }
            set
            {
                this._flags[8] = true;
                this._location = value;
            }
        }

        public bool NoStore
        {
            get
            {
                return this._noStore;
            }
            set
            {
                this._flags[0x10] = true;
                this._noStore = value;
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
                this._flags[0x20] = true;
                this._sqlDependency = value;
            }
        }

        public string VaryByContentEncoding
        {
            get
            {
                return this._varyByContentEncoding;
            }
            set
            {
                this._flags[0x400] = true;
                this._varyByContentEncoding = value;
            }
        }

        public string VaryByControl
        {
            get
            {
                return this._varyByControl;
            }
            set
            {
                this._flags[0x40] = true;
                this._varyByControl = value;
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
                this._flags[0x80] = true;
                this._varyByCustom = value;
            }
        }

        public string VaryByHeader
        {
            get
            {
                return this._varyByHeader;
            }
            set
            {
                this._flags[0x100] = true;
                this._varyByHeader = value;
            }
        }

        public string VaryByParam
        {
            get
            {
                return this._varyByParam;
            }
            set
            {
                this._flags[0x200] = true;
                this._varyByParam = value;
            }
        }
    }
}

