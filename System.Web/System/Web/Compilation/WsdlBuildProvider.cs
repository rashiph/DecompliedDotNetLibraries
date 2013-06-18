namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.IO;
    using System.Web.Services.Description;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.Serialization;

    [BuildProviderAppliesTo(BuildProviderAppliesTo.Code)]
    internal class WsdlBuildProvider : BuildProvider
    {
        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            ServiceDescription description;
            string namespaceFromVirtualPath = Util.GetNamespaceFromVirtualPath(base.VirtualPathObject);
            Stream stream = base.VirtualPathObject.OpenFile();
            try
            {
                description = ServiceDescription.Read(stream);
            }
            catch (InvalidOperationException exception)
            {
                XmlException innerException = exception.InnerException as XmlException;
                if (innerException != null)
                {
                    throw innerException;
                }
                throw;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
            ServiceDescriptionImporter importer = new ServiceDescriptionImporter {
                CodeGenerator = assemblyBuilder.CodeDomProvider,
                CodeGenerationOptions = CodeGenerationOptions.GenerateOldAsync | CodeGenerationOptions.GenerateNewAsync | CodeGenerationOptions.GenerateProperties
            };
            importer.ServiceDescriptions.Add(description);
            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            CodeNamespace namespace2 = new CodeNamespace(namespaceFromVirtualPath);
            codeCompileUnit.Namespaces.Add(namespace2);
            importer.Import(namespace2, codeCompileUnit);
            assemblyBuilder.AddCodeCompileUnit(this, codeCompileUnit);
        }
    }
}

