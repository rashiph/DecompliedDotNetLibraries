namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    public abstract class ServiceModelEnhancedConfigurationElementCollection<TConfigurationElement> : ServiceModelConfigurationElementCollection<TConfigurationElement> where TConfigurationElement: ConfigurationElement, new()
    {
        internal ServiceModelEnhancedConfigurationElementCollection(string elementName) : base(ConfigurationElementCollectionType.AddRemoveClearMap, elementName)
        {
            base.AddElementName = elementName;
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            object elementKey = this.GetElementKey(element);
            if (this.ContainsKey(elementKey))
            {
                ConfigurationElement element2 = base.BaseGet(elementKey);
                if (element2 != null)
                {
                    if (element2.ElementInformation.IsPresent)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigDuplicateKeyAtSameScope", new object[] { this.ElementName, elementKey })));
                    }
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>(6);
                        dictionary.Add("ElementName", this.ElementName);
                        dictionary.Add("Name", elementKey.ToString());
                        dictionary.Add("OldElementLocation", element2.ElementInformation.Source);
                        dictionary.Add("OldElementLineNumber", element2.ElementInformation.LineNumber.ToString(NumberFormatInfo.CurrentInfo));
                        dictionary.Add("NewElementLocation", element.ElementInformation.Source);
                        dictionary.Add("NewElementLineNumber", element.ElementInformation.LineNumber.ToString(NumberFormatInfo.CurrentInfo));
                        DictionaryTraceRecord extendedData = new DictionaryTraceRecord(dictionary);
                        TraceUtility.TraceEvent(TraceEventType.Warning, 0x80029, System.ServiceModel.SR.GetString("TraceCodeOverridingDuplicateConfigurationKey"), extendedData, this, null);
                    }
                }
            }
            base.BaseAdd(element);
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return false;
            }
        }
    }
}

