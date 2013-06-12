namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class StoreTransaction : IDisposable
    {
        private ArrayList _list = new ArrayList();
        private StoreTransactionOperation[] _storeOps;

        public void Add(StoreOperationInstallDeployment o)
        {
            this._list.Add(o);
        }

        public void Add(StoreOperationPinDeployment o)
        {
            this._list.Add(o);
        }

        public void Add(StoreOperationScavenge o)
        {
            this._list.Add(o);
        }

        public void Add(StoreOperationSetCanonicalizationContext o)
        {
            this._list.Add(o);
        }

        public void Add(StoreOperationSetDeploymentMetadata o)
        {
            this._list.Add(o);
        }

        public void Add(StoreOperationStageComponent o)
        {
            this._list.Add(o);
        }

        public void Add(StoreOperationStageComponentFile o)
        {
            this._list.Add(o);
        }

        public void Add(StoreOperationUninstallDeployment o)
        {
            this._list.Add(o);
        }

        public void Add(StoreOperationUnpinDeployment o)
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
            StoreTransactionOperation[] operationArray = this._storeOps;
            this._storeOps = null;
            if (operationArray != null)
            {
                for (int i = 0; i != operationArray.Length; i++)
                {
                    StoreTransactionOperation operation = operationArray[i];
                    if (operation.Data.DataPtr != IntPtr.Zero)
                    {
                        switch (operation.Operation)
                        {
                            case StoreTransactionOperationType.SetCanonicalizationContext:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(StoreOperationSetCanonicalizationContext));
                                break;

                            case StoreTransactionOperationType.StageComponent:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(StoreOperationStageComponent));
                                break;

                            case StoreTransactionOperationType.PinDeployment:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(StoreOperationPinDeployment));
                                break;

                            case StoreTransactionOperationType.UnpinDeployment:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(StoreOperationUnpinDeployment));
                                break;

                            case StoreTransactionOperationType.StageComponentFile:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(StoreOperationStageComponentFile));
                                break;

                            case StoreTransactionOperationType.InstallDeployment:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(StoreOperationInstallDeployment));
                                break;

                            case StoreTransactionOperationType.UninstallDeployment:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(StoreOperationUninstallDeployment));
                                break;

                            case StoreTransactionOperationType.SetDeploymentMetadata:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(StoreOperationSetDeploymentMetadata));
                                break;

                            case StoreTransactionOperationType.Scavenge:
                                Marshal.DestroyStructure(operation.Data.DataPtr, typeof(StoreOperationScavenge));
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
        private StoreTransactionOperation[] GenerateStoreOpsList()
        {
            StoreTransactionOperation[] operationArray = new StoreTransactionOperation[this._list.Count];
            for (int i = 0; i != this._list.Count; i++)
            {
                object structure = this._list[i];
                Type type = structure.GetType();
                operationArray[i].Data.DataPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, operationArray[i].Data.DataPtr, false);
                if (type == typeof(StoreOperationSetCanonicalizationContext))
                {
                    operationArray[i].Operation = StoreTransactionOperationType.SetCanonicalizationContext;
                }
                else if (type == typeof(StoreOperationStageComponent))
                {
                    operationArray[i].Operation = StoreTransactionOperationType.StageComponent;
                }
                else if (type == typeof(StoreOperationPinDeployment))
                {
                    operationArray[i].Operation = StoreTransactionOperationType.PinDeployment;
                }
                else if (type == typeof(StoreOperationUnpinDeployment))
                {
                    operationArray[i].Operation = StoreTransactionOperationType.UnpinDeployment;
                }
                else if (type == typeof(StoreOperationStageComponentFile))
                {
                    operationArray[i].Operation = StoreTransactionOperationType.StageComponentFile;
                }
                else if (type == typeof(StoreOperationInstallDeployment))
                {
                    operationArray[i].Operation = StoreTransactionOperationType.InstallDeployment;
                }
                else if (type == typeof(StoreOperationUninstallDeployment))
                {
                    operationArray[i].Operation = StoreTransactionOperationType.UninstallDeployment;
                }
                else if (type == typeof(StoreOperationSetDeploymentMetadata))
                {
                    operationArray[i].Operation = StoreTransactionOperationType.SetDeploymentMetadata;
                }
                else
                {
                    if (type != typeof(StoreOperationScavenge))
                    {
                        throw new Exception("How did you get here?");
                    }
                    operationArray[i].Operation = StoreTransactionOperationType.Scavenge;
                }
            }
            return operationArray;
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        public StoreTransactionOperation[] Operations
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

