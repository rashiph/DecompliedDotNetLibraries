namespace System.Data
{
    using System;

    public class FillErrorEventArgs : EventArgs
    {
        private bool continueFlag;
        private System.Data.DataTable dataTable;
        private Exception errors;
        private object[] values;

        public FillErrorEventArgs(System.Data.DataTable dataTable, object[] values)
        {
            this.dataTable = dataTable;
            this.values = values;
            if (this.values == null)
            {
                this.values = new object[0];
            }
        }

        public bool Continue
        {
            get
            {
                return this.continueFlag;
            }
            set
            {
                this.continueFlag = value;
            }
        }

        public System.Data.DataTable DataTable
        {
            get
            {
                return this.dataTable;
            }
        }

        public Exception Errors
        {
            get
            {
                return this.errors;
            }
            set
            {
                this.errors = value;
            }
        }

        public object[] Values
        {
            get
            {
                object[] objArray = new object[this.values.Length];
                for (int i = 0; i < this.values.Length; i++)
                {
                    objArray[i] = this.values[i];
                }
                return objArray;
            }
        }
    }
}

