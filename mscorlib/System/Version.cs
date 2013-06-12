namespace System
{
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public sealed class Version : ICloneable, IComparable, IComparable<Version>, IEquatable<Version>
    {
        private int _Build;
        private int _Major;
        private int _Minor;
        private int _Revision;

        public Version()
        {
            this._Build = -1;
            this._Revision = -1;
            this._Major = 0;
            this._Minor = 0;
        }

        public Version(string version)
        {
            this._Build = -1;
            this._Revision = -1;
            Version version2 = Parse(version);
            this._Major = version2.Major;
            this._Minor = version2.Minor;
            this._Build = version2.Build;
            this._Revision = version2.Revision;
        }

        public Version(int major, int minor)
        {
            this._Build = -1;
            this._Revision = -1;
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("ArgumentOutOfRange_Version"));
            }
            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("ArgumentOutOfRange_Version"));
            }
            this._Major = major;
            this._Minor = minor;
        }

        public Version(int major, int minor, int build)
        {
            this._Build = -1;
            this._Revision = -1;
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("ArgumentOutOfRange_Version"));
            }
            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("ArgumentOutOfRange_Version"));
            }
            if (build < 0)
            {
                throw new ArgumentOutOfRangeException("build", Environment.GetResourceString("ArgumentOutOfRange_Version"));
            }
            this._Major = major;
            this._Minor = minor;
            this._Build = build;
        }

        public Version(int major, int minor, int build, int revision)
        {
            this._Build = -1;
            this._Revision = -1;
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("ArgumentOutOfRange_Version"));
            }
            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("ArgumentOutOfRange_Version"));
            }
            if (build < 0)
            {
                throw new ArgumentOutOfRangeException("build", Environment.GetResourceString("ArgumentOutOfRange_Version"));
            }
            if (revision < 0)
            {
                throw new ArgumentOutOfRangeException("revision", Environment.GetResourceString("ArgumentOutOfRange_Version"));
            }
            this._Major = major;
            this._Minor = minor;
            this._Build = build;
            this._Revision = revision;
        }

        public object Clone()
        {
            return new Version { _Major = this._Major, _Minor = this._Minor, _Build = this._Build, _Revision = this._Revision };
        }

        public int CompareTo(object version)
        {
            if (version == null)
            {
                return 1;
            }
            Version version2 = version as Version;
            if (version2 == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeVersion"));
            }
            if (this._Major != version2._Major)
            {
                if (this._Major > version2._Major)
                {
                    return 1;
                }
                return -1;
            }
            if (this._Minor != version2._Minor)
            {
                if (this._Minor > version2._Minor)
                {
                    return 1;
                }
                return -1;
            }
            if (this._Build != version2._Build)
            {
                if (this._Build > version2._Build)
                {
                    return 1;
                }
                return -1;
            }
            if (this._Revision == version2._Revision)
            {
                return 0;
            }
            if (this._Revision > version2._Revision)
            {
                return 1;
            }
            return -1;
        }

        public int CompareTo(Version value)
        {
            if (value == null)
            {
                return 1;
            }
            if (this._Major != value._Major)
            {
                if (this._Major > value._Major)
                {
                    return 1;
                }
                return -1;
            }
            if (this._Minor != value._Minor)
            {
                if (this._Minor > value._Minor)
                {
                    return 1;
                }
                return -1;
            }
            if (this._Build != value._Build)
            {
                if (this._Build > value._Build)
                {
                    return 1;
                }
                return -1;
            }
            if (this._Revision == value._Revision)
            {
                return 0;
            }
            if (this._Revision > value._Revision)
            {
                return 1;
            }
            return -1;
        }

        public override bool Equals(object obj)
        {
            Version version = obj as Version;
            if (version == null)
            {
                return false;
            }
            return (((this._Major == version._Major) && (this._Minor == version._Minor)) && ((this._Build == version._Build) && (this._Revision == version._Revision)));
        }

        public bool Equals(Version obj)
        {
            if (obj == null)
            {
                return false;
            }
            return (((this._Major == obj._Major) && (this._Minor == obj._Minor)) && ((this._Build == obj._Build) && (this._Revision == obj._Revision)));
        }

        public override int GetHashCode()
        {
            int num = 0;
            num |= (this._Major & 15) << 0x1c;
            num |= (this._Minor & 0xff) << 20;
            num |= (this._Build & 0xff) << 12;
            return (num | (this._Revision & 0xfff));
        }

        public static bool operator ==(Version v1, Version v2)
        {
            if (object.ReferenceEquals(v1, null))
            {
                return object.ReferenceEquals(v2, null);
            }
            return v1.Equals(v2);
        }

        public static bool operator >(Version v1, Version v2)
        {
            return (v2 < v1);
        }

        public static bool operator >=(Version v1, Version v2)
        {
            return (v2 <= v1);
        }

        public static bool operator !=(Version v1, Version v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(Version v1, Version v2)
        {
            if (v1 == null)
            {
                throw new ArgumentNullException("v1");
            }
            return (v1.CompareTo(v2) < 0);
        }

        public static bool operator <=(Version v1, Version v2)
        {
            if (v1 == null)
            {
                throw new ArgumentNullException("v1");
            }
            return (v1.CompareTo(v2) <= 0);
        }

        public static Version Parse(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            VersionResult result = new VersionResult();
            result.Init("input", true);
            if (!TryParseVersion(input, ref result))
            {
                throw result.GetVersionParseException();
            }
            return result.m_parsedVersion;
        }

        public override string ToString()
        {
            if (this._Build == -1)
            {
                return this.ToString(2);
            }
            if (this._Revision == -1)
            {
                return this.ToString(3);
            }
            return this.ToString(4);
        }

        public string ToString(int fieldCount)
        {
            switch (fieldCount)
            {
                case 0:
                    return string.Empty;

                case 1:
                    return (this._Major);

                case 2:
                    return (this._Major + "." + this._Minor);
            }
            if (this._Build == -1)
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", new object[] { "0", "2" }), "fieldCount");
            }
            if (fieldCount == 3)
            {
                return string.Concat(new object[] { this._Major, ".", this._Minor, ".", this._Build });
            }
            if (this._Revision == -1)
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", new object[] { "0", "3" }), "fieldCount");
            }
            if (fieldCount != 4)
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", new object[] { "0", "4" }), "fieldCount");
            }
            return string.Concat(new object[] { this.Major, ".", this._Minor, ".", this._Build, ".", this._Revision });
        }

        public static bool TryParse(string input, out Version result)
        {
            VersionResult result2 = new VersionResult();
            result2.Init("input", false);
            bool flag = TryParseVersion(input, ref result2);
            result = result2.m_parsedVersion;
            return flag;
        }

        private static bool TryParseComponent(string component, string componentName, ref VersionResult result, out int parsedComponent)
        {
            if (!int.TryParse(component, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedComponent))
            {
                result.SetFailure(ParseFailureKind.FormatException, component);
                return false;
            }
            if (parsedComponent < 0)
            {
                result.SetFailure(ParseFailureKind.ArgumentOutOfRangeException, componentName);
                return false;
            }
            return true;
        }

        private static bool TryParseVersion(string version, ref VersionResult result)
        {
            int num;
            int num2;
            if (version == null)
            {
                result.SetFailure(ParseFailureKind.ArgumentNullException);
                return false;
            }
            string[] strArray = version.Split(new char[] { '.' });
            int length = strArray.Length;
            if ((length < 2) || (length > 4))
            {
                result.SetFailure(ParseFailureKind.ArgumentException);
                return false;
            }
            if (!TryParseComponent(strArray[0], "version", ref result, out num))
            {
                return false;
            }
            if (!TryParseComponent(strArray[1], "version", ref result, out num2))
            {
                return false;
            }
            length -= 2;
            if (length > 0)
            {
                int num3;
                if (!TryParseComponent(strArray[2], "build", ref result, out num3))
                {
                    return false;
                }
                length--;
                if (length > 0)
                {
                    int num4;
                    if (!TryParseComponent(strArray[3], "revision", ref result, out num4))
                    {
                        return false;
                    }
                    result.m_parsedVersion = new Version(num, num2, num3, num4);
                }
                else
                {
                    result.m_parsedVersion = new Version(num, num2, num3);
                }
            }
            else
            {
                result.m_parsedVersion = new Version(num, num2);
            }
            return true;
        }

        public int Build
        {
            get
            {
                return this._Build;
            }
        }

        public int Major
        {
            get
            {
                return this._Major;
            }
        }

        public short MajorRevision
        {
            get
            {
                return (short) (this._Revision >> 0x10);
            }
        }

        public int Minor
        {
            get
            {
                return this._Minor;
            }
        }

        public short MinorRevision
        {
            get
            {
                return (short) (this._Revision & 0xffff);
            }
        }

        public int Revision
        {
            get
            {
                return this._Revision;
            }
        }

        internal enum ParseFailureKind
        {
            ArgumentNullException,
            ArgumentException,
            ArgumentOutOfRangeException,
            FormatException
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct VersionResult
        {
            internal Version m_parsedVersion;
            internal Version.ParseFailureKind m_failure;
            internal string m_exceptionArgument;
            internal string m_argumentName;
            internal bool m_canThrow;
            internal void Init(string argumentName, bool canThrow)
            {
                this.m_canThrow = canThrow;
                this.m_argumentName = argumentName;
            }

            internal void SetFailure(Version.ParseFailureKind failure)
            {
                this.SetFailure(failure, string.Empty);
            }

            internal void SetFailure(Version.ParseFailureKind failure, string argument)
            {
                this.m_failure = failure;
                this.m_exceptionArgument = argument;
                if (this.m_canThrow)
                {
                    throw this.GetVersionParseException();
                }
            }

            internal Exception GetVersionParseException()
            {
                switch (this.m_failure)
                {
                    case Version.ParseFailureKind.ArgumentNullException:
                        return new ArgumentNullException(this.m_argumentName);

                    case Version.ParseFailureKind.ArgumentException:
                        return new ArgumentException(Environment.GetResourceString("Arg_VersionString"));

                    case Version.ParseFailureKind.ArgumentOutOfRangeException:
                        return new ArgumentOutOfRangeException(this.m_exceptionArgument, Environment.GetResourceString("ArgumentOutOfRange_Version"));

                    case Version.ParseFailureKind.FormatException:
                        try
                        {
                            int.Parse(this.m_exceptionArgument, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException exception)
                        {
                            return exception;
                        }
                        catch (OverflowException exception2)
                        {
                            return exception2;
                        }
                        return new FormatException(Environment.GetResourceString("Format_InvalidString"));
                }
                return new ArgumentException(Environment.GetResourceString("Arg_VersionString"));
            }
        }
    }
}

