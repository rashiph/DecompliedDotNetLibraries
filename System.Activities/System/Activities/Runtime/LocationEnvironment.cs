namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal sealed class LocationEnvironment : ActivityInstanceMap.IActivityReference
    {
        private ActivityExecutor executor;
        [DataMember(EmitDefaultValue=false)]
        private List<Handle> handles;
        private bool hasHandles;
        [DataMember(EmitDefaultValue=false)]
        private bool hasMappableLocations;
        [DataMember(EmitDefaultValue=false)]
        private bool hasOwnerCompleted;
        private bool isDisposed;
        [DataMember(EmitDefaultValue=false)]
        private Location[] locations;
        [DataMember(EmitDefaultValue=false)]
        private LocationEnvironment parent;
        [DataMember(EmitDefaultValue=false)]
        private int referenceCountMinusOne;
        [DataMember(EmitDefaultValue=false)]
        private Location singleLocation;

        internal LocationEnvironment(ActivityExecutor executor, Activity definition)
        {
            this.executor = executor;
            this.Definition = definition;
        }

        internal LocationEnvironment(ActivityExecutor executor, Activity definition, LocationEnvironment parent, int capacity) : this(executor, definition)
        {
            this.parent = parent;
            if (capacity > 1)
            {
                this.locations = new Location[capacity];
            }
        }

        internal void AddHandle(Handle handleToAdd)
        {
            if (this.handles == null)
            {
                this.handles = new List<Handle>();
            }
            this.handles.Add(handleToAdd);
            this.hasHandles = true;
        }

        internal void AddReference()
        {
            this.referenceCountMinusOne++;
        }

        private void CleanupMappedLocations()
        {
            if (this.hasMappableLocations)
            {
                if (this.singleLocation != null)
                {
                    this.MappableObjectManager.Unregister(this.singleLocation);
                }
                else if (this.locations != null)
                {
                    for (int i = 0; i < this.locations.Length; i++)
                    {
                        Location location = this.locations[i];
                        if (location.CanBeMapped)
                        {
                            this.MappableObjectManager.Unregister(location);
                        }
                    }
                }
            }
        }

        internal void CollapseTemporaryResolutionLocations()
        {
            if (this.locations == null)
            {
                if ((this.singleLocation != null) && object.ReferenceEquals(this.singleLocation.TemporaryResolutionEnvironment, this))
                {
                    if (this.singleLocation.Value == null)
                    {
                        this.singleLocation = (Location) this.singleLocation.CreateDefaultValue();
                    }
                    else
                    {
                        this.singleLocation = ((Location) this.singleLocation.Value).CreateReference(this.singleLocation.BufferGetsOnCollapse);
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.locations.Length; i++)
                {
                    Location location = this.locations[i];
                    if ((location != null) && object.ReferenceEquals(location.TemporaryResolutionEnvironment, this))
                    {
                        if (location.Value == null)
                        {
                            this.locations[i] = (Location) location.CreateDefaultValue();
                        }
                        else
                        {
                            this.locations[i] = ((Location) location.Value).CreateReference(location.BufferGetsOnCollapse);
                        }
                    }
                }
            }
        }

        internal void Declare(LocationReference locationReference, Location location, System.Activities.ActivityInstance activityInstance)
        {
            if (location.CanBeMapped)
            {
                this.hasMappableLocations = true;
                this.MappableObjectManager.Register(location, this.Definition, locationReference, activityInstance);
            }
            if (this.locations == null)
            {
                this.singleLocation = location;
            }
            else
            {
                this.locations[locationReference.Id] = location;
            }
        }

        internal void DeclareHandle(LocationReference locationReference, Location location, System.Activities.ActivityInstance activityInstance)
        {
            this.hasHandles = true;
            this.Declare(locationReference, location, activityInstance);
        }

        internal void DeclareTemporaryLocation<T>(LocationReference locationReference, System.Activities.ActivityInstance activityInstance, bool bufferGetsOnCollapse) where T: Location
        {
            Location location = new Location<T>();
            location.SetTemporaryResolutionData(this, bufferGetsOnCollapse);
            this.Declare(locationReference, location, activityInstance);
        }

        internal void Dispose()
        {
            this.isDisposed = true;
            this.CleanupMappedLocations();
        }

        internal Location GetSpecificLocation(int id)
        {
            if (this.locations == null)
            {
                return this.singleLocation;
            }
            return this.locations[id];
        }

        internal Location<T> GetSpecificLocation<T>(int id)
        {
            return (this.GetSpecificLocation(id) as Location<T>);
        }

        internal void OnDeserialized(ActivityExecutor executor, System.Activities.ActivityInstance handleScope)
        {
            this.executor = executor;
            if (this.Definition == null)
            {
                this.Definition = handleScope.Activity;
            }
            this.ReinitializeHandles(handleScope);
        }

        internal void ReinitializeHandles(System.Activities.ActivityInstance handleScope)
        {
            if (this.handles != null)
            {
                int count = this.handles.Count;
                for (int i = 0; i < count; i++)
                {
                    this.handles[i].Reinitialize(handleScope);
                    this.hasHandles = true;
                }
            }
        }

        internal void RemoveReference(bool isOwner)
        {
            if (isOwner)
            {
                this.hasOwnerCompleted = true;
            }
            this.referenceCountMinusOne--;
        }

        void ActivityInstanceMap.IActivityReference.Load(Activity activity, ActivityInstanceMap instanceMap)
        {
            this.Definition = activity;
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(base.GetType().FullName, System.Activities.SR.EnvironmentDisposed));
            }
        }

        internal bool TryGetLocation(int id, out Location value)
        {
            this.ThrowIfDisposed();
            value = null;
            if (this.locations == null)
            {
                if (id == 0)
                {
                    value = this.singleLocation;
                }
            }
            else if (this.locations.Length > id)
            {
                value = this.locations[id];
            }
            return (value != null);
        }

        internal bool TryGetLocation(int id, Activity environmentOwner, out Location value)
        {
            this.ThrowIfDisposed();
            LocationEnvironment parent = this;
            while ((parent != null) && (parent.Definition != environmentOwner))
            {
                parent = parent.Parent;
            }
            if (parent == null)
            {
                value = null;
                return false;
            }
            value = null;
            if ((id == 0) && (parent.locations == null))
            {
                value = parent.singleLocation;
            }
            else if ((parent.locations != null) && (parent.locations.Length > id))
            {
                value = parent.locations[id];
            }
            return (value != null);
        }

        internal void UninitializeHandles(System.Activities.ActivityInstance scope)
        {
            if (this.hasHandles)
            {
                using (HandleInitializationContext context = null)
                {
                    this.UninitializeHandles(scope, this.Definition.RuntimeVariables, ref context);
                    this.UninitializeHandles(scope, this.Definition.ImplementationVariables, ref context);
                    this.hasHandles = false;
                }
            }
        }

        private void UninitializeHandles(System.Activities.ActivityInstance scope, IList<Variable> variables, ref HandleInitializationContext context)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                Variable variable = variables[i];
                if (variable.IsHandle)
                {
                    Location specificLocation = this.GetSpecificLocation(variable.Id);
                    if (specificLocation != null)
                    {
                        Handle handle = (Handle) specificLocation.Value;
                        if (handle != null)
                        {
                            if (context == null)
                            {
                                context = new HandleInitializationContext(this.executor, scope);
                            }
                            handle.Uninitialize(context);
                        }
                        specificLocation.Value = null;
                    }
                }
            }
        }

        internal Activity Definition { get; private set; }

        internal List<Handle> Handles
        {
            get
            {
                return this.handles;
            }
        }

        internal bool HasHandles
        {
            get
            {
                return this.hasHandles;
            }
        }

        internal bool HasOwnerCompleted
        {
            get
            {
                return this.hasOwnerCompleted;
            }
        }

        private System.Activities.Runtime.MappableObjectManager MappableObjectManager
        {
            get
            {
                return this.executor.MappableObjectManager;
            }
        }

        internal LocationEnvironment Parent
        {
            get
            {
                return this.parent;
            }
        }

        internal bool ShouldDispose
        {
            get
            {
                return (this.referenceCountMinusOne == -1);
            }
        }

        Activity ActivityInstanceMap.IActivityReference.Activity
        {
            get
            {
                return this.Definition;
            }
        }
    }
}

