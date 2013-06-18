namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Text;
    using System.Workflow.Runtime;

    [Serializable]
    public sealed class EventQueueName : IComparable
    {
        private string activityId;
        [NonSerialized]
        private string assemblyQualifiedName;
        private CorrelationProperty[] correlationValues;
        private Type interfaceType;
        private string operation;
        [NonSerialized]
        private string stringifiedKey;

        public EventQueueName(Type interfaceType, string operation)
        {
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }
            this.interfaceType = interfaceType;
            this.operation = operation;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal EventQueueName(Type interfaceType, string operation, string activityId) : this(interfaceType, operation)
        {
            this.activityId = activityId;
        }

        public EventQueueName(Type interfaceType, string operation, ICollection<CorrelationProperty> propertyValues) : this(interfaceType, operation)
        {
            if (propertyValues != null)
            {
                this.correlationValues = new CorrelationProperty[propertyValues.Count];
                propertyValues.CopyTo(this.correlationValues, 0);
            }
        }

        public int CompareTo(object toCompare)
        {
            if (toCompare is EventQueueName)
            {
                return this.CompareTo(toCompare as EventQueueName);
            }
            return -1;
        }

        public int CompareTo(EventQueueName eventQueueName)
        {
            if (eventQueueName == null)
            {
                return -1;
            }
            int num = StringComparer.Ordinal.Compare(this.MethodName, eventQueueName.MethodName);
            if (num == 0)
            {
                num = StringComparer.Ordinal.Compare(this.AssemblyQualifiedName, eventQueueName.AssemblyQualifiedName);
                if ((num != 0) || (this.correlationValues == null))
                {
                    return num;
                }
                num = (eventQueueName.correlationValues != null) ? (this.correlationValues.Length - eventQueueName.correlationValues.Length) : -1;
                if (num != 0)
                {
                    return num;
                }
                for (int i = 0; i < this.correlationValues.Length; i++)
                {
                    if ((this.correlationValues[i] == null) || (eventQueueName.correlationValues[i] == null))
                    {
                        return -1;
                    }
                    object obj2 = this.correlationValues[i].Value;
                    object obj3 = this.FindCorrelationValue(this.correlationValues[i].Name, eventQueueName.correlationValues);
                    if ((obj2 != null) || (obj3 != null))
                    {
                        if (obj2 == null)
                        {
                            return -1;
                        }
                        IComparable comparable = obj2 as IComparable;
                        if (comparable != null)
                        {
                            num = comparable.CompareTo(obj3);
                            if (num != 0)
                            {
                                return num;
                            }
                        }
                        else if (!obj2.Equals(obj3))
                        {
                            return -1;
                        }
                    }
                }
            }
            return num;
        }

        public override bool Equals(object obj)
        {
            EventQueueName eventQueueName = obj as EventQueueName;
            if (eventQueueName == null)
            {
                return false;
            }
            return (0 == this.CompareTo(eventQueueName));
        }

        private object FindCorrelationValue(string name, CorrelationProperty[] correlationValues)
        {
            foreach (CorrelationProperty property in correlationValues)
            {
                if ((property != null) && (property.Name == name))
                {
                    return property.Value;
                }
            }
            return null;
        }

        public CorrelationProperty[] GetCorrelationValues()
        {
            return this.correlationValues;
        }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(this.activityId))
            {
                return (this.AssemblyQualifiedName.GetHashCode() ^ this.operation.GetHashCode());
            }
            return ((this.AssemblyQualifiedName.GetHashCode() ^ this.operation.GetHashCode()) ^ this.activityId.GetHashCode());
        }

        public static bool operator ==(EventQueueName queueKey1, EventQueueName queueKey2)
        {
            bool flag = false;
            if (queueKey1 != null)
            {
                if (queueKey2 != null)
                {
                    flag = 0 == queueKey1.CompareTo(queueKey2);
                }
                return flag;
            }
            if (queueKey2 == null)
            {
                flag = true;
            }
            return flag;
        }

        public static bool operator >(EventQueueName queueKey1, EventQueueName queueKey2)
        {
            if (queueKey1 == null)
            {
                throw new ArgumentNullException("queueKey1");
            }
            if (queueKey2 == null)
            {
                throw new ArgumentNullException("queueKey2");
            }
            return (queueKey1.CompareTo(queueKey2) > 0);
        }

        public static bool operator !=(EventQueueName queueKey1, EventQueueName queueKey2)
        {
            return !(queueKey1 == queueKey2);
        }

        public static bool operator <(EventQueueName queueKey1, EventQueueName queueKey2)
        {
            if (queueKey1 == null)
            {
                throw new ArgumentNullException("queueKey1");
            }
            if (queueKey2 == null)
            {
                throw new ArgumentNullException("queueKey2");
            }
            return (queueKey1.CompareTo(queueKey2) < 0);
        }

        public override string ToString()
        {
            if (this.stringifiedKey == null)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Message Properties");
                builder.AppendLine("Interface Type:" + this.interfaceType.ToString());
                builder.AppendLine("Method Name:" + this.operation.ToString());
                builder.AppendLine("CorrelationValues:");
                if (this.correlationValues != null)
                {
                    foreach (CorrelationProperty property in this.correlationValues)
                    {
                        if ((property != null) && (property.Value != null))
                        {
                            builder.AppendLine(property.Value.ToString());
                        }
                    }
                }
                this.stringifiedKey = builder.ToString();
            }
            return this.stringifiedKey;
        }

        private string AssemblyQualifiedName
        {
            get
            {
                if (this.assemblyQualifiedName == null)
                {
                    this.assemblyQualifiedName = this.interfaceType.AssemblyQualifiedName;
                }
                return this.assemblyQualifiedName;
            }
        }

        public Type InterfaceType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.interfaceType;
            }
        }

        public string MethodName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.operation;
            }
        }
    }
}

