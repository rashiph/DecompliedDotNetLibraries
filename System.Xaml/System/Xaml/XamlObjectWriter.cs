namespace System.Xaml
{
    using MS.Internal.Xaml.Context;
    using MS.Internal.Xaml.Parser;
    using MS.Internal.Xaml.Runtime;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Windows.Markup;
    using System.Xaml.Context;
    using System.Xaml.MS.Impl;
    using System.Xaml.Schema;

    public class XamlObjectWriter : XamlWriter, IXamlLineInfoConsumer, IAddLineInfo, ICheckIfInitialized
    {
        private EventHandler<XamlObjectEventArgs> _afterBeginInitHandler;
        private EventHandler<XamlObjectEventArgs> _afterEndInitHandler;
        private EventHandler<XamlObjectEventArgs> _afterPropertiesHandler;
        private EventHandler<XamlObjectEventArgs> _beforePropertiesHandler;
        private ObjectWriterContext _context;
        private DeferringWriter _deferringWriter;
        private bool _inDispose;
        private object _lastInstance;
        private int _lineNumber;
        private int _linePosition;
        private MS.Internal.Xaml.Context.NameFixupGraph _nameFixupGraph;
        private bool _nextNodeMustBeEndMember;
        private Dictionary<object, List<PendingCollectionAdd>> _pendingCollectionAdds;
        private Dictionary<object, ObjectWriterContext> _pendingKeyConversionContexts;
        private bool _preferUnconvertedDictionaryKeys;
        private INameScope _rootNamescope;
        private object _rootObjectInstance;
        private bool _skipDuplicatePropertyCheck;
        private bool _skipProvideValueOnRoot;
        private EventHandler<XamlSetValueEventArgs> _xamlSetValueHandler;

        public XamlObjectWriter(XamlSchemaContext schemaContext)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.Initialize(schemaContext, null, null);
        }

        internal XamlObjectWriter(XamlSavedContext savedContext, XamlObjectWriterSettings settings)
        {
            if (savedContext == null)
            {
                throw new ArgumentNullException("savedContext");
            }
            if (savedContext.SchemaContext == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("SavedContextSchemaContextNull"), "savedContext");
            }
            this.Initialize(savedContext.SchemaContext, savedContext, settings);
        }

        public XamlObjectWriter(XamlSchemaContext schemaContext, XamlObjectWriterSettings settings)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.Initialize(schemaContext, null, settings);
        }

        private void AddDependencyForUnresolvedChildren(object childThatHasUnresolvedChildren, XamlMember property, object parentInstance, XamlType parentType, bool parentIsGotten, XamlSavedContext deferredMarkupExtensionContext)
        {
            if (((property != null) && property.IsDirective) && ((parentInstance == null) && (property != XamlLanguage.Key)))
            {
                List<string> result = new List<string>();
                this._nameFixupGraph.GetDependentNames(childThatHasUnresolvedChildren, result);
                string str = string.Join(", ", result.ToArray());
                throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("TransitiveForwardRefDirectives", new object[] { childThatHasUnresolvedChildren.GetType(), property, str })));
            }
            NameFixupToken token = this.GetTokenForUnresolvedChildren(childThatHasUnresolvedChildren, property, deferredMarkupExtensionContext);
            token.Target.Instance = parentInstance;
            token.Target.InstanceType = parentType;
            token.Target.InstanceWasGotten = parentIsGotten;
            this.PendCurrentFixupToken_SetValue(token);
        }

        public void Clear()
        {
            this.ThrowIfDisposed();
            while (this._context.LiveDepth > 0)
            {
                this._context.PopScope();
            }
            this._rootNamescope = null;
            this._nextNodeMustBeEndMember = false;
            this._deferringWriter.Clear();
            this._context.PushScope();
        }

        private void CompleteDeferredInitialization(FixupTarget target)
        {
            this.ExecutePendingAdds(target.InstanceType, target.Instance);
            if (!target.InstanceWasGotten)
            {
                IAddLineInfo lineInfo = this.Runtime.LineInfo;
                this.Runtime.LineInfo = target;
                try
                {
                    this.Runtime.InitializationGuard(target.InstanceType, target.Instance, false);
                }
                finally
                {
                    this.Runtime.LineInfo = lineInfo;
                }
                this.OnAfterEndInit(target.Instance);
            }
        }

        private void CompleteNameReferences()
        {
            if (this._nameFixupGraph != null)
            {
                List<NameFixupToken> unresolvedRefs = null;
                foreach (NameFixupToken token in this._nameFixupGraph.GetRemainingSimpleFixups())
                {
                    object obj2 = token.ResolveName(token.NeededNames[0]);
                    if (obj2 == null)
                    {
                        if (unresolvedRefs == null)
                        {
                            unresolvedRefs = new List<NameFixupToken>();
                        }
                        unresolvedRefs.Add(token);
                    }
                    else if (unresolvedRefs == null)
                    {
                        token.ReferencedObject = obj2;
                        token.NeededNames.RemoveAt(0);
                        this.ProcessNameFixup(token, true);
                        this._nameFixupGraph.AddEndOfParseDependency(token.ReferencedObject, token.Target);
                    }
                }
                if (unresolvedRefs != null)
                {
                    this.ThrowUnresolvedRefs(unresolvedRefs);
                }
                foreach (NameFixupToken token2 in this._nameFixupGraph.GetRemainingReparses())
                {
                    this.ProcessNameFixup(token2, true);
                    this._nameFixupGraph.AddEndOfParseDependency(token2.TargetContext.CurrentInstance, token2.Target);
                }
                foreach (NameFixupToken token3 in this._nameFixupGraph.GetRemainingObjectDependencies())
                {
                    this.ProcessNameFixup(token3, true);
                    if ((token3.Target.Instance != null) && !this._nameFixupGraph.HasUnresolvedChildren(token3.Target.Instance))
                    {
                        this.CompleteDeferredInitialization(token3.Target);
                    }
                }
            }
        }

        private XamlRuntime CreateRuntime(XamlObjectWriterSettings settings, XamlSchemaContext schemaContext)
        {
            XamlRuntime runtime = null;
            XamlRuntimeSettings runtimeSettings = null;
            if (settings != null)
            {
                runtimeSettings = new XamlRuntimeSettings {
                    IgnoreCanConvert = settings.IgnoreCanConvert
                };
                if (settings.AccessLevel != null)
                {
                    runtime = new PartialTrustTolerantRuntime(runtimeSettings, settings.AccessLevel, schemaContext);
                }
            }
            if (runtime == null)
            {
                runtime = new ClrObjectRuntime(runtimeSettings, true);
            }
            runtime.LineInfo = this;
            return runtime;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                this._inDispose = true;
                if (disposing && !base.IsDisposed)
                {
                    if ((this._context.LiveDepth > 1) || (this._context.CurrentType != null))
                    {
                        while (this._context.LiveDepth > 0)
                        {
                            if (this._context.CurrentProperty != null)
                            {
                                this.WriteEndMember();
                            }
                            this.WriteEndObject();
                        }
                    }
                    this._deferringWriter.Close();
                    this._deferringWriter = null;
                    this._context = null;
                    this._afterBeginInitHandler = null;
                    this._beforePropertiesHandler = null;
                    this._afterPropertiesHandler = null;
                    this._afterEndInitHandler = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
                this._inDispose = false;
            }
        }

        private void ExecutePendingAdds(XamlType instanceType, object instance)
        {
            List<PendingCollectionAdd> list;
            if ((this._pendingCollectionAdds != null) && this.PendingCollectionAdds.TryGetValue(instance, out list))
            {
                foreach (PendingCollectionAdd add in list)
                {
                    XamlType type = add.ItemType ?? instanceType.ItemType;
                    IAddLineInfo lineInfo = this.Runtime.LineInfo;
                    this.Runtime.LineInfo = add;
                    try
                    {
                        if (instanceType.IsDictionary)
                        {
                            if (!add.KeyIsSet)
                            {
                                add.Key = this.GetKeyFromInstance(add.Item, type, add);
                                add.KeyIsSet = true;
                            }
                            if (add.KeyIsUnconverted)
                            {
                                ObjectWriterContext ctx = this.PendingKeyConversionContexts[instance];
                                ctx.PopScope();
                                ctx.PushScope();
                                ctx.CurrentType = type;
                                ctx.CurrentInstance = add.Item;
                                ctx.CurrentKeyIsUnconverted = add.KeyIsUnconverted;
                                this.Logic_AddToParentDictionary(ctx, add.Key, add.Item);
                            }
                            else
                            {
                                this.Runtime.AddToDictionary(instance, instanceType, add.Item, type, add.Key);
                            }
                        }
                        else
                        {
                            this.Runtime.Add(instance, instanceType, add.Item, add.ItemType);
                        }
                    }
                    finally
                    {
                        this.Runtime.LineInfo = lineInfo;
                    }
                }
                this.PendingCollectionAdds.Remove(instance);
                if ((this._pendingKeyConversionContexts != null) && this._pendingKeyConversionContexts.ContainsKey(instance))
                {
                    this._pendingKeyConversionContexts.Remove(instance);
                }
            }
        }

        private object GetKeyFromInstance(object instance, XamlType instanceType, IAddLineInfo lineInfo)
        {
            XamlMember aliasedProperty = instanceType.GetAliasedProperty(XamlLanguage.Key);
            if ((aliasedProperty == null) || (instance == null))
            {
                throw lineInfo.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("MissingKey", new object[] { instanceType.Name })));
            }
            return this.Runtime.GetValue(instance, aliasedProperty);
        }

        private NameFixupToken GetTokenForUnresolvedChildren(object childThatHasUnresolvedChildren, XamlMember property, XamlSavedContext deferredMarkupExtensionContext)
        {
            NameFixupToken token = new NameFixupToken();
            if (deferredMarkupExtensionContext != null)
            {
                token.FixupType = FixupType.MarkupExtensionFirstRun;
                token.SavedContext = deferredMarkupExtensionContext;
            }
            else
            {
                token.FixupType = FixupType.UnresolvedChildren;
            }
            token.ReferencedObject = childThatHasUnresolvedChildren;
            token.Target.Property = property;
            return token;
        }

        private XamlType GetXamlType(Type clrType)
        {
            XamlType xamlType = this.SchemaContext.GetXamlType(clrType);
            if (xamlType == null)
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("ObjectWriterTypeNotAllowed", new object[] { this.SchemaContext.GetType(), clrType }));
            }
            return xamlType;
        }

        private bool HasUnresolvedChildren(object parent)
        {
            if (this._nameFixupGraph == null)
            {
                return false;
            }
            return this._nameFixupGraph.HasUnresolvedChildren(parent);
        }

        private void Initialize(XamlSchemaContext schemaContext, XamlSavedContext savedContext, XamlObjectWriterSettings settings)
        {
            this._inDispose = false;
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            if ((savedContext != null) && (schemaContext != savedContext.SchemaContext))
            {
                throw new ArgumentException(System.Xaml.SR.Get("SavedContextSchemaContextMismatch"), "schemaContext");
            }
            if (settings != null)
            {
                this._afterBeginInitHandler = settings.AfterBeginInitHandler;
                this._beforePropertiesHandler = settings.BeforePropertiesHandler;
                this._afterPropertiesHandler = settings.AfterPropertiesHandler;
                this._afterEndInitHandler = settings.AfterEndInitHandler;
                this._xamlSetValueHandler = settings.XamlSetValueHandler;
                this._rootObjectInstance = settings.RootObjectInstance;
                this._skipDuplicatePropertyCheck = settings.SkipDuplicatePropertyCheck;
                this._skipProvideValueOnRoot = settings.SkipProvideValueOnRoot;
                this._preferUnconvertedDictionaryKeys = settings.PreferUnconvertedDictionaryKeys;
            }
            INameScope rootNameScope = (settings != null) ? settings.ExternalNameScope : null;
            XamlRuntime runtime = this.CreateRuntime(settings, schemaContext);
            if (savedContext != null)
            {
                this._context = new ObjectWriterContext(savedContext, settings, rootNameScope, runtime);
            }
            else
            {
                if (schemaContext == null)
                {
                    throw this.WithLineInfo(new XamlInternalException());
                }
                this._context = new ObjectWriterContext(schemaContext, settings, rootNameScope, runtime);
                this._context.AddNamespacePrefix("xml", "http://www.w3.org/XML/1998/namespace");
            }
            this._context.IsInitializedCallback = this;
            this._deferringWriter = new DeferringWriter(this._context);
            this._rootNamescope = null;
        }

        private static bool IsBuiltInGenericDictionary(Type type)
        {
            if ((type == null) || !type.IsGenericType)
            {
                return false;
            }
            Type genericTypeDefinition = type.GetGenericTypeDefinition();
            if ((!(genericTypeDefinition == typeof(Dictionary<,>)) && !(genericTypeDefinition == typeof(SortedDictionary<,>))) && !(genericTypeDefinition == typeof(SortedList<,>)))
            {
                return (genericTypeDefinition == typeof(ConcurrentDictionary<,>));
            }
            return true;
        }

        private bool IsConstructionDirective(XamlMember xamlMember)
        {
            if ((((xamlMember != XamlLanguage.Arguments) && (xamlMember != XamlLanguage.Base)) && ((xamlMember != XamlLanguage.FactoryMethod) && (xamlMember != XamlLanguage.Initialization))) && (xamlMember != XamlLanguage.PositionalParameters))
            {
                return (xamlMember == XamlLanguage.TypeArguments);
            }
            return true;
        }

        private bool IsDirectiveAllowedOnNullInstance(XamlMember xamlMember, XamlType xamlType)
        {
            return ((xamlMember == XamlLanguage.Key) || ((xamlMember == XamlLanguage.Uid) && (null == xamlType.GetAliasedProperty(XamlLanguage.Uid))));
        }

        private bool IsTextConstructionDirective(XamlMember xamlMember)
        {
            if (((xamlMember != XamlLanguage.Arguments) && (xamlMember != XamlLanguage.FactoryMethod)) && (xamlMember != XamlLanguage.PositionalParameters))
            {
                return (xamlMember == XamlLanguage.TypeArguments);
            }
            return true;
        }

        private void Logic_AddToParentDictionary(ObjectWriterContext ctx, object key, object value)
        {
            if (ctx.CurrentKeyIsUnconverted && !ctx.ParentShouldNotConvertChildKeys)
            {
                if (!ctx.ParentShouldConvertChildKeys)
                {
                    try
                    {
                        this.Runtime.AddToDictionary(ctx.ParentCollection, ctx.ParentType, value, ctx.CurrentType, key);
                        ctx.ParentShouldNotConvertChildKeys = true;
                        return;
                    }
                    catch (XamlObjectWriterException exception)
                    {
                        if (!(exception.InnerException is ArgumentException) && !(exception.InnerException is InvalidCastException))
                        {
                            throw;
                        }
                        Debugger.IsLogging();
                    }
                    ctx.ParentShouldConvertChildKeys = true;
                }
                ctx.CurrentProperty = XamlLanguage.Key;
                ctx.PushScope();
                ctx.CurrentInstance = key;
                this.Logic_CreatePropertyValueFromValue(ctx);
                key = ctx.CurrentInstance;
                ctx.PopScope();
                ctx.CurrentProperty = null;
            }
            this.Runtime.AddToDictionary(ctx.ParentCollection, ctx.ParentType, value, ctx.CurrentType, key);
        }

        private void Logic_ApplyCurrentPreconstructionPropertyValues(ObjectWriterContext ctx)
        {
            this.Logic_ApplyCurrentPreconstructionPropertyValues(ctx, false);
        }

        private void Logic_ApplyCurrentPreconstructionPropertyValues(ObjectWriterContext ctx, bool skipDirectives)
        {
            if (ctx.CurrentHasPreconstructionPropertyValuesDictionary)
            {
                Dictionary<XamlMember, object> currentPreconstructionPropertyValues = ctx.CurrentPreconstructionPropertyValues;
                object obj2 = null;
                foreach (XamlMember member in currentPreconstructionPropertyValues.Keys)
                {
                    if (!skipDirectives || !member.IsDirective)
                    {
                        obj2 = currentPreconstructionPropertyValues[member];
                        MarkupExtension me = obj2 as MarkupExtension;
                        if ((me != null) && !member.IsDirective)
                        {
                            this.Logic_PushAndPopAProvideValueStackFrame(ctx, member, me, true);
                        }
                        else
                        {
                            this.Logic_ApplyPropertyValue(ctx, member, obj2, false);
                        }
                    }
                }
            }
        }

        private void Logic_ApplyPropertyValue(ObjectWriterContext ctx, XamlMember prop, object value, bool onParent)
        {
            object inst = onParent ? ctx.ParentInstance : ctx.CurrentInstance;
            if (value is XData)
            {
                XData xData = value as XData;
                if (prop.Type.IsXData)
                {
                    this.Runtime.SetXmlInstance(inst, prop, xData);
                    return;
                }
                value = xData.Text;
            }
            this.SetValue(inst, prop, value);
            if (prop.IsDirective)
            {
                XamlMember aliasedProperty = (onParent ? ctx.ParentType : ctx.CurrentType).GetAliasedProperty(prop as XamlDirective);
                if ((prop != XamlLanguage.Key) && (aliasedProperty != null))
                {
                    this.Logic_DuplicatePropertyCheck(ctx, aliasedProperty, onParent);
                    object obj3 = this.Logic_CreateFromValue(ctx, aliasedProperty.TypeConverter, value, aliasedProperty, aliasedProperty.Name);
                    this.SetValue(inst, aliasedProperty, obj3);
                }
                if (prop == XamlLanguage.Name)
                {
                    if (inst == ctx.CurrentInstance)
                    {
                        this.Logic_RegisterName_OnCurrent(ctx, (string) value);
                    }
                    else
                    {
                        this.Logic_RegisterName_OnParent(ctx, (string) value);
                    }
                }
                else if (prop == XamlLanguage.ConnectionId)
                {
                    this.Logic_SetConnectionId(ctx, (int) value, inst);
                }
                else if (prop == XamlLanguage.Base)
                {
                    this.Logic_CheckBaseUri(ctx, (string) value);
                    ctx.BaseUri = new Uri((string) value);
                    if (ctx.ParentInstance != null)
                    {
                        this.Runtime.SetUriBase(ctx.ParentType, ctx.ParentInstance, ctx.BaseUri);
                    }
                }
            }
        }

        private void Logic_AssignProvidedValue(ObjectWriterContext ctx)
        {
            if (!this.Logic_ProvideValue(ctx) && (ctx.ParentProperty != null))
            {
                this.Logic_DoAssignmentToParentProperty(ctx);
            }
        }

        private void Logic_BeginInit(ObjectWriterContext ctx)
        {
            object currentInstance = ctx.CurrentInstance;
            XamlType currentType = ctx.CurrentType;
            this.Runtime.InitializationGuard(currentType, currentInstance, true);
            if (ctx.BaseUri != null)
            {
                this.Runtime.SetUriBase(currentType, currentInstance, ctx.BaseUri);
            }
            if (currentInstance == ctx.RootInstance)
            {
                this.Logic_SetConnectionId(ctx, 0, currentInstance);
            }
            this.OnAfterBeginInit(currentInstance);
        }

        private void Logic_CheckAssignmentToParentStart(ObjectWriterContext ctx)
        {
            bool flag = (ctx.ParentProperty == XamlLanguage.Items) && ctx.ParentType.IsDictionary;
            if (ctx.CurrentType.IsUsableDuringInitialization && !flag)
            {
                ctx.CurrentWasAssignedAtCreation = true;
                this.Logic_DoAssignmentToParentProperty(ctx);
            }
            else
            {
                ctx.CurrentWasAssignedAtCreation = false;
            }
        }

        private void Logic_CheckBaseUri(ObjectWriterContext ctx, string value)
        {
            if ((ctx.BaseUri != null) || (ctx.Depth > 2))
            {
                throw new XamlObjectWriterException(System.Xaml.SR.Get("CannotSetBaseUri"));
            }
        }

        private void Logic_ConvertPositionalParamsToArgs(ObjectWriterContext ctx)
        {
            XamlType currentType = ctx.CurrentType;
            if (!currentType.IsMarkupExtension)
            {
                throw this.WithLineInfo(new XamlInternalException(System.Xaml.SR.Get("NonMEWithPositionalParameters")));
            }
            List<PositionalParameterDescriptor> currentCollection = (List<PositionalParameterDescriptor>) ctx.CurrentCollection;
            object[] objArray = new object[currentCollection.Count];
            IEnumerable<XamlType> positionalParameters = currentType.GetPositionalParameters(currentCollection.Count);
            if (positionalParameters == null)
            {
                string message = string.Format(TypeConverterHelper.InvariantEnglishUS, System.Xaml.SR.Get("NoSuchConstructor"), new object[] { currentCollection.Count, currentType.Name });
                throw this.WithLineInfo(new XamlObjectWriterException(message));
            }
            int num = 0;
            foreach (XamlType type2 in positionalParameters)
            {
                object obj2;
                if (num >= currentCollection.Count)
                {
                    throw this.WithLineInfo(new XamlInternalException(System.Xaml.SR.Get("PositionalParamsWrongLength")));
                }
                PositionalParameterDescriptor descriptor = currentCollection[num];
                if (descriptor.WasText)
                {
                    XamlValueConverter<TypeConverter> typeConverter = type2.TypeConverter;
                    object obj3 = descriptor.Value;
                    obj2 = this.Logic_CreateFromValue(ctx, typeConverter, obj3, null, type2.Name);
                }
                else
                {
                    obj2 = currentCollection[num].Value;
                }
                objArray[num++] = obj2;
                ctx.CurrentCtorArgs = objArray;
            }
        }

        private void Logic_CreateAndAssignToParentStart(ObjectWriterContext ctx)
        {
            object obj2;
            object obj3;
            XamlType currentType = ctx.CurrentType;
            if (ctx.CurrentIsObjectFromMember)
            {
                throw this.WithLineInfo(new XamlInternalException(System.Xaml.SR.Get("ConstructImplicitType")));
            }
            if (currentType.IsMarkupExtension && (ctx.CurrentCtorArgs != null))
            {
                object[] currentCtorArgs = ctx.CurrentCtorArgs;
                for (int i = 0; i < currentCtorArgs.Length; i++)
                {
                    MarkupExtension me = currentCtorArgs[i] as MarkupExtension;
                    if (me != null)
                    {
                        currentCtorArgs[i] = this.Logic_PushAndPopAProvideValueStackFrame(ctx, XamlLanguage.PositionalParameters, me, false);
                    }
                }
            }
            if (!ctx.CurrentHasPreconstructionPropertyValuesDictionary || !ctx.CurrentPreconstructionPropertyValues.TryGetValue(XamlLanguage.FactoryMethod, out obj3))
            {
                obj2 = this.Runtime.CreateInstance(currentType, ctx.CurrentCtorArgs);
            }
            else
            {
                XamlType type2;
                XamlPropertyName name = XamlPropertyName.Parse((string) obj3);
                if (name == null)
                {
                    string message = string.Format(TypeConverterHelper.InvariantEnglishUS, System.Xaml.SR.Get("InvalidExpression"), new object[] { obj3 });
                    throw this.WithLineInfo(new XamlInternalException(message));
                }
                if (name.Owner == null)
                {
                    type2 = currentType;
                }
                else
                {
                    type2 = ctx.GetXamlType(name.Owner);
                    if (type2 == null)
                    {
                        XamlTypeName xamlTypeName = ctx.GetXamlTypeName(name.Owner);
                        throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("CannotResolveTypeForFactoryMethod", new object[] { xamlTypeName, name.Name })));
                    }
                }
                obj2 = this.Runtime.CreateWithFactoryMethod(type2, name.Name, ctx.CurrentCtorArgs);
                XamlType xamlType = this.GetXamlType(obj2.GetType());
                if (!xamlType.CanAssignTo(currentType))
                {
                    throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("NotAssignableFrom", new object[] { currentType.GetQualifiedName(), xamlType.GetQualifiedName() })));
                }
            }
            ctx.CurrentCtorArgs = null;
            ctx.CurrentInstance = obj2;
            if (currentType.IsCollection || currentType.IsDictionary)
            {
                ctx.CurrentCollection = obj2;
            }
            this.Logic_BeginInit(ctx);
            if (((ctx.LiveDepth > 1) && !(obj2 is MarkupExtension)) && (ctx.LiveDepth > 1))
            {
                this.Logic_CheckAssignmentToParentStart(ctx);
            }
            this.OnBeforeProperties(obj2);
            this.Logic_ApplyCurrentPreconstructionPropertyValues(ctx);
        }

        private void Logic_CreateFromInitializationValue(ObjectWriterContext ctx)
        {
            XamlType parentType = ctx.ParentType;
            XamlValueConverter<TypeConverter> typeConverter = parentType.TypeConverter;
            object currentInstance = ctx.CurrentInstance;
            object obj3 = null;
            if (parentType.IsUnknown)
            {
                string message = System.Xaml.SR.Get("CantCreateUnknownType", new object[] { parentType.GetQualifiedName() });
                throw this.WithLineInfo(new XamlObjectWriterException(message));
            }
            if (typeConverter == null)
            {
                throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("InitializationSyntaxWithoutTypeConverter", new object[] { parentType.GetQualifiedName() })));
            }
            obj3 = this.Logic_CreateFromValue(ctx, typeConverter, currentInstance, null, parentType.Name);
            ctx.PopScope();
            ctx.CurrentInstance = obj3;
            ctx.CurrentIsTypeConvertedObject = true;
            if (!(obj3 is NameFixupToken))
            {
                if (parentType.IsCollection || parentType.IsDictionary)
                {
                    ctx.CurrentCollection = obj3;
                }
                this.Logic_ApplyCurrentPreconstructionPropertyValues(ctx, true);
            }
        }

        private object Logic_CreateFromValue(ObjectWriterContext ctx, XamlValueConverter<TypeConverter> typeConverter, object value, XamlMember property, string targetName)
        {
            return this.Logic_CreateFromValue(ctx, typeConverter, value, property, targetName, this);
        }

        private object Logic_CreateFromValue(ObjectWriterContext ctx, XamlValueConverter<TypeConverter> typeConverter, object value, XamlMember property, string targetName, IAddLineInfo lineInfo)
        {
            object obj3;
            try
            {
                obj3 = this.Runtime.CreateFromValue(ctx.ServiceProviderContext, typeConverter, value, property);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                string message = System.Xaml.SR.Get("TypeConverterFailed", new object[] { targetName, value });
                throw lineInfo.WithLineInfo(new XamlObjectWriterException(message, exception));
            }
            return obj3;
        }

        private bool Logic_CreatePropertyValueFromValue(ObjectWriterContext ctx)
        {
            XamlMember parentProperty = ctx.ParentProperty;
            XamlType type1 = parentProperty.Type;
            object currentInstance = ctx.CurrentInstance;
            XamlReader deferredContent = currentInstance as XamlReader;
            if (deferredContent != null)
            {
                XamlValueConverter<XamlDeferringLoader> deferringLoader = parentProperty.DeferringLoader;
                if (deferringLoader != null)
                {
                    ctx.CurrentInstance = this.Runtime.DeferredLoad(ctx.ServiceProviderContext, deferringLoader, deferredContent);
                    return true;
                }
            }
            XamlValueConverter<TypeConverter> typeConverter = parentProperty.TypeConverter;
            object obj3 = null;
            XamlType declaringType = null;
            if (parentProperty.IsAttachable)
            {
                declaringType = parentProperty.DeclaringType;
            }
            else
            {
                declaringType = ctx.ParentType;
            }
            if (((parentProperty != null) && !parentProperty.IsUnknown) && (declaringType != null))
            {
                XamlType grandParentType = ctx.GrandParentType;
                if ((parentProperty.IsDirective && (parentProperty == XamlLanguage.Key)) && ((grandParentType != null) && grandParentType.IsDictionary))
                {
                    typeConverter = grandParentType.KeyType.TypeConverter;
                }
                if (((typeConverter != null) && (typeConverter.ConverterType != null)) && (typeConverter != BuiltInValueConverter.String))
                {
                    TypeConverter converterInstance = this.Runtime.GetConverterInstance<TypeConverter>(typeConverter);
                    if ((converterInstance != null) && (declaringType.SetTypeConverterHandler != null))
                    {
                        XamlSetTypeConverterEventArgs e = new XamlSetTypeConverterEventArgs(parentProperty, converterInstance, currentInstance, ctx.ServiceProviderContext, TypeConverterHelper.InvariantEnglishUS, ctx.ParentInstance) {
                            CurrentType = declaringType
                        };
                        declaringType.SetTypeConverterHandler(ctx.ParentInstance, e);
                        if (e.Handled)
                        {
                            return false;
                        }
                    }
                }
            }
            if (typeConverter != null)
            {
                obj3 = this.Logic_CreateFromValue(ctx, typeConverter, currentInstance, parentProperty, parentProperty.Name);
            }
            else
            {
                obj3 = currentInstance;
            }
            ctx.CurrentInstance = obj3;
            return true;
        }

        private void Logic_DeferProvideValue(ObjectWriterContext ctx)
        {
            XamlSavedContext savedContext = ctx.GetSavedContext(SavedContextType.ReparseMarkupExtension);
            if (((ctx.LiveDepth > 2) && (ctx.ParentProperty == XamlLanguage.Key)) && ctx.GrandParentType.IsDictionary)
            {
                NameFixupToken token = this.GetTokenForUnresolvedChildren(ctx.CurrentInstance, XamlLanguage.Key, savedContext);
                this.Logic_PendKeyFixupToken(ctx, token);
            }
            else
            {
                this.AddDependencyForUnresolvedChildren(ctx.CurrentInstance, ctx.ParentProperty, ctx.ParentInstance, ctx.ParentType, ctx.ParentIsObjectFromMember, savedContext);
            }
        }

        private void Logic_DoAssignmentToParentCollection(ObjectWriterContext ctx)
        {
            object parentCollection = ctx.ParentCollection;
            XamlType parentType = ctx.ParentType;
            XamlType currentType = ctx.CurrentType;
            object currentInstance = ctx.CurrentInstance;
            if (!parentType.IsDictionary)
            {
                if (!this.Logic_PendAssignmentToParentCollection(ctx, null, false))
                {
                    MarkupExtension me = currentInstance as MarkupExtension;
                    if ((me != null) && !this.Logic_WillParentCollectionAdd(ctx, currentInstance.GetType(), true))
                    {
                        currentInstance = this.Runtime.CallProvideValue(me, ctx.ServiceProviderContext);
                    }
                    this.Runtime.Add(parentCollection, parentType, currentInstance, currentType);
                }
            }
            else
            {
                if (currentType == null)
                {
                    currentType = (currentInstance == null) ? parentType.ItemType : this.GetXamlType(currentInstance.GetType());
                }
                object currentKey = ctx.CurrentKey;
                bool currentIsKeySet = ctx.CurrentIsKeySet;
                if (!this.Logic_PendAssignmentToParentCollection(ctx, currentKey, currentIsKeySet))
                {
                    if (!currentIsKeySet)
                    {
                        currentKey = this.GetKeyFromInstance(currentInstance, currentType, this);
                    }
                    this.Logic_AddToParentDictionary(ctx, currentKey, currentInstance);
                }
            }
        }

        private void Logic_DoAssignmentToParentProperty(ObjectWriterContext ctx)
        {
            XamlMember parentProperty = ctx.ParentProperty;
            object currentInstance = ctx.CurrentInstance;
            XamlType type = parentProperty.Type;
            if (parentProperty.IsDirective && (type.IsCollection || type.IsDictionary))
            {
                if ((currentInstance is NameFixupToken) && (parentProperty != XamlLanguage.Items))
                {
                    NameFixupToken token = currentInstance as NameFixupToken;
                    string str = string.Join(",", token.NeededNames.ToArray());
                    string message = System.Xaml.SR.Get("ForwardRefDirectives", new object[] { str });
                    throw this.WithLineInfo(new XamlObjectWriterException(message));
                }
                if (parentProperty == XamlLanguage.PositionalParameters)
                {
                    ctx.CurrentType = XamlLanguage.PositionalParameterDescriptor;
                    ctx.CurrentInstance = new PositionalParameterDescriptor(currentInstance, false);
                }
                this.Logic_DoAssignmentToParentCollection(ctx);
            }
            else if (ctx.ParentInstance != null)
            {
                if (ctx.ParentIsPropertyValueSet)
                {
                    throw this.WithLineInfo(new XamlDuplicateMemberException(ctx.ParentProperty, ctx.ParentType));
                }
                ctx.ParentIsPropertyValueSet = true;
                if (currentInstance is NameFixupToken)
                {
                    NameFixupToken token2 = (NameFixupToken) currentInstance;
                    if (parentProperty.IsDirective)
                    {
                        if (parentProperty != XamlLanguage.Key)
                        {
                            string str3 = string.Join(",", token2.NeededNames.ToArray());
                            string str4 = System.Xaml.SR.Get("ForwardRefDirectives", new object[] { str3 });
                            throw this.WithLineInfo(new XamlObjectWriterException(str4));
                        }
                        this.Logic_PendKeyFixupToken(ctx, token2);
                    }
                    else
                    {
                        this.PendCurrentFixupToken_SetValue(token2);
                    }
                }
                else
                {
                    XamlType parentType = ctx.ParentType;
                    if (!ctx.CurrentIsObjectFromMember)
                    {
                        this.Logic_ApplyPropertyValue(ctx, parentProperty, currentInstance, true);
                        if (parentProperty == parentType.GetAliasedProperty(XamlLanguage.Name))
                        {
                            this.Logic_RegisterName_OnParent(ctx, (string) currentInstance);
                        }
                        if (parentProperty == XamlLanguage.Key)
                        {
                            ctx.ParentKey = currentInstance;
                        }
                    }
                }
            }
            else
            {
                if (!parentProperty.IsDirective)
                {
                    throw new XamlInternalException(System.Xaml.SR.Get("BadStateObjectWriter"));
                }
                if (parentProperty == XamlLanguage.Base)
                {
                    this.Logic_CheckBaseUri(ctx, (string) currentInstance);
                    ctx.BaseUri = new Uri((string) currentInstance);
                }
                else if (currentInstance is NameFixupToken)
                {
                    if (parentProperty != XamlLanguage.Key)
                    {
                        NameFixupToken token3 = (NameFixupToken) currentInstance;
                        string str5 = string.Join(",", token3.NeededNames.ToArray());
                        string str6 = System.Xaml.SR.Get("ForwardRefDirectives", new object[] { str5 });
                        throw this.WithLineInfo(new XamlObjectWriterException(str6));
                    }
                    this.Logic_PendKeyFixupToken(ctx, (NameFixupToken) currentInstance);
                }
                else if (parentProperty == XamlLanguage.Key)
                {
                    ctx.ParentKey = currentInstance;
                }
                else
                {
                    ctx.ParentPreconstructionPropertyValues.Add(parentProperty, currentInstance);
                }
            }
        }

        private void Logic_DuplicatePropertyCheck(ObjectWriterContext ctx, XamlMember property, bool onParent)
        {
            if (!this._skipDuplicatePropertyCheck)
            {
                System.Xaml.Context.HashSet<XamlMember> set = onParent ? ctx.ParentAssignedProperties : ctx.CurrentAssignedProperties;
                if (set.ContainsKey(property))
                {
                    if (property != XamlLanguage.Space)
                    {
                        XamlType type = onParent ? ctx.ParentType : ctx.CurrentType;
                        throw this.WithLineInfo(new XamlDuplicateMemberException(property, type));
                    }
                }
                else
                {
                    set.Add(property);
                }
            }
        }

        private void Logic_EndInit(ObjectWriterContext ctx)
        {
            XamlType currentType = ctx.CurrentType;
            object currentInstance = ctx.CurrentInstance;
            this.Runtime.InitializationGuard(currentType, currentInstance, false);
            this.OnAfterEndInit(currentInstance);
        }

        private bool Logic_PendAssignmentToParentCollection(ObjectWriterContext ctx, object key, bool keyIsSet)
        {
            object parentCollection = ctx.ParentCollection;
            object currentInstance = ctx.CurrentInstance;
            NameFixupToken token = key as NameFixupToken;
            NameFixupToken token2 = currentInstance as NameFixupToken;
            List<PendingCollectionAdd> list = null;
            if (this._pendingCollectionAdds != null)
            {
                this.PendingCollectionAdds.TryGetValue(parentCollection, out list);
            }
            if ((list == null) && (((token != null) || (token2 != null)) || (this.HasUnresolvedChildren(key) || this.HasUnresolvedChildren(currentInstance))))
            {
                list = new List<PendingCollectionAdd>();
                this.PendingCollectionAdds.Add(parentCollection, list);
            }
            if (token != null)
            {
                token.Target.KeyHolder = null;
                token.Target.TemporaryCollectionIndex = list.Count;
            }
            if (token2 != null)
            {
                this.PendCurrentFixupToken_SetValue(token2);
                token2.Target.TemporaryCollectionIndex = list.Count;
            }
            if (list == null)
            {
                return false;
            }
            PendingCollectionAdd item = new PendingCollectionAdd {
                Key = key,
                KeyIsSet = keyIsSet,
                KeyIsUnconverted = ctx.CurrentKeyIsUnconverted,
                Item = currentInstance,
                ItemType = ctx.CurrentType,
                LineNumber = this._lineNumber,
                LinePosition = this._linePosition
            };
            list.Add(item);
            if (item.KeyIsUnconverted && !this.PendingKeyConversionContexts.ContainsKey(parentCollection))
            {
                XamlSavedContext savedContext = ctx.GetSavedContext(SavedContextType.ReparseMarkupExtension);
                this.PendingKeyConversionContexts.Add(parentCollection, new ObjectWriterContext(savedContext, null, null, this.Runtime));
            }
            return true;
        }

        private void Logic_PendKeyFixupToken(ObjectWriterContext ctx, NameFixupToken token)
        {
            token.Target.Instance = ctx.GrandParentInstance;
            token.Target.InstanceType = ctx.GrandParentType;
            token.Target.InstanceWasGotten = ctx.GrandParentIsObjectFromMember;
            FixupTargetKeyHolder holder = new FixupTargetKeyHolder(token);
            token.Target.KeyHolder = holder;
            ctx.ParentKey = holder;
            if (token.Target.Instance != null)
            {
                this.PendCurrentFixupToken_SetValue(token);
            }
        }

        private bool Logic_ProvideValue(ObjectWriterContext ctx)
        {
            MarkupExtension currentInstance = (MarkupExtension) ctx.CurrentInstance;
            object parentInstance = ctx.ParentInstance;
            XamlMember parentProperty = ctx.ParentProperty;
            if ((parentProperty != null) && !parentProperty.IsUnknown)
            {
                XamlType declaringType = null;
                if (parentProperty.IsAttachable)
                {
                    declaringType = parentProperty.DeclaringType;
                }
                else
                {
                    declaringType = ctx.ParentType;
                }
                if ((declaringType != null) && (declaringType.SetMarkupExtensionHandler != null))
                {
                    XamlSetMarkupExtensionEventArgs e = new XamlSetMarkupExtensionEventArgs(parentProperty, currentInstance, ctx.ServiceProviderContext, parentInstance) {
                        CurrentType = declaringType
                    };
                    declaringType.SetMarkupExtensionHandler(parentInstance, e);
                    if (e.Handled)
                    {
                        return true;
                    }
                }
            }
            object obj4 = currentInstance;
            if ((ctx.LiveDepth != 1) || !this._skipProvideValueOnRoot)
            {
                obj4 = this.Runtime.CallProvideValue(currentInstance, ctx.ServiceProviderContext);
            }
            if (ctx.ParentProperty != null)
            {
                if (obj4 != null)
                {
                    if (!(obj4 is NameFixupToken))
                    {
                        ctx.CurrentType = this.GetXamlType(obj4.GetType());
                    }
                }
                else if (ctx.ParentProperty == XamlLanguage.Items)
                {
                    ctx.CurrentType = ctx.ParentType.ItemType;
                }
                else
                {
                    ctx.CurrentType = ctx.ParentProperty.Type;
                }
                ctx.CurrentInstance = obj4;
            }
            else
            {
                ctx.CurrentInstance = obj4;
            }
            return false;
        }

        private object Logic_PushAndPopAProvideValueStackFrame(ObjectWriterContext ctx, XamlMember prop, MarkupExtension me, bool useIRME)
        {
            XamlMember currentProperty = ctx.CurrentProperty;
            ctx.CurrentProperty = prop;
            ctx.PushScope();
            ctx.CurrentInstance = me;
            object obj2 = null;
            if (useIRME)
            {
                this.Logic_AssignProvidedValue(ctx);
            }
            else
            {
                obj2 = this.Runtime.CallProvideValue(me, ctx.ServiceProviderContext);
            }
            ctx.PopScope();
            ctx.CurrentProperty = currentProperty;
            return obj2;
        }

        private void Logic_RegisterName_OnCurrent(ObjectWriterContext ctx, string name)
        {
            bool isRoot = ctx.LiveDepth == 1;
            this.RegisterName(ctx, name, ctx.CurrentInstance, ctx.CurrentType, ctx.CurrentNameScope, ctx.ParentNameScope, isRoot);
            ctx.CurrentInstanceRegisteredName = name;
        }

        private void Logic_RegisterName_OnParent(ObjectWriterContext ctx, string name)
        {
            this.RegisterName(ctx, name, ctx.ParentInstance, ctx.ParentType, ctx.ParentNameScope, ctx.GrandParentNameScope, false);
            ctx.ParentInstanceRegisteredName = name;
        }

        private void Logic_SetConnectionId(ObjectWriterContext ctx, int connectionId, object instance)
        {
            object rootInstance = ctx.RootInstance;
            this.Runtime.SetConnectionId(rootInstance, connectionId, instance);
        }

        private bool Logic_ShouldConvertKey(ObjectWriterContext ctx)
        {
            if (this._preferUnconvertedDictionaryKeys && !ctx.GrandParentShouldConvertChildKeys)
            {
                if (ctx.GrandParentShouldNotConvertChildKeys)
                {
                    return false;
                }
                XamlType grandParentType = ctx.GrandParentType;
                if (((grandParentType != null) && grandParentType.IsDictionary) && (typeof(IDictionary).IsAssignableFrom(grandParentType.UnderlyingType) && !IsBuiltInGenericDictionary(grandParentType.UnderlyingType)))
                {
                    return false;
                }
                ctx.GrandParentShouldConvertChildKeys = true;
            }
            return true;
        }

        private void Logic_ValidateXClass(ObjectWriterContext ctx, object value)
        {
            if (ctx.Depth > 1)
            {
                throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("DirectiveNotAtRoot", new object[] { XamlLanguage.Class })));
            }
            string str = value as string;
            if (str == null)
            {
                throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("DirectiveMustBeString", new object[] { XamlLanguage.Class })));
            }
            object currentInstance = ctx.CurrentInstance;
            Type type = (currentInstance != null) ? currentInstance.GetType() : ctx.CurrentType.UnderlyingType;
            if (type.FullName != str)
            {
                string rootNamespace = this.SchemaContext.GetRootNamespace(type.Assembly);
                if (!string.IsNullOrEmpty(rootNamespace))
                {
                    str = rootNamespace + "." + str;
                }
                if (type.FullName != str)
                {
                    throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("XClassMustMatchRootInstance", new object[] { str, type.FullName })));
                }
            }
        }

        private bool Logic_WillParentCollectionAdd(ObjectWriterContext ctx, Type type, bool excludeObjectType)
        {
            XamlType itemType = ctx.ParentType.ItemType;
            if (excludeObjectType && (itemType == XamlLanguage.Object))
            {
                return false;
            }
            return itemType.UnderlyingType.IsAssignableFrom(type);
        }

        bool ICheckIfInitialized.IsFullyInitialized(object instance)
        {
            if (instance != null)
            {
                if (this._context.LiveDepth > 0)
                {
                    if (this._context.IsOnTheLiveStack(instance))
                    {
                        return false;
                    }
                    if (this._nameFixupGraph != null)
                    {
                        return !this._nameFixupGraph.HasUnresolvedOrPendingChildren(instance);
                    }
                    return true;
                }
                if (this._nameFixupGraph != null)
                {
                    return !this._nameFixupGraph.WasUninitializedAtEndOfParse(instance);
                }
            }
            return true;
        }

        XamlException IAddLineInfo.WithLineInfo(XamlException ex)
        {
            return this.WithLineInfo(ex);
        }

        protected virtual void OnAfterBeginInit(object value)
        {
            if (this._afterBeginInitHandler != null)
            {
                this._afterBeginInitHandler(this, new XamlObjectEventArgs(value));
            }
        }

        protected virtual void OnAfterEndInit(object value)
        {
            if (this._afterEndInitHandler != null)
            {
                this._afterEndInitHandler(this, new XamlObjectEventArgs(value));
            }
        }

        protected virtual void OnAfterProperties(object value)
        {
            if (this._afterPropertiesHandler != null)
            {
                this._afterPropertiesHandler(this, new XamlObjectEventArgs(value));
            }
        }

        protected virtual void OnBeforeProperties(object value)
        {
            if (this._beforePropertiesHandler != null)
            {
                this._beforePropertiesHandler(this, new XamlObjectEventArgs(value));
            }
        }

        protected virtual bool OnSetValue(object eventSender, XamlMember member, object value)
        {
            if (this._xamlSetValueHandler != null)
            {
                XamlSetValueEventArgs e = new XamlSetValueEventArgs(member, value);
                this._xamlSetValueHandler(eventSender, e);
                return e.Handled;
            }
            return false;
        }

        private void PendCurrentFixupToken_SetValue(NameFixupToken token)
        {
            token.LineNumber = this._lineNumber;
            token.LinePosition = this._linePosition;
            token.Runtime = this.Runtime;
            this.NameFixupGraph.AddDependency(token);
        }

        private void ProcessNameFixup(NameFixupToken token, bool nameResolutionIsComplete)
        {
            IAddLineInfo lineInfo = this.Runtime.LineInfo;
            try
            {
                this.Runtime.LineInfo = token;
                if (token.CanAssignDirectly)
                {
                    this.ProcessNameFixup_Simple(token);
                }
                else if (token.FixupType != FixupType.UnresolvedChildren)
                {
                    this.ProcessNameFixup_Reparse(token, nameResolutionIsComplete);
                }
            }
            finally
            {
                this.Runtime.LineInfo = lineInfo;
            }
        }

        private void ProcessNameFixup_Reparse(NameFixupToken token, bool nameResolutionIsComplete)
        {
            object obj2 = null;
            ObjectWriterContext targetContext = token.TargetContext;
            targetContext.NameResolutionComplete = nameResolutionIsComplete;
            targetContext.IsInitializedCallback = this;
            switch (token.FixupType)
            {
                case FixupType.MarkupExtensionFirstRun:
                    if (!this.Logic_ProvideValue(targetContext))
                    {
                        break;
                    }
                    return;

                case FixupType.MarkupExtensionRerun:
                    obj2 = this.Runtime.CallProvideValue((MarkupExtension) targetContext.CurrentInstance, targetContext.ServiceProviderContext);
                    targetContext.CurrentInstance = obj2;
                    break;

                case FixupType.PropertyValue:
                    obj2 = this.Logic_CreateFromValue(targetContext, targetContext.ParentProperty.TypeConverter, targetContext.CurrentInstance, targetContext.ParentProperty, targetContext.ParentProperty.Name, token);
                    token.TargetContext.CurrentInstance = obj2;
                    break;

                case FixupType.ObjectInitializationValue:
                    this.Logic_CreateFromInitializationValue(targetContext);
                    if (token.TargetContext.CurrentInstanceRegisteredName != null)
                    {
                        this.Logic_RegisterName_OnCurrent(token.TargetContext, token.TargetContext.CurrentInstanceRegisteredName);
                    }
                    break;
            }
            if (token.Target.Property == XamlLanguage.Key)
            {
                this.ProcessNameFixup_UpdatePendingAddKey(token, targetContext.CurrentInstance);
            }
            else if (token.Target.Property == XamlLanguage.Items)
            {
                this.ProcessNameFixup_UpdatePendingAddItem(token, targetContext.CurrentInstance);
            }
            else if (token.Target.Property != null)
            {
                this.Logic_DoAssignmentToParentProperty(targetContext);
            }
            else
            {
                this._lastInstance = targetContext.CurrentInstance;
            }
            NameFixupToken currentInstance = targetContext.CurrentInstance as NameFixupToken;
            if (currentInstance != null)
            {
                currentInstance.Target = token.Target;
                currentInstance.LineNumber = token.LineNumber;
                currentInstance.LinePosition = token.LinePosition;
                if ((token.Target.Property == XamlLanguage.Key) || (token.Target.Property == XamlLanguage.Items))
                {
                    this._nameFixupGraph.AddDependency(currentInstance);
                }
            }
        }

        private void ProcessNameFixup_Simple(NameFixupToken token)
        {
            object referencedObject = token.ReferencedObject;
            if (token.Target.Property == XamlLanguage.Key)
            {
                this.ProcessNameFixup_UpdatePendingAddKey(token, referencedObject);
            }
            else if (token.Target.Property == XamlLanguage.Items)
            {
                this.ProcessNameFixup_UpdatePendingAddItem(token, referencedObject);
            }
            else
            {
                this.SetValue(token.Target.Instance, token.Target.Property, referencedObject);
            }
        }

        private void ProcessNameFixup_UpdatePendingAddItem(NameFixupToken token, object item)
        {
            List<PendingCollectionAdd> list = this.PendingCollectionAdds[token.Target.Instance];
            PendingCollectionAdd add = list[token.Target.TemporaryCollectionIndex];
            add.Item = item;
            if (!(item is NameFixupToken))
            {
                add.ItemType = (item != null) ? this.GetXamlType(item.GetType()) : null;
            }
        }

        private void ProcessNameFixup_UpdatePendingAddKey(NameFixupToken token, object key)
        {
            if (token.Target.KeyHolder != null)
            {
                token.Target.KeyHolder.Key = key;
            }
            else if (token.Target.TemporaryCollectionIndex >= 0)
            {
                List<PendingCollectionAdd> list = this.PendingCollectionAdds[token.Target.Instance];
                PendingCollectionAdd add = list[token.Target.TemporaryCollectionIndex];
                add.Key = key;
                add.KeyIsSet = true;
            }
        }

        private void RegisterName(ObjectWriterContext ctx, string name, object inst, XamlType xamlType, INameScope nameScope, INameScope parentNameScope, bool isRoot)
        {
            INameScope objA = nameScope;
            NameScopeDictionary dictionary = nameScope as NameScopeDictionary;
            if (dictionary != null)
            {
                objA = dictionary.UnderlyingNameScope;
            }
            if (object.ReferenceEquals(objA, inst) && !isRoot)
            {
                nameScope = parentNameScope;
            }
            if (!(inst is NameFixupToken))
            {
                try
                {
                    nameScope.RegisterName(name, inst);
                }
                catch (Exception exception)
                {
                    if (CriticalExceptions.IsCriticalException(exception))
                    {
                        throw;
                    }
                    throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("NameScopeException", new object[] { exception.Message }), exception));
                }
            }
        }

        public void SetLineInfo(int lineNumber, int linePosition)
        {
            this.ThrowIfDisposed();
            this._lineNumber = lineNumber;
            this._linePosition = linePosition;
        }

        private void SetValue(object inst, XamlMember property, object value)
        {
            if (property.IsDirective || !this.OnSetValue(inst, property, value))
            {
                this.Runtime.SetValue(inst, property, value);
            }
        }

        private void ThrowIfDisposed()
        {
            if (base.IsDisposed)
            {
                throw new ObjectDisposedException("XamlObjectWriter");
            }
        }

        private void ThrowUnresolvedRefs(IEnumerable<NameFixupToken> unresolvedRefs)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (NameFixupToken token in unresolvedRefs)
            {
                if (!flag)
                {
                    builder.AppendLine();
                }
                builder.Append(System.Xaml.SR.Get("UnresolvedForwardReferences", new object[] { token.NeededNames[0] }));
                if (token.LineNumber != 0)
                {
                    if (token.LinePosition != 0)
                    {
                        builder.Append(System.Xaml.SR.Get("LineNumberAndPosition", new object[] { string.Empty, token.LineNumber, token.LinePosition }));
                    }
                    builder.Append(System.Xaml.SR.Get("LineNumberOnly", new object[] { string.Empty, token.LineNumber }));
                }
                flag = false;
            }
            throw new XamlObjectWriterException(builder.ToString());
        }

        private void TriggerNameResolution(object instance, string name)
        {
            this._nameFixupGraph.ResolveDependenciesTo(instance, name);
            while (this._nameFixupGraph.HasResolvedTokensPendingProcessing)
            {
                NameFixupToken nextResolvedTokenPendingProcessing = this._nameFixupGraph.GetNextResolvedTokenPendingProcessing();
                this.ProcessNameFixup(nextResolvedTokenPendingProcessing, false);
                if (((nextResolvedTokenPendingProcessing.FixupType == FixupType.ObjectInitializationValue) && !nextResolvedTokenPendingProcessing.CanAssignDirectly) && ((nextResolvedTokenPendingProcessing.TargetContext.CurrentInstanceRegisteredName != null) && !this._context.IsOnTheLiveStack(nextResolvedTokenPendingProcessing.TargetContext.CurrentInstance)))
                {
                    string currentInstanceRegisteredName = nextResolvedTokenPendingProcessing.TargetContext.CurrentInstanceRegisteredName;
                    object currentInstance = nextResolvedTokenPendingProcessing.TargetContext.CurrentInstance;
                    this._nameFixupGraph.ResolveDependenciesTo(currentInstance, currentInstanceRegisteredName);
                }
                if (!nextResolvedTokenPendingProcessing.Target.InstanceIsOnTheStack && !this._nameFixupGraph.HasUnresolvedOrPendingChildren(nextResolvedTokenPendingProcessing.Target.Instance))
                {
                    this.CompleteDeferredInitialization(nextResolvedTokenPendingProcessing.Target);
                    object obj3 = nextResolvedTokenPendingProcessing.Target.Instance;
                    string instanceName = nextResolvedTokenPendingProcessing.Target.InstanceName;
                    this._nameFixupGraph.ResolveDependenciesTo(obj3, instanceName);
                }
            }
        }

        private void TryCreateParentInstance(ObjectWriterContext ctx)
        {
            if ((ctx.ParentInstance == null) && (ctx.ParentProperty != XamlLanguage.Arguments))
            {
                ctx.LiftScope();
                this.Logic_CreateAndAssignToParentStart(ctx);
                ctx.UnLiftScope();
            }
        }

        private XamlException WithLineInfo(XamlException ex)
        {
            ex.SetLineInfo(this._lineNumber, this._linePosition);
            return ex;
        }

        public override void WriteEndMember()
        {
            this.ThrowIfDisposed();
            this._deferringWriter.WriteEndMember();
            if (!this._deferringWriter.Handled)
            {
                XamlMember parentProperty;
                if (this._context.CurrentType == null)
                {
                    parentProperty = this._context.ParentProperty;
                }
                else
                {
                    parentProperty = this._context.CurrentProperty;
                }
                if (parentProperty == null)
                {
                    string message = (this._context.CurrentType != null) ? System.Xaml.SR.Get("NoPropertyInCurrentFrame_EM", new object[] { this._context.CurrentType.ToString() }) : System.Xaml.SR.Get("NoPropertyInCurrentFrame_EM_noType");
                    throw this.WithLineInfo(new XamlObjectWriterException(message));
                }
                this._nextNodeMustBeEndMember = false;
                this._lastInstance = null;
                if (parentProperty == XamlLanguage.Arguments)
                {
                    this._context.CurrentCtorArgs = ((List<object>) this._context.CurrentCollection).ToArray();
                }
                else if (parentProperty == XamlLanguage.Initialization)
                {
                    this.Logic_CreateFromInitializationValue(this._context);
                }
                else if (parentProperty == XamlLanguage.Items)
                {
                    this._context.CurrentCollection = null;
                }
                else if (parentProperty == XamlLanguage.PositionalParameters)
                {
                    this.Logic_ConvertPositionalParamsToArgs(this._context);
                }
                else if (parentProperty == XamlLanguage.Class)
                {
                    object currentInstance = null;
                    if (this._context.CurrentType == null)
                    {
                        currentInstance = this._context.CurrentInstance;
                        this._context.PopScope();
                    }
                    this.Logic_ValidateXClass(this._context, currentInstance);
                }
                else if (this._context.CurrentType == null)
                {
                    object obj3 = this._context.CurrentInstance;
                    bool flag = true;
                    if (obj3 != null)
                    {
                        MarkupExtension extension = obj3 as MarkupExtension;
                        if (extension != null)
                        {
                            this._context.CurrentInstance = extension;
                            XamlType xamlType = this.GetXamlType(obj3.GetType());
                            if (!parentProperty.Type.IsMarkupExtension || !xamlType.CanAssignTo(parentProperty.Type))
                            {
                                this.Logic_AssignProvidedValue(this._context);
                                flag = false;
                            }
                        }
                        else
                        {
                            XamlType type2 = this.GetXamlType(obj3.GetType());
                            if ((type2 == XamlLanguage.String) || !type2.CanAssignTo(parentProperty.Type))
                            {
                                if ((parentProperty.IsDirective && (parentProperty == XamlLanguage.Key)) && !this.Logic_ShouldConvertKey(this._context))
                                {
                                    flag = true;
                                    this._context.ParentKeyIsUnconverted = true;
                                }
                                else
                                {
                                    flag = this.Logic_CreatePropertyValueFromValue(this._context);
                                }
                            }
                        }
                    }
                    this._lastInstance = this._context.CurrentInstance;
                    if (flag)
                    {
                        this.Logic_DoAssignmentToParentProperty(this._context);
                    }
                    this._context.PopScope();
                }
                this._context.CurrentProperty = null;
                this._context.CurrentIsPropertyValueSet = false;
            }
        }

        public override void WriteEndObject()
        {
            this.ThrowIfDisposed();
            this._deferringWriter.WriteEndObject();
            if (this._deferringWriter.Handled)
            {
                if (this._deferringWriter.Mode == DeferringMode.TemplateReady)
                {
                    XamlNodeList list = this._deferringWriter.CollectTemplateList();
                    this._context.PushScope();
                    this._context.CurrentInstance = list.GetReader();
                }
            }
            else
            {
                if (this._nextNodeMustBeEndMember)
                {
                    string message = System.Xaml.SR.Get("ValueMustBeFollowedByEndMember");
                    throw this.WithLineInfo(new XamlObjectWriterException(message));
                }
                if (this._context.CurrentType == null)
                {
                    string str2 = System.Xaml.SR.Get("NoTypeInCurrentFrame_EO");
                    throw this.WithLineInfo(new XamlObjectWriterException(str2));
                }
                if (this._context.CurrentProperty != null)
                {
                    string str3 = System.Xaml.SR.Get("OpenPropertyInCurrentFrame_EO", new object[] { this._context.CurrentType.ToString(), this._context.CurrentProperty.ToString() });
                    throw this.WithLineInfo(new XamlObjectWriterException(str3));
                }
                bool flag = this.HasUnresolvedChildren(this._context.CurrentInstance);
                bool flag2 = this._context.CurrentInstance is NameFixupToken;
                if (!this._context.CurrentIsObjectFromMember)
                {
                    if (this._context.CurrentInstance == null)
                    {
                        this.Logic_CreateAndAssignToParentStart(this._context);
                    }
                    XamlType currentType = this._context.CurrentType;
                    object currentInstance = this._context.CurrentInstance;
                    this.OnAfterProperties(currentInstance);
                    if (this._context.CurrentType.IsMarkupExtension)
                    {
                        if (flag)
                        {
                            this.Logic_DeferProvideValue(this._context);
                        }
                        else
                        {
                            this.ExecutePendingAdds(this._context.CurrentType, this._context.CurrentInstance);
                            this.Logic_EndInit(this._context);
                            currentInstance = this._context.CurrentInstance;
                            this.Logic_AssignProvidedValue(this._context);
                            if (this._context.CurrentInstanceRegisteredName != null)
                            {
                                if (this._nameFixupGraph != null)
                                {
                                    this.TriggerNameResolution(currentInstance, this._context.CurrentInstanceRegisteredName);
                                }
                                this._context.CurrentInstanceRegisteredName = null;
                            }
                            currentInstance = this._context.CurrentInstance;
                            flag2 = currentInstance is NameFixupToken;
                            flag = !flag2 && this.HasUnresolvedChildren(currentInstance);
                        }
                    }
                    else
                    {
                        if ((this._context.LiveDepth > 1) && !this._context.CurrentWasAssignedAtCreation)
                        {
                            this.Logic_DoAssignmentToParentProperty(this._context);
                        }
                        if (flag)
                        {
                            if (this._context.LiveDepth > 1)
                            {
                                this.AddDependencyForUnresolvedChildren(this._context.CurrentInstance, this._context.ParentProperty, this._context.ParentInstance, this._context.ParentType, this._context.ParentIsObjectFromMember, null);
                            }
                        }
                        else if (!flag2)
                        {
                            this.ExecutePendingAdds(this._context.CurrentType, this._context.CurrentInstance);
                            this.Logic_EndInit(this._context);
                        }
                    }
                }
                else
                {
                    if (flag)
                    {
                        this.AddDependencyForUnresolvedChildren(this._context.CurrentInstance, this._context.ParentProperty, this._context.ParentInstance, this._context.ParentType, this._context.ParentIsObjectFromMember, null);
                    }
                    else
                    {
                        this.ExecutePendingAdds(this._context.CurrentType, this._context.CurrentInstance);
                    }
                    if (this._context.ParentIsPropertyValueSet)
                    {
                        throw this.WithLineInfo(new XamlDuplicateMemberException(this._context.ParentProperty, this._context.ParentType));
                    }
                }
                this._lastInstance = this._context.CurrentInstance;
                string currentInstanceRegisteredName = this._context.CurrentInstanceRegisteredName;
                if (this._context.LiveDepth == 1)
                {
                    this._rootNamescope = this._context.RootNameScope;
                }
                this._context.PopScope();
                if (flag)
                {
                    this._nameFixupGraph.IsOffTheStack(this._lastInstance, currentInstanceRegisteredName, this._lineNumber, this._linePosition);
                }
                else if (flag2)
                {
                    if (currentInstanceRegisteredName != null)
                    {
                        NameFixupToken token = (NameFixupToken) this._lastInstance;
                        if ((token.FixupType == FixupType.ObjectInitializationValue) && !token.CanAssignDirectly)
                        {
                            token.SavedContext.Stack.PreviousFrame.InstanceRegisteredName = currentInstanceRegisteredName;
                        }
                    }
                }
                else if (this._nameFixupGraph != null)
                {
                    this.TriggerNameResolution(this._lastInstance, currentInstanceRegisteredName);
                }
                if ((this._context.LiveDepth == 0) && !this._inDispose)
                {
                    this.CompleteNameReferences();
                    this._context.RaiseNameScopeInitializationCompleteEvent();
                }
            }
        }

        public override void WriteGetObject()
        {
            this.ThrowIfDisposed();
            this._deferringWriter.WriteGetObject();
            if (!this._deferringWriter.Handled)
            {
                if (this._nextNodeMustBeEndMember)
                {
                    string message = System.Xaml.SR.Get("ValueMustBeFollowedByEndMember");
                    throw this.WithLineInfo(new XamlObjectWriterException(message));
                }
                XamlMember property = ((this._context.CurrentType == null) && (this._context.Depth > 1)) ? this._context.ParentProperty : this._context.CurrentProperty;
                if (property == null)
                {
                    XamlType type = ((this._context.CurrentType == null) && (this._context.Depth > 1)) ? this._context.ParentType : this._context.CurrentType;
                    string str2 = (type != null) ? System.Xaml.SR.Get("NoPropertyInCurrentFrame_GO", new object[] { type.ToString() }) : System.Xaml.SR.Get("NoPropertyInCurrentFrame_GO_noType");
                    throw this.WithLineInfo(new XamlObjectWriterException(str2));
                }
                this._lastInstance = null;
                if (this._context.CurrentType != null)
                {
                    this._context.PushScope();
                }
                this.TryCreateParentInstance(this._context);
                this._context.CurrentIsObjectFromMember = true;
                object parentInstance = this._context.ParentInstance;
                this._context.CurrentType = property.Type;
                object obj3 = this.Runtime.GetValue(parentInstance, property);
                if (obj3 == null)
                {
                    throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("GetObjectNull", new object[] { parentInstance.GetType(), property.Name })));
                }
                this._context.CurrentInstance = obj3;
                if (property.Type.IsCollection || property.Type.IsDictionary)
                {
                    this._context.CurrentCollection = obj3;
                }
            }
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            this.ThrowIfDisposed();
            if (namespaceDeclaration == null)
            {
                throw new ArgumentNullException("namespaceDeclaration");
            }
            if (namespaceDeclaration.Prefix == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("NamespaceDeclarationPrefixCannotBeNull"));
            }
            if (namespaceDeclaration.Namespace == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("NamespaceDeclarationNamespaceCannotBeNull"));
            }
            this._deferringWriter.WriteNamespace(namespaceDeclaration);
            if (!this._deferringWriter.Handled)
            {
                if (this._nextNodeMustBeEndMember)
                {
                    string message = System.Xaml.SR.Get("ValueMustBeFollowedByEndMember");
                    throw this.WithLineInfo(new XamlObjectWriterException(message));
                }
                if ((this._context.CurrentType != null) && (this._context.CurrentProperty == null))
                {
                    string str2 = System.Xaml.SR.Get("NoPropertyInCurrentFrame_NS", new object[] { namespaceDeclaration.Prefix, namespaceDeclaration.Namespace, this._context.CurrentType.ToString() });
                    throw this.WithLineInfo(new XamlObjectWriterException(str2));
                }
                if (this._context.CurrentType != null)
                {
                    this._context.PushScope();
                }
                this._context.AddNamespacePrefix(namespaceDeclaration.Prefix, namespaceDeclaration.Namespace);
            }
        }

        public override void WriteStartMember(XamlMember property)
        {
            this.ThrowIfDisposed();
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            this._deferringWriter.WriteStartMember(property);
            if (!this._deferringWriter.Handled)
            {
                string message = null;
                if (this._nextNodeMustBeEndMember)
                {
                    message = System.Xaml.SR.Get("ValueMustBeFollowedByEndMember");
                }
                else if (property == XamlLanguage.UnknownContent)
                {
                    message = System.Xaml.SR.Get("TypeHasNoContentProperty", new object[] { this._context.CurrentType });
                }
                else if (property.IsUnknown)
                {
                    message = System.Xaml.SR.Get("CantSetUnknownProperty", new object[] { property.ToString() });
                }
                else if (this._context.CurrentProperty != null)
                {
                    message = System.Xaml.SR.Get("OpenPropertyInCurrentFrame_SM", new object[] { this._context.CurrentType.ToString(), this._context.CurrentProperty.ToString(), property.ToString() });
                }
                else if (this._context.CurrentType == null)
                {
                    message = System.Xaml.SR.Get("NoTypeInCurrentFrame_SM", new object[] { property.ToString() });
                }
                if (message != null)
                {
                    throw this.WithLineInfo(new XamlObjectWriterException(message));
                }
                this._context.CurrentProperty = property;
                this.Logic_DuplicatePropertyCheck(this._context, property, false);
                if (this._context.CurrentInstance == null)
                {
                    if (!this.IsConstructionDirective(this._context.CurrentProperty) && !this.IsDirectiveAllowedOnNullInstance(this._context.CurrentProperty, this._context.CurrentType))
                    {
                        this.Logic_CreateAndAssignToParentStart(this._context);
                    }
                    if (property == XamlLanguage.PositionalParameters)
                    {
                        this._context.CurrentCollection = new List<PositionalParameterDescriptor>();
                    }
                }
                else
                {
                    if (this.IsTextConstructionDirective(property))
                    {
                        throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("LateConstructionDirective", new object[] { property.Name })));
                    }
                    if (this._context.CurrentIsTypeConvertedObject)
                    {
                        if (!property.IsDirective && !property.IsAttachable)
                        {
                            throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("SettingPropertiesIsNotAllowed", new object[] { property.Name })));
                        }
                        if (property.IsAttachable && (this._context.CurrentInstance is NameFixupToken))
                        {
                            NameFixupToken currentInstance = (NameFixupToken) this._context.CurrentInstance;
                            throw this.WithLineInfo(new XamlObjectWriterException(System.Xaml.SR.Get("AttachedPropOnFwdRefTC", new object[] { property, this._context.CurrentType, string.Join(", ", currentInstance.NeededNames.ToArray()) })));
                        }
                    }
                }
                if ((property.IsDirective && (property != XamlLanguage.Items)) && (property != XamlLanguage.PositionalParameters))
                {
                    XamlType type = property.Type;
                    if (type.IsCollection || type.IsDictionary)
                    {
                        this._context.CurrentCollection = this.Runtime.CreateInstance(property.Type, null);
                    }
                }
            }
        }

        public override void WriteStartObject(XamlType xamlType)
        {
            this.ThrowIfDisposed();
            if (xamlType == null)
            {
                throw new ArgumentNullException("xamlType");
            }
            this._deferringWriter.WriteStartObject(xamlType);
            if (!this._deferringWriter.Handled)
            {
                if (this._nextNodeMustBeEndMember)
                {
                    string message = System.Xaml.SR.Get("ValueMustBeFollowedByEndMember");
                    throw this.WithLineInfo(new XamlObjectWriterException(message));
                }
                if (xamlType.IsUnknown)
                {
                    string str2 = System.Xaml.SR.Get("CantCreateUnknownType", new object[] { xamlType.GetQualifiedName() });
                    throw this.WithLineInfo(new XamlObjectWriterException(str2));
                }
                if ((this._context.CurrentType != null) && (this._context.CurrentProperty == null))
                {
                    string str3 = System.Xaml.SR.Get("NoPropertyInCurrentFrame_SO", new object[] { xamlType.ToString(), this._context.CurrentType.ToString() });
                    throw this.WithLineInfo(new XamlObjectWriterException(str3));
                }
                this._lastInstance = null;
                if (this._context.CurrentType != null)
                {
                    this._context.PushScope();
                }
                this._context.CurrentType = xamlType;
                if ((this._context.LiveDepth == 1) && (this._rootObjectInstance != null))
                {
                    XamlType type = this.GetXamlType(this._rootObjectInstance.GetType());
                    if (!type.CanAssignTo(this._context.CurrentType))
                    {
                        throw new XamlParseException(System.Xaml.SR.Get("CantAssignRootInstance", new object[] { type.GetQualifiedName(), xamlType.GetQualifiedName() }));
                    }
                    this._context.CurrentInstance = this._rootObjectInstance;
                    if (this._context.CurrentType.IsCollection || this._context.CurrentType.IsDictionary)
                    {
                        this._context.CurrentCollection = this._rootObjectInstance;
                    }
                    this.Logic_BeginInit(this._context);
                }
            }
        }

        public override void WriteValue(object value)
        {
            this.ThrowIfDisposed();
            this._deferringWriter.WriteValue(value);
            if (this._deferringWriter.Handled)
            {
                if (this._deferringWriter.Mode == DeferringMode.TemplateReady)
                {
                    XamlNodeList list = this._deferringWriter.CollectTemplateList();
                    this._context.PushScope();
                    this._context.CurrentInstance = list.GetReader();
                }
            }
            else
            {
                XamlMember currentProperty = this._context.CurrentProperty;
                if (currentProperty == null)
                {
                    string message = (this._context.CurrentType != null) ? System.Xaml.SR.Get("NoPropertyInCurrentFrame_V", new object[] { value, this._context.CurrentType.ToString() }) : System.Xaml.SR.Get("NoPropertyInCurrentFrame_V_noType", new object[] { value });
                    throw this.WithLineInfo(new XamlObjectWriterException(message));
                }
                this._lastInstance = null;
                this._context.PushScope();
                this._context.CurrentInstance = value;
                XamlMember member2 = currentProperty;
                currentProperty = null;
                this._nextNodeMustBeEndMember = true;
                if (member2.IsDirective)
                {
                    XamlType type = member2.Type;
                    if (type.IsCollection || type.IsDictionary)
                    {
                        this._nextNodeMustBeEndMember = false;
                        if (member2 == XamlLanguage.PositionalParameters)
                        {
                            this._context.CurrentType = XamlLanguage.PositionalParameterDescriptor;
                            this._context.CurrentInstance = new PositionalParameterDescriptor(value, true);
                            this.Logic_DoAssignmentToParentCollection(this._context);
                            this._context.PopScope();
                        }
                        else
                        {
                            this._context.CurrentInstance = value;
                            this.Logic_DoAssignmentToParentCollection(this._context);
                            this._context.PopScope();
                        }
                    }
                }
            }
        }

        private MS.Internal.Xaml.Context.NameFixupGraph NameFixupGraph
        {
            get
            {
                if (this._nameFixupGraph == null)
                {
                    this._nameFixupGraph = new MS.Internal.Xaml.Context.NameFixupGraph();
                }
                return this._nameFixupGraph;
            }
        }

        private Dictionary<object, List<PendingCollectionAdd>> PendingCollectionAdds
        {
            get
            {
                if (this._pendingCollectionAdds == null)
                {
                    this._pendingCollectionAdds = new Dictionary<object, List<PendingCollectionAdd>>();
                }
                return this._pendingCollectionAdds;
            }
        }

        private Dictionary<object, ObjectWriterContext> PendingKeyConversionContexts
        {
            get
            {
                if (this._pendingKeyConversionContexts == null)
                {
                    this._pendingKeyConversionContexts = new Dictionary<object, ObjectWriterContext>();
                }
                return this._pendingKeyConversionContexts;
            }
        }

        public virtual object Result
        {
            get
            {
                return this._lastInstance;
            }
        }

        public INameScope RootNameScope
        {
            get
            {
                if (this._rootNamescope != null)
                {
                    return this._rootNamescope;
                }
                return this._context.RootNameScope;
            }
        }

        private XamlRuntime Runtime
        {
            get
            {
                return this._context.Runtime;
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                this.ThrowIfDisposed();
                return this._context.SchemaContext;
            }
        }

        public bool ShouldProvideLineInfo
        {
            get
            {
                this.ThrowIfDisposed();
                return true;
            }
        }

        private class PendingCollectionAdd : IAddLineInfo
        {
            XamlException IAddLineInfo.WithLineInfo(XamlException ex)
            {
                if (this.LineNumber > 0)
                {
                    ex.SetLineInfo(this.LineNumber, this.LinePosition);
                }
                return ex;
            }

            public object Item { get; set; }

            public XamlType ItemType { get; set; }

            public object Key { get; set; }

            public bool KeyIsSet { get; set; }

            public bool KeyIsUnconverted { get; set; }

            public int LineNumber { get; set; }

            public int LinePosition { get; set; }
        }
    }
}

