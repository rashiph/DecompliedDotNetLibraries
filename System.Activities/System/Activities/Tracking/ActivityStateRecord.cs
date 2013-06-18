namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class ActivityStateRecord : TrackingRecord
    {
        [DataMember(EmitDefaultValue=false)]
        private IDictionary<string, object> arguments;
        [DataMember(EmitDefaultValue=false)]
        private IDictionary<string, object> variables;
        private static ReadOnlyCollection<string> wildcardCollection = new ReadOnlyCollection<string>(new List<string>(1) { "*" });

        private ActivityStateRecord(ActivityStateRecord record) : base(record)
        {
            this.Activity = record.Activity;
            this.State = record.State;
            if (record.variables != null)
            {
                if (record.variables == ActivityUtilities.EmptyParameters)
                {
                    this.variables = ActivityUtilities.EmptyParameters;
                }
                else
                {
                    this.variables = new Dictionary<string, object>(record.variables);
                }
            }
            if (record.arguments != null)
            {
                if (record.arguments == ActivityUtilities.EmptyParameters)
                {
                    this.arguments = ActivityUtilities.EmptyParameters;
                }
                else
                {
                    this.arguments = new Dictionary<string, object>(record.arguments);
                }
            }
        }

        internal ActivityStateRecord(Guid instanceId, System.Activities.ActivityInstance instance, ActivityInstanceState state) : base(instanceId)
        {
            this.Activity = new ActivityInfo(instance);
            switch (state)
            {
                case ActivityInstanceState.Executing:
                    this.State = "Executing";
                    return;

                case ActivityInstanceState.Closed:
                    this.State = "Closed";
                    return;

                case ActivityInstanceState.Canceled:
                    this.State = "Canceled";
                    return;

                case ActivityInstanceState.Faulted:
                    this.State = "Faulted";
                    return;
            }
            throw Fx.AssertAndThrow("Invalid state value");
        }

        public ActivityStateRecord(Guid instanceId, long recordNumber, ActivityInfo activity, string state) : base(instanceId, recordNumber)
        {
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }
            if (string.IsNullOrEmpty(state))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("state");
            }
            this.Activity = activity;
            this.State = state;
        }

        protected internal override TrackingRecord Clone()
        {
            return new ActivityStateRecord(this);
        }

        internal IDictionary<string, object> GetArguments(ICollection<string> arguments)
        {
            Dictionary<string, object> trackedData = null;
            System.Activities.ActivityInstance currentInstance = this.Activity.Instance;
            if (currentInstance != null)
            {
                System.Activities.Activity activity = currentInstance.Activity;
                bool wildcard = arguments.Contains("*");
                int num = wildcard ? activity.RuntimeArguments.Count : arguments.Count;
                bool flag2 = "Executing".Equals(this.State, StringComparison.Ordinal);
                for (int i = 0; i < activity.RuntimeArguments.Count; i++)
                {
                    RuntimeArgument argument = activity.RuntimeArguments[i];
                    if ((!flag2 || (argument.Direction != ArgumentDirection.Out)) && (this.TrackData(argument.Name, argument.Id, currentInstance, arguments, wildcard, ref trackedData) && (trackedData.Count == num)))
                    {
                        break;
                    }
                }
            }
            if (trackedData == null)
            {
                return ActivityUtilities.EmptyParameters;
            }
            return new ReadOnlyDictionary<string, object>(trackedData, false);
        }

        internal IDictionary<string, object> GetVariables(ICollection<string> variables)
        {
            Dictionary<string, object> trackedData = null;
            System.Activities.ActivityInstance currentInstance = this.Activity.Instance;
            if (currentInstance != null)
            {
                System.Activities.Activity activity = currentInstance.Activity;
                System.Activities.Activity activity2 = currentInstance.Activity;
                bool flag = variables.Contains("*");
                int num = flag ? ((activity.RuntimeVariables.Count + variables.Count) - 1) : variables.Count;
                IdSpace memberOf = activity.MemberOf;
                while (currentInstance != null)
                {
                    bool wildcard = flag && (activity2 == activity);
                    for (int i = 0; i < activity.RuntimeVariables.Count; i++)
                    {
                        Variable variable = activity.RuntimeVariables[i];
                        if (this.TrackData(variable.Name, variable.Id, currentInstance, variables, wildcard, ref trackedData) && (trackedData.Count == num))
                        {
                            return new ReadOnlyDictionary<string, object>(trackedData, false);
                        }
                    }
                    bool flag3 = false;
                    while (!flag3)
                    {
                        currentInstance = currentInstance.Parent;
                        if (currentInstance != null)
                        {
                            activity = currentInstance.Activity;
                            flag3 = activity.MemberOf.Equals(memberOf);
                        }
                        else
                        {
                            flag3 = true;
                        }
                    }
                }
            }
            if (trackedData == null)
            {
                return ActivityUtilities.EmptyParameters;
            }
            return new ReadOnlyDictionary<string, object>(trackedData, false);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "ActivityStateRecord {{ {0}, Activity {{ {1} }}, State = {2} }}", new object[] { base.ToString(), this.Activity.ToString(), this.State });
        }

        private bool TrackData(string name, int id, System.Activities.ActivityInstance currentInstance, ICollection<string> data, bool wildcard, ref Dictionary<string, object> trackedData)
        {
            if (wildcard || data.Contains(name))
            {
                System.Activities.Location specificLocation = currentInstance.Environment.GetSpecificLocation(id);
                if (specificLocation != null)
                {
                    if (trackedData == null)
                    {
                        trackedData = new Dictionary<string, object>(10);
                    }
                    string str = name ?? NameGenerator.Next();
                    trackedData[str] = specificLocation.Value;
                    if (TD.TrackingDataExtractedIsEnabled())
                    {
                        TD.TrackingDataExtracted(str, this.Activity.Name);
                    }
                    return true;
                }
            }
            return false;
        }

        [DataMember]
        public ActivityInfo Activity { get; private set; }

        public IDictionary<string, object> Arguments
        {
            get
            {
                if (this.arguments == null)
                {
                    this.arguments = this.GetArguments(wildcardCollection);
                }
                return this.arguments;
            }
            internal set
            {
                this.arguments = value;
            }
        }

        [DataMember]
        public string State { get; private set; }

        public IDictionary<string, object> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    this.variables = this.GetVariables(wildcardCollection);
                }
                return this.variables;
            }
            internal set
            {
                this.variables = value;
            }
        }
    }
}

