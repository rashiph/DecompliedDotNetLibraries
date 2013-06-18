namespace System.EnterpriseServices
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;

    internal sealed class ComponentSerializer
    {
        private BinaryFormatter _formatter = new BinaryFormatter();
        private HeaderHandler _headerhandler;
        private ISurrogateSelector _selector = new ComSurrogateSelector();
        private static InterlockedStack _stack = new InterlockedStack();
        private MemoryStream _stream = new MemoryStream(0);
        private StreamingContext _streamingCtx = new StreamingContext(StreamingContextStates.Other);
        private object _tp;
        private static readonly int MaxBuffersCached = 40;
        private static readonly int MaxCachedBufferLength = 0x40000;

        public ComponentSerializer()
        {
            this._formatter.Context = this._streamingCtx;
            this._headerhandler = new HeaderHandler(this.TPHeaderHandler);
        }

        internal static ComponentSerializer Get()
        {
            ComponentSerializer serializer = (ComponentSerializer) _stack.Pop();
            if (serializer == null)
            {
                serializer = new ComponentSerializer();
            }
            return serializer;
        }

        internal byte[] MarshalToBuffer(object o, out long numBytes)
        {
            this.SetStream(null);
            this._formatter.SurrogateSelector = this._selector;
            this._formatter.AssemblyFormat = FormatterAssemblyStyle.Full;
            this._formatter.Serialize(this._stream, o, null);
            numBytes = this._stream.Position;
            if ((numBytes % 2L) != 0L)
            {
                this._stream.WriteByte(0);
                numBytes += 1L;
            }
            return this._stream.GetBuffer();
        }

        internal void Release()
        {
            if ((_stack.Count < MaxBuffersCached) && (this._stream.Capacity < MaxCachedBufferLength))
            {
                _stack.Push(this);
            }
        }

        internal void SetStream(byte[] b)
        {
            this._stream.SetLength(0L);
            if (b != null)
            {
                this._stream.Write(b, 0, b.Length);
                this._stream.Position = 0L;
            }
        }

        public object TPHeaderHandler(Header[] Headers)
        {
            return this._tp;
        }

        internal object UnmarshalFromBuffer(byte[] b, object tp)
        {
            object obj2 = null;
            this.SetStream(b);
            this._tp = tp;
            try
            {
                this._formatter.SurrogateSelector = null;
                this._formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
                obj2 = this._formatter.Deserialize(this._stream, this._headerhandler);
            }
            finally
            {
                this._tp = null;
            }
            return obj2;
        }

        internal object UnmarshalReturnMessageFromBuffer(byte[] b, IMethodCallMessage msg)
        {
            this.SetStream(b);
            this._formatter.SurrogateSelector = null;
            this._formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
            return this._formatter.DeserializeMethodResponse(this._stream, null, msg);
        }
    }
}

