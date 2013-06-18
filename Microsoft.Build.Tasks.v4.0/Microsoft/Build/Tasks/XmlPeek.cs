namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    public class XmlPeek : TaskExtension
    {
        private string namespaces;
        private string query;
        private ITaskItem[] result;
        private string xmlContent;
        private ITaskItem xmlInputPath;

        public override bool Execute()
        {
            XmlInput input;
            XPathDocument document;
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.query, "Query");
            try
            {
                input = new XmlInput(this.xmlInputPath, this.xmlContent);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("XmlPeek.ArgumentError", new object[] { exception.Message });
                return false;
            }
            try
            {
                using (XmlReader reader = input.CreateReader())
                {
                    document = new XPathDocument(reader);
                    reader.Close();
                }
            }
            catch (Exception exception2)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception2))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("XmlPeekPoke.InputFileError", new object[] { this.xmlInputPath.ItemSpec, exception2.Message });
                return false;
            }
            finally
            {
                input.CloseReader();
            }
            XPathNavigator navigator = document.CreateNavigator();
            XPathExpression expr = null;
            try
            {
                expr = navigator.Compile(this.query);
            }
            catch (Exception exception3)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception3))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("XmlPeekPoke.XPathError", new object[] { this.query, exception3.Message });
                return false;
            }
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(navigator.NameTable);
            try
            {
                this.LoadNamespaces(ref namespaceManager, this.namespaces);
            }
            catch (Exception exception4)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception4))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("XmlPeek.NamespacesError", new object[] { exception4.Message });
                return false;
            }
            try
            {
                expr.SetContext(namespaceManager);
            }
            catch (XPathException exception5)
            {
                base.Log.LogErrorWithCodeFromResources("XmlPeek.XPathContextError", new object[] { exception5.Message });
                return false;
            }
            XPathNodeIterator iterator = navigator.Select(expr);
            List<string> list = new List<string>();
            while (iterator.MoveNext())
            {
                if ((iterator.Current.NodeType == XPathNodeType.Attribute) || (iterator.Current.NodeType == XPathNodeType.Text))
                {
                    list.Add(iterator.Current.Value);
                }
                else
                {
                    list.Add(iterator.Current.OuterXml);
                }
            }
            this.result = new ITaskItem[list.Count];
            int num = 0;
            foreach (string str in list)
            {
                this.result[num++] = new TaskItem(str);
                base.Log.LogMessageFromResources("XmlPeek.Found", new object[] { str });
            }
            if (this.result.Length == 0)
            {
                base.Log.LogMessageFromResources("XmlPeek.NotFound", new object[0]);
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
                throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XmlPeek.NamespacesParameterNotWellFormed", new object[0]), exception);
            }
            XmlNodeList list = document.SelectNodes("/Namespaces/*[local-name() = 'Namespace']");
            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list[i];
                if (node.Attributes["Prefix"] == null)
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XmlPeek.NamespacesParameterNoAttribute", new object[] { "Name" }));
                }
                if (node.Attributes["Uri"] == null)
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XmlPeek.NamespacesParameterNoAttribute", new object[] { "Uri" }));
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

        [Output]
        public ITaskItem[] Result
        {
            get
            {
                return this.result;
            }
        }

        public string XmlContent
        {
            get
            {
                return this.xmlContent;
            }
            set
            {
                this.xmlContent = value;
            }
        }

        public ITaskItem XmlInputPath
        {
            get
            {
                return this.xmlInputPath;
            }
            set
            {
                this.xmlInputPath = value;
            }
        }

        internal class XmlInput
        {
            private string data;
            private FileStream fs;
            private XmlModes xmlMode;

            public XmlInput(ITaskItem xmlInputPath, string xmlContent)
            {
                if ((xmlInputPath != null) && (xmlContent != null))
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XmlPeek.XmlInput.TooMany", new object[0]));
                }
                if ((xmlInputPath == null) && (xmlContent == null))
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XmlPeek.XmlInput.TooFew", new object[0]));
                }
                if (xmlInputPath != null)
                {
                    this.xmlMode = XmlModes.XmlFile;
                    this.data = xmlInputPath.ItemSpec;
                }
                else
                {
                    this.xmlMode = XmlModes.Xml;
                    this.data = xmlContent;
                }
            }

            public void CloseReader()
            {
                if (this.fs != null)
                {
                    this.fs.Close();
                    this.fs = null;
                }
            }

            public XmlReader CreateReader()
            {
                if (this.xmlMode == XmlModes.XmlFile)
                {
                    this.fs = new FileStream(this.data, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    return XmlReader.Create(this.fs);
                }
                return XmlReader.Create(new StringReader(this.data));
            }

            public XmlModes XmlMode
            {
                get
                {
                    return this.xmlMode;
                }
            }

            public enum XmlModes
            {
                XmlFile,
                Xml
            }
        }
    }
}

