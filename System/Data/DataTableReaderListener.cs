namespace System.Data
{
    using System;
    using System.ComponentModel;

    internal sealed class DataTableReaderListener
    {
        private DataTable currentDataTable;
        private bool isSubscribed;
        private WeakReference readerWeak;

        internal DataTableReaderListener(DataTableReader reader)
        {
            if (reader == null)
            {
                throw ExceptionBuilder.ArgumentNull("DataTableReader");
            }
            if (this.currentDataTable != null)
            {
                this.UnSubscribeEvents();
            }
            this.readerWeak = new WeakReference(reader);
            this.currentDataTable = reader.CurrentDataTable;
            if (this.currentDataTable != null)
            {
                this.SubscribeEvents();
            }
        }

        internal void CleanUp()
        {
            this.UnSubscribeEvents();
        }

        private void DataChanged(object sender, DataRowChangeEventArgs args)
        {
            DataTableReader target = (DataTableReader) this.readerWeak.Target;
            if (target != null)
            {
                target.DataChanged(args);
            }
            else
            {
                this.UnSubscribeEvents();
            }
        }

        private void DataTableCleared(object sender, DataTableClearEventArgs e)
        {
            DataTableReader target = (DataTableReader) this.readerWeak.Target;
            if (target != null)
            {
                target.DataTableCleared();
            }
            else
            {
                this.UnSubscribeEvents();
            }
        }

        private void SchemaChanged(object sender, CollectionChangeEventArgs e)
        {
            DataTableReader target = (DataTableReader) this.readerWeak.Target;
            if (target != null)
            {
                target.SchemaChanged();
            }
            else
            {
                this.UnSubscribeEvents();
            }
        }

        private void SubscribeEvents()
        {
            if ((this.currentDataTable != null) && !this.isSubscribed)
            {
                this.currentDataTable.Columns.ColumnPropertyChanged += new CollectionChangeEventHandler(this.SchemaChanged);
                this.currentDataTable.Columns.CollectionChanged += new CollectionChangeEventHandler(this.SchemaChanged);
                this.currentDataTable.RowChanged += new DataRowChangeEventHandler(this.DataChanged);
                this.currentDataTable.RowDeleted += new DataRowChangeEventHandler(this.DataChanged);
                this.currentDataTable.TableCleared += new DataTableClearEventHandler(this.DataTableCleared);
                this.isSubscribed = true;
            }
        }

        private void UnSubscribeEvents()
        {
            if ((this.currentDataTable != null) && this.isSubscribed)
            {
                this.currentDataTable.Columns.ColumnPropertyChanged -= new CollectionChangeEventHandler(this.SchemaChanged);
                this.currentDataTable.Columns.CollectionChanged -= new CollectionChangeEventHandler(this.SchemaChanged);
                this.currentDataTable.RowChanged -= new DataRowChangeEventHandler(this.DataChanged);
                this.currentDataTable.RowDeleted -= new DataRowChangeEventHandler(this.DataChanged);
                this.currentDataTable.TableCleared -= new DataTableClearEventHandler(this.DataTableCleared);
                this.isSubscribed = false;
            }
        }

        internal void UpdataTable(DataTable datatable)
        {
            if (datatable == null)
            {
                throw ExceptionBuilder.ArgumentNull("DataTable");
            }
            this.UnSubscribeEvents();
            this.currentDataTable = datatable;
            this.SubscribeEvents();
        }
    }
}

