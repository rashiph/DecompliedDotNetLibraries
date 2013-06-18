namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;

    public class OperationDescriptionCollection : Collection<OperationDescription>
    {
        internal OperationDescriptionCollection()
        {
        }

        public OperationDescription Find(string name)
        {
            for (int i = 0; i < base.Count; i++)
            {
                if (base[i].Name == name)
                {
                    return base[i];
                }
            }
            return null;
        }

        public Collection<OperationDescription> FindAll(string name)
        {
            Collection<OperationDescription> collection = new Collection<OperationDescription>();
            for (int i = 0; i < base.Count; i++)
            {
                if (base[i].Name == name)
                {
                    collection.Add(base[i]);
                }
            }
            return collection;
        }

        protected override void InsertItem(int index, OperationDescription item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, OperationDescription item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            base.SetItem(index, item);
        }
    }
}

