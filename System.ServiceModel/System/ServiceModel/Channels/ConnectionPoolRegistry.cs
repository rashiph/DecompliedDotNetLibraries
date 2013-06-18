namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;

    internal abstract class ConnectionPoolRegistry
    {
        private Dictionary<string, List<ConnectionPool>> registry = new Dictionary<string, List<ConnectionPool>>();

        protected ConnectionPoolRegistry()
        {
        }

        protected abstract ConnectionPool CreatePool(IConnectionOrientedTransportChannelFactorySettings settings);
        public ConnectionPool Lookup(IConnectionOrientedTransportChannelFactorySettings settings)
        {
            ConnectionPool item = null;
            string connectionPoolGroupName = settings.ConnectionPoolGroupName;
            lock (this.ThisLock)
            {
                List<ConnectionPool> list = null;
                if (this.registry.TryGetValue(connectionPoolGroupName, out list))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].IsCompatible(settings) && list[i].TryOpen())
                        {
                            item = list[i];
                            break;
                        }
                    }
                }
                else
                {
                    list = new List<ConnectionPool>();
                    this.registry.Add(connectionPoolGroupName, list);
                }
                if (item == null)
                {
                    item = this.CreatePool(settings);
                    list.Add(item);
                }
            }
            return item;
        }

        public void Release(ConnectionPool pool, TimeSpan timeout)
        {
            lock (this.ThisLock)
            {
                if (pool.Close(timeout))
                {
                    List<ConnectionPool> list = this.registry[pool.Name];
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (object.ReferenceEquals(list[i], pool))
                        {
                            list.RemoveAt(i);
                            break;
                        }
                    }
                    if (list.Count == 0)
                    {
                        this.registry.Remove(pool.Name);
                    }
                }
            }
        }

        private object ThisLock
        {
            get
            {
                return this.registry;
            }
        }
    }
}

