namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.Runtime.Hosting;
    using System.Xml;

    internal sealed class WorkflowDefinitionDispenser : IDisposable
    {
        private ReaderWriterLock parametersLock;
        private bool validateOnCreate = true;
        internal static DependencyProperty WorkflowDefinitionHashCodeProperty = DependencyProperty.RegisterAttached("WorkflowDefinitionHashCode", typeof(byte[]), typeof(WorkflowDefinitionDispenser));
        private Dictionary<Type, List<PropertyInfo>> workflowOutParameters;
        private WorkflowRuntime workflowRuntime;
        private MruCache workflowTypes;
        private MruCache xomlFragments;

        internal event EventHandler<WorkflowDefinitionEventArgs> WorkflowDefinitionLoaded;

        internal WorkflowDefinitionDispenser(WorkflowRuntime runtime, bool validateOnCreate, int capacity)
        {
            if (capacity <= 0)
            {
                capacity = 0x7d0;
            }
            this.workflowRuntime = runtime;
            this.workflowTypes = new MruCache(capacity, this, CacheType.Type);
            this.xomlFragments = new MruCache(capacity, this, CacheType.Xoml);
            this.workflowOutParameters = new Dictionary<Type, List<PropertyInfo>>();
            this.parametersLock = new ReaderWriterLock();
            this.validateOnCreate = validateOnCreate;
        }

        private void CacheOutputParameters(Activity rootActivity)
        {
            Type key = rootActivity.GetType();
            List<PropertyInfo> list = null;
            this.parametersLock.AcquireWriterLock(-1);
            try
            {
                if (!this.workflowOutParameters.ContainsKey(key))
                {
                    list = new List<PropertyInfo>();
                    this.workflowOutParameters.Add(key, list);
                    foreach (PropertyInfo info in key.GetProperties())
                    {
                        if ((!info.CanRead || (info.DeclaringType == typeof(DependencyObject))) || ((info.DeclaringType == typeof(Activity)) || (info.DeclaringType == typeof(CompositeActivity))))
                        {
                            continue;
                        }
                        bool flag = false;
                        foreach (DependencyProperty property in rootActivity.MetaDependencyProperties)
                        {
                            if ((property.Name == info.Name) && property.DefaultMetadata.IsMetaProperty)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            list.Add(info);
                        }
                    }
                }
            }
            finally
            {
                Thread.MemoryBarrier();
                this.parametersLock.ReleaseLock();
            }
        }

        public void Dispose()
        {
            this.xomlFragments.Dispose();
            this.workflowTypes.Dispose();
        }

        internal ReadOnlyCollection<PropertyInfo> GetOutputParameters(Activity rootActivity)
        {
            Type key = rootActivity.GetType();
            this.parametersLock.AcquireReaderLock(-1);
            try
            {
                if (this.workflowOutParameters.ContainsKey(key))
                {
                    return new ReadOnlyCollection<PropertyInfo>(this.workflowOutParameters[key]);
                }
            }
            finally
            {
                this.parametersLock.ReleaseLock();
            }
            this.CacheOutputParameters(rootActivity);
            return this.GetOutputParameters(rootActivity);
        }

        internal Activity GetRootActivity(Type workflowType, bool createNew, bool initForRuntime)
        {
            Activity dependencyObject = null;
            bool flag;
            if (createNew)
            {
                return this.LoadRootActivity(workflowType, false, initForRuntime);
            }
            dependencyObject = this.workflowTypes.GetOrGenerateDefinition(workflowType, null, null, null, initForRuntime, out flag);
            if (!flag)
            {
                WorkflowDefinitionLock.SetWorkflowDefinitionLockObject(dependencyObject, new object());
                EventHandler<WorkflowDefinitionEventArgs> workflowDefinitionLoaded = this.WorkflowDefinitionLoaded;
                if (workflowDefinitionLoaded != null)
                {
                    workflowDefinitionLoaded(this.workflowRuntime, new WorkflowDefinitionEventArgs(workflowType));
                }
            }
            return dependencyObject;
        }

        internal Activity GetRootActivity(string xomlText, string rulesText, bool createNew, bool initForRuntime)
        {
            bool flag;
            if (string.IsNullOrEmpty(xomlText))
            {
                throw new ArgumentNullException("xomlText");
            }
            byte[] xomlHashCode = null;
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(xomlText);
                if (!string.IsNullOrEmpty(rulesText))
                {
                    writer.Write(rulesText);
                }
                writer.Flush();
                stream.Position = 0L;
                xomlHashCode = MD5HashHelper.ComputeHash(stream.GetBuffer());
            }
            if (createNew)
            {
                return this.LoadRootActivity(xomlText, rulesText, xomlHashCode, false, initForRuntime);
            }
            Activity dependencyObject = this.xomlFragments.GetOrGenerateDefinition(null, xomlText, rulesText, xomlHashCode, initForRuntime, out flag);
            if (!flag)
            {
                WorkflowDefinitionLock.SetWorkflowDefinitionLockObject(dependencyObject, new object());
                EventHandler<WorkflowDefinitionEventArgs> workflowDefinitionLoaded = this.WorkflowDefinitionLoaded;
                if (workflowDefinitionLoaded != null)
                {
                    workflowDefinitionLoaded(this.workflowRuntime, new WorkflowDefinitionEventArgs(xomlHashCode));
                }
            }
            return dependencyObject;
        }

        internal Activity GetWorkflowDefinition(byte[] xomlHashCode)
        {
            Activity definition = null;
            if (xomlHashCode == null)
            {
                throw new ArgumentNullException("xomlHashCode");
            }
            definition = this.xomlFragments.GetDefinition(xomlHashCode);
            if (definition == null)
            {
                throw new ArgumentException("xomlHashCode");
            }
            return definition;
        }

        internal Activity GetWorkflowDefinition(Type workflowType)
        {
            if (workflowType == null)
            {
                throw new ArgumentNullException("workflowType");
            }
            return this.GetRootActivity(workflowType, false, true);
        }

        internal void GetWorkflowDefinitions(out ReadOnlyCollection<byte[]> keys, out ReadOnlyCollection<Activity> values)
        {
            this.xomlFragments.GetWorkflowDefinitions<byte[]>(out keys, out values);
        }

        internal void GetWorkflowTypes(out ReadOnlyCollection<Type> keys, out ReadOnlyCollection<Activity> values)
        {
            this.workflowTypes.GetWorkflowDefinitions<Type>(out keys, out values);
        }

        private Activity LoadRootActivity(Type workflowType, bool createDefinition, bool initForRuntime)
        {
            Activity root = this.workflowRuntime.GetService<WorkflowLoaderService>().CreateInstance(workflowType);
            if (root == null)
            {
                throw new InvalidOperationException(ExecutionStringManager.CannotCreateRootActivity);
            }
            if (root.GetType() != workflowType)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.WorkflowTypeMismatch, new object[] { workflowType.FullName }));
            }
            if (createDefinition)
            {
                this.ValidateDefinition(root, true, this.workflowRuntime.GetService<ITypeProvider>());
            }
            if (initForRuntime)
            {
                ((IDependencyObjectAccessor) root).InitializeDefinitionForRuntime(null);
            }
            root.SetValue(Activity.WorkflowRuntimeProperty, this.workflowRuntime);
            return root;
        }

        private Activity LoadRootActivity(string xomlText, string rulesText, byte[] xomlHashCode, bool createDefinition, bool initForRuntime)
        {
            Activity root = null;
            WorkflowLoaderService service = this.workflowRuntime.GetService<WorkflowLoaderService>();
            using (StringReader reader = new StringReader(xomlText))
            {
                using (XmlReader reader2 = XmlReader.Create(reader))
                {
                    XmlReader rulesReader = null;
                    StringReader input = null;
                    try
                    {
                        if (!string.IsNullOrEmpty(rulesText))
                        {
                            input = new StringReader(rulesText);
                            rulesReader = XmlReader.Create(input);
                        }
                        root = service.CreateInstance(reader2, rulesReader);
                    }
                    finally
                    {
                        if (rulesReader != null)
                        {
                            rulesReader.Close();
                        }
                        if (input != null)
                        {
                            input.Close();
                        }
                    }
                }
            }
            if (root == null)
            {
                throw new InvalidOperationException(ExecutionStringManager.CannotCreateRootActivity);
            }
            if (createDefinition)
            {
                ITypeProvider typeProvider = this.workflowRuntime.GetService<ITypeProvider>();
                this.ValidateDefinition(root, false, typeProvider);
            }
            if (initForRuntime)
            {
                ((IDependencyObjectAccessor) root).InitializeDefinitionForRuntime(null);
            }
            root.SetValue(Activity.WorkflowXamlMarkupProperty, xomlText);
            root.SetValue(Activity.WorkflowRulesMarkupProperty, rulesText);
            root.SetValue(WorkflowDefinitionHashCodeProperty, xomlHashCode);
            root.SetValue(Activity.WorkflowRuntimeProperty, this.workflowRuntime);
            return root;
        }

        internal void ValidateDefinition(Activity root, bool isNewType, ITypeProvider typeProvider)
        {
            if (this.validateOnCreate)
            {
                ValidationErrorCollection errors = new ValidationErrorCollection();
                if (typeProvider == null)
                {
                    typeProvider = WorkflowRuntime.CreateTypeProvider(root);
                }
                if (!isNewType)
                {
                    if (!string.IsNullOrEmpty(root.GetValue(WorkflowMarkupSerializer.XClassProperty) as string))
                    {
                        errors.Add(new ValidationError(ExecutionStringManager.XomlWorkflowHasClassName, 0x61c));
                    }
                    Queue queue = new Queue();
                    queue.Enqueue(root);
                    while (queue.Count > 0)
                    {
                        Activity activity = queue.Dequeue() as Activity;
                        if (activity.GetValue(WorkflowMarkupSerializer.XCodeProperty) != null)
                        {
                            errors.Add(new ValidationError(ExecutionStringManager.XomlWorkflowHasCode, 0x61d));
                        }
                        CompositeActivity activity2 = activity as CompositeActivity;
                        if (activity2 != null)
                        {
                            foreach (Activity activity3 in activity2.EnabledActivities)
                            {
                                queue.Enqueue(activity3);
                            }
                        }
                    }
                }
                ServiceContainer serviceProvider = new ServiceContainer();
                serviceProvider.AddService(typeof(ITypeProvider), typeProvider);
                ValidationManager manager = new ValidationManager(serviceProvider);
                using (WorkflowCompilationContext.CreateScope(manager))
                {
                    foreach (Validator validator in manager.GetValidators(root.GetType()))
                    {
                        foreach (ValidationError error in validator.Validate(manager, root))
                        {
                            if (!error.UserData.Contains(typeof(Activity)))
                            {
                                error.UserData[typeof(Activity)] = root;
                            }
                            errors.Add(error);
                        }
                    }
                }
                if (errors.HasErrors)
                {
                    throw new WorkflowValidationFailedException(ExecutionStringManager.WorkflowValidationFailure, errors);
                }
            }
        }

        private enum CacheType
        {
            Type,
            Xoml
        }

        private class DigestComparerWrapper : IEqualityComparer
        {
            private IEqualityComparer<byte[]> comparer = new DigestComparer();

            bool IEqualityComparer.Equals(object object1, object object2)
            {
                return this.comparer.Equals((byte[]) object1, (byte[]) object2);
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                return this.comparer.GetHashCode((byte[]) obj);
            }
        }

        private class MruCache : IDisposable
        {
            private int capacity;
            private WorkflowDefinitionDispenser dispenser;
            private Hashtable hashtable;
            private LinkedList<Activity> mruList;
            private int size;
            private WorkflowDefinitionDispenser.CacheType type;

            internal MruCache(int capacity, WorkflowDefinitionDispenser dispenser, WorkflowDefinitionDispenser.CacheType type)
            {
                if (type == WorkflowDefinitionDispenser.CacheType.Xoml)
                {
                    this.hashtable = new Hashtable(new WorkflowDefinitionDispenser.DigestComparerWrapper());
                }
                else
                {
                    this.hashtable = new Hashtable();
                }
                this.mruList = new LinkedList<Activity>();
                this.capacity = capacity;
                this.dispenser = dispenser;
                this.type = type;
            }

            private void AddToDictionary(LinkedListNode<Activity> node)
            {
                byte[] key = node.Value.GetValue(WorkflowDefinitionDispenser.WorkflowDefinitionHashCodeProperty) as byte[];
                if (key != null)
                {
                    this.hashtable.Add(key, node);
                }
                else
                {
                    Type type = node.Value.GetType();
                    this.hashtable.Add(type, node);
                }
            }

            public void Dispose()
            {
                foreach (LinkedListNode<Activity> node in this.hashtable.Values)
                {
                    try
                    {
                        node.Value.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            internal Activity GetDefinition(byte[] md5Codes)
            {
                LinkedListNode<Activity> node = this.hashtable[md5Codes] as LinkedListNode<Activity>;
                if (node != null)
                {
                    return node.Value;
                }
                return null;
            }

            internal Activity GetOrGenerateDefinition(Type type, string xomlText, string rulesText, byte[] md5Codes, bool initForRuntime, out bool exist)
            {
                LinkedListNode<Activity> node;
                object obj2;
                if (type != null)
                {
                    obj2 = type;
                }
                else
                {
                    obj2 = md5Codes;
                }
                try
                {
                    exist = false;
                    node = this.hashtable[obj2] as LinkedListNode<Activity>;
                    if (node != null)
                    {
                        lock (this.mruList)
                        {
                            node = this.hashtable[obj2] as LinkedListNode<Activity>;
                            if (node != null)
                            {
                                exist = true;
                                this.mruList.Remove(node);
                                this.mruList.AddFirst(node);
                            }
                            else
                            {
                                exist = false;
                            }
                        }
                    }
                    if (!exist)
                    {
                        lock (this.hashtable)
                        {
                            Activity activity;
                            node = this.hashtable[obj2] as LinkedListNode<Activity>;
                            if (node != null)
                            {
                                exist = true;
                                lock (this.mruList)
                                {
                                    this.mruList.Remove(node);
                                    this.mruList.AddFirst(node);
                                    goto Label_01B6;
                                }
                            }
                            exist = false;
                            if (type != null)
                            {
                                activity = this.dispenser.LoadRootActivity(type, true, initForRuntime);
                            }
                            else
                            {
                                activity = this.dispenser.LoadRootActivity(xomlText, rulesText, obj2 as byte[], true, initForRuntime);
                            }
                            lock (this.mruList)
                            {
                                if (this.size < this.capacity)
                                {
                                    this.size++;
                                }
                                else
                                {
                                    this.RemoveFromDictionary(this.mruList.Last.Value);
                                    this.mruList.RemoveLast();
                                }
                                node = new LinkedListNode<Activity>(activity);
                                this.AddToDictionary(node);
                                this.mruList.AddFirst(node);
                            }
                        }
                    }
                }
                finally
                {
                    Thread.MemoryBarrier();
                }
            Label_01B6:
                return node.Value;
            }

            internal void GetWorkflowDefinitions<K>(out ReadOnlyCollection<K> keys, out ReadOnlyCollection<Activity> values)
            {
                lock (this.hashtable)
                {
                    if (((typeof(K) == typeof(Type)) && (this.type == WorkflowDefinitionDispenser.CacheType.Type)) || ((typeof(K) == typeof(byte[])) && (this.type == WorkflowDefinitionDispenser.CacheType.Xoml)))
                    {
                        List<K> list = new List<K>();
                        foreach (K local in this.hashtable.Keys)
                        {
                            list.Add(local);
                        }
                        keys = new ReadOnlyCollection<K>(list);
                        List<Activity> list2 = new List<Activity>();
                        foreach (LinkedListNode<Activity> node in this.hashtable.Values)
                        {
                            list2.Add(node.Value);
                        }
                        values = new ReadOnlyCollection<Activity>(list2);
                    }
                    else
                    {
                        keys = null;
                        values = null;
                    }
                }
            }

            private void RemoveFromDictionary(Activity activity)
            {
                byte[] key = activity.GetValue(WorkflowDefinitionDispenser.WorkflowDefinitionHashCodeProperty) as byte[];
                if (key != null)
                {
                    this.hashtable.Remove(key);
                }
                else
                {
                    Type type = activity.GetType();
                    this.hashtable.Remove(type);
                }
            }
        }
    }
}

