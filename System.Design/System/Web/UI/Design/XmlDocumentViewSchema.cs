namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.UI;

    internal sealed class XmlDocumentViewSchema : IDataSourceViewSchema
    {
        private ArrayList _attrs;
        private OrderedDictionary _children;
        private IDataSourceFieldSchema[] _fieldSchemas;
        private bool _includeSpecialSchema;
        private string _name;
        private IDataSourceViewSchema[] _viewSchemas;

        public XmlDocumentViewSchema(string name, Pair data, bool includeSpecialSchema)
        {
            this._includeSpecialSchema = includeSpecialSchema;
            this._children = (OrderedDictionary) data.First;
            this._attrs = (ArrayList) data.Second;
            this._name = name;
        }

        public IDataSourceViewSchema[] GetChildren()
        {
            if (this._viewSchemas == null)
            {
                this._viewSchemas = new IDataSourceViewSchema[this._children.Count];
                int index = 0;
                foreach (DictionaryEntry entry in this._children)
                {
                    this._viewSchemas[index] = new XmlDocumentViewSchema((string) entry.Key, (Pair) entry.Value, this._includeSpecialSchema);
                    index++;
                }
            }
            return this._viewSchemas;
        }

        public IDataSourceFieldSchema[] GetFields()
        {
            if (this._fieldSchemas == null)
            {
                int num = this._includeSpecialSchema ? 3 : 0;
                this._fieldSchemas = new IDataSourceFieldSchema[this._attrs.Count + num];
                if (this._includeSpecialSchema)
                {
                    this._fieldSchemas[0] = new XmlDocumentFieldSchema("#Name");
                    this._fieldSchemas[1] = new XmlDocumentFieldSchema("#Value");
                    this._fieldSchemas[2] = new XmlDocumentFieldSchema("#InnerText");
                }
                for (int i = 0; i < this._attrs.Count; i++)
                {
                    this._fieldSchemas[i + num] = new XmlDocumentFieldSchema((string) this._attrs[i]);
                }
            }
            return this._fieldSchemas;
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

