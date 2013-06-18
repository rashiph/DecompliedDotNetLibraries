namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal class ChainedCloseAsyncResult : ChainedAsyncResult
    {
        private IList<ICommunicationObject> collection;

        public ChainedCloseAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ChainedBeginHandler begin1, ChainedEndHandler end1, IList<ICommunicationObject> collection) : base(timeout, callback, state)
        {
            this.collection = collection;
            base.Begin(new ChainedBeginHandler(this.BeginClose), new ChainedEndHandler(this.EndClose), begin1, end1);
        }

        public ChainedCloseAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ChainedBeginHandler begin1, ChainedEndHandler end1, params ICommunicationObject[] objs) : base(timeout, callback, state)
        {
            this.collection = new List<ICommunicationObject>();
            if (objs != null)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i] != null)
                    {
                        this.collection.Add(objs[i]);
                    }
                }
            }
            base.Begin(new ChainedBeginHandler(this.BeginClose), new ChainedEndHandler(this.EndClose), begin1, end1);
        }

        private IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseCollectionAsyncResult(timeout, callback, state, this.collection);
        }

        private void EndClose(IAsyncResult result)
        {
            CloseCollectionAsyncResult.End((CloseCollectionAsyncResult) result);
        }
    }
}

