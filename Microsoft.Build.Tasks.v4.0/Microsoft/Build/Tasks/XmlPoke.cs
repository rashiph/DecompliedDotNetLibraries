namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    public class XmlPoke : TaskExtension
    {
        private string namespaces;
        private string query;
        private ITaskItem value;
        private ITaskItem xmlInputPath;

        public override bool Execute()
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.query, "Query");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.value, "Value");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.xmlInputPath, "XmlInputPath");
            XmlDocument document = new XmlDocument();
            try
            {
                using (FileStream stream = new FileStream(this.xmlInputPath.ItemSpec, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        document.Load(reader);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("XmlPeekPoke.InputFileError", new object[] { this.xmlInputPath.ItemSpec, exception.Message });
                return false;
            }
            XPathNavigator navigator = document.CreateNavigator();
            XPathExpression expr = null;
            try
            {
                expr = navigator.Compile(this.query);
            }
            catch (Exception exception2)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception2))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("XmlPeekPoke.XPathError", new object[] { this.query, exception2.Message });
                return false;
            }
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(navigator.NameTable);
            try
            {
                this.LoadNamespaces(ref namespaceManager, this.namespaces);
            }
            catch (Exception exception3)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception3))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("XmlPoke.NamespacesError", new object[] { exception3.Message });
                return false;
            }
            try
            {
                expr.SetContext(namespaceManager);
            }
            catch (XPathException exception4)
            {
                base.Log.LogErrorWithCodeFromResources("XmlPoke.XPathContextError", new object[] { exception4.Message });
                return false;
            }
            XPathNodeIterator iterator = navigator.Select(expr);
            while (iterator.MoveNext())
            {
                try
                {
                    iterator.Current.InnerXml = this.value.ItemSpec;
                    base.Log.LogMessageFromResources(MessageImportance.Low, "XmlPoke.Replaced", new object[] { iterator.Current.Name, this.value.ItemSpec });
                    continue;
                }
                catch (Exception exception5)
                {
                    if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception5))
                    {
                        throw;
                    }
                    base.Log.LogErrorWithCodeFromResources("XmlPoke.PokeError", new object[] { this.value.ItemSpec, exception5.Message });
                    return false;
                }
            }
            base.Log.LogMessageFromResources(MessageImportance.Normal, "XmlPoke.Count", new object[] { iterator.Count });
            if (iterator.Count > 0)
            {
                document.Save(this.xmlInputPath.ItemSpec);
            }
            return true;
        }

        private void LoadNamespaces(ref XmlNamespaceManager namespaceManager, string namepaces)
        {
            XmlDocument document = new XmlDocument();
            try
            {
                document.LoadXml("<Namespaces>" + namepaces + "</Namespaces>");
            }
            catch (XmlException exception)
            {
                throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XmlPoke.NamespacesParameterNotWellFormed", new object[0]), exception);
            }
            XmlNodeList list = document.SelectNodes("/Namespaces/*[local-name() = 'Namespace']");
            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list[i];
                if (node.Attributes["Prefix"] == null)
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XmlPoke.NamespacesParameterNoAttribute", new object[] { "Name" }));
                }
                if (node.Attributes["Uri"] == null)
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XmlPoke.NamespacesParameterNoAttribute", new object[] { "Uri" }));
                }
                namespaceManager.AddNamespace(node.Attributes["Prefix"].Value, node.Attributes["Uri"].Value);
            }
        }

        public string Namespaces
        {
            get
            {
                return this.namespaces;
            }
            set
            {
                this.namespaces = value;
            }
        }

        public string Query
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.query, "Query");
                return this.query;
            }
            set
            {
                this.query = value;
            }
        }

        [Required]
        public ITaskItem Value
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.value, "Value");
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        public ITaskItem XmlInputPath
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.xmlInputPath, "XmlInputPath");
                return this.xmlInputPath;
            }
            set
            {
                this.xmlInputPath = value;
            }
        }
    }
}

