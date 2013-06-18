namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;

    internal sealed class ReferenceService : IReferenceService, IDisposable
    {
        private ArrayList _addedComponents;
        private static readonly Attribute[] _attributes = new Attribute[] { DesignerSerializationVisibilityAttribute.Content };
        private bool _populating;
        private IServiceProvider _provider;
        private ArrayList _references;
        private ArrayList _removedComponents;

        internal ReferenceService(IServiceProvider provider)
        {
            this._provider = provider;
        }

        private void CreateReferences(IComponent component)
        {
            this.CreateReferences(string.Empty, component, component);
        }

        private void CreateReferences(string trailingName, object reference, IComponent sitedComponent)
        {
            if (!object.ReferenceEquals(reference, null))
            {
                this._references.Add(new ReferenceHolder(trailingName, reference, sitedComponent));
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(reference, _attributes))
                {
                    if (descriptor.IsReadOnly)
                    {
                        this.CreateReferences(string.Format(CultureInfo.CurrentCulture, "{0}.{1}", new object[] { trailingName, descriptor.Name }), descriptor.GetValue(reference), sitedComponent);
                    }
                }
            }
        }

        private void EnsureReferences()
        {
            if (this._references == null)
            {
                if (this._provider == null)
                {
                    throw new ObjectDisposedException("IReferenceService");
                }
                IComponentChangeService service = this._provider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
                    service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                    service.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
                }
                IContainer container = this._provider.GetService(typeof(IContainer)) as IContainer;
                if (container == null)
                {
                    throw new InvalidOperationException();
                }
                this._references = new ArrayList(container.Components.Count);
                foreach (IComponent component in container.Components)
                {
                    this.CreateReferences(component);
                }
            }
            else if (!this._populating)
            {
                this._populating = true;
                try
                {
                    if ((this._addedComponents != null) && (this._addedComponents.Count > 0))
                    {
                        foreach (IComponent component2 in this._addedComponents)
                        {
                            this.RemoveReferences(component2);
                            this.CreateReferences(component2);
                        }
                        this._addedComponents.Clear();
                    }
                    if ((this._removedComponents != null) && (this._removedComponents.Count > 0))
                    {
                        foreach (IComponent component3 in this._removedComponents)
                        {
                            this.RemoveReferences(component3);
                        }
                        this._removedComponents.Clear();
                    }
                }
                finally
                {
                    this._populating = false;
                }
            }
        }

        private void OnComponentAdded(object sender, ComponentEventArgs cevent)
        {
            if (this._addedComponents == null)
            {
                this._addedComponents = new ArrayList();
            }
            IComponent component = cevent.Component;
            if (!(component.Site is INestedSite))
            {
                this._addedComponents.Add(component);
                if (this._removedComponents != null)
                {
                    this._removedComponents.Remove(component);
                }
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs cevent)
        {
            if (this._removedComponents == null)
            {
                this._removedComponents = new ArrayList();
            }
            IComponent component = cevent.Component;
            if (!(component.Site is INestedSite))
            {
                this._removedComponents.Add(component);
                if (this._addedComponents != null)
                {
                    this._addedComponents.Remove(component);
                }
            }
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs cevent)
        {
            foreach (ReferenceHolder holder in this._references)
            {
                if (object.ReferenceEquals(holder.SitedComponent, cevent.Component))
                {
                    holder.ResetName();
                    break;
                }
            }
        }

        private void RemoveReferences(IComponent component)
        {
            if (this._references != null)
            {
                for (int i = this._references.Count - 1; i >= 0; i--)
                {
                    if (object.ReferenceEquals(((ReferenceHolder) this._references[i]).SitedComponent, component))
                    {
                        this._references.RemoveAt(i);
                    }
                }
            }
        }

        IComponent IReferenceService.GetComponent(object reference)
        {
            if (object.ReferenceEquals(reference, null))
            {
                throw new ArgumentNullException("reference");
            }
            this.EnsureReferences();
            foreach (ReferenceHolder holder in this._references)
            {
                if (object.ReferenceEquals(holder.Reference, reference))
                {
                    return holder.SitedComponent;
                }
            }
            return null;
        }

        string IReferenceService.GetName(object reference)
        {
            if (object.ReferenceEquals(reference, null))
            {
                throw new ArgumentNullException("reference");
            }
            this.EnsureReferences();
            foreach (ReferenceHolder holder in this._references)
            {
                if (object.ReferenceEquals(holder.Reference, reference))
                {
                    return holder.Name;
                }
            }
            return null;
        }

        object IReferenceService.GetReference(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.EnsureReferences();
            foreach (ReferenceHolder holder in this._references)
            {
                if (string.Equals(holder.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return holder.Reference;
                }
            }
            return null;
        }

        object[] IReferenceService.GetReferences()
        {
            this.EnsureReferences();
            object[] objArray = new object[this._references.Count];
            for (int i = 0; i < objArray.Length; i++)
            {
                objArray[i] = ((ReferenceHolder) this._references[i]).Reference;
            }
            return objArray;
        }

        object[] IReferenceService.GetReferences(Type baseType)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException("baseType");
            }
            this.EnsureReferences();
            ArrayList list = new ArrayList(this._references.Count);
            foreach (ReferenceHolder holder in this._references)
            {
                object reference = holder.Reference;
                if (baseType.IsAssignableFrom(reference.GetType()))
                {
                    list.Add(reference);
                }
            }
            object[] array = new object[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        void IDisposable.Dispose()
        {
            if ((this._references != null) && (this._provider != null))
            {
                IComponentChangeService service = this._provider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                    service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                    service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
                }
                this._references = null;
                this._provider = null;
            }
        }

        private sealed class ReferenceHolder
        {
            private string _fullName;
            private object _reference;
            private IComponent _sitedComponent;
            private string _trailingName;

            internal ReferenceHolder(string trailingName, object reference, IComponent sitedComponent)
            {
                this._trailingName = trailingName;
                this._reference = reference;
                this._sitedComponent = sitedComponent;
            }

            internal void ResetName()
            {
                this._fullName = null;
            }

            internal string Name
            {
                get
                {
                    if (this._fullName == null)
                    {
                        if (this._sitedComponent != null)
                        {
                            string componentName = TypeDescriptor.GetComponentName(this._sitedComponent);
                            if (componentName != null)
                            {
                                this._fullName = string.Format(CultureInfo.CurrentCulture, "{0}{1}", new object[] { componentName, this._trailingName });
                            }
                        }
                        if (this._fullName == null)
                        {
                            this._fullName = string.Empty;
                        }
                    }
                    return this._fullName;
                }
            }

            internal object Reference
            {
                get
                {
                    return this._reference;
                }
            }

            internal IComponent SitedComponent
            {
                get
                {
                    return this._sitedComponent;
                }
            }
        }
    }
}

