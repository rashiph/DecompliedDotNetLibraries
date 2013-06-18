namespace System.ServiceModel.Activities
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal static class ClientOperationFormatterProvider
    {
        private static DispatchRuntime dummyDispatchRuntime;

        internal static IClientMessageFormatter GetFormatterFromRuntime(OperationDescription operationDescription)
        {
            ClientOperation clientOperation = new ClientOperation(DummyClientRuntime, operationDescription.Name, operationDescription.Messages[0].Action);
            foreach (IOperationBehavior behavior in operationDescription.Behaviors)
            {
                behavior.ApplyClientBehavior(operationDescription, clientOperation);
            }
            return clientOperation.Formatter;
        }

        private static ClientRuntime DummyClientRuntime
        {
            get
            {
                return DummyDispatchRuntime.CallbackClientRuntime;
            }
        }

        private static DispatchRuntime DummyDispatchRuntime
        {
            get
            {
                if (dummyDispatchRuntime == null)
                {
                    EndpointDispatcher dispatcher = new EndpointDispatcher(new EndpointAddress("http://dummyuri/"), "dummyContract", "urn:dummyContractNs");
                    dummyDispatchRuntime = dispatcher.DispatchRuntime;
                }
                return dummyDispatchRuntime;
            }
        }
    }
}

