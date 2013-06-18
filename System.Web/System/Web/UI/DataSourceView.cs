namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;

    public abstract class DataSourceView
    {
        private EventHandlerList _events;
        private string _name;
        private static readonly object EventDataSourceViewChanged = new object();

        public event EventHandler DataSourceViewChanged
        {
            add
            {
                this.Events.AddHandler(EventDataSourceViewChanged, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventDataSourceViewChanged, value);
            }
        }

        protected DataSourceView(IDataSource owner, string viewName)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (viewName == null)
            {
                throw new ArgumentNullException("viewName");
            }
            this._name = viewName;
            DataSourceControl control = owner as DataSourceControl;
            if (control != null)
            {
                control.DataSourceChangedInternal += new EventHandler(this.OnDataSourceChangedInternal);
            }
            else
            {
                owner.DataSourceChanged += new EventHandler(this.OnDataSourceChangedInternal);
            }
        }

        public virtual bool CanExecute(string commandName)
        {
            return false;
        }

        public virtual void Delete(IDictionary keys, IDictionary oldValues, DataSourceViewOperationCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            int affectedRecords = 0;
            bool flag = false;
            try
            {
                affectedRecords = this.ExecuteDelete(keys, oldValues);
            }
            catch (Exception exception)
            {
                flag = true;
                if (!callback(affectedRecords, exception))
                {
                    throw;
                }
            }
            finally
            {
                if (!flag)
                {
                    callback(affectedRecords, null);
                }
            }
        }

        protected virtual int ExecuteCommand(string commandName, IDictionary keys, IDictionary values)
        {
            throw new NotSupportedException();
        }

        public virtual void ExecuteCommand(string commandName, IDictionary keys, IDictionary values, DataSourceViewOperationCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            int affectedRecords = 0;
            bool flag = false;
            try
            {
                affectedRecords = this.ExecuteCommand(commandName, keys, values);
            }
            catch (Exception exception)
            {
                flag = true;
                if (!callback(affectedRecords, exception))
                {
                    throw;
                }
            }
            finally
            {
                if (!flag)
                {
                    callback(affectedRecords, null);
                }
            }
        }

        protected virtual int ExecuteDelete(IDictionary keys, IDictionary oldValues)
        {
            throw new NotSupportedException();
        }

        protected virtual int ExecuteInsert(IDictionary values)
        {
            throw new NotSupportedException();
        }

        protected internal abstract IEnumerable ExecuteSelect(DataSourceSelectArguments arguments);
        protected virtual int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            throw new NotSupportedException();
        }

        public virtual void Insert(IDictionary values, DataSourceViewOperationCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            int affectedRecords = 0;
            bool flag = false;
            try
            {
                affectedRecords = this.ExecuteInsert(values);
            }
            catch (Exception exception)
            {
                flag = true;
                if (!callback(affectedRecords, exception))
                {
                    throw;
                }
            }
            finally
            {
                if (!flag)
                {
                    callback(affectedRecords, null);
                }
            }
        }

        private void OnDataSourceChangedInternal(object sender, EventArgs e)
        {
            this.OnDataSourceViewChanged(e);
        }

        protected virtual void OnDataSourceViewChanged(EventArgs e)
        {
            EventHandler handler = this.Events[EventDataSourceViewChanged] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal virtual void RaiseUnsupportedCapabilityError(DataSourceCapabilities capability)
        {
            if (!this.CanPage && ((capability & DataSourceCapabilities.Page) != DataSourceCapabilities.None))
            {
                throw new NotSupportedException(System.Web.SR.GetString("DataSourceView_NoPaging"));
            }
            if (!this.CanSort && ((capability & DataSourceCapabilities.Sort) != DataSourceCapabilities.None))
            {
                throw new NotSupportedException(System.Web.SR.GetString("DataSourceView_NoSorting"));
            }
            if (!this.CanRetrieveTotalRowCount && ((capability & DataSourceCapabilities.RetrieveTotalRowCount) != DataSourceCapabilities.None))
            {
                throw new NotSupportedException(System.Web.SR.GetString("DataSourceView_NoRowCount"));
            }
        }

        public virtual void Select(DataSourceSelectArguments arguments, DataSourceViewSelectCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            callback(this.ExecuteSelect(arguments));
        }

        public virtual void Update(IDictionary keys, IDictionary values, IDictionary oldValues, DataSourceViewOperationCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            int affectedRecords = 0;
            bool flag = false;
            try
            {
                affectedRecords = this.ExecuteUpdate(keys, values, oldValues);
            }
            catch (Exception exception)
            {
                flag = true;
                if (!callback(affectedRecords, exception))
                {
                    throw;
                }
            }
            finally
            {
                if (!flag)
                {
                    callback(affectedRecords, null);
                }
            }
        }

        public virtual bool CanDelete
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanInsert
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanPage
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanRetrieveTotalRowCount
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanSort
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanUpdate
        {
            get
            {
                return false;
            }
        }

        protected EventHandlerList Events
        {
            get
            {
                if (this._events == null)
                {
                    this._events = new EventHandlerList();
                }
                return this._events;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

