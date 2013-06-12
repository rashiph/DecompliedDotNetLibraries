namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public sealed class OperatingSystem : ICloneable, ISerializable
    {
        private PlatformID _platform;
        private string _servicePack;
        private System.Version _version;
        private string _versionString;

        private OperatingSystem()
        {
        }

        public OperatingSystem(PlatformID platform, System.Version version) : this(platform, version, null)
        {
        }

        private OperatingSystem(SerializationInfo info, StreamingContext context)
        {
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string name = enumerator.Name;
                if (name != null)
                {
                    if (!(name == "_version"))
                    {
                        if (name == "_platform")
                        {
                            goto Label_0067;
                        }
                        if (name == "_servicePack")
                        {
                            goto Label_0089;
                        }
                    }
                    else
                    {
                        this._version = (System.Version) info.GetValue("_version", typeof(System.Version));
                    }
                }
                continue;
            Label_0067:
                this._platform = (PlatformID) info.GetValue("_platform", typeof(PlatformID));
                continue;
            Label_0089:
                this._servicePack = info.GetString("_servicePack");
            }
            if (this._version == null)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_MissField", new object[] { "_version" }));
            }
        }

        internal OperatingSystem(PlatformID platform, System.Version version, string servicePack)
        {
            if ((platform < PlatformID.Win32S) || (platform > PlatformID.MacOSX))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) platform }), "platform");
            }
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            this._platform = platform;
            this._version = (System.Version) version.Clone();
            this._servicePack = servicePack;
        }

        public object Clone()
        {
            return new OperatingSystem(this._platform, this._version, this._servicePack);
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("_version", this._version);
            info.AddValue("_platform", this._platform);
            info.AddValue("_servicePack", this._servicePack);
        }

        public override string ToString()
        {
            return this.VersionString;
        }

        public PlatformID Platform
        {
            get
            {
                return this._platform;
            }
        }

        public string ServicePack
        {
            get
            {
                if (this._servicePack == null)
                {
                    return string.Empty;
                }
                return this._servicePack;
            }
        }

        public System.Version Version
        {
            get
            {
                return this._version;
            }
        }

        public string VersionString
        {
            get
            {
                if (this._versionString == null)
                {
                    string str;
                    switch (this._platform)
                    {
                        case PlatformID.Win32S:
                            str = "Microsoft Win32S ";
                            break;

                        case PlatformID.Win32Windows:
                            if ((this._version.Major <= 4) && ((this._version.Major != 4) || (this._version.Minor <= 0)))
                            {
                                str = "Microsoft Windows 95 ";
                            }
                            else
                            {
                                str = "Microsoft Windows 98 ";
                            }
                            break;

                        case PlatformID.Win32NT:
                            str = "Microsoft Windows NT ";
                            break;

                        case PlatformID.WinCE:
                            str = "Microsoft Windows CE ";
                            break;

                        case PlatformID.MacOSX:
                            str = "Mac OS X ";
                            break;

                        default:
                            str = "<unknown> ";
                            break;
                    }
                    if (string.IsNullOrEmpty(this._servicePack))
                    {
                        this._versionString = str + this._version.ToString();
                    }
                    else
                    {
                        this._versionString = str + this._version.ToString(3) + " " + this._servicePack;
                    }
                }
                return this._versionString;
            }
        }
    }
}

