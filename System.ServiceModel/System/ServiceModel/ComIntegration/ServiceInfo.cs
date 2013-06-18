namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.EnterpriseServices;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.Transactions;

    internal class ServiceInfo
    {
        private Guid appid;
        private System.ServiceModel.ComIntegration.Bitness bitness;
        private bool checkRoles;
        private Guid clsid;
        private string[] componentRoleMembers;
        private List<ContractInfo> contracts;
        private System.ServiceModel.ComIntegration.HostingMode hostingMode;
        private System.Transactions.IsolationLevel isolationLevel;
        private Type managedType;
        private int maxPoolSize;
        private bool objectPoolingEnabled;
        private Guid partitionId;
        private System.ServiceModel.Configuration.ServiceElement service;
        private string serviceName;
        private System.ServiceModel.ComIntegration.ThreadingModel threadingModel;
        private System.EnterpriseServices.TransactionOption transactionOption;
        private Dictionary<Guid, List<Type>> udts;

        public ServiceInfo(Guid clsid, System.ServiceModel.Configuration.ServiceElement service, ComCatalogObject application, ComCatalogObject classObject, System.ServiceModel.ComIntegration.HostingMode hostingMode)
        {
            this.service = service;
            this.clsid = clsid;
            this.appid = Fx.CreateGuid((string) application.GetValue("ID"));
            this.partitionId = Fx.CreateGuid((string) application.GetValue("AppPartitionID"));
            this.bitness = (System.ServiceModel.ComIntegration.Bitness) classObject.GetValue("Bitness");
            this.transactionOption = (System.EnterpriseServices.TransactionOption) classObject.GetValue("Transaction");
            this.hostingMode = hostingMode;
            this.managedType = TypeCacheManager.ResolveClsidToType(clsid);
            this.serviceName = application.Name + "." + classObject.Name;
            this.udts = new Dictionary<Guid, List<Type>>();
            COMAdminIsolationLevel level = (COMAdminIsolationLevel) classObject.GetValue("TxIsolationLevel");
            switch (level)
            {
                case COMAdminIsolationLevel.Any:
                    this.isolationLevel = System.Transactions.IsolationLevel.Unspecified;
                    break;

                case COMAdminIsolationLevel.ReadUncommitted:
                    this.isolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                    break;

                case COMAdminIsolationLevel.ReadCommitted:
                    this.isolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                    break;

                case COMAdminIsolationLevel.RepeatableRead:
                    this.isolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                    break;

                case COMAdminIsolationLevel.Serializable:
                    this.isolationLevel = System.Transactions.IsolationLevel.Serializable;
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("InvalidIsolationLevelValue", new object[] { this.clsid, level })));
            }
            switch (((COMAdminThreadingModel) classObject.GetValue("ThreadingModel")))
            {
                case COMAdminThreadingModel.Apartment:
                case COMAdminThreadingModel.Main:
                    this.threadingModel = System.ServiceModel.ComIntegration.ThreadingModel.STA;
                    this.objectPoolingEnabled = false;
                    break;

                default:
                    this.threadingModel = System.ServiceModel.ComIntegration.ThreadingModel.MTA;
                    this.objectPoolingEnabled = (bool) classObject.GetValue("ObjectPoolingEnabled");
                    break;
            }
            if (this.objectPoolingEnabled)
            {
                this.maxPoolSize = (int) classObject.GetValue("MaxPoolSize");
            }
            else
            {
                this.maxPoolSize = 0;
            }
            if (((bool) application.GetValue("ApplicationAccessChecksEnabled")) && ((bool) classObject.GetValue("ComponentAccessChecksEnabled")))
            {
                this.checkRoles = true;
            }
            ComCatalogCollection collection = classObject.GetCollection("RolesForComponent");
            this.componentRoleMembers = CatalogUtil.GetRoleMembers(application, collection);
            this.contracts = new List<ContractInfo>();
            ComCatalogCollection catalogs2 = classObject.GetCollection("InterfacesForComponent");
            foreach (ServiceEndpointElement element in service.Endpoints)
            {
                ContractInfo item = null;
                if (element.Contract != "IMetadataExchange")
                {
                    Guid guid;
                    if (DiagnosticUtility.Utility.TryCreateGuid(element.Contract, out guid))
                    {
                        bool flag3 = false;
                        foreach (ContractInfo info2 in this.contracts)
                        {
                            if (guid == info2.IID)
                            {
                                flag3 = true;
                                break;
                            }
                        }
                        if (flag3)
                        {
                            continue;
                        }
                        ComCatalogCollection.Enumerator enumerator = catalogs2.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            Guid guid2;
                            ComCatalogObject current = enumerator.Current;
                            if (DiagnosticUtility.Utility.TryCreateGuid((string) current.GetValue("IID"), out guid2) && (guid2 == guid))
                            {
                                item = new ContractInfo(guid, element, current, application);
                                break;
                            }
                        }
                    }
                    if (item == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("EndpointNotAnIID", new object[] { clsid.ToString("B").ToUpperInvariant(), element.Contract })));
                    }
                    this.contracts.Add(item);
                }
            }
        }

        internal void AddUdt(Type udt, Guid assemblyId)
        {
            if (!this.udts.ContainsKey(assemblyId))
            {
                this.udts[assemblyId] = new List<Type>();
            }
            if (!this.udts[assemblyId].Contains(udt))
            {
                this.udts[assemblyId].Add(udt);
            }
        }

        internal Type[] GetTypes(Guid assemblyId)
        {
            List<Type> list = null;
            this.udts.TryGetValue(assemblyId, out list);
            if (list == null)
            {
                return new Type[0];
            }
            return list.ToArray();
        }

        internal bool HasUdts()
        {
            return (this.udts.Keys.Count > 0);
        }

        public Guid AppID
        {
            get
            {
                return this.appid;
            }
        }

        internal Guid[] Assemblies
        {
            get
            {
                Guid[] array = new Guid[this.udts.Keys.Count];
                this.udts.Keys.CopyTo(array, 0);
                return array;
            }
        }

        public System.ServiceModel.ComIntegration.Bitness Bitness
        {
            get
            {
                return this.bitness;
            }
        }

        public bool CheckRoles
        {
            get
            {
                return this.checkRoles;
            }
        }

        public Guid Clsid
        {
            get
            {
                return this.clsid;
            }
        }

        public string[] ComponentRoleMembers
        {
            get
            {
                return this.componentRoleMembers;
            }
        }

        public List<ContractInfo> Contracts
        {
            get
            {
                return this.contracts;
            }
        }

        public System.ServiceModel.ComIntegration.HostingMode HostingMode
        {
            get
            {
                return this.hostingMode;
            }
        }

        public System.Transactions.IsolationLevel IsolationLevel
        {
            get
            {
                return this.isolationLevel;
            }
        }

        public int MaxPoolSize
        {
            get
            {
                return this.maxPoolSize;
            }
        }

        public Guid PartitionId
        {
            get
            {
                return this.partitionId;
            }
        }

        public bool Pooled
        {
            get
            {
                return this.objectPoolingEnabled;
            }
        }

        public System.ServiceModel.Configuration.ServiceElement ServiceElement
        {
            get
            {
                return this.service;
            }
        }

        public string ServiceName
        {
            get
            {
                return this.serviceName;
            }
        }

        public Type ServiceType
        {
            get
            {
                return this.managedType;
            }
        }

        public System.ServiceModel.ComIntegration.ThreadingModel ThreadingModel
        {
            get
            {
                return this.threadingModel;
            }
        }

        public System.EnterpriseServices.TransactionOption TransactionOption
        {
            get
            {
                return this.transactionOption;
            }
        }
    }
}

