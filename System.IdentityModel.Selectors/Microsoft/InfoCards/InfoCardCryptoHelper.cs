namespace Microsoft.InfoCards
{
    using System;

    internal static class InfoCardCryptoHelper
    {
        internal static bool IsAsymmetricAlgorithm(string algorithm)
        {
            string str;
            if (((str = algorithm) == null) || ((!(str == "http://www.w3.org/2000/09/xmldsig#dsa-sha1") && !(str == "http://www.w3.org/2000/09/xmldsig#rsa-sha1")) && ((!(str == "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256") && !(str == "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p")) && !(str == "http://www.w3.org/2001/04/xmlenc#rsa-1_5"))))
            {
                return false;
            }
            return true;
        }

        internal static bool IsSymmetricAlgorithm(string algorithm)
        {
            switch (algorithm)
            {
                case "http://www.w3.org/2000/09/xmldsig#hmac-sha1":
                case "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256":
                case "http://www.w3.org/2001/04/xmlenc#aes128-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes192-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes256-cbc":
                case "http://www.w3.org/2001/04/xmlenc#tripledes-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes128":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes192":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes256":
                case "http://www.w3.org/2001/04/xmlenc#kw-tripledes":
                case "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1":
                case "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1":
                    return true;
            }
            return false;
        }
    }
}

