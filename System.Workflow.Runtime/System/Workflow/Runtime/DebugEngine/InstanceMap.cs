namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Collections.Generic;

    internal sealed class InstanceMap : Dictionary<Guid, InstanceData>, ICloneable
    {
        object ICloneable.Clone()
        {
            InstanceMap map = new InstanceMap();
            foreach (Guid guid in base.Keys)
            {
                map.Add(guid, (InstanceData) base[guid].Clone());
            }
            return map;
        }
    }
}

