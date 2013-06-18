namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Resources.Tools;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using System.Xml.Schema;

    [BuildProviderAppliesTo(BuildProviderAppliesTo.Resources)]
    internal abstract class BaseResourcesBuildProvider : BuildProvider
    {
        private string _cultureName;
        private bool _dontGenerateStronglyTypedClass;
        private string _ns;
        private string _typeName;
        internal const string DefaultResourcesNamespace = "Resources";

        protected BaseResourcesBuildProvider()
        {
        }

        internal void DontGenerateStronglyTypedClass()
        {
            this._dontGenerateStronglyTypedClass = true;
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            this._cultureName = base.GetCultureName();
            if (!this._dontGenerateStronglyTypedClass)
            {
                this._ns = Util.GetNamespaceAndTypeNameFromVirtualPath(base.VirtualPathObject, (this._cultureName == null) ? 1 : 2, out this._typeName);
                if (this._ns.Length == 0)
                {
                    this._ns = "Resources";
                }
                else
                {
                    this._ns = "Resources." + this._ns;
                }
            }
            using (Stream stream = base.OpenStream())
            {
                IResourceReader resourceReader = this.GetResourceReader(stream);
                try
                {
                    this.GenerateResourceFile(assemblyBuilder, resourceReader);
                }
                catch (ArgumentException exception)
                {
                    if ((exception.InnerException == null) || (!(exception.InnerException is XmlException) && !(exception.InnerException is XmlSchemaException)))
                    {
                        throw;
                    }
                    throw exception.InnerException;
                }
                if ((this._cultureName == null) && !this._dontGenerateStronglyTypedClass)
                {
                    this.GenerateStronglyTypedClass(assemblyBuilder, resourceReader);
                }
            }
        }

        private void GenerateResourceFile(AssemblyBuilder assemblyBuilder, IResourceReader reader)
        {
            string str;
            if (this._ns == null)
            {
                str = UrlPath.GetFileNameWithoutExtension(base.VirtualPath) + ".resources";
            }
            else if (this._cultureName == null)
            {
                str = this._ns + "." + this._typeName + ".resources";
            }
            else
            {
                str = this._ns + "." + this._typeName + "." + this._cultureName + ".resources";
            }
            str = str.ToLower(CultureInfo.InvariantCulture);
            Stream stream = null;
            try
            {
                try
                {
                    try
                    {
                    }
                    finally
                    {
                        stream = assemblyBuilder.CreateEmbeddedResource(this, str);
                    }
                }
                catch (ArgumentException)
                {
                    throw new HttpException(System.Web.SR.GetString("Duplicate_Resource_File", new object[] { base.VirtualPath }));
                }
                using (stream)
                {
                    using (ResourceWriter writer = new ResourceWriter(stream))
                    {
                        writer.TypeNameConverter = new Func<Type, string>(TargetFrameworkUtil.TypeNameConverter);
                        foreach (DictionaryEntry entry in reader)
                        {
                            writer.AddResource((string) entry.Key, entry.Value);
                        }
                    }
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        private void GenerateStronglyTypedClass(AssemblyBuilder assemblyBuilder, IResourceReader reader)
        {
            IDictionary resourceList;
            string[] strArray;
            using (reader)
            {
                resourceList = this.GetResourceList(reader);
            }
            CodeDomProvider codeDomProvider = assemblyBuilder.CodeDomProvider;
            CodeCompileUnit compileUnit = StronglyTypedResourceBuilder.Create(resourceList, this._typeName, this._ns, codeDomProvider, false, out strArray);
            assemblyBuilder.AddCodeCompileUnit(this, compileUnit);
        }

        private IDictionary GetResourceList(IResourceReader reader)
        {
            IDictionary dictionary = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in reader)
            {
                dictionary.Add(entry.Key, entry.Value);
            }
            return dictionary;
        }

        protected abstract IResourceReader GetResourceReader(Stream inputStream);
    }
}

