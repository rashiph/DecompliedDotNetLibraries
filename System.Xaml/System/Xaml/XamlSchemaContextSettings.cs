namespace System.Xaml
{
    using System;
    using System.Runtime.CompilerServices;

    public class XamlSchemaContextSettings
    {
        public XamlSchemaContextSettings()
        {
        }

        public XamlSchemaContextSettings(XamlSchemaContextSettings settings)
        {
            if (settings != null)
            {
                this.SupportMarkupExtensionsWithDuplicateArity = settings.SupportMarkupExtensionsWithDuplicateArity;
                this.FullyQualifyAssemblyNamesInClrNamespaces = settings.FullyQualifyAssemblyNamesInClrNamespaces;
            }
        }

        public bool FullyQualifyAssemblyNamesInClrNamespaces { get; set; }

        public bool SupportMarkupExtensionsWithDuplicateArity { get; set; }
    }
}

