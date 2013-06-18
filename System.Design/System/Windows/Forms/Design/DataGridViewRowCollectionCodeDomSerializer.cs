namespace System.Windows.Forms.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;

    internal class DataGridViewRowCollectionCodeDomSerializer : CollectionCodeDomSerializer
    {
        private static DataGridViewRowCollectionCodeDomSerializer defaultSerializer;

        private DataGridViewRowCollectionCodeDomSerializer()
        {
        }

        protected override object SerializeCollection(IDesignerSerializationManager manager, CodeExpression targetExpression, Type targetType, ICollection originalCollection, ICollection valuesToSerialize)
        {
            return new CodeStatementCollection();
        }

        internal static DataGridViewRowCollectionCodeDomSerializer DefaultSerializer
        {
            get
            {
                if (defaultSerializer == null)
                {
                    defaultSerializer = new DataGridViewRowCollectionCodeDomSerializer();
                }
                return defaultSerializer;
            }
        }
    }
}

