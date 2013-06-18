namespace System.Web.Services.Description
{
    using System;
    using System.Reflection;
    using System.Web.Services;

    public sealed class OperationMessageCollection : ServiceDescriptionBaseCollection
    {
        internal OperationMessageCollection(Operation operation) : base(operation)
        {
        }

        public int Add(OperationMessage operationMessage)
        {
            return base.List.Add(operationMessage);
        }

        public bool Contains(OperationMessage operationMessage)
        {
            return base.List.Contains(operationMessage);
        }

        public void CopyTo(OperationMessage[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(OperationMessage operationMessage)
        {
            return base.List.IndexOf(operationMessage);
        }

        public void Insert(int index, OperationMessage operationMessage)
        {
            base.List.Insert(index, operationMessage);
        }

        protected override void OnInsert(int index, object value)
        {
            if ((base.Count > 1) || ((base.Count == 1) && (value.GetType() == base.List[0].GetType())))
            {
                throw new InvalidOperationException(Res.GetString("WebDescriptionTooManyMessages"));
            }
            base.OnInsert(index, value);
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            if (oldValue.GetType() != newValue.GetType())
            {
                throw new InvalidOperationException(Res.GetString("WebDescriptionTooManyMessages"));
            }
            base.OnSet(index, oldValue, newValue);
        }

        protected override void OnValidate(object value)
        {
            if (!(value is OperationInput) && !(value is OperationOutput))
            {
                throw new ArgumentException(Res.GetString("OnlyOperationInputOrOperationOutputTypes"), "value");
            }
            base.OnValidate(value);
        }

        public void Remove(OperationMessage operationMessage)
        {
            base.List.Remove(operationMessage);
        }

        protected override void SetParent(object value, object parent)
        {
            ((OperationMessage) value).SetParent((Operation) parent);
        }

        public OperationFlow Flow
        {
            get
            {
                if (base.List.Count == 0)
                {
                    return OperationFlow.None;
                }
                if (base.List.Count == 1)
                {
                    if (base.List[0] is OperationInput)
                    {
                        return OperationFlow.OneWay;
                    }
                    return OperationFlow.Notification;
                }
                if (base.List[0] is OperationInput)
                {
                    return OperationFlow.RequestResponse;
                }
                return OperationFlow.SolicitResponse;
            }
        }

        public OperationInput Input
        {
            get
            {
                for (int i = 0; i < base.List.Count; i++)
                {
                    OperationInput input = base.List[i] as OperationInput;
                    if (input != null)
                    {
                        return input;
                    }
                }
                return null;
            }
        }

        public OperationMessage this[int index]
        {
            get
            {
                return (OperationMessage) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public OperationOutput Output
        {
            get
            {
                for (int i = 0; i < base.List.Count; i++)
                {
                    OperationOutput output = base.List[i] as OperationOutput;
                    if (output != null)
                    {
                        return output;
                    }
                }
                return null;
            }
        }
    }
}

