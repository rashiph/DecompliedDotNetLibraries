namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Diagnostics;

    internal static class ConfigurationHelpers
    {
        internal static BindingCollectionElement GetAssociatedBindingCollectionElement(ContextInformation evaluationContext, string bindingCollectionName)
        {
            BindingCollectionElement element = null;
            BindingsSection associatedSection = (BindingsSection) GetAssociatedSection(evaluationContext, ConfigurationStrings.BindingsSectionGroupPath);
            if (associatedSection != null)
            {
                associatedSection.UpdateBindingSections(evaluationContext);
                try
                {
                    element = associatedSection[bindingCollectionName];
                }
                catch (KeyNotFoundException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigBindingExtensionNotFound", new object[] { GetBindingsSectionPath(bindingCollectionName) })));
                }
                catch (NullReferenceException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigBindingExtensionNotFound", new object[] { GetBindingsSectionPath(bindingCollectionName) })));
                }
            }
            return element;
        }

        internal static EndpointCollectionElement GetAssociatedEndpointCollectionElement(ContextInformation evaluationContext, string endpointCollectionName)
        {
            EndpointCollectionElement element = null;
            StandardEndpointsSection associatedSection = (StandardEndpointsSection) GetAssociatedSection(evaluationContext, ConfigurationStrings.StandardEndpointsSectionPath);
            if (associatedSection != null)
            {
                associatedSection.UpdateEndpointSections(evaluationContext);
                try
                {
                    element = associatedSection[endpointCollectionName];
                }
                catch (KeyNotFoundException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigEndpointExtensionNotFound", new object[] { GetEndpointsSectionPath(endpointCollectionName) })));
                }
                catch (NullReferenceException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigEndpointExtensionNotFound", new object[] { GetEndpointsSectionPath(endpointCollectionName) })));
                }
            }
            return element;
        }

        internal static object GetAssociatedSection(ContextInformation evalContext, string sectionPath)
        {
            object section = null;
            if (evalContext != null)
            {
                section = evalContext.GetSection(sectionPath);
            }
            else
            {
                section = AspNetEnvironment.Current.GetConfigurationSection(sectionPath);
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80024, System.ServiceModel.SR.GetString("TraceCodeGetConfigurationSection"), new StringTraceRecord("ConfigurationSection", sectionPath), null, null);
                }
            }
            if (section == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigSectionNotFound", new object[] { sectionPath })));
            }
            return section;
        }

        internal static BindingCollectionElement GetBindingCollectionElement(string bindingCollectionName)
        {
            return GetAssociatedBindingCollectionElement(null, bindingCollectionName);
        }

        internal static string GetBindingsSectionPath(string sectionName)
        {
            return (ConfigurationStrings.BindingsSectionGroupPath + "/" + sectionName);
        }

        internal static EndpointCollectionElement GetEndpointCollectionElement(string endpointCollectionName)
        {
            return GetAssociatedEndpointCollectionElement(null, endpointCollectionName);
        }

        internal static string GetEndpointsSectionPath(string sectionName)
        {
            return ("standardEndpoints" + "/" + sectionName);
        }

        internal static ContextInformation GetEvaluationContext(IConfigurationContextProviderInternal provider)
        {
            if (provider != null)
            {
                try
                {
                    return provider.GetEvaluationContext();
                }
                catch (ConfigurationErrorsException)
                {
                }
            }
            return null;
        }

        internal static ContextInformation GetOriginalEvaluationContext(IConfigurationContextProviderInternal provider)
        {
            if (provider != null)
            {
                try
                {
                    return provider.GetOriginalEvaluationContext();
                }
                catch (ConfigurationErrorsException)
                {
                }
            }
            return null;
        }

        internal static object GetSection(string sectionPath)
        {
            return GetAssociatedSection(null, sectionPath);
        }

        internal static string GetSectionPath(string sectionName)
        {
            return ("system.serviceModel" + "/" + sectionName);
        }

        [SecurityCritical]
        internal static void SetIsPresent(ConfigurationElement element)
        {
            SetIsPresentWithAssert(element.GetType().GetProperty("ElementPresent", BindingFlags.NonPublic | BindingFlags.Instance), element, true);
        }

        [SecurityCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private static void SetIsPresentWithAssert(PropertyInfo elementPresent, ConfigurationElement element, bool value)
        {
            elementPresent.SetValue(element, value, null);
        }

        internal static void TraceExtensionTypeNotFound(ExtensionElement extensionElement)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
                dictionary.Add("ExtensionName", extensionElement.Name);
                dictionary.Add("ExtensionType", extensionElement.Type);
                DictionaryTraceRecord extendedData = new DictionaryTraceRecord(dictionary);
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x80045, System.ServiceModel.SR.GetString("TraceCodeExtensionTypeNotFound"), extendedData, null, null);
            }
        }

        [SecurityCritical]
        internal static BindingCollectionElement UnsafeGetAssociatedBindingCollectionElement(ContextInformation evaluationContext, string bindingCollectionName)
        {
            BindingCollectionElement element = null;
            BindingsSection section = (BindingsSection) UnsafeGetAssociatedSection(evaluationContext, ConfigurationStrings.BindingsSectionGroupPath);
            if (section != null)
            {
                section.UpdateBindingSections(evaluationContext);
                try
                {
                    element = section[bindingCollectionName];
                }
                catch (KeyNotFoundException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigBindingExtensionNotFound", new object[] { GetBindingsSectionPath(bindingCollectionName) })));
                }
                catch (NullReferenceException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigBindingExtensionNotFound", new object[] { GetBindingsSectionPath(bindingCollectionName) })));
                }
            }
            return element;
        }

        [SecurityCritical]
        internal static EndpointCollectionElement UnsafeGetAssociatedEndpointCollectionElement(ContextInformation evaluationContext, string endpointCollectionName)
        {
            EndpointCollectionElement element = null;
            StandardEndpointsSection section = (StandardEndpointsSection) UnsafeGetAssociatedSection(evaluationContext, ConfigurationStrings.StandardEndpointsSectionPath);
            if (section != null)
            {
                section.UpdateEndpointSections(evaluationContext);
                try
                {
                    element = section[endpointCollectionName];
                }
                catch (KeyNotFoundException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigEndpointExtensionNotFound", new object[] { GetEndpointsSectionPath(endpointCollectionName) })));
                }
                catch (NullReferenceException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigEndpointExtensionNotFound", new object[] { GetEndpointsSectionPath(endpointCollectionName) })));
                }
            }
            return element;
        }

        [SecurityCritical]
        internal static object UnsafeGetAssociatedSection(ContextInformation evalContext, string sectionPath)
        {
            object obj2 = null;
            if (evalContext != null)
            {
                obj2 = UnsafeGetSectionFromContext(evalContext, sectionPath);
            }
            else
            {
                obj2 = AspNetEnvironment.Current.UnsafeGetConfigurationSection(sectionPath);
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80024, System.ServiceModel.SR.GetString("TraceCodeGetConfigurationSection"), new StringTraceRecord("ConfigurationSection", sectionPath), null, null);
                }
            }
            if (obj2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigSectionNotFound", new object[] { sectionPath })));
            }
            return obj2;
        }

        [SecurityCritical]
        internal static BindingCollectionElement UnsafeGetBindingCollectionElement(string bindingCollectionName)
        {
            return UnsafeGetAssociatedBindingCollectionElement(null, bindingCollectionName);
        }

        [SecurityCritical]
        internal static EndpointCollectionElement UnsafeGetEndpointCollectionElement(string endpointCollectionName)
        {
            return UnsafeGetAssociatedEndpointCollectionElement(null, endpointCollectionName);
        }

        [SecurityCritical]
        internal static object UnsafeGetSection(string sectionPath)
        {
            return UnsafeGetAssociatedSection(null, sectionPath);
        }

        [SecurityCritical, ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        internal static object UnsafeGetSectionFromContext(ContextInformation evalContext, string sectionPath)
        {
            return evalContext.GetSection(sectionPath);
        }

        [SecurityCritical]
        internal static object UnsafeGetSectionNoTrace(string sectionPath)
        {
            object obj2 = AspNetEnvironment.Current.UnsafeGetConfigurationSection(sectionPath);
            if (obj2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigSectionNotFound", new object[] { sectionPath })));
            }
            return obj2;
        }
    }
}

