namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web;

    public sealed class TransformerTypeCollection : ReadOnlyCollectionBase
    {
        public static readonly TransformerTypeCollection Empty = new TransformerTypeCollection();

        public TransformerTypeCollection()
        {
        }

        public TransformerTypeCollection(ICollection transformerTypes)
        {
            this.Initialize(null, transformerTypes);
        }

        public TransformerTypeCollection(TransformerTypeCollection existingTransformerTypes, ICollection transformerTypes)
        {
            this.Initialize(existingTransformerTypes, transformerTypes);
        }

        internal int Add(Type value)
        {
            if (!value.IsSubclassOf(typeof(WebPartTransformer)))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartTransformerAttribute_NotTransformer", new object[] { value.Name }));
            }
            return base.InnerList.Add(value);
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

        private void Initialize(TransformerTypeCollection existingTransformerTypes, ICollection transformerTypes)
        {
            if (existingTransformerTypes != null)
            {
                foreach (Type type in existingTransformerTypes)
                {
                    base.InnerList.Add(type);
                }
            }
            if (transformerTypes != null)
            {
                foreach (object obj2 in transformerTypes)
                {
                    if (obj2 == null)
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Collection_CantAddNull"), "transformerTypes");
                    }
                    if (!(obj2 is Type))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "Type" }), "transformerTypes");
                    }
                    if (!((Type) obj2).IsSubclassOf(typeof(WebPartTransformer)))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("WebPartTransformerAttribute_NotTransformer", new object[] { ((Type) obj2).Name }), "transformerTypes");
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

