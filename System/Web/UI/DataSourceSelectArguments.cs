namespace System.Web.UI
{
    using System;
    using System.Web.Util;

    public sealed class DataSourceSelectArguments
    {
        private int _maximumRows;
        private DataSourceCapabilities _requestedCapabilities;
        private bool _retrieveTotalRowCount;
        private string _sortExpression;
        private int _startRowIndex;
        private DataSourceCapabilities _supportedCapabilities;
        private int _totalRowCount;

        public DataSourceSelectArguments() : this(string.Empty, 0, 0)
        {
        }

        public DataSourceSelectArguments(string sortExpression) : this(sortExpression, 0, 0)
        {
        }

        public DataSourceSelectArguments(int startRowIndex, int maximumRows) : this(string.Empty, startRowIndex, maximumRows)
        {
        }

        public DataSourceSelectArguments(string sortExpression, int startRowIndex, int maximumRows)
        {
            this._totalRowCount = -1;
            this.SortExpression = sortExpression;
            this.StartRowIndex = startRowIndex;
            this.MaximumRows = maximumRows;
        }

        public void AddSupportedCapabilities(DataSourceCapabilities capabilities)
        {
            this._supportedCapabilities |= capabilities;
        }

        public override bool Equals(object obj)
        {
            DataSourceSelectArguments arguments = obj as DataSourceSelectArguments;
            if (arguments == null)
            {
                return false;
            }
            return ((((arguments.MaximumRows == this._maximumRows) && (arguments.RetrieveTotalRowCount == this._retrieveTotalRowCount)) && ((arguments.SortExpression == this._sortExpression) && (arguments.StartRowIndex == this._startRowIndex))) && (arguments.TotalRowCount == this._totalRowCount));
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes(this._maximumRows.GetHashCode(), this._retrieveTotalRowCount.GetHashCode(), this._sortExpression.GetHashCode(), this._startRowIndex.GetHashCode(), this._totalRowCount.GetHashCode());
        }

        public void RaiseUnsupportedCapabilitiesError(DataSourceView view)
        {
            DataSourceCapabilities capabilities = this._requestedCapabilities & ~this._supportedCapabilities;
            if ((capabilities & DataSourceCapabilities.Sort) != DataSourceCapabilities.None)
            {
                view.RaiseUnsupportedCapabilityError(DataSourceCapabilities.Sort);
            }
            if ((capabilities & DataSourceCapabilities.Page) != DataSourceCapabilities.None)
            {
                view.RaiseUnsupportedCapabilityError(DataSourceCapabilities.Page);
            }
            if ((capabilities & DataSourceCapabilities.RetrieveTotalRowCount) != DataSourceCapabilities.None)
            {
                view.RaiseUnsupportedCapabilityError(DataSourceCapabilities.RetrieveTotalRowCount);
            }
        }

        public static DataSourceSelectArguments Empty
        {
            get
            {
                return new DataSourceSelectArguments();
            }
        }

        public int MaximumRows
        {
            get
            {
                return this._maximumRows;
            }
            set
            {
                if (value == 0)
                {
                    if (this._startRowIndex == 0)
                    {
                        this._requestedCapabilities &= ~DataSourceCapabilities.Page;
                    }
                }
                else
                {
                    this._requestedCapabilities |= DataSourceCapabilities.Page;
                }
                this._maximumRows = value;
            }
        }

        public bool RetrieveTotalRowCount
        {
            get
            {
                return this._retrieveTotalRowCount;
            }
            set
            {
                if (value)
                {
                    this._requestedCapabilities |= DataSourceCapabilities.RetrieveTotalRowCount;
                }
                else
                {
                    this._requestedCapabilities &= ~DataSourceCapabilities.RetrieveTotalRowCount;
                }
                this._retrieveTotalRowCount = value;
            }
        }

        public string SortExpression
        {
            get
            {
                if (this._sortExpression == null)
                {
                    this._sortExpression = string.Empty;
                }
                return this._sortExpression;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this._requestedCapabilities &= ~DataSourceCapabilities.Sort;
                }
                else
                {
                    this._requestedCapabilities |= DataSourceCapabilities.Sort;
                }
                this._sortExpression = value;
            }
        }

        public int StartRowIndex
        {
            get
            {
                return this._startRowIndex;
            }
            set
            {
                if (value == 0)
                {
                    if (this._maximumRows == 0)
                    {
                        this._requestedCapabilities &= ~DataSourceCapabilities.Page;
                    }
                }
                else
                {
                    this._requestedCapabilities |= DataSourceCapabilities.Page;
                }
                this._startRowIndex = value;
            }
        }

        public int TotalRowCount
        {
            get
            {
                return this._totalRowCount;
            }
            set
            {
                this._totalRowCount = value;
            }
        }
    }
}

