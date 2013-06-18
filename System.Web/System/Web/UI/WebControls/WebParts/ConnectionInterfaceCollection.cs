namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web;

    public sealed class ConnectionInterfaceCollection : ReadOnlyCollectionBase
    {
        public static readonly ConnectionInterfaceCollection Empty = new ConnectionInterfaceCollection();

        public ConnectionInterfaceCollection()
        {
        }

        public ConnectionInterfaceCollection(ICollection connectionInterfaces)
        {
            this.Initialize(null, connectionInterfaces);
        }

        public ConnectionInterfaceCollection(ConnectionInterfaceCollection existingConnectionInterfaces, ICollection connectionInterfaces)
        {
            this.Initialize(existingConnectionInterfaces, connectionInterfaces);
        }

        public bool Contains(Type value)
        {
            return base.InnerList.Contains(value);
        }

        public void CopyTo(Type[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public int IndexOf(Type value)
        {
            return base.InnerList.IndexOf(value);
        }

        private void Initialize(ConnectionInterfaceCollection existingConnectionInterfaces, ICollection connectionInterfaces)
        {
            if (existingConnectionInterfaces != null)
            {
                foreach (Type type in existingConnectionInterfaces)
                {
                    base.InnerList.Add(type);
                }
            }
            if (connectionInterfaces != null)
            {
                foreach (object obj2 in connectionInterfaces)
                {
                    if (obj2 == null)
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Collection_CantAddNull"), "connectionInterfaces");
                    }
                    if (!(obj2 is Type))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "Type" }), "connectionInterfaces");
                    }
                    base.InnerList.Add(obj2);
                }
            }
        }

        public Type this[int index]
        {
            get
            {
                return (Type) base.InnerList[index];
            }
        }
    }
}

