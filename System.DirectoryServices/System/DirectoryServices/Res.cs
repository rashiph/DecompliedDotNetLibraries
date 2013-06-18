namespace System.DirectoryServices
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class Res
    {
        internal const string ADAMInstanceNotFound = "ADAMInstanceNotFound";
        internal const string ADAMInstanceNotFoundInConfigSet = "ADAMInstanceNotFoundInConfigSet";
        internal const string AINotFound = "AINotFound";
        internal const string AlreadyExistingDomainTrust = "AlreadyExistingDomainTrust";
        internal const string AlreadyExistingForestTrust = "AlreadyExistingForestTrust";
        internal const string AlreadyExistingInCollection = "AlreadyExistingInCollection";
        internal const string ApplicationPartitionTypeUnknown = "ApplicationPartitionTypeUnknown";
        internal const string AppNCNotFound = "AppNCNotFound";
        internal const string CannotDelete = "CannotDelete";
        internal const string CannotGetObject = "CannotGetObject";
        internal const string CannotModifyDacl = "CannotModifyDacl";
        internal const string CannotModifySacl = "CannotModifySacl";
        internal const string CannotPerformOnGC = "CannotPerformOnGC";
        internal const string CannotPerformOnGCObject = "CannotPerformOnGCObject";
        internal const string CannotPerformOperationOnUncommittedObject = "CannotPerformOperationOnUncommittedObject";
        internal const string ComputerNotJoinedToDomain = "ComputerNotJoinedToDomain";
        internal const string ComputerObjectNameNotFound = "ComputerObjectNameNotFound";
        internal const string ConfigSectionsUnique = "ConfigSectionsUnique";
        internal const string ConfigSetNotFound = "ConfigSetNotFound";
        internal const string ConnectionNotCommitted = "ConnectionNotCommitted";
        internal const string ConnectionSourcServerSameConfigSet = "ConnectionSourcServerSameConfigSet";
        internal const string ConnectionSourcServerSameForest = "ConnectionSourcServerSameForest";
        internal const string ConnectionSourcServerShouldBeADAM = "ConnectionSourcServerShouldBeADAM";
        internal const string ConnectionSourcServerShouldBeDC = "ConnectionSourcServerShouldBeDC";
        internal const string ContextNotAssociatedWithDomain = "ContextNotAssociatedWithDomain";
        internal const string DCInfoNotFound = "DCInfoNotFound";
        internal const string DCNotFound = "DCNotFound";
        internal const string DCNotFoundInDomain = "DCNotFoundInDomain";
        internal const string DestinationArrayNotLargeEnough = "DestinationArrayNotLargeEnough";
        internal const string DirectoryContextNeedHost = "DirectoryContextNeedHost";
        internal const string DirectoryEntryDesc = "DirectoryEntryDesc";
        internal const string DirectorySearcherDesc = "DirectorySearcherDesc";
        internal const string DomainNotFound = "DomainNotFound";
        internal const string DomainTrustDoesNotExist = "DomainTrustDoesNotExist";
        internal const string DSAddNotSupported = "DSAddNotSupported";
        internal const string DSAdsiNotInstalled = "DSAdsiNotInstalled";
        internal const string DSAdsvalueTypeNYI = "DSAdsvalueTypeNYI";
        internal const string DSAfterCount = "DSAfterCount";
        internal const string DSApproximateTotal = "DSApproximateTotal";
        internal const string DSAsynchronous = "DSAsynchronous";
        internal const string DSAttributeQuery = "DSAttributeQuery";
        internal const string DSAuthenticationType = "DSAuthenticationType";
        internal const string DSBadAfterCount = "DSBadAfterCount";
        internal const string DSBadApproximateTotal = "DSBadApproximateTotal";
        internal const string DSBadASQSearchScope = "DSBadASQSearchScope";
        internal const string DSBadBeforeCount = "DSBadBeforeCount";
        internal const string DSBadCacheResultsVLV = "DSBadCacheResultsVLV";
        internal const string DSBadDirectorySynchronizationFlag = "DSBadDirectorySynchronizationFlag";
        internal const string DSBadOffset = "DSBadOffset";
        internal const string DSBadPageSize = "DSBadPageSize";
        internal const string DSBadPageSizeDirsync = "DSBadPageSizeDirsync";
        internal const string DSBadSizeLimit = "DSBadSizeLimit";
        internal const string DSBadTargetPercentage = "DSBadTargetPercentage";
        internal const string DSBeforeCount = "DSBeforeCount";
        internal const string DSCacheResults = "DSCacheResults";
        internal const string DSCannotBeIndexed = "DSCannotBeIndexed";
        internal const string DSCannotCount = "DSCannotCount";
        internal const string DSCannotDelete = "DSCannotDelete";
        internal const string DSCannotEmunerate = "DSCannotEmunerate";
        internal const string DSCannotGetKeys = "DSCannotGetKeys";
        internal const string DSChildren = "DSChildren";
        internal const string DSClearNotSupported = "DSClearNotSupported";
        internal const string DSClientTimeout = "DSClientTimeout";
        internal const string DSConvertFailed = "DSConvertFailed";
        internal const string DSConvertTypeInvalid = "DSConvertTypeInvalid";
        internal const string DSDerefAlias = "DSDerefAlias";
        internal const string DSDirectorySynchronization = "DSDirectorySynchronization";
        internal const string DSDirectorySynchronizationCookie = "DSDirectorySynchronizationCookie";
        internal const string DSDirectorySynchronizationFlag = "DSDirectorySynchronizationFlag";
        internal const string DSDirectoryVirtualListViewContext = "DSDirectoryVirtualListViewContext";
        internal const string DSDoesNotImplementIADs = "DSDoesNotImplementIADs";
        internal const string DSDoesNotImplementIADsObjectOptions = "DSDoesNotImplementIADsObjectOptions";
        internal const string DSEnumerator = "DSEnumerator";
        internal const string DSExtendedDn = "DSExtendedDn";
        internal const string DSFilter = "DSFilter";
        internal const string DSGuid = "DSGuid";
        internal const string DSInvalidPath = "DSInvalidPath";
        internal const string DSInvalidSearchFilter = "DSInvalidSearchFilter";
        internal const string DSMultipleSDNotSupported = "DSMultipleSDNotSupported";
        internal const string DSName = "DSName";
        internal const string DSNativeGuid = "DSNativeGuid";
        internal const string DSNativeObject = "DSNativeObject";
        internal const string DSNoCurrentChild = "DSNoCurrentChild";
        internal const string DSNoCurrentEntry = "DSNoCurrentEntry";
        internal const string DSNoCurrentProperty = "DSNoCurrentProperty";
        internal const string DSNoCurrentValue = "DSNoCurrentValue";
        internal const string DSNoObject = "DSNoObject";
        internal const string DSNotAContainer = "DSNotAContainer";
        internal const string DSNotFound = "DSNotFound";
        internal const string DSNotInCollection = "DSNotInCollection";
        internal const string DSNotSet = "DSNotSet";
        internal const string DSNotSupportOnClient = "DSNotSupportOnClient";
        internal const string DSNotSupportOnDC = "DSNotSupportOnDC";
        internal const string DSObjectSecurity = "DSObjectSecurity";
        internal const string DSOffset = "DSOffset";
        internal const string DSOptions = "DSOptions";
        internal const string DSPageSize = "DSPageSize";
        internal const string DSParent = "DSParent";
        internal const string DSPassword = "DSPassword";
        internal const string DSPath = "DSPath";
        internal const string DSPathIsNotSet = "DSPathIsNotSet";
        internal const string DSProperties = "DSProperties";
        internal const string DSPropertiesToLoad = "DSPropertiesToLoad";
        internal const string DSPropertyListUnsupported = "DSPropertyListUnsupported";
        internal const string DSPropertyNamesOnly = "DSPropertyNamesOnly";
        internal const string DSPropertyNotFound = "DSPropertyNotFound";
        internal const string DSPropertySetSupported = "DSPropertySetSupported";
        internal const string DSPropertyValueSupportOneOperation = "DSPropertyValueSupportOneOperation";
        internal const string DSReferralChasing = "DSReferralChasing";
        internal const string DSRemoveNotSupported = "DSRemoveNotSupported";
        internal const string DSSchemaClassName = "DSSchemaClassName";
        internal const string DSSchemaEntry = "DSSchemaEntry";
        internal const string DSSDNoValues = "DSSDNoValues";
        internal const string DSSearchPreferencesNotAccepted = "DSSearchPreferencesNotAccepted";
        internal const string DSSearchRoot = "DSSearchRoot";
        internal const string DSSearchScope = "DSSearchScope";
        internal const string DSSearchUnsupported = "DSSearchUnsupported";
        internal const string DSSecurityMasks = "DSSecurityMasks";
        internal const string DSServerPageTimeLimit = "DSServerPageTimeLimit";
        internal const string DSServerTimeLimit = "DSServerTimeLimit";
        internal const string DSSizeLimit = "DSSizeLimit";
        internal const string DSSort = "DSSort";
        internal const string DSSortDirection = "DSSortDirection";
        internal const string DSSortName = "DSSortName";
        internal const string DSSyncAllFailure = "DSSyncAllFailure";
        internal const string DSTarget = "DSTarget";
        internal const string DSTargetPercentage = "DSTargetPercentage";
        internal const string DSTombstone = "DSTombstone";
        internal const string DSUnknown = "DSUnknown";
        internal const string DSUnknownFailure = "DSUnknownFailure";
        internal const string DSUsePropertyCache = "DSUsePropertyCache";
        internal const string DSUsername = "DSUsername";
        internal const string DSVirtualListView = "DSVirtualListView";
        internal const string EmptyStringParameter = "EmptyStringParameter";
        internal const string ForestNotFound = "ForestNotFound";
        internal const string ForestTrustCollision = "ForestTrustCollision";
        internal const string ForestTrustDoesNotExist = "ForestTrustDoesNotExist";
        internal const string GCDisabled = "GCDisabled";
        internal const string GCNotFound = "GCNotFound";
        internal const string GCNotFoundInForest = "GCNotFoundInForest";
        internal const string Invalid_boolean_attribute = "Invalid_boolean_attribute";
        internal const string InvalidContextTarget = "InvalidContextTarget";
        internal const string InvalidDNFormat = "InvalidDNFormat";
        internal const string InvalidDnsName = "InvalidDnsName";
        internal const string InvalidFlags = "InvalidFlags";
        internal const string InvalidMode = "InvalidMode";
        internal const string InvalidServerNameFormat = "InvalidServerNameFormat";
        internal const string InvalidTime = "InvalidTime";
        internal const string KerberosNotSupported = "KerberosNotSupported";
        internal const string LessThanZero = "LessThanZero";
        internal const string LinkedPropertyNotFound = "LinkedPropertyNotFound";
        internal const string LinkIdNotEvenNumber = "LinkIdNotEvenNumber";
        private static Res loader;
        internal const string Name = "Name";
        internal const string NDNCNotFound = "NDNCNotFound";
        internal const string NoCurrentSite = "NoCurrentSite";
        internal const string NoHostName = "NoHostName";
        internal const string NoHostNameOrPortNumber = "NoHostNameOrPortNumber";
        internal const string NoNegativeTime = "NoNegativeTime";
        internal const string NoObjectClassForADPartition = "NoObjectClassForADPartition";
        internal const string NotADOrADAM = "NotADOrADAM";
        internal const string NotFoundInCollection = "NotFoundInCollection";
        internal const string NotSupportTransportSMTP = "NotSupportTransportSMTP";
        internal const string NotWithinSite = "NotWithinSite";
        internal const string NoW2K3DCs = "NoW2K3DCs";
        internal const string NoW2K3DCsInForest = "NoW2K3DCsInForest";
        internal const string NT4NotSupported = "NT4NotSupported";
        internal const string NTAuthority = "NTAuthority";
        internal const string NtdsaObjectGuidNotFound = "NtdsaObjectGuidNotFound";
        internal const string NtdsaObjectNameNotFound = "NtdsaObjectNameNotFound";
        internal const string NTDSSiteSetting = "NTDSSiteSetting";
        internal const string OneLevelPartitionNotSupported = "OneLevelPartitionNotSupported";
        internal const string OnlyAllowSingleDimension = "OnlyAllowSingleDimension";
        internal const string OnlyDomainOrForest = "OnlyDomainOrForest";
        internal const string OperationInvalidForADAM = "OperationInvalidForADAM";
        internal const string PropertyInvalidForADAM = "PropertyInvalidForADAM";
        internal const string PropertyNotFound = "PropertyNotFound";
        internal const string PropertyNotFoundOnObject = "PropertyNotFoundOnObject";
        internal const string PropertyNotSet = "PropertyNotSet";
        internal const string ReplicaNotFound = "ReplicaNotFound";
        internal const string ReplicationIntervalExceedMax = "ReplicationIntervalExceedMax";
        internal const string ReplicationIntervalInMinutes = "ReplicationIntervalInMinutes";
        private ResourceManager resources;
        internal const string SchemaObjectNotCommitted = "SchemaObjectNotCommitted";
        internal const string ServerNotAReplica = "ServerNotAReplica";
        internal const string ServerNotFound = "ServerNotFound";
        internal const string ServerObjectNameNotFound = "ServerObjectNameNotFound";
        internal const string ServerShouldBeAI = "ServerShouldBeAI";
        internal const string ServerShouldBeDC = "ServerShouldBeDC";
        internal const string ServerShouldBeW2K3 = "ServerShouldBeW2K3";
        internal const string SiteLinkNotCommitted = "SiteLinkNotCommitted";
        internal const string SiteNameNotFound = "SiteNameNotFound";
        internal const string SiteNotCommitted = "SiteNotCommitted";
        internal const string SiteNotExist = "SiteNotExist";
        internal const string SiteObjectNameNotFound = "SiteObjectNameNotFound";
        internal const string SubnetNotCommitted = "SubnetNotCommitted";
        internal const string SupportedPlatforms = "SupportedPlatforms";
        internal const string TargetShouldBeADAMServer = "TargetShouldBeADAMServer";
        internal const string TargetShouldBeAppNCDnsName = "TargetShouldBeAppNCDnsName";
        internal const string TargetShouldBeConfigSet = "TargetShouldBeConfigSet";
        internal const string TargetShouldBeDC = "TargetShouldBeDC";
        internal const string TargetShouldBeDomain = "TargetShouldBeDomain";
        internal const string TargetShouldBeForest = "TargetShouldBeForest";
        internal const string TargetShouldBeGC = "TargetShouldBeGC";
        internal const string TargetShouldBeServer = "TargetShouldBeServer";
        internal const string TargetShouldBeServerORConfigSet = "TargetShouldBeServerORConfigSet";
        internal const string TargetShouldBeServerORDomain = "TargetShouldBeServerORDomain";
        internal const string TargetShouldBeServerORForest = "TargetShouldBeServerORForest";
        internal const string TimespanExceedMax = "TimespanExceedMax";
        internal const string TransportNotFound = "TransportNotFound";
        internal const string TrustVerificationNotSupport = "TrustVerificationNotSupport";
        internal const string UnknownSyntax = "UnknownSyntax";
        internal const string UnknownTransport = "UnknownTransport";
        internal const string ValueCannotBeModified = "ValueCannotBeModified";
        internal const string VersionFailure = "VersionFailure";
        internal const string WrongForestTrust = "WrongForestTrust";
        internal const string WrongTrustDirection = "WrongTrustDirection";

        internal Res()
        {
            this.resources = new ResourceManager("System.DirectoryServices", base.GetType().Assembly);
        }

        private static Res GetLoader()
        {
            if (loader == null)
            {
                Res res = new Res();
                Interlocked.CompareExchange<Res>(ref loader, res, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

