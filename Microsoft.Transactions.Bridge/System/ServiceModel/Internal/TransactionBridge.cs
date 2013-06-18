namespace System.ServiceModel.Internal
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Bridge.Configuration;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [Guid("bffecca7-4069-49f9-b5ab-7ccbb078ed91"), ComVisible(true), ProgId("")]
    public class TransactionBridge : ITransactionBridge
    {
        private object bridgeConfig;
        private TransactionBridgeSection config;
        private List<TransactionManager> transactionManagers;

        public TransactionBridge()
        {
            PropagationProtocolsTracing.TraceVerbose("Transaction Bridge Created");
            this.transactionManagers = new List<TransactionManager>();
        }

        public void Init(object bridgeConfig)
        {
            this.bridgeConfig = bridgeConfig;
            PropagationProtocolsTracing.TraceVerbose("Initializing...");
            try
            {
                this.config = TransactionBridgeSection.GetSection();
            }
            catch (Exception exception)
            {
                PropagationProtocolsTracing.TraceError("Error reading configuration: " + exception);
                throw;
            }
            this.config.Protocols.AssertBothWsatProtocolVersions();
            try
            {
                PropagationProtocolsTracing.TraceVerbose("Reading transaction managers from configuration...");
                if (!IsAssemblyMicrosoftSigned(this.config.TransactionManagerType))
                {
                    PropagationProtocolsTracing.TraceVerbose("Transaction manager type has wrong signature: " + this.config.TransactionManagerType);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(Microsoft.Transactions.SR.GetString("TransactionManagerTypeWrongSignature")));
                }
                PropagationProtocolsTracing.TraceVerbose("Loading transaction manager " + this.config.TransactionManagerType);
                Type type = Type.GetType(this.config.TransactionManagerType);
                if (type == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(Microsoft.Transactions.SR.GetString("CouldNotLoadTM", new object[] { this.config.TransactionManagerType })));
                }
                PropagationProtocolsTracing.TraceVerbose("Initializing transaction managers...");
                try
                {
                    foreach (ProtocolElement element in this.config.Protocols)
                    {
                        TransactionManager item = (TransactionManager) Activator.CreateInstance(type);
                        item.Initialize(element.Type, this.bridgeConfig);
                        this.transactionManagers.Add(item);
                    }
                }
                catch
                {
                    this.transactionManagers.Clear();
                    throw;
                }
            }
            catch (Exception exception2)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                PropagationProtocolsTracing.TraceError("Error initializing: " + exception2);
                throw;
            }
        }

        internal static bool IsAssemblyMicrosoftSigned(string assemblyQualifiedTypeName)
        {
            string[] strArray = assemblyQualifiedTypeName.Split(new char[] { ',' }, 2);
            if (strArray.Length == 2)
            {
                byte[] publicKeyToken = new AssemblyName(strArray[1].Trim()).GetPublicKeyToken();
                if (publicKeyToken != null)
                {
                    string strA = string.Empty;
                    foreach (byte num in publicKeyToken)
                    {
                        strA = strA + num.ToString("x2", CultureInfo.InvariantCulture);
                    }
                    return (string.Compare(strA, "b03f5f7f11d50a3a", StringComparison.OrdinalIgnoreCase) == 0);
                }
            }
            return false;
        }

        private void RecoverWorkItem(object obj)
        {
            try
            {
                PropagationProtocolsTracing.TraceVerbose("Recovering...");
                ((TransactionManager) obj).Recover();
            }
            catch (PluggableProtocolException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                PropagationProtocolsTracing.TraceError("Error recovering: " + exception);
            }
            catch (Exception exception2)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Critical);
                PropagationProtocolsTracing.TraceError("Unknown error recovering: " + exception2);
                TransactionBridgeRecoveryFailureRecord.TraceAndLog(exception2);
                DiagnosticUtility.InvokeFinalHandler(exception2);
            }
        }

        public void Shutdown()
        {
        }

        public void Start()
        {
            List<TransactionManager> list = new List<TransactionManager>();
            try
            {
                PropagationProtocolsTracing.TraceVerbose("Starting...");
                Action<object> callback = new Action<object>(this.RecoverWorkItem);
                foreach (TransactionManager manager in this.transactionManagers)
                {
                    manager.Start();
                    list.Add(manager);
                    ActionItem.Schedule(callback, manager);
                }
            }
            catch (Exception exception)
            {
                foreach (TransactionManager manager2 in list)
                {
                    manager2.Stop();
                }
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                PropagationProtocolsTracing.TraceError("Error starting: " + exception);
                throw;
            }
        }
    }
}

