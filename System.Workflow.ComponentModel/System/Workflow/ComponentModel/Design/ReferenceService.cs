namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Runtime;

    internal sealed class ReferenceService : IReferenceService, IDisposable
    {
        private ArrayList addedComponents;
        private static readonly Attribute[] Attributes = new Attribute[] { BrowsableAttribute.Yes };
        private ArrayList changedComponents;
        private IServiceProvider provider;
        private ArrayList references;
        private ArrayList removedComponents;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ReferenceService(IServiceProvider provider)
        {
            this.provider = provider;
        }

        private void CreateReferences(IComponent component)
        {
            this.CreateReferences(string.Empty, component, component);
        }

        private void CreateReferences(string trailingName, object reference, IComponent sitedComponent)
        {
            if (!object.ReferenceEquals(reference, null))
            {
                this.references.Add(new ReferenceHolder(trailingName, reference, sitedComponent));
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(reference, Attributes))
                {
                    object obj2 = null;
                    try
                    {
                        obj2 = descriptor.GetValue(reference);
                    }
                    catch
                    {
                    }
                    if (obj2 != null)
                    {
                        BrowsableAttribute[] customAttributes = (BrowsableAttribute[]) obj2.GetType().GetCustomAttributes(typeof(BrowsableAttribute), true);
                        if ((customAttributes.Length > 0) && customAttributes[0].Browsable)
                        {
                            this.CreateReferences(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { trailingName, descriptor.Name }), descriptor.GetValue(reference), sitedComponent);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if ((this.references != null) && (this.provider != null))
            {
                IComponentChangeService service = this.provider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                    service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                    service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
                    service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                }
                TypeDescriptor.Refreshed -= new RefreshEventHandler(this.OnComponentRefreshed);
                this.references = null;
                this.provider = null;
            }
        }

        private void EnsureReferences()
        {
            if (this.references == null)
            {
                if (this.provider == null)
                {
                    throw new ObjectDisposedException("IReferenceService");
                }
                IComponentChangeService service = this.provider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
                    service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                    service.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
                    service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                }
                TypeDescriptor.Refreshed += new RefreshEventHandler(this.OnComponentRefreshed);
                IContainer container = this.provider.GetService(typeof(IContainer)) as IContainer;
                if (container == null)
                {
                    throw new InvalidOperationException();
                }
                this.references = new ArrayList(container.Components.Count);
                foreach (IComponent component in container.Components)
                {
                    this.CreateReferences(component);
                }
            }
            else
            {
                if ((this.addedComponents != null) && (this.addedComponents.Count > 0))
                {
                    ArrayList list = new ArrayList(this.addedComponents);
                    foreach (IComponent component2 in list)
                    {
                        this.RemoveReferences(component2);
                        this.CreateReferences(component2);
                    }
                    this.addedComponents.Clear();
                }
                if ((this.removedComponents != null) && (this.removedComponents.Count > 0))
                {
                    ArrayList list2 = new ArrayList(this.removedComponents);
                    foreach (IComponent component3 in list2)
                    {
                        this.RemoveReferences(component3);
                    }
                    this.removedComponents.Clear();
                }
                if ((this.changedComponents != null) && (this.changedComponents.Count > 0))
                {
                    ArrayList list3 = new ArrayList(this.changedComponents);
                    foreach (IComponent component4 in list3)
                    {
                        this.RemoveReferences(component4);
                        this.CreateReferences(component4);
                    }
                    this.changedComponents.Clear();
                }
            }
        }

        ~ReferenceService()
        {
            this.Dispose(false);
        }

        private void OnComponentAdded(object sender, ComponentEventArgs cevent)
        {
            if (this.addedComponents == null)
            {
                this.addedComponents = new ArrayList();
            }
            this.addedComponents.Add(cevent.Component);
            if (this.removedComponents != null)
            {
                this.removedComponents.Remove(cevent.Component);
            }
            if (this.changedComponents != null)
            {
                this.changedComponents.Remove(cevent.Component);
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs cevent)
        {
            IComponent item = ((IReferenceService) this).GetComponent(cevent.Component);
            if (((item != null) && ((this.addedComponents == null) || !this.addedComponents.Contains(item))) && ((this.removedComponents == null) || !this.removedComponents.Contains(item)))
            {
                if (this.changedComponents == null)
                {
                    this.changedComponents = new ArrayList();
                    this.changedComponents.Add(item);
                }
                else if (!this.changedComponents.Contains(item))
                {
                    this.changedComponents.Add(item);
                }
            }
        }

        private void OnComponentRefreshed(RefreshEventArgs e)
        {
            if (e.ComponentChanged != null)
            {
                this.OnComponentChanged(this, new ComponentChangedEventArgs(e.ComponentChanged, null, null, null));
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs cevent)
        {
            if (this.removedComponents == null)
            {
                this.removedComponents = new ArrayList();
            }
            this.removedComponents.Add(cevent.Component);
            if (this.addedComponents != null)
            {
                this.addedComponents.Remove(cevent.Component);
            }
            if (this.changedComponents != null)
            {
                this.changedComponents.Remove(cevent.Component);
            }
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs cevent)
        {
            foreach (ReferenceHolder holder in this.references)
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
            if (this.references != null)
            {
                for (int i = this.references.Count - 1; i >= 0; i--)
                {
                    if (object.ReferenceEquals(((ReferenceHolder) this.references[i]).SitedComponent, component))
                    {
                        this.references.RemoveAt(i);
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
            foreach (ReferenceHolder holder in this.references)
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
            foreach (ReferenceHolder holder in this.references)
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
            foreach (ReferenceHolder holder in this.references)
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
            object[] objArray = new object[this.references.Count];
            for (int i = 0; i < objArray.Length; i++)
            {
                objArray[i] = ((ReferenceHolder) this.references[i]).Reference;
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
            ArrayList list = new ArrayList(this.references.Count);
            foreach (ReferenceHolder holder in this.references)
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

        private sealed class ReferenceHolder
        {
            private string fullName;
            private object reference;
            private IComponent sitedComponent;
            private string trailingName;

            internal ReferenceHolder(string trailingName, object reference, IComponent sitedComponent)
            {
                this.trailingName = trailingName;
                this.reference = reference;
                this.sitedComponent = sitedComponent;
            }

            internal void ResetName()
            {
                this.fullName = null;
            }

            internal string Name
            {
                get
                {
                    if (this.fullName == null)
                    {
                        if (((this.sitedComponent != null) && (this.sitedComponent.Site != null)) && (this.sitedComponent.Site.Name != null))
                        {
                            this.fullName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { this.sitedComponent.Site.Name, this.trailingName });
                        }
                        else
                        {
                            this.fullName = string.Empty;
                        }
                    }
                    return this.fullName;
                }
            }

            internal object Reference
            {
                get
                {
                    return this.reference;
                }
            }

            internal IComponent SitedComponent
            {
                get
                {
                    return this.sitedComponent;
                }
            }
        }
    }
}

