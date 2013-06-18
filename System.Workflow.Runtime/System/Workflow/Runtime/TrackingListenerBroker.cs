namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    internal class TrackingListenerBroker : ISerializable
    {
        private int _eventOrderId;
        [NonSerialized]
        private System.Workflow.Runtime.TrackingListener _listener;
        private Dictionary<Guid, ServiceProfileContainer> _services;

        internal TrackingListenerBroker()
        {
            this._services = new Dictionary<Guid, ServiceProfileContainer>();
        }

        internal TrackingListenerBroker(System.Workflow.Runtime.TrackingListener listener)
        {
            this._services = new Dictionary<Guid, ServiceProfileContainer>();
            this._listener = listener;
        }

        private TrackingListenerBroker(SerializationInfo info, StreamingContext context)
        {
            this._services = new Dictionary<Guid, ServiceProfileContainer>();
            this._eventOrderId = info.GetInt32("eventOrderId");
            this._services = (Dictionary<Guid, ServiceProfileContainer>) info.GetValue("services", typeof(Dictionary<Guid, ServiceProfileContainer>));
            if (this._services == null)
            {
                this._services = new Dictionary<Guid, ServiceProfileContainer>();
            }
        }

        internal void AddService(Type trackingServiceType, Version profileVersionId)
        {
            this._services.Add(HashHelper.HashServiceType(trackingServiceType), new ServiceProfileContainer(profileVersionId));
        }

        internal bool ContainsService(Type trackingServiceType)
        {
            return this._services.ContainsKey(HashHelper.HashServiceType(trackingServiceType));
        }

        internal int GetNextEventOrderId()
        {
            return ++this._eventOrderId;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("eventOrderId", this._eventOrderId);
            info.AddValue("services", (this._services.Count == 0) ? null : this._services);
        }

        internal bool IsProfileInstance(Type trackingServiceType)
        {
            ServiceProfileContainer container = null;
            if (!this._services.TryGetValue(HashHelper.HashServiceType(trackingServiceType), out container))
            {
                throw new ArgumentException(ExecutionStringManager.InvalidTrackingService);
            }
            return container.IsInstance;
        }

        internal bool IsProfilePrivate(Type trackingServiceType)
        {
            ServiceProfileContainer container = null;
            if (!this._services.TryGetValue(HashHelper.HashServiceType(trackingServiceType), out container))
            {
                throw new ArgumentException(ExecutionStringManager.InvalidTrackingService);
            }
            return container.IsPrivate;
        }

        internal void MakeProfileInstance(Type trackingServiceType)
        {
            ServiceProfileContainer container = null;
            if (!this._services.TryGetValue(HashHelper.HashServiceType(trackingServiceType), out container))
            {
                throw new ArgumentException(ExecutionStringManager.InvalidTrackingService);
            }
            container.IsPrivate = true;
            container.IsInstance = true;
        }

        internal void MakeProfilePrivate(Type trackingServiceType)
        {
            ServiceProfileContainer container = null;
            if (!this._services.TryGetValue(HashHelper.HashServiceType(trackingServiceType), out container))
            {
                throw new ArgumentException(ExecutionStringManager.InvalidTrackingService);
            }
            container.IsPrivate = true;
        }

        internal void RemoveService(Type trackingServiceType)
        {
            this._services.Remove(HashHelper.HashServiceType(trackingServiceType));
        }

        internal void ReplaceServices(Dictionary<string, Type> replacements)
        {
            if ((replacements != null) && (replacements.Count > 0))
            {
                foreach (KeyValuePair<string, Type> pair in replacements)
                {
                    ServiceProfileContainer container;
                    Guid key = HashHelper.HashServiceType(pair.Key);
                    if (this._services.TryGetValue(key, out container))
                    {
                        this._services.Remove(key);
                        Guid guid2 = HashHelper.HashServiceType(pair.Value);
                        if (!this._services.ContainsKey(guid2))
                        {
                            this._services.Add(guid2, container);
                        }
                    }
                }
            }
        }

        internal bool TryGetProfileVersionId(Type trackingServiceType, out Version profileVersionId)
        {
            profileVersionId = new Version(0, 0);
            ServiceProfileContainer container = null;
            if (this._services.TryGetValue(HashHelper.HashServiceType(trackingServiceType), out container))
            {
                profileVersionId = container.ProfileVersionId;
                return true;
            }
            return false;
        }

        internal System.Workflow.Runtime.TrackingListener TrackingListener
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._listener = value;
            }
        }

        [Serializable]
        internal class ServiceProfileContainer
        {
            private bool _isInstance;
            private bool _isPrivate;
            private Version _profileVersionId;

            protected ServiceProfileContainer()
            {
                this._profileVersionId = new Version(0, 0);
            }

            internal ServiceProfileContainer(Version profileVersionId)
            {
                this._profileVersionId = new Version(0, 0);
                this._profileVersionId = profileVersionId;
            }

            internal bool IsInstance
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._isInstance;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this._isInstance = value;
                }
            }

            internal bool IsPrivate
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._isPrivate;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this._isPrivate = value;
                }
            }

            internal Version ProfileVersionId
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._profileVersionId;
                }
            }
        }
    }
}

