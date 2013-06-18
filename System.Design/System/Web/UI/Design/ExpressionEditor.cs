namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Configuration;
    using System.Design;
    using System.Runtime.InteropServices;
    using System.Web.Compilation;
    using System.Web.Configuration;

    public abstract class ExpressionEditor
    {
        private string _expressionPrefix;
        private const string expressionEditorsByTypeKey = "ExpressionEditorsByType";
        private const string expressionEditorsKey = "ExpressionEditors";

        protected ExpressionEditor()
        {
        }

        public abstract object EvaluateExpression(string expression, object parseTimeData, Type propertyType, IServiceProvider serviceProvider);
        internal static Type GetExpressionBuilderType(string expressionPrefix, IServiceProvider serviceProvider, out string trueExpressionPrefix)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            trueExpressionPrefix = expressionPrefix;
            if (expressionPrefix.Length == 0)
            {
                return null;
            }
            Type type = null;
            IWebApplication application = (IWebApplication) serviceProvider.GetService(typeof(IWebApplication));
            if (application != null)
            {
                System.Configuration.Configuration configuration = application.OpenWebConfiguration(true);
                if (configuration == null)
                {
                    return type;
                }
                CompilationSection section = (CompilationSection) configuration.GetSection("system.web/compilation");
                foreach (System.Web.Configuration.ExpressionBuilder builder in section.ExpressionBuilders)
                {
                    if (string.Equals(expressionPrefix, builder.ExpressionPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        trueExpressionPrefix = builder.ExpressionPrefix;
                        type = Type.GetType(builder.Type);
                        if (type == null)
                        {
                            ITypeResolutionService service = (ITypeResolutionService) serviceProvider.GetService(typeof(ITypeResolutionService));
                            if (service != null)
                            {
                                type = service.GetType(builder.Type);
                            }
                        }
                    }
                }
            }
            return type;
        }

        public static ExpressionEditor GetExpressionEditor(string expressionPrefix, IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            if (expressionPrefix.Length == 0)
            {
                return null;
            }
            ExpressionEditor editor = null;
            IWebApplication service = (IWebApplication) serviceProvider.GetService(typeof(IWebApplication));
            if (service != null)
            {
                IDictionary expressionEditorsCache = GetExpressionEditorsCache(service);
                if (expressionEditorsCache != null)
                {
                    editor = (ExpressionEditor) expressionEditorsCache[expressionPrefix];
                }
                if (editor == null)
                {
                    string str;
                    Type expressionBuilderType = GetExpressionBuilderType(expressionPrefix, serviceProvider, out str);
                    if (expressionBuilderType != null)
                    {
                        editor = GetExpressionEditorInternal(expressionBuilderType, str, service, serviceProvider);
                    }
                }
            }
            return editor;
        }

        public static ExpressionEditor GetExpressionEditor(Type expressionBuilderType, IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            if (expressionBuilderType == null)
            {
                throw new ArgumentNullException("expressionBuilderType");
            }
            ExpressionEditor editor = null;
            IWebApplication service = (IWebApplication) serviceProvider.GetService(typeof(IWebApplication));
            if (service != null)
            {
                IDictionary expressionEditorsByTypeCache = GetExpressionEditorsByTypeCache(service);
                if (expressionEditorsByTypeCache != null)
                {
                    editor = (ExpressionEditor) expressionEditorsByTypeCache[expressionBuilderType];
                }
                if (editor != null)
                {
                    return editor;
                }
                System.Configuration.Configuration configuration = service.OpenWebConfiguration(true);
                if (configuration == null)
                {
                    return editor;
                }
                CompilationSection section = (CompilationSection) configuration.GetSection("system.web/compilation");
                ExpressionBuilderCollection expressionBuilders = section.ExpressionBuilders;
                bool flag = false;
                string fullName = expressionBuilderType.FullName;
                foreach (System.Web.Configuration.ExpressionBuilder builder in expressionBuilders)
                {
                    if (string.Equals(builder.Type, fullName, StringComparison.OrdinalIgnoreCase))
                    {
                        editor = GetExpressionEditorInternal(expressionBuilderType, builder.ExpressionPrefix, service, serviceProvider);
                        flag = true;
                    }
                }
                if (flag)
                {
                    return editor;
                }
                object[] customAttributes = expressionBuilderType.GetCustomAttributes(typeof(ExpressionPrefixAttribute), true);
                ExpressionPrefixAttribute attribute = null;
                if (customAttributes.Length > 0)
                {
                    attribute = (ExpressionPrefixAttribute) customAttributes[0];
                }
                if (attribute != null)
                {
                    System.Web.Configuration.ExpressionBuilder buildProvider = new System.Web.Configuration.ExpressionBuilder(attribute.ExpressionPrefix, expressionBuilderType.FullName);
                    configuration = service.OpenWebConfiguration(false);
                    section = (CompilationSection) configuration.GetSection("system.web/compilation");
                    section.ExpressionBuilders.Add(buildProvider);
                    configuration.Save();
                    editor = GetExpressionEditorInternal(expressionBuilderType, buildProvider.ExpressionPrefix, service, serviceProvider);
                }
            }
            return editor;
        }

        internal static ExpressionEditor GetExpressionEditorInternal(Type expressionBuilderType, string expressionPrefix, IWebApplication webApp, IServiceProvider serviceProvider)
        {
            if (expressionBuilderType == null)
            {
                throw new ArgumentNullException("expressionBuilderType");
            }
            ExpressionEditor editor = null;
            object[] customAttributes = expressionBuilderType.GetCustomAttributes(typeof(ExpressionEditorAttribute), true);
            ExpressionEditorAttribute attribute = null;
            if (customAttributes.Length > 0)
            {
                attribute = (ExpressionEditorAttribute) customAttributes[0];
            }
            if (attribute != null)
            {
                string editorTypeName = attribute.EditorTypeName;
                Type c = Type.GetType(editorTypeName);
                if (c == null)
                {
                    ITypeResolutionService service = (ITypeResolutionService) serviceProvider.GetService(typeof(ITypeResolutionService));
                    if (service != null)
                    {
                        c = service.GetType(editorTypeName);
                    }
                }
                if ((c != null) && typeof(ExpressionEditor).IsAssignableFrom(c))
                {
                    editor = (ExpressionEditor) Activator.CreateInstance(c);
                    editor.SetExpressionPrefix(expressionPrefix);
                }
                IDictionary expressionEditorsCache = GetExpressionEditorsCache(webApp);
                if (expressionEditorsCache != null)
                {
                    expressionEditorsCache[expressionPrefix] = editor;
                }
                IDictionary expressionEditorsByTypeCache = GetExpressionEditorsByTypeCache(webApp);
                if (expressionEditorsByTypeCache != null)
                {
                    expressionEditorsByTypeCache[expressionBuilderType] = editor;
                }
            }
            return editor;
        }

        private static IDictionary GetExpressionEditorsByTypeCache(IWebApplication webApp)
        {
            IDictionaryService service = (IDictionaryService) webApp.GetService(typeof(IDictionaryService));
            if (service == null)
            {
                return null;
            }
            IDictionary dictionary = (IDictionary) service.GetValue("ExpressionEditorsByType");
            if (dictionary == null)
            {
                dictionary = new HybridDictionary();
                service.SetValue("ExpressionEditorsByType", dictionary);
            }
            return dictionary;
        }

        private static IDictionary GetExpressionEditorsCache(IWebApplication webApp)
        {
            IDictionaryService service = (IDictionaryService) webApp.GetService(typeof(IDictionaryService));
            if (service == null)
            {
                return null;
            }
            IDictionary dictionary = (IDictionary) service.GetValue("ExpressionEditors");
            if (dictionary == null)
            {
                dictionary = new HybridDictionary(true);
                service.SetValue("ExpressionEditors", dictionary);
            }
            return dictionary;
        }

        public virtual ExpressionEditorSheet GetExpressionEditorSheet(string expression, IServiceProvider serviceProvider)
        {
            return new GenericExpressionEditorSheet(expression, serviceProvider);
        }

        internal void SetExpressionPrefix(string expressionPrefix)
        {
            this._expressionPrefix = expressionPrefix;
        }

        public string ExpressionPrefix
        {
            get
            {
                return this._expressionPrefix;
            }
        }

        private class GenericExpressionEditorSheet : ExpressionEditorSheet
        {
            private string _expression;

            public GenericExpressionEditorSheet(string expression, IServiceProvider serviceProvider) : base(serviceProvider)
            {
                this._expression = expression;
            }

            public override string GetExpression()
            {
                return this._expression;
            }

            [System.Design.SRDescription("ExpressionEditor_Expression"), DefaultValue("")]
            public string Expression
            {
                get
                {
                    if (this._expression == null)
                    {
                        return string.Empty;
                    }
                    return this._expression;
                }
                set
                {
                    this._expression = value;
                }
            }
        }
    }
}

