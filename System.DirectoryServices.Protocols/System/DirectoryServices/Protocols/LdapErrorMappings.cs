namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;

    internal class LdapErrorMappings
    {
        private static Hashtable ResultCodeHash = new Hashtable();

        static LdapErrorMappings()
        {
            ResultCodeHash.Add(LdapError.IsLeaf, Res.GetString("LDAP_IS_LEAF"));
            ResultCodeHash.Add(LdapError.InvalidCredentials, Res.GetString("LDAP_INVALID_CREDENTIALS"));
            ResultCodeHash.Add(LdapError.ServerDown, Res.GetString("LDAP_SERVER_DOWN"));
            ResultCodeHash.Add(LdapError.LocalError, Res.GetString("LDAP_LOCAL_ERROR"));
            ResultCodeHash.Add(LdapError.EncodingError, Res.GetString("LDAP_ENCODING_ERROR"));
            ResultCodeHash.Add(LdapError.DecodingError, Res.GetString("LDAP_DECODING_ERROR"));
            ResultCodeHash.Add(LdapError.TimeOut, Res.GetString("LDAP_TIMEOUT"));
            ResultCodeHash.Add(LdapError.AuthUnknown, Res.GetString("LDAP_AUTH_UNKNOWN"));
            ResultCodeHash.Add(LdapError.FilterError, Res.GetString("LDAP_FILTER_ERROR"));
            ResultCodeHash.Add(LdapError.UserCancelled, Res.GetString("LDAP_USER_CANCELLED"));
            ResultCodeHash.Add(LdapError.ParameterError, Res.GetString("LDAP_PARAM_ERROR"));
            ResultCodeHash.Add(LdapError.NoMemory, Res.GetString("LDAP_NO_MEMORY"));
            ResultCodeHash.Add(LdapError.ConnectError, Res.GetString("LDAP_CONNECT_ERROR"));
            ResultCodeHash.Add(LdapError.NotSupported, Res.GetString("LDAP_NOT_SUPPORTED"));
            ResultCodeHash.Add(LdapError.NoResultsReturned, Res.GetString("LDAP_NO_RESULTS_RETURNED"));
            ResultCodeHash.Add(LdapError.ControlNotFound, Res.GetString("LDAP_CONTROL_NOT_FOUND"));
            ResultCodeHash.Add(LdapError.MoreResults, Res.GetString("LDAP_MORE_RESULTS_TO_RETURN"));
            ResultCodeHash.Add(LdapError.ClientLoop, Res.GetString("LDAP_CLIENT_LOOP"));
            ResultCodeHash.Add(LdapError.ReferralLimitExceeded, Res.GetString("LDAP_REFERRAL_LIMIT_EXCEEDED"));
            ResultCodeHash.Add(LdapError.SendTimeOut, Res.GetString("LDAP_SEND_TIMEOUT"));
        }

        public static string MapResultCode(int errorCode)
        {
            return (string) ResultCodeHash[(LdapError) errorCode];
        }
    }
}

