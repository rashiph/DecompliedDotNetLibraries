namespace System.Web.Services.Description
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Threading;
    using System.Web.Services;

    public abstract class ServiceDescriptionBaseCollection : CollectionBase
    {
        private object parent;
        private Hashtable table;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ServiceDescriptionBaseCollection(object parent)
        {
            this.parent = parent;
        }

        private void AddValue(object value)
        {
            string key = this.GetKey(value);
            if (key != null)
            {
                try
                {
                    this.Table.Add(key, value);
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    if (this.Table[key] != null)
                    {
                        throw new ArgumentException(GetDuplicateMessage(value.GetType(), key), exception.InnerException);
                    }
                    throw exception;
                }
            }
            this.SetParent(value, this.parent);
        }

        private static string GetDuplicateMessage(Type type, string elemName)
        {
            if (type == typeof(ServiceDescriptionFormatExtension))
            {
                return Res.GetString("WebDuplicateFormatExtension", new object[] { elemName });
            }
            if (type == typeof(OperationMessage))
            {
                return Res.GetString("WebDuplicateOperationMessage", new object[] { elemName });
            }
            if (type == typeof(Import))
            {
                return Res.GetString("WebDuplicateImport", new object[] { elemName });
            }
            if (type == typeof(Message))
            {
                return Res.GetString("WebDuplicateMessage", new object[] { elemName });
            }
            if (type == typeof(Port))
            {
                return Res.GetString("WebDuplicatePort", new object[] { elemName });
            }
            if (type == typeof(PortType))
            {
                return Res.GetString("WebDuplicatePortType", new object[] { elemName });
            }
            if (type == typeof(Binding))
            {
                return Res.GetString("WebDuplicateBinding", new object[] { elemName });
            }
            if (type == typeof(Service))
            {
                return Res.GetString("WebDuplicateService", new object[] { elemName });
            }
            if (type == typeof(MessagePart))
            {
                return Res.GetString("WebDuplicateMessagePart", new object[] { elemName });
            }
            if (type == typeof(OperationBinding))
            {
                return Res.GetString("WebDuplicateOperationBinding", new object[] { elemName });
            }
            if (type == typeof(FaultBinding))
            {
                return Res.GetString("WebDuplicateFaultBinding", new object[] { elemName });
            }
            if (type == typeof(Operation))
            {
                return Res.GetString("WebDuplicateOperation", new object[] { elemName });
            }
            if (type == typeof(OperationFault))
            {
                return Res.GetString("WebDuplicateOperationFault", new object[] { elemName });
            }
            return Res.GetString("WebDuplicateUnknownElement", new object[] { type, elemName });
        }

        protected virtual string GetKey(object value)
        {
            return null;
        }

        protected override void OnClear()
        {
            for (int i = 0; i < base.List.Count; i++)
            {
                this.RemoveValue(base.List[i]);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected override void OnInsertComplete(int index, object value)
        {
            this.AddValue(value);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected override void OnRemove(int index, object value)
        {
            this.RemoveValue(value);
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            this.RemoveValue(oldValue);
            this.AddValue(newValue);
        }

        private void RemoveValue(object value)
        {
            string key = this.GetKey(value);
            if (key != null)
            {
                this.Table.Remove(key);
            }
            this.SetParent(value, null);
        }

        protected virtual void SetParent(object value, object parent)
        {
        }

        protected virtual IDictionary Table
        {
            get
            {
                if (this.table == null)
                {
                    this.table = new Hashtable();
                }
                return this.table;
            }
        }
    }
}

