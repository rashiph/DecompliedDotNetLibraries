namespace System.Security.Policy
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public class CodeConnectAccess
    {
        private int _IntPort;
        private string _LowerCasePort;
        private string _LowerCaseScheme;
        internal const int AnyPort = -2;
        public static readonly string AnyScheme = "*";
        public static readonly int DefaultPort = -3;
        private const string DefaultStr = "$default";
        internal const int NoPort = -1;
        public static readonly int OriginPort = -4;
        public static readonly string OriginScheme = "$origin";
        private const string OriginStr = "$origin";

        private CodeConnectAccess()
        {
        }

        public CodeConnectAccess(string allowScheme, int allowPort)
        {
            if (!IsValidScheme(allowScheme))
            {
                throw new ArgumentOutOfRangeException("allowScheme");
            }
            this.SetCodeConnectAccess(allowScheme.ToLower(CultureInfo.InvariantCulture), allowPort);
        }

        internal CodeConnectAccess(string allowScheme, string allowPort)
        {
            if ((allowScheme == null) || (allowScheme.Length == 0))
            {
                throw new ArgumentNullException("allowScheme");
            }
            if ((allowPort == null) || (allowPort.Length == 0))
            {
                throw new ArgumentNullException("allowPort");
            }
            this._LowerCaseScheme = allowScheme.ToLower(CultureInfo.InvariantCulture);
            if (this._LowerCaseScheme == OriginScheme)
            {
                this._LowerCaseScheme = OriginScheme;
            }
            else if (this._LowerCaseScheme == AnyScheme)
            {
                this._LowerCaseScheme = AnyScheme;
            }
            else if (!IsValidScheme(this._LowerCaseScheme))
            {
                throw new ArgumentOutOfRangeException("allowScheme");
            }
            this._LowerCasePort = allowPort.ToLower(CultureInfo.InvariantCulture);
            if (this._LowerCasePort == "$default")
            {
                this._IntPort = DefaultPort;
            }
            else if (this._LowerCasePort == "$origin")
            {
                this._IntPort = OriginPort;
            }
            else
            {
                this._IntPort = int.Parse(allowPort, CultureInfo.InvariantCulture);
                if ((this._IntPort < 0) || (this._IntPort > 0xffff))
                {
                    throw new ArgumentOutOfRangeException("allowPort");
                }
                this._LowerCasePort = this._IntPort.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static CodeConnectAccess CreateAnySchemeAccess(int allowPort)
        {
            CodeConnectAccess access = new CodeConnectAccess();
            access.SetCodeConnectAccess(AnyScheme, allowPort);
            return access;
        }

        public static CodeConnectAccess CreateOriginSchemeAccess(int allowPort)
        {
            CodeConnectAccess access = new CodeConnectAccess();
            access.SetCodeConnectAccess(OriginScheme, allowPort);
            return access;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            CodeConnectAccess access = o as CodeConnectAccess;
            if (access == null)
            {
                return false;
            }
            return ((this.Scheme == access.Scheme) && (this.Port == access.Port));
        }

        public override int GetHashCode()
        {
            return (this.Scheme.GetHashCode() + this.Port.GetHashCode());
        }

        private static bool IsAsciiLetter(char character)
        {
            return (((character >= 'a') && (character <= 'z')) || ((character >= 'A') && (character <= 'Z')));
        }

        private static bool IsAsciiLetterOrDigit(char character)
        {
            return (IsAsciiLetter(character) || ((character >= '0') && (character <= '9')));
        }

        internal static bool IsValidScheme(string scheme)
        {
            if (((scheme == null) || (scheme.Length == 0)) || !IsAsciiLetter(scheme[0]))
            {
                return false;
            }
            for (int i = scheme.Length - 1; i > 0; i--)
            {
                if ((!IsAsciiLetterOrDigit(scheme[i]) && (scheme[i] != '+')) && ((scheme[i] != '-') && (scheme[i] != '.')))
                {
                    return false;
                }
            }
            return true;
        }

        private void SetCodeConnectAccess(string lowerCaseScheme, int allowPort)
        {
            this._LowerCaseScheme = lowerCaseScheme;
            if (allowPort == DefaultPort)
            {
                this._LowerCasePort = "$default";
            }
            else if (allowPort == OriginPort)
            {
                this._LowerCasePort = "$origin";
            }
            else
            {
                if ((allowPort < 0) || (allowPort > 0xffff))
                {
                    throw new ArgumentOutOfRangeException("allowPort");
                }
                this._LowerCasePort = allowPort.ToString(CultureInfo.InvariantCulture);
            }
            this._IntPort = allowPort;
        }

        internal bool IsAnyScheme
        {
            get
            {
                return (this._LowerCaseScheme == AnyScheme);
            }
        }

        internal bool IsDefaultPort
        {
            get
            {
                return (this.Port == DefaultPort);
            }
        }

        internal bool IsOriginPort
        {
            get
            {
                return (this.Port == OriginPort);
            }
        }

        internal bool IsOriginScheme
        {
            get
            {
                return (this._LowerCaseScheme == OriginScheme);
            }
        }

        public int Port
        {
            get
            {
                return this._IntPort;
            }
        }

        public string Scheme
        {
            get
            {
                return this._LowerCaseScheme;
            }
        }

        internal string StrPort
        {
            get
            {
                return this._LowerCasePort;
            }
        }
    }
}

