namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [Guid("EFFA164B-3E87-4195-88DB-8AC004DDFE2A"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class ProductCollection : IProductCollection, IEnumerable
    {
        private ArrayList list = new ArrayList();
        private Hashtable table = new Hashtable();

        internal ProductCollection()
        {
        }

        internal void Add(Microsoft.Build.Tasks.Deployment.Bootstrapper.Product product)
        {
            if (!this.table.Contains(product.ProductCode.ToUpperInvariant()))
            {
                this.list.Add(product);
                this.table.Add(product.ProductCode.ToUpperInvariant(), product);
            }
        }

        internal void Clear()
        {
            this.list.Clear();
            this.table.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public Microsoft.Build.Tasks.Deployment.Bootstrapper.Product Item(int index)
        {
            return (Microsoft.Build.Tasks.Deployment.Bootstrapper.Product) this.list[index];
        }

        public Microsoft.Build.Tasks.Deployment.Bootstrapper.Product Product(string productCode)
        {
            return (Microsoft.Build.Tasks.Deployment.Bootstrapper.Product) this.table[productCode.ToUpperInvariant()];
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }
    }
}

