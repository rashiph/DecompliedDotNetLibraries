namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class ExecutionProperties : IEnumerable<KeyValuePair<string, object>>, IEnumerable
    {
        private ActivityContext context;
        private IdSpace currentIdSpace;
        private static IEnumerable<KeyValuePair<string, object>> emptyKeyValues;
        private ExecutionPropertyManager properties;
        private System.Activities.ActivityInstance scope;

        internal ExecutionProperties(ActivityContext currentContext, System.Activities.ActivityInstance scope, ExecutionPropertyManager properties)
        {
            this.context = currentContext;
            this.scope = scope;
            this.properties = properties;
            if (this.context != null)
            {
                this.currentIdSpace = this.context.Activity.MemberOf;
            }
        }

        public void Add(string name, object property)
        {
            this.Add(name, property, false, false);
        }

        public void Add(string name, object property, bool onlyVisibleToPublicChildren)
        {
            this.Add(name, property, false, onlyVisibleToPublicChildren);
        }

        internal void Add(string name, object property, bool skipValidations, bool onlyVisibleToPublicChildren)
        {
            if (!skipValidations)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("name");
                }
                if (property == null)
                {
                    throw FxTrace.Exception.ArgumentNull("property");
                }
                this.ThrowIfActivityExecutionContextDisposed();
                this.ThrowIfChildrenAreExecuting();
            }
            if (this.properties != null)
            {
                this.properties.ThrowIfAlreadyDefined(name, this.scope);
            }
            IPropertyRegistrationCallback callback = property as IPropertyRegistrationCallback;
            if (callback != null)
            {
                callback.Register(new RegistrationContext(this.properties, this.currentIdSpace));
            }
            if (this.properties == null)
            {
                this.properties = new ExecutionPropertyManager(this.scope);
            }
            else if (!this.properties.IsOwner(this.scope))
            {
                this.properties = new ExecutionPropertyManager(this.scope, this.properties);
            }
            IdSpace visibility = null;
            if (onlyVisibleToPublicChildren)
            {
                visibility = this.currentIdSpace;
            }
            this.properties.Add(name, property, visibility);
        }

        public object Find(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }
            if (this.properties == null)
            {
                return null;
            }
            return this.properties.GetProperty(name, this.currentIdSpace);
        }

        internal object FindAtCurrentScope(string name)
        {
            if ((this.properties != null) && this.properties.IsOwner(this.scope))
            {
                return this.properties.GetPropertyAtCurrentScope(name);
            }
            return null;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.GetKeyValues().GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            if (this.properties != null)
            {
                return this.properties.GetFlattenedProperties(this.currentIdSpace);
            }
            return EmptyKeyValues;
        }

        public bool Remove(string name)
        {
            return this.Remove(name, false);
        }

        internal bool Remove(string name, bool skipValidations)
        {
            if (!skipValidations)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("name");
                }
                this.ThrowIfActivityExecutionContextDisposed();
            }
            if ((this.properties != null) && this.properties.IsOwner(this.scope))
            {
                object propertyAtCurrentScope = this.properties.GetPropertyAtCurrentScope(name);
                if (propertyAtCurrentScope != null)
                {
                    if (!skipValidations)
                    {
                        Handle handle = propertyAtCurrentScope as Handle;
                        if ((handle == null) || !handle.CanBeRemovedWithExecutingChildren)
                        {
                            this.ThrowIfChildrenAreExecuting();
                        }
                    }
                    this.properties.Remove(name);
                    IPropertyRegistrationCallback callback = propertyAtCurrentScope as IPropertyRegistrationCallback;
                    if (callback != null)
                    {
                        callback.Unregister(new RegistrationContext(this.properties, this.currentIdSpace));
                    }
                    return true;
                }
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetKeyValues().GetEnumerator();
        }

        private void ThrowIfActivityExecutionContextDisposed()
        {
            if (this.context.IsDisposed)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.AECForPropertiesHasBeenDisposed));
            }
        }

        private void ThrowIfChildrenAreExecuting()
        {
            if (this.scope.HasChildren)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotAddOrRemoveWithChildren));
            }
        }

        private static IEnumerable<KeyValuePair<string, object>> EmptyKeyValues
        {
            get
            {
                if (emptyKeyValues == null)
                {
                    emptyKeyValues = new KeyValuePair<string, object>[0];
                }
                return emptyKeyValues;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.properties == null);
            }
        }
    }
}

