namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Net.Configuration;
    using System.Reflection;
    using System.Security.Authentication.ExtendedProtection;
    using System.Threading;

    public class AuthenticationManager
    {
        private static System.Net.SpnDictionary m_SpnDictionary = new System.Net.SpnDictionary();
        private static ICredentialPolicy s_ICredentialPolicy;
        private static PrefixLookup s_ModuleBinding = new PrefixLookup();
        private static ArrayList s_ModuleList;
        private static TriState s_OSSupportsExtendedProtection = TriState.Unspecified;
        private static TriState s_SspSupportsExtendedProtection = TriState.Unspecified;

        private AuthenticationManager()
        {
        }

        public static Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }
            if (challenge == null)
            {
                throw new ArgumentNullException("challenge");
            }
            Authorization authorization = null;
            HttpWebRequest request2 = request as HttpWebRequest;
            if ((request2 != null) && (request2.CurrentAuthenticationState.Module != null))
            {
                return request2.CurrentAuthenticationState.Module.Authenticate(challenge, request, credentials);
            }
            lock (s_ModuleBinding)
            {
                for (int i = 0; i < ModuleList.Count; i++)
                {
                    IAuthenticationModule module = (IAuthenticationModule) ModuleList[i];
                    if (request2 != null)
                    {
                        request2.CurrentAuthenticationState.Module = module;
                    }
                    authorization = module.Authenticate(challenge, request, credentials);
                    if (authorization != null)
                    {
                        return authorization;
                    }
                }
            }
            return authorization;
        }

        internal static void BindModule(Uri uri, Authorization response, IAuthenticationModule module)
        {
            if (response.ProtectionRealm != null)
            {
                string[] protectionRealm = response.ProtectionRealm;
                for (int i = 0; i < protectionRealm.Length; i++)
                {
                    s_ModuleBinding.Add(protectionRealm[i], module.AuthenticationType);
                }
            }
            else
            {
                string prefix = generalize(uri);
                s_ModuleBinding.Add(prefix, module.AuthenticationType);
            }
        }

        internal static void EnsureConfigLoaded()
        {
            try
            {
                ArrayList moduleList = ModuleList;
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is OutOfMemoryException)) || (exception is StackOverflowException))
                {
                    throw;
                }
            }
        }

        private static IAuthenticationModule findModule(string authenticationType)
        {
            ArrayList moduleList = ModuleList;
            for (int i = 0; i < moduleList.Count; i++)
            {
                IAuthenticationModule module2 = (IAuthenticationModule) moduleList[i];
                if (string.Compare(module2.AuthenticationType, authenticationType, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return module2;
                }
            }
            return null;
        }

        internal static int FindSubstringNotInQuotes(string challenge, string signature)
        {
            int num = -1;
            if (((challenge != null) && (signature != null)) && (challenge.Length >= signature.Length))
            {
                int length = -1;
                int num3 = -1;
                for (int i = 0; (i < challenge.Length) && (num < 0); i++)
                {
                    if (challenge[i] == '"')
                    {
                        if (length <= num3)
                        {
                            length = i;
                        }
                        else
                        {
                            num3 = i;
                        }
                    }
                    if ((i == (challenge.Length - 1)) || ((challenge[i] == '"') && (length > num3)))
                    {
                        if (i == (challenge.Length - 1))
                        {
                            length = challenge.Length;
                        }
                        if (length >= (num3 + 3))
                        {
                            int start = num3 + 1;
                            int count = (length - num3) - 1;
                            do
                            {
                                num = IndexOf(challenge, signature, start, count);
                                if (num >= 0)
                                {
                                    if ((((num == 0) || (challenge[num - 1] == ' ')) || (challenge[num - 1] == ',')) && ((((num + signature.Length) == challenge.Length) || (challenge[num + signature.Length] == ' ')) || (challenge[num + signature.Length] == ',')))
                                    {
                                        break;
                                    }
                                    count -= (num - start) + 1;
                                    start = num + 1;
                                }
                            }
                            while (num >= 0);
                        }
                    }
                }
            }
            return num;
        }

        private static string generalize(Uri location)
        {
            string absoluteUri = location.AbsoluteUri;
            int num = absoluteUri.LastIndexOf('/');
            if (num < 0)
            {
                return absoluteUri;
            }
            return absoluteUri.Substring(0, num + 1);
        }

        internal static Authorization GetGroupAuthorization(IAuthenticationModule thisModule, string token, bool finished, NTAuthentication authSession, bool shareAuthenticatedConnections, bool mutualAuth)
        {
            return new Authorization(token, finished, shareAuthenticatedConnections ? null : (thisModule.GetType().FullName + "/" + authSession.UniqueUserId), mutualAuth);
        }

        private static int IndexOf(string challenge, string lwrCaseSignature, int start, int count)
        {
            count += (start + 1) - lwrCaseSignature.Length;
            while (start < count)
            {
                int num = 0;
                while (num < lwrCaseSignature.Length)
                {
                    if ((challenge[start + num] | ' ') != lwrCaseSignature[num])
                    {
                        break;
                    }
                    num++;
                }
                if (num == lwrCaseSignature.Length)
                {
                    return start;
                }
                start++;
            }
            return -1;
        }

        public static Authorization PreAuthenticate(WebRequest request, ICredentials credentials)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (credentials == null)
            {
                return null;
            }
            HttpWebRequest request2 = request as HttpWebRequest;
            if (request2 == null)
            {
                return null;
            }
            string authenticationType = s_ModuleBinding.Lookup(request2.ChallengedUri.AbsoluteUri) as string;
            if (authenticationType == null)
            {
                return null;
            }
            IAuthenticationModule module = findModule(authenticationType);
            if (module == null)
            {
                return null;
            }
            if (request2.ChallengedUri.Scheme == Uri.UriSchemeHttps)
            {
                ChannelBinding cachedChannelBinding = request2.ServicePoint.CachedChannelBinding as ChannelBinding;
                if (cachedChannelBinding != null)
                {
                    request2.CurrentAuthenticationState.TransportContext = new CachedTransportContext(cachedChannelBinding);
                }
            }
            Authorization authorization = module.PreAuthenticate(request, credentials);
            if (((authorization != null) && !authorization.Complete) && (request2 != null))
            {
                request2.CurrentAuthenticationState.Module = module;
            }
            return authorization;
        }

        public static void Register(IAuthenticationModule authenticationModule)
        {
            ExceptionHelper.UnmanagedPermission.Demand();
            if (authenticationModule == null)
            {
                throw new ArgumentNullException("authenticationModule");
            }
            lock (s_ModuleBinding)
            {
                IAuthenticationModule module = findModule(authenticationModule.AuthenticationType);
                if (module != null)
                {
                    ModuleList.Remove(module);
                }
                ModuleList.Add(authenticationModule);
            }
        }

        private static void RemoveAuthenticationType(ArrayList list, string typeToRemove)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Compare(((IAuthenticationModule) list[i]).AuthenticationType, typeToRemove, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        internal static int SplitNoQuotes(string challenge, ref int offset)
        {
            int num = offset;
            offset = -1;
            if ((challenge != null) && (num < challenge.Length))
            {
                int num2 = -1;
                int num3 = -1;
                for (int i = num; i < challenge.Length; i++)
                {
                    if (((num2 > num3) && (challenge[i] == '\\')) && (((i + 1) < challenge.Length) && (challenge[i + 1] == '"')))
                    {
                        i++;
                    }
                    else if (challenge[i] == '"')
                    {
                        if (num2 <= num3)
                        {
                            num2 = i;
                        }
                        else
                        {
                            num3 = i;
                        }
                    }
                    else if (((challenge[i] == '=') && (num2 <= num3)) && (offset < 0))
                    {
                        offset = i;
                    }
                    else if ((challenge[i] == ',') && (num2 <= num3))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static void Unregister(IAuthenticationModule authenticationModule)
        {
            ExceptionHelper.UnmanagedPermission.Demand();
            if (authenticationModule == null)
            {
                throw new ArgumentNullException("authenticationModule");
            }
            lock (s_ModuleBinding)
            {
                if (!ModuleList.Contains(authenticationModule))
                {
                    throw new InvalidOperationException(SR.GetString("net_authmodulenotregistered"));
                }
                ModuleList.Remove(authenticationModule);
            }
        }

        public static void Unregister(string authenticationScheme)
        {
            ExceptionHelper.UnmanagedPermission.Demand();
            if (authenticationScheme == null)
            {
                throw new ArgumentNullException("authenticationScheme");
            }
            lock (s_ModuleBinding)
            {
                IAuthenticationModule module = findModule(authenticationScheme);
                if (module == null)
                {
                    throw new InvalidOperationException(SR.GetString("net_authschemenotregistered"));
                }
                ModuleList.Remove(module);
            }
        }

        public static ICredentialPolicy CredentialPolicy
        {
            get
            {
                return s_ICredentialPolicy;
            }
            set
            {
                ExceptionHelper.ControlPolicyPermission.Demand();
                s_ICredentialPolicy = value;
            }
        }

        public static StringDictionary CustomTargetNameDictionary
        {
            get
            {
                return m_SpnDictionary;
            }
        }

        private static ArrayList ModuleList
        {
            get
            {
                if (s_ModuleList == null)
                {
                    lock (s_ModuleBinding)
                    {
                        if (s_ModuleList == null)
                        {
                            List<Type> authenticationModules = AuthenticationModulesSectionInternal.GetSection().AuthenticationModules;
                            ArrayList list = new ArrayList();
                            foreach (Type type in authenticationModules)
                            {
                                try
                                {
                                    IAuthenticationModule module = Activator.CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[0], CultureInfo.InvariantCulture) as IAuthenticationModule;
                                    if (module != null)
                                    {
                                        RemoveAuthenticationType(list, module.AuthenticationType);
                                        list.Add(module);
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                            s_ModuleList = list;
                        }
                    }
                }
                return s_ModuleList;
            }
        }

        internal static bool OSSupportsExtendedProtection
        {
            get
            {
                if (s_OSSupportsExtendedProtection == TriState.Unspecified)
                {
                    if (ComNetOS.IsWin7)
                    {
                        s_OSSupportsExtendedProtection = TriState.True;
                    }
                    else if (SspSupportsExtendedProtection)
                    {
                        if (UnsafeNclNativeMethods.HttpApi.ExtendedProtectionSupported)
                        {
                            s_OSSupportsExtendedProtection = TriState.True;
                        }
                        else
                        {
                            s_OSSupportsExtendedProtection = TriState.False;
                        }
                    }
                    else
                    {
                        s_OSSupportsExtendedProtection = TriState.False;
                    }
                }
                return (s_OSSupportsExtendedProtection == TriState.True);
            }
        }

        public static IEnumerator RegisteredModules
        {
            get
            {
                return ModuleList.GetEnumerator();
            }
        }

        internal static System.Net.SpnDictionary SpnDictionary
        {
            get
            {
                return m_SpnDictionary;
            }
        }

        internal static bool SspSupportsExtendedProtection
        {
            get
            {
                if (s_SspSupportsExtendedProtection == TriState.Unspecified)
                {
                    if (ComNetOS.IsWin7)
                    {
                        s_SspSupportsExtendedProtection = TriState.True;
                    }
                    else
                    {
                        ContextFlags requestedContextFlags = ContextFlags.AcceptIntegrity | ContextFlags.Connection;
                        NTAuthentication authentication = new NTAuthentication(false, "NTLM", SystemNetworkCredential.defaultCredential, "http/localhost", requestedContextFlags, null);
                        try
                        {
                            NTAuthentication authentication2 = new NTAuthentication(true, "NTLM", SystemNetworkCredential.defaultCredential, null, ContextFlags.Connection, null);
                            try
                            {
                                SecurityStatus status;
                                for (byte[] buffer = null; !authentication2.IsCompleted; buffer = authentication2.GetOutgoingBlob(buffer, true, out status))
                                {
                                    buffer = authentication.GetOutgoingBlob(buffer, true, out status);
                                }
                                if (authentication2.OSSupportsExtendedProtection)
                                {
                                    s_SspSupportsExtendedProtection = TriState.True;
                                }
                                else
                                {
                                    if (Logging.On)
                                    {
                                        Logging.PrintWarning(Logging.Web, SR.GetString("net_ssp_dont_support_cbt"));
                                    }
                                    s_SspSupportsExtendedProtection = TriState.False;
                                }
                            }
                            finally
                            {
                                authentication2.CloseContext();
                            }
                        }
                        finally
                        {
                            authentication.CloseContext();
                        }
                    }
                }
                return (s_SspSupportsExtendedProtection == TriState.True);
            }
        }
    }
}

