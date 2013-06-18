namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;

    internal sealed class NativeBuffer_RowBuffer : NativeBuffer
    {
        private int _numberOfRows;
        private bool _ready;
        private int _rowLength;

        internal NativeBuffer_RowBuffer(int initialSize, int numberOfRows) : base(initialSize * numberOfRows, false)
        {
            this._rowLength = initialSize;
            this._numberOfRows = numberOfRows;
        }

        internal void MoveFirst()
        {
            base.BaseOffset = 0;
            this._ready = true;
        }

        internal bool MoveNext()
        {
            if (!this._ready)
            {
                return false;
            }
            base.BaseOffset += this.RowLength;
            return this.CurrentPositionIsValid;
        }

        internal bool MovePrevious()
        {
            if (!this._ready)
            {
                return false;
            }
            if (base.BaseOffset <= -this.RowLength)
            {
                return false;
            }
            base.BaseOffset -= this.RowLength;
            return true;
        }

        internal bool CurrentPositionIsValid
        {
            get
            {
                return ((base.BaseOffset >= 0) && (base.BaseOffset < (this.NumberOfRows * this.RowLength)));
            }
        }

        internal int NumberOfRows
        {
            get
            {
                return this._numberOfRows;
            }
            set
            {
                if ((value < 0) || (base.Length < (value * this.RowLength)))
                {
                    throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.InvalidNumberOfRows);
                }
                this._numberOfRows = value;
            }
        }

        internal int RowLength
        {
            get
            {
                return this._rowLength;
            }
        }
    }
}

