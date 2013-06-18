namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [Guid("D25C0741-99CA-49f7-9460-95E5F25EEF43"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class ProductBuilderCollection : IProductBuilderCollection, IEnumerable
    {
        private ArrayList list = new ArrayList();

        internal ProductBuilderCollection()
        {
        }

        public void Add(ProductBuilder builder)
        {
            this.list.Add(builder);
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        internal void Insert(int index, ProductBuilder builder)
        {
            this.list.Insert(index, builder);
        }

        internal ProductBuilder Item(int index)
        {
            return (ProductBuilder) this.list[index];
        }

        internal int Count
        {
            get
            {
                return this.list.Count;
            }
        }
    }
}

