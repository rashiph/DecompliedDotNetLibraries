namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal abstract class TypedHeaderManager
    {
        private static Dictionary<Type, TypedHeaderManager> cache = new Dictionary<Type, TypedHeaderManager>();
        private static ReaderWriterLock cacheLock = new ReaderWriterLock();
        private static Type GenericAdapterType = typeof(GenericAdapter);

        protected TypedHeaderManager()
        {
        }

        protected abstract object Create(object content, bool mustUnderstand, bool relay, string actor);
        internal static object Create(Type t, object content, bool mustUnderstand, bool relay, string actor)
        {
            return GetTypedHeaderManager(t).Create(content, mustUnderstand, relay, actor);
        }

        protected abstract object GetContent(object typedHeaderInstance, out bool mustUnderstand, out bool relay, out string actor);
        internal static object GetContent(Type t, object typedHeaderInstance, out bool mustUnderstand, out bool relay, out string actor)
        {
            return GetTypedHeaderManager(t).GetContent(typedHeaderInstance, out mustUnderstand, out relay, out actor);
        }

        internal static Type GetHeaderType(Type headerParameterType)
        {
            if (headerParameterType.IsGenericType && (headerParameterType.GetGenericTypeDefinition() == typeof(MessageHeader<>)))
            {
                return headerParameterType.GetGenericArguments()[0];
            }
            return headerParameterType;
        }

        protected abstract Type GetMessageHeaderType();
        internal static Type GetMessageHeaderType(Type contentType)
        {
            return GetTypedHeaderManager(contentType).GetMessageHeaderType();
        }

        private static TypedHeaderManager GetTypedHeaderManager(Type t)
        {
            TypedHeaderManager manager = null;
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    cacheLock.AcquireReaderLock(0x7fffffff);
                    flag = true;
                }
                if (!cache.TryGetValue(t, out manager))
                {
                    cacheLock.UpgradeToWriterLock(0x7fffffff);
                    if (!cache.TryGetValue(t, out manager))
                    {
                        manager = (TypedHeaderManager) Activator.CreateInstance(GenericAdapterType.MakeGenericType(new Type[] { t }));
                        cache.Add(t, manager);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    cacheLock.ReleaseLock();
                }
            }
            return manager;
        }

        private class GenericAdapter<T> : TypedHeaderManager
        {
            protected override object Create(object content, bool mustUnderstand, bool relay, string actor)
            {
                return new MessageHeader<T> { Content = (T) content, MustUnderstand = mustUnderstand, Relay = relay, Actor = actor };
            }

            protected override object GetContent(object typedHeaderInstance, out bool mustUnderstand, out bool relay, out string actor)
            {
                mustUnderstand = false;
                relay = false;
                actor = null;
                if (typedHeaderInstance == null)
                {
                    return null;
                }
                MessageHeader<T> header = typedHeaderInstance as MessageHeader<T>;
                if (header == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException("typedHeaderInstance"));
                }
                mustUnderstand = header.MustUnderstand;
                relay = header.Relay;
                actor = header.Actor;
                return header.Content;
            }

            protected override Type GetMessageHeaderType()
            {
                return typeof(MessageHeader<T>);
            }
        }
    }
}

