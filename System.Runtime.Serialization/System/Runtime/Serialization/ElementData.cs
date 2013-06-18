namespace System.Runtime.Serialization
{
    using System;

    internal class ElementData
    {
        public int attributeCount;
        public AttributeData[] attributes;
        public int childElementIndex;
        public IDataNode dataNode;
        public string localName;
        public string ns;
        public string prefix;

        public void AddAttribute(string prefix, string ns, string name, string value)
        {
            this.GrowAttributesIfNeeded();
            AttributeData data = this.attributes[this.attributeCount];
            if (data == null)
            {
                this.attributes[this.attributeCount] = data = new AttributeData();
            }
            data.prefix = prefix;
            data.ns = ns;
            data.localName = name;
            data.value = value;
            this.attributeCount++;
        }

        private void GrowAttributesIfNeeded()
        {
            if (this.attributes == null)
            {
                this.attributes = new AttributeData[4];
            }
            else if (this.attributes.Length == this.attributeCount)
            {
                AttributeData[] destinationArray = new AttributeData[this.attributes.Length * 2];
                Array.Copy(this.attributes, 0, destinationArray, 0, this.attributes.Length);
                this.attributes = destinationArray;
            }
        }
    }
}

