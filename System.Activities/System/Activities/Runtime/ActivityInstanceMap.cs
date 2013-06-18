namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract(Name="InstanceMap", Namespace="http://schemas.datacontract.org/2010/02/System.Activities")]
    internal class ActivityInstanceMap
    {
        private IDictionary<Activity, InstanceList> instanceMapping;
        private InstanceList[] rawDeserializedLists;

        internal ActivityInstanceMap()
        {
        }

        public void AddEntry(IActivityReference reference)
        {
            this.AddEntry(reference, false);
        }

        public void AddEntry(IActivityReference reference, bool skipIfDuplicate)
        {
            InstanceList list;
            Activity key = reference.Activity;
            if (this.InstanceMapping.TryGetValue(key, out list))
            {
                list.Add(reference, skipIfDuplicate);
            }
            else
            {
                this.InstanceMapping.Add(key, new InstanceList(reference));
            }
        }

        public void LoadActivityTree(Activity rootActivity, System.Activities.ActivityInstance rootInstance, List<System.Activities.ActivityInstance> secondaryRootInstances, ActivityExecutor executor)
        {
            this.instanceMapping = new Dictionary<Activity, InstanceList>(this.rawDeserializedLists.Length);
            for (int i = 0; i < this.rawDeserializedLists.Length; i++)
            {
                Activity activity;
                InstanceList list = this.rawDeserializedLists[i];
                if (!QualifiedId.TryGetElementFromRoot(rootActivity, list.ActivityId, out activity))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ActivityInstanceFixupFailed));
                }
                this.instanceMapping.Add(activity, list);
                list.Load(activity, this);
            }
            this.rawDeserializedLists = null;
            Func<System.Activities.ActivityInstance, ActivityExecutor, bool> callback = new Func<System.Activities.ActivityInstance, ActivityExecutor, bool>(this.OnActivityInstanceLoaded);
            rootInstance.FixupInstance(null, this, executor);
            ActivityUtilities.ProcessActivityInstanceTree(rootInstance, executor, callback);
            if (secondaryRootInstances != null)
            {
                foreach (System.Activities.ActivityInstance instance in secondaryRootInstances)
                {
                    instance.FixupInstance(null, this, executor);
                    ActivityUtilities.ProcessActivityInstanceTree(instance, executor, callback);
                }
            }
        }

        private bool OnActivityInstanceLoaded(System.Activities.ActivityInstance activityInstance, ActivityExecutor executor)
        {
            return activityInstance.TryFixupChildren(this, executor);
        }

        public bool RemoveEntry(IActivityReference reference)
        {
            InstanceList list;
            if (this.instanceMapping == null)
            {
                return false;
            }
            Activity key = reference.Activity;
            if (!this.InstanceMapping.TryGetValue(key, out list))
            {
                return false;
            }
            if (list.Count == 1)
            {
                this.InstanceMapping.Remove(key);
            }
            else
            {
                list.Remove(reference);
            }
            return true;
        }

        private IDictionary<Activity, InstanceList> InstanceMapping
        {
            get
            {
                if (this.instanceMapping == null)
                {
                    this.instanceMapping = new Dictionary<Activity, InstanceList>();
                }
                return this.instanceMapping;
            }
        }

        [DataMember(EmitDefaultValue=false)]
        private InstanceList[] SerializedInstanceLists
        {
            get
            {
                if ((this.instanceMapping == null) || (this.instanceMapping.Count == 0))
                {
                    return this.rawDeserializedLists;
                }
                InstanceList[] listArray = new InstanceList[this.instanceMapping.Count];
                int index = 0;
                foreach (KeyValuePair<Activity, InstanceList> pair in this.instanceMapping)
                {
                    pair.Value.ActivityId = pair.Key.QualifiedId.AsByteArray();
                    listArray[index] = pair.Value;
                    index++;
                }
                return listArray;
            }
            set
            {
                this.rawDeserializedLists = value;
            }
        }

        public interface IActivityReference
        {
            void Load(System.Activities.Activity activity, ActivityInstanceMap instanceMap);

            System.Activities.Activity Activity { get; }
        }

        [DataContract]
        private class InstanceList : HybridCollection<ActivityInstanceMap.IActivityReference>
        {
            public InstanceList(ActivityInstanceMap.IActivityReference reference) : base(reference)
            {
            }

            public void Add(ActivityInstanceMap.IActivityReference reference, bool skipIfDuplicate)
            {
                if (skipIfDuplicate)
                {
                    if (base.SingleItem != null)
                    {
                        if (base.SingleItem == reference)
                        {
                            return;
                        }
                    }
                    else if (base.MultipleItems.Contains(reference))
                    {
                        return;
                    }
                }
                base.Add(reference);
            }

            public void Load(Activity activity, ActivityInstanceMap instanceMap)
            {
                if (base.SingleItem != null)
                {
                    base.SingleItem.Load(activity, instanceMap);
                }
                else
                {
                    for (int i = 0; i < base.MultipleItems.Count; i++)
                    {
                        base.MultipleItems[i].Load(activity, instanceMap);
                    }
                }
            }

            [OnSerializing]
            private void OnSerializing(StreamingContext context)
            {
                base.Compress();
            }

            [DataMember]
            public byte[] ActivityId { get; set; }
        }
    }
}

