namespace System.Runtime.Remoting.Services
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Threading;

    [SecurityCritical, ComVisible(true)]
    public class TrackingServices
    {
        private static ITrackingHandler[] _Handlers = new ITrackingHandler[0];
        private static int _Size = 0;
        private static object s_TrackingServicesSyncObject = null;

        [SecurityCritical]
        internal static void DisconnectedObject(object obj)
        {
            try
            {
                ITrackingHandler[] handlerArray = _Handlers;
                for (int i = 0; i < _Size; i++)
                {
                    handlerArray[i].DisconnectedObject(obj);
                }
            }
            catch
            {
            }
        }

        [SecurityCritical]
        internal static void MarshaledObject(object obj, ObjRef or)
        {
            try
            {
                ITrackingHandler[] handlerArray = _Handlers;
                for (int i = 0; i < _Size; i++)
                {
                    handlerArray[i].MarshaledObject(obj, or);
                }
            }
            catch
            {
            }
        }

        private static int Match(ITrackingHandler handler)
        {
            for (int i = 0; i < _Size; i++)
            {
                if (_Handlers[i] == handler)
                {
                    return i;
                }
            }
            return -1;
        }

        [SecurityCritical]
        public static void RegisterTrackingHandler(ITrackingHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            lock (TrackingServicesSyncObject)
            {
                if (-1 != Match(handler))
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_TrackingHandlerAlreadyRegistered", new object[] { "handler" }));
                }
                if ((_Handlers == null) || (_Size == _Handlers.Length))
                {
                    ITrackingHandler[] destinationArray = new ITrackingHandler[(_Size * 2) + 4];
                    if (_Handlers != null)
                    {
                        Array.Copy(_Handlers, destinationArray, _Size);
                    }
                    _Handlers = destinationArray;
                }
                _Handlers[_Size++] = handler;
            }
        }

        [SecurityCritical]
        internal static void UnmarshaledObject(object obj, ObjRef or)
        {
            try
            {
                ITrackingHandler[] handlerArray = _Handlers;
                for (int i = 0; i < _Size; i++)
                {
                    handlerArray[i].UnmarshaledObject(obj, or);
                }
            }
            catch
            {
            }
        }

        [SecurityCritical]
        public static void UnregisterTrackingHandler(ITrackingHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            lock (TrackingServicesSyncObject)
            {
                int destinationIndex = Match(handler);
                if (-1 == destinationIndex)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_HandlerNotRegistered", new object[] { handler }));
                }
                Array.Copy(_Handlers, destinationIndex + 1, _Handlers, destinationIndex, (_Size - destinationIndex) - 1);
                _Size--;
            }
        }

        public static ITrackingHandler[] RegisteredHandlers
        {
            [SecurityCritical]
            get
            {
                lock (TrackingServicesSyncObject)
                {
                    if (_Size == 0)
                    {
                        return new ITrackingHandler[0];
                    }
                    ITrackingHandler[] handlerArray = new ITrackingHandler[_Size];
                    for (int i = 0; i < _Size; i++)
                    {
                        handlerArray[i] = _Handlers[i];
                    }
                    return handlerArray;
                }
            }
        }

        private static object TrackingServicesSyncObject
        {
            get
            {
                if (s_TrackingServicesSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_TrackingServicesSyncObject, obj2, null);
                }
                return s_TrackingServicesSyncObject;
            }
        }
    }
}

