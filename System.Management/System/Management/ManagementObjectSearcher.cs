namespace System.Management
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [ToolboxItem(false)]
    public class ManagementObjectSearcher : Component
    {
        private EnumerationOptions options;
        private ObjectQuery query;
        private ManagementScope scope;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectSearcher() : this((ManagementScope) null, (ObjectQuery) null, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectSearcher(ObjectQuery query) : this(null, query, null)
        {
        }

        public ManagementObjectSearcher(string queryString) : this(null, new ObjectQuery(queryString), null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectSearcher(ManagementScope scope, ObjectQuery query) : this(scope, query, null)
        {
        }

        public ManagementObjectSearcher(string scope, string queryString) : this(new ManagementScope(scope), new ObjectQuery(queryString), null)
        {
        }

        public ManagementObjectSearcher(ManagementScope scope, ObjectQuery query, EnumerationOptions options)
        {
            this.scope = ManagementScope._Clone(scope);
            if (query != null)
            {
                this.query = (ObjectQuery) query.Clone();
            }
            else
            {
                this.query = new ObjectQuery();
            }
            if (options != null)
            {
                this.options = (EnumerationOptions) options.Clone();
            }
            else
            {
                this.options = new EnumerationOptions();
            }
        }

        public ManagementObjectSearcher(string scope, string queryString, EnumerationOptions options) : this(new ManagementScope(scope), new ObjectQuery(queryString), options)
        {
        }

        public ManagementObjectCollection Get()
        {
            this.Initialize();
            IEnumWbemClassObject ppEnum = null;
            SecurityHandler securityHandler = this.scope.GetSecurityHandler();
            EnumerationOptions options = (EnumerationOptions) this.options.Clone();
            int errorCode = 0;
            try
            {
                if (((this.query.GetType() == typeof(SelectQuery)) && (((SelectQuery) this.query).Condition == null)) && ((((SelectQuery) this.query).SelectedProperties == null) && this.options.EnumerateDeep))
                {
                    options.EnsureLocatable = false;
                    options.PrototypeOnly = false;
                    if (!((SelectQuery) this.query).IsSchemaQuery)
                    {
                        errorCode = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).CreateInstanceEnum_(((SelectQuery) this.query).ClassName, options.Flags, options.GetContext(), ref ppEnum);
                    }
                    else
                    {
                        errorCode = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).CreateClassEnum_(((SelectQuery) this.query).ClassName, options.Flags, options.GetContext(), ref ppEnum);
                    }
                }
                else
                {
                    options.EnumerateDeep = true;
                    errorCode = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecQuery_(this.query.QueryLanguage, this.query.QueryString, options.Flags, options.GetContext(), ref ppEnum);
                }
            }
            catch (COMException exception)
            {
                ManagementException.ThrowWithExtendedInfo(exception);
            }
            finally
            {
                securityHandler.Reset();
            }
            if ((errorCode & 0xfffff000L) == 0x80041000L)
            {
                ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
            }
            else if ((errorCode & 0x80000000L) != 0L)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return new ManagementObjectCollection(this.scope, this.options, ppEnum);
        }

        public void Get(ManagementOperationObserver watcher)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            this.Initialize();
            IWbemServices iWbemServices = this.scope.GetIWbemServices();
            EnumerationOptions options = (EnumerationOptions) this.options.Clone();
            options.ReturnImmediately = false;
            if (watcher.HaveListenersForProgress)
            {
                options.SendStatus = true;
            }
            WmiEventSink newSink = watcher.GetNewSink(this.scope, options.Context);
            SecurityHandler securityHandler = this.scope.GetSecurityHandler();
            int errorCode = 0;
            try
            {
                if (((this.query.GetType() == typeof(SelectQuery)) && (((SelectQuery) this.query).Condition == null)) && ((((SelectQuery) this.query).SelectedProperties == null) && this.options.EnumerateDeep))
                {
                    options.EnsureLocatable = false;
                    options.PrototypeOnly = false;
                    if (!((SelectQuery) this.query).IsSchemaQuery)
                    {
                        errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).CreateInstanceEnumAsync_(((SelectQuery) this.query).ClassName, options.Flags, options.GetContext(), newSink.Stub);
                    }
                    else
                    {
                        errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).CreateClassEnumAsync_(((SelectQuery) this.query).ClassName, options.Flags, options.GetContext(), newSink.Stub);
                    }
                }
                else
                {
                    options.EnumerateDeep = true;
                    errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).ExecQueryAsync_(this.query.QueryLanguage, this.query.QueryString, options.Flags, options.GetContext(), newSink.Stub);
                }
            }
            catch (COMException exception)
            {
                watcher.RemoveSink(newSink);
                ManagementException.ThrowWithExtendedInfo(exception);
            }
            finally
            {
                securityHandler.Reset();
            }
            if ((errorCode & 0xfffff000L) == 0x80041000L)
            {
                ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
            }
            else if ((errorCode & 0x80000000L) != 0L)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        private void Initialize()
        {
            if (this.query == null)
            {
                throw new InvalidOperationException();
            }
            lock (this)
            {
                if (this.scope == null)
                {
                    this.scope = ManagementScope._Clone(null);
                }
            }
            lock (this.scope)
            {
                if (!this.scope.IsConnected)
                {
                    this.scope.Initialize();
                }
            }
        }

        public EnumerationOptions Options
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.options;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.options = (EnumerationOptions) value.Clone();
            }
        }

        public ObjectQuery Query
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.query;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.query = (ObjectQuery) value.Clone();
            }
        }

        public ManagementScope Scope
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.scope;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.scope = value.Clone();
            }
        }
    }
}

