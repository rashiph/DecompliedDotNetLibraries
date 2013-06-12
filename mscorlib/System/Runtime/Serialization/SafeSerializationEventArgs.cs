namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;

    public sealed class SafeSerializationEventArgs : EventArgs
    {
        private List<object> m_serializedStates = new List<object>();
        private System.Runtime.Serialization.StreamingContext m_streamingContext;

        internal SafeSerializationEventArgs(System.Runtime.Serialization.StreamingContext streamingContext)
        {
            this.m_streamingContext = streamingContext;
        }

        public void AddSerializedState(ISafeSerializationData serializedState)
        {
            if (serializedState == null)
            {
                throw new ArgumentNullException("serializedState");
            }
            if (!serializedState.GetType().IsSerializable)
            {
                throw new ArgumentException(Environment.GetResourceString("Serialization_NonSerType", new object[] { serializedState.GetType(), serializedState.GetType().Assembly.FullName }));
            }
            this.m_serializedStates.Add(serializedState);
        }

        internal IList<object> SerializedStates
        {
            get
            {
                return this.m_serializedStates;
            }
        }

        public System.Runtime.Serialization.StreamingContext StreamingContext
        {
            get
            {
                return this.m_streamingContext;
            }
        }
    }
}

