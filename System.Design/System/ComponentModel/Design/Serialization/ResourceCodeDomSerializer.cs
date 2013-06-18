namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Windows.Forms;

    internal class ResourceCodeDomSerializer : CodeDomSerializer
    {
        private static ResourceCodeDomSerializer defaultSerializer;

        internal void ApplyCacheEntry(IDesignerSerializationManager manager, ComponentCache.Entry entry)
        {
            SerializationResourceManager resourceManager = this.GetResourceManager(manager);
            if (entry.Metadata != null)
            {
                foreach (ComponentCache.ResourceEntry entry2 in entry.Metadata)
                {
                    resourceManager.SetMetadata(manager, entry2.Name, entry2.Value, entry2.ShouldSerializeValue, true);
                }
            }
            if (entry.Resources != null)
            {
                foreach (ComponentCache.ResourceEntry entry3 in entry.Resources)
                {
                    manager.Context.Push(entry3.PropertyDescriptor);
                    manager.Context.Push(entry3.ExpressionContext);
                    try
                    {
                        resourceManager.SetValue(manager, entry3.Name, entry3.Value, entry3.ForceInvariant, entry3.ShouldSerializeValue, entry3.EnsureInvariant, true);
                    }
                    finally
                    {
                        manager.Context.Pop();
                        manager.Context.Pop();
                    }
                }
            }
        }

        private SerializationResourceManager CreateResourceManager(IDesignerSerializationManager manager)
        {
            SerializationResourceManager resourceManager = this.GetResourceManager(manager);
            if (!resourceManager.DeclarationAdded)
            {
                resourceManager.DeclarationAdded = true;
                manager.SetName(resourceManager, this.ResourceManagerName);
            }
            return resourceManager;
        }

        public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
        {
            object obj2 = null;
            if ((manager == null) || (codeObject == null))
            {
                throw new ArgumentNullException((manager == null) ? "manager" : "codeObject");
            }
            using (CodeDomSerializerBase.TraceScope("ResourceCodeDomSerializer::Deserialize"))
            {
                CodeExpression expression = codeObject as CodeExpression;
                if (expression != null)
                {
                    return base.DeserializeExpression(manager, null, expression);
                }
                CodeStatementCollection statements = codeObject as CodeStatementCollection;
                if (statements != null)
                {
                    foreach (CodeStatement statement in statements)
                    {
                        if (statement is CodeVariableDeclarationStatement)
                        {
                            CodeVariableDeclarationStatement statement2 = (CodeVariableDeclarationStatement) statement;
                            if (statement2.Name.Equals(this.ResourceManagerName))
                            {
                                obj2 = this.CreateResourceManager(manager);
                            }
                        }
                        else if (obj2 == null)
                        {
                            obj2 = base.DeserializeStatementToInstance(manager, statement);
                        }
                        else
                        {
                            base.DeserializeStatement(manager, statement);
                        }
                    }
                    return obj2;
                }
                if (!(codeObject is CodeStatement))
                {
                    string str = string.Format(CultureInfo.CurrentCulture, "{0}, {1}, {2}", new object[] { typeof(CodeExpression).Name, typeof(CodeStatement).Name, typeof(CodeStatementCollection).Name });
                    throw new ArgumentException(System.Design.SR.GetString("SerializerBadElementTypes", new object[] { codeObject.GetType().Name, str }));
                }
            }
            return obj2;
        }

        protected override object DeserializeInstance(IDesignerSerializationManager manager, System.Type type, object[] parameters, string name, bool addToContainer)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (((name != null) && name.Equals(this.ResourceManagerName)) && typeof(ResourceManager).IsAssignableFrom(type))
            {
                return this.CreateResourceManager(manager);
            }
            return manager.CreateInstance(type, parameters, name, addToContainer);
        }

        public object DeserializeInvariant(IDesignerSerializationManager manager, string resourceName)
        {
            return this.GetResourceManager(manager).GetObject(resourceName, true);
        }

        private System.Type GetCastType(IDesignerSerializationManager manager, object value)
        {
            ExpressionContext context = (ExpressionContext) manager.Context[typeof(ExpressionContext)];
            if (context != null)
            {
                return context.ExpressionType;
            }
            if (value == null)
            {
                return null;
            }
            System.Type baseType = value.GetType();
            while (!baseType.IsPublic && !baseType.IsNestedPublic)
            {
                baseType = baseType.BaseType;
            }
            return baseType;
        }

        public IDictionaryEnumerator GetEnumerator(IDesignerSerializationManager manager, CultureInfo culture)
        {
            return this.GetResourceManager(manager).GetEnumerator(culture);
        }

        public IDictionaryEnumerator GetMetadataEnumerator(IDesignerSerializationManager manager)
        {
            return this.GetResourceManager(manager).GetMetadataEnumerator();
        }

        private SerializationResourceManager GetResourceManager(IDesignerSerializationManager manager)
        {
            SerializationResourceManager context = manager.Context[typeof(SerializationResourceManager)] as SerializationResourceManager;
            if (context == null)
            {
                context = new SerializationResourceManager(manager);
                manager.Context.Append(context);
            }
            return context;
        }

        public override string GetTargetComponentName(CodeStatement statement, CodeExpression expression, System.Type type)
        {
            string fieldName = null;
            CodeExpressionStatement statement2 = statement as CodeExpressionStatement;
            if (statement2 != null)
            {
                CodeMethodInvokeExpression expression2 = statement2.Expression as CodeMethodInvokeExpression;
                if (expression2 != null)
                {
                    CodeMethodReferenceExpression method = expression2.Method;
                    if (((method != null) && string.Equals(method.MethodName, "ApplyResources", StringComparison.OrdinalIgnoreCase)) && (expression2.Parameters.Count > 0))
                    {
                        CodeFieldReferenceExpression expression4 = expression2.Parameters[0] as CodeFieldReferenceExpression;
                        CodeVariableReferenceExpression expression5 = expression2.Parameters[0] as CodeVariableReferenceExpression;
                        if ((expression4 != null) && (expression4.TargetObject is CodeThisReferenceExpression))
                        {
                            fieldName = expression4.FieldName;
                        }
                        else if (expression5 != null)
                        {
                            fieldName = expression5.VariableName;
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(fieldName))
            {
                fieldName = base.GetTargetComponentName(statement, expression, type);
            }
            return fieldName;
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            return this.Serialize(manager, value, false, false, true);
        }

        public object Serialize(IDesignerSerializationManager manager, object value, bool shouldSerializeInvariant)
        {
            return this.Serialize(manager, value, false, shouldSerializeInvariant, true);
        }

        public object Serialize(IDesignerSerializationManager manager, object value, bool shouldSerializeInvariant, bool ensureInvariant)
        {
            return this.Serialize(manager, value, false, shouldSerializeInvariant, ensureInvariant);
        }

        private object Serialize(IDesignerSerializationManager manager, object value, bool forceInvariant, bool shouldSerializeInvariant, bool ensureInvariant)
        {
            using (CodeDomSerializerBase.TraceScope("ResourceCodeDomSerializer::Serialize"))
            {
                bool flag;
                string str3;
                SerializationResourceManager resourceManager = this.GetResourceManager(manager);
                CodeStatementCollection statements = (CodeStatementCollection) manager.Context[typeof(CodeStatementCollection)];
                if (!forceInvariant)
                {
                    if (!resourceManager.DeclarationAdded)
                    {
                        resourceManager.DeclarationAdded = true;
                        RootContext context = manager.Context[typeof(RootContext)] as RootContext;
                        if (statements != null)
                        {
                            CodeExpression[] expressionArray;
                            if (context != null)
                            {
                                string name = manager.GetName(context.Value);
                                expressionArray = new CodeExpression[] { new CodeTypeOfExpression(name) };
                            }
                            else
                            {
                                expressionArray = new CodeExpression[] { new CodePrimitiveExpression(this.ResourceManagerName) };
                            }
                            CodeExpression initExpression = new CodeObjectCreateExpression(typeof(ComponentResourceManager), expressionArray);
                            statements.Add(new CodeVariableDeclarationStatement(typeof(ComponentResourceManager), this.ResourceManagerName, initExpression));
                            base.SetExpression(manager, resourceManager, new CodeVariableReferenceExpression(this.ResourceManagerName));
                            resourceManager.ExpressionAdded = true;
                        }
                    }
                    else if (!resourceManager.ExpressionAdded)
                    {
                        if (base.GetExpression(manager, resourceManager) == null)
                        {
                            base.SetExpression(manager, resourceManager, new CodeVariableReferenceExpression(this.ResourceManagerName));
                        }
                        resourceManager.ExpressionAdded = true;
                    }
                }
                ExpressionContext tree = (ExpressionContext) manager.Context[typeof(ExpressionContext)];
                string str2 = resourceManager.SetValue(manager, tree, value, forceInvariant, shouldSerializeInvariant, ensureInvariant, false);
                if ((value is string) || ((tree != null) && (tree.ExpressionType == typeof(string))))
                {
                    flag = false;
                    str3 = "GetString";
                }
                else
                {
                    flag = true;
                    str3 = "GetObject";
                }
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                    Method = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(this.ResourceManagerName), str3)
                };
                expression.Parameters.Add(new CodePrimitiveExpression(str2));
                if (flag)
                {
                    System.Type castType = this.GetCastType(manager, value);
                    if (castType != null)
                    {
                        return new CodeCastExpression(castType, expression);
                    }
                    return expression;
                }
                return expression;
            }
        }

        public object SerializeInvariant(IDesignerSerializationManager manager, object value, bool shouldSerializeValue)
        {
            return this.Serialize(manager, value, true, shouldSerializeValue, true);
        }

        public void SerializeMetadata(IDesignerSerializationManager manager, string name, object value, bool shouldSerializeValue)
        {
            using (CodeDomSerializerBase.TraceScope("ResourceCodeDomSerializer::SerializeMetadata"))
            {
                this.GetResourceManager(manager).SetMetadata(manager, name, value, shouldSerializeValue, false);
            }
        }

        public void WriteResource(IDesignerSerializationManager manager, string name, object value)
        {
            using (CodeDomSerializerBase.TraceScope("ResourceCodeDomSerializer::WriteResource"))
            {
                this.GetResourceManager(manager).SetValue(manager, name, value, false, false, true, false);
            }
        }

        public void WriteResourceInvariant(IDesignerSerializationManager manager, string name, object value)
        {
            using (CodeDomSerializerBase.TraceScope("ResourceCodeDomSerializer::WriteResourceInvariant"))
            {
                this.GetResourceManager(manager).SetValue(manager, name, value, true, true, true, false);
            }
        }

        internal static ResourceCodeDomSerializer Default
        {
            get
            {
                if (defaultSerializer == null)
                {
                    defaultSerializer = new ResourceCodeDomSerializer();
                }
                return defaultSerializer;
            }
        }

        private string ResourceManagerName
        {
            get
            {
                return "resources";
            }
        }

        internal class SerializationResourceManager : ComponentResourceManager
        {
            private bool checkedLocalizationLanguage;
            private bool declarationAdded;
            private bool expressionAdded;
            private bool invariantCultureResourcesDirty;
            private CultureInfo localizationLanguage;
            private IDesignerSerializationManager manager;
            private Hashtable mergedMetadata;
            private Hashtable metadata;
            private bool metadataResourcesDirty;
            private Hashtable nameTable;
            private Hashtable propertyFillAdded;
            private CultureInfo readCulture;
            private Hashtable resourceSets;
            private static object resourceSetSentinel = new object();
            private object rootComponent;
            private IResourceWriter writer;

            public SerializationResourceManager(IDesignerSerializationManager manager)
            {
                this.manager = manager;
                this.nameTable = new Hashtable();
                manager.SerializationComplete += new EventHandler(this.OnSerializationComplete);
            }

            private void AddCacheEntry(IDesignerSerializationManager manager, string name, object value, bool isMetadata, bool forceInvariant, bool shouldSerializeValue, bool ensureInvariant)
            {
                ComponentCache.Entry entry = manager.Context[typeof(ComponentCache.Entry)] as ComponentCache.Entry;
                if (entry != null)
                {
                    ComponentCache.ResourceEntry re = new ComponentCache.ResourceEntry {
                        Name = name,
                        Value = value,
                        ForceInvariant = forceInvariant,
                        ShouldSerializeValue = shouldSerializeValue,
                        EnsureInvariant = ensureInvariant,
                        PropertyDescriptor = (PropertyDescriptor) manager.Context[typeof(PropertyDescriptor)],
                        ExpressionContext = (ExpressionContext) manager.Context[typeof(ExpressionContext)]
                    };
                    if (isMetadata)
                    {
                        entry.AddMetadata(re);
                    }
                    else
                    {
                        entry.AddResource(re);
                    }
                }
            }

            public bool AddPropertyFill(object value)
            {
                bool flag = false;
                if (this.propertyFillAdded == null)
                {
                    this.propertyFillAdded = new Hashtable();
                }
                else
                {
                    flag = this.propertyFillAdded.ContainsKey(value);
                }
                if (!flag)
                {
                    this.propertyFillAdded[value] = value;
                }
                return !flag;
            }

            public override void ApplyResources(object value, string objectName, CultureInfo culture)
            {
                if (culture == null)
                {
                    culture = this.ReadCulture;
                }
                Control control = value as Control;
                if (control != null)
                {
                    control.SuspendLayout();
                }
                base.ApplyResources(value, objectName, culture);
                if (control != null)
                {
                    control.ResumeLayout(false);
                }
            }

            private CompareValue CompareWithParentValue(string name, object value)
            {
                if (this.ReadCulture.Equals(CultureInfo.InvariantCulture))
                {
                    return CompareValue.Different;
                }
                CultureInfo readCulture = this.ReadCulture;
                do
                {
                    readCulture = readCulture.Parent;
                    Hashtable resourceSet = this.GetResourceSet(readCulture);
                    if ((resourceSet != null) && resourceSet.ContainsKey(name))
                    {
                        object obj2 = (resourceSet != null) ? resourceSet[name] : null;
                        if (obj2 == value)
                        {
                            return CompareValue.Same;
                        }
                        if ((obj2 != null) && obj2.Equals(value))
                        {
                            return CompareValue.Same;
                        }
                        return CompareValue.Different;
                    }
                }
                while (!readCulture.Equals(CultureInfo.InvariantCulture));
                return CompareValue.New;
            }

            private Hashtable CreateResourceSet(IResourceReader reader, CultureInfo culture)
            {
                Hashtable hashtable = new Hashtable();
                try
                {
                    IDictionaryEnumerator enumerator = reader.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        string key = (string) enumerator.Key;
                        object obj2 = enumerator.Value;
                        hashtable[key] = obj2;
                    }
                }
                catch (Exception exception)
                {
                    Exception exception2;
                    string message = exception.Message;
                    if ((message == null) || (message.Length == 0))
                    {
                        message = exception.GetType().Name;
                    }
                    if (culture == CultureInfo.InvariantCulture)
                    {
                        exception2 = new SerializationException(System.Design.SR.GetString("SerializerResourceExceptionInvariant", new object[] { message }), exception);
                    }
                    else
                    {
                        exception2 = new SerializationException(System.Design.SR.GetString("SerializerResourceException", new object[] { culture.ToString(), message }), exception);
                    }
                    this.manager.ReportError(exception2);
                }
                return hashtable;
            }

            public IDictionaryEnumerator GetEnumerator(CultureInfo culture)
            {
                Hashtable resourceSet = this.GetResourceSet(culture);
                if (resourceSet != null)
                {
                    return resourceSet.GetEnumerator();
                }
                return null;
            }

            private Hashtable GetMetadata()
            {
                if (this.metadata == null)
                {
                    IResourceService service = (IResourceService) this.manager.GetService(typeof(IResourceService));
                    if (service != null)
                    {
                        IResourceReader resourceReader = service.GetResourceReader(CultureInfo.InvariantCulture);
                        if (resourceReader != null)
                        {
                            try
                            {
                                ResXResourceReader reader2 = resourceReader as ResXResourceReader;
                                if (reader2 != null)
                                {
                                    this.metadata = new Hashtable();
                                    IDictionaryEnumerator metadataEnumerator = reader2.GetMetadataEnumerator();
                                    while (metadataEnumerator.MoveNext())
                                    {
                                        this.metadata[metadataEnumerator.Key] = metadataEnumerator.Value;
                                    }
                                }
                            }
                            finally
                            {
                                resourceReader.Close();
                            }
                        }
                    }
                }
                return this.metadata;
            }

            public IDictionaryEnumerator GetMetadataEnumerator()
            {
                if (this.mergedMetadata == null)
                {
                    Hashtable metadata = this.GetMetadata();
                    if (metadata != null)
                    {
                        Hashtable resourceSet = this.GetResourceSet(CultureInfo.InvariantCulture);
                        if (resourceSet != null)
                        {
                            foreach (DictionaryEntry entry in resourceSet)
                            {
                                if (!metadata.ContainsKey(entry.Key))
                                {
                                    metadata.Add(entry.Key, entry.Value);
                                }
                            }
                        }
                        this.mergedMetadata = metadata;
                    }
                }
                if (this.mergedMetadata != null)
                {
                    return this.mergedMetadata.GetEnumerator();
                }
                return null;
            }

            public override object GetObject(string resourceName)
            {
                return this.GetObject(resourceName, false);
            }

            public object GetObject(string resourceName, bool forceInvariant)
            {
                CultureInfo invariantCulture;
                if (forceInvariant)
                {
                    invariantCulture = CultureInfo.InvariantCulture;
                }
                else
                {
                    invariantCulture = this.ReadCulture;
                }
                object obj2 = null;
                while (obj2 == null)
                {
                    Hashtable resourceSet = this.GetResourceSet(invariantCulture);
                    if (resourceSet != null)
                    {
                        obj2 = resourceSet[resourceName];
                    }
                    CultureInfo info2 = invariantCulture;
                    invariantCulture = invariantCulture.Parent;
                    if (info2.Equals(invariantCulture))
                    {
                        return obj2;
                    }
                }
                return obj2;
            }

            private Hashtable GetResourceSet(CultureInfo culture)
            {
                Hashtable hashtable = null;
                object obj2 = this.ResourceTable[culture];
                if (obj2 == null)
                {
                    IResourceService service = (IResourceService) this.manager.GetService(typeof(IResourceService));
                    if (service != null)
                    {
                        IResourceReader resourceReader = service.GetResourceReader(culture);
                        if (resourceReader != null)
                        {
                            try
                            {
                                hashtable = this.CreateResourceSet(resourceReader, culture);
                            }
                            finally
                            {
                                resourceReader.Close();
                            }
                            this.ResourceTable[culture] = hashtable;
                            return hashtable;
                        }
                        if (culture.Equals(CultureInfo.InvariantCulture))
                        {
                            hashtable = new Hashtable();
                            this.ResourceTable[culture] = hashtable;
                            return hashtable;
                        }
                        this.ResourceTable[culture] = resourceSetSentinel;
                    }
                    return hashtable;
                }
                return (obj2 as Hashtable);
            }

            public override ResourceSet GetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
            {
                if (culture == null)
                {
                    throw new ArgumentNullException("culture");
                }
                CultureInfo info = culture;
                do
                {
                    Hashtable resourceSet = this.GetResourceSet(culture);
                    if (resourceSet != null)
                    {
                        return new CodeDomResourceSet(resourceSet);
                    }
                    info = culture;
                    culture = culture.Parent;
                }
                while (tryParents && !info.Equals(culture));
                if (createIfNotExists)
                {
                    return new CodeDomResourceSet();
                }
                return null;
            }

            public override string GetString(string resourceName)
            {
                return (this.GetObject(resourceName, false) as string);
            }

            private void OnSerializationComplete(object sender, EventArgs e)
            {
                if (this.writer != null)
                {
                    this.writer.Close();
                    this.writer = null;
                }
                if (this.invariantCultureResourcesDirty || this.metadataResourcesDirty)
                {
                    IResourceService service = (IResourceService) this.manager.GetService(typeof(IResourceService));
                    if (service != null)
                    {
                        IResourceWriter resourceWriter = service.GetResourceWriter(CultureInfo.InvariantCulture);
                        try
                        {
                            object obj2 = this.ResourceTable[CultureInfo.InvariantCulture];
                            IDictionaryEnumerator enumerator = ((Hashtable) obj2).GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                string key = (string) enumerator.Key;
                                object obj3 = enumerator.Value;
                                resourceWriter.AddResource(key, obj3);
                            }
                            this.invariantCultureResourcesDirty = false;
                            ResXResourceWriter writer2 = resourceWriter as ResXResourceWriter;
                            if (writer2 != null)
                            {
                                foreach (DictionaryEntry entry in this.metadata)
                                {
                                    writer2.AddMetadata((string) entry.Key, entry.Value);
                                }
                            }
                            this.metadataResourcesDirty = false;
                            return;
                        }
                        finally
                        {
                            resourceWriter.Close();
                        }
                    }
                    this.invariantCultureResourcesDirty = false;
                    this.metadataResourcesDirty = false;
                }
            }

            public void SetMetadata(IDesignerSerializationManager manager, string resourceName, object value, bool shouldSerializeValue, bool applyingCachedResources)
            {
                if ((value == null) || value.GetType().IsSerializable)
                {
                    if (this.ReadCulture.Equals(CultureInfo.InvariantCulture))
                    {
                        ResXResourceWriter writer = this.Writer as ResXResourceWriter;
                        if (shouldSerializeValue)
                        {
                            if (writer != null)
                            {
                                writer.AddMetadata(resourceName, value);
                            }
                            else
                            {
                                this.Writer.AddResource(resourceName, value);
                            }
                        }
                    }
                    else
                    {
                        Hashtable metadata = null;
                        IResourceWriter resourceWriter = null;
                        IResourceService service = (IResourceService) manager.GetService(typeof(IResourceService));
                        if (service != null)
                        {
                            resourceWriter = service.GetResourceWriter(CultureInfo.InvariantCulture);
                        }
                        Hashtable resourceSet = this.GetResourceSet(CultureInfo.InvariantCulture);
                        if ((resourceWriter == null) || (resourceWriter is ResXResourceWriter))
                        {
                            metadata = this.GetMetadata();
                            if (metadata == null)
                            {
                                this.metadata = new Hashtable();
                                metadata = this.metadata;
                            }
                            if (resourceSet.ContainsKey(resourceName))
                            {
                                resourceSet.Remove(resourceName);
                            }
                            this.metadataResourcesDirty = true;
                        }
                        else
                        {
                            metadata = resourceSet;
                            this.invariantCultureResourcesDirty = true;
                        }
                        if (metadata != null)
                        {
                            if (shouldSerializeValue)
                            {
                                metadata[resourceName] = value;
                            }
                            else
                            {
                                metadata.Remove(resourceName);
                            }
                        }
                        this.mergedMetadata = null;
                    }
                    if (!applyingCachedResources)
                    {
                        this.AddCacheEntry(manager, resourceName, value, true, false, shouldSerializeValue, false);
                    }
                }
            }

            public string SetValue(IDesignerSerializationManager manager, ExpressionContext tree, object value, bool forceInvariant, bool shouldSerializeInvariant, bool ensureInvariant, bool applyingCachedResources)
            {
                string name = null;
                bool flag = false;
                if (tree != null)
                {
                    string propertyName;
                    if (tree.Owner == this.RootComponent)
                    {
                        name = "$this";
                    }
                    else
                    {
                        name = manager.GetName(tree.Owner);
                        if (name == null)
                        {
                            IReferenceService service = (IReferenceService) manager.GetService(typeof(IReferenceService));
                            if (service != null)
                            {
                                name = service.GetName(tree.Owner);
                            }
                        }
                    }
                    CodeExpression expression = tree.Expression;
                    if (expression is CodePropertyReferenceExpression)
                    {
                        propertyName = ((CodePropertyReferenceExpression) expression).PropertyName;
                    }
                    else if (expression is CodeFieldReferenceExpression)
                    {
                        propertyName = ((CodeFieldReferenceExpression) expression).FieldName;
                    }
                    else if (expression is CodeMethodReferenceExpression)
                    {
                        propertyName = ((CodeMethodReferenceExpression) expression).MethodName;
                        if (propertyName.StartsWith("Set"))
                        {
                            propertyName = propertyName.Substring(3);
                        }
                    }
                    else
                    {
                        propertyName = null;
                    }
                    if (name == null)
                    {
                        name = "resource";
                    }
                    if (propertyName != null)
                    {
                        name = name + "." + propertyName;
                    }
                }
                else
                {
                    name = "resource";
                    flag = true;
                }
                string key = name;
                int num = 1;
                do
                {
                    if (flag)
                    {
                        key = name + num.ToString(CultureInfo.InvariantCulture);
                        num++;
                    }
                    else
                    {
                        flag = true;
                    }
                }
                while (this.nameTable.ContainsKey(key));
                this.SetValue(manager, key, value, forceInvariant, shouldSerializeInvariant, ensureInvariant, applyingCachedResources);
                this.nameTable[key] = key;
                return key;
            }

            public void SetValue(IDesignerSerializationManager manager, string resourceName, object value, bool forceInvariant, bool shouldSerializeInvariant, bool ensureInvariant, bool applyingCachedResources)
            {
                if ((value == null) || value.GetType().IsSerializable)
                {
                    if (forceInvariant)
                    {
                        if (this.ReadCulture.Equals(CultureInfo.InvariantCulture))
                        {
                            if (shouldSerializeInvariant)
                            {
                                this.Writer.AddResource(resourceName, value);
                            }
                        }
                        else
                        {
                            Hashtable resourceSet = this.GetResourceSet(CultureInfo.InvariantCulture);
                            if (shouldSerializeInvariant)
                            {
                                resourceSet[resourceName] = value;
                            }
                            else
                            {
                                resourceSet.Remove(resourceName);
                            }
                            this.invariantCultureResourcesDirty = true;
                        }
                    }
                    else
                    {
                        switch (this.CompareWithParentValue(resourceName, value))
                        {
                            case CompareValue.Different:
                                this.Writer.AddResource(resourceName, value);
                                break;

                            case CompareValue.New:
                                if (!ensureInvariant)
                                {
                                    bool flag = true;
                                    bool flag2 = false;
                                    PropertyDescriptor descriptor = (PropertyDescriptor) manager.Context[typeof(PropertyDescriptor)];
                                    if (descriptor != null)
                                    {
                                        ExpressionContext context = (ExpressionContext) manager.Context[typeof(ExpressionContext)];
                                        if ((context != null) && (context.Expression is CodePropertyReferenceExpression))
                                        {
                                            flag = descriptor.ShouldSerializeValue(context.Owner);
                                            flag2 = !descriptor.CanResetValue(context.Owner);
                                        }
                                    }
                                    if (flag)
                                    {
                                        this.Writer.AddResource(resourceName, value);
                                        if (flag2)
                                        {
                                            this.GetResourceSet(CultureInfo.InvariantCulture)[resourceName] = value;
                                            this.invariantCultureResourcesDirty = true;
                                        }
                                    }
                                    break;
                                }
                                this.GetResourceSet(CultureInfo.InvariantCulture)[resourceName] = value;
                                this.invariantCultureResourcesDirty = true;
                                this.Writer.AddResource(resourceName, value);
                                break;
                        }
                    }
                    if (!applyingCachedResources)
                    {
                        this.AddCacheEntry(manager, resourceName, value, false, forceInvariant, shouldSerializeInvariant, ensureInvariant);
                    }
                }
            }

            public bool DeclarationAdded
            {
                get
                {
                    return this.declarationAdded;
                }
                set
                {
                    this.declarationAdded = value;
                }
            }

            public bool ExpressionAdded
            {
                get
                {
                    return this.expressionAdded;
                }
                set
                {
                    this.expressionAdded = value;
                }
            }

            private CultureInfo LocalizationLanguage
            {
                get
                {
                    if (!this.checkedLocalizationLanguage)
                    {
                        RootContext context = this.manager.Context[typeof(RootContext)] as RootContext;
                        if (context != null)
                        {
                            object component = context.Value;
                            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["LoadLanguage"];
                            if ((descriptor != null) && (descriptor.PropertyType == typeof(CultureInfo)))
                            {
                                this.localizationLanguage = (CultureInfo) descriptor.GetValue(component);
                            }
                        }
                        this.checkedLocalizationLanguage = true;
                    }
                    return this.localizationLanguage;
                }
            }

            private CultureInfo ReadCulture
            {
                get
                {
                    if (this.readCulture == null)
                    {
                        CultureInfo localizationLanguage = this.LocalizationLanguage;
                        if (localizationLanguage != null)
                        {
                            this.readCulture = localizationLanguage;
                        }
                        else
                        {
                            this.readCulture = CultureInfo.InvariantCulture;
                        }
                    }
                    return this.readCulture;
                }
            }

            private Hashtable ResourceTable
            {
                get
                {
                    if (this.resourceSets == null)
                    {
                        this.resourceSets = new Hashtable();
                    }
                    return this.resourceSets;
                }
            }

            private object RootComponent
            {
                get
                {
                    if (this.rootComponent == null)
                    {
                        RootContext context = this.manager.Context[typeof(RootContext)] as RootContext;
                        if (context != null)
                        {
                            this.rootComponent = context.Value;
                        }
                    }
                    return this.rootComponent;
                }
            }

            private IResourceWriter Writer
            {
                get
                {
                    if (this.writer == null)
                    {
                        IResourceService service = (IResourceService) this.manager.GetService(typeof(IResourceService));
                        if (service != null)
                        {
                            this.writer = service.GetResourceWriter(this.ReadCulture);
                        }
                        else
                        {
                            this.writer = new ResourceWriter(new MemoryStream());
                        }
                    }
                    return this.writer;
                }
            }

            private class CodeDomResourceSet : ResourceSet
            {
                public CodeDomResourceSet()
                {
                }

                public CodeDomResourceSet(Hashtable resources)
                {
                    base.Table = resources;
                }
            }

            private enum CompareValue
            {
                Same,
                Different,
                New
            }
        }
    }
}

