namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity, ComVisible(false)]
    internal class Wldap32
    {
        public const string MICROSOFT_KERBEROS_NAME_W = "Kerberos";
        public const int SEC_WINNT_AUTH_IDENTITY_UNICODE = 2;
        public const int SEC_WINNT_AUTH_IDENTITY_VERSION = 0x200;

        [DllImport("wldap32.dll", EntryPoint="ber_alloc_t", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ber_alloc(int option);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ber_bvecfree(IntPtr value);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ber_bvfree(IntPtr value);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ber_flatten(BerSafeHandle berElement, ref IntPtr value);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ber_free([In] IntPtr berelement, int option);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ber_init(berval value);
        [DllImport("wldap32.dll", EntryPoint="ber_printf", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ber_printf_berarray(BerSafeHandle berElement, string format, IntPtr value);
        [DllImport("wldap32.dll", EntryPoint="ber_printf", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ber_printf_bytearray(BerSafeHandle berElement, string format, HGlobalMemHandle value, int length);
        [DllImport("wldap32.dll", EntryPoint="ber_printf", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ber_printf_emptyarg(BerSafeHandle berElement, string format);
        [DllImport("wldap32.dll", EntryPoint="ber_printf", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ber_printf_int(BerSafeHandle berElement, string format, int value);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ber_scanf(BerSafeHandle berElement, string format);
        [DllImport("wldap32.dll", EntryPoint="ber_scanf", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ber_scanf_bitstring(BerSafeHandle berElement, string format, ref IntPtr value, ref int length);
        [DllImport("wldap32.dll", EntryPoint="ber_scanf", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ber_scanf_int(BerSafeHandle berElement, string format, ref int value);
        [DllImport("wldap32.dll", EntryPoint="ber_scanf", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ber_scanf_ptr(BerSafeHandle berElement, string format, ref IntPtr value);
        [DllImport("Crypt32.dll", CharSet=CharSet.Unicode)]
        public static extern int CertFreeCRLContext(IntPtr certContext);
        [DllImport("wldap32.dll", EntryPoint="cldap_openW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern IntPtr cldap_open(string hostName, int portNumber);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_abandon([In] IntPtr ldapHandle, [In] int messagId);
        [DllImport("wldap32.dll", EntryPoint="ldap_add_extW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_add([In] IntPtr ldapHandle, string dn, IntPtr attrs, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);
        [DllImport("wldap32.dll", EntryPoint="ldap_bind_sW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_bind_s([In] IntPtr ldapHandle, string dn, SEC_WINNT_AUTH_IDENTITY_EX credentials, BindMethod method);
        [DllImport("wldap32.dll", EntryPoint="ldap_compare_extW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_compare([In] IntPtr ldapHandle, string dn, string attributeName, string strValue, berval binaryValue, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern int ldap_connect([In] IntPtr ldapHandle, LDAP_TIMEVAL timeout);
        [DllImport("wldap32.dll", EntryPoint="ldap_control_freeW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_control_free(IntPtr control);
        [DllImport("wldap32.dll", EntryPoint="ldap_controls_freeW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_controls_free([In] IntPtr value);
        [DllImport("wldap32.dll", EntryPoint="ldap_create_sort_controlW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_create_sort_control(ConnectionHandle handle, IntPtr keys, byte critical, ref IntPtr control);
        [DllImport("wldap32.dll", EntryPoint="ldap_delete_extW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_delete_ext([In] IntPtr ldapHandle, string dn, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);
        [DllImport("wldap32.dll", EntryPoint="ldap_extended_operationW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_extended_operation([In] IntPtr ldapHandle, string oid, berval data, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);
        [DllImport("wldap32.dll", EntryPoint="ldap_first_attributeW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ldap_first_attribute([In] IntPtr ldapHandle, [In] IntPtr result, ref IntPtr address);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ldap_first_entry([In] IntPtr ldapHandle, [In] IntPtr result);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ldap_first_reference([In] IntPtr ldapHandle, [In] IntPtr result);
        [DllImport("wldap32.dll", EntryPoint="ldap_get_dnW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ldap_get_dn([In] IntPtr ldapHandle, [In] IntPtr result);
        [DllImport("wldap32.dll", EntryPoint="ldap_get_optionW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_get_option_int([In] IntPtr ldapHandle, [In] LdapOption option, ref int outValue);
        [DllImport("wldap32.dll", EntryPoint="ldap_get_optionW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_get_option_ptr([In] IntPtr ldapHandle, [In] LdapOption option, ref IntPtr outValue);
        [DllImport("wldap32.dll", EntryPoint="ldap_get_optionW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_get_option_sechandle([In] IntPtr ldapHandle, [In] LdapOption option, ref SecurityHandle outValue);
        [DllImport("wldap32.dll", EntryPoint="ldap_get_optionW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_get_option_secInfo([In] IntPtr ldapHandle, [In] LdapOption option, [In, Out] SecurityPackageContextConnectionInformation outValue);
        [DllImport("wldap32.dll", EntryPoint="ldap_get_values_lenW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ldap_get_values_len([In] IntPtr ldapHandle, [In] IntPtr result, string name);
        [DllImport("wldap32.dll", EntryPoint="ldap_initW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern IntPtr ldap_init(string hostName, int portNumber);
        [DllImport("wldap32.dll", EntryPoint="ldap_memfreeW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern void ldap_memfree([In] IntPtr value);
        [DllImport("wldap32.dll", EntryPoint="ldap_modify_extW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_modify([In] IntPtr ldapHandle, string dn, IntPtr attrs, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_msgfree([In] IntPtr result);
        [DllImport("wldap32.dll", EntryPoint="ldap_next_attributeW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ldap_next_attribute([In] IntPtr ldapHandle, [In] IntPtr result, [In, Out] IntPtr address);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ldap_next_entry([In] IntPtr ldapHandle, [In] IntPtr result);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ldap_next_reference([In] IntPtr ldapHandle, [In] IntPtr result);
        [DllImport("wldap32.dll", EntryPoint="ldap_parse_extended_resultW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_parse_extended_result([In] IntPtr ldapHandle, [In] IntPtr result, ref IntPtr oid, ref IntPtr data, byte freeIt);
        [DllImport("wldap32.dll", EntryPoint="ldap_parse_referenceW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_parse_reference([In] IntPtr ldapHandle, [In] IntPtr result, ref IntPtr referrals);
        [DllImport("wldap32.dll", EntryPoint="ldap_parse_resultW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_parse_result([In] IntPtr ldapHandle, [In] IntPtr result, ref int serverError, ref IntPtr dn, ref IntPtr message, ref IntPtr referral, ref IntPtr control, byte freeIt);
        [DllImport("wldap32.dll", EntryPoint="ldap_parse_resultW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_parse_result_referral([In] IntPtr ldapHandle, [In] IntPtr result, IntPtr serverError, IntPtr dn, IntPtr message, ref IntPtr referral, IntPtr control, byte freeIt);
        [DllImport("wldap32.dll", EntryPoint="ldap_rename_extW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_rename([In] IntPtr ldapHandle, string dn, string newRdn, string newParentDn, int deleteOldRdn, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int ldap_result([In] IntPtr ldapHandle, int messageId, int all, LDAP_TIMEVAL timeout, ref IntPtr Mesage);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_result2error([In] IntPtr ldapHandle, [In] IntPtr result, int freeIt);
        [DllImport("wldap32.dll", EntryPoint="ldap_search_extW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_search([In] IntPtr ldapHandle, string dn, int scope, string filter, IntPtr attributes, bool attributeOnly, IntPtr servercontrol, IntPtr clientcontrol, int timelimit, int sizelimit, ref int messageNumber);
        [DllImport("wldap32.dll", EntryPoint="ldap_set_optionW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_set_option_clientcert([In] IntPtr ldapHandle, [In] LdapOption option, QUERYCLIENTCERT outValue);
        [DllImport("wldap32.dll", EntryPoint="ldap_set_optionW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_set_option_int([In] IntPtr ldapHandle, [In] LdapOption option, ref int inValue);
        [DllImport("wldap32.dll", EntryPoint="ldap_set_optionW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_set_option_ptr([In] IntPtr ldapHandle, [In] LdapOption option, ref IntPtr inValue);
        [DllImport("wldap32.dll", EntryPoint="ldap_set_optionW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_set_option_referral([In] IntPtr ldapHandle, [In] LdapOption option, ref LdapReferralCallback outValue);
        [DllImport("wldap32.dll", EntryPoint="ldap_set_optionW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_set_option_servercert([In] IntPtr ldapHandle, [In] LdapOption option, VERIFYSERVERCERT outValue);
        [DllImport("wldap32.dll", EntryPoint="ldap_simple_bind_sW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_simple_bind_s([In] IntPtr ldapHandle, string distinguishedName, string password);
        [DllImport("wldap32.dll", EntryPoint="ldap_start_tls_sW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_start_tls(IntPtr ldapHandle, ref int ServerReturnValue, ref IntPtr Message, IntPtr ServerControls, IntPtr ClientControls);
        [DllImport("wldap32.dll", EntryPoint="ldap_stop_tls_s", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern byte ldap_stop_tls(IntPtr ldapHandle);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern int ldap_unbind([In] IntPtr ldapHandle);
        [DllImport("wldap32.dll", EntryPoint="ldap_value_freeW", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern int ldap_value_free([In] IntPtr value);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
        public static extern IntPtr ldap_value_free_len([In] IntPtr berelement);
        [DllImport("wldap32.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int LdapGetLastError();
    }
}

