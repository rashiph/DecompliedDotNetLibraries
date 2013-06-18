namespace System.ServiceModel.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Activities.Dispatcher;
    using System.Xml.Linq;

    public sealed class DurableInstancingOptions : IDurableInstancingOptions
    {
        private DurableInstanceManager instanceManager;

        internal DurableInstancingOptions(DurableInstanceManager instanceManager)
        {
            this.instanceManager = instanceManager;
        }

        public void AddInitialInstanceValues(IDictionary<XName, object> writeOnlyValues)
        {
            this.instanceManager.AddInitialInstanceValues(writeOnlyValues);
        }

        public void AddInstanceOwnerValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
        {
            this.instanceManager.AddInstanceOwnerValues(readWriteValues, writeOnlyValues);
        }

        void IDurableInstancingOptions.SetScopeName(XName scopeName)
        {
            this.ScopeName = scopeName;
        }

        public System.Runtime.DurableInstancing.InstanceStore InstanceStore
        {
            get
            {
                return this.instanceManager.InstanceStore;
            }
            set
            {
                this.instanceManager.InstanceStore = value;
            }
        }

        internal XName ScopeName { get; set; }
    }
}

