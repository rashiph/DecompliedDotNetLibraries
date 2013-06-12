namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    internal sealed class DataViewListener
    {
        private readonly WeakReference _dvWeak;
        private Index _index;
        private DataTable _table;
        internal readonly int ObjectID;

        internal DataViewListener(DataView dv)
        {
            this.ObjectID = dv.ObjectID;
            this._dvWeak = new WeakReference(dv);
        }

        private void ChildRelationCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            DataView target = (DataView) this._dvWeak.Target;
            if (target != null)
            {
                target.ChildRelationCollectionChanged(sender, e);
            }
            else
            {
                this.CleanUp(true);
            }
        }

        private void CleanUp(bool updateListeners)
        {
            this.UnregisterMetaDataEvents(updateListeners);
            this.UnregisterListChangedEvent();
        }

        private void ColumnCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            DataView target = (DataView) this._dvWeak.Target;
            if (target != null)
            {
                target.ColumnCollectionChangedInternal(sender, e);
            }
            else
            {
                this.CleanUp(true);
            }
        }

        internal void IndexListChanged(ListChangedEventArgs e)
        {
            DataView target = (DataView) this._dvWeak.Target;
            if (target != null)
            {
                target.IndexListChangedInternal(e);
            }
            else
            {
                this.CleanUp(true);
            }
        }

        internal void MaintainDataView(ListChangedType changedType, DataRow row, bool trackAddRemove)
        {
            DataView target = (DataView) this._dvWeak.Target;
            if (target != null)
            {
                target.MaintainDataView(changedType, row, trackAddRemove);
            }
            else
            {
                this.CleanUp(true);
            }
        }

        private void ParentRelationCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            DataView target = (DataView) this._dvWeak.Target;
            if (target != null)
            {
                target.ParentRelationCollectionChanged(sender, e);
            }
            else
            {
                this.CleanUp(true);
            }
        }

        internal void RegisterListChangedEvent(Index index)
        {
            this._index = index;
            if (index != null)
            {
                lock (index)
                {
                    index.AddRef();
                    index.ListChangedAdd(this);
                }
            }
        }

        private void RegisterListener(DataTable table)
        {
            List<DataViewListener> listeners = table.GetListeners();
            lock (listeners)
            {
                for (int i = listeners.Count - 1; 0 <= i; i--)
                {
                    DataViewListener listener = listeners[i];
                    if (!listener._dvWeak.IsAlive)
                    {
                        listeners.RemoveAt(i);
                        listener.CleanUp(false);
                    }
                }
                listeners.Add(this);
            }
        }

        internal void RegisterMetaDataEvents(DataTable table)
        {
            this._table = table;
            if (table != null)
            {
                this.RegisterListener(table);
                CollectionChangeEventHandler handler3 = new CollectionChangeEventHandler(this.ColumnCollectionChanged);
                table.Columns.ColumnPropertyChanged += handler3;
                table.Columns.CollectionChanged += handler3;
                CollectionChangeEventHandler handler2 = new CollectionChangeEventHandler(this.ChildRelationCollectionChanged);
                ((DataRelationCollection.DataTableRelationCollection) table.ChildRelations).RelationPropertyChanged += handler2;
                table.ChildRelations.CollectionChanged += handler2;
                CollectionChangeEventHandler handler = new CollectionChangeEventHandler(this.ParentRelationCollectionChanged);
                ((DataRelationCollection.DataTableRelationCollection) table.ParentRelations).RelationPropertyChanged += handler;
                table.ParentRelations.CollectionChanged += handler;
            }
        }

        internal void UnregisterListChangedEvent()
        {
            Index index = this._index;
            this._index = null;
            if (index != null)
            {
                lock (index)
                {
                    index.ListChangedRemove(this);
                    if (index.RemoveRef() <= 1)
                    {
                        index.RemoveRef();
                    }
                }
            }
        }

        internal void UnregisterMetaDataEvents()
        {
            this.UnregisterMetaDataEvents(true);
        }

        private void UnregisterMetaDataEvents(bool updateListeners)
        {
            DataTable table = this._table;
            this._table = null;
            if (table != null)
            {
                CollectionChangeEventHandler handler3 = new CollectionChangeEventHandler(this.ColumnCollectionChanged);
                table.Columns.ColumnPropertyChanged -= handler3;
                table.Columns.CollectionChanged -= handler3;
                CollectionChangeEventHandler handler2 = new CollectionChangeEventHandler(this.ChildRelationCollectionChanged);
                ((DataRelationCollection.DataTableRelationCollection) table.ChildRelations).RelationPropertyChanged -= handler2;
                table.ChildRelations.CollectionChanged -= handler2;
                CollectionChangeEventHandler handler = new CollectionChangeEventHandler(this.ParentRelationCollectionChanged);
                ((DataRelationCollection.DataTableRelationCollection) table.ParentRelations).RelationPropertyChanged -= handler;
                table.ParentRelations.CollectionChanged -= handler;
                if (updateListeners)
                {
                    List<DataViewListener> listeners = table.GetListeners();
                    lock (listeners)
                    {
                        listeners.Remove(this);
                    }
                }
            }
        }
    }
}

