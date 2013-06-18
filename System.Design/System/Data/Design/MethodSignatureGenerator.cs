namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;

    public class MethodSignatureGenerator
    {
        private CodeDomProvider codeProvider;
        private Type containerParameterType = typeof(DataSet);
        private string datasetClassName;
        private DesignTable designTable;
        private static readonly char endOfStatement = ';';
        private bool getMethod;
        private DbSource methodSource;
        private bool pagingMethod;
        private ParameterGenerationOption parameterOption;
        private string tableClassName;

        public CodeMemberMethod GenerateMethod()
        {
            if (this.codeProvider == null)
            {
                throw new ArgumentException("codeProvider");
            }
            if (this.methodSource == null)
            {
                throw new ArgumentException("MethodSource");
            }
            QueryGeneratorBase base2 = null;
            if ((this.methodSource.QueryType == QueryType.Rowset) && (this.methodSource.CommandOperation == CommandOperation.Select))
            {
                base2 = new QueryGenerator(null) {
                    ContainerParameterTypeName = this.GetParameterTypeName(),
                    ContainerParameterName = this.GetParameterName(),
                    ContainerParameterType = this.containerParameterType
                };
            }
            else
            {
                base2 = new FunctionGenerator(null);
            }
            base2.DeclarationOnly = true;
            base2.CodeProvider = this.codeProvider;
            base2.MethodSource = this.methodSource;
            base2.MethodName = this.GetMethodName();
            base2.ParameterOption = this.parameterOption;
            base2.GeneratePagingMethod = this.pagingMethod;
            base2.GenerateGetMethod = this.getMethod;
            return base2.Generate();
        }

        public string GenerateMethodSignature()
        {
            if (this.codeProvider == null)
            {
                throw new ArgumentException("codeProvider");
            }
            if (this.methodSource == null)
            {
                throw new ArgumentException("MethodSource");
            }
            string methodName = null;
            CodeTypeDeclaration codeType = this.GenerateMethodWrapper(out methodName);
            StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
            this.codeProvider.GenerateCodeFromType(codeType, writer, null);
            foreach (string str3 in writer.GetStringBuilder().ToString().Split(Environment.NewLine.ToCharArray()))
            {
                if (str3.Contains(methodName))
                {
                    return str3.Trim().TrimEnd(new char[] { endOfStatement });
                }
            }
            return null;
        }

        private CodeTypeDeclaration GenerateMethodWrapper(out string methodName)
        {
            CodeTypeDeclaration declaration = new CodeTypeDeclaration("Wrapper") {
                IsInterface = true
            };
            CodeMemberMethod method = this.GenerateMethod();
            declaration.Members.Add(method);
            methodName = method.Name;
            return declaration;
        }

        public CodeTypeDeclaration GenerateUpdatingMethods()
        {
            if (this.designTable == null)
            {
                throw new InternalException("DesignTable should not be null.");
            }
            if (StringUtil.Empty(this.datasetClassName))
            {
                throw new InternalException("DatasetClassName should not be empty.");
            }
            CodeTypeDeclaration classDeclaration = new CodeTypeDeclaration("wrapper") {
                IsInterface = true
            };
            new QueryHandler(this.codeProvider, this.designTable) { DeclarationsOnly = true }.AddUpdateQueriesToDataComponent(classDeclaration, this.datasetClassName, this.codeProvider);
            return classDeclaration;
        }

        private string GetMethodName()
        {
            if (this.methodSource.QueryType == QueryType.Rowset)
            {
                if (this.getMethod)
                {
                    if (this.pagingMethod)
                    {
                        if (this.methodSource.GeneratorGetMethodNameForPaging != null)
                        {
                            return this.methodSource.GeneratorGetMethodNameForPaging;
                        }
                        return (this.methodSource.GetMethodName + DataComponentNameHandler.PagingMethodSuffix);
                    }
                    if (this.methodSource.GeneratorGetMethodName != null)
                    {
                        return this.methodSource.GeneratorGetMethodName;
                    }
                    return this.methodSource.GetMethodName;
                }
                if (this.pagingMethod)
                {
                    if (this.methodSource.GeneratorSourceNameForPaging != null)
                    {
                        return this.methodSource.GeneratorSourceNameForPaging;
                    }
                    return (this.methodSource.Name + DataComponentNameHandler.PagingMethodSuffix);
                }
                if (this.methodSource.GeneratorSourceName != null)
                {
                    return this.methodSource.GeneratorSourceName;
                }
                return this.methodSource.Name;
            }
            if (this.methodSource.GeneratorSourceName != null)
            {
                return this.methodSource.GeneratorSourceName;
            }
            return this.methodSource.Name;
        }

        private string GetParameterName()
        {
            if (this.containerParameterType == typeof(DataTable))
            {
                return "dataTable";
            }
            return "dataSet";
        }

        private string GetParameterTypeName()
        {
            if (StringUtil.Empty(this.datasetClassName))
            {
                throw new InternalException("DatasetClassName should not be empty.");
            }
            if (!(this.containerParameterType == typeof(DataTable)))
            {
                return this.datasetClassName;
            }
            if (StringUtil.Empty(this.tableClassName))
            {
                throw new InternalException("TableClassName should not be empty.");
            }
            return CodeGenHelper.GetTypeName(this.codeProvider, this.datasetClassName, this.tableClassName);
        }

        public void SetDesignTableContent(string designTableContent)
        {
            DesignDataSource source = new DesignDataSource();
            StringReader textReader = new StringReader(designTableContent);
            source.ReadXmlSchema(textReader, null);
            if ((source.DesignTables == null) || (source.DesignTables.Count != 1))
            {
                throw new InternalException("Unexpected number of sources in deserialized DataSource.");
            }
            IEnumerator enumerator = source.DesignTables.GetEnumerator();
            enumerator.MoveNext();
            this.designTable = (DesignTable) enumerator.Current;
        }

        public void SetMethodSourceContent(string methodSourceContent)
        {
            DesignDataSource source = new DesignDataSource();
            StringReader textReader = new StringReader(methodSourceContent);
            source.ReadXmlSchema(textReader, null);
            if ((source.Sources == null) || (source.Sources.Count != 1))
            {
                throw new InternalException("Unexpected number of sources in deserialized DataSource.");
            }
            IEnumerator enumerator = source.Sources.GetEnumerator();
            enumerator.MoveNext();
            this.methodSource = (DbSource) enumerator.Current;
        }

        public CodeDomProvider CodeProvider
        {
            get
            {
                return this.codeProvider;
            }
            set
            {
                this.codeProvider = value;
            }
        }

        public Type ContainerParameterType
        {
            get
            {
                return this.containerParameterType;
            }
            set
            {
                if ((value != typeof(DataSet)) && (value != typeof(DataTable)))
                {
                    throw new InternalException("Unsupported container parameter type.");
                }
                this.containerParameterType = value;
            }
        }

        public string DataSetClassName
        {
            get
            {
                return this.datasetClassName;
            }
            set
            {
                this.datasetClassName = value;
            }
        }

        public bool IsGetMethod
        {
            get
            {
                return this.getMethod;
            }
            set
            {
                this.getMethod = value;
            }
        }

        public bool PagingMethod
        {
            get
            {
                return this.pagingMethod;
            }
            set
            {
                this.pagingMethod = value;
            }
        }

        public ParameterGenerationOption ParameterOption
        {
            get
            {
                return this.parameterOption;
            }
            set
            {
                this.parameterOption = value;
            }
        }

        public string TableClassName
        {
            get
            {
                return this.tableClassName;
            }
            set
            {
                this.tableClassName = value;
            }
        }
    }
}

