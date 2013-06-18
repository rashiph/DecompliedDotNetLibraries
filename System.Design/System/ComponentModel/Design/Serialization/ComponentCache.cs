namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class ComponentCache : IDisposable
    {
        private Dictionary<object, Entry> cache;
        private bool enabled = true;
        private IDesignerSerializationManager serManager;

        internal ComponentCache(IDesignerSerializationManager manager)
        {
            this.serManager = manager;
            IComponentChangeService service = (IComponentChangeService) manager.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentChanging += new ComponentChangingEventHandler(this.OnComponentChanging);
                service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                service.ComponentRemoving += new ComponentEventHandler(this.OnComponentRemove);
                service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemove);
                service.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
            }
            DesignerOptionService service2 = manager.GetService(typeof(DesignerOptionService)) as DesignerOptionService;
            object obj2 = null;
            if (service2 != null)
            {
                PropertyDescriptor descriptor = service2.Options.Properties["UseOptimizedCodeGeneration"];
                if (descriptor != null)
                {
                    obj2 = descriptor.GetValue(null);
                }
                if ((obj2 != null) && (obj2 is bool))
                {
                    this.enabled = (bool) obj2;
                }
            }
        }

        internal bool ContainsLocalName(string name)
        {
            if (this.cache != null)
            {
                foreach (KeyValuePair<object, Entry> pair in this.cache)
                {
                    List<string> localNames = pair.Value.LocalNames;
                    if ((localNames != null) && localNames.Contains(name))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Dispose()
        {
            if (this.serManager != null)
            {
                IComponentChangeService service = (IComponentChangeService) this.serManager.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentChanging -= new ComponentChangingEventHandler(this.OnComponentChanging);
                    service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                    service.ComponentRemoving -= new ComponentEventHandler(this.OnComponentRemove);
                    service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemove);
                    service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
                }
            }
        }

        internal Entry GetEntryAll(object component)
        {
            Entry entry = null;
            if ((this.cache != null) && this.cache.TryGetValue(component, out entry))
            {
                return entry;
            }
            return null;
        }

        private void OnComponentChanged(object source, ComponentChangedEventArgs ce)
        {
            if (this.cache != null)
            {
                if (ce.Component != null)
                {
                    this.RemoveEntry(ce.Component);
                    if (!(ce.Component is IComponent) && (this.serManager != null))
                    {
                        IReferenceService service = this.serManager.GetService(typeof(IReferenceService)) as IReferenceService;
                        if (service != null)
                        {
                            IComponent component = service.GetComponent(ce.Component);
                            if (component != null)
                            {
                                this.RemoveEntry(component);
                            }
                            else
                            {
                                this.cache.Clear();
                            }
                        }
                    }
                }
                else
                {
                    this.cache.Clear();
                }
            }
        }

        private void OnComponentChanging(object source, ComponentChangingEventArgs ce)
        {
            if (this.cache != null)
            {
                if (ce.Component != null)
                {
                    this.RemoveEntry(ce.Component);
                    if (!(ce.Component is IComponent) && (this.serManager != null))
                    {
                        IReferenceService service = this.serManager.GetService(typeof(IReferenceService)) as IReferenceService;
                        if (service != null)
                        {
                            IComponent component = service.GetComponent(ce.Component);
                            if (component != null)
                            {
                                this.RemoveEntry(component);
                            }
                            else
                            {
                                this.cache.Clear();
                            }
                        }
                    }
                }
                else
                {
                    this.cache.Clear();
                }
            }
        }

        private void OnComponentRemove(object source, ComponentEventArgs ce)
        {
            if (this.cache != null)
            {
                if ((ce.Component != null) && !(ce.Component is IExtenderProvider))
                {
                    this.RemoveEntry(ce.Component);
                }
                else
                {
                    this.cache.Clear();
                }
            }
        }

        private void OnComponentRename(object source, ComponentRenameEventArgs args)
        {
            if (this.cache != null)
            {
                this.cache.Clear();
            }
        }

        internal void RemoveEntry(object component)
        {
            Entry entry = null;
            if ((this.cache != null) && this.cache.TryGetValue(component, out entry))
            {
                if (entry.Tracking)
                {
                    this.cache.Clear();
                }
                else
                {
                    this.cache.Remove(component);
                    if (entry.Dependencies != null)
                    {
                        foreach (object obj2 in entry.Dependencies)
                        {
                            this.RemoveEntry(obj2);
                        }
                    }
                }
            }
        }

        internal bool Enabled
        {
            get
            {
                return this.enabled;
            }
        }

        internal Entry this[object component]
        {
            get
            {
                Entry entry;
                if (component == null)
                {
                    throw new ArgumentNullException("component");
                }
                if ((((this.cache != null) && this.cache.TryGetValue(component, out entry)) && ((entry != null) && entry.Valid)) && this.Enabled)
                {
                    return entry;
                }
                return null;
            }
            set
            {
                if ((this.cache == null) && this.Enabled)
                {
                    this.cache = new Dictionary<object, Entry>();
                }
                if ((this.cache != null) && (component is IComponent))
                {
                    if ((value != null) && (value.Component == null))
                    {
                        value.Component = component;
                    }
                    this.cache[component] = value;
                }
            }
        }

        internal sealed class Entry
        {
            private ComponentCache cache;
            public object Component;
            private List<object> dependencies;
            private List<string> localNames;
            private List<ComponentCache.ResourceEntry> metadata;
            private List<ComponentCache.ResourceEntry> resources;
            public CodeStatementCollection Statements;
            private bool tracking;
            private bool valid;

            internal Entry(ComponentCache cache)
            {
                this.cache = cache;
                this.valid = true;
            }

            public void AddDependency(object dep)
            {
                if (this.dependencies == null)
                {
                    this.dependencies = new List<object>();
                }
                if (!this.dependencies.Contains(dep))
                {
                    this.dependencies.Add(dep);
                }
            }

            internal void AddLocalName(string name)
            {
                if (this.localNames == null)
                {
                    this.localNames = new List<string>();
                }
                this.localNames.Add(name);
            }

            public void AddMetadata(ComponentCache.ResourceEntry re)
            {
                if (this.metadata == null)
                {
                    this.metadata = new List<ComponentCache.ResourceEntry>();
                }
                this.metadata.Add(re);
            }

            public void AddResource(ComponentCache.ResourceEntry re)
            {
                if (this.resources == null)
                {
                    this.resources = new List<ComponentCache.ResourceEntry>();
                }
                this.resources.Add(re);
            }

            public List<object> Dependencies
            {
                get
                {
                    return this.dependencies;
                }
            }

            internal List<string> LocalNames
            {
                get
                {
                    return this.localNames;
                }
            }

            public ICollection<ComponentCache.ResourceEntry> Metadata
            {
                get
                {
                    return this.metadata;
                }
            }

            public ICollection<ComponentCache.ResourceEntry> Resources
            {
                get
                {
                    return this.resources;
                }
            }

            internal bool Tracking
            {
                get
                {
                    return this.tracking;
                }
                set
                {
                    this.tracking = value;
                }
            }

            internal bool Valid
            {
                get
                {
                    return this.valid;
                }
                set
                {
                    this.valid = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ResourceEntry
        {
            public bool ForceInvariant;
            public bool EnsureInvariant;
            public bool ShouldSerializeValue;
            public string Name;
            public object Value;
            public System.ComponentModel.PropertyDescriptor PropertyDescriptor;
            public System.ComponentModel.Design.Serialization.ExpressionContext ExpressionContext;
        }
    }
}

