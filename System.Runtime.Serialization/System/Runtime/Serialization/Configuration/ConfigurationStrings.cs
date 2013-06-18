namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Runtime;

    internal static class ConfigurationStrings
    {
        internal const string DataContractSerializerSectionName = "dataContractSerializer";
        internal const string DeclaredTypes = "declaredTypes";
        internal const string DefaultCollectionName = "";
        internal const string Index = "index";
        internal const string Parameter = "parameter";
        internal const string SectionGroupName = "system.runtime.serialization";
        internal const string Type = "type";

        private static string GetSectionPath(string sectionName)
        {
            return ("system.runtime.serialization" + "/" + sectionName);
        }

        internal static string DataContractSerializerSectionPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return GetSectionPath("dataContractSerializer");
            }
        }
    }
}

