namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Policy;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    [Guid("1D9FE38A-0226-4b95-9C6B-6DFFA2236270"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class BootstrapperBuilder : IBootstrapperBuilder
    {
        private const string BOOTSTRAPPER_NAMESPACE = "http://schemas.microsoft.com/developer/2004/01/bootstrapper";
        private const string BOOTSTRAPPER_PREFIX = "bootstrapper";
        private const string CHILD_MANIFEST_FILE = "package.xml";
        private const string CONFIG_TRANSFORM = "xmltoconfig.xsl";
        private Hashtable cultures = new Hashtable();
        private XmlDocument document;
        private const string ENGINE_PATH = "Engine";
        private const string EULA_ATTRIBUTE = "LicenseAgreement";
        private bool fInitialized;
        private bool fValidate = true;
        private const string HASH_ATTRIBUTE = "Hash";
        private const string HOMESITE_ATTRIBUTE = "HomeSite";
        private static readonly bool logging = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSPLOG"));
        private static readonly string logPath = GetLogPath();
        private BuildResults loopDependenciesWarnings;
        private const string MANIFEST_FILE_SCHEMA = "package.xsd";
        private const int MESSAGE_TABLE = 0x2b;
        private const string PACKAGE_PATH = "Packages";
        private string path = Util.DefaultPath;
        private ProductCollection products = new ProductCollection();
        private const string PUBLICKEY_ATTRIBUTE = "PublicKey";
        private const int RESOURCE_TABLE = 0x2d;
        private const string RESOURCES_PATH = "";
        private BuildResults results;
        private const string ROOT_MANIFEST_FILE = "product.xml";
        private const string SCHEMA_PATH = "Schemas";
        private const string SETUP_BIN = "setup.bin";
        private const string SETUP_EXE = "setup.exe";
        private const string SETUP_RESOURCES_FILE = "setup.xml";
        private const string URLNAME_ATTRIBUTE = "UrlName";
        private Hashtable validationResults = new Hashtable();
        private XmlNamespaceManager xmlNamespaceManager;

        private void AddAttribute(XmlNode node, string attributeName, string attributeValue)
        {
            XmlAttribute attribute = node.OwnerDocument.CreateAttribute(attributeName);
            attribute.Value = attributeValue;
            node.Attributes.Append(attribute);
        }

        private void AddBuiltProducts(BuildSettings settings)
        {
            Dictionary<string, ProductBuilder> dictionary = new Dictionary<string, ProductBuilder>();
            Dictionary<string, Product> output = new Dictionary<string, Product>();
            if ((this.loopDependenciesWarnings != null) && (this.loopDependenciesWarnings.Messages != null))
            {
                foreach (BuildMessage message in this.loopDependenciesWarnings.Messages)
                {
                    this.results.AddMessage(message);
                }
            }
            foreach (ProductBuilder builder in settings.ProductBuilders)
            {
                dictionary.Add(builder.Product.ProductCode.ToLowerInvariant(), builder);
                this.Merge(output, this.GetIncludedProducts(builder.Product));
                this.AddProduct(output, builder.Product);
            }
            foreach (ProductBuilder builder2 in settings.ProductBuilders)
            {
                foreach (Product product in this.GetIncludedProducts(builder2.Product).Values)
                {
                    if (dictionary.ContainsKey(product.ProductCode.ToLowerInvariant()))
                    {
                        this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.IncludedProductIncluded", new object[] { builder2.Name, product.Name }));
                    }
                }
                foreach (List<Product> list in builder2.Product.Dependencies)
                {
                    bool flag = false;
                    foreach (Product product2 in list)
                    {
                        if (output.ContainsKey(product2.ProductCode.ToLowerInvariant()))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        if (list.Count == 1)
                        {
                            this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.MissingDependency", new object[] { list[0].Name, builder2.Name }));
                        }
                        else
                        {
                            StringBuilder builder3 = new StringBuilder();
                            foreach (Product product3 in list)
                            {
                                builder3.Append(product3.Name);
                                builder3.Append(", ");
                            }
                            string str = builder3.ToString();
                            str = str.Substring(0, str.Length - 2);
                            this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.MissingDependencyMultiple", new object[] { str, builder2.Name }));
                        }
                    }
                }
                foreach (ArrayList list2 in builder2.Product.MissingDependencies)
                {
                    if (list2.Count == 1)
                    {
                        this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.DependencyNotFound", new object[] { builder2.Name, list2[0] }));
                    }
                    else
                    {
                        StringBuilder builder4 = new StringBuilder();
                        foreach (string str2 in list2)
                        {
                            builder4.Append(str2);
                            builder4.Append(", ");
                        }
                        string str3 = builder4.ToString();
                        str3 = str3.Substring(0, str3.Length - 2);
                        this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.MultipleDependeciesNotFound", new object[] { builder2.Name, str3 }));
                    }
                }
            }
        }

        private void AddDependencies(Product p, Hashtable availableProducts)
        {
            foreach (string str in this.SelectRelatedProducts(p, "DependsOnProduct"))
            {
                if (availableProducts.Contains(str))
                {
                    p.AddDependentProduct((Product) availableProducts[str]);
                }
                else
                {
                    ArrayList productCodes = new ArrayList();
                    productCodes.Add(str);
                    p.AddMissingDependency(productCodes);
                }
            }
            foreach (XmlNode node in this.SelectEitherProducts(p))
            {
                List<Product> dependenciesToCheck = new List<Product>();
                ArrayList list3 = new ArrayList();
                foreach (XmlNode node2 in node.SelectNodes(string.Format(CultureInfo.InvariantCulture, "{0}:DependsOnProduct", new object[] { "bootstrapper" }), this.xmlNamespaceManager))
                {
                    XmlAttribute namedItem = (XmlAttribute) node2.Attributes.GetNamedItem("Code");
                    if (namedItem != null)
                    {
                        string key = namedItem.Value;
                        if (availableProducts.Contains(key))
                        {
                            dependenciesToCheck.Add((Product) availableProducts[key]);
                        }
                        list3.Add(key);
                    }
                }
                if (dependenciesToCheck.Count > 0)
                {
                    if (!p.ContainsDependencies(dependenciesToCheck))
                    {
                        p.Dependencies.Add(dependenciesToCheck);
                    }
                }
                else if (list3.Count > 0)
                {
                    p.AddMissingDependency(list3);
                }
            }
        }

        private void AddIncludedProducts(Product product, Dictionary<string, Product> includedProducts)
        {
            if (!includedProducts.ContainsKey(product.ProductCode))
            {
                includedProducts.Add(product.ProductCode, product);
                foreach (Product product2 in product.Includes)
                {
                    this.AddIncludedProducts(product2, includedProducts);
                }
            }
        }

        private void AddIncludes(Product p, Hashtable availableProducts)
        {
            foreach (string str in this.SelectRelatedProducts(p, "IncludesProduct"))
            {
                if (availableProducts.Contains(str))
                {
                    p.Includes.Add((Product) availableProducts[str]);
                }
            }
        }

        private void AddProduct(Dictionary<string, Product> output, Product product)
        {
            if (!output.ContainsKey(product.ProductCode.ToLowerInvariant()))
            {
                output.Add(product.ProductCode.ToLowerInvariant(), product);
            }
        }

        private void AddStringResourceForUrl(ResourceUpdater resourceUpdater, string name, string url, string nameToUseInLog)
        {
            if (!string.IsNullOrEmpty(url))
            {
                resourceUpdater.AddStringResource(40, name, url);
                if (!Util.IsWebUrl(url) && !Util.IsUncPath(url))
                {
                    this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.InvalidUrl", new object[] { nameToUseInLog, url }));
                }
            }
        }

        private bool AddVerificationInformation(XmlNode packageFileNode, string fileSource, string fileName, ProductBuilder builder, BuildSettings settings, BuildResults results)
        {
            XmlAttribute attribute = packageFileNode.Attributes["Hash"];
            XmlAttribute attribute2 = packageFileNode.Attributes["PublicKey"];
            if (File.Exists(fileSource))
            {
                string publicKeyOfFile = this.GetPublicKeyOfFile(fileSource);
                if ((attribute == null) && (attribute2 == null))
                {
                    if (publicKeyOfFile != null)
                    {
                        this.AddAttribute(packageFileNode, "PublicKey", publicKeyOfFile);
                    }
                    else
                    {
                        this.AddAttribute(packageFileNode, "Hash", this.GetFileHash(fileSource));
                    }
                }
                if (attribute2 != null)
                {
                    if (publicKeyOfFile != null)
                    {
                        this.ReplaceAttribute(packageFileNode, "PublicKey", publicKeyOfFile);
                    }
                    else
                    {
                        packageFileNode.Attributes.RemoveNamedItem("PublicKey");
                        if (attribute == null)
                        {
                            this.AddAttribute(packageFileNode, "Hash", this.GetFileHash(fileSource));
                        }
                    }
                    if (((publicKeyOfFile == null) || !publicKeyOfFile.ToLowerInvariant().Equals(attribute2.Value.ToLowerInvariant())) && (results != null))
                    {
                        results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.DifferingPublicKeys", new object[] { "PublicKey", builder.Name, fileSource }));
                    }
                }
                if (attribute != null)
                {
                    string fileHash = this.GetFileHash(fileSource);
                    this.ReplaceAttribute(packageFileNode, "Hash", fileHash);
                    if (!fileHash.ToLowerInvariant().Equals(attribute.Value.ToLowerInvariant()) && (results != null))
                    {
                        results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.DifferingPublicKeys", new object[] { "Hash", builder.Name, fileSource }));
                    }
                }
            }
            else if (((settings.ComponentsLocation == ComponentsLocation.HomeSite) && (attribute == null)) && (attribute2 == null))
            {
                if (results != null)
                {
                    results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.MissingVerificationInformation", new object[] { fileName, builder.Name }));
                }
                return false;
            }
            return true;
        }

        private void AppendNode(XmlElement element, string nodeName, XmlElement mergeElement)
        {
            XmlNode newChild = element.SelectSingleNode("bootstrapper:" + nodeName, this.xmlNamespaceManager);
            if (newChild != null)
            {
                mergeElement.AppendChild(newChild);
            }
        }

        public BuildResults Build(BuildSettings settings)
        {
            this.results = new BuildResults();
            try
            {
                if ((settings.ApplicationFile == null) && ((settings.ProductBuilders == null) || (settings.ProductBuilders.Count == 0)))
                {
                    this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.InvalidInput", new object[0]));
                    return this.results;
                }
                if (string.IsNullOrEmpty(settings.OutputPath))
                {
                    this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.NoOutputPath", new object[0]));
                    return this.results;
                }
                if (!this.fInitialized)
                {
                    this.Refresh();
                }
                if (string.IsNullOrEmpty(settings.Culture))
                {
                    settings.Culture = this.MapLCIDToCultureName(settings.LCID);
                }
                if (string.IsNullOrEmpty(settings.FallbackCulture))
                {
                    settings.FallbackCulture = this.MapLCIDToCultureName(settings.FallbackLCID);
                }
                this.AddBuiltProducts(settings);
                ArrayList filesCopied = new ArrayList();
                string strOutputExe = System.IO.Path.Combine(settings.OutputPath, "setup.exe");
                if (!this.CopySetupToOutputDirectory(settings, strOutputExe))
                {
                    return this.results;
                }
                ResourceUpdater resourceUpdater = new ResourceUpdater();
                if (!this.BuildResources(settings, resourceUpdater))
                {
                    return this.results;
                }
                this.AddStringResourceForUrl(resourceUpdater, "BASEURL", settings.ApplicationUrl, "ApplicationUrl");
                this.AddStringResourceForUrl(resourceUpdater, "COMPONENTSURL", settings.ComponentsUrl, "ComponentsUrl");
                this.AddStringResourceForUrl(resourceUpdater, "SUPPORTURL", settings.SupportUrl, "SupportUrl");
                if (settings.ComponentsLocation == ComponentsLocation.HomeSite)
                {
                    resourceUpdater.AddStringResource(40, "HOMESITE", true.ToString());
                }
                XmlElement configElement = this.document.CreateElement("Configuration");
                XmlElement newChild = this.CreateApplicationElement(configElement, settings);
                if (newChild != null)
                {
                    configElement.AppendChild(newChild);
                }
                Hashtable eulas = new Hashtable();
                if (!this.BuildPackages(settings, configElement, resourceUpdater, filesCopied, eulas))
                {
                    return this.results;
                }
                this.DumpXmlToFile(configElement, "bootstrapper.cfg.xml");
                string data = this.XmlToConfigurationFile(configElement);
                resourceUpdater.AddStringResource(0x29, "SETUPCFG", data);
                this.DumpStringToFile(data, "bootstrapper.cfg", false);
                foreach (object obj2 in eulas.Values)
                {
                    string str3;
                    DictionaryEntry entry = (DictionaryEntry) obj2;
                    FileInfo info = new FileInfo(entry.Value.ToString());
                    using (FileStream stream = info.OpenRead())
                    {
                        str3 = new StreamReader(stream).ReadToEnd();
                    }
                    resourceUpdater.AddStringResource(0x2c, entry.Key.ToString(), str3);
                }
                resourceUpdater.AddStringResource(0x2c, "COUNT", eulas.Count.ToString(CultureInfo.InvariantCulture));
                if (!resourceUpdater.UpdateResources(strOutputExe, this.results))
                {
                    return this.results;
                }
                this.results.SetKeyFile(strOutputExe);
                string[] array = new string[filesCopied.Count];
                filesCopied.CopyTo(array);
                this.results.AddComponentFiles(array);
                this.results.BuildSucceeded();
            }
            catch (Exception exception)
            {
                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.General", new object[] { exception.Message }));
            }
            return this.results;
        }

        private bool BuildPackages(BuildSettings settings, XmlElement configElement, ResourceUpdater resourceUpdater, ArrayList filesCopied, Hashtable eulas)
        {
            bool flag = true;
            foreach (ProductBuilder builder in settings.ProductBuilders)
            {
                if ((this.Validate && !builder.Product.ValidationPassed) && (this.results != null))
                {
                    this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.ProductValidation", new object[] { builder.Name, builder.Product.ValidationResults.FilePath }));
                    foreach (string str in builder.Product.ValidationResults.ValidationErrors)
                    {
                        this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.ValidationError", new object[] { builder.Product.ValidationResults.FilePath, str }));
                    }
                    foreach (string str2 in builder.Product.ValidationResults.ValidationWarnings)
                    {
                        this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.ValidationWarning", new object[] { builder.Product.ValidationResults.FilePath, str2 }));
                    }
                }
                Package package = this.GetPackageForSettings(settings, builder, this.results);
                if (package != null)
                {
                    if ((this.Validate && !package.ValidationPassed) && (this.results != null))
                    {
                        this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.PackageValidation", new object[] { builder.Name, package.ValidationResults.FilePath }));
                        foreach (string str3 in package.ValidationResults.ValidationErrors)
                        {
                            this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.ValidationError", new object[] { package.ValidationResults.FilePath, str3 }));
                        }
                        foreach (string str4 in package.ValidationResults.ValidationWarnings)
                        {
                            this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.ValidationWarning", new object[] { package.ValidationResults.FilePath, str4 }));
                        }
                    }
                    XmlNode node = package.Node;
                    XmlAttribute attribute = node.Attributes["LicenseAgreement"];
                    XmlNodeList list = node.SelectNodes("bootstrapper:PackageFiles/bootstrapper:PackageFile", this.xmlNamespaceManager);
                    XmlNode subNode = node.SelectSingleNode("bootstrapper:InstallChecks", this.xmlNamespaceManager);
                    foreach (XmlNode node3 in list)
                    {
                        XmlAttribute namedItem = (XmlAttribute) node3.Attributes.GetNamedItem("SourcePath");
                        XmlAttribute attribute3 = (XmlAttribute) node3.Attributes.GetNamedItem("TargetPath");
                        XmlAttribute attribute4 = (XmlAttribute) node3.Attributes.GetNamedItem("Name");
                        XmlAttribute attribute5 = (XmlAttribute) node3.Attributes.GetNamedItem("CopyOnBuild");
                        if (((namedItem != null) && (attribute != null)) && (!string.IsNullOrEmpty(attribute.Value) && (namedItem.Value == attribute.Value)))
                        {
                            node.SelectSingleNode("bootstrapper:PackageFiles", this.xmlNamespaceManager).RemoveChild(node3);
                        }
                        else
                        {
                            if (((namedItem != null) && (attribute3 != null)) && !this.AddVerificationInformation(node3, namedItem.Value, attribute4.Value, builder, settings, this.results))
                            {
                                flag = false;
                            }
                            if (((namedItem != null) && (attribute3 != null)) && ((attribute5 == null) || (string.Compare(attribute5.Value, "False", StringComparison.InvariantCulture) != 0)))
                            {
                                XmlNode node5 = null;
                                if ((subNode != null) && (attribute4 != null))
                                {
                                    node5 = this.QueryForSubNode(subNode, "PackageFile", attribute4.Value);
                                }
                                if (node5 != null)
                                {
                                    if (resourceUpdater != null)
                                    {
                                        if (!File.Exists(namedItem.Value))
                                        {
                                            if (this.results != null)
                                            {
                                                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.PackageResourceFileNotFound", new object[] { namedItem.Value, builder.Name }));
                                            }
                                            flag = false;
                                        }
                                        else
                                        {
                                            resourceUpdater.AddFileResource(namedItem.Value, attribute3.Value);
                                        }
                                    }
                                }
                                else if ((settings.ComponentsLocation != ComponentsLocation.HomeSite) || !this.VerifyHomeSiteInformation(node3, builder, settings, this.results))
                                {
                                    if (settings.CopyComponents)
                                    {
                                        string path = System.IO.Path.Combine(settings.OutputPath, attribute3.Value);
                                        try
                                        {
                                            if (!File.Exists(namedItem.Value))
                                            {
                                                if (this.results != null)
                                                {
                                                    this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.PackageFileNotFound", new object[] { attribute3.Value, builder.Name }));
                                                }
                                                flag = false;
                                                goto Label_06D6;
                                            }
                                            this.EnsureFolderExists(System.IO.Path.GetDirectoryName(path));
                                            File.Copy(namedItem.Value, path, true);
                                            this.ClearReadOnlyAttribute(path);
                                        }
                                        catch (UnauthorizedAccessException exception)
                                        {
                                            if (this.results != null)
                                            {
                                                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.CopyPackageError", new object[] { namedItem.Value, builder.Name, exception.Message }));
                                            }
                                            flag = false;
                                            goto Label_06D6;
                                        }
                                        catch (IOException exception2)
                                        {
                                            if (this.results != null)
                                            {
                                                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.CopyPackageError", new object[] { namedItem.Value, builder.Name, exception2.Message }));
                                            }
                                            flag = false;
                                            goto Label_06D6;
                                        }
                                        catch (ArgumentException exception3)
                                        {
                                            if (this.results != null)
                                            {
                                                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.CopyPackageError", new object[] { namedItem.Value, builder.Name, exception3.Message }));
                                            }
                                            flag = false;
                                            goto Label_06D6;
                                        }
                                        catch (NotSupportedException exception4)
                                        {
                                            if (this.results != null)
                                            {
                                                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.CopyPackageError", new object[] { namedItem.Value, builder.Name, exception4.Message }));
                                            }
                                            flag = false;
                                            goto Label_06D6;
                                        }
                                        filesCopied.Add(path);
                                    }
                                    else
                                    {
                                        filesCopied.Add(namedItem.Value);
                                    }
                                    XmlAttribute attribute6 = node3.OwnerDocument.CreateAttribute("Size");
                                    FileInfo info = new FileInfo(namedItem.Value);
                                    attribute6.Value = info.Length.ToString(CultureInfo.InvariantCulture) ?? "";
                                    this.MergeAttribute(node3, attribute6);
                                }
                            }
                        Label_06D6:;
                        }
                    }
                    if (((eulas != null) && (attribute != null)) && !string.IsNullOrEmpty(attribute.Value))
                    {
                        if (File.Exists(attribute.Value))
                        {
                            string fileHash = this.GetFileHash(attribute.Value);
                            if (eulas.ContainsKey(fileHash))
                            {
                                DictionaryEntry entry2 = (DictionaryEntry) eulas[fileHash];
                                attribute.Value = entry2.Key.ToString();
                            }
                            else
                            {
                                string key = string.Format(CultureInfo.InvariantCulture, "EULA{0}", new object[] { eulas.Count });
                                DictionaryEntry entry = new DictionaryEntry(key, attribute.Value);
                                eulas[fileHash] = entry;
                                attribute.Value = key;
                            }
                        }
                        else
                        {
                            if (this.results != null)
                            {
                                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.PackageResourceFileNotFound", new object[] { attribute.Value, builder.Name }));
                            }
                            flag = false;
                            continue;
                        }
                    }
                    if (configElement != null)
                    {
                        configElement.AppendChild(configElement.OwnerDocument.ImportNode(node, true));
                        this.DumpXmlToFile(node, string.Format(CultureInfo.CurrentCulture, "{0}.{1}.xml", new object[] { package.Product.ProductCode, package.Culture }));
                    }
                }
            }
            return flag;
        }

        private bool BuildResources(BuildSettings settings, ResourceUpdater resourceUpdater)
        {
            if (this.cultures.Count == 0)
            {
                if (this.results != null)
                {
                    this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.NoResources", new object[0]));
                }
                return false;
            }
            int codepage = -1;
            XmlNode node = this.GetResourcesNodeForSettings(settings, this.results, ref codepage);
            XmlNode node2 = node.SelectSingleNode("Strings");
            XmlNode input = node.SelectSingleNode("Fonts");
            if (node2 == null)
            {
                if (this.results != null)
                {
                    this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.NoStringsForCulture", new object[] { node.Attributes.GetNamedItem("Culture").Value }));
                }
                return false;
            }
            foreach (XmlNode node4 in node2.SelectNodes("String"))
            {
                XmlAttribute namedItem = (XmlAttribute) node4.Attributes.GetNamedItem("Name");
                if (namedItem != null)
                {
                    resourceUpdater.AddStringResource(0x2b, namedItem.Value.ToUpper(CultureInfo.InvariantCulture), node4.InnerText);
                }
            }
            if (input != null)
            {
                foreach (XmlNode node5 in input.SelectNodes("Font"))
                {
                    this.ConvertChildsNodeToAttributes(node5);
                }
                string data = this.XmlToConfigurationFile(input);
                resourceUpdater.AddStringResource(0x2d, "SETUPRES", data);
                this.DumpXmlToFile(input, "fonts.cfg.xml");
                this.DumpStringToFile(data, "fonts.cfg", false);
                if (codepage != -1)
                {
                    resourceUpdater.AddStringResource(0x2d, "CODEPAGE", codepage.ToString(CultureInfo.InvariantCulture));
                }
            }
            return true;
        }

        private string ByteArrayToString(byte[] byteArray)
        {
            if (byteArray == null)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(byteArray.Length);
            foreach (byte num in byteArray)
            {
                builder.Append(num.ToString("X02", CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        private void ClearReadOnlyAttribute(string strFileName)
        {
            FileAttributes fileAttributes = File.GetAttributes(strFileName);
            if ((fileAttributes & FileAttributes.ReadOnly) != 0)
            {
                fileAttributes &= ~FileAttributes.ReadOnly;
                File.SetAttributes(strFileName, fileAttributes);
            }
        }

        private void CombineElements(XmlElement langElement, XmlElement baseElement, string strNodeName, string strSubNodeKey, XmlElement mergeElement)
        {
            XmlNode newChild = langElement.SelectSingleNode("bootstrapper:" + strNodeName, this.xmlNamespaceManager);
            XmlNode node2 = baseElement.SelectSingleNode("bootstrapper:" + strNodeName, this.xmlNamespaceManager);
            if (node2 == null)
            {
                if (newChild != null)
                {
                    mergeElement.AppendChild(newChild);
                }
            }
            else if (newChild == null)
            {
                mergeElement.AppendChild(node2);
            }
            else
            {
                XmlNode node4;
                XmlNode node = this.document.CreateElement(strNodeName, "http://schemas.microsoft.com/developer/2004/01/bootstrapper");
                for (node4 = node2.FirstChild; node4 != null; node4 = node4.NextSibling)
                {
                    if (node4.NodeType == XmlNodeType.Element)
                    {
                        XmlAttribute namedItem = (XmlAttribute) node4.Attributes.GetNamedItem(strSubNodeKey);
                        if (namedItem != null)
                        {
                            XmlNode node5 = this.QueryForSubNode(newChild, strSubNodeKey, namedItem.Value);
                            if (node5 == null)
                            {
                                node.AppendChild(node.OwnerDocument.ImportNode(node4, true));
                            }
                            else
                            {
                                node.AppendChild(node.OwnerDocument.ImportNode(node5, true));
                                newChild.RemoveChild(node5);
                            }
                        }
                    }
                }
                for (node4 = newChild.FirstChild; node4 != null; node4 = node4.NextSibling)
                {
                    node.AppendChild(node.OwnerDocument.ImportNode(node4, true));
                }
                foreach (XmlAttribute attribute2 in newChild.Attributes)
                {
                    this.AddAttribute(node, attribute2.Name, attribute2.Value);
                }
                foreach (XmlAttribute attribute3 in node2.Attributes)
                {
                    if (node.Attributes.GetNamedItem(attribute3.Name) == null)
                    {
                        this.AddAttribute(node, attribute3.Name, attribute3.Value);
                    }
                }
                mergeElement.AppendChild(node);
            }
        }

        internal bool ContainsCulture(string culture)
        {
            if (!this.fInitialized)
            {
                this.Refresh();
            }
            return this.cultures.Contains(culture);
        }

        private void ConvertChildsNodeToAttributes(XmlNode node)
        {
            XmlNode firstChild = node.FirstChild;
            while (firstChild != null)
            {
                XmlNode oldChild = firstChild;
                firstChild = oldChild.NextSibling;
                if ((oldChild.Attributes.Count == 0) && (oldChild.InnerText.Length > 0))
                {
                    this.AddAttribute(node, oldChild.Name, oldChild.InnerText);
                    node.RemoveChild(oldChild);
                }
            }
        }

        private bool CopySetupToOutputDirectory(BuildSettings settings, string strOutputExe)
        {
            string bootstrapperPath = this.BootstrapperPath;
            string path = System.IO.Path.Combine(bootstrapperPath, "setup.bin");
            if (!File.Exists(path))
            {
                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.MissingSetupBin", new object[] { "setup.bin", bootstrapperPath }));
                return false;
            }
            try
            {
                this.EnsureFolderExists(settings.OutputPath);
                File.Copy(path, strOutputExe, true);
                this.ClearReadOnlyAttribute(strOutputExe);
            }
            catch (IOException exception)
            {
                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.CopyError", new object[] { path, strOutputExe, exception.Message }));
                return false;
            }
            catch (UnauthorizedAccessException exception2)
            {
                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.CopyError", new object[] { path, strOutputExe, exception2.Message }));
                return false;
            }
            catch (ArgumentException exception3)
            {
                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.CopyError", new object[] { path, strOutputExe, exception3.Message }));
                return false;
            }
            catch (NotSupportedException exception4)
            {
                this.results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.CopyError", new object[] { path, strOutputExe, exception4.Message }));
                return false;
            }
            return true;
        }

        private void CorrectPackageFiles(XmlNode node)
        {
            XmlNode node2 = node.SelectSingleNode("bootstrapper:PackageFiles", this.xmlNamespaceManager);
            if (node2 != null)
            {
                foreach (XmlNode node3 in node.SelectNodes("//bootstrapper:*[@PackageFile]", this.xmlNamespaceManager))
                {
                    XmlAttribute namedItem = (XmlAttribute) node3.Attributes.GetNamedItem("PackageFile");
                    string xpath = "bootstrapper:PackageFile[@Name='" + namedItem.Value + "']";
                    XmlNode node4 = node2.SelectSingleNode(xpath, this.xmlNamespaceManager);
                    if (node4 != null)
                    {
                        XmlAttribute attribute2 = (XmlAttribute) node4.Attributes.GetNamedItem("TargetPath");
                        namedItem.Value = attribute2.Value;
                    }
                }
            }
        }

        private XmlElement CreateApplicationElement(XmlElement configElement, BuildSettings settings)
        {
            XmlElement node = null;
            if (!string.IsNullOrEmpty(settings.ApplicationName) || !string.IsNullOrEmpty(settings.ApplicationFile))
            {
                node = configElement.OwnerDocument.CreateElement("Application");
                if (!string.IsNullOrEmpty(settings.ApplicationName))
                {
                    this.AddAttribute(node, "Name", settings.ApplicationName);
                }
                this.AddAttribute(node, "RequiresElevation", settings.ApplicationRequiresElevation ? "true" : "false");
                if (!string.IsNullOrEmpty(settings.ApplicationFile))
                {
                    XmlElement newChild = node.OwnerDocument.CreateElement("Files");
                    XmlElement element3 = newChild.OwnerDocument.CreateElement("File");
                    this.AddAttribute(element3, "Name", settings.ApplicationFile);
                    this.AddAttribute(element3, "UrlName", Uri.EscapeUriString(settings.ApplicationFile));
                    newChild.AppendChild(element3);
                    node.AppendChild(newChild);
                }
            }
            return node;
        }

        private Package CreatePackage(XmlNode node, Product product)
        {
            string culture = this.ReadAttribute(node, "Culture");
            if (culture != null)
            {
                return new Package(product, node, product.GetPackageValidationResults(culture), this.ReadAttribute(node, "Name"), this.ReadAttribute(node, "Culture"));
            }
            return null;
        }

        private Product CreateProduct(XmlNode node)
        {
            bool flag = false;
            string str = this.ReadAttribute(node, "ProductCode");
            Product product = null;
            if (!string.IsNullOrEmpty(str))
            {
                ProductValidationResults validationResults = (ProductValidationResults) this.validationResults[str];
                XmlNode node2 = node.SelectSingleNode("bootstrapper:Package/bootstrapper:PackageFiles", this.xmlNamespaceManager);
                string copyAll = string.Empty;
                if (node2 != null)
                {
                    copyAll = this.ReadAttribute(node2, "CopyAllPackageFiles");
                }
                product = new Product(node, str, validationResults, copyAll);
                foreach (XmlNode node3 in node.SelectNodes("bootstrapper:Package", this.xmlNamespaceManager))
                {
                    Package package = this.CreatePackage(node3, product);
                    if (package != null)
                    {
                        product.AddPackage(package);
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                return product;
            }
            return null;
        }

        private XmlNode CreateProductNode(XmlNode node)
        {
            XmlNode node2 = this.document.CreateElement("Product", "http://schemas.microsoft.com/developer/2004/01/bootstrapper");
            XmlAttribute namedItem = (XmlAttribute) node.Attributes.GetNamedItem("ProductCode");
            this.AddAttribute(node2, "ProductCode", namedItem.Value);
            node.Attributes.Remove(namedItem);
            return node2;
        }

        private void DumpStringToFile(string text, string fileName, bool append)
        {
            if (logging)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(System.IO.Path.Combine(logPath, fileName), append))
                    {
                        writer.Write(text);
                    }
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (NotSupportedException)
                {
                }
            }
        }

        private void DumpXmlToFile(XmlNode node, string fileName)
        {
            if (logging)
            {
                try
                {
                    using (XmlTextWriter writer = new XmlTextWriter(System.IO.Path.Combine(logPath, fileName), Encoding.UTF8))
                    {
                        writer.Formatting = Formatting.Indented;
                        writer.Indentation = 4;
                        writer.WriteNode(new XmlNodeReader(node), true);
                    }
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (NotSupportedException)
                {
                }
                catch (XmlException)
                {
                }
            }
        }

        private void EnsureFolderExists(string strFolderPath)
        {
            if (!Directory.Exists(strFolderPath))
            {
                Directory.CreateDirectory(strFolderPath);
            }
        }

        private void ExploreDirectory(string strSubDirectory, XmlElement rootElement)
        {
            try
            {
                string packagePath = this.PackagePath;
                string str2 = System.IO.Path.Combine(packagePath, strSubDirectory);
                string filePath = System.IO.Path.Combine(str2, "product.xml");
                string schemaPath = System.IO.Path.Combine(this.SchemaPath, "package.xsd");
                ProductValidationResults results = new ProductValidationResults(filePath);
                XmlDocument document = this.LoadAndValidateXmlDocument(filePath, false, schemaPath, "http://schemas.microsoft.com/developer/2004/01/bootstrapper", results);
                if (document != null)
                {
                    bool flag = false;
                    XmlNode node = document.SelectSingleNode("bootstrapper:Product", this.xmlNamespaceManager);
                    if (node != null)
                    {
                        XmlAttribute namedItem = (XmlAttribute) node.Attributes.GetNamedItem("ProductCode");
                        if (namedItem != null)
                        {
                            XmlNode newChild = rootElement.SelectSingleNode("bootstrapper:Product[@ProductCode='" + namedItem.Value + "']", this.xmlNamespaceManager);
                            if (newChild == null)
                            {
                                newChild = this.CreateProductNode(node);
                            }
                            else
                            {
                                results = (ProductValidationResults) this.validationResults[namedItem];
                            }
                            XmlNode packageFilesNode = node.SelectSingleNode("bootstrapper:PackageFiles", this.xmlNamespaceManager);
                            XmlNode targetNodes = node.SelectSingleNode("bootstrapper:InstallChecks", this.xmlNamespaceManager);
                            XmlNode node5 = node.SelectSingleNode("bootstrapper:Commands", this.xmlNamespaceManager);
                            if (packageFilesNode != null)
                            {
                                this.UpdatePackageFileNodes(packageFilesNode, System.IO.Path.Combine(packagePath, strSubDirectory), strSubDirectory);
                                this.ReplacePackageFileAttributes(targetNodes, "PackageFile", packageFilesNode, "PackageFile", "OldName", "Name");
                                this.ReplacePackageFileAttributes(node5, "PackageFile", packageFilesNode, "PackageFile", "OldName", "Name");
                                this.ReplacePackageFileAttributes(node, "LicenseAgreement", packageFilesNode, "PackageFile", "OldName", "SourcePath");
                            }
                            foreach (string str5 in Directory.GetDirectories(str2))
                            {
                                XmlElement baseElement = (XmlElement) this.document.ImportNode(node, true);
                                string path = System.IO.Path.Combine(str5, "package.xml");
                                string str7 = System.IO.Path.Combine(this.SchemaPath, "package.xsd");
                                if (File.Exists(path))
                                {
                                    XmlValidationResults results2 = new XmlValidationResults(path);
                                    XmlDocument document2 = this.LoadAndValidateXmlDocument(path, false, str7, "http://schemas.microsoft.com/developer/2004/01/bootstrapper", results2);
                                    if (document2 != null)
                                    {
                                        XmlNode node6 = document2.SelectSingleNode("bootstrapper:Package", this.xmlNamespaceManager);
                                        if (node6 != null)
                                        {
                                            XmlElement element2 = (XmlElement) this.document.ImportNode(node6, true);
                                            XmlElement targetNode = this.document.CreateElement("Package", "http://schemas.microsoft.com/developer/2004/01/bootstrapper");
                                            XmlNode node7 = element2.SelectSingleNode("bootstrapper:PackageFiles", this.xmlNamespaceManager);
                                            targetNodes = element2.SelectSingleNode("bootstrapper:InstallChecks", this.xmlNamespaceManager);
                                            node5 = element2.SelectSingleNode("bootstrapper:Commands", this.xmlNamespaceManager);
                                            if (node7 != null)
                                            {
                                                int length = packagePath.Length;
                                                if (str5.ToCharArray()[length] == System.IO.Path.DirectorySeparatorChar)
                                                {
                                                    length++;
                                                }
                                                this.UpdatePackageFileNodes(node7, str5, strSubDirectory);
                                                this.ReplacePackageFileAttributes(targetNodes, "PackageFile", node7, "PackageFile", "OldName", "Name");
                                                this.ReplacePackageFileAttributes(node5, "PackageFile", node7, "PackageFile", "OldName", "Name");
                                                this.ReplacePackageFileAttributes(element2, "LicenseAgreement", node7, "PackageFile", "OldName", "SourcePath");
                                            }
                                            if (packageFilesNode != null)
                                            {
                                                this.ReplacePackageFileAttributes(targetNodes, "PackageFile", packageFilesNode, "PackageFile", "OldName", "Name");
                                                this.ReplacePackageFileAttributes(node5, "PackageFile", packageFilesNode, "PackageFile", "OldName", "Name");
                                                this.ReplacePackageFileAttributes(element2, "LicenseAgreement", packageFilesNode, "PackageFile", "OldName", "SourcePath");
                                            }
                                            foreach (XmlAttribute attribute2 in element2.Attributes)
                                            {
                                                targetNode.Attributes.Append((XmlAttribute) targetNode.OwnerDocument.ImportNode(attribute2, false));
                                            }
                                            foreach (XmlAttribute attribute3 in baseElement.Attributes)
                                            {
                                                XmlAttribute attribute = (XmlAttribute) targetNode.OwnerDocument.ImportNode(attribute3, false);
                                                this.MergeAttribute(targetNode, attribute);
                                            }
                                            this.CombineElements(element2, baseElement, "Commands", "PackageFile", targetNode);
                                            this.CombineElements(element2, baseElement, "InstallChecks", "Property", targetNode);
                                            this.CombineElements(element2, baseElement, "PackageFiles", "Name", targetNode);
                                            this.CombineElements(element2, baseElement, "Schedules", "Name", targetNode);
                                            this.CombineElements(element2, baseElement, "Strings", "Name", targetNode);
                                            this.ReplaceStrings(targetNode);
                                            this.CorrectPackageFiles(targetNode);
                                            this.AppendNode(baseElement, "RelatedProducts", targetNode);
                                            XmlAttribute attribute5 = (XmlAttribute) targetNode.Attributes.GetNamedItem("Culture");
                                            if ((attribute5 != null) && !string.IsNullOrEmpty(attribute5.Value))
                                            {
                                                string attributeValue = namedItem.Value + "." + attribute5.Value;
                                                this.AddAttribute(targetNode, "PackageCode", attributeValue);
                                                if ((results != null) && (results2 != null))
                                                {
                                                    results.AddPackageResults(attribute5.Value, results2);
                                                }
                                                newChild.AppendChild(targetNode);
                                                flag = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (flag)
                            {
                                rootElement.AppendChild(newChild);
                                if (!this.validationResults.Contains(namedItem.Value))
                                {
                                    this.validationResults.Add(namedItem.Value, results);
                                }
                            }
                        }
                    }
                }
            }
            catch (XmlException)
            {
            }
            catch (IOException)
            {
            }
            catch (ArgumentException)
            {
            }
        }

        private string GetAssemblyPath()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private void GetCodePage(string culture, ref int codePage)
        {
            try
            {
                CultureInfo info = new CultureInfo(culture);
                codePage = info.TextInfo.ANSICodePage;
            }
            catch (ArgumentException)
            {
            }
        }

        private Stream GetEmbeddedResourceStream(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { typeof(BootstrapperBuilder).Namespace, name }));
        }

        private string GetFileHash(string filePath)
        {
            FileInfo info = new FileInfo(filePath);
            SHA1CryptoServiceProvider provider = new SHA1CryptoServiceProvider();
            using (Stream stream = info.OpenRead())
            {
                return this.ByteArrayToString(provider.ComputeHash(stream));
            }
        }

        private Dictionary<string, Product> GetIncludedProducts(Product product)
        {
            Dictionary<string, Product> includedProducts = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);
            includedProducts.Add(product.ProductCode, product);
            foreach (Product product2 in product.Includes)
            {
                this.AddIncludedProducts(product2, includedProducts);
            }
            includedProducts.Remove(product.ProductCode);
            return includedProducts;
        }

        private static string GetLogPath()
        {
            if (!logging)
            {
                return null;
            }
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\VisualStudio\10.0\VSPLOG");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public string[] GetOutputFolders(string[] productCodes, string culture, string fallbackCulture, ComponentsLocation componentsLocation)
        {
            if (!this.fInitialized)
            {
                this.Refresh();
            }
            Hashtable hashtable = new Hashtable();
            BuildSettings settings = new BuildSettings();
            string strB = Util.AddTrailingChar(this.PackagePath.ToLowerInvariant(), System.IO.Path.DirectorySeparatorChar);
            settings.CopyComponents = false;
            settings.Culture = culture;
            settings.FallbackCulture = fallbackCulture;
            settings.ComponentsLocation = componentsLocation;
            foreach (string str2 in productCodes)
            {
                Product product = this.Products.Product(str2);
                if (product != null)
                {
                    settings.ProductBuilders.Add(product.ProductBuilder);
                }
            }
            ArrayList filesCopied = new ArrayList();
            this.BuildPackages(settings, null, null, filesCopied, null);
            foreach (string str3 in filesCopied)
            {
                string directoryName = System.IO.Path.GetDirectoryName(str3);
                if (directoryName.Substring(0, strB.Length).ToLowerInvariant().CompareTo(strB) == 0)
                {
                    string key = directoryName.Substring(strB.Length).ToLowerInvariant();
                    if (!hashtable.Contains(key))
                    {
                        hashtable.Add(key, key);
                    }
                }
            }
            ArrayList list2 = new ArrayList(hashtable.Values);
            string[] array = new string[list2.Count];
            list2.CopyTo(array, 0);
            return array;
        }

        private Package GetPackageForSettings(BuildSettings settings, ProductBuilder builder, BuildResults results)
        {
            CultureInfo cultureInfoFromString = Util.GetCultureInfoFromString(settings.Culture);
            CultureInfo altCulture = Util.GetCultureInfoFromString(settings.FallbackCulture);
            Package package = null;
            if (builder.Product.Packages.Count == 0)
            {
                if (results != null)
                {
                    results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Error, "GenerateBootstrapper.ProductCultureNotFound", new object[] { builder.Name }));
                }
                return null;
            }
            if (cultureInfoFromString != null)
            {
                package = builder.Product.Packages.Package(cultureInfoFromString.Name);
                if (package != null)
                {
                    return package;
                }
                for (CultureInfo info3 = cultureInfoFromString.Parent; (info3 != null) && (info3 != CultureInfo.InvariantCulture); info3 = info3.Parent)
                {
                    package = this.GetPackageForSettings_Helper(cultureInfoFromString, info3, builder, results, false);
                    if (package != null)
                    {
                        return package;
                    }
                }
            }
            if (altCulture != null)
            {
                package = this.GetPackageForSettings_Helper(cultureInfoFromString, altCulture, builder, results, true);
                if (package != null)
                {
                    return package;
                }
                if (!altCulture.IsNeutralCulture)
                {
                    package = this.GetPackageForSettings_Helper(cultureInfoFromString, altCulture.Parent, builder, results, true);
                    if (package != null)
                    {
                        return package;
                    }
                }
            }
            package = this.GetPackageForSettings_Helper(cultureInfoFromString, Util.DefaultCultureInfo, builder, results, true);
            if (package != null)
            {
                return package;
            }
            if (!Util.DefaultCultureInfo.IsNeutralCulture)
            {
                package = this.GetPackageForSettings_Helper(cultureInfoFromString, Util.DefaultCultureInfo.Parent, builder, results, true);
                if (package != null)
                {
                    return package;
                }
            }
            if ((results != null) && (cultureInfoFromString != null))
            {
                results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.UsingProductCulture", new object[] { cultureInfoFromString.Name, builder.Name, builder.Product.Packages.Item(0).Culture }));
            }
            return builder.Product.Packages.Item(0);
        }

        private Package GetPackageForSettings_Helper(CultureInfo culture, CultureInfo altCulture, ProductBuilder builder, BuildResults results, bool fShowWarning)
        {
            if (altCulture == null)
            {
                return null;
            }
            Package package = builder.Product.Packages.Package(altCulture.Name);
            if (package == null)
            {
                return null;
            }
            if ((fShowWarning && (culture != null)) && (results != null))
            {
                results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.UsingProductCulture", new object[] { culture.Name, builder.Name, altCulture.Name }));
            }
            return package;
        }

        private string GetPublicKeyOfFile(string fileSource)
        {
            if (File.Exists(fileSource))
            {
                try
                {
                    X509Certificate certificate = new X509Certificate(fileSource);
                    return certificate.GetPublicKeyString();
                }
                catch (CryptographicException)
                {
                }
            }
            return null;
        }

        private XmlNode GetResourcesNodeForSettings(BuildSettings settings, BuildResults results, ref int codepage)
        {
            CultureInfo cultureInfoFromString = Util.GetCultureInfoFromString(settings.Culture);
            CultureInfo altCulture = Util.GetCultureInfoFromString(settings.FallbackCulture);
            XmlNode node = null;
            if (cultureInfoFromString != null)
            {
                node = this.GetResourcesNodeForSettings_Helper(cultureInfoFromString, cultureInfoFromString, results, ref codepage, false);
                if (node != null)
                {
                    return node;
                }
                for (CultureInfo info3 = cultureInfoFromString.Parent; (info3 != null) && (info3 != CultureInfo.InvariantCulture); info3 = info3.Parent)
                {
                    node = this.GetResourcesNodeForSettings_Helper(cultureInfoFromString, info3, results, ref codepage, false);
                    if (node != null)
                    {
                        return node;
                    }
                }
            }
            if (altCulture != null)
            {
                node = this.GetResourcesNodeForSettings_Helper(cultureInfoFromString, altCulture, results, ref codepage, true);
                if (node != null)
                {
                    return node;
                }
                if (!altCulture.IsNeutralCulture)
                {
                    node = this.GetResourcesNodeForSettings_Helper(cultureInfoFromString, altCulture.Parent, results, ref codepage, true);
                    if (node != null)
                    {
                        return node;
                    }
                }
            }
            node = this.GetResourcesNodeForSettings_Helper(cultureInfoFromString, Util.DefaultCultureInfo, results, ref codepage, true);
            if (node != null)
            {
                return node;
            }
            if (!Util.DefaultCultureInfo.IsNeutralCulture)
            {
                node = this.GetResourcesNodeForSettings_Helper(cultureInfoFromString, Util.DefaultCultureInfo.Parent, results, ref codepage, true);
                if (node != null)
                {
                    return node;
                }
            }
            IEnumerator enumerator = this.cultures.Keys.GetEnumerator();
            enumerator.MoveNext();
            string current = (string) enumerator.Current;
            if ((cultureInfoFromString != null) && (results != null))
            {
                results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.UsingResourcesCulture", new object[] { cultureInfoFromString.Name, current }));
            }
            this.GetCodePage(current, ref codepage);
            return (XmlNode) this.cultures[current.ToLowerInvariant()];
        }

        private XmlNode GetResourcesNodeForSettings_Helper(CultureInfo culture, CultureInfo altCulture, BuildResults results, ref int codepage, bool fShowWarning)
        {
            if ((altCulture == null) || !this.cultures.Contains(altCulture.Name.ToLowerInvariant()))
            {
                return null;
            }
            if ((fShowWarning && (culture != null)) && (results != null))
            {
                results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.UsingResourcesCulture", new object[] { culture.Name, altCulture.Name }));
            }
            codepage = altCulture.TextInfo.ANSICodePage;
            return (XmlNode) this.cultures[altCulture.Name.ToLowerInvariant()];
        }

        private XmlDocument LoadAndValidateXmlDocument(string filePath, bool validateFilePresent, string schemaPath, string schemaNamespace, XmlValidationResults results)
        {
            XmlDocument document = null;
            if (((filePath != null) && (schemaPath != null)) && (schemaNamespace != null))
            {
                bool flag = true;
                bool flag2 = File.Exists(filePath);
                if (!File.Exists(schemaPath))
                {
                    flag = false;
                }
                if ((flag && !flag2) && validateFilePresent)
                {
                    flag = false;
                }
                if (!flag2)
                {
                    return document;
                }
                XmlReader reader = new XmlTextReader(filePath);
                if (flag)
                {
                    XmlValidatingReader reader2 = new XmlValidatingReader(reader);
                    try
                    {
                        reader2.Schemas.Add(null, schemaPath);
                        reader2.ValidationEventHandler += new ValidationEventHandler(results.SchemaValidationEventHandler);
                        reader = reader2;
                    }
                    catch (XmlException)
                    {
                        flag = false;
                    }
                    catch (XmlSchemaException)
                    {
                        flag = false;
                    }
                }
                try
                {
                    document = new XmlDocument(this.document.NameTable);
                    document.Load(reader);
                }
                catch (XmlException)
                {
                    return null;
                }
                catch (XmlSchemaException)
                {
                    return null;
                }
                finally
                {
                    reader.Close();
                }
                if (document.DocumentElement != null)
                {
                    string.Equals(document.DocumentElement.NamespaceURI, schemaNamespace, StringComparison.Ordinal);
                }
            }
            return document;
        }

        private string MapLCIDToCultureName(int lcid)
        {
            if (lcid == 0)
            {
                return Util.DefaultCultureInfo.Name;
            }
            try
            {
                CultureInfo info = new CultureInfo(lcid);
                return info.Name;
            }
            catch (ArgumentException)
            {
                return Util.DefaultCultureInfo.Name;
            }
        }

        private void Merge(Dictionary<string, Product> output, Dictionary<string, Product> input)
        {
            foreach (Product product in input.Values)
            {
                this.AddProduct(output, product);
            }
        }

        private void MergeAttribute(XmlNode targetNode, XmlAttribute attribute)
        {
            if (((XmlAttribute) targetNode.Attributes.GetNamedItem(attribute.Name)) == null)
            {
                targetNode.Attributes.Append(attribute);
            }
        }

        private void OrderProducts(Hashtable availableProducts, Hashtable buildQueue)
        {
            bool flag = false;
            this.loopDependenciesWarnings = new BuildResults();
            StringBuilder builder = new StringBuilder();
            while (buildQueue.Count > 0)
            {
                List<string> list = new List<string>();
                foreach (Product product in buildQueue.Values)
                {
                    if (product.Dependencies.Count == 0)
                    {
                        this.products.Add((Product) availableProducts[product.ProductCode]);
                        this.RemoveDependency(buildQueue, product);
                        list.Add(product.ProductCode);
                    }
                }
                foreach (string str in list)
                {
                    buildQueue.Remove(str);
                    if (flag)
                    {
                        builder.Append(str);
                        builder.Append(", ");
                    }
                }
                if ((buildQueue.Count > 0) && (list.Count == 0))
                {
                    IDictionaryEnumerator enumerator = buildQueue.GetEnumerator();
                    enumerator.MoveNext();
                    ((Product) enumerator.Value).Dependencies.RemoveAll(m => true);
                    flag = true;
                }
                if ((builder.Length > 0) && ((buildQueue.Count == 0) || (list.Count == 0)))
                {
                    builder.Remove(builder.Length - 2, 2);
                    this.loopDependenciesWarnings.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.CircularDependency", new object[] { builder.ToString() }));
                    builder.Remove(0, builder.Length);
                }
            }
        }

        private XmlNode QueryForSubNode(XmlNode subNode, string strSubNodeKey, string strTargetValue)
        {
            string xpath = string.Format(CultureInfo.InvariantCulture, "{0}:*[@{1}='{2}']", new object[] { "bootstrapper", strSubNodeKey, strTargetValue });
            return subNode.SelectSingleNode(xpath, this.xmlNamespaceManager);
        }

        private string ReadAttribute(XmlNode node, string strAttributeName)
        {
            XmlAttribute namedItem = (XmlAttribute) node.Attributes.GetNamedItem(strAttributeName);
            if (namedItem != null)
            {
                return namedItem.Value;
            }
            return null;
        }

        private void Refresh()
        {
            this.RefreshResources();
            this.RefreshProducts();
            this.fInitialized = true;
            if (logging)
            {
                StringBuilder builder = new StringBuilder();
                foreach (Product product in this.Products)
                {
                    builder.Append(product.ProductCode + Environment.NewLine);
                }
                this.DumpStringToFile(builder.ToString(), "BootstrapperInstallOrder.txt", false);
            }
        }

        private void RefreshProducts()
        {
            this.products.Clear();
            this.validationResults.Clear();
            this.document = new XmlDocument();
            this.xmlNamespaceManager = new XmlNamespaceManager(this.document.NameTable);
            this.xmlNamespaceManager.AddNamespace("bootstrapper", "http://schemas.microsoft.com/developer/2004/01/bootstrapper");
            XmlElement rootElement = this.document.CreateElement("Products", "http://schemas.microsoft.com/developer/2004/01/bootstrapper");
            string packagePath = this.PackagePath;
            if (Directory.Exists(packagePath))
            {
                foreach (string str2 in Directory.GetDirectories(packagePath))
                {
                    int length = packagePath.Length;
                    if (str2.ToCharArray()[length] == System.IO.Path.DirectorySeparatorChar)
                    {
                        length++;
                    }
                    this.ExploreDirectory(str2.Substring(length), rootElement);
                }
            }
            this.document.AppendChild(rootElement);
            Hashtable availableProducts = new Hashtable();
            Hashtable hashtable2 = new Hashtable();
            foreach (XmlNode node in rootElement.SelectNodes("bootstrapper:Product", this.xmlNamespaceManager))
            {
                Product product = this.CreateProduct(node);
                if (product != null)
                {
                    availableProducts.Add(product.ProductCode, product);
                    hashtable2.Add(product.ProductCode, this.CreateProduct(node));
                }
            }
            foreach (Product product2 in availableProducts.Values)
            {
                this.AddDependencies(product2, availableProducts);
                this.AddIncludes(product2, availableProducts);
            }
            foreach (Product product3 in hashtable2.Values)
            {
                this.AddDependencies(product3, hashtable2);
            }
            this.OrderProducts(availableProducts, hashtable2);
        }

        private void RefreshResources()
        {
            string path = System.IO.Path.Combine(this.BootstrapperPath, "");
            this.cultures.Clear();
            if (Directory.Exists(path))
            {
                foreach (string str2 in Directory.GetDirectories(path))
                {
                    string str4 = System.IO.Path.Combine(System.IO.Path.Combine(path, str2), "setup.xml");
                    if (File.Exists(str4))
                    {
                        XmlDocument document = new XmlDocument();
                        try
                        {
                            document.Load(str4);
                        }
                        catch (XmlException)
                        {
                            continue;
                        }
                        XmlNode node = document.SelectSingleNode("Resources");
                        if (node != null)
                        {
                            XmlAttribute namedItem = (XmlAttribute) node.Attributes.GetNamedItem("Culture");
                            if (namedItem != null)
                            {
                                XmlNode node2 = node.SelectSingleNode("Strings");
                                if (node2 != null)
                                {
                                    XmlNode node3 = node2.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "String[@Name='{0}']", new object[] { namedItem.Value }));
                                    if (node3 != null)
                                    {
                                        string innerText = node3.InnerText;
                                        XmlNode node4 = node.OwnerDocument.ImportNode(node, true);
                                        node4.Attributes.RemoveNamedItem("Culture");
                                        XmlAttribute attribute2 = (XmlAttribute) node.OwnerDocument.ImportNode(namedItem, false);
                                        attribute2.Value = node3.InnerText;
                                        node4.Attributes.Append(attribute2);
                                        if (!this.cultures.Contains(innerText.ToLowerInvariant()))
                                        {
                                            this.cultures.Add(innerText.ToLowerInvariant(), node4);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RemoveDependency(Hashtable availableProducts, Product product)
        {
            Predicate<Product> match = null;
            foreach (Product product2 in availableProducts.Values)
            {
                foreach (List<Product> list in product2.Dependencies)
                {
                    if (match == null)
                    {
                        match = m => m == product;
                    }
                    list.RemoveAll(match);
                }
                product2.Dependencies.RemoveAll(m => m.Count == 0);
            }
        }

        private void ReplaceAttribute(XmlNode targetNode, string attributeName, string attributeValue)
        {
            XmlAttribute node = targetNode.OwnerDocument.CreateAttribute(attributeName);
            node.Value = attributeValue;
            targetNode.Attributes.SetNamedItem(node);
        }

        private void ReplaceAttributes(XmlNode targetNode, string attributeName, string oldValue, string newValue)
        {
            if (targetNode != null)
            {
                foreach (XmlNode node in targetNode.SelectNodes("bootstrapper" + string.Format(CultureInfo.InvariantCulture, ":*[@{0}='{1}']", new object[] { attributeName, oldValue }), this.xmlNamespaceManager))
                {
                    this.ReplaceAttribute(node, attributeName, newValue);
                }
                XmlAttribute attribute = targetNode.Attributes[attributeName];
                if ((attribute != null) && (attribute.Value == oldValue))
                {
                    attribute.Value = newValue;
                }
            }
        }

        private void ReplaceAttributeString(XmlNode node, string attributeName, XmlNode stringsNode)
        {
            string format = "bootstrapper:String[@Name='{0}']";
            XmlAttribute namedItem = (XmlAttribute) node.Attributes.GetNamedItem(attributeName);
            if (namedItem != null)
            {
                XmlNode node2 = stringsNode.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, format, new object[] { namedItem.Value }), this.xmlNamespaceManager);
                if (node2 != null)
                {
                    namedItem.Value = node2.InnerText;
                }
            }
        }

        private void ReplacePackageFileAttributes(XmlNode targetNodes, string targetAttribute, XmlNode sourceNodes, string sourceSubNodeName, string sourceOldName, string sourceNewName)
        {
            foreach (XmlNode node in sourceNodes.SelectNodes("bootstrapper:" + sourceSubNodeName, this.xmlNamespaceManager))
            {
                XmlAttribute namedItem = (XmlAttribute) node.Attributes.GetNamedItem(sourceOldName);
                XmlAttribute attribute2 = (XmlAttribute) node.Attributes.GetNamedItem(sourceNewName);
                if ((namedItem != null) && (attribute2 != null))
                {
                    this.ReplaceAttributes(targetNodes, targetAttribute, namedItem.Value, attribute2.Value);
                }
            }
        }

        private void ReplaceStrings(XmlNode node)
        {
            XmlNode stringsNode = node.SelectSingleNode("bootstrapper:Strings", this.xmlNamespaceManager);
            if (stringsNode != null)
            {
                string format = "bootstrapper:String[@Name='{0}']";
                this.ReplaceAttributeString(node, "Name", stringsNode);
                this.ReplaceAttributeString(node, "Culture", stringsNode);
                XmlNode node4 = node.SelectSingleNode("bootstrapper:PackageFiles", this.xmlNamespaceManager);
                if (node4 != null)
                {
                    foreach (XmlNode node5 in node4.SelectNodes("bootstrapper:PackageFile", this.xmlNamespaceManager))
                    {
                        this.ReplaceAttributeString(node5, "HomeSite", stringsNode);
                    }
                }
                foreach (XmlNode node6 in node.SelectNodes("//bootstrapper:*[@String]", this.xmlNamespaceManager))
                {
                    XmlAttribute namedItem = (XmlAttribute) node6.Attributes.GetNamedItem("String");
                    XmlNode node3 = stringsNode.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, format, new object[] { namedItem.Value }), this.xmlNamespaceManager);
                    if (node3 != null)
                    {
                        this.AddAttribute(node6, "Text", node3.InnerText);
                    }
                    node6.Attributes.Remove(namedItem);
                }
                node.RemoveChild(stringsNode);
            }
        }

        private XmlNodeList SelectEitherProducts(Product p)
        {
            return p.Node.SelectNodes(string.Format(CultureInfo.InvariantCulture, "{0}:Package/{1}:RelatedProducts/{2}:EitherProducts", new object[] { "bootstrapper", "bootstrapper", "bootstrapper" }), this.xmlNamespaceManager);
        }

        private string[] SelectRelatedProducts(Product p, string nodeName)
        {
            ArrayList list = new ArrayList();
            XmlNodeList list2 = p.Node.SelectNodes(string.Format(CultureInfo.InvariantCulture, "{0}:Package/{1}:RelatedProducts/{2}:{3}", new object[] { "bootstrapper", "bootstrapper", "bootstrapper", nodeName }), this.xmlNamespaceManager);
            if (list2 != null)
            {
                foreach (XmlNode node in list2)
                {
                    XmlAttribute namedItem = (XmlAttribute) node.Attributes.GetNamedItem("Code");
                    if (namedItem != null)
                    {
                        list.Add(namedItem.Value);
                    }
                }
            }
            string[] array = new string[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        private void UpdatePackageFileNodes(XmlNode packageFilesNode, string strSourcePath, string strTargetPath)
        {
            foreach (XmlNode node in packageFilesNode.SelectNodes("bootstrapper:PackageFile", this.xmlNamespaceManager))
            {
                XmlAttribute namedItem = (XmlAttribute) node.Attributes.GetNamedItem("Name");
                if (namedItem != null)
                {
                    string str = namedItem.Value;
                    XmlAttribute attribute = packageFilesNode.OwnerDocument.CreateAttribute("SourcePath");
                    string str2 = System.IO.Path.Combine(strSourcePath, str);
                    attribute.Value = str2;
                    XmlAttribute attribute3 = packageFilesNode.OwnerDocument.CreateAttribute("TargetPath");
                    attribute3.Value = System.IO.Path.Combine(strTargetPath, str);
                    string str3 = namedItem.Value;
                    string attributeValue = System.IO.Path.Combine(strTargetPath, str);
                    XmlAttribute attribute4 = packageFilesNode.OwnerDocument.CreateAttribute("OldName");
                    attribute4.Value = str3;
                    this.ReplaceAttribute(node, "Name", attributeValue);
                    this.MergeAttribute(node, attribute);
                    this.MergeAttribute(node, attribute3);
                    this.MergeAttribute(node, attribute4);
                }
            }
        }

        private bool VerifyHomeSiteInformation(XmlNode packageFileNode, ProductBuilder builder, BuildSettings settings, BuildResults results)
        {
            if (settings.ComponentsLocation != ComponentsLocation.HomeSite)
            {
                return true;
            }
            if ((packageFileNode.Attributes["HomeSite"] != null) || (builder.Product.CopyAllPackageFiles == CopyAllFilesType.CopyAllFilesIfNotHomeSite))
            {
                return true;
            }
            if (results != null)
            {
                results.AddMessage(BuildMessage.CreateMessage(BuildMessageSeverity.Warning, "GenerateBootstrapper.PackageHomeSiteMissing", new object[] { builder.Name }));
            }
            return false;
        }

        private string XmlToConfigurationFile(XmlNode input)
        {
            string str2;
            using (XmlNodeReader reader = new XmlNodeReader(input))
            {
                XPathDocument stylesheet = new XPathDocument(this.GetEmbeddedResourceStream("xmltoconfig.xsl"));
                Evidence evidence = XmlSecureResolver.CreateEvidenceForUrl(this.GetAssemblyPath());
                XslTransform transform = new XslTransform();
                transform.Load(stylesheet, null, evidence);
                XPathDocument document2 = new XPathDocument(reader);
                using (MemoryStream stream2 = new MemoryStream())
                {
                    using (StreamWriter writer = new StreamWriter(stream2))
                    {
                        transform.Transform((IXPathNavigable) document2, null, (TextWriter) writer, null);
                        writer.Flush();
                        stream2.Position = 0L;
                        using (StreamReader reader2 = new StreamReader(stream2))
                        {
                            str2 = reader2.ReadToEnd().Replace("%NEWLINE%", Environment.NewLine);
                        }
                    }
                }
            }
            return str2;
        }

        private string BootstrapperPath
        {
            get
            {
                return System.IO.Path.Combine(this.Path, "Engine");
            }
        }

        internal string[] Cultures
        {
            get
            {
                if (!this.fInitialized)
                {
                    this.Refresh();
                }
                ArrayList list = new ArrayList(this.cultures.Values);
                list.Sort();
                string[] array = new string[list.Count];
                list.CopyTo(array, 0);
                return array;
            }
        }

        private string PackagePath
        {
            get
            {
                return System.IO.Path.Combine(this.Path, "Packages");
            }
        }

        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (!this.fInitialized || (string.Compare(this.path, value, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    this.path = value;
                    this.Refresh();
                }
            }
        }

        public ProductCollection Products
        {
            get
            {
                if (!this.fInitialized)
                {
                    this.Refresh();
                }
                return this.products;
            }
        }

        private string SchemaPath
        {
            get
            {
                return System.IO.Path.Combine(this.Path, "Schemas");
            }
        }

        internal bool Validate
        {
            get
            {
                return this.fValidate;
            }
            set
            {
                this.fValidate = value;
            }
        }
    }
}

