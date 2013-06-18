namespace MS.Internal.Xaml.Context
{
    using MS.Internal.Xaml;
    using MS.Internal.Xaml.Runtime;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xaml.Context;
    using System.Xaml.Schema;

    internal class ObjectWriterContext : XamlContext
    {
        private bool _nameResolutionComplete;
        private List<NameScopeInitializationCompleteSubscriber> _nameScopeInitializationCompleteSubscribers;
        private object _rootInstance;
        private XamlRuntime _runtime;
        private int _savedDepth;
        private MS.Internal.Xaml.ServiceProviderContext _serviceProviderContext;
        private XamlObjectWriterSettings _settings;
        private XamlContextStack<ObjectWriterFrame> _stack;

        public ObjectWriterContext(XamlSavedContext savedContext, XamlObjectWriterSettings settings, INameScope rootNameScope, XamlRuntime runtime) : base(savedContext.SchemaContext)
        {
            INameScopeDictionary dictionary;
            this._stack = new XamlContextStack<ObjectWriterFrame>(savedContext.Stack, false);
            if (settings != null)
            {
                this._settings = settings.StripDelegates();
            }
            this._runtime = runtime;
            this.BaseUri = savedContext.BaseUri;
            switch (savedContext.SaveContextType)
            {
                case SavedContextType.Template:
                    dictionary = null;
                    if (rootNameScope != null)
                    {
                        dictionary = rootNameScope as INameScopeDictionary;
                        if (dictionary == null)
                        {
                            dictionary = new NameScopeDictionary(rootNameScope);
                        }
                        break;
                    }
                    dictionary = new NameScope();
                    break;

                case SavedContextType.ReparseValue:
                case SavedContextType.ReparseMarkupExtension:
                    this._savedDepth = this._stack.Depth - 1;
                    return;

                default:
                    return;
            }
            this._stack.PushScope();
            this._savedDepth = this._stack.Depth;
            this._stack.CurrentFrame.NameScopeDictionary = dictionary;
            this._stack.PushScope();
        }

        public ObjectWriterContext(XamlSchemaContext schemaContext, XamlObjectWriterSettings settings, INameScope rootNameScope, XamlRuntime runtime) : base(schemaContext)
        {
            this._stack = new XamlContextStack<ObjectWriterFrame>(() => new ObjectWriterFrame());
            INameScopeDictionary dictionary = null;
            if (rootNameScope == null)
            {
                dictionary = new NameScope();
            }
            else
            {
                dictionary = rootNameScope as INameScopeDictionary;
                if (dictionary == null)
                {
                    dictionary = new NameScopeDictionary(rootNameScope);
                }
            }
            this._stack.CurrentFrame.NameScopeDictionary = dictionary;
            this._stack.PushScope();
            if (settings != null)
            {
                this._settings = settings.StripDelegates();
            }
            this._runtime = runtime;
            this._savedDepth = 0;
        }

        internal void AddNameScopeInitializationCompleteSubscriber(EventHandler handler)
        {
            if (this._nameScopeInitializationCompleteSubscribers == null)
            {
                this._nameScopeInitializationCompleteSubscribers = new List<NameScopeInitializationCompleteSubscriber>();
            }
            NameScopeInitializationCompleteSubscriber item = new NameScopeInitializationCompleteSubscriber {
                Handler = handler
            };
            item.NameScopeDictionaryList.AddRange(this.StackWalkOfNameScopes);
            this._nameScopeInitializationCompleteSubscribers.Add(item);
        }

        public override void AddNamespacePrefix(string prefix, string xamlNS)
        {
            this._stack.CurrentFrame.AddNamespace(prefix, xamlNS);
        }

        private static void CheckAmbient(XamlMember xamlMember)
        {
            if (!xamlMember.IsAmbient)
            {
                throw new ArgumentException(System.Xaml.SR.Get("NotAmbientProperty", new object[] { xamlMember.DeclaringType.Name, xamlMember.Name }), "xamlMember");
            }
        }

        private static void CheckAmbient(XamlType xamlType)
        {
            if (!xamlType.IsAmbient)
            {
                throw new ArgumentException(System.Xaml.SR.Get("NotAmbientType", new object[] { xamlType.Name }), "xamlType");
            }
        }

        private List<object> FindAmbientValues(XamlType[] types, bool stopAfterFirst)
        {
            ArrayHelper.ForAll<XamlType>(types, new Action<XamlType>(ObjectWriterContext.CheckAmbient));
            List<object> list = new List<object>();
            ObjectWriterFrame previousFrame = this._stack.PreviousFrame;
            ObjectWriterFrame currentFrame = this._stack.CurrentFrame;
            while (previousFrame.Depth >= 1)
            {
                foreach (XamlType type in types)
                {
                    object instance = previousFrame.Instance;
                    if (((previousFrame.XamlType != null) && previousFrame.XamlType.CanAssignTo(type)) && (instance != null))
                    {
                        list.Add(instance);
                        if (stopAfterFirst)
                        {
                            return list;
                        }
                    }
                }
                previousFrame = (ObjectWriterFrame) previousFrame.Previous;
            }
            return list;
        }

        private List<AmbientPropertyValue> FindAmbientValues(IEnumerable<XamlType> ceilingTypesEnumerable, bool searchLiveStackOnly, IEnumerable<XamlType> types, XamlMember[] properties, bool stopAfterFirst)
        {
            ArrayHelper.ForAll<XamlMember>(properties, new Action<XamlMember>(ObjectWriterContext.CheckAmbient));
            List<XamlType> list = ArrayHelper.ToList<XamlType>(ceilingTypesEnumerable);
            List<AmbientPropertyValue> list2 = new List<AmbientPropertyValue>();
            ObjectWriterFrame previousFrame = this._stack.PreviousFrame;
            ObjectWriterFrame currentFrame = this._stack.CurrentFrame;
            while (previousFrame.Depth >= 1)
            {
                if (searchLiveStackOnly && (previousFrame.Depth <= this.SavedDepth))
                {
                    return list2;
                }
                object instance = previousFrame.Instance;
                if (types != null)
                {
                    foreach (XamlType type in types)
                    {
                        if (((previousFrame.XamlType != null) && previousFrame.XamlType.CanAssignTo(type)) && (instance != null))
                        {
                            AmbientPropertyValue item = new AmbientPropertyValue(null, instance);
                            list2.Add(item);
                        }
                    }
                }
                if (properties != null)
                {
                    foreach (XamlMember member in properties)
                    {
                        bool flag = false;
                        object obj3 = null;
                        if ((previousFrame.XamlType != null) && previousFrame.XamlType.CanAssignTo(member.DeclaringType))
                        {
                            if (instance != null)
                            {
                                if (((member == previousFrame.Member) && (currentFrame.Instance != null)) && ((currentFrame.XamlType != null) && !currentFrame.XamlType.IsUsableDuringInitialization))
                                {
                                    if (!typeof(MarkupExtension).IsAssignableFrom(currentFrame.Instance.GetType()))
                                    {
                                        flag = true;
                                        obj3 = currentFrame.Instance;
                                    }
                                }
                                else
                                {
                                    IQueryAmbient ambient = instance as IQueryAmbient;
                                    if ((ambient == null) || ambient.IsAmbientPropertyAvailable(member.Name))
                                    {
                                        flag = true;
                                        obj3 = this._runtime.GetValue(instance, member);
                                    }
                                }
                            }
                            if (flag)
                            {
                                AmbientPropertyValue value3 = new AmbientPropertyValue(member, obj3);
                                list2.Add(value3);
                            }
                        }
                    }
                }
                if (stopAfterFirst && (list2.Count > 0))
                {
                    return list2;
                }
                if ((list != null) && list.Contains(previousFrame.XamlType))
                {
                    return list2;
                }
                currentFrame = previousFrame;
                previousFrame = (ObjectWriterFrame) previousFrame.Previous;
            }
            return list2;
        }

        public override string FindNamespaceByPrefix(string prefix)
        {
            for (ObjectWriterFrame frame = this._stack.CurrentFrame; frame.Depth > 0; frame = (ObjectWriterFrame) frame.Previous)
            {
                string str;
                if (frame.TryGetNamespaceByPrefix(prefix, out str))
                {
                    return str;
                }
            }
            return null;
        }

        public IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope()
        {
            List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();
            foreach (INameScopeDictionary dictionary in this.StackWalkOfNameScopes)
            {
                using (IEnumerator<KeyValuePair<string, object>> enumerator2 = dictionary.GetEnumerator())
                {
                    Predicate<KeyValuePair<string, object>> match = null;
                    KeyValuePair<string, object> nameValuePair;
                    while (enumerator2.MoveNext())
                    {
                        nameValuePair = enumerator2.Current;
                        if (match == null)
                        {
                            match = pair => pair.Key == nameValuePair.Key;
                        }
                        if (!list.Exists(match))
                        {
                            list.Add(nameValuePair);
                        }
                    }
                }
            }
            return list;
        }

        internal XamlType GetDestinationType()
        {
            ObjectWriterFrame currentFrame = this._stack.CurrentFrame;
            if (currentFrame == null)
            {
                return null;
            }
            if ((currentFrame.Instance != null) && (currentFrame.XamlType == null))
            {
                currentFrame = currentFrame.Previous as ObjectWriterFrame;
            }
            if (currentFrame.Member == XamlLanguage.Initialization)
            {
                return currentFrame.XamlType;
            }
            return currentFrame.Member.Type;
        }

        public override IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
        {
            ObjectWriterFrame currentFrame = this._stack.CurrentFrame;
            Dictionary<string, string> iteratorVariable1 = new Dictionary<string, string>();
            while (currentFrame.Depth > 0)
            {
                if (currentFrame._namespaces != null)
                {
                    foreach (NamespaceDeclaration iteratorVariable2 in currentFrame.GetNamespacePrefixes())
                    {
                        if (iteratorVariable1.ContainsKey(iteratorVariable2.Prefix))
                        {
                            continue;
                        }
                        iteratorVariable1.Add(iteratorVariable2.Prefix, null);
                        yield return iteratorVariable2;
                    }
                }
                currentFrame = (ObjectWriterFrame) currentFrame.Previous;
            }
        }

        public XamlSavedContext GetSavedContext(SavedContextType savedContextType)
        {
            ObjectWriterFrame topFrame = this.GetTopFrame();
            if (topFrame.NameScopeDictionary == null)
            {
                topFrame.NameScopeDictionary = this.LookupNameScopeDictionary(topFrame);
            }
            return new XamlSavedContext(savedContextType, this, new XamlContextStack<ObjectWriterFrame>(this._stack, true));
        }

        private ObjectWriterFrame GetTopFrame()
        {
            if (this._stack.Depth == 0)
            {
                return null;
            }
            XamlFrame currentFrame = this._stack.CurrentFrame;
            while (currentFrame.Depth > 1)
            {
                currentFrame = currentFrame.Previous;
            }
            return (ObjectWriterFrame) currentFrame;
        }

        private INameScopeDictionary HuntAroundForARootNameScope(ObjectWriterFrame rootFrame)
        {
            object instance = rootFrame.Instance;
            if ((instance == null) && rootFrame.XamlType.IsNameScope)
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("NameScopeOnRootInstance"));
            }
            INameScopeDictionary nameScopeDictionary = null;
            nameScopeDictionary = instance as INameScopeDictionary;
            if (nameScopeDictionary == null)
            {
                INameScope underlyingNameScope = instance as INameScope;
                if (underlyingNameScope != null)
                {
                    nameScopeDictionary = new NameScopeDictionary(underlyingNameScope);
                }
            }
            if (nameScopeDictionary == null)
            {
                XamlType xamlType = rootFrame.XamlType;
                if (xamlType.UnderlyingType != null)
                {
                    XamlMember nameScopeProperty = TypeReflector.LookupNameScopeProperty(xamlType);
                    if (nameScopeProperty != null)
                    {
                        INameScope scope2 = (INameScope) this._runtime.GetValue(instance, nameScopeProperty, false);
                        if (scope2 == null)
                        {
                            nameScopeDictionary = new NameScope();
                            this._runtime.SetValue(instance, nameScopeProperty, nameScopeDictionary);
                        }
                        else
                        {
                            nameScopeDictionary = scope2 as INameScopeDictionary;
                            if (nameScopeDictionary == null)
                            {
                                nameScopeDictionary = new NameScopeDictionary(scope2);
                            }
                        }
                    }
                }
            }
            if (((nameScopeDictionary == null) && (this._settings != null)) && this._settings.RegisterNamesOnExternalNamescope)
            {
                ObjectWriterFrame previous = (ObjectWriterFrame) rootFrame.Previous;
                nameScopeDictionary = previous.NameScopeDictionary;
            }
            if (nameScopeDictionary == null)
            {
                nameScopeDictionary = new NameScope();
            }
            rootFrame.NameScopeDictionary = nameScopeDictionary;
            return nameScopeDictionary;
        }

        public bool IsOnTheLiveStack(object instance)
        {
            for (ObjectWriterFrame frame = this._stack.CurrentFrame; frame.Depth > this.SavedDepth; frame = (ObjectWriterFrame) frame.Previous)
            {
                if (instance == frame.Instance)
                {
                    return true;
                }
            }
            return false;
        }

        public void LiftScope()
        {
            this._stack.Depth--;
        }

        private INameScopeDictionary LookupNameScopeDictionary(ObjectWriterFrame frame)
        {
            if (frame.NameScopeDictionary == null)
            {
                if ((frame.XamlType != null) && frame.XamlType.IsNameScope)
                {
                    frame.NameScopeDictionary = (frame.Instance as INameScopeDictionary) ?? new NameScopeDictionary(frame.Instance as INameScope);
                }
                if (frame.NameScopeDictionary == null)
                {
                    if (frame.Depth == 1)
                    {
                        frame.NameScopeDictionary = this.HuntAroundForARootNameScope(frame);
                    }
                    else if (frame.Depth > 1)
                    {
                        if (((frame.Depth == (this.SavedDepth + 1)) && (this._settings != null)) && !this._settings.RegisterNamesOnExternalNamescope)
                        {
                            frame.NameScopeDictionary = new NameScope();
                        }
                        else
                        {
                            ObjectWriterFrame previous = (ObjectWriterFrame) frame.Previous;
                            frame.NameScopeDictionary = this.LookupNameScopeDictionary(previous);
                        }
                    }
                }
            }
            return frame.NameScopeDictionary;
        }

        public void PopScope()
        {
            this._stack.PopScope();
        }

        public void PushScope()
        {
            this._stack.PushScope();
        }

        internal void RaiseNameScopeInitializationCompleteEvent()
        {
            if (this._nameScopeInitializationCompleteSubscribers != null)
            {
                EventArgs e = new EventArgs();
                foreach (NameScopeInitializationCompleteSubscriber subscriber in this._nameScopeInitializationCompleteSubscribers)
                {
                    StackWalkNameResolver sender = new StackWalkNameResolver(subscriber.NameScopeDictionaryList);
                    subscriber.Handler(sender, e);
                }
            }
        }

        internal void RemoveNameScopeInitializationCompleteSubscriber(EventHandler handler)
        {
            NameScopeInitializationCompleteSubscriber item = this._nameScopeInitializationCompleteSubscribers.Find(o => o.Handler == handler);
            if (item != null)
            {
                this._nameScopeInitializationCompleteSubscribers.Remove(item);
            }
        }

        public object ResolveName(string name, out bool isFullyInitialized)
        {
            isFullyInitialized = false;
            object obj2 = null;
            foreach (INameScope scope in this.StackWalkOfNameScopes)
            {
                object obj3 = scope.FindName(name);
                if (obj3 != null)
                {
                    if (this.IsInitializedCallback != null)
                    {
                        isFullyInitialized = this.IsInitializedCallback.IsFullyInitialized(obj3);
                    }
                    if ((!this.NameResolutionComplete && !isFullyInitialized) && (this.IsInitializedCallback != null))
                    {
                        return obj2;
                    }
                    return obj3;
                }
            }
            return obj2;
        }

        internal IEnumerable<object> ServiceProvider_GetAllAmbientValues(XamlType[] types)
        {
            return this.FindAmbientValues(types, false);
        }

        internal IEnumerable<AmbientPropertyValue> ServiceProvider_GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, XamlMember[] properties)
        {
            return this.FindAmbientValues(ceilingTypes, false, null, properties, false);
        }

        internal IEnumerable<AmbientPropertyValue> ServiceProvider_GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, XamlMember[] properties)
        {
            return this.FindAmbientValues(ceilingTypes, searchLiveStackOnly, types, properties, false);
        }

        internal object ServiceProvider_GetFirstAmbientValue(XamlType[] types)
        {
            List<object> list = this.FindAmbientValues(types, true);
            if (list.Count != 0)
            {
                return list[0];
            }
            return null;
        }

        internal AmbientPropertyValue ServiceProvider_GetFirstAmbientValue(IEnumerable<XamlType> ceilingTypes, XamlMember[] properties)
        {
            List<AmbientPropertyValue> list = this.FindAmbientValues(ceilingTypes, false, null, properties, true);
            if (list.Count != 0)
            {
                return list[0];
            }
            return null;
        }

        internal XamlObjectWriterSettings ServiceProvider_GetSettings()
        {
            if (this._settings == null)
            {
                this._settings = new XamlObjectWriterSettings();
            }
            return this._settings;
        }

        internal Type ServiceProvider_Resolve(string qName)
        {
            XamlType type = this.ServiceProvider_ResolveXamlType(qName);
            if ((type != null) && (type.UnderlyingType != null))
            {
                return type.UnderlyingType;
            }
            XamlTypeName typeName = XamlTypeName.Parse(qName, this._serviceProviderContext);
            type = base.GetXamlType(typeName, true, true);
            throw new XamlParseException(System.Xaml.SR.Get("TypeNotFound", new object[] { type.GetQualifiedName() }));
        }

        internal XamlType ServiceProvider_ResolveXamlType(string qName)
        {
            return base.ResolveXamlType(qName, true);
        }

        public void UnLiftScope()
        {
            this._stack.Depth++;
        }

        public Uri BaseUri { get; set; }

        public System.Xaml.Context.HashSet<XamlMember> CurrentAssignedProperties
        {
            get
            {
                return this._stack.CurrentFrame.AssignedProperties;
            }
        }

        public object CurrentCollection
        {
            get
            {
                return this._stack.CurrentFrame.Collection;
            }
            set
            {
                this._stack.CurrentFrame.Collection = value;
            }
        }

        public object[] CurrentCtorArgs
        {
            get
            {
                return this._stack.CurrentFrame.PositionalCtorArgs;
            }
            set
            {
                this._stack.CurrentFrame.PositionalCtorArgs = value;
            }
        }

        public bool CurrentHasPreconstructionPropertyValuesDictionary
        {
            get
            {
                return this._stack.CurrentFrame.HasPreconstructionPropertyValuesDictionary;
            }
        }

        public object CurrentInstance
        {
            get
            {
                return this._stack.CurrentFrame.Instance;
            }
            set
            {
                this._stack.CurrentFrame.Instance = value;
            }
        }

        public string CurrentInstanceRegisteredName
        {
            get
            {
                return this._stack.CurrentFrame.InstanceRegisteredName;
            }
            set
            {
                this._stack.CurrentFrame.InstanceRegisteredName = value;
            }
        }

        public bool CurrentIsKeySet
        {
            get
            {
                return this._stack.CurrentFrame.IsKeySet;
            }
        }

        public bool CurrentIsObjectFromMember
        {
            get
            {
                return this._stack.CurrentFrame.IsObjectFromMember;
            }
            set
            {
                this._stack.CurrentFrame.IsObjectFromMember = value;
            }
        }

        public bool CurrentIsPropertyValueSet
        {
            set
            {
                this._stack.CurrentFrame.IsPropertyValueSet = value;
            }
        }

        public bool CurrentIsTypeConvertedObject
        {
            get
            {
                return this._stack.CurrentFrame.IsTypeConvertedObject;
            }
            set
            {
                this._stack.CurrentFrame.IsTypeConvertedObject = value;
            }
        }

        public object CurrentKey
        {
            get
            {
                return this._stack.CurrentFrame.Key;
            }
        }

        public bool CurrentKeyIsUnconverted
        {
            get
            {
                return this._stack.CurrentFrame.KeyIsUnconverted;
            }
            set
            {
                this._stack.CurrentFrame.KeyIsUnconverted = value;
            }
        }

        public INameScopeDictionary CurrentNameScope
        {
            get
            {
                return this.LookupNameScopeDictionary(this._stack.CurrentFrame);
            }
        }

        public Dictionary<XamlMember, object> CurrentPreconstructionPropertyValues
        {
            get
            {
                return this._stack.CurrentFrame.PreconstructionPropertyValues;
            }
        }

        public XamlMember CurrentProperty
        {
            get
            {
                return this._stack.CurrentFrame.Member;
            }
            set
            {
                this._stack.CurrentFrame.Member = value;
            }
        }

        public XamlType CurrentType
        {
            get
            {
                return this._stack.CurrentFrame.XamlType;
            }
            set
            {
                this._stack.CurrentFrame.XamlType = value;
            }
        }

        public bool CurrentWasAssignedAtCreation
        {
            get
            {
                return this._stack.CurrentFrame.WasAssignedAtCreation;
            }
            set
            {
                this._stack.CurrentFrame.WasAssignedAtCreation = value;
            }
        }

        public int Depth
        {
            get
            {
                return this._stack.Depth;
            }
        }

        public object GrandParentInstance
        {
            get
            {
                if (this._stack.PreviousPreviousFrame == null)
                {
                    return null;
                }
                return this._stack.PreviousPreviousFrame.Instance;
            }
        }

        public bool GrandParentIsObjectFromMember
        {
            get
            {
                if (this._stack.PreviousPreviousFrame == null)
                {
                    return false;
                }
                return this._stack.PreviousPreviousFrame.IsObjectFromMember;
            }
        }

        public INameScopeDictionary GrandParentNameScope
        {
            get
            {
                return this.LookupNameScopeDictionary(this._stack.PreviousPreviousFrame);
            }
        }

        public XamlMember GrandParentProperty
        {
            get
            {
                return this._stack.PreviousPreviousFrame.Member;
            }
        }

        public bool GrandParentShouldConvertChildKeys
        {
            get
            {
                return this._stack.PreviousPreviousFrame.ShouldConvertChildKeys;
            }
            set
            {
                this._stack.PreviousPreviousFrame.ShouldConvertChildKeys = value;
            }
        }

        public bool GrandParentShouldNotConvertChildKeys
        {
            get
            {
                return this._stack.PreviousPreviousFrame.ShouldNotConvertChildKeys;
            }
        }

        public XamlType GrandParentType
        {
            get
            {
                if (this._stack.PreviousPreviousFrame == null)
                {
                    return null;
                }
                return this._stack.PreviousPreviousFrame.XamlType;
            }
        }

        internal ICheckIfInitialized IsInitializedCallback { get; set; }

        public int LiveDepth
        {
            get
            {
                return (this.Depth - this.SavedDepth);
            }
        }

        public override Assembly LocalAssembly
        {
            get
            {
                Assembly localAssembly = base.LocalAssembly;
                if (((localAssembly == null) && (this._settings != null)) && (this._settings.AccessLevel != null))
                {
                    localAssembly = Assembly.Load(this._settings.AccessLevel.AssemblyAccessToAssemblyName);
                    base.LocalAssembly = localAssembly;
                }
                return localAssembly;
            }
            protected set
            {
                base.LocalAssembly = value;
            }
        }

        internal bool NameResolutionComplete
        {
            get
            {
                return this._nameResolutionComplete;
            }
            set
            {
                this._nameResolutionComplete = value;
            }
        }

        public System.Xaml.Context.HashSet<XamlMember> ParentAssignedProperties
        {
            get
            {
                return this._stack.PreviousFrame.AssignedProperties;
            }
        }

        public object ParentCollection
        {
            get
            {
                return this._stack.PreviousFrame.Collection;
            }
        }

        public object ParentInstance
        {
            get
            {
                return this._stack.PreviousFrame.Instance;
            }
        }

        public string ParentInstanceRegisteredName
        {
            get
            {
                return this._stack.PreviousFrame.InstanceRegisteredName;
            }
            set
            {
                this._stack.PreviousFrame.InstanceRegisteredName = value;
            }
        }

        public bool ParentIsObjectFromMember
        {
            get
            {
                return this._stack.PreviousFrame.IsObjectFromMember;
            }
        }

        public bool ParentIsPropertyValueSet
        {
            get
            {
                return this._stack.PreviousFrame.IsPropertyValueSet;
            }
            set
            {
                this._stack.PreviousFrame.IsPropertyValueSet = value;
            }
        }

        public object ParentKey
        {
            get
            {
                return this._stack.PreviousFrame.Key;
            }
            set
            {
                this._stack.PreviousFrame.Key = value;
            }
        }

        public bool ParentKeyIsUnconverted
        {
            set
            {
                this._stack.PreviousFrame.KeyIsUnconverted = value;
            }
        }

        public INameScopeDictionary ParentNameScope
        {
            get
            {
                return this.LookupNameScopeDictionary(this._stack.PreviousFrame);
            }
        }

        public Dictionary<XamlMember, object> ParentPreconstructionPropertyValues
        {
            get
            {
                return this._stack.PreviousFrame.PreconstructionPropertyValues;
            }
        }

        public XamlMember ParentProperty
        {
            get
            {
                return this._stack.PreviousFrame.Member;
            }
        }

        public bool ParentShouldConvertChildKeys
        {
            get
            {
                return this._stack.PreviousFrame.ShouldConvertChildKeys;
            }
            set
            {
                this._stack.PreviousPreviousFrame.ShouldConvertChildKeys = value;
            }
        }

        public bool ParentShouldNotConvertChildKeys
        {
            get
            {
                return this._stack.PreviousFrame.ShouldNotConvertChildKeys;
            }
            set
            {
                this._stack.PreviousPreviousFrame.ShouldNotConvertChildKeys = value;
            }
        }

        public XamlType ParentType
        {
            get
            {
                return this._stack.PreviousFrame.XamlType;
            }
        }

        public object RootInstance
        {
            get
            {
                if (this._rootInstance == null)
                {
                    ObjectWriterFrame topFrame = this.GetTopFrame();
                    this._rootInstance = topFrame.Instance;
                }
                return this._rootInstance;
            }
        }

        public INameScopeDictionary RootNameScope
        {
            get
            {
                ObjectWriterFrame frame = this._stack.GetFrame(this.SavedDepth + 1);
                return this.LookupNameScopeDictionary(frame);
            }
        }

        internal XamlRuntime Runtime
        {
            get
            {
                return this._runtime;
            }
        }

        public int SavedDepth
        {
            get
            {
                return this._savedDepth;
            }
        }

        internal MS.Internal.Xaml.ServiceProviderContext ServiceProviderContext
        {
            get
            {
                if (this._serviceProviderContext == null)
                {
                    this._serviceProviderContext = new MS.Internal.Xaml.ServiceProviderContext(this);
                }
                return this._serviceProviderContext;
            }
        }

        public IEnumerable<INameScopeDictionary> StackWalkOfNameScopes
        {
            get
            {
                ObjectWriterFrame currentFrame = this._stack.CurrentFrame;
                INameScopeDictionary iteratorVariable1 = null;
                INameScopeDictionary nameScopeDictionary = null;
                while (currentFrame.Depth > 0)
                {
                    nameScopeDictionary = this.LookupNameScopeDictionary(currentFrame);
                    if (currentFrame.NameScopeDictionary != iteratorVariable1)
                    {
                        iteratorVariable1 = nameScopeDictionary;
                        yield return nameScopeDictionary;
                    }
                    currentFrame = (ObjectWriterFrame) currentFrame.Previous;
                }
                if ((currentFrame.NameScopeDictionary != null) && (currentFrame.NameScopeDictionary != iteratorVariable1))
                {
                    yield return currentFrame.NameScopeDictionary;
                }
            }
        }



        internal class NameScopeInitializationCompleteSubscriber
        {
            private List<INameScopeDictionary> _nameScopeDictionaryList = new List<INameScopeDictionary>();

            public EventHandler Handler { get; set; }

            public List<INameScopeDictionary> NameScopeDictionaryList
            {
                get
                {
                    return this._nameScopeDictionaryList;
                }
            }
        }

        private class StackWalkNameResolver : IXamlNameResolver
        {
            private List<INameScopeDictionary> _nameScopeDictionaryList;

            public event EventHandler OnNameScopeInitializationComplete
            {
                add
                {
                }
                remove
                {
                }
            }

            public StackWalkNameResolver(List<INameScopeDictionary> nameScopeDictionaryList)
            {
                this._nameScopeDictionaryList = nameScopeDictionaryList;
            }

            public IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope()
            {
                List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();
                foreach (INameScopeDictionary dictionary in this._nameScopeDictionaryList)
                {
                    using (IEnumerator<KeyValuePair<string, object>> enumerator2 = dictionary.GetEnumerator())
                    {
                        Predicate<KeyValuePair<string, object>> match = null;
                        KeyValuePair<string, object> nameValuePair;
                        while (enumerator2.MoveNext())
                        {
                            nameValuePair = enumerator2.Current;
                            if (match == null)
                            {
                                match = pair => pair.Key == nameValuePair.Key;
                            }
                            if (!list.Exists(match))
                            {
                                list.Add(nameValuePair);
                            }
                        }
                    }
                }
                return list;
            }

            public object GetFixupToken(IEnumerable<string> name)
            {
                return null;
            }

            public object GetFixupToken(IEnumerable<string> name, bool canAssignDirectly)
            {
                return null;
            }

            public object Resolve(string name)
            {
                foreach (INameScopeDictionary dictionary in this._nameScopeDictionaryList)
                {
                    object obj3 = dictionary.FindName(name);
                    if (obj3 != null)
                    {
                        return obj3;
                    }
                }
                return null;
            }

            public object Resolve(string name, out bool isFullyInitialized)
            {
                object obj2 = this.Resolve(name);
                isFullyInitialized = obj2 != null;
                return obj2;
            }

            public bool IsFixupTokenAvailable
            {
                get
                {
                    return false;
                }
            }
        }
    }
}

