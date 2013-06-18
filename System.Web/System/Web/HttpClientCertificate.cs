namespace System.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Text;
    using System.Web.Util;

    public class HttpClientCertificate : NameValueCollection
    {
        private byte[] _BinaryIssuer = new byte[0];
        private int _CertEncoding;
        private byte[] _Certificate = new byte[0];
        private string _Cookie = string.Empty;
        private int _Flags;
        private string _Issuer = string.Empty;
        private int _KeySize;
        private byte[] _PublicKey = new byte[0];
        private int _SecretKeySize;
        private string _SerialNumber = string.Empty;
        private string _ServerIssuer = string.Empty;
        private string _ServerSubject = string.Empty;
        private string _Subject = string.Empty;
        private DateTime _ValidFrom = DateTime.Now;
        private DateTime _ValidUntil = DateTime.Now;

        internal HttpClientCertificate(HttpContext context)
        {
            string str = context.Request.ServerVariables["CERT_FLAGS"];
            if (!string.IsNullOrEmpty(str))
            {
                this._Flags = int.Parse(str, CultureInfo.InvariantCulture);
            }
            else
            {
                this._Flags = 0;
            }
            if (this.IsPresent)
            {
                this._Cookie = context.Request.ServerVariables["CERT_COOKIE"];
                this._Issuer = context.Request.ServerVariables["CERT_ISSUER"];
                this._ServerIssuer = context.Request.ServerVariables["CERT_SERVER_ISSUER"];
                this._Subject = context.Request.ServerVariables["CERT_SUBJECT"];
                this._ServerSubject = context.Request.ServerVariables["CERT_SERVER_SUBJECT"];
                this._SerialNumber = context.Request.ServerVariables["CERT_SERIALNUMBER"];
                this._Certificate = context.WorkerRequest.GetClientCertificate();
                this._ValidFrom = context.WorkerRequest.GetClientCertificateValidFrom();
                this._ValidUntil = context.WorkerRequest.GetClientCertificateValidUntil();
                this._BinaryIssuer = context.WorkerRequest.GetClientCertificateBinaryIssuer();
                this._PublicKey = context.WorkerRequest.GetClientCertificatePublicKey();
                this._CertEncoding = context.WorkerRequest.GetClientCertificateEncoding();
                string str2 = context.Request.ServerVariables["CERT_KEYSIZE"];
                string str3 = context.Request.ServerVariables["CERT_SECRETKEYSIZE"];
                if (!string.IsNullOrEmpty(str2))
                {
                    this._KeySize = int.Parse(str2, CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(str3))
                {
                    this._SecretKeySize = int.Parse(str3, CultureInfo.InvariantCulture);
                }
                base.Add("ISSUER", null);
                base.Add("SUBJECTEMAIL", null);
                base.Add("BINARYISSUER", null);
                base.Add("FLAGS", null);
                base.Add("ISSUERO", null);
                base.Add("PUBLICKEY", null);
                base.Add("ISSUEROU", null);
                base.Add("ENCODING", null);
                base.Add("ISSUERCN", null);
                base.Add("SERIALNUMBER", null);
                base.Add("SUBJECT", null);
                base.Add("SUBJECTCN", null);
                base.Add("CERTIFICATE", null);
                base.Add("SUBJECTO", null);
                base.Add("SUBJECTOU", null);
                base.Add("VALIDUNTIL", null);
                base.Add("VALIDFROM", null);
            }
        }

        private string ExtractString(string strAll, string strSubject)
        {
            if ((strAll == null) || (strSubject == null))
            {
                return string.Empty;
            }
            string str = string.Empty;
            int startIndex = 0;
            string str2 = strAll.ToLower(CultureInfo.InvariantCulture);
            while (startIndex < str2.Length)
            {
                startIndex = str2.IndexOf(strSubject + "=", startIndex, StringComparison.Ordinal);
                if (startIndex < 0)
                {
                    return str;
                }
                if (str.Length > 0)
                {
                    str = str + ";";
                }
                startIndex += strSubject.Length + 1;
                int index = 0;
                if (strAll[startIndex] == '"')
                {
                    startIndex++;
                    index = strAll.IndexOf('"', startIndex);
                }
                else
                {
                    index = strAll.IndexOf(',', startIndex);
                }
                if (index < 0)
                {
                    index = strAll.Length;
                }
                str = str + strAll.Substring(startIndex, index - startIndex);
                startIndex = index + 1;
            }
            return str;
        }

        public override string Get(string field)
        {
            if (field != null)
            {
                field = field.ToLower(CultureInfo.InvariantCulture);
                switch (field)
                {
                    case "cookie":
                        return this.Cookie;

                    case "flags":
                        return this.Flags.ToString("G", CultureInfo.InvariantCulture);

                    case "keysize":
                        return this.KeySize.ToString("G", CultureInfo.InvariantCulture);

                    case "secretkeysize":
                        return this.SecretKeySize.ToString(CultureInfo.InvariantCulture);

                    case "issuer":
                        return this.Issuer;

                    case "serverissuer":
                        return this.ServerIssuer;

                    case "subject":
                        return this.Subject;

                    case "serversubject":
                        return this.ServerSubject;

                    case "serialnumber":
                        return this.SerialNumber;

                    case "certificate":
                        return Encoding.Default.GetString(this.Certificate);

                    case "binaryissuer":
                        return Encoding.Default.GetString(this.BinaryIssuer);

                    case "publickey":
                        return Encoding.Default.GetString(this.PublicKey);

                    case "encoding":
                        return this.CertEncoding.ToString("G", CultureInfo.InvariantCulture);

                    case "validfrom":
                        return HttpUtility.FormatHttpDateTime(this.ValidFrom);

                    case "validuntil":
                        return HttpUtility.FormatHttpDateTime(this.ValidUntil);
                }
                if (StringUtil.StringStartsWith(field, "issuer"))
                {
                    return this.ExtractString(this.Issuer, field.Substring(6));
                }
                if (StringUtil.StringStartsWith(field, "subject"))
                {
                    if (field.Equals("subjectemail"))
                    {
                        return this.ExtractString(this.Subject, "e");
                    }
                    return this.ExtractString(this.Subject, field.Substring(7));
                }
                if (StringUtil.StringStartsWith(field, "serversubject"))
                {
                    return this.ExtractString(this.ServerSubject, field.Substring(13));
                }
                if (StringUtil.StringStartsWith(field, "serverissuer"))
                {
                    return this.ExtractString(this.ServerIssuer, field.Substring(12));
                }
            }
            return string.Empty;
        }

        public byte[] BinaryIssuer
        {
            get
            {
                return this._BinaryIssuer;
            }
        }

        public int CertEncoding
        {
            get
            {
                return this._CertEncoding;
            }
        }

        public byte[] Certificate
        {
            get
            {
                return this._Certificate;
            }
        }

        public string Cookie
        {
            get
            {
                return this._Cookie;
            }
        }

        public int Flags
        {
            get
            {
                return this._Flags;
            }
        }

        public bool IsPresent
        {
            get
            {
                return ((this._Flags & 1) == 1);
            }
        }

        public string Issuer
        {
            get
            {
                return this._Issuer;
            }
        }

        public bool IsValid
        {
            get
            {
                return ((this._Flags & 2) == 0);
            }
        }

        public int KeySize
        {
            get
            {
                return this._KeySize;
            }
        }

        public byte[] PublicKey
        {
            get
            {
                return this._PublicKey;
            }
        }

        public int SecretKeySize
        {
            get
            {
                return this._SecretKeySize;
            }
        }

        public string SerialNumber
        {
            get
            {
                return this._SerialNumber;
            }
        }

        public string ServerIssuer
        {
            get
            {
                return this._ServerIssuer;
            }
        }

        public string ServerSubject
        {
            get
            {
                return this._ServerSubject;
            }
        }

        public string Subject
        {
            get
            {
                return this._Subject;
            }
        }

        public DateTime ValidFrom
        {
            get
            {
                return this._ValidFrom;
            }
        }

        public DateTime ValidUntil
        {
            get
            {
                return this._ValidUntil;
            }
        }
    }
}

