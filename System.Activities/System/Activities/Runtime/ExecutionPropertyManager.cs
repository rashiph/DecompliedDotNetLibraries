namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class ExecutionPropertyManager
    {
        [DataMember(EmitDefaultValue=false)]
        private int exclusiveHandleCount;
        private object lastProperty;
        private string lastPropertyName;
        private IdSpace lastPropertyVisibility;
        private System.Activities.ActivityInstance owningInstance;
        private bool ownsThreadPropertiesList;
        [DataMember(EmitDefaultValue=false)]
        private Dictionary<string, ExecutionProperty> properties;
        private ExecutionPropertyManager rootPropertyManager;
        private List<ExecutionProperty> threadProperties;

        public ExecutionPropertyManager(System.Activities.ActivityInstance owningInstance)
        {
            this.owningInstance = owningInstance;
            this.properties = new Dictionary<string, ExecutionProperty>();
            if (owningInstance.HasChildren)
            {
                System.Activities.ActivityInstance previousOwner = (owningInstance.PropertyManager != null) ? owningInstance.PropertyManager.owningInstance : null;
                ActivityUtilities.ProcessActivityInstanceTree(owningInstance, null, (instance, executor) => this.AttachPropertyManager(instance, previousOwner));
            }
            else
            {
                owningInstance.PropertyManager = this;
            }
        }

        public ExecutionPropertyManager(System.Activities.ActivityInstance owningInstance, ExecutionPropertyManager parentPropertyManager) : this(owningInstance)
        {
            this.threadProperties = parentPropertyManager.threadProperties;
            if (owningInstance.Parent == null)
            {
                this.rootPropertyManager = parentPropertyManager.rootPropertyManager;
            }
        }

        internal ExecutionPropertyManager(System.Activities.ActivityInstance owningInstance, Dictionary<string, ExecutionProperty> properties)
        {
            this.owningInstance = owningInstance;
            this.properties = properties;
            if (owningInstance == null)
            {
                this.rootPropertyManager = this;
            }
        }

        public void Add(string name, object property, IdSpace visibility)
        {
            ExecutionProperty property2 = new ExecutionProperty(name, property, visibility);
            this.properties.Add(name, property2);
            if (this.lastPropertyName == name)
            {
                this.lastProperty = property;
            }
            if (property is ExclusiveHandle)
            {
                this.exclusiveHandleCount++;
                this.UpdateChildExclusiveHandleCounts(1);
            }
            if (property is IExecutionProperty)
            {
                this.AddIExecutionProperty(property2, false);
            }
        }

        private void AddIExecutionProperty(ExecutionProperty property, bool isDeserializationFixup)
        {
            bool flag = !isDeserializationFixup;
            if (this.threadProperties == null)
            {
                this.threadProperties = new List<ExecutionProperty>(1);
                this.ownsThreadPropertiesList = true;
            }
            else if (!this.ownsThreadPropertiesList)
            {
                List<ExecutionProperty> list = new List<ExecutionProperty>(this.threadProperties.Count);
                for (int i = 0; i < this.threadProperties.Count; i++)
                {
                    ExecutionProperty item = this.threadProperties[i];
                    if (item.Name == property.Name)
                    {
                        if (flag)
                        {
                            item.ShouldBeRemovedAfterCleanup = true;
                            list.Add(item);
                        }
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
                this.threadProperties = list;
                this.ownsThreadPropertiesList = true;
            }
            else
            {
                for (int j = this.threadProperties.Count - 1; j >= 0; j--)
                {
                    ExecutionProperty property3 = this.threadProperties[j];
                    if (property3.Name == property.Name)
                    {
                        if (flag)
                        {
                            property3.ShouldBeRemovedAfterCleanup = true;
                        }
                        else
                        {
                            this.threadProperties.RemoveAt(j);
                        }
                        break;
                    }
                }
            }
            property.ShouldSkipNextCleanup = flag;
            this.threadProperties.Add(property);
        }

        private void AddProperties(IDictionary<string, ExecutionProperty> properties, IDictionary<string, object> flattenedProperties, IdSpace currentIdSpace)
        {
            foreach (KeyValuePair<string, ExecutionProperty> pair in properties)
            {
                if ((!pair.Value.IsRemoved && !flattenedProperties.ContainsKey(pair.Key)) && (!pair.Value.HasRestrictedVisibility || (pair.Value.Visibility == currentIdSpace)))
                {
                    flattenedProperties.Add(pair.Key, pair.Value.Property);
                }
            }
        }

        private bool AttachPropertyManager(System.Activities.ActivityInstance instance, System.Activities.ActivityInstance previousOwner)
        {
            if ((instance.PropertyManager != null) && (instance.PropertyManager.owningInstance != previousOwner))
            {
                return false;
            }
            instance.PropertyManager = this;
            return true;
        }

        public void CleanupWorkflowThread(ref Exception abortException)
        {
            if (this.threadProperties != null)
            {
                for (int i = this.threadProperties.Count - 1; i >= 0; i--)
                {
                    ExecutionProperty property = this.threadProperties[i];
                    if (property.ShouldSkipNextCleanup)
                    {
                        property.ShouldSkipNextCleanup = false;
                    }
                    else
                    {
                        IExecutionProperty property2 = (IExecutionProperty) property.Property;
                        try
                        {
                            property2.CleanupWorkflowThread();
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            abortException = exception;
                        }
                    }
                    if (property.ShouldBeRemovedAfterCleanup)
                    {
                        this.threadProperties.RemoveAt(i);
                        property.ShouldBeRemovedAfterCleanup = false;
                    }
                }
            }
        }

        internal List<T> FindAll<T>() where T: class
        {
            ExecutionPropertyManager currentManager = this;
            List<T> list = null;
            while (currentManager != null)
            {
                foreach (ExecutionProperty property in currentManager.Properties.Values)
                {
                    if (property.Property is T)
                    {
                        if (list == null)
                        {
                            list = new List<T>();
                        }
                        list.Add((T) property.Property);
                    }
                }
                currentManager = GetParent(currentManager);
            }
            return list;
        }

        public IEnumerable<KeyValuePair<string, object>> GetFlattenedProperties(IdSpace currentIdSpace)
        {
            ExecutionPropertyManager currentManager = this;
            Dictionary<string, object> flattenedProperties = new Dictionary<string, object>();
            while (currentManager != null)
            {
                this.AddProperties(currentManager.Properties, flattenedProperties, currentIdSpace);
                currentManager = GetParent(currentManager);
            }
            return flattenedProperties;
        }

        private static ExecutionPropertyManager GetParent(ExecutionPropertyManager currentManager)
        {
            if (currentManager.owningInstance == null)
            {
                return null;
            }
            if (currentManager.owningInstance.Parent != null)
            {
                return currentManager.owningInstance.Parent.PropertyManager;
            }
            return currentManager.rootPropertyManager;
        }

        public object GetProperty(string name, IdSpace currentIdSpace)
        {
            if ((this.lastPropertyName == name) && ((this.lastPropertyVisibility == null) || (this.lastPropertyVisibility == currentIdSpace)))
            {
                return this.lastProperty;
            }
            for (ExecutionPropertyManager manager = this; manager != null; manager = GetParent(manager))
            {
                ExecutionProperty property;
                if ((manager.properties.TryGetValue(name, out property) && !property.IsRemoved) && (!property.HasRestrictedVisibility || (property.Visibility == currentIdSpace)))
                {
                    this.lastPropertyName = name;
                    this.lastProperty = property.Property;
                    this.lastPropertyVisibility = property.Visibility;
                    return this.lastProperty;
                }
            }
            return null;
        }

        public object GetPropertyAtCurrentScope(string name)
        {
            ExecutionProperty property;
            if (this.properties.TryGetValue(name, out property))
            {
                return property.Property;
            }
            return null;
        }

        public bool IsOwner(System.Activities.ActivityInstance instance)
        {
            return (this.owningInstance == instance);
        }

        public void OnDeserialized(System.Activities.ActivityInstance owner, System.Activities.ActivityInstance parent, IdSpace visibility, ActivityExecutor executor)
        {
            this.owningInstance = owner;
            if (parent != null)
            {
                if (parent.PropertyManager != null)
                {
                    this.threadProperties = parent.PropertyManager.threadProperties;
                }
            }
            else
            {
                this.rootPropertyManager = executor.RootPropertyManager;
            }
            foreach (ExecutionProperty property in this.properties.Values)
            {
                if (property.Property is IExecutionProperty)
                {
                    this.AddIExecutionProperty(property, true);
                }
                if (property.HasRestrictedVisibility)
                {
                    property.Visibility = visibility;
                }
            }
        }

        private void ProcessChildrenForExclusiveHandles(HybridCollection<System.Activities.ActivityInstance> children, int amountToUpdate, ref Queue<HybridCollection<System.Activities.ActivityInstance>> toProcess)
        {
            for (int i = 0; i < children.Count; i++)
            {
                System.Activities.ActivityInstance instance = children[i];
                ExecutionPropertyManager propertyManager = instance.PropertyManager;
                if (propertyManager.IsOwner(instance))
                {
                    propertyManager.exclusiveHandleCount += amountToUpdate;
                }
                HybridCollection<System.Activities.ActivityInstance> rawChildren = instance.GetRawChildren();
                if ((rawChildren != null) && (rawChildren.Count > 0))
                {
                    if (toProcess == null)
                    {
                        toProcess = new Queue<HybridCollection<System.Activities.ActivityInstance>>();
                    }
                    toProcess.Enqueue(rawChildren);
                }
            }
        }

        public void Remove(string name)
        {
            ExecutionProperty item = this.properties[name];
            if (item.Property is IExecutionProperty)
            {
                this.threadProperties.Remove(item);
            }
            this.properties.Remove(name);
            if (item.Property is ExclusiveHandle)
            {
                this.exclusiveHandleCount--;
                this.UpdateChildExclusiveHandleCounts(-1);
            }
            if (this.lastPropertyName == name)
            {
                this.lastPropertyName = null;
                this.lastProperty = null;
            }
        }

        public void SetupWorkflowThread()
        {
            if (this.threadProperties != null)
            {
                for (int i = 0; i < this.threadProperties.Count; i++)
                {
                    ExecutionProperty property = this.threadProperties[i];
                    property.ShouldSkipNextCleanup = false;
                    ((IExecutionProperty) property.Property).SetupWorkflowThread();
                }
            }
        }

        internal bool ShouldSerialize(System.Activities.ActivityInstance instance)
        {
            return (this.IsOwner(instance) && (this.properties.Count > 0));
        }

        public void ThrowIfAlreadyDefined(string name, System.Activities.ActivityInstance executingInstance)
        {
            if ((executingInstance == this.owningInstance) && this.properties.ContainsKey(name))
            {
                throw FxTrace.Exception.Argument("name", System.Activities.SR.ExecutionPropertyAlreadyDefined(name));
            }
        }

        public void UnregisterProperties(System.Activities.ActivityInstance completedInstance, IdSpace currentIdSpace)
        {
            this.UnregisterProperties(completedInstance, currentIdSpace, false);
        }

        public void UnregisterProperties(System.Activities.ActivityInstance completedInstance, IdSpace currentIdSpace, bool ignoreExceptions)
        {
            if (this.IsOwner(completedInstance))
            {
                RegistrationContext context = new RegistrationContext(this, currentIdSpace);
                foreach (ExecutionProperty property in this.properties.Values)
                {
                    property.IsRemoved = true;
                    IPropertyRegistrationCallback callback = property.Property as IPropertyRegistrationCallback;
                    if (callback != null)
                    {
                        try
                        {
                            callback.Unregister(context);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception) || !ignoreExceptions)
                            {
                                throw;
                            }
                        }
                    }
                }
                this.properties.Clear();
            }
        }

        private void UpdateChildExclusiveHandleCounts(int amountToUpdate)
        {
            Queue<HybridCollection<System.Activities.ActivityInstance>> toProcess = null;
            HybridCollection<System.Activities.ActivityInstance> rawChildren = this.owningInstance.GetRawChildren();
            if ((rawChildren != null) && (rawChildren.Count > 0))
            {
                this.ProcessChildrenForExclusiveHandles(rawChildren, amountToUpdate, ref toProcess);
                if (toProcess != null)
                {
                    while (toProcess.Count > 0)
                    {
                        rawChildren = toProcess.Dequeue();
                        this.ProcessChildrenForExclusiveHandles(rawChildren, amountToUpdate, ref toProcess);
                    }
                }
            }
        }

        internal bool HasExclusiveHandlesInScope
        {
            get
            {
                return (this.exclusiveHandleCount > 0);
            }
        }

        internal Dictionary<string, ExecutionProperty> Properties
        {
            get
            {
                return this.properties;
            }
        }

        [DataContract]
        internal class ExecutionProperty
        {
            public ExecutionProperty(string name, object property, IdSpace visibility)
            {
                this.Name = name;
                this.Property = property;
                if (visibility != null)
                {
                    this.Visibility = visibility;
                    this.HasRestrictedVisibility = true;
                }
            }

            [DataMember(EmitDefaultValue=false)]
            public bool HasRestrictedVisibility { get; private set; }

            public bool IsRemoved { get; set; }

            [DataMember]
            public string Name { get; private set; }

            [DataMember]
            public object Property { get; private set; }

            public bool ShouldBeRemovedAfterCleanup { get; set; }

            public bool ShouldSkipNextCleanup { get; set; }

            public IdSpace Visibility { get; set; }
        }
    }
}

