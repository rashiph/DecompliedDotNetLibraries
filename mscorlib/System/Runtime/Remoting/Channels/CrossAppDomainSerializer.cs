namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;

    internal static class CrossAppDomainSerializer
    {
        [SecurityCritical]
        internal static IMessage DeserializeMessage(MemoryStream stm)
        {
            return DeserializeMessage(stm, null);
        }

        [SecurityCritical]
        internal static IMessage DeserializeMessage(MemoryStream stm, IMethodCallMessage reqMsg)
        {
            if (stm == null)
            {
                throw new ArgumentNullException("stm");
            }
            stm.Position = 0L;
            BinaryFormatter formatter = new BinaryFormatter {
                SurrogateSelector = null,
                Context = new StreamingContext(StreamingContextStates.CrossAppDomain)
            };
            return (IMessage) formatter.Deserialize(stm, null, false, true, reqMsg);
        }

        [SecurityCritical]
        internal static ArrayList DeserializeMessageParts(MemoryStream stm)
        {
            return (ArrayList) DeserializeObject(stm);
        }

        [SecurityCritical]
        internal static object DeserializeObject(MemoryStream stm)
        {
            stm.Position = 0L;
            BinaryFormatter formatter = new BinaryFormatter {
                Context = new StreamingContext(StreamingContextStates.CrossAppDomain)
            };
            return formatter.Deserialize(stm, null, false, true, null);
        }

        [SecurityCritical]
        internal static MemoryStream SerializeMessage(IMessage msg)
        {
            MemoryStream serializationStream = new MemoryStream();
            RemotingSurrogateSelector selector = new RemotingSurrogateSelector();
            new BinaryFormatter { SurrogateSelector = selector, Context = new StreamingContext(StreamingContextStates.CrossAppDomain) }.Serialize(serializationStream, msg, null, false);
            serializationStream.Position = 0L;
            return serializationStream;
        }

        [SecurityCritical]
        internal static MemoryStream SerializeMessageParts(ArrayList argsToSerialize)
        {
            MemoryStream serializationStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            RemotingSurrogateSelector selector = new RemotingSurrogateSelector();
            formatter.SurrogateSelector = selector;
            formatter.Context = new StreamingContext(StreamingContextStates.CrossAppDomain);
            formatter.Serialize(serializationStream, argsToSerialize, null, false);
            serializationStream.Position = 0L;
            return serializationStream;
        }

        [SecurityCritical]
        internal static MemoryStream SerializeObject(object obj)
        {
            MemoryStream stm = new MemoryStream();
            SerializeObject(obj, stm);
            stm.Position = 0L;
            return stm;
        }

        [SecurityCritical]
        internal static void SerializeObject(object obj, MemoryStream stm)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            RemotingSurrogateSelector selector = new RemotingSurrogateSelector();
            formatter.SurrogateSelector = selector;
            formatter.Context = new StreamingContext(StreamingContextStates.CrossAppDomain);
            formatter.Serialize(stm, obj, null, false);
        }
    }
}

