namespace System.Web.Services.Description
{
    using System;
    using System.Reflection;

    public sealed class OperationFaultCollection : ServiceDescriptionBaseCollection
    {
        internal OperationFaultCollection(Operation operation) : base(operation)
        {
        }

        public int Add(OperationFault operationFaultMessage)
        {
            return base.List.Add(operationFaultMessage);
        }

        public bool Contains(OperationFault operationFaultMessage)
        {
            return base.List.Contains(operationFaultMessage);
        }

        public void CopyTo(OperationFault[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        protected override string GetKey(object value)
        {
            return ((OperationFault) value).Name;
        }

        public int IndexOf(OperationFault operationFaultMessage)
        {
            return base.List.IndexOf(operationFaultMessage);
        }

        public void Insert(int index, OperationFault operationFaultMessage)
        {
            base.List.Insert(index, operationFaultMessage);
        }

        public void Remove(OperationFault operationFaultMessage)
        {
            base.List.Remove(operationFaultMessage);
        }

        protected override void SetParent(object value, object parent)
        {
            ((OperationFault) value).SetParent((Operation) parent);
        }

        public OperationFault this[int index]
        {
            get
            {
                return (OperationFault) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public OperationFault this[string name]
        {
            get
            {
                return (OperationFault) this.Table[name];
            }
        }
    }
}

