namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Collections;

    internal class GetAttributesEvent : EventArgs
    {
        private ArrayList attrList;

        public GetAttributesEvent(ArrayList attrList)
        {
            this.attrList = attrList;
        }

        public void Add(Attribute attribute)
        {
            this.attrList.Add(attribute);
        }
    }
}

