namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    public class XslTransformation : TaskExtension
    {
        private ITaskItem[] outputPaths;
        private string parameters;
        private ITaskItem[] xmlInputPaths;
        private string xmlString;
        private ITaskItem xsltCompiledDll;
        private ITaskItem xsltFile;
        private string xsltString;

        public override bool Execute()
        {
            XmlInput input;
            XsltInput input2;
            XsltArgumentList list;
            XslCompiledTransform transform;
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.outputPaths, "OutputPath");
            try
            {
                input = new XmlInput(this.xmlInputPaths, this.xmlString);
                input2 = new XsltInput(this.xsltFile, this.xsltString, this.xsltCompiledDll);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("XslTransform.ArgumentError", new object[] { exception.Message });
                return false;
            }
            if ((this.xmlInputPaths != null) && (this.xmlInputPaths.Length != this.outputPaths.Length))
            {
                base.Log.LogErrorWithCodeFromResources("General.TwoVectorsMustHaveSameLength", new object[] { this.outputPaths.Length, this.xmlInputPaths.Length, "XmlContent", "XmlInputPaths" });
                return false;
            }
            if ((this.xmlString != null) && (this.outputPaths.Length != 1))
            {
                base.Log.LogErrorWithCodeFromResources("General.TwoVectorsMustHaveSameLength", new object[] { this.outputPaths.Length, 1, "XmlContent", "OutputPaths" });
                return false;
            }
            try
            {
                list = ProcessXsltArguments(this.parameters);
            }
            catch (Exception exception2)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception2))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("XslTransform.XsltArgumentsError", new object[] { exception2.Message });
                return false;
            }
            try
            {
                transform = input2.LoadXslt();
            }
            catch (Exception exception3)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception3))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("XslTransform.XsltLoadError", new object[] { exception3.Message });
                return false;
            }
            try
            {
                for (int i = 0; i < input.Count; i++)
                {
                    using (XmlWriter writer = XmlWriter.Create(this.outputPaths[i].ItemSpec, transform.OutputSettings))
                    {
                        using (XmlReader reader = input.CreateReader(i))
                        {
                            transform.Transform(reader, list, writer);
                        }
                        writer.Close();
                    }
                }
            }
            catch (Exception exception4)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception4))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("XslTransform.TransformError", new object[] { exception4.Message });
                return false;
            }
            if (input.XmlMode == XmlInput.XmlModes.XmlFile)
            {
                for (int j = 0; j < this.xmlInputPaths.Length; j++)
                {
                    this.xmlInputPaths[j].CopyMetadataTo(this.outputPaths[j]);
                }
            }
            return true;
        }

        private static XsltArgumentList ProcessXsltArguments(string xsltParametersXml)
        {
            XsltArgumentList list = new XsltArgumentList();
            if (xsltParametersXml != null)
            {
                XmlDocument document = new XmlDocument();
                try
                {
                    document.LoadXml("<XsltParameters>" + xsltParametersXml + "</XsltParameters>");
                }
                catch (XmlException exception)
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XslTransform.XsltParameterNotWellFormed", new object[0]), exception);
                }
                XmlNodeList list2 = document.SelectNodes("/XsltParameters/*[local-name() = 'Parameter']");
                for (int i = 0; i < list2.Count; i++)
                {
                    XmlNode node = list2[i];
                    if (node.Attributes["Name"] == null)
                    {
                        throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XslTransform.XsltParameterNoAttribute", new object[] { "Name" }));
                    }
                    if (node.Attributes["Value"] == null)
                    {
                        throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XslTransform.XsltParameterNoAttribute", new object[] { "Value" }));
                    }
                    string namespaceUri = string.Empty;
                    if (node.Attributes["Namespace"] != null)
                    {
                        namespaceUri = node.Attributes["Namespace"].Value;
                    }
                    list.AddParam(node.Attributes["Name"].Value, namespaceUri, node.Attributes["Value"].Value);
                }
            }
            return list;
        }

        [Required]
        public ITaskItem[] OutputPaths
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.outputPaths, "OutputPath");
                return this.outputPaths;
            }
            set
            {
                this.outputPaths = value;
            }
        }

        public string Parameters
        {
            get
            {
                return this.parameters;
            }
            set
            {
                this.parameters = value;
            }
        }

        public string XmlContent
        {
            get
            {
                return this.xmlString;
            }
            set
            {
                this.xmlString = value;
            }
        }

        public ITaskItem[] XmlInputPaths
        {
            get
            {
                return this.xmlInputPaths;
            }
            set
            {
                this.xmlInputPaths = value;
            }
        }

        public ITaskItem XslCompiledDllPath
        {
            get
            {
                return this.xsltCompiledDll;
            }
            set
            {
                this.xsltCompiledDll = value;
            }
        }

        public string XslContent
        {
            get
            {
                return this.xsltString;
            }
            set
            {
                this.xsltString = value;
            }
        }

        public ITaskItem XslInputPath
        {
            get
            {
                return this.xsltFile;
            }
            set
            {
                this.xsltFile = value;
            }
        }

        internal class XmlInput
        {
            private string[] data;
            private XmlModes xmlMode;

            public XmlInput(ITaskItem[] xmlFile, string xml)
            {
                if ((xmlFile != null) && (xml != null))
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XslTransform.XmlInput.TooMany", new object[0]));
                }
                if ((xmlFile == null) && (xml == null))
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XslTransform.XmlInput.TooFew", new object[0]));
                }
                if (xmlFile != null)
                {
                    this.xmlMode = XmlModes.XmlFile;
                    this.data = new string[xmlFile.Length];
                    for (int i = 0; i < xmlFile.Length; i++)
                    {
                        this.data[i] = xmlFile[i].ItemSpec;
                    }
                }
                else
                {
                    this.xmlMode = XmlModes.Xml;
                    this.data = new string[] { xml };
                }
            }

            public XmlReader CreateReader(int itemPos)
            {
                if (this.xmlMode == XmlModes.XmlFile)
                {
                    return XmlReader.Create(this.data[0]);
                }
                return XmlReader.Create(new StringReader(this.data[itemPos]));
            }

            public int Count
            {
                get
                {
                    return this.data.Length;
                }
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

        internal class XsltInput
        {
            private string data;
            private XslModes xslMode;

            public XsltInput(ITaskItem xsltFile, string xslt, ITaskItem xsltCompiledDll)
            {
                if ((((xsltFile != null) && (xslt != null)) || ((xsltFile != null) && (xsltCompiledDll != null))) || ((xslt != null) && (xsltCompiledDll != null)))
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XslTransform.XsltInput.TooMany", new object[0]));
                }
                if (((xsltFile == null) && (xslt == null)) && (xsltCompiledDll == null))
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XslTransform.XsltInput.TooFew", new object[0]));
                }
                if (xsltFile != null)
                {
                    this.xslMode = XslModes.XsltFile;
                    this.data = xsltFile.ItemSpec;
                }
                else if (xslt != null)
                {
                    this.xslMode = XslModes.Xslt;
                    this.data = xslt;
                }
                else
                {
                    this.xslMode = XslModes.XsltCompiledDll;
                    this.data = xsltCompiledDll.ItemSpec;
                }
            }

            private static Type FindType(string assemblyPath, string typeName)
            {
                AssemblyName assemblyRef = new AssemblyName {
                    CodeBase = assemblyPath
                };
                Assembly assembly = Assembly.Load(assemblyRef);
                if (typeName != null)
                {
                    return assembly.GetType(typeName);
                }
                List<Type> list = new List<Type>();
                foreach (Type type in assembly.GetTypes())
                {
                    if (!type.Name.StartsWith("$", StringComparison.Ordinal))
                    {
                        list.Add(type);
                    }
                }
                if (list.Count != 1)
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("XslTransform.MustSpecifyType", new object[] { assemblyPath }));
                }
                return list[0];
            }

            public XslCompiledTransform LoadXslt()
            {
                XslCompiledTransform transform = new XslCompiledTransform();
                switch (this.xslMode)
                {
                    case XslModes.XsltFile:
                        transform.Load(new XPathDocument(this.data), XsltSettings.TrustedXslt, new XmlUrlResolver());
                        return transform;

                    case XslModes.Xslt:
                        transform.Load(XmlReader.Create(new StringReader(this.data)));
                        return transform;

                    case XslModes.XsltCompiledDll:
                    {
                        string[] strArray = this.data.Split(new char[] { ';' });
                        string assemblyPath = strArray[0];
                        string typeName = (strArray.Length == 2) ? strArray[1] : null;
                        Type compiledStylesheet = FindType(assemblyPath, typeName);
                        transform.Load(compiledStylesheet);
                        return transform;
                    }
                }
                Microsoft.Build.Shared.ErrorUtilities.ThrowInternalErrorUnreachable();
                return transform;
            }

            public enum XslModes
            {
                XsltFile,
                Xslt,
                XsltCompiledDll
            }
        }
    }
}

