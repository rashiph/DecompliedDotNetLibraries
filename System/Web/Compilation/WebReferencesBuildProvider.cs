namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Services.Description;
    using System.Web.Services.Discovery;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml.Serialization;

    internal class WebReferencesBuildProvider : BuildProvider
    {
        private VirtualDirectory _vdir;
        private const string IndigoWebRefProviderTypeName = "System.Web.Compilation.WCFBuildProvider";
        private static Type s_indigoWebRefProviderType;
        private static bool s_triedToGetWebRefType;

        internal WebReferencesBuildProvider(VirtualDirectory vdir)
        {
            this._vdir = vdir;
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            string str2;
            if (!s_triedToGetWebRefType)
            {
                s_indigoWebRefProviderType = BuildManager.GetType("System.Web.Compilation.WCFBuildProvider", false);
                s_triedToGetWebRefType = true;
            }
            if (s_indigoWebRefProviderType != null)
            {
                BuildProvider provider = (BuildProvider) HttpRuntime.CreateNonPublicInstance(s_indigoWebRefProviderType);
                provider.SetVirtualPath(base.VirtualPathObject);
                provider.GenerateCode(assemblyBuilder);
            }
            VirtualPath webRefDirectoryVirtualPath = HttpRuntime.WebRefDirectoryVirtualPath;
            string virtualPath = this._vdir.VirtualPath;
            if (webRefDirectoryVirtualPath.VirtualPathString.Length == virtualPath.Length)
            {
                str2 = string.Empty;
            }
            else
            {
                string[] strArray = UrlPath.RemoveSlashFromPathIfNeeded(virtualPath).Substring(webRefDirectoryVirtualPath.VirtualPathString.Length).Split(new char[] { '/' });
                for (int i = 0; i < strArray.Length; i++)
                {
                    strArray[i] = Util.MakeValidTypeNameFromString(strArray[i]);
                }
                str2 = string.Join(".", strArray);
            }
            CodeNamespace proxyCode = new CodeNamespace(str2);
            WebReferenceCollection webReferences = new WebReferenceCollection();
            bool flag = false;
            foreach (VirtualFile file in this._vdir.Files)
            {
                if (UrlPath.GetExtension(file.VirtualPath).ToLower(CultureInfo.InvariantCulture) == ".discomap")
                {
                    string topLevelFilename = HostingEnvironment.MapPath(file.VirtualPath);
                    DiscoveryClientProtocol protocol = new DiscoveryClientProtocol {
                        AllowAutoRedirect = true,
                        Credentials = CredentialCache.DefaultCredentials
                    };
                    protocol.ReadAll(topLevelFilename);
                    WebReference reference = new WebReference(protocol.Documents, proxyCode);
                    string str5 = Path.ChangeExtension(UrlPath.GetFileName(file.VirtualPath), null);
                    string appSettingUrlKey = str2 + "." + str5;
                    WebReference webReference = new WebReference(protocol.Documents, proxyCode, reference.ProtocolName, appSettingUrlKey, null);
                    webReferences.Add(webReference);
                    flag = true;
                }
            }
            if (flag)
            {
                CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
                codeCompileUnit.Namespaces.Add(proxyCode);
                WebReferenceOptions options = new WebReferenceOptions {
                    CodeGenerationOptions = CodeGenerationOptions.GenerateOldAsync | CodeGenerationOptions.GenerateNewAsync | CodeGenerationOptions.GenerateProperties,
                    Style = ServiceDescriptionImportStyle.Client,
                    Verbose = true
                };
                ServiceDescriptionImporter.GenerateWebReferences(webReferences, assemblyBuilder.CodeDomProvider, codeCompileUnit, options);
                assemblyBuilder.AddCodeCompileUnit(this, codeCompileUnit);
            }
        }
    }
}

