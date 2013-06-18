namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Configuration;

    internal class ContractInfo
    {
        private Guid iid;
        private string[] interfaceRoleMembers;
        private string name;
        private List<OperationInfo> operations;

        public ContractInfo(Guid iid, ServiceEndpointElement endpoint, ComCatalogObject interfaceObject, ComCatalogObject application)
        {
            this.name = endpoint.Contract;
            this.iid = iid;
            ComCatalogCollection collection = interfaceObject.GetCollection("RolesForInterface");
            this.interfaceRoleMembers = CatalogUtil.GetRoleMembers(application, collection);
            this.operations = new List<OperationInfo>();
            ComCatalogCollection.Enumerator enumerator = interfaceObject.GetCollection("MethodsForInterface").GetEnumerator();
            while (enumerator.MoveNext())
            {
                ComCatalogObject current = enumerator.Current;
                this.operations.Add(new OperationInfo(current, application));
            }
        }

        public Guid IID
        {
            get
            {
                return this.iid;
            }
        }

        public string[] InterfaceRoleMembers
        {
            get
            {
                return this.interfaceRoleMembers;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public List<OperationInfo> Operations
        {
            get
            {
                return this.operations;
            }
        }
    }
}

