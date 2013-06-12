namespace System.Security.Permissions
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Policy;
    using System.Security.Util;
    using System.Text;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class PermissionSetAttribute : CodeAccessSecurityAttribute
    {
        private string m_file;
        private string m_hex;
        private string m_name;
        private bool m_unicode;
        private string m_xml;

        public PermissionSetAttribute(SecurityAction action) : base(action)
        {
            this.m_unicode = false;
        }

        private PermissionSet BruteForceParseStream(Stream stream)
        {
            Encoding[] encodingArray = new Encoding[] { Encoding.UTF8, Encoding.ASCII, Encoding.Unicode };
            StreamReader input = null;
            Exception exception = null;
            for (int i = 0; (input == null) && (i < encodingArray.Length); i++)
            {
                try
                {
                    stream.Position = 0L;
                    input = new StreamReader(stream, encodingArray[i]);
                    return this.ParsePermissionSet(new Parser(input));
                }
                catch (Exception exception2)
                {
                    if (exception == null)
                    {
                        exception = exception2;
                    }
                }
            }
            throw exception;
        }

        public override IPermission CreatePermission()
        {
            return null;
        }

        [SecuritySafeCritical]
        public PermissionSet CreatePermissionSet()
        {
            if (base.m_unrestricted)
            {
                return new PermissionSet(PermissionState.Unrestricted);
            }
            if (this.m_name != null)
            {
                return PolicyLevel.GetBuiltInSet(this.m_name);
            }
            if (this.m_xml != null)
            {
                return this.ParsePermissionSet(new Parser(this.m_xml.ToCharArray()));
            }
            if (this.m_hex != null)
            {
                return this.BruteForceParseStream(new MemoryStream(System.Security.Util.Hex.DecodeHexString(this.m_hex)));
            }
            if (this.m_file != null)
            {
                return this.BruteForceParseStream(new FileStream(this.m_file, FileMode.Open, FileAccess.Read));
            }
            return new PermissionSet(PermissionState.None);
        }

        private PermissionSet ParsePermissionSet(Parser parser)
        {
            SecurityElement topElement = parser.GetTopElement();
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.FromXml(topElement);
            return set;
        }

        public string File
        {
            get
            {
                return this.m_file;
            }
            set
            {
                this.m_file = value;
            }
        }

        public string Hex
        {
            get
            {
                return this.m_hex;
            }
            set
            {
                this.m_hex = value;
            }
        }

        public string Name
        {
            get
            {
                return this.m_name;
            }
            set
            {
                this.m_name = value;
            }
        }

        public bool UnicodeEncoded
        {
            get
            {
                return this.m_unicode;
            }
            set
            {
                this.m_unicode = value;
            }
        }

        public string XML
        {
            get
            {
                return this.m_xml;
            }
            set
            {
                this.m_xml = value;
            }
        }
    }
}

