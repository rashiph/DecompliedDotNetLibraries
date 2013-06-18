namespace System.Activities.Hosting
{
    using System;
    using System.Activities;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class WorkflowInstanceExtensionCollection
    {
        private List<object> additionalInstanceExtensions;
        private List<object> allSingletonExtensions;
        private WorkflowInstanceExtensionManager extensionManager;
        private bool hasPersistenceModule;
        private bool hasTrackingParticipant;
        private List<KeyValuePair<WorkflowInstanceExtensionProvider, object>> instanceExtensions;
        private object lastObjectCached;
        private Type lastTypeCached;
        private bool shouldSetInstanceForInstanceExtensions;
        private Dictionary<Type, object> singleTypeCache;
        private List<IWorkflowInstanceExtension> workflowInstanceExtensions;

        internal WorkflowInstanceExtensionCollection(Activity workflowDefinition, WorkflowInstanceExtensionManager extensionManager)
        {
            Dictionary<Type, WorkflowInstanceExtensionProvider> dictionary;
            HashSet<Type> set;
            this.extensionManager = extensionManager;
            int capacity = 0;
            if (extensionManager != null)
            {
                capacity = extensionManager.ExtensionProviders.Count;
                this.hasTrackingParticipant = extensionManager.HasSingletonTrackingParticipant;
                this.hasPersistenceModule = extensionManager.HasSingletonPersistenceModule;
                this.allSingletonExtensions = this.extensionManager.GetAllSingletonExtensions();
            }
            else
            {
                this.allSingletonExtensions = WorkflowInstanceExtensionManager.EmptySingletonExtensions;
            }
            Dictionary<Type, WorkflowInstanceExtensionProvider> dictionary2 = null;
            if (workflowDefinition.GetActivityExtensionInformation(out dictionary, out set))
            {
                HashSet<Type> extensionTypes = new HashSet<Type>();
                if (extensionManager != null)
                {
                    extensionManager.AddAllExtensionTypes(extensionTypes);
                }
                if (dictionary != null)
                {
                    dictionary2 = new Dictionary<Type, WorkflowInstanceExtensionProvider>(dictionary.Count);
                    foreach (KeyValuePair<Type, WorkflowInstanceExtensionProvider> pair in dictionary)
                    {
                        Type key = pair.Key;
                        if (!System.Runtime.TypeHelper.ContainsCompatibleType(extensionTypes, key))
                        {
                            List<Type> list = null;
                            bool flag = false;
                            foreach (Type type2 in dictionary2.Keys)
                            {
                                if (System.Runtime.TypeHelper.AreReferenceTypesCompatible(type2, key))
                                {
                                    flag = true;
                                    break;
                                }
                                if (System.Runtime.TypeHelper.AreReferenceTypesCompatible(key, type2))
                                {
                                    if (list == null)
                                    {
                                        list = new List<Type>();
                                    }
                                    list.Add(type2);
                                }
                            }
                            if (list != null)
                            {
                                for (int i = 0; i < list.Count; i++)
                                {
                                    dictionary2.Remove(list[i]);
                                }
                            }
                            if (!flag)
                            {
                                dictionary2.Add(key, pair.Value);
                            }
                        }
                    }
                    if (dictionary2.Count > 0)
                    {
                        extensionTypes.UnionWith(dictionary2.Keys);
                        capacity += dictionary2.Count;
                    }
                }
                if ((set != null) && (set.Count > 0))
                {
                    foreach (Type type3 in set)
                    {
                        if (!System.Runtime.TypeHelper.ContainsCompatibleType(extensionTypes, type3))
                        {
                            throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.RequiredExtensionTypeNotFound(type3.ToString())));
                        }
                    }
                }
            }
            if (capacity > 0)
            {
                this.instanceExtensions = new List<KeyValuePair<WorkflowInstanceExtensionProvider, object>>(capacity);
                if (extensionManager != null)
                {
                    List<KeyValuePair<Type, WorkflowInstanceExtensionProvider>> extensionProviders = extensionManager.ExtensionProviders;
                    for (int j = 0; j < extensionProviders.Count; j++)
                    {
                        KeyValuePair<Type, WorkflowInstanceExtensionProvider> pair2 = extensionProviders[j];
                        this.AddInstanceExtension(pair2.Value);
                    }
                }
                if (dictionary2 != null)
                {
                    foreach (WorkflowInstanceExtensionProvider provider in dictionary2.Values)
                    {
                        this.AddInstanceExtension(provider);
                    }
                }
            }
        }

        private void AddInstanceExtension(WorkflowInstanceExtensionProvider extensionProvider)
        {
            object obj2 = extensionProvider.ProvideValue();
            if (obj2 is SymbolResolver)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.SymbolResolverMustBeSingleton));
            }
            if (!this.shouldSetInstanceForInstanceExtensions && (obj2 is IWorkflowInstanceExtension))
            {
                this.shouldSetInstanceForInstanceExtensions = true;
            }
            if (!this.hasTrackingParticipant && extensionProvider.IsMatch<TrackingParticipant>(obj2))
            {
                this.hasTrackingParticipant = true;
            }
            if (!this.hasPersistenceModule && extensionProvider.IsMatch<IPersistencePipelineModule>(obj2))
            {
                this.hasPersistenceModule = true;
            }
            this.instanceExtensions.Add(new KeyValuePair<WorkflowInstanceExtensionProvider, object>(extensionProvider, obj2));
            WorkflowInstanceExtensionManager.AddExtensionClosure(obj2, ref this.additionalInstanceExtensions, ref this.hasTrackingParticipant, ref this.hasPersistenceModule);
        }

        private void CacheExtension<T>(T extension) where T: class
        {
            if (extension != null)
            {
                this.CacheExtension(typeof(T), extension);
            }
        }

        private void CacheExtension(Type extensionType, object extension)
        {
            if (extension != null)
            {
                if (this.singleTypeCache == null)
                {
                    this.singleTypeCache = new Dictionary<Type, object>();
                }
                this.lastTypeCached = extensionType;
                this.lastObjectCached = extension;
                this.singleTypeCache[extensionType] = extension;
            }
        }

        public void Cancel()
        {
            foreach (ICancelable cancelable in this.GetInstanceExtensions<ICancelable>(true))
            {
                cancelable.Cancel();
            }
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in this.GetInstanceExtensions<IDisposable>(true))
            {
                disposable.Dispose();
            }
        }

        public T Find<T>() where T: class
        {
            object obj2;
            T local2;
            T extension = default(T);
            if (this.TryGetCachedExtension(typeof(T), out obj2))
            {
                return (T) obj2;
            }
            try
            {
                for (int i = 0; i < this.allSingletonExtensions.Count; i++)
                {
                    object obj3 = this.allSingletonExtensions[i];
                    extension = obj3 as T;
                    if (extension != null)
                    {
                        return extension;
                    }
                }
                if (this.instanceExtensions != null)
                {
                    for (int j = 0; j < this.instanceExtensions.Count; j++)
                    {
                        KeyValuePair<WorkflowInstanceExtensionProvider, object> pair = this.instanceExtensions[j];
                        if (pair.Key.IsMatch<T>(pair.Value))
                        {
                            return (T) pair.Value;
                        }
                    }
                    if (this.additionalInstanceExtensions != null)
                    {
                        for (int k = 0; k < this.additionalInstanceExtensions.Count; k++)
                        {
                            object obj4 = this.additionalInstanceExtensions[k];
                            extension = obj4 as T;
                            if (extension != null)
                            {
                                return extension;
                            }
                        }
                    }
                }
                local2 = extension;
            }
            finally
            {
                this.CacheExtension<T>(extension);
            }
            return local2;
        }

        public IEnumerable<T> FindAll<T>() where T: class
        {
            return this.FindAll<T>(false);
        }

        private IEnumerable<T> FindAll<T>(bool useObjectTypeForComparison) where T: class
        {
            object iteratorVariable0;
            if (this.TryGetCachedExtension(typeof(T), out iteratorVariable0))
            {
                yield return (T) iteratorVariable0;
            }
            else
            {
                T extension = default(T);
                bool iteratorVariable2 = false;
                foreach (T iteratorVariable3 in this.allSingletonExtensions.OfType<T>())
                {
                    if (extension == null)
                    {
                        extension = iteratorVariable3;
                    }
                    else
                    {
                        iteratorVariable2 = true;
                    }
                    yield return iteratorVariable3;
                }
                foreach (T iteratorVariable4 in this.GetInstanceExtensions<T>(useObjectTypeForComparison))
                {
                    if (extension == null)
                    {
                        extension = iteratorVariable4;
                    }
                    else
                    {
                        iteratorVariable2 = true;
                    }
                    yield return iteratorVariable4;
                }
                if (!iteratorVariable2)
                {
                    this.CacheExtension<T>(extension);
                }
            }
        }

        private IEnumerable<T> GetInstanceExtensions<T>(bool useObjectTypeForComparison) where T: class
        {
            if (this.instanceExtensions != null)
            {
                for (int i = 0; i < this.instanceExtensions.Count; i++)
                {
                    KeyValuePair<WorkflowInstanceExtensionProvider, object> iteratorVariable1 = this.instanceExtensions[i];
                    if ((useObjectTypeForComparison && (iteratorVariable1.Value is T)) || iteratorVariable1.Key.IsMatch<T>(iteratorVariable1.Value))
                    {
                        yield return (T) iteratorVariable1.Value;
                    }
                }
                if (this.additionalInstanceExtensions != null)
                {
                    foreach (object iteratorVariable2 in this.additionalInstanceExtensions)
                    {
                        if (!(iteratorVariable2 is T))
                        {
                            continue;
                        }
                        yield return (T) iteratorVariable2;
                    }
                }
            }
        }

        internal void Initialize()
        {
            if ((this.extensionManager != null) && this.extensionManager.HasSingletonIWorkflowInstanceExtensions)
            {
                this.SetInstance(this.extensionManager.SingletonExtensions);
                if (this.extensionManager.HasAdditionalSingletonIWorkflowInstanceExtensions)
                {
                    this.SetInstance(this.extensionManager.AdditionalSingletonExtensions);
                }
            }
            if (this.shouldSetInstanceForInstanceExtensions)
            {
                for (int i = 0; i < this.instanceExtensions.Count; i++)
                {
                    KeyValuePair<WorkflowInstanceExtensionProvider, object> pair = this.instanceExtensions[i];
                    IWorkflowInstanceExtension item = pair.Value as IWorkflowInstanceExtension;
                    if (item != null)
                    {
                        if (this.workflowInstanceExtensions == null)
                        {
                            this.workflowInstanceExtensions = new List<IWorkflowInstanceExtension>();
                        }
                        this.workflowInstanceExtensions.Add(item);
                    }
                }
                if (this.additionalInstanceExtensions != null)
                {
                    this.SetInstance(this.additionalInstanceExtensions);
                }
            }
        }

        private void SetInstance(List<object> extensionsList)
        {
            for (int i = 0; i < extensionsList.Count; i++)
            {
                object obj2 = extensionsList[i];
                if (obj2 is IWorkflowInstanceExtension)
                {
                    if (this.workflowInstanceExtensions == null)
                    {
                        this.workflowInstanceExtensions = new List<IWorkflowInstanceExtension>();
                    }
                    this.workflowInstanceExtensions.Add((IWorkflowInstanceExtension) obj2);
                }
            }
        }

        private bool TryGetCachedExtension(Type type, out object extension)
        {
            if (this.singleTypeCache == null)
            {
                extension = null;
                return false;
            }
            if (object.ReferenceEquals(type, this.lastTypeCached))
            {
                extension = this.lastObjectCached;
                return true;
            }
            return this.singleTypeCache.TryGetValue(type, out extension);
        }

        internal bool HasPersistenceModule
        {
            get
            {
                return this.hasPersistenceModule;
            }
        }

        internal bool HasTrackingParticipant
        {
            get
            {
                return this.hasTrackingParticipant;
            }
        }

        public bool HasWorkflowInstanceExtensions
        {
            get
            {
                return ((this.workflowInstanceExtensions != null) && (this.workflowInstanceExtensions.Count > 0));
            }
        }

        public List<IWorkflowInstanceExtension> WorkflowInstanceExtensions
        {
            get
            {
                return this.workflowInstanceExtensions;
            }
        }

        [CompilerGenerated]
        private sealed class <FindAll>d__0<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T: class
        {
            private int <>1__state;
            private T <>2__current;
            public bool <>3__useObjectTypeForComparison;
            public WorkflowInstanceExtensionCollection <>4__this;
            public IEnumerator<T> <>7__wrap6;
            public IEnumerator<T> <>7__wrap8;
            private int <>l__initialThreadId;
            public object <cachedExtension>5__1;
            public T <extension>5__4;
            public T <extension>5__5;
            public bool <hasMultiple>5__3;
            public T <lastExtension>5__2;
            public bool useObjectTypeForComparison;

            [DebuggerHidden]
            public <FindAll>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally7()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap6 != null)
                {
                    this.<>7__wrap6.Dispose();
                }
            }

            private void <>m__Finally9()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap8 != null)
                {
                    this.<>7__wrap8.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            if (!this.<>4__this.TryGetCachedExtension(typeof(T), out this.<cachedExtension>5__1))
                            {
                                break;
                            }
                            this.<>2__current = (T) this.<cachedExtension>5__1;
                            this.<>1__state = 1;
                            return true;

                        case 1:
                            this.<>1__state = -1;
                            goto Label_01B9;

                        case 3:
                            goto Label_00FD;

                        case 5:
                            goto Label_0186;

                        default:
                            goto Label_01B9;
                    }
                    this.<lastExtension>5__2 = default(T);
                    this.<hasMultiple>5__3 = false;
                    this.<>7__wrap6 = this.<>4__this.allSingletonExtensions.OfType<T>().GetEnumerator();
                    this.<>1__state = 2;
                    while (this.<>7__wrap6.MoveNext())
                    {
                        this.<extension>5__4 = this.<>7__wrap6.Current;
                        if (this.<lastExtension>5__2 == null)
                        {
                            this.<lastExtension>5__2 = this.<extension>5__4;
                        }
                        else
                        {
                            this.<hasMultiple>5__3 = true;
                        }
                        this.<>2__current = this.<extension>5__4;
                        this.<>1__state = 3;
                        return true;
                    Label_00FD:
                        this.<>1__state = 2;
                    }
                    this.<>m__Finally7();
                    this.<>7__wrap8 = this.<>4__this.GetInstanceExtensions<T>(this.useObjectTypeForComparison).GetEnumerator();
                    this.<>1__state = 4;
                    while (this.<>7__wrap8.MoveNext())
                    {
                        this.<extension>5__5 = this.<>7__wrap8.Current;
                        if (this.<lastExtension>5__2 == null)
                        {
                            this.<lastExtension>5__2 = this.<extension>5__5;
                        }
                        else
                        {
                            this.<hasMultiple>5__3 = true;
                        }
                        this.<>2__current = this.<extension>5__5;
                        this.<>1__state = 5;
                        return true;
                    Label_0186:
                        this.<>1__state = 4;
                    }
                    this.<>m__Finally9();
                    if (!this.<hasMultiple>5__3)
                    {
                        this.<>4__this.CacheExtension<T>(this.<lastExtension>5__2);
                    }
                Label_01B9:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                WorkflowInstanceExtensionCollection.<FindAll>d__0<T> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (WorkflowInstanceExtensionCollection.<FindAll>d__0<T>) this;
                }
                else
                {
                    d__ = new WorkflowInstanceExtensionCollection.<FindAll>d__0<T>(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.useObjectTypeForComparison = this.<>3__useObjectTypeForComparison;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 2:
                    case 3:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally7();
                        }
                        break;

                    case 4:
                    case 5:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally9();
                        }
                        return;
                }
            }

            T IEnumerator<T>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <GetInstanceExtensions>d__c<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T: class
        {
            private int <>1__state;
            private T <>2__current;
            public bool <>3__useObjectTypeForComparison;
            public WorkflowInstanceExtensionCollection <>4__this;
            public List<object>.Enumerator <>7__wrap10;
            private int <>l__initialThreadId;
            public object <additionalExtension>5__f;
            public int <i>5__d;
            public KeyValuePair<WorkflowInstanceExtensionProvider, object> <keyedExtension>5__e;
            public bool useObjectTypeForComparison;

            [DebuggerHidden]
            public <GetInstanceExtensions>d__c(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally11()
            {
                this.<>1__state = -1;
                this.<>7__wrap10.Dispose();
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            if (this.<>4__this.instanceExtensions == null)
                            {
                                goto Label_016C;
                            }
                            this.<i>5__d = 0;
                            goto Label_00D1;

                        case 1:
                            goto Label_00BC;

                        case 3:
                            goto Label_0152;

                        default:
                            goto Label_016C;
                    }
                Label_0045:
                    this.<keyedExtension>5__e = this.<>4__this.instanceExtensions[this.<i>5__d];
                    if ((!this.useObjectTypeForComparison || !(this.<keyedExtension>5__e.Value is T)) && !this.<keyedExtension>5__e.Key.IsMatch<T>(this.<keyedExtension>5__e.Value))
                    {
                        goto Label_00C3;
                    }
                    this.<>2__current = (T) this.<keyedExtension>5__e.Value;
                    this.<>1__state = 1;
                    return true;
                Label_00BC:
                    this.<>1__state = -1;
                Label_00C3:
                    this.<i>5__d++;
                Label_00D1:
                    if (this.<i>5__d < this.<>4__this.instanceExtensions.Count)
                    {
                        goto Label_0045;
                    }
                    if (this.<>4__this.additionalInstanceExtensions != null)
                    {
                        this.<>7__wrap10 = this.<>4__this.additionalInstanceExtensions.GetEnumerator();
                        this.<>1__state = 2;
                        while (this.<>7__wrap10.MoveNext())
                        {
                            this.<additionalExtension>5__f = this.<>7__wrap10.Current;
                            if (!(this.<additionalExtension>5__f is T))
                            {
                                continue;
                            }
                            this.<>2__current = (T) this.<additionalExtension>5__f;
                            this.<>1__state = 3;
                            return true;
                        Label_0152:
                            this.<>1__state = 2;
                        }
                        this.<>m__Finally11();
                    }
                Label_016C:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                WorkflowInstanceExtensionCollection.<GetInstanceExtensions>d__c<T> _c;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    _c = (WorkflowInstanceExtensionCollection.<GetInstanceExtensions>d__c<T>) this;
                }
                else
                {
                    _c = new WorkflowInstanceExtensionCollection.<GetInstanceExtensions>d__c<T>(0) {
                        <>4__this = this.<>4__this
                    };
                }
                _c.useObjectTypeForComparison = this.<>3__useObjectTypeForComparison;
                return _c;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 2:
                    case 3:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally11();
                        }
                        return;
                }
            }

            T IEnumerator<T>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}

