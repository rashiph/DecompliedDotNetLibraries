namespace System.Net
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;

    internal class HttpDigestChallenge
    {
        internal string Algorithm;
        internal System.Uri ChallengedUri;
        internal string ChannelBinding;
        internal string Charset;
        internal string ClientNonce;
        internal string Domain;
        internal string HostName;
        internal MD5CryptoServiceProvider MD5provider = new MD5CryptoServiceProvider();
        internal string Method;
        internal string Nonce;
        internal int NonceCount;
        internal string Opaque;
        internal bool QopPresent;
        internal string QualityOfProtection;
        internal string Realm;
        internal string ServiceName;
        internal bool Stale;
        internal string Uri;
        internal bool UTF8Charset;

        internal HttpDigestChallenge CopyAndIncrementNonce()
        {
            HttpDigestChallenge challenge = null;
            lock (this)
            {
                challenge = base.MemberwiseClone() as HttpDigestChallenge;
                this.NonceCount++;
            }
            challenge.MD5provider = new MD5CryptoServiceProvider();
            return challenge;
        }

        public bool defineAttribute(string name, string value)
        {
            name = name.Trim().ToLower(CultureInfo.InvariantCulture);
            if (name.Equals("algorithm"))
            {
                this.Algorithm = value;
            }
            else if (name.Equals("cnonce"))
            {
                this.ClientNonce = value;
            }
            else if (name.Equals("nc"))
            {
                this.NonceCount = int.Parse(value, NumberFormatInfo.InvariantInfo);
            }
            else if (name.Equals("nonce"))
            {
                this.Nonce = value;
            }
            else if (name.Equals("opaque"))
            {
                this.Opaque = value;
            }
            else if (name.Equals("qop"))
            {
                this.QualityOfProtection = value;
                this.QopPresent = (this.QualityOfProtection != null) && (this.QualityOfProtection.Length > 0);
            }
            else if (name.Equals("realm"))
            {
                this.Realm = value;
            }
            else if (name.Equals("domain"))
            {
                this.Domain = value;
            }
            else if (!name.Equals("response"))
            {
                if (name.Equals("stale"))
                {
                    this.Stale = value.ToLower(CultureInfo.InvariantCulture).Equals("true");
                }
                else if (name.Equals("uri"))
                {
                    this.Uri = value;
                }
                else if (name.Equals("charset"))
                {
                    this.Charset = value;
                }
                else if (!name.Equals("cipher") && !name.Equals("username"))
                {
                    return false;
                }
            }
            return true;
        }

        internal void SetFromRequest(HttpWebRequest httpWebRequest)
        {
            this.HostName = httpWebRequest.ChallengedUri.Host;
            this.Method = httpWebRequest.CurrentMethod.Name;
            this.Uri = httpWebRequest.Address.AbsolutePath;
            this.ChallengedUri = httpWebRequest.ChallengedUri;
        }

        internal string ToBlob()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(HttpDigest.pair("realm", this.Realm, true));
            if (this.Algorithm != null)
            {
                builder.Append(",");
                builder.Append(HttpDigest.pair("algorithm", this.Algorithm, true));
            }
            if (this.Charset != null)
            {
                builder.Append(",");
                builder.Append(HttpDigest.pair("charset", this.Charset, false));
            }
            if (this.Nonce != null)
            {
                builder.Append(",");
                builder.Append(HttpDigest.pair("nonce", this.Nonce, true));
            }
            if (this.Uri != null)
            {
                builder.Append(",");
                builder.Append(HttpDigest.pair("uri", this.Uri, true));
            }
            if (this.ClientNonce != null)
            {
                builder.Append(",");
                builder.Append(HttpDigest.pair("cnonce", this.ClientNonce, true));
            }
            if (this.NonceCount > 0)
            {
                builder.Append(",");
                builder.Append(HttpDigest.pair("nc", this.NonceCount.ToString("x8", NumberFormatInfo.InvariantInfo), true));
            }
            if (this.QualityOfProtection != null)
            {
                builder.Append(",");
                builder.Append(HttpDigest.pair("qop", this.QualityOfProtection, true));
            }
            if (this.Opaque != null)
            {
                builder.Append(",");
                builder.Append(HttpDigest.pair("opaque", this.Opaque, true));
            }
            if (this.Domain != null)
            {
                builder.Append(",");
                builder.Append(HttpDigest.pair("domain", this.Domain, true));
            }
            if (this.Stale)
            {
                builder.Append(",");
                builder.Append(HttpDigest.pair("stale", "true", true));
            }
            return builder.ToString();
        }
    }
}

