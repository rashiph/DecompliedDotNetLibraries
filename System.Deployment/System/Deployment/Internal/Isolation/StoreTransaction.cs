namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class StoreTransaction : IDisposable
    {
        private ArrayList _list = new ArrayList();
        private System.Deployment.Internal.Isolation.StoreTransactionOperation[] _storeOps;

        public void Add(System.Deployment.Internal.Isolation.StoreOperationInstallDeployment o)
        {
            this._list.Add(o);
        }

        public void Add(System.Deployment.Internal.Isolation.StoreOperationPinDeployment o)
        {
            this._list.Add(o);
        }

        public void Add(System.Deployment.Internal.Isolation.StoreOperationScavenge o)
        {
            this._list.Add(o);
        }

        public void Add(System.Deployment.Internal.Isolation.StoreOperationSetCanonicalizationContext o)
        {
            this._list.Add(o);
        }

        public void Add(System.Deployment.Internal.Isolation.StoreOperationSetDeploymentMetadata o)
        {
            this._list.Add(o);
        }

        public void Add(System.Deployment.Internal.Isolation.StoreOperationStageComponent o)
        {
            this._list.Add(o);
        }

        public void Add(System.Deployment.Internal.Isolation.StoreOperationStageComponentFile o)
        {
            this._list.Add(o);
        }

        public void Add(System.Deployment.Internal.Isolation.StoreOperationUninstallDeployment o)
        {
            this._list.Add(o);
        }

        public void Add(System.Deployment.Internal.Isolation.StoreOperationUnpinDeployment o)
        {
            this._list.Add(o);
        }

        [SecuritySafeCritical]
        private void Dispose(bool fDisposing)
        {
            if (fDisposing)
            {
                GC.SuppressFinalize(this);
            }
            System.Deployment.Internal.Isolation.StoreTransactionOperation[] operationArray = this._storeOps;
            this._storeOps = null;
            if (operationArray != null)
            {
                for (int i = 0; i != operationArray.Length; i++)
                {
                    System.Deployment.Internal.Isolation.StoreTransactionOperation operation = operationArray[i];
                    if (operation.Data.DataPtr != IntPtr.Zero)
                    {
                        switch (operation.Operation)
                        {
                            case System.Deployment.Internal.Isolation.StoreTransactionOperationType.SetCanonicalizationContext:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(System.Deployment.Internal.Isolation.StoreOperationSetCanonicalizationContext));
                                break;

                            case System.Deployment.Internal.Isolation.StoreTransactionOperationType.StageComponent:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(System.Deployment.Internal.Isolation.StoreOperationStageComponent));
                                break;

                            case System.Deployment.Internal.Isolation.StoreTransactionOperationType.PinDeployment:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(System.Deployment.Internal.Isolation.StoreOperationPinDeployment));
                                break;

                            case System.Deployment.Internal.Isolation.StoreTransactionOperationType.UnpinDeployment:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(System.Deployment.Internal.Isolation.StoreOperationUnpinDeployment));
                                break;

                            case System.Deployment.Internal.Isolation.StoreTransactionOperationType.StageComponentFile:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(System.Deployment.Internal.Isolation.StoreOperationStageComponentFile));
                                break;

                            case System.Deployment.Internal.Isolation.StoreTransactionOperationType.InstallDeployment:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(System.Deployment.Internal.Isolation.StoreOperationInstallDeployment));
                                break;

                            case System.Deployment.Internal.Isolation.StoreTransactionOperationType.UninstallDeployment:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(System.Deployment.Internal.Isolation.StoreOperationUninstallDeployment));
                                break;

                            case System.Deployment.Internal.Isolation.StoreTransactionOperationType.SetDeploymentMetadata:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(System.Deployment.Internal.Isolation.StoreOperationSetDeploymentMetadata));
                                break;

                            case System.Deployment.Internal.Isolation.StoreTransactionOperationType.Scavenge:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(System.Deployment.Internal.Isolation.StoreOperationScavenge));
                                break;
                        }
                        Marshal.FreeCoTaskMem(operation.Data.DataPtr);
                    }
                }
            }
        }

        ~StoreTransaction()
        {
            this.Dispose(false);
        }

        [SecuritySafeCritical]
        private System.Deployment.Internal.Isolation.StoreTransactionOperation[] GenerateStoreOpsList()
        {
            System.Deployment.Internal.Isolation.StoreTransactionOperation[] operationArray = new System.Deployment.Internal.Isolation.StoreTransactionOperation[this._list.Count];
            for (int i = 0; i != this._list.Count; i++)
            {
                object structure = this._list[i];
                Type type = structure.GetType();
                operationArray[i].Data.DataPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, operationArray[i].Data.DataPtr, false);
                if (type == typeof(System.Deployment.Internal.Isolation.StoreOperationSetCanonicalizationContext))
                {
                    operationArray[i].Operation = System.Deployment.Internal.Isolation.StoreTransactionOperationType.SetCanonicalizationContext;
                }
                else if (type == typeof(System.Deployment.Internal.Isolation.StoreOperationStageComponent))
                {
                    operationArray[i].Operation = System.Deployment.Internal.Isolation.StoreTransactionOperationType.StageComponent;
                }
                else if (type == typeof(System.Deployment.Internal.Isolation.StoreOperationPinDeployment))
                {
                    operationArray[i].Operation = System.Deployment.Internal.Isolation.StoreTransactionOperationType.PinDeployment;
                }
                else if (type == typeof(System.Deployment.Internal.Isolation.StoreOperationUnpinDeployment))
                {
                    operationArray[i].Operation = System.Deployment.Internal.Isolation.StoreTransactionOperationType.UnpinDeployment;
                }
                else if (type == typeof(System.Deployment.Internal.Isolation.StoreOperationStageComponentFile))
                {
                    operationArray[i].Operation = System.Deployment.Internal.Isolation.StoreTransactionOperationType.StageComponentFile;
                }
                else if (type == typeof(System.Deployment.Internal.Isolation.StoreOperationInstallDeployment))
                {
                    operationArray[i].Operation = System.Deployment.Internal.Isolation.StoreTransactionOperationType.InstallDeployment;
                }
                else if (type == typeof(System.Deployment.Internal.Isolation.StoreOperationUninstallDeployment))
                {
                    operationArray[i].Operation = System.Deployment.Internal.Isolation.StoreTransactionOperationType.UninstallDeployment;
                }
                else if (type == typeof(System.Deployment.Internal.Isolation.StoreOperationSetDeploymentMetadata))
                {
                    operationArray[i].Operation = System.Deployment.Internal.Isolation.StoreTransactionOperationType.SetDeploymentMetadata;
                }
                else
                {
                    if (type != typeof(System.Deployment.Internal.Isolation.StoreOperationScavenge))
                    {
                        throw new Exception("How did you get here?");
                    }
                    operationArray[i].Operation = System.Deployment.Internal.Isolation.StoreTransactionOperationType.Scavenge;
                }
            }
            return operationArray;
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        public System.Deployment.Internal.Isolation.StoreTransactionOperation[] Operations
        {
            get
            {
                if (this._storeOps == null)
                {
                    this._storeOps = this.GenerateStoreOpsList();
                }
                return this._storeOps;
            }
        }
    }
}

