namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class OptionalMessageQuery : MessageQuery
    {
        public override TResult Evaluate<TResult>(Message message)
        {
            return this.Query.Evaluate<TResult>(message);
        }

        public override TResult Evaluate<TResult>(MessageBuffer buffer)
        {
            return this.Query.Evaluate<TResult>(buffer);
        }

        public MessageQuery Query { get; set; }
    }
}

