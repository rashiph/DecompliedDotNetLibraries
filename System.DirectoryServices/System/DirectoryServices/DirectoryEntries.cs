namespace System.DirectoryServices
{
    using System;
    using System.Collections;
    using System.DirectoryServices.Interop;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class DirectoryEntries : IEnumerable
    {
        private DirectoryEntry container;

        internal DirectoryEntries(DirectoryEntry parent)
        {
            this.container = parent;
        }

        public DirectoryEntry Add(string name, string schemaClassName)
        {
            this.CheckIsContainer();
            return new DirectoryEntry(this.container.ContainerObject.Create(schemaClassName, name), this.container.UsePropertyCache, this.container.GetUsername(), this.container.GetPassword(), this.container.AuthenticationType) { JustCreated = true };
        }

        private void CheckIsContainer()
        {
            if (!this.container.IsContainer)
            {
                throw new InvalidOperationException(Res.GetString("DSNotAContainer", new object[] { this.container.Path }));
            }
        }

        public DirectoryEntry Find(string name)
        {
            return this.Find(name, null);
        }

        public DirectoryEntry Find(string name, string schemaClassName)
        {
            this.CheckIsContainer();
            object adsObject = null;
            try
            {
                adsObject = this.container.ContainerObject.GetObject(schemaClassName, name);
            }
            catch (COMException exception)
            {
                throw COMExceptionHelper.CreateFormattedComException(exception);
            }
            return new DirectoryEntry(adsObject, this.container.UsePropertyCache, this.container.GetUsername(), this.container.GetPassword(), this.container.AuthenticationType);
        }

        public IEnumerator GetEnumerator()
        {
            return new ChildEnumerator(this.container);
        }

        public void Remove(DirectoryEntry entry)
        {
            this.CheckIsContainer();
            try
            {
                this.container.ContainerObject.Delete(entry.SchemaClassName, entry.Name);
            }
            catch (COMException exception)
            {
                throw COMExceptionHelper.CreateFormattedComException(exception);
            }
        }

        public SchemaNameCollection SchemaFilter
        {
            get
            {
                this.CheckIsContainer();
                SchemaNameCollection.FilterDelegateWrapper wrapper = new SchemaNameCollection.FilterDelegateWrapper(this.container.ContainerObject);
                return new SchemaNameCollection(wrapper.Getter, wrapper.Setter);
            }
        }

        private class ChildEnumerator : IEnumerator
        {
            private DirectoryEntry container;
            private DirectoryEntry currentEntry;
            private SafeNativeMethods.EnumVariant enumVariant;

            internal ChildEnumerator(DirectoryEntry container)
            {
                this.container = container;
                if (container.IsContainer)
                {
                    this.enumVariant = new SafeNativeMethods.EnumVariant((SafeNativeMethods.IEnumVariant) container.ContainerObject._NewEnum);
                }
            }

            public bool MoveNext()
            {
                if (this.enumVariant == null)
                {
                    return false;
                }
                this.currentEntry = null;
                return this.enumVariant.GetNext();
            }

            public void Reset()
            {
                if (this.enumVariant != null)
                {
                    try
                    {
                        this.enumVariant.Reset();
                    }
                    catch (NotImplementedException)
                    {
                        this.enumVariant = new SafeNativeMethods.EnumVariant((SafeNativeMethods.IEnumVariant) this.container.ContainerObject._NewEnum);
                    }
                    this.currentEntry = null;
                }
            }

            public DirectoryEntry Current
            {
                get
                {
                    if (this.enumVariant == null)
                    {
                        throw new InvalidOperationException(Res.GetString("DSNoCurrentChild"));
                    }
                    if (this.currentEntry == null)
                    {
                        this.currentEntry = new DirectoryEntry(this.enumVariant.GetValue(), this.container.UsePropertyCache, this.container.GetUsername(), this.container.GetPassword(), this.container.AuthenticationType);
                    }
                    return this.currentEntry;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }
    }
}

