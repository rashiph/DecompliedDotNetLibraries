namespace System.Data.OleDb
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Text;

    internal sealed class Bindings
    {
        private readonly tagDBPARAMBINDINFO[] _bindInfo;
        private int _collectionChangeID;
        private ColumnBinding[] _columnBindings;
        private int _count;
        private int _dataBufferSize;
        private OleDbDataReader _dataReader;
        private readonly tagDBBINDING[] _dbbindings;
        private readonly tagDBCOLUMNACCESS[] _dbcolumns;
        private bool _forceRebind;
        private bool _ifIRowsetElseIRow;
        private int _index;
        private bool _needToReset;
        private OleDbParameter[] _parameters;
        private System.Data.OleDb.RowBinding _rowBinding;

        private Bindings(int count)
        {
            this._count = count;
            this._dbbindings = new tagDBBINDING[count];
            for (int i = 0; i < this._dbbindings.Length; i++)
            {
                this._dbbindings[i] = new tagDBBINDING();
            }
            this._dbcolumns = new tagDBCOLUMNACCESS[count];
        }

        internal Bindings(OleDbParameter[] parameters, int collectionChangeID) : this(parameters.Length)
        {
            this._bindInfo = new tagDBPARAMBINDINFO[parameters.Length];
            this._parameters = parameters;
            this._collectionChangeID = collectionChangeID;
            this._ifIRowsetElseIRow = true;
        }

        internal Bindings(OleDbDataReader dataReader, bool ifIRowsetElseIRow, int count) : this(count)
        {
            this._dataReader = dataReader;
            this._ifIRowsetElseIRow = ifIRowsetElseIRow;
        }

        internal int AllocateForAccessor(OleDbDataReader dataReader, int indexStart, int indexForAccessor)
        {
            System.Data.OleDb.RowBinding binding = System.Data.OleDb.RowBinding.CreateBuffer(this._count, this._dataBufferSize, this._needToReset);
            this._rowBinding = binding;
            ColumnBinding[] bindingArray = binding.SetBindings(dataReader, this, indexStart, indexForAccessor, this._parameters, this._dbbindings, this._ifIRowsetElseIRow);
            this._columnBindings = bindingArray;
            if (!this._ifIRowsetElseIRow)
            {
                for (int i = 0; i < bindingArray.Length; i++)
                {
                    this._dbcolumns[i].pData = binding.DangerousGetDataPtr(bindingArray[i].ValueOffset);
                }
            }
            return (indexStart + bindingArray.Length);
        }

        internal void ApplyInputParameters()
        {
            ColumnBinding[] bindingArray = this.ColumnBindings();
            OleDbParameter[] parameterArray = this.Parameters();
            this.RowBinding().StartDataBlock();
            for (int i = 0; i < parameterArray.Length; i++)
            {
                if (ADP.IsDirection(parameterArray[i], ParameterDirection.Input))
                {
                    bindingArray[i].SetOffset(parameterArray[i].Offset);
                    bindingArray[i].Value(parameterArray[i].GetCoercedValue());
                }
                else
                {
                    parameterArray[i].Value = null;
                }
            }
        }

        internal void ApplyOutputParameters()
        {
            ColumnBinding[] bindingArray = this.ColumnBindings();
            OleDbParameter[] parameterArray = this.Parameters();
            for (int i = 0; i < parameterArray.Length; i++)
            {
                if (ADP.IsDirection(parameterArray[i], ParameterDirection.Output))
                {
                    parameterArray[i].Value = bindingArray[i].Value();
                }
            }
            this.CleanupBindings();
        }

        internal bool AreParameterBindingsInvalid(OleDbParameterCollection collection)
        {
            ColumnBinding[] bindingArray = this.ColumnBindings();
            if ((this.ForceRebind || (collection.ChangeID != this._collectionChangeID)) || (this._parameters.Length != collection.Count))
            {
                return true;
            }
            for (int i = 0; i < bindingArray.Length; i++)
            {
                ColumnBinding binding = bindingArray[i];
                if (binding.IsParameterBindingInvalid(collection[i]))
                {
                    return true;
                }
            }
            return false;
        }

        internal void CleanupBindings()
        {
            System.Data.OleDb.RowBinding binding2 = this.RowBinding();
            if (binding2 != null)
            {
                binding2.ResetValues();
                foreach (ColumnBinding binding in this.ColumnBindings())
                {
                    if (binding != null)
                    {
                        binding.ResetValue();
                    }
                }
            }
        }

        internal void CloseFromConnection()
        {
            if (this._rowBinding != null)
            {
                this._rowBinding.CloseFromConnection();
            }
            this.Dispose();
        }

        internal ColumnBinding[] ColumnBindings()
        {
            return this._columnBindings;
        }

        internal OleDbHResult CreateAccessor(UnsafeNativeMethods.IAccessor iaccessor, int flags)
        {
            return this._rowBinding.CreateAccessor(iaccessor, flags, this._columnBindings);
        }

        public void Dispose()
        {
            this._parameters = null;
            this._dataReader = null;
            this._columnBindings = null;
            System.Data.OleDb.RowBinding binding = this._rowBinding;
            this._rowBinding = null;
            if (binding != null)
            {
                binding.Dispose();
            }
        }

        internal void GuidKindName(Guid guid, int eKind, IntPtr propid)
        {
            tagDBCOLUMNACCESS[] dBColumnAccess = this.DBColumnAccess;
            dBColumnAccess[this._index].columnid.uGuid = guid;
            dBColumnAccess[this._index].columnid.eKind = eKind;
            dBColumnAccess[this._index].columnid.ulPropid = propid;
        }

        internal OleDbParameter[] Parameters()
        {
            return this._parameters;
        }

        internal void ParameterStatus(StringBuilder builder)
        {
            ColumnBinding[] bindingArray = this.ColumnBindings();
            for (int i = 0; i < bindingArray.Length; i++)
            {
                ODB.CommandParameterStatus(builder, i, bindingArray[i].StatusValue());
            }
        }

        internal System.Data.OleDb.RowBinding RowBinding()
        {
            return this._rowBinding;
        }

        internal tagDBPARAMBINDINFO[] BindInfo
        {
            get
            {
                return this._bindInfo;
            }
        }

        internal int CurrentIndex
        {
            set
            {
                this._index = value;
            }
        }

        internal IntPtr DataSourceType
        {
            set
            {
                this._bindInfo[this._index].pwszDataSourceType = value;
            }
        }

        internal tagDBCOLUMNACCESS[] DBColumnAccess
        {
            get
            {
                return this._dbcolumns;
            }
        }

        internal int DbType
        {
            get
            {
                return this._dbbindings[this._index].wType;
            }
            set
            {
                this._dbbindings[this._index].wType = (short) value;
                this._dbcolumns[this._index].wType = (short) value;
            }
        }

        internal int Flags
        {
            set
            {
                this._bindInfo[this._index].dwFlags = value;
            }
        }

        internal bool ForceRebind
        {
            get
            {
                return this._forceRebind;
            }
            set
            {
                this._forceRebind = value;
            }
        }

        internal int MaxLen
        {
            set
            {
                this._dbbindings[this._index].obStatus = (IntPtr) this._dataBufferSize;
                this._dbbindings[this._index].obLength = (IntPtr) (this._dataBufferSize + ADP.PtrSize);
                this._dbbindings[this._index].obValue = (IntPtr) ((this._dataBufferSize + ADP.PtrSize) + ADP.PtrSize);
                this._dataBufferSize += ADP.PtrSize + ADP.PtrSize;
                switch (this.DbType)
                {
                    case 8:
                    case 12:
                    case 0x88:
                    case 0x8a:
                    case 0x4080:
                    case 0x4082:
                        this._dataBufferSize += System.Data.OleDb.RowBinding.AlignDataSize(value * 2);
                        this._needToReset = true;
                        break;

                    default:
                        this._dataBufferSize += System.Data.OleDb.RowBinding.AlignDataSize(value);
                        break;
                }
                this._dbbindings[this._index].cbMaxLen = (IntPtr) value;
                this._dbcolumns[this._index].cbMaxLen = (IntPtr) value;
            }
        }

        internal IntPtr Name
        {
            set
            {
                this._bindInfo[this._index].pwszName = value;
            }
        }

        internal IntPtr Ordinal
        {
            set
            {
                this._dbbindings[this._index].iOrdinal = value;
            }
        }

        internal int ParamIO
        {
            set
            {
                this._dbbindings[this._index].eParamIO = value;
            }
        }

        internal IntPtr ParamSize
        {
            get
            {
                if (this._bindInfo != null)
                {
                    return this._bindInfo[this._index].ulParamSize;
                }
                return IntPtr.Zero;
            }
            set
            {
                this._bindInfo[this._index].ulParamSize = value;
            }
        }

        internal int Part
        {
            set
            {
                this._dbbindings[this._index].dwPart = value;
            }
        }

        internal byte Precision
        {
            set
            {
                if (this._bindInfo != null)
                {
                    this._bindInfo[this._index].bPrecision = value;
                }
                this._dbbindings[this._index].bPrecision = value;
                this._dbcolumns[this._index].bPrecision = value;
            }
        }

        internal byte Scale
        {
            set
            {
                if (this._bindInfo != null)
                {
                    this._bindInfo[this._index].bScale = value;
                }
                this._dbbindings[this._index].bScale = value;
                this._dbcolumns[this._index].bScale = value;
            }
        }
    }
}

