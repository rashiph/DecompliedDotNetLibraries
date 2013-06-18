namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;

    internal class DataContractSerializerFaultFormatter : FaultFormatter
    {
        internal DataContractSerializerFaultFormatter(Type[] detailTypes) : base(detailTypes)
        {
        }

        internal DataContractSerializerFaultFormatter(SynchronizedCollection<FaultContractInfo> faultContractInfoCollection) : base(faultContractInfoCollection)
        {
        }
    }
}

