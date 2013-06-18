namespace System.Web.UI
{
    using System;

    public class ComplexPropertyEntry : BuilderPropertyEntry
    {
        private bool _isCollectionItem;
        private bool _readOnly;

        internal ComplexPropertyEntry()
        {
        }

        internal ComplexPropertyEntry(bool isCollectionItem)
        {
            this._isCollectionItem = isCollectionItem;
        }

        public bool IsCollectionItem
        {
            get
            {
                return this._isCollectionItem;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return this._readOnly;
            }
            set
            {
                this._readOnly = value;
            }
        }
    }
}

