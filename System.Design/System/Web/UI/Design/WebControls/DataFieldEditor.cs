namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal class DataFieldEditor : DataFieldCollectionEditor
    {
        public DataFieldEditor(Type type) : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return base.CollectionType.GetElementType();
        }

        protected override object[] GetItems(object editValue)
        {
            if (editValue is Array)
            {
                Array sourceArray = (Array) editValue;
                object[] destinationArray = new object[sourceArray.GetLength(0)];
                Array.Copy(sourceArray, destinationArray, destinationArray.Length);
                return destinationArray;
            }
            return new object[0];
        }

        protected override object SetItems(object editValue, object[] value)
        {
            if (!(editValue is Array) && (editValue != null))
            {
                return editValue;
            }
            Array destinationArray = Array.CreateInstance(base.CollectionItemType, value.Length);
            Array.Copy(value, destinationArray, value.Length);
            return destinationArray;
        }
    }
}

