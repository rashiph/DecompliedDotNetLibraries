namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Xml;

    public class LdapConnection : DirectoryConnection, IDisposable
    {
        private static Hashtable asyncResultTable;
        internal bool automaticBind;
        private bool bounded;
        internal QUERYCLIENTCERT clientCertificateRoutine;
        private bool connected;
        private System.DirectoryServices.Protocols.AuthType connectionAuthType;
        internal bool disposed;
        private GetLdapResponseCallback fd;
        internal static Hashtable handleTable = new Hashtable();
        private const int LDAP_MOD_BVALUES = 0x80;
        internal IntPtr ldapHandle;
        internal bool needDispose;
        private bool needRebind;
        internal static object objectLock = new object();
        private LdapSessionOptions options;
        private static LdapPartialResultsProcessor partialResultsProcessor;
        private static PartialResultsRetriever retriever;
        private bool setFQDNDone;
        private static ManualResetEvent waitHandle;

        static LdapConnection()
        {
            Hashtable table = new Hashtable();
            asyncResultTable = Hashtable.Synchronized(table);
            waitHandle = new ManualResetEvent(false);
            partialResultsProcessor = new LdapPartialResultsProcessor(waitHandle);
            retriever = new PartialResultsRetriever(waitHandle, partialResultsProcessor);
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public LdapConnection(LdapDirectoryIdentifier identifier) : this(identifier, null, System.DirectoryServices.Protocols.AuthType.Negotiate)
        {
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public LdapConnection(string server) : this(new LdapDirectoryIdentifier(server))
        {
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public LdapConnection(LdapDirectoryIdentifier identifier, NetworkCredential credential) : this(identifier, credential, System.DirectoryServices.Protocols.AuthType.Negotiate)
        {
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        public LdapConnection(LdapDirectoryIdentifier identifier, NetworkCredential credential, System.DirectoryServices.Protocols.AuthType authType)
        {
            this.connectionAuthType = System.DirectoryServices.Protocols.AuthType.Negotiate;
            this.ldapHandle = IntPtr.Zero;
            this.automaticBind = true;
            this.needDispose = true;
            this.fd = new GetLdapResponseCallback(this.ConstructResponse);
            base.directoryIdentifier = identifier;
            base.directoryCredential = (credential != null) ? new NetworkCredential(credential.UserName, credential.Password, credential.Domain) : null;
            this.connectionAuthType = authType;
            if ((authType < System.DirectoryServices.Protocols.AuthType.Anonymous) || (authType > System.DirectoryServices.Protocols.AuthType.Kerberos))
            {
                throw new InvalidEnumArgumentException("authType", (int) authType, typeof(System.DirectoryServices.Protocols.AuthType));
            }
            if (((this.AuthType == System.DirectoryServices.Protocols.AuthType.Anonymous) && (base.directoryCredential != null)) && (((base.directoryCredential.Password != null) && (base.directoryCredential.Password.Length != 0)) || ((base.directoryCredential.UserName != null) && (base.directoryCredential.UserName.Length != 0))))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("InvalidAuthCredential"));
            }
            this.Init();
            this.options = new LdapSessionOptions(this);
            this.clientCertificateRoutine = new QUERYCLIENTCERT(this.ProcessClientCertificate);
        }

        internal LdapConnection(LdapDirectoryIdentifier identifier, NetworkCredential credential, System.DirectoryServices.Protocols.AuthType authType, IntPtr handle)
        {
            this.connectionAuthType = System.DirectoryServices.Protocols.AuthType.Negotiate;
            this.ldapHandle = IntPtr.Zero;
            this.automaticBind = true;
            this.needDispose = true;
            base.directoryIdentifier = identifier;
            this.ldapHandle = handle;
            base.directoryCredential = credential;
            this.connectionAuthType = authType;
            this.options = new LdapSessionOptions(this);
            this.needDispose = false;
            this.clientCertificateRoutine = new QUERYCLIENTCERT(this.ProcessClientCertificate);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public void Abort(IAsyncResult asyncResult)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (!(asyncResult is LdapAsyncResult))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("NotReturnedAsyncResult", new object[] { "asyncResult" }));
            }
            int messagId = -1;
            LdapAsyncResult result = (LdapAsyncResult) asyncResult;
            if (!result.partialResults)
            {
                if (!asyncResultTable.Contains(asyncResult))
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("InvalidAsyncResult"));
                }
                messagId = (int) asyncResultTable[asyncResult];
                asyncResultTable.Remove(asyncResult);
            }
            else
            {
                partialResultsProcessor.Remove((LdapPartialAsyncResult) asyncResult);
                messagId = ((LdapPartialAsyncResult) asyncResult).messageID;
            }
            Wldap32.ldap_abandon(this.ldapHandle, messagId);
            LdapRequestState resultObject = result.resultObject;
            if (resultObject != null)
            {
                resultObject.abortCalled = true;
            }
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public IAsyncResult BeginSendRequest(DirectoryRequest request, PartialResultProcessing partialMode, AsyncCallback callback, object state)
        {
            return this.BeginSendRequest(request, base.connectionTimeOut, partialMode, callback, state);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public IAsyncResult BeginSendRequest(DirectoryRequest request, TimeSpan requestTimeout, PartialResultProcessing partialMode, AsyncCallback callback, object state)
        {
            int messageID = 0;
            int error = 0;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if ((partialMode < PartialResultProcessing.NoPartialResultSupport) || (partialMode > PartialResultProcessing.ReturnPartialResultsAndNotifyCallback))
            {
                throw new InvalidEnumArgumentException("partialMode", (int) partialMode, typeof(PartialResultProcessing));
            }
            if ((partialMode != PartialResultProcessing.NoPartialResultSupport) && !(request is SearchRequest))
            {
                throw new NotSupportedException(System.DirectoryServices.Protocols.Res.GetString("PartialResultsNotSupported"));
            }
            if ((partialMode == PartialResultProcessing.ReturnPartialResultsAndNotifyCallback) && (callback == null))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("CallBackIsNull"), "callback");
            }
            error = this.SendRequestHelper(request, ref messageID);
            LdapOperation ldapSearch = LdapOperation.LdapSearch;
            if (request is DeleteRequest)
            {
                ldapSearch = LdapOperation.LdapDelete;
            }
            else if (request is AddRequest)
            {
                ldapSearch = LdapOperation.LdapAdd;
            }
            else if (request is ModifyRequest)
            {
                ldapSearch = LdapOperation.LdapModify;
            }
            else if (request is SearchRequest)
            {
                ldapSearch = LdapOperation.LdapSearch;
            }
            else if (request is ModifyDNRequest)
            {
                ldapSearch = LdapOperation.LdapModifyDn;
            }
            else if (request is CompareRequest)
            {
                ldapSearch = LdapOperation.LdapCompare;
            }
            else if (request is ExtendedRequest)
            {
                ldapSearch = LdapOperation.LdapExtendedRequest;
            }
            if ((error == 0) && (messageID != -1))
            {
                if (partialMode == PartialResultProcessing.NoPartialResultSupport)
                {
                    LdapRequestState state2 = new LdapRequestState();
                    LdapAsyncResult key = new LdapAsyncResult(callback, state, false);
                    state2.ldapAsync = key;
                    key.resultObject = state2;
                    asyncResultTable.Add(key, messageID);
                    this.fd.BeginInvoke(messageID, ldapSearch, ResultAll.LDAP_MSG_ALL, requestTimeout, true, new AsyncCallback(this.ResponseCallback), state2);
                    return key;
                }
                bool partialCallback = false;
                if (partialMode == PartialResultProcessing.ReturnPartialResultsAndNotifyCallback)
                {
                    partialCallback = true;
                }
                LdapPartialAsyncResult asyncResult = new LdapPartialAsyncResult(messageID, callback, state, true, this, partialCallback, requestTimeout);
                partialResultsProcessor.Add(asyncResult);
                return asyncResult;
            }
            if (error == 0)
            {
                error = Wldap32.LdapGetLastError();
            }
            throw this.ConstructException(error, ldapSearch);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public void Bind()
        {
            this.BindHelper(base.directoryCredential, false);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public void Bind(NetworkCredential newCredential)
        {
            this.BindHelper(newCredential, true);
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        private void BindHelper(NetworkCredential newCredential, bool needSetCredential)
        {
            string str2;
            string str3;
            string str4;
            int errorCode = 0;
            NetworkCredential directoryCredential = null;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (((this.AuthType == System.DirectoryServices.Protocols.AuthType.Anonymous) && (newCredential != null)) && (((newCredential.Password != null) && (newCredential.Password.Length != 0)) || ((newCredential.UserName != null) && (newCredential.UserName.Length != 0))))
            {
                throw new InvalidOperationException(System.DirectoryServices.Protocols.Res.GetString("InvalidAuthCredential"));
            }
            if (needSetCredential)
            {
                base.directoryCredential = directoryCredential = (newCredential != null) ? new NetworkCredential(newCredential.UserName, newCredential.Password, newCredential.Domain) : null;
            }
            else
            {
                directoryCredential = base.directoryCredential;
            }
            if (!this.connected)
            {
                this.Connect();
                this.connected = true;
            }
            if (((directoryCredential != null) && (directoryCredential.UserName.Length == 0)) && ((directoryCredential.Password.Length == 0) && (directoryCredential.Domain.Length == 0)))
            {
                str2 = null;
                str3 = null;
                str4 = null;
            }
            else
            {
                str2 = (directoryCredential == null) ? null : directoryCredential.UserName;
                str3 = (directoryCredential == null) ? null : directoryCredential.Domain;
                str4 = (directoryCredential == null) ? null : directoryCredential.Password;
            }
            if (this.AuthType == System.DirectoryServices.Protocols.AuthType.Anonymous)
            {
                errorCode = Wldap32.ldap_simple_bind_s(this.ldapHandle, null, null);
            }
            else if (this.AuthType == System.DirectoryServices.Protocols.AuthType.Basic)
            {
                StringBuilder builder = new StringBuilder(100);
                if ((str3 != null) && (str3.Length != 0))
                {
                    builder.Append(str3);
                    builder.Append(@"\");
                }
                builder.Append(str2);
                errorCode = Wldap32.ldap_simple_bind_s(this.ldapHandle, builder.ToString(), str4);
            }
            else
            {
                SEC_WINNT_AUTH_IDENTITY_EX credentials = new SEC_WINNT_AUTH_IDENTITY_EX {
                    version = 0x200,
                    length = Marshal.SizeOf(typeof(SEC_WINNT_AUTH_IDENTITY_EX)),
                    flags = 2
                };
                if (this.AuthType == System.DirectoryServices.Protocols.AuthType.Kerberos)
                {
                    credentials.packageList = "Kerberos";
                    credentials.packageListLength = credentials.packageList.Length;
                }
                if (directoryCredential != null)
                {
                    credentials.user = str2;
                    credentials.userLength = (str2 == null) ? 0 : str2.Length;
                    credentials.domain = str3;
                    credentials.domainLength = (str3 == null) ? 0 : str3.Length;
                    credentials.password = str4;
                    credentials.passwordLength = (str4 == null) ? 0 : str4.Length;
                }
                BindMethod method = BindMethod.LDAP_AUTH_NEGOTIATE;
                switch (this.AuthType)
                {
                    case System.DirectoryServices.Protocols.AuthType.Negotiate:
                        method = BindMethod.LDAP_AUTH_NEGOTIATE;
                        break;

                    case System.DirectoryServices.Protocols.AuthType.Ntlm:
                        method = BindMethod.LDAP_AUTH_NTLM;
                        break;

                    case System.DirectoryServices.Protocols.AuthType.Digest:
                        method = BindMethod.LDAP_AUTH_DIGEST;
                        break;

                    case System.DirectoryServices.Protocols.AuthType.Sicily:
                        method = BindMethod.LDAP_AUTH_SICILY;
                        break;

                    case System.DirectoryServices.Protocols.AuthType.Dpa:
                        method = BindMethod.LDAP_AUTH_DPA;
                        break;

                    case System.DirectoryServices.Protocols.AuthType.Msn:
                        method = BindMethod.LDAP_AUTH_MSN;
                        break;

                    case System.DirectoryServices.Protocols.AuthType.External:
                        method = BindMethod.LDAP_AUTH_EXTERNAL;
                        break;

                    case System.DirectoryServices.Protocols.AuthType.Kerberos:
                        method = BindMethod.LDAP_AUTH_NEGOTIATE;
                        break;
                }
                if ((directoryCredential == null) && (this.AuthType == System.DirectoryServices.Protocols.AuthType.External))
                {
                    errorCode = Wldap32.ldap_bind_s(this.ldapHandle, null, null, method);
                }
                else
                {
                    errorCode = Wldap32.ldap_bind_s(this.ldapHandle, null, credentials, method);
                }
            }
            if (errorCode != 0)
            {
                string str;
                if (Utility.IsResultCode((ResultCode) errorCode))
                {
                    str = OperationErrorMappings.MapResultCode(errorCode);
                    throw new DirectoryOperationException(null, str);
                }
                if (!Utility.IsLdapError((LdapError) errorCode))
                {
                    throw new LdapException(errorCode);
                }
                str = LdapErrorMappings.MapResultCode(errorCode);
                string serverErrorMessage = this.options.ServerErrorMessage;
                if ((serverErrorMessage != null) && (serverErrorMessage.Length > 0))
                {
                    throw new LdapException(errorCode, str, serverErrorMessage);
                }
                throw new LdapException(errorCode, str);
            }
            this.bounded = true;
            this.needRebind = false;
        }

        internal LdapMod[] BuildAttributes(CollectionBase directoryAttributes, ArrayList ptrToFree)
        {
            LdapMod[] modArray = null;
            UTF8Encoding encoding = new UTF8Encoding();
            DirectoryAttributeCollection attributes = null;
            DirectoryAttributeModificationCollection modifications = null;
            DirectoryAttribute attribute = null;
            if ((directoryAttributes != null) && (directoryAttributes.Count != 0))
            {
                if (directoryAttributes is DirectoryAttributeModificationCollection)
                {
                    modifications = (DirectoryAttributeModificationCollection) directoryAttributes;
                }
                else
                {
                    attributes = (DirectoryAttributeCollection) directoryAttributes;
                }
                modArray = new LdapMod[directoryAttributes.Count];
                for (int i = 0; i < directoryAttributes.Count; i++)
                {
                    if (attributes != null)
                    {
                        attribute = attributes[i];
                    }
                    else
                    {
                        attribute = modifications[i];
                    }
                    modArray[i] = new LdapMod();
                    if (attribute is DirectoryAttributeModification)
                    {
                        modArray[i].type = (int) ((DirectoryAttributeModification) attribute).Operation;
                    }
                    else
                    {
                        modArray[i].type = 0;
                    }
                    LdapMod mod1 = modArray[i];
                    mod1.type |= 0x80;
                    modArray[i].attribute = Marshal.StringToHGlobalUni(attribute.Name);
                    int count = 0;
                    berval[] bervalArray = null;
                    if (attribute.Count > 0)
                    {
                        count = attribute.Count;
                        bervalArray = new berval[count];
                        for (int j = 0; j < count; j++)
                        {
                            byte[] source = null;
                            if (attribute[j] is string)
                            {
                                source = encoding.GetBytes((string) attribute[j]);
                            }
                            else if (attribute[j] is Uri)
                            {
                                source = encoding.GetBytes(((Uri) attribute[j]).ToString());
                            }
                            else
                            {
                                source = (byte[]) attribute[j];
                            }
                            bervalArray[j] = new berval();
                            bervalArray[j].bv_len = source.Length;
                            bervalArray[j].bv_val = Marshal.AllocHGlobal(bervalArray[j].bv_len);
                            ptrToFree.Add(bervalArray[j].bv_val);
                            Marshal.Copy(source, 0, bervalArray[j].bv_val, bervalArray[j].bv_len);
                        }
                    }
                    modArray[i].values = Marshal.AllocHGlobal((int) ((count + 1) * Marshal.SizeOf(typeof(IntPtr))));
                    int cb = Marshal.SizeOf(typeof(berval));
                    IntPtr zero = IntPtr.Zero;
                    IntPtr ptr = IntPtr.Zero;
                    int index = 0;
                    index = 0;
                    while (index < count)
                    {
                        zero = Marshal.AllocHGlobal(cb);
                        ptrToFree.Add(zero);
                        Marshal.StructureToPtr(bervalArray[index], zero, false);
                        ptr = (IntPtr) (((long) modArray[i].values) + (Marshal.SizeOf(typeof(IntPtr)) * index));
                        Marshal.WriteIntPtr(ptr, zero);
                        index++;
                    }
                    ptr = (IntPtr) (((long) modArray[i].values) + (Marshal.SizeOf(typeof(IntPtr)) * index));
                    Marshal.WriteIntPtr(ptr, IntPtr.Zero);
                }
            }
            return modArray;
        }

        internal LdapControl[] BuildControlArray(DirectoryControlCollection controls, bool serverControl)
        {
            int count = 0;
            LdapControl[] controlArray = null;
            if ((controls != null) && (controls.Count != 0))
            {
                ArrayList list = new ArrayList();
                foreach (DirectoryControl control in controls)
                {
                    if (serverControl)
                    {
                        if (control.ServerSide)
                        {
                            list.Add(control);
                        }
                    }
                    else if (!control.ServerSide)
                    {
                        list.Add(control);
                    }
                }
                if (list.Count == 0)
                {
                    return controlArray;
                }
                count = list.Count;
                controlArray = new LdapControl[count];
                for (int i = 0; i < count; i++)
                {
                    controlArray[i] = new LdapControl();
                    controlArray[i].ldctl_oid = Marshal.StringToHGlobalUni(((DirectoryControl) list[i]).Type);
                    controlArray[i].ldctl_iscritical = ((DirectoryControl) list[i]).IsCritical;
                    byte[] source = ((DirectoryControl) list[i]).GetValue();
                    if ((source == null) || (source.Length == 0))
                    {
                        controlArray[i].ldctl_value = new berval();
                        controlArray[i].ldctl_value.bv_len = 0;
                        controlArray[i].ldctl_value.bv_val = IntPtr.Zero;
                    }
                    else
                    {
                        controlArray[i].ldctl_value = new berval();
                        controlArray[i].ldctl_value.bv_len = source.Length;
                        controlArray[i].ldctl_value.bv_val = Marshal.AllocHGlobal((int) (Marshal.SizeOf(typeof(byte)) * controlArray[i].ldctl_value.bv_len));
                        Marshal.Copy(source, 0, controlArray[i].ldctl_value.bv_val, controlArray[i].ldctl_value.bv_len);
                    }
                }
            }
            return controlArray;
        }

        private void Connect()
        {
            int errorCode = 0;
            if (base.ClientCertificates.Count > 1)
            {
                throw new InvalidOperationException(System.DirectoryServices.Protocols.Res.GetString("InvalidClientCertificates"));
            }
            if (base.ClientCertificates.Count != 0)
            {
                int num2 = Wldap32.ldap_set_option_clientcert(this.ldapHandle, LdapOption.LDAP_OPT_CLIENT_CERTIFICATE, this.clientCertificateRoutine);
                if (num2 != 0)
                {
                    if (Utility.IsLdapError((LdapError) num2))
                    {
                        string message = LdapErrorMappings.MapResultCode(num2);
                        throw new LdapException(num2, message);
                    }
                    throw new LdapException(num2);
                }
                this.automaticBind = false;
            }
            if (((LdapDirectoryIdentifier) this.Directory).FullyQualifiedDnsHostName && !this.setFQDNDone)
            {
                this.SessionOptions.FQDN = true;
                this.setFQDNDone = true;
            }
            LDAP_TIMEVAL timeout = new LDAP_TIMEVAL {
                tv_sec = (int) (this.connectionTimeOut.Ticks / 0x989680L)
            };
            errorCode = Wldap32.ldap_connect(this.ldapHandle, timeout);
            if (errorCode != 0)
            {
                if (Utility.IsLdapError((LdapError) errorCode))
                {
                    string str = LdapErrorMappings.MapResultCode(errorCode);
                    throw new LdapException(errorCode, str);
                }
                throw new LdapException(errorCode);
            }
        }

        internal DirectoryAttribute ConstructAttribute(IntPtr entryMessage, IntPtr attributeName)
        {
            DirectoryAttribute attribute = new DirectoryAttribute {
                isSearchResult = true
            };
            string name = Marshal.PtrToStringUni(attributeName);
            attribute.Name = name;
            IntPtr ptr = Wldap32.ldap_get_values_len(this.ldapHandle, entryMessage, name);
            try
            {
                IntPtr zero = IntPtr.Zero;
                int num = 0;
                if (!(ptr != IntPtr.Zero))
                {
                    return attribute;
                }
                for (zero = Marshal.ReadIntPtr(ptr, Marshal.SizeOf(typeof(IntPtr)) * num); zero != IntPtr.Zero; zero = Marshal.ReadIntPtr(ptr, Marshal.SizeOf(typeof(IntPtr)) * num))
                {
                    berval structure = new berval();
                    Marshal.PtrToStructure(zero, structure);
                    byte[] destination = null;
                    if ((structure.bv_len > 0) && (structure.bv_val != IntPtr.Zero))
                    {
                        destination = new byte[structure.bv_len];
                        Marshal.Copy(structure.bv_val, destination, 0, structure.bv_len);
                        attribute.Add(destination);
                    }
                    num++;
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Wldap32.ldap_value_free_len(ptr);
                }
            }
            return attribute;
        }

        private DirectoryControl ConstructControl(IntPtr controlPtr)
        {
            LdapControl structure = new LdapControl();
            Marshal.PtrToStructure(controlPtr, structure);
            string type = Marshal.PtrToStringUni(structure.ldctl_oid);
            byte[] destination = new byte[structure.ldctl_value.bv_len];
            Marshal.Copy(structure.ldctl_value.bv_val, destination, 0, structure.ldctl_value.bv_len);
            return new DirectoryControl(type, destination, structure.ldctl_iscritical, true);
        }

        internal SearchResultEntry ConstructEntry(IntPtr entryMessage)
        {
            SearchResultEntry entry2;
            IntPtr zero = IntPtr.Zero;
            string dn = null;
            IntPtr attributeName = IntPtr.Zero;
            IntPtr address = IntPtr.Zero;
            SearchResultAttributeCollection attributes = null;
            try
            {
                zero = Wldap32.ldap_get_dn(this.ldapHandle, entryMessage);
                if (zero != IntPtr.Zero)
                {
                    dn = Marshal.PtrToStringUni(zero);
                    Wldap32.ldap_memfree(zero);
                    zero = IntPtr.Zero;
                }
                SearchResultEntry entry = new SearchResultEntry(dn);
                attributes = entry.Attributes;
                attributeName = Wldap32.ldap_first_attribute(this.ldapHandle, entryMessage, ref address);
                int num = 0;
                while (attributeName != IntPtr.Zero)
                {
                    DirectoryAttribute attribute = this.ConstructAttribute(entryMessage, attributeName);
                    attributes.Add(attribute.Name, attribute);
                    Wldap32.ldap_memfree(attributeName);
                    num++;
                    attributeName = Wldap32.ldap_next_attribute(this.ldapHandle, entryMessage, address);
                }
                if (address != IntPtr.Zero)
                {
                    Wldap32.ber_free(address, 0);
                    address = IntPtr.Zero;
                }
                entry2 = entry;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Wldap32.ldap_memfree(zero);
                }
                if (attributeName != IntPtr.Zero)
                {
                    Wldap32.ldap_memfree(attributeName);
                }
                if (address != IntPtr.Zero)
                {
                    Wldap32.ber_free(address, 0);
                }
            }
            return entry2;
        }

        private DirectoryException ConstructException(int error, LdapOperation operation)
        {
            DirectoryResponse response = null;
            if (Utility.IsResultCode((ResultCode) error))
            {
                if (operation == LdapOperation.LdapAdd)
                {
                    response = new AddResponse(null, null, (ResultCode) error, OperationErrorMappings.MapResultCode(error), null);
                }
                else if (operation == LdapOperation.LdapModify)
                {
                    response = new ModifyResponse(null, null, (ResultCode) error, OperationErrorMappings.MapResultCode(error), null);
                }
                else if (operation == LdapOperation.LdapDelete)
                {
                    response = new DeleteResponse(null, null, (ResultCode) error, OperationErrorMappings.MapResultCode(error), null);
                }
                else if (operation == LdapOperation.LdapModifyDn)
                {
                    response = new ModifyDNResponse(null, null, (ResultCode) error, OperationErrorMappings.MapResultCode(error), null);
                }
                else if (operation == LdapOperation.LdapCompare)
                {
                    response = new CompareResponse(null, null, (ResultCode) error, OperationErrorMappings.MapResultCode(error), null);
                }
                else if (operation == LdapOperation.LdapSearch)
                {
                    response = new SearchResponse(null, null, (ResultCode) error, OperationErrorMappings.MapResultCode(error), null);
                }
                else if (operation == LdapOperation.LdapExtendedRequest)
                {
                    response = new ExtendedResponse(null, null, (ResultCode) error, OperationErrorMappings.MapResultCode(error), null);
                }
                return new DirectoryOperationException(response, OperationErrorMappings.MapResultCode(error));
            }
            if (!Utility.IsLdapError((LdapError) error))
            {
                return new LdapException(error);
            }
            string message = LdapErrorMappings.MapResultCode(error);
            string serverErrorMessage = this.options.ServerErrorMessage;
            if ((serverErrorMessage != null) && (serverErrorMessage.Length > 0))
            {
                throw new LdapException(error, message, serverErrorMessage);
            }
            return new LdapException(error, message);
        }

        internal unsafe int ConstructParsedResult(IntPtr ldapResult, ref int serverError, ref string responseDn, ref string responseMessage, ref Uri[] responseReferral, ref DirectoryControl[] responseControl)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr message = IntPtr.Zero;
            IntPtr referral = IntPtr.Zero;
            IntPtr ptr4 = IntPtr.Zero;
            int num = 0;
            try
            {
                num = Wldap32.ldap_parse_result(this.ldapHandle, ldapResult, ref serverError, ref zero, ref message, ref referral, ref ptr4, 0);
                switch (num)
                {
                    case 0:
                        responseDn = Marshal.PtrToStringUni(zero);
                        responseMessage = Marshal.PtrToStringUni(message);
                        if (referral != IntPtr.Zero)
                        {
                            char** chPtr = (char**) referral;
                            char* chPtr2 = chPtr[0];
                            int index = 0;
                            ArrayList list = new ArrayList();
                            while (chPtr2 != null)
                            {
                                string str = Marshal.PtrToStringUni((IntPtr) chPtr2);
                                list.Add(str);
                                index++;
                                chPtr2 = chPtr[index];
                            }
                            if (list.Count > 0)
                            {
                                responseReferral = new Uri[list.Count];
                                for (int i = 0; i < list.Count; i++)
                                {
                                    responseReferral[i] = new Uri((string) list[i]);
                                }
                            }
                        }
                        if (ptr4 != IntPtr.Zero)
                        {
                            int num4 = 0;
                            IntPtr ptr = ptr4;
                            IntPtr controlPtr = Marshal.ReadIntPtr(ptr, 0);
                            ArrayList list2 = new ArrayList();
                            while (controlPtr != IntPtr.Zero)
                            {
                                DirectoryControl control = this.ConstructControl(controlPtr);
                                list2.Add(control);
                                num4++;
                                controlPtr = Marshal.ReadIntPtr(ptr, num4 * Marshal.SizeOf(typeof(IntPtr)));
                            }
                            responseControl = new DirectoryControl[list2.Count];
                            list2.CopyTo(responseControl);
                        }
                        return num;

                    case 0x52:
                    {
                        int num5 = Wldap32.ldap_result2error(this.ldapHandle, ldapResult, 0);
                        if (num5 != 0)
                        {
                            num = num5;
                        }
                        break;
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Wldap32.ldap_memfree(zero);
                }
                if (message != IntPtr.Zero)
                {
                    Wldap32.ldap_memfree(message);
                }
                if (referral != IntPtr.Zero)
                {
                    Wldap32.ldap_value_free(referral);
                }
                if (ptr4 != IntPtr.Zero)
                {
                    Wldap32.ldap_controls_free(ptr4);
                }
            }
            return num;
        }

        internal SearchResultReference ConstructReference(IntPtr referenceMessage)
        {
            SearchResultReference reference = null;
            ArrayList list = new ArrayList();
            Uri[] uris = null;
            IntPtr zero = IntPtr.Zero;
            int num = Wldap32.ldap_parse_reference(this.ldapHandle, referenceMessage, ref zero);
            try
            {
                if (num != 0)
                {
                    return reference;
                }
                IntPtr ptr = IntPtr.Zero;
                int num2 = 0;
                if (zero != IntPtr.Zero)
                {
                    for (ptr = Marshal.ReadIntPtr(zero, Marshal.SizeOf(typeof(IntPtr)) * num2); ptr != IntPtr.Zero; ptr = Marshal.ReadIntPtr(zero, Marshal.SizeOf(typeof(IntPtr)) * num2))
                    {
                        string str = Marshal.PtrToStringUni(ptr);
                        list.Add(str);
                        num2++;
                    }
                    Wldap32.ldap_value_free(zero);
                    zero = IntPtr.Zero;
                }
                if (list.Count <= 0)
                {
                    return reference;
                }
                uris = new Uri[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    uris[i] = new Uri((string) list[i]);
                }
                reference = new SearchResultReference(uris);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Wldap32.ldap_value_free(zero);
                }
            }
            return reference;
        }

        internal DirectoryResponse ConstructResponse(int messageId, LdapOperation operation, ResultAll resultType, TimeSpan requestTimeOut, bool exceptionOnTimeOut)
        {
            LDAP_TIMEVAL timeout = new LDAP_TIMEVAL {
                tv_sec = (int) (requestTimeOut.Ticks / 0x989680L)
            };
            IntPtr zero = IntPtr.Zero;
            DirectoryResponse response = null;
            IntPtr oid = IntPtr.Zero;
            IntPtr data = IntPtr.Zero;
            IntPtr entryMessage = IntPtr.Zero;
            bool flag = true;
            if (resultType != ResultAll.LDAP_MSG_ALL)
            {
                timeout.tv_sec = 0;
                timeout.tv_usec = 0;
                if (resultType == ResultAll.LDAP_MSG_POLLINGALL)
                {
                    resultType = ResultAll.LDAP_MSG_ALL;
                }
                flag = false;
            }
            int error = Wldap32.ldap_result(this.ldapHandle, messageId, (int) resultType, timeout, ref zero);
            switch (error)
            {
                case -1:
                case 0:
                    break;

                default:
                {
                    int serverError = 0;
                    try
                    {
                        int errorCode = 0;
                        string responseDn = null;
                        string responseMessage = null;
                        Uri[] responseReferral = null;
                        DirectoryControl[] responseControl = null;
                        if ((error != 100) && (error != 0x73))
                        {
                            errorCode = this.ConstructParsedResult(zero, ref serverError, ref responseDn, ref responseMessage, ref responseReferral, ref responseControl);
                        }
                        if (errorCode == 0)
                        {
                            errorCode = serverError;
                            switch (error)
                            {
                                case 0x69:
                                    response = new AddResponse(responseDn, responseControl, (ResultCode) errorCode, responseMessage, responseReferral);
                                    break;

                                case 0x67:
                                    response = new ModifyResponse(responseDn, responseControl, (ResultCode) errorCode, responseMessage, responseReferral);
                                    break;

                                case 0x6b:
                                    response = new DeleteResponse(responseDn, responseControl, (ResultCode) errorCode, responseMessage, responseReferral);
                                    break;

                                case 0x6d:
                                    response = new ModifyDNResponse(responseDn, responseControl, (ResultCode) errorCode, responseMessage, responseReferral);
                                    break;

                                case 0x6f:
                                    response = new CompareResponse(responseDn, responseControl, (ResultCode) errorCode, responseMessage, responseReferral);
                                    break;

                                case 120:
                                    response = new ExtendedResponse(responseDn, responseControl, (ResultCode) errorCode, responseMessage, responseReferral);
                                    if (errorCode == 0)
                                    {
                                        errorCode = Wldap32.ldap_parse_extended_result(this.ldapHandle, zero, ref oid, ref data, 0);
                                        if (errorCode == 0)
                                        {
                                            string str3 = null;
                                            if (oid != IntPtr.Zero)
                                            {
                                                str3 = Marshal.PtrToStringUni(oid);
                                            }
                                            berval structure = null;
                                            byte[] destination = null;
                                            if (data != IntPtr.Zero)
                                            {
                                                structure = new berval();
                                                Marshal.PtrToStructure(data, structure);
                                                if ((structure.bv_len != 0) && (structure.bv_val != IntPtr.Zero))
                                                {
                                                    destination = new byte[structure.bv_len];
                                                    Marshal.Copy(structure.bv_val, destination, 0, structure.bv_len);
                                                }
                                            }
                                            ((ExtendedResponse) response).name = str3;
                                            ((ExtendedResponse) response).value = destination;
                                        }
                                    }
                                    break;

                                case 0x65:
                                case 100:
                                case 0x73:
                                {
                                    response = new SearchResponse(responseDn, responseControl, (ResultCode) errorCode, responseMessage, responseReferral);
                                    if (error == 0x65)
                                    {
                                        ((SearchResponse) response).searchDone = true;
                                    }
                                    SearchResultEntryCollection col = new SearchResultEntryCollection();
                                    SearchResultReferenceCollection references = new SearchResultReferenceCollection();
                                    entryMessage = Wldap32.ldap_first_entry(this.ldapHandle, zero);
                                    int num4 = 0;
                                    while (entryMessage != IntPtr.Zero)
                                    {
                                        SearchResultEntry entry = this.ConstructEntry(entryMessage);
                                        if (entry != null)
                                        {
                                            col.Add(entry);
                                        }
                                        num4++;
                                        entryMessage = Wldap32.ldap_next_entry(this.ldapHandle, entryMessage);
                                    }
                                    for (IntPtr ptr5 = Wldap32.ldap_first_reference(this.ldapHandle, zero); ptr5 != IntPtr.Zero; ptr5 = Wldap32.ldap_next_reference(this.ldapHandle, ptr5))
                                    {
                                        SearchResultReference reference = this.ConstructReference(ptr5);
                                        if (reference != null)
                                        {
                                            references.Add(reference);
                                        }
                                    }
                                    ((SearchResponse) response).SetEntries(col);
                                    ((SearchResponse) response).SetReferences(references);
                                    break;
                                }
                            }
                            switch (errorCode)
                            {
                                case 0:
                                case 5:
                                case 6:
                                case 10:
                                case 9:
                                    return response;

                                default:
                                    if (Utility.IsResultCode((ResultCode) errorCode))
                                    {
                                        throw new DirectoryOperationException(response, OperationErrorMappings.MapResultCode(errorCode));
                                    }
                                    throw new DirectoryOperationException(response);
                            }
                        }
                        error = errorCode;
                        goto Label_03A7;
                    }
                    finally
                    {
                        if (oid != IntPtr.Zero)
                        {
                            Wldap32.ldap_memfree(oid);
                        }
                        if (data != IntPtr.Zero)
                        {
                            Wldap32.ldap_memfree(data);
                        }
                        if (zero != IntPtr.Zero)
                        {
                            Wldap32.ldap_msgfree(zero);
                        }
                    }
                    break;
                }
            }
            if (error == 0)
            {
                if (!exceptionOnTimeOut)
                {
                    return null;
                }
                error = 0x55;
            }
            else
            {
                error = Wldap32.LdapGetLastError();
            }
            if (flag)
            {
                Wldap32.ldap_abandon(this.ldapHandle, messageId);
            }
        Label_03A7:
            throw this.ConstructException(error, operation);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (objectLock)
                {
                    handleTable.Remove(this.ldapHandle);
                }
            }
            if (this.needDispose && (this.ldapHandle != IntPtr.Zero))
            {
                Wldap32.ldap_unbind(this.ldapHandle);
            }
            this.ldapHandle = IntPtr.Zero;
            this.disposed = true;
        }

        public DirectoryResponse EndSendRequest(IAsyncResult asyncResult)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (!(asyncResult is LdapAsyncResult))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("NotReturnedAsyncResult", new object[] { "asyncResult" }));
            }
            LdapAsyncResult result = (LdapAsyncResult) asyncResult;
            if (!result.partialResults)
            {
                if (!asyncResultTable.Contains(asyncResult))
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("InvalidAsyncResult"));
                }
                asyncResultTable.Remove(asyncResult);
                asyncResult.AsyncWaitHandle.WaitOne();
                if (result.resultObject.exception != null)
                {
                    throw result.resultObject.exception;
                }
                return result.resultObject.response;
            }
            partialResultsProcessor.NeedCompleteResult((LdapPartialAsyncResult) asyncResult);
            asyncResult.AsyncWaitHandle.WaitOne();
            return partialResultsProcessor.GetCompleteResult((LdapPartialAsyncResult) asyncResult);
        }

        ~LdapConnection()
        {
            this.Dispose(false);
        }

        public PartialResultsCollection GetPartialResults(IAsyncResult asyncResult)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (!(asyncResult is LdapAsyncResult))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("NotReturnedAsyncResult", new object[] { "asyncResult" }));
            }
            if (!(asyncResult is LdapPartialAsyncResult))
            {
                throw new InvalidOperationException(System.DirectoryServices.Protocols.Res.GetString("NoPartialResults"));
            }
            return partialResultsProcessor.GetPartialResults((LdapPartialAsyncResult) asyncResult);
        }

        internal void Init()
        {
            string hostName = null;
            string[] strArray = (base.directoryIdentifier == null) ? null : ((LdapDirectoryIdentifier) base.directoryIdentifier).Servers;
            if ((strArray != null) && (strArray.Length != 0))
            {
                StringBuilder builder = new StringBuilder(200);
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (strArray[i] != null)
                    {
                        builder.Append(strArray[i]);
                        if (i < (strArray.Length - 1))
                        {
                            builder.Append(" ");
                        }
                    }
                }
                if (builder.Length != 0)
                {
                    hostName = builder.ToString();
                }
            }
            if (((LdapDirectoryIdentifier) base.directoryIdentifier).Connectionless)
            {
                this.ldapHandle = Wldap32.cldap_open(hostName, ((LdapDirectoryIdentifier) base.directoryIdentifier).PortNumber);
            }
            else
            {
                this.ldapHandle = Wldap32.ldap_init(hostName, ((LdapDirectoryIdentifier) base.directoryIdentifier).PortNumber);
            }
            if (this.ldapHandle == IntPtr.Zero)
            {
                int errorCode = Wldap32.LdapGetLastError();
                if (Utility.IsLdapError((LdapError) errorCode))
                {
                    string message = LdapErrorMappings.MapResultCode(errorCode);
                    throw new LdapException(errorCode, message);
                }
                throw new LdapException(errorCode);
            }
            lock (objectLock)
            {
                if (handleTable[this.ldapHandle] != null)
                {
                    handleTable.Remove(this.ldapHandle);
                }
                handleTable.Add(this.ldapHandle, new WeakReference(this));
            }
        }

        private bool ProcessClientCertificate(IntPtr ldapHandle, IntPtr CAs, ref IntPtr certificate)
        {
            ArrayList list = new ArrayList();
            byte[][] trustedCAs = null;
            if ((((base.ClientCertificates == null) ? 0 : base.ClientCertificates.Count) != 0) || (this.options.clientCertificateDelegate != null))
            {
                if (this.options.clientCertificateDelegate == null)
                {
                    certificate = base.ClientCertificates[0].Handle;
                    return true;
                }
                if (CAs != IntPtr.Zero)
                {
                    SecPkgContext_IssuerListInfoEx ex = (SecPkgContext_IssuerListInfoEx) Marshal.PtrToStructure(CAs, typeof(SecPkgContext_IssuerListInfoEx));
                    int cIssuers = ex.cIssuers;
                    IntPtr zero = IntPtr.Zero;
                    for (int i = 0; i < cIssuers; i++)
                    {
                        zero = (IntPtr) (((long) ex.aIssuers) + (Marshal.SizeOf(typeof(CRYPTOAPI_BLOB)) * i));
                        CRYPTOAPI_BLOB cryptoapi_blob = (CRYPTOAPI_BLOB) Marshal.PtrToStructure(zero, typeof(CRYPTOAPI_BLOB));
                        int cbData = cryptoapi_blob.cbData;
                        byte[] destination = new byte[cbData];
                        Marshal.Copy(cryptoapi_blob.pbData, destination, 0, cbData);
                        list.Add(destination);
                    }
                }
                if (list.Count != 0)
                {
                    trustedCAs = new byte[list.Count][];
                    for (int j = 0; j < list.Count; j++)
                    {
                        trustedCAs[j] = (byte[]) list[j];
                    }
                }
                X509Certificate certificate2 = this.options.clientCertificateDelegate(this, trustedCAs);
                if (certificate2 != null)
                {
                    certificate = certificate2.Handle;
                    return true;
                }
                certificate = IntPtr.Zero;
            }
            return false;
        }

        private void ResponseCallback(IAsyncResult asyncResult)
        {
            LdapRequestState asyncState = (LdapRequestState) asyncResult.AsyncState;
            try
            {
                DirectoryResponse response = this.fd.EndInvoke(asyncResult);
                asyncState.response = response;
            }
            catch (Exception exception)
            {
                asyncState.exception = exception;
                asyncState.response = null;
            }
            asyncState.ldapAsync.manualResetEvent.Set();
            asyncState.ldapAsync.completed = true;
            if ((asyncState.ldapAsync.callback != null) && !asyncState.abortCalled)
            {
                asyncState.ldapAsync.callback(asyncState.ldapAsync);
            }
        }

        private bool SameCredential(NetworkCredential oldCredential, NetworkCredential newCredential)
        {
            if ((oldCredential == null) && (newCredential == null))
            {
                return true;
            }
            if ((oldCredential == null) && (newCredential != null))
            {
                return false;
            }
            if ((oldCredential != null) && (newCredential == null))
            {
                return false;
            }
            return (((oldCredential.Domain == newCredential.Domain) && (oldCredential.UserName == newCredential.UserName)) && (oldCredential.Password == newCredential.Password));
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override DirectoryResponse SendRequest(DirectoryRequest request)
        {
            return this.SendRequest(request, base.connectionTimeOut);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public DirectoryResponse SendRequest(DirectoryRequest request, TimeSpan requestTimeout)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (request is DsmlAuthRequest)
            {
                throw new NotSupportedException(System.DirectoryServices.Protocols.Res.GetString("DsmlAuthRequestNotSupported"));
            }
            int messageID = 0;
            int error = this.SendRequestHelper(request, ref messageID);
            LdapOperation ldapSearch = LdapOperation.LdapSearch;
            if (request is DeleteRequest)
            {
                ldapSearch = LdapOperation.LdapDelete;
            }
            else if (request is AddRequest)
            {
                ldapSearch = LdapOperation.LdapAdd;
            }
            else if (request is ModifyRequest)
            {
                ldapSearch = LdapOperation.LdapModify;
            }
            else if (request is SearchRequest)
            {
                ldapSearch = LdapOperation.LdapSearch;
            }
            else if (request is ModifyDNRequest)
            {
                ldapSearch = LdapOperation.LdapModifyDn;
            }
            else if (request is CompareRequest)
            {
                ldapSearch = LdapOperation.LdapCompare;
            }
            else if (request is ExtendedRequest)
            {
                ldapSearch = LdapOperation.LdapExtendedRequest;
            }
            if ((error == 0) && (messageID != -1))
            {
                return this.ConstructResponse(messageID, ldapSearch, ResultAll.LDAP_MSG_ALL, requestTimeout, true);
            }
            if (error == 0)
            {
                error = Wldap32.LdapGetLastError();
            }
            throw this.ConstructException(error, ldapSearch);
        }

        private int SendRequestHelper(DirectoryRequest request, ref int messageID)
        {
            int num19;
            IntPtr zero = IntPtr.Zero;
            LdapControl[] controlArray = null;
            IntPtr clientcontrol = IntPtr.Zero;
            LdapControl[] controlArray2 = null;
            string strValue = null;
            ArrayList ptrToFree = new ArrayList();
            LdapMod[] modArray = null;
            IntPtr attrs = IntPtr.Zero;
            int num = 0;
            berval binaryValue = null;
            IntPtr attributes = IntPtr.Zero;
            int num2 = 0;
            int num3 = 0;
            if (!this.connected)
            {
                this.Connect();
                this.connected = true;
            }
            if ((this.AutoBind && (!this.bounded || this.needRebind)) && !((LdapDirectoryIdentifier) this.Directory).Connectionless)
            {
                this.Bind();
            }
            try
            {
                IntPtr ptr = IntPtr.Zero;
                IntPtr ptr6 = IntPtr.Zero;
                controlArray = this.BuildControlArray(request.Controls, true);
                int cb = Marshal.SizeOf(typeof(LdapControl));
                if (controlArray != null)
                {
                    zero = Marshal.AllocHGlobal((int) (Marshal.SizeOf(typeof(IntPtr)) * (controlArray.Length + 1)));
                    for (int i = 0; i < controlArray.Length; i++)
                    {
                        ptr = Marshal.AllocHGlobal(cb);
                        Marshal.StructureToPtr(controlArray[i], ptr, false);
                        ptr6 = (IntPtr) (((long) zero) + (Marshal.SizeOf(typeof(IntPtr)) * i));
                        Marshal.WriteIntPtr(ptr6, ptr);
                    }
                    ptr6 = (IntPtr) (((long) zero) + (Marshal.SizeOf(typeof(IntPtr)) * controlArray.Length));
                    Marshal.WriteIntPtr(ptr6, IntPtr.Zero);
                }
                controlArray2 = this.BuildControlArray(request.Controls, false);
                if (controlArray2 != null)
                {
                    clientcontrol = Marshal.AllocHGlobal((int) (Marshal.SizeOf(typeof(IntPtr)) * (controlArray2.Length + 1)));
                    for (int j = 0; j < controlArray2.Length; j++)
                    {
                        ptr = Marshal.AllocHGlobal(cb);
                        Marshal.StructureToPtr(controlArray2[j], ptr, false);
                        ptr6 = (IntPtr) (((long) clientcontrol) + (Marshal.SizeOf(typeof(IntPtr)) * j));
                        Marshal.WriteIntPtr(ptr6, ptr);
                    }
                    ptr6 = (IntPtr) (((long) clientcontrol) + (Marshal.SizeOf(typeof(IntPtr)) * controlArray2.Length));
                    Marshal.WriteIntPtr(ptr6, IntPtr.Zero);
                }
                if (request is DeleteRequest)
                {
                    num3 = Wldap32.ldap_delete_ext(this.ldapHandle, ((DeleteRequest) request).DistinguishedName, zero, clientcontrol, ref messageID);
                }
                else if (request is ModifyDNRequest)
                {
                    num3 = Wldap32.ldap_rename(this.ldapHandle, ((ModifyDNRequest) request).DistinguishedName, ((ModifyDNRequest) request).NewName, ((ModifyDNRequest) request).NewParentDistinguishedName, ((ModifyDNRequest) request).DeleteOldRdn ? 1 : 0, zero, clientcontrol, ref messageID);
                }
                else if (request is CompareRequest)
                {
                    DirectoryAttribute assertion = ((CompareRequest) request).Assertion;
                    if (assertion == null)
                    {
                        throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("WrongAssertionCompare"));
                    }
                    if (assertion.Count != 1)
                    {
                        throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("WrongNumValuesCompare"));
                    }
                    byte[] source = assertion[0] as byte[];
                    if (source != null)
                    {
                        if ((source != null) && (source.Length != 0))
                        {
                            binaryValue = new berval {
                                bv_len = source.Length,
                                bv_val = Marshal.AllocHGlobal(source.Length)
                            };
                            Marshal.Copy(source, 0, binaryValue.bv_val, source.Length);
                        }
                    }
                    else
                    {
                        strValue = assertion[0].ToString();
                    }
                    num3 = Wldap32.ldap_compare(this.ldapHandle, ((CompareRequest) request).DistinguishedName, assertion.Name, strValue, binaryValue, zero, clientcontrol, ref messageID);
                }
                else if ((request is AddRequest) || (request is ModifyRequest))
                {
                    if (request is AddRequest)
                    {
                        modArray = this.BuildAttributes(((AddRequest) request).Attributes, ptrToFree);
                    }
                    else
                    {
                        modArray = this.BuildAttributes(((ModifyRequest) request).Modifications, ptrToFree);
                    }
                    num = (modArray == null) ? 1 : (modArray.Length + 1);
                    attrs = Marshal.AllocHGlobal((int) (Marshal.SizeOf(typeof(IntPtr)) * num));
                    int num7 = Marshal.SizeOf(typeof(LdapMod));
                    int index = 0;
                    index = 0;
                    while (index < (num - 1))
                    {
                        ptr = Marshal.AllocHGlobal(num7);
                        Marshal.StructureToPtr(modArray[index], ptr, false);
                        ptr6 = (IntPtr) (((long) attrs) + (Marshal.SizeOf(typeof(IntPtr)) * index));
                        Marshal.WriteIntPtr(ptr6, ptr);
                        index++;
                    }
                    ptr6 = (IntPtr) (((long) attrs) + (Marshal.SizeOf(typeof(IntPtr)) * index));
                    Marshal.WriteIntPtr(ptr6, IntPtr.Zero);
                    if (request is AddRequest)
                    {
                        num3 = Wldap32.ldap_add(this.ldapHandle, ((AddRequest) request).DistinguishedName, attrs, zero, clientcontrol, ref messageID);
                    }
                    else
                    {
                        num3 = Wldap32.ldap_modify(this.ldapHandle, ((ModifyRequest) request).DistinguishedName, attrs, zero, clientcontrol, ref messageID);
                    }
                }
                else if (request is ExtendedRequest)
                {
                    string requestName = ((ExtendedRequest) request).RequestName;
                    byte[] requestValue = ((ExtendedRequest) request).RequestValue;
                    if ((requestValue != null) && (requestValue.Length != 0))
                    {
                        binaryValue = new berval {
                            bv_len = requestValue.Length,
                            bv_val = Marshal.AllocHGlobal(requestValue.Length)
                        };
                        Marshal.Copy(requestValue, 0, binaryValue.bv_val, requestValue.Length);
                    }
                    num3 = Wldap32.ldap_extended_operation(this.ldapHandle, requestName, binaryValue, zero, clientcontrol, ref messageID);
                }
                else
                {
                    if (request is SearchRequest)
                    {
                        SearchRequest request2 = (SearchRequest) request;
                        object filter = request2.Filter;
                        if ((filter != null) && (filter is XmlDocument))
                        {
                            throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("InvalidLdapSearchRequestFilter"));
                        }
                        string str3 = (string) filter;
                        num2 = (request2.Attributes == null) ? 0 : request2.Attributes.Count;
                        if (num2 != 0)
                        {
                            attributes = Marshal.AllocHGlobal((int) (Marshal.SizeOf(typeof(IntPtr)) * (num2 + 1)));
                            int num9 = 0;
                            num9 = 0;
                            while (num9 < num2)
                            {
                                ptr = Marshal.StringToHGlobalUni(request2.Attributes[num9]);
                                ptr6 = (IntPtr) (((long) attributes) + (Marshal.SizeOf(typeof(IntPtr)) * num9));
                                Marshal.WriteIntPtr(ptr6, ptr);
                                num9++;
                            }
                            ptr6 = (IntPtr) (((long) attributes) + (Marshal.SizeOf(typeof(IntPtr)) * num9));
                            Marshal.WriteIntPtr(ptr6, IntPtr.Zero);
                        }
                        int scope = (int) request2.Scope;
                        int timelimit = (int) (request2.TimeLimit.Ticks / 0x989680L);
                        System.DirectoryServices.Protocols.DereferenceAlias derefAlias = this.options.DerefAlias;
                        this.options.DerefAlias = request2.Aliases;
                        try
                        {
                            num3 = Wldap32.ldap_search(this.ldapHandle, request2.DistinguishedName, scope, str3, attributes, request2.TypesOnly, zero, clientcontrol, timelimit, request2.SizeLimit, ref messageID);
                            goto Label_06A6;
                        }
                        finally
                        {
                            this.options.DerefAlias = derefAlias;
                        }
                    }
                    throw new NotSupportedException(System.DirectoryServices.Protocols.Res.GetString("InvliadRequestType"));
                }
            Label_06A6:
                if (num3 == 0x55)
                {
                    num3 = 0x70;
                }
                num19 = num3;
            }
            finally
            {
                GC.KeepAlive(modArray);
                if (zero != IntPtr.Zero)
                {
                    for (int m = 0; m < controlArray.Length; m++)
                    {
                        IntPtr hglobal = Marshal.ReadIntPtr(zero, Marshal.SizeOf(typeof(IntPtr)) * m);
                        if (hglobal != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(hglobal);
                        }
                    }
                    Marshal.FreeHGlobal(zero);
                }
                if (controlArray != null)
                {
                    for (int n = 0; n < controlArray.Length; n++)
                    {
                        if (controlArray[n].ldctl_oid != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(controlArray[n].ldctl_oid);
                        }
                        if ((controlArray[n].ldctl_value != null) && (controlArray[n].ldctl_value.bv_val != IntPtr.Zero))
                        {
                            Marshal.FreeHGlobal(controlArray[n].ldctl_value.bv_val);
                        }
                    }
                }
                if (clientcontrol != IntPtr.Zero)
                {
                    for (int num14 = 0; num14 < controlArray2.Length; num14++)
                    {
                        IntPtr ptr8 = Marshal.ReadIntPtr(clientcontrol, Marshal.SizeOf(typeof(IntPtr)) * num14);
                        if (ptr8 != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(ptr8);
                        }
                    }
                    Marshal.FreeHGlobal(clientcontrol);
                }
                if (controlArray2 != null)
                {
                    for (int num15 = 0; num15 < controlArray2.Length; num15++)
                    {
                        if (controlArray2[num15].ldctl_oid != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(controlArray2[num15].ldctl_oid);
                        }
                        if ((controlArray2[num15].ldctl_value != null) && (controlArray2[num15].ldctl_value.bv_val != IntPtr.Zero))
                        {
                            Marshal.FreeHGlobal(controlArray2[num15].ldctl_value.bv_val);
                        }
                    }
                }
                if (attrs != IntPtr.Zero)
                {
                    for (int num16 = 0; num16 < (num - 1); num16++)
                    {
                        IntPtr ptr9 = Marshal.ReadIntPtr(attrs, Marshal.SizeOf(typeof(IntPtr)) * num16);
                        if (ptr9 != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(ptr9);
                        }
                    }
                    Marshal.FreeHGlobal(attrs);
                }
                for (int k = 0; k < ptrToFree.Count; k++)
                {
                    IntPtr ptr10 = (IntPtr) ptrToFree[k];
                    Marshal.FreeHGlobal(ptr10);
                }
                if ((binaryValue != null) && (binaryValue.bv_val != IntPtr.Zero))
                {
                    Marshal.FreeHGlobal(binaryValue.bv_val);
                }
                if (attributes != IntPtr.Zero)
                {
                    for (int num18 = 0; num18 < num2; num18++)
                    {
                        IntPtr ptr11 = Marshal.ReadIntPtr(attributes, Marshal.SizeOf(typeof(IntPtr)) * num18);
                        if (ptr11 != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(ptr11);
                        }
                    }
                    Marshal.FreeHGlobal(attributes);
                }
            }
            return num19;
        }

        public System.DirectoryServices.Protocols.AuthType AuthType
        {
            get
            {
                return this.connectionAuthType;
            }
            set
            {
                if ((value < System.DirectoryServices.Protocols.AuthType.Anonymous) || (value > System.DirectoryServices.Protocols.AuthType.Kerberos))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.DirectoryServices.Protocols.AuthType));
                }
                if (this.bounded && (value != this.connectionAuthType))
                {
                    this.needRebind = true;
                }
                this.connectionAuthType = value;
            }
        }

        public bool AutoBind
        {
            get
            {
                return this.automaticBind;
            }
            set
            {
                this.automaticBind = value;
            }
        }

        public override NetworkCredential Credential
        {
            [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
            set
            {
                if (this.bounded && !this.SameCredential(base.directoryCredential, value))
                {
                    this.needRebind = true;
                }
                base.directoryCredential = (value != null) ? new NetworkCredential(value.UserName, value.Password, value.Domain) : null;
            }
        }

        public LdapSessionOptions SessionOptions
        {
            get
            {
                return this.options;
            }
        }

        public override TimeSpan Timeout
        {
            get
            {
                return base.connectionTimeOut;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("NoNegativeTime"), "value");
                }
                if (value.TotalSeconds > 2147483647.0)
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("TimespanExceedMax"), "value");
                }
                base.connectionTimeOut = value;
            }
        }

        internal enum LdapResult
        {
            LDAP_RES_ADD = 0x69,
            LDAP_RES_COMPARE = 0x6f,
            LDAP_RES_DELETE = 0x6b,
            LDAP_RES_EXTENDED = 120,
            LDAP_RES_MODIFY = 0x67,
            LDAP_RES_MODRDN = 0x6d,
            LDAP_RES_REFERRAL = 0x73,
            LDAP_RES_SEARCH_ENTRY = 100,
            LDAP_RES_SEARCH_RESULT = 0x65
        }
    }
}

