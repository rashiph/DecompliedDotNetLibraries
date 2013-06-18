namespace System.Messaging
{
    using System;
    using System.Collections.Generic;

    internal class QueueAccessModeHolder
    {
        private QueueAccessMode accessMode;
        private static Dictionary<QueueAccessMode, QueueAccessModeHolder> holders = new Dictionary<QueueAccessMode, QueueAccessModeHolder>();

        private QueueAccessModeHolder(QueueAccessMode accessMode)
        {
            this.accessMode = accessMode;
        }

        public bool CanRead()
        {
            if ((this.accessMode & QueueAccessMode.Receive) == ((QueueAccessMode) 0))
            {
                return ((this.accessMode & QueueAccessMode.Peek) != ((QueueAccessMode) 0));
            }
            return true;
        }

        public bool CanWrite()
        {
            return ((this.accessMode & QueueAccessMode.Send) != ((QueueAccessMode) 0));
        }

        public static QueueAccessModeHolder GetQueueAccessModeHolder(QueueAccessMode accessMode)
        {
            if (holders.ContainsKey(accessMode))
            {
                return holders[accessMode];
            }
            lock (holders)
            {
                QueueAccessModeHolder holder = new QueueAccessModeHolder(accessMode);
                holders[accessMode] = holder;
                return holder;
            }
        }

        public int GetReadAccessMode()
        {
            int num = ((int) this.accessMode) & -3;
            if (num == 0)
            {
                throw new MessageQueueException(-1072824283);
            }
            return num;
        }

        public int GetWriteAccessMode()
        {
            int num = ((int) this.accessMode) & 2;
            if (num == 0)
            {
                throw new MessageQueueException(-1072824283);
            }
            return num;
        }
    }
}

