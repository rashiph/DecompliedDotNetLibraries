namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;

    internal class OperationErrorMappings
    {
        private static Hashtable ResultCodeHash = new Hashtable();

        static OperationErrorMappings()
        {
            ResultCodeHash.Add(ResultCode.Success, Res.GetString("LDAP_SUCCESS"));
            ResultCodeHash.Add(ResultCode.OperationsError, Res.GetString("LDAP_OPERATIONS_ERROR"));
            ResultCodeHash.Add(ResultCode.ProtocolError, Res.GetString("LDAP_PROTOCOL_ERROR"));
            ResultCodeHash.Add(ResultCode.TimeLimitExceeded, Res.GetString("LDAP_TIMELIMIT_EXCEEDED"));
            ResultCodeHash.Add(ResultCode.SizeLimitExceeded, Res.GetString("LDAP_SIZELIMIT_EXCEEDED"));
            ResultCodeHash.Add(ResultCode.CompareFalse, Res.GetString("LDAP_COMPARE_FALSE"));
            ResultCodeHash.Add(ResultCode.CompareTrue, Res.GetString("LDAP_COMPARE_TRUE"));
            ResultCodeHash.Add(ResultCode.AuthMethodNotSupported, Res.GetString("LDAP_AUTH_METHOD_NOT_SUPPORTED"));
            ResultCodeHash.Add(ResultCode.StrongAuthRequired, Res.GetString("LDAP_STRONG_AUTH_REQUIRED"));
            ResultCodeHash.Add(ResultCode.ReferralV2, Res.GetString("LDAP_PARTIAL_RESULTS"));
            ResultCodeHash.Add(ResultCode.Referral, Res.GetString("LDAP_REFERRAL"));
            ResultCodeHash.Add(ResultCode.AdminLimitExceeded, Res.GetString("LDAP_ADMIN_LIMIT_EXCEEDED"));
            ResultCodeHash.Add(ResultCode.UnavailableCriticalExtension, Res.GetString("LDAP_UNAVAILABLE_CRIT_EXTENSION"));
            ResultCodeHash.Add(ResultCode.ConfidentialityRequired, Res.GetString("LDAP_CONFIDENTIALITY_REQUIRED"));
            ResultCodeHash.Add(ResultCode.SaslBindInProgress, Res.GetString("LDAP_SASL_BIND_IN_PROGRESS"));
            ResultCodeHash.Add(ResultCode.NoSuchAttribute, Res.GetString("LDAP_NO_SUCH_ATTRIBUTE"));
            ResultCodeHash.Add(ResultCode.UndefinedAttributeType, Res.GetString("LDAP_UNDEFINED_TYPE"));
            ResultCodeHash.Add(ResultCode.InappropriateMatching, Res.GetString("LDAP_INAPPROPRIATE_MATCHING"));
            ResultCodeHash.Add(ResultCode.ConstraintViolation, Res.GetString("LDAP_CONSTRAINT_VIOLATION"));
            ResultCodeHash.Add(ResultCode.AttributeOrValueExists, Res.GetString("LDAP_ATTRIBUTE_OR_VALUE_EXISTS"));
            ResultCodeHash.Add(ResultCode.InvalidAttributeSyntax, Res.GetString("LDAP_INVALID_SYNTAX"));
            ResultCodeHash.Add(ResultCode.NoSuchObject, Res.GetString("LDAP_NO_SUCH_OBJECT"));
            ResultCodeHash.Add(ResultCode.AliasProblem, Res.GetString("LDAP_ALIAS_PROBLEM"));
            ResultCodeHash.Add(ResultCode.InvalidDNSyntax, Res.GetString("LDAP_INVALID_DN_SYNTAX"));
            ResultCodeHash.Add(ResultCode.AliasDereferencingProblem, Res.GetString("LDAP_ALIAS_DEREF_PROBLEM"));
            ResultCodeHash.Add(ResultCode.InappropriateAuthentication, Res.GetString("LDAP_INAPPROPRIATE_AUTH"));
            ResultCodeHash.Add(ResultCode.InsufficientAccessRights, Res.GetString("LDAP_INSUFFICIENT_RIGHTS"));
            ResultCodeHash.Add(ResultCode.Busy, Res.GetString("LDAP_BUSY"));
            ResultCodeHash.Add(ResultCode.Unavailable, Res.GetString("LDAP_UNAVAILABLE"));
            ResultCodeHash.Add(ResultCode.UnwillingToPerform, Res.GetString("LDAP_UNWILLING_TO_PERFORM"));
            ResultCodeHash.Add(ResultCode.LoopDetect, Res.GetString("LDAP_LOOP_DETECT"));
            ResultCodeHash.Add(ResultCode.SortControlMissing, Res.GetString("LDAP_SORT_CONTROL_MISSING"));
            ResultCodeHash.Add(ResultCode.OffsetRangeError, Res.GetString("LDAP_OFFSET_RANGE_ERROR"));
            ResultCodeHash.Add(ResultCode.NamingViolation, Res.GetString("LDAP_NAMING_VIOLATION"));
            ResultCodeHash.Add(ResultCode.ObjectClassViolation, Res.GetString("LDAP_OBJECT_CLASS_VIOLATION"));
            ResultCodeHash.Add(ResultCode.NotAllowedOnNonLeaf, Res.GetString("LDAP_NOT_ALLOWED_ON_NONLEAF"));
            ResultCodeHash.Add(ResultCode.NotAllowedOnRdn, Res.GetString("LDAP_NOT_ALLOWED_ON_RDN"));
            ResultCodeHash.Add(ResultCode.EntryAlreadyExists, Res.GetString("LDAP_ALREADY_EXISTS"));
            ResultCodeHash.Add(ResultCode.ObjectClassModificationsProhibited, Res.GetString("LDAP_NO_OBJECT_CLASS_MODS"));
            ResultCodeHash.Add(ResultCode.ResultsTooLarge, Res.GetString("LDAP_RESULTS_TOO_LARGE"));
            ResultCodeHash.Add(ResultCode.AffectsMultipleDsas, Res.GetString("LDAP_AFFECTS_MULTIPLE_DSAS"));
            ResultCodeHash.Add(ResultCode.VirtualListViewError, Res.GetString("LDAP_VIRTUAL_LIST_VIEW_ERROR"));
            ResultCodeHash.Add(ResultCode.Other, Res.GetString("LDAP_OTHER"));
        }

        public static string MapResultCode(int errorCode)
        {
            return (string) ResultCodeHash[(ResultCode) errorCode];
        }
    }
}

