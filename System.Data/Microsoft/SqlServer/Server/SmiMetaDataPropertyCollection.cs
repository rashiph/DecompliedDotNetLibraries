namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Reflection;

    internal class SmiMetaDataPropertyCollection
    {
        private static readonly SmiDefaultFieldsProperty __emptyDefaultFields = new SmiDefaultFieldsProperty(new List<bool>());
        private static readonly SmiOrderProperty __emptySortOrder = new SmiOrderProperty(new List<SmiOrderProperty.SmiColumnOrder>());
        private static readonly SmiUniqueKeyProperty __emptyUniqueKey = new SmiUniqueKeyProperty(new List<bool>());
        private bool _isReadOnly = false;
        private SmiMetaDataProperty[] _properties = new SmiMetaDataProperty[3];
        internal static readonly SmiMetaDataPropertyCollection EmptyInstance = new SmiMetaDataPropertyCollection();
        private const int SelectorCount = 3;

        static SmiMetaDataPropertyCollection()
        {
            EmptyInstance.SetReadOnly();
        }

        internal SmiMetaDataPropertyCollection()
        {
            this._properties[0] = __emptyDefaultFields;
            this._properties[1] = __emptySortOrder;
            this._properties[2] = __emptyUniqueKey;
        }

        private void EnsureWritable()
        {
            if (this.IsReadOnly)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
        }

        internal void SetReadOnly()
        {
            this._isReadOnly = true;
        }

        internal bool IsReadOnly
        {
            get
            {
                return this._isReadOnly;
            }
        }

        internal SmiMetaDataProperty this[SmiPropertySelector key]
        {
            get
            {
                return this._properties[(int) key];
            }
            set
            {
                if (value == null)
                {
                    throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
                }
                this.EnsureWritable();
                this._properties[(int) key] = value;
            }
        }

        internal IEnumerable<SmiMetaDataProperty> Values
        {
            get
            {
                return new List<SmiMetaDataProperty>(this._properties);
            }
        }
    }
}

