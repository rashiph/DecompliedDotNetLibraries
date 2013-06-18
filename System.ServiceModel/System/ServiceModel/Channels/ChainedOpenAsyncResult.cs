namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal class ChainedOpenAsyncResult : ChainedAsyncResult
    {
        private IList<ICommunicationObject> collection;

        public ChainedOpenAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ChainedBeginHandler begin1, ChainedEndHandler end1, IList<ICommunicationObject> collection) : base(timeout, callback, state)
        {
            this.collection = collection;
            base.Begin(begin1, end1, new ChainedBeginHandler(this.BeginOpen), new ChainedEndHandler(this.EndOpen));
        }

        public ChainedOpenAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ChainedBeginHandler begin1, ChainedEndHandler end1, params ICommunicationObject[] objs) : base(timeout, callback, state)
        {
            this.collection = new List<ICommunicationObject>();
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] != null)
                {
                    this.collection.Add(objs[i]);
                }
            }
            base.Begin(begin1, end1, new ChainedBeginHandler(this.BeginOpen), new ChainedEndHandler(this.EndOpen));
        }

        private IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenCollectionAsyncResult(timeout, callback, state, this.collection);
        }

        private void EndOpen(IAsyncResult result)
        {
            OpenCollectionAsyncResult.End((OpenCollectionAsyncResult) result);
        }
    }
}

