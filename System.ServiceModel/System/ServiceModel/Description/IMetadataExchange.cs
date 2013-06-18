namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(ConfigurationName="IMetadataExchange", Name="IMetadataExchange", Namespace="http://schemas.microsoft.com/2006/04/mex")]
    public interface IMetadataExchange
    {
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/09/transfer/Get", ReplyAction="http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse", AsyncPattern=true)]
        IAsyncResult BeginGet(Message request, AsyncCallback callback, object state);
        Message EndGet(IAsyncResult result);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/09/transfer/Get", ReplyAction="http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse")]
        Message Get(Message request);
    }
}

