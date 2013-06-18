namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;

    internal class PropertyManager
    {
        public static string AttributeID = "attributeID";
        public static string AttributeSyntax = "attributeSyntax";
        public static string AuxiliaryClass = "auxiliaryClass";
        public static string BecomeDomainMaster = "becomeDomainMaster";
        public static string BecomeInfrastructureMaster = "becomeInfrastructureMaster";
        public static string BecomePdc = "becomePdc";
        public static string BecomeRidMaster = "becomeRidMaster";
        public static string BecomeSchemaMaster = "becomeSchemaMaster";
        public static string Cn = "cn";
        public static string ConfigurationNamingContext = "configurationNamingContext";
        public static string Container = "container";
        public static string CurrentTime = "currentTime";
        public static string DefaultNamingContext = "defaultNamingContext";
        public static string DefaultSecurityDescriptor = "defaultSecurityDescriptor";
        public static string Description = "description";
        public static string DistinguishedName = "distinguishedName";
        public static string DnsHostName = "dnsHostName";
        public static string DnsRoot = "dnsRoot";
        public static string DomainDNS = "domainDNS";
        public static string DomainFunctionality = "domainFunctionality";
        public static string DsServiceName = "dsServiceName";
        public static string Enabled = "Enabled";
        public static string Flags = "flags";
        public static string FlatName = "flatName";
        public static string ForestFunctionality = "forestFunctionality";
        public static string FromServer = "fromServer";
        public static string FsmoRoleOwner = "fsmoRoleOwner";
        public static string GovernsID = "governsID";
        public static string HasMasterNCs = "hasMasterNCs";
        public static string HasPartialReplicaNCs = "hasPartialReplicaNCs";
        public static string HighestCommittedUSN = "highestCommittedUSN";
        public static string InstanceType = "instanceType";
        public static string InterSiteTopologyGenerator = "interSiteTopologyGenerator";
        public static string IsDefunct = "isDefunct";
        public static string IsGlobalCatalogReady = "isGlobalCatalogReady";
        public static string IsMemberOfPartialAttributeSet = "isMemberOfPartialAttributeSet";
        public static string IsSingleValued = "isSingleValued";
        public static string Keywords = "keywords";
        public static string LdapDisplayName = "ldapDisplayName";
        public static string LinkID = "linkID";
        public static string MayContain = "mayContain";
        public static string MsDSBehaviorVersion = "msDS-Behavior-Version";
        public static string MsDSDefaultNamingContext = "msDS-DefaultNamingContext";
        public static string MsDSHasFullReplicaNCs = "msDS-hasFullReplicaNCs";
        public static string MsDSHasInstantiatedNCs = "msDS-HasInstantiatedNCs";
        public static string MsDSHasMasterNCs = "msDS-HasMasterNCs";
        public static string MsDSMasteredBy = "msDS-masteredBy";
        public static string MsDSNCReplicaLocations = "msDS-NC-Replica-Locations";
        public static string MsDSNCROReplicaLocations = "msDS-NC-RO-Replica-Locations";
        public static string MsDSPortLDAP = "msDS-PortLDAP";
        public static string MsDSPortSSL = "msDS-PortSSL";
        public static string MsDSReplAuthenticationMode = "msDS-ReplAuthenticationMode";
        public static string MsDSSDReferenceDomain = "msDS-SDReferenceDomain";
        public static string MustContain = "mustContain";
        public static string Name = "name";
        public static string NamingContexts = "namingContexts";
        public static string NCName = "nCName";
        public static string NETBIOSName = "nETBIOSName";
        public static string NTMixedDomain = "ntMixedDomain";
        public static string NTSecurityDescriptor = "ntSecurityDescriptor";
        public static string ObjectCategory = "objectCategory";
        public static string ObjectClassCategory = "objectClassCategory";
        public static string ObjectGuid = "objectGuid";
        public static string ObjectVersion = "objectVersion";
        public static string OMObjectClass = "oMObjectClass";
        public static string OMSyntax = "oMSyntax";
        public static string OperatingSystem = "operatingSystem";
        public static string OperatingSystemVersion = "operatingSystemVersion";
        public static string Options = "options";
        public static string PossibleInferiors = "possibleInferiors";
        public static string PossibleSuperiors = "possSuperiors";
        public static string RangeLower = "rangeLower";
        public static string RangeUpper = "rangeUpper";
        public static string ReplicateSingleObject = "replicateSingleObject";
        public static string RootDomainNamingContext = "rootDomainNamingContext";
        public static string SchemaIDGuid = "schemaIDGUID";
        public static string SchemaNamingContext = "schemaNamingContext";
        public static string SchemaUpdateNow = "schemaUpdateNow";
        public static string SearchFlags = "searchFlags";
        public static string ServerName = "serverName";
        public static string ServiceBindingInformation = "serviceBindingInformation";
        public static string SiteList = "siteList";
        public static string SubClassOf = "subClassOf";
        public static string SupportedCapabilities = "supportedCapabilities";
        public static string SystemAuxiliaryClass = "systemAuxiliaryClass";
        public static string SystemFlags = "systemFlags";
        public static string SystemMayContain = "systemMayContain";
        public static string SystemMustContain = "systemMustContain";
        public static string SystemPossibleSuperiors = "systemPossSuperiors";
        public static string TrustAttributes = "trustAttributes";
        public static string TrustParent = "trustParent";
        public static string TrustType = "trustType";

        public static object GetPropertyValue(DirectoryEntry directoryEntry, string propertyName)
        {
            return GetPropertyValue(null, directoryEntry, propertyName);
        }

        public static object GetPropertyValue(DirectoryContext context, DirectoryEntry directoryEntry, string propertyName)
        {
            try
            {
                if (directoryEntry.Properties[propertyName].Count == 0)
                {
                    if (directoryEntry.Properties[DistinguishedName].Count != 0)
                    {
                        throw new ActiveDirectoryOperationException(Res.GetString("PropertyNotFoundOnObject", new object[] { propertyName, directoryEntry.Properties[DistinguishedName].Value }));
                    }
                    throw new ActiveDirectoryOperationException(Res.GetString("PropertyNotFound", new object[] { propertyName }));
                }
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            return directoryEntry.Properties[propertyName].Value;
        }

        public static object GetSearchResultPropertyValue(SearchResult res, string propertyName)
        {
            ResultPropertyValueCollection values = null;
            try
            {
                values = res.Properties[propertyName];
                if ((values == null) || (values.Count < 1))
                {
                    throw new ActiveDirectoryOperationException(Res.GetString("PropertyNotFound", new object[] { propertyName }));
                }
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(exception);
            }
            return values[0];
        }
    }
}

