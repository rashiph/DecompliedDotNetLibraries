namespace System.Net
{
    using System;
    using System.Text;

    internal class BasicClient : IAuthenticationModule
    {
        internal const string AuthType = "Basic";
        internal static string Signature = "Basic".ToLower(CultureInfo.InvariantCulture);
        internal static int SignatureSize = Signature.Length;

        public Authorization Authenticate(string challenge, WebRequest webRequest, ICredentials credentials)
        {
            if ((credentials == null) || (credentials is SystemNetworkCredential))
            {
                return null;
            }
            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
            if ((httpWebRequest == null) || (httpWebRequest.ChallengedUri == null))
            {
                return null;
            }
            if (AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature) < 0)
            {
                return null;
            }
            return this.Lookup(httpWebRequest, credentials);
        }

        internal static byte[] EncodingRightGetBytes(string rawString)
        {
            byte[] bytes = Encoding.Default.GetBytes(rawString);
            string strB = Encoding.Default.GetString(bytes);
            if (string.Compare(rawString, strB, StringComparison.Ordinal) != 0)
            {
                throw ExceptionHelper.MethodNotSupportedException;
            }
            return bytes;
        }

        private Authorization Lookup(HttpWebRequest httpWebRequest, ICredentials credentials)
        {
            NetworkCredential credential = credentials.GetCredential(httpWebRequest.ChallengedUri, Signature);
            if (credential == null)
            {
                return null;
            }
            ICredentialPolicy credentialPolicy = AuthenticationManager.CredentialPolicy;
            if ((credentialPolicy != null) && !credentialPolicy.ShouldSendCredential(httpWebRequest.ChallengedUri, httpWebRequest, credential, this))
            {
                return null;
            }
            string userName = credential.InternalGetUserName();
            string domain = credential.InternalGetDomain();
            if (ValidationHelper.IsBlankString(userName))
            {
                return null;
            }
            byte[] inArray = EncodingRightGetBytes((!ValidationHelper.IsBlankString(domain) ? (domain + @"\") : "") + userName + ":" + credential.InternalGetPassword());
            return new Authorization("Basic " + Convert.ToBase64String(inArray), true);
        }

        public Authorization PreAuthenticate(WebRequest webRequest, ICredentials credentials)
        {
            if ((credentials == null) || (credentials is SystemNetworkCredential))
            {
                return null;
            }
            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
            if (httpWebRequest == null)
            {
                return null;
            }
            return this.Lookup(httpWebRequest, credentials);
        }

        public string AuthenticationType
        {
            get
            {
                return "Basic";
            }
        }

        public bool CanPreAuthenticate
        {
            get
            {
                return true;
            }
        }
    }
}

