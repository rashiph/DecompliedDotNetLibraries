namespace System.Web.SessionState
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web;

    public static class SessionStateUtility
    {
        internal const string SESSION_KEY = "AspSession";

        internal static void AddDelayedHttpSessionStateToContext(HttpContext context, SessionStateModule module)
        {
            context.AddDelayedHttpSessionState(module);
        }

        public static void AddHttpSessionStateToContext(HttpContext context, IHttpSessionState container)
        {
            HttpSessionState state = new HttpSessionState(container);
            try
            {
                context.Items.Add("AspSession", state);
            }
            catch (ArgumentException)
            {
                throw new HttpException(System.Web.SR.GetString("Cant_have_multiple_session_module"));
            }
        }

        internal static SessionStateStoreData CreateLegitStoreData(HttpContext context, ISessionStateItemCollection sessionItems, HttpStaticObjectsCollection staticObjects, int timeout)
        {
            if (sessionItems == null)
            {
                sessionItems = new SessionStateItemCollection();
            }
            if ((staticObjects == null) && (context != null))
            {
                staticObjects = GetSessionStaticObjects(context);
            }
            return new SessionStateStoreData(sessionItems, staticObjects, timeout);
        }

        [SecurityPermission(SecurityAction.Assert, SerializationFormatter=true)]
        internal static SessionStateStoreData Deserialize(HttpContext context, Stream stream)
        {
            int num;
            SessionStateItemCollection items;
            HttpStaticObjectsCollection sessionStaticObjects;
            try
            {
                BinaryReader reader = new BinaryReader(stream);
                num = reader.ReadInt32();
                bool flag = reader.ReadBoolean();
                bool flag2 = reader.ReadBoolean();
                if (flag)
                {
                    items = SessionStateItemCollection.Deserialize(reader);
                }
                else
                {
                    items = new SessionStateItemCollection();
                }
                if (flag2)
                {
                    sessionStaticObjects = HttpStaticObjectsCollection.Deserialize(reader);
                }
                else
                {
                    sessionStaticObjects = GetSessionStaticObjects(context);
                }
                if (reader.ReadByte() != 0xff)
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_session_state"));
                }
            }
            catch (EndOfStreamException)
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_session_state"));
            }
            return new SessionStateStoreData(items, sessionStaticObjects, num);
        }

        internal static SessionStateStoreData DeserializeStoreData(HttpContext context, Stream stream, bool compressionEnabled)
        {
            if (compressionEnabled)
            {
                using (DeflateStream stream2 = new DeflateStream(stream, CompressionMode.Decompress, true))
                {
                    return Deserialize(context, stream2);
                }
            }
            return Deserialize(context, stream);
        }

        public static IHttpSessionState GetHttpSessionStateFromContext(HttpContext context)
        {
            return context.Session.Container;
        }

        public static HttpStaticObjectsCollection GetSessionStaticObjects(HttpContext context)
        {
            return context.Application.SessionStaticObjects.Clone();
        }

        public static void RaiseSessionEnd(IHttpSessionState session, object eventSource, EventArgs eventArgs)
        {
            HttpApplicationFactory.EndSession(new HttpSessionState(session), eventSource, eventArgs);
        }

        public static void RemoveHttpSessionStateFromContext(HttpContext context)
        {
            RemoveHttpSessionStateFromContext(context, false);
        }

        internal static void RemoveHttpSessionStateFromContext(HttpContext context, bool delayed)
        {
            if (delayed)
            {
                context.RemoveDelayedHttpSessionState();
            }
            else
            {
                context.Items.Remove("AspSession");
            }
        }

        [SecurityPermission(SecurityAction.Assert, SerializationFormatter=true)]
        internal static void Serialize(SessionStateStoreData item, Stream stream)
        {
            bool flag = true;
            bool flag2 = true;
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(item.Timeout);
            if ((item.Items == null) || (item.Items.Count == 0))
            {
                flag = false;
            }
            writer.Write(flag);
            if ((item.StaticObjects == null) || item.StaticObjects.NeverAccessed)
            {
                flag2 = false;
            }
            writer.Write(flag2);
            if (flag)
            {
                ((SessionStateItemCollection) item.Items).Serialize(writer);
            }
            if (flag2)
            {
                item.StaticObjects.Serialize(writer);
            }
            writer.Write((byte) 0xff);
        }

        internal static void SerializeStoreData(SessionStateStoreData item, int initialStreamSize, out byte[] buf, out int length, bool compressionEnabled)
        {
            using (MemoryStream stream = new MemoryStream(initialStreamSize))
            {
                Serialize(item, stream);
                if (compressionEnabled)
                {
                    byte[] buffer = stream.GetBuffer();
                    int count = (int) stream.Length;
                    stream.SetLength(0L);
                    using (DeflateStream stream2 = new DeflateStream(stream, CompressionMode.Compress, true))
                    {
                        stream2.Write(buffer, 0, count);
                    }
                    stream.WriteByte(0xff);
                }
                buf = stream.GetBuffer();
                length = (int) stream.Length;
            }
        }
    }
}

