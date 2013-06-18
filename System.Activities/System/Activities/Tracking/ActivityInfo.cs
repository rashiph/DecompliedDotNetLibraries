namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class ActivityInfo
    {
        private string id;
        private string instanceId;
        private string name;
        private string typeName;

        internal ActivityInfo(System.Activities.ActivityInstance instance)
        {
            this.Instance = instance;
        }

        public ActivityInfo(string name, string id, string instanceId, string typeName)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }
            if (string.IsNullOrEmpty(id))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("id");
            }
            if (string.IsNullOrEmpty(instanceId))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("instanceId");
            }
            if (string.IsNullOrEmpty(typeName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("typeName");
            }
            this.Name = name;
            this.Id = id;
            this.InstanceId = instanceId;
            this.TypeName = typeName;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Name={0}, ActivityId = {1}, ActivityInstanceId = {2}, TypeName={3}", new object[] { this.Name, this.Id, this.InstanceId, this.TypeName });
        }

        [DataMember]
        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(this.id))
                {
                    this.id = this.Instance.Activity.Id;
                }
                return this.id;
            }
            private set
            {
                this.id = value;
            }
        }

        internal System.Activities.ActivityInstance Instance { get; private set; }

        [DataMember]
        public string InstanceId
        {
            get
            {
                if (string.IsNullOrEmpty(this.instanceId))
                {
                    this.instanceId = this.Instance.Id;
                }
                return this.instanceId;
            }
            private set
            {
                this.instanceId = value;
            }
        }

        [DataMember]
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(this.name))
                {
                    this.name = this.Instance.Activity.DisplayName;
                }
                return this.name;
            }
            private set
            {
                this.name = value;
            }
        }

        [DataMember]
        public string TypeName
        {
            get
            {
                if (string.IsNullOrEmpty(this.typeName))
                {
                    this.typeName = this.Instance.Activity.GetType().FullName;
                }
                return this.typeName;
            }
            private set
            {
                this.typeName = value;
            }
        }
    }
}

