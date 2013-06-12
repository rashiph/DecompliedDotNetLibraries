namespace System.Xml.Xsl
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl.Qil;
    using System.Xml.Xsl.Runtime;
    using System.Xml.Xsl.Xslt;

    public sealed class XslCompiledTransform
    {
        private XmlILCommand command;
        private CompilerResults compilerResults;
        private bool enableDebug;
        private static ConstructorInfo GeneratedCodeCtor;
        private static readonly PermissionSet MemberAccessPermissionSet = new PermissionSet(PermissionState.None);
        private XmlWriterSettings outputSettings;
        private QilExpression qil;
        private static readonly XmlReaderSettings ReaderSettings = null;
        private const string Version = "4.0.0.0";

        static XslCompiledTransform()
        {
            MemberAccessPermissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
        }

        public XslCompiledTransform()
        {
        }

        public XslCompiledTransform(bool enableDebug)
        {
            this.enableDebug = enableDebug;
        }

        private static void CheckArguments(object input, object results)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (results == null)
            {
                throw new ArgumentNullException("results");
            }
        }

        private static void CheckArguments(string inputUri, object results)
        {
            if (inputUri == null)
            {
                throw new ArgumentNullException("inputUri");
            }
            if (results == null)
            {
                throw new ArgumentNullException("results");
            }
        }

        private void CheckCommand()
        {
            if (this.command == null)
            {
                throw new InvalidOperationException(Res.GetString("Xslt_NoStylesheetLoaded"));
            }
        }

        private void CompileQilToMsil(XsltSettings settings)
        {
            this.command = new XmlILGenerator().Generate(this.qil, null);
            this.outputSettings = this.command.StaticData.DefaultWriterSettings;
            this.qil = null;
        }

        public static CompilerErrorCollection CompileToType(XmlReader stylesheet, XsltSettings settings, XmlResolver stylesheetResolver, bool debug, TypeBuilder typeBuilder, string scriptAssemblyPath)
        {
            QilExpression expression;
            if (stylesheet == null)
            {
                throw new ArgumentNullException("stylesheet");
            }
            if (typeBuilder == null)
            {
                throw new ArgumentNullException("typeBuilder");
            }
            if (settings == null)
            {
                settings = XsltSettings.Default;
            }
            if (settings.EnableScript && (scriptAssemblyPath == null))
            {
                throw new ArgumentNullException("scriptAssemblyPath");
            }
            if (scriptAssemblyPath != null)
            {
                scriptAssemblyPath = Path.GetFullPath(scriptAssemblyPath);
            }
            CompilerErrorCollection errors = new Compiler(settings, debug, scriptAssemblyPath).Compile(stylesheet, stylesheetResolver, out expression).Errors;
            if (!errors.HasErrors)
            {
                if (GeneratedCodeCtor == null)
                {
                    GeneratedCodeCtor = typeof(GeneratedCodeAttribute).GetConstructor(new Type[] { typeof(string), typeof(string) });
                }
                typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(GeneratedCodeCtor, new object[] { typeof(XslCompiledTransform).FullName, "4.0.0.0" }));
                new XmlILGenerator().Generate(expression, typeBuilder);
            }
            return errors;
        }

        private void CompileXsltToQil(object stylesheet, XsltSettings settings, XmlResolver stylesheetResolver)
        {
            this.compilerResults = new Compiler(settings, this.enableDebug, null).Compile(stylesheet, stylesheetResolver, out this.qil);
        }

        private CompilerError GetFirstError()
        {
            foreach (CompilerError error in this.compilerResults.Errors)
            {
                if (!error.IsWarning)
                {
                    return error;
                }
            }
            return null;
        }

        public void Load(string stylesheetUri)
        {
            this.Reset();
            if (stylesheetUri == null)
            {
                throw new ArgumentNullException("stylesheetUri");
            }
            this.LoadInternal(stylesheetUri, XsltSettings.Default, new XmlUrlResolver());
        }

        public void Load(Type compiledStylesheet)
        {
            this.Reset();
            if (compiledStylesheet == null)
            {
                throw new ArgumentNullException("compiledStylesheet");
            }
            object[] customAttributes = compiledStylesheet.GetCustomAttributes(typeof(GeneratedCodeAttribute), false);
            GeneratedCodeAttribute attribute = (customAttributes.Length > 0) ? ((GeneratedCodeAttribute) customAttributes[0]) : null;
            if ((attribute != null) && (attribute.Tool == typeof(XslCompiledTransform).FullName))
            {
                if (new System.Version("4.0.0.0").CompareTo(new System.Version(attribute.Version)) < 0)
                {
                    throw new ArgumentException(Res.GetString("Xslt_IncompatibleCompiledStylesheetVersion", new object[] { attribute.Version, "4.0.0.0" }), "compiledStylesheet");
                }
                FieldInfo field = compiledStylesheet.GetField("staticData", BindingFlags.NonPublic | BindingFlags.Static);
                FieldInfo info2 = compiledStylesheet.GetField("ebTypes", BindingFlags.NonPublic | BindingFlags.Static);
                if ((field != null) && (info2 != null))
                {
                    new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
                    byte[] queryData = field.GetValue(null) as byte[];
                    if (queryData != null)
                    {
                        MethodInfo method = compiledStylesheet.GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Static);
                        Type[] earlyBoundTypes = (Type[]) info2.GetValue(null);
                        this.Load(method, queryData, earlyBoundTypes);
                        return;
                    }
                }
            }
            if (this.command == null)
            {
                throw new ArgumentException(Res.GetString("Xslt_NotCompiledStylesheet", new object[] { compiledStylesheet.FullName }), "compiledStylesheet");
            }
        }

        public void Load(XmlReader stylesheet)
        {
            this.Reset();
            this.LoadInternal(stylesheet, XsltSettings.Default, new XmlUrlResolver());
        }

        public void Load(IXPathNavigable stylesheet)
        {
            this.Reset();
            this.LoadInternal(stylesheet, XsltSettings.Default, new XmlUrlResolver());
        }

        public void Load(MethodInfo executeMethod, byte[] queryData, Type[] earlyBoundTypes)
        {
            this.Reset();
            if (executeMethod == null)
            {
                throw new ArgumentNullException("executeMethod");
            }
            if (queryData == null)
            {
                throw new ArgumentNullException("queryData");
            }
            DynamicMethod method = executeMethod as DynamicMethod;
            Delegate delegate2 = (method != null) ? method.CreateDelegate(typeof(ExecuteDelegate)) : Delegate.CreateDelegate(typeof(ExecuteDelegate), executeMethod);
            this.command = new XmlILCommand((ExecuteDelegate) delegate2, new XmlQueryStaticData(queryData, earlyBoundTypes));
            this.outputSettings = this.command.StaticData.DefaultWriterSettings;
        }

        public void Load(string stylesheetUri, XsltSettings settings, XmlResolver stylesheetResolver)
        {
            this.Reset();
            if (stylesheetUri == null)
            {
                throw new ArgumentNullException("stylesheetUri");
            }
            this.LoadInternal(stylesheetUri, settings, stylesheetResolver);
        }

        public void Load(XmlReader stylesheet, XsltSettings settings, XmlResolver stylesheetResolver)
        {
            this.Reset();
            this.LoadInternal(stylesheet, settings, stylesheetResolver);
        }

        public void Load(IXPathNavigable stylesheet, XsltSettings settings, XmlResolver stylesheetResolver)
        {
            this.Reset();
            this.LoadInternal(stylesheet, settings, stylesheetResolver);
        }

        private CompilerResults LoadInternal(object stylesheet, XsltSettings settings, XmlResolver stylesheetResolver)
        {
            if (stylesheet == null)
            {
                throw new ArgumentNullException("stylesheet");
            }
            if (settings == null)
            {
                settings = XsltSettings.Default;
            }
            this.CompileXsltToQil(stylesheet, settings, stylesheetResolver);
            CompilerError firstError = this.GetFirstError();
            if (firstError != null)
            {
                throw new XslLoadException(firstError);
            }
            if (!settings.CheckOnly)
            {
                this.CompileQilToMsil(settings);
            }
            return this.compilerResults;
        }

        internal static void PrintQil(object qil, XmlWriter xw, bool printComments, bool printTypes, bool printLineInfo)
        {
            QilExpression node = (QilExpression) qil;
            QilXmlWriter.Options none = QilXmlWriter.Options.None;
            if (printComments)
            {
                none |= QilXmlWriter.Options.Annotations;
            }
            if (printTypes)
            {
                none |= QilXmlWriter.Options.TypeInfo;
            }
            if (printLineInfo)
            {
                none |= QilXmlWriter.Options.LineInfo;
            }
            new QilXmlWriter(xw, none).ToXml(node);
            xw.Flush();
        }

        private void Reset()
        {
            this.compilerResults = null;
            this.outputSettings = null;
            this.qil = null;
            this.command = null;
        }

        private QilExpression TestCompile(object stylesheet, XsltSettings settings, XmlResolver stylesheetResolver)
        {
            this.Reset();
            this.CompileXsltToQil(stylesheet, settings, stylesheetResolver);
            return this.qil;
        }

        private void TestGenerate(XsltSettings settings)
        {
            this.CompileQilToMsil(settings);
        }

        public void Transform(string inputUri, string resultsFile)
        {
            if (inputUri == null)
            {
                throw new ArgumentNullException("inputUri");
            }
            if (resultsFile == null)
            {
                throw new ArgumentNullException("resultsFile");
            }
            using (XmlReader reader = XmlReader.Create(inputUri, ReaderSettings))
            {
                using (XmlWriter writer = XmlWriter.Create(resultsFile, this.OutputSettings))
                {
                    this.Transform(reader, null, writer, new XmlUrlResolver());
                    writer.Close();
                }
            }
        }

        public void Transform(string inputUri, XmlWriter results)
        {
            CheckArguments(inputUri, results);
            using (XmlReader reader = XmlReader.Create(inputUri, ReaderSettings))
            {
                this.Transform(reader, null, results, new XmlUrlResolver());
            }
        }

        public void Transform(XmlReader input, XmlWriter results)
        {
            CheckArguments(input, results);
            this.Transform(input, null, results, new XmlUrlResolver());
        }

        public void Transform(IXPathNavigable input, XmlWriter results)
        {
            CheckArguments(input, results);
            this.Transform(input, null, results, new XmlUrlResolver());
        }

        public void Transform(string inputUri, XsltArgumentList arguments, Stream results)
        {
            CheckArguments(inputUri, results);
            using (XmlReader reader = XmlReader.Create(inputUri, ReaderSettings))
            {
                using (XmlWriter writer = XmlWriter.Create(results, this.OutputSettings))
                {
                    this.Transform(reader, arguments, writer, new XmlUrlResolver());
                    writer.Close();
                }
            }
        }

        public void Transform(string inputUri, XsltArgumentList arguments, TextWriter results)
        {
            CheckArguments(inputUri, results);
            using (XmlReader reader = XmlReader.Create(inputUri, ReaderSettings))
            {
                using (XmlWriter writer = XmlWriter.Create(results, this.OutputSettings))
                {
                    this.Transform(reader, arguments, writer, new XmlUrlResolver());
                    writer.Close();
                }
            }
        }

        public void Transform(string inputUri, XsltArgumentList arguments, XmlWriter results)
        {
            CheckArguments(inputUri, results);
            using (XmlReader reader = XmlReader.Create(inputUri, ReaderSettings))
            {
                this.Transform(reader, arguments, results, new XmlUrlResolver());
            }
        }

        public void Transform(XmlReader input, XsltArgumentList arguments, Stream results)
        {
            CheckArguments(input, results);
            using (XmlWriter writer = XmlWriter.Create(results, this.OutputSettings))
            {
                this.Transform(input, arguments, writer, new XmlUrlResolver());
                writer.Close();
            }
        }

        public void Transform(XmlReader input, XsltArgumentList arguments, TextWriter results)
        {
            CheckArguments(input, results);
            using (XmlWriter writer = XmlWriter.Create(results, this.OutputSettings))
            {
                this.Transform(input, arguments, writer, new XmlUrlResolver());
                writer.Close();
            }
        }

        public void Transform(XmlReader input, XsltArgumentList arguments, XmlWriter results)
        {
            CheckArguments(input, results);
            this.Transform(input, arguments, results, new XmlUrlResolver());
        }

        public void Transform(IXPathNavigable input, XsltArgumentList arguments, Stream results)
        {
            CheckArguments(input, results);
            using (XmlWriter writer = XmlWriter.Create(results, this.OutputSettings))
            {
                this.Transform(input, arguments, writer, new XmlUrlResolver());
                writer.Close();
            }
        }

        public void Transform(IXPathNavigable input, XsltArgumentList arguments, TextWriter results)
        {
            CheckArguments(input, results);
            using (XmlWriter writer = XmlWriter.Create(results, this.OutputSettings))
            {
                this.Transform(input, arguments, writer, new XmlUrlResolver());
                writer.Close();
            }
        }

        public void Transform(IXPathNavigable input, XsltArgumentList arguments, XmlWriter results)
        {
            CheckArguments(input, results);
            this.Transform(input, arguments, results, new XmlUrlResolver());
        }

        private void Transform(string inputUri, XsltArgumentList arguments, XmlWriter results, XmlResolver documentResolver)
        {
            this.command.Execute(inputUri, documentResolver, arguments, results);
        }

        public void Transform(XmlReader input, XsltArgumentList arguments, XmlWriter results, XmlResolver documentResolver)
        {
            CheckArguments(input, results);
            this.CheckCommand();
            this.command.Execute(input, documentResolver, arguments, results);
        }

        public void Transform(IXPathNavigable input, XsltArgumentList arguments, XmlWriter results, XmlResolver documentResolver)
        {
            CheckArguments(input, results);
            this.CheckCommand();
            this.command.Execute(input.CreateNavigator(), documentResolver, arguments, results);
        }

        internal CompilerErrorCollection Errors
        {
            get
            {
                if (this.compilerResults == null)
                {
                    return null;
                }
                return this.compilerResults.Errors;
            }
        }

        public XmlWriterSettings OutputSettings
        {
            get
            {
                return this.outputSettings;
            }
        }

        public TempFileCollection TemporaryFiles
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                if (this.compilerResults == null)
                {
                    return null;
                }
                return this.compilerResults.TempFiles;
            }
        }
    }
}

