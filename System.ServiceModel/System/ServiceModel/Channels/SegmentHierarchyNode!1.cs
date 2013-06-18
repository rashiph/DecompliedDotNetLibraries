namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class SegmentHierarchyNode<TData> where TData: class
    {
        private Dictionary<string, SegmentHierarchyNode<TData>> children;
        private TData data;
        private string name;
        private BaseUriWithWildcard path;
        private bool useWeakReferences;
        private WeakReference weakData;

        public SegmentHierarchyNode(string name, bool useWeakReferences)
        {
            this.name = name;
            this.useWeakReferences = useWeakReferences;
            this.children = new Dictionary<string, SegmentHierarchyNode<TData>>(StringComparer.OrdinalIgnoreCase);
        }

        public void Collect(List<KeyValuePair<BaseUriWithWildcard, TData>> result)
        {
            TData data = this.Data;
            if (data != null)
            {
                result.Add(new KeyValuePair<BaseUriWithWildcard, TData>(this.path, data));
            }
            foreach (SegmentHierarchyNode<TData> node in this.children.Values)
            {
                node.Collect(result);
            }
        }

        public void RemoveData()
        {
            this.SetData(default(TData), null);
        }

        public bool RemovePath(string[] path, int seg)
        {
            SegmentHierarchyNode<TData> node;
            if (seg == path.Length)
            {
                this.RemoveData();
                return (this.children.Count == 0);
            }
            if (this.TryGetChild(path[seg], out node))
            {
                if (!node.RemovePath(path, seg + 1))
                {
                    return false;
                }
                this.children.Remove(path[seg]);
            }
            return ((this.children.Count == 0) && (this.Data == null));
        }

        public void SetChildNode(string name, SegmentHierarchyNode<TData> node)
        {
            this.children[name] = node;
        }

        public void SetData(TData data, BaseUriWithWildcard path)
        {
            this.path = path;
            if (this.useWeakReferences)
            {
                if (data == null)
                {
                    this.weakData = null;
                }
                else
                {
                    this.weakData = new WeakReference(data);
                }
            }
            else
            {
                this.data = data;
            }
        }

        public bool TryGetChild(string segment, out SegmentHierarchyNode<TData> value)
        {
            return this.children.TryGetValue(segment, out value);
        }

        public TData Data
        {
            get
            {
                if (!this.useWeakReferences)
                {
                    return this.data;
                }
                if (this.weakData == null)
                {
                    return default(TData);
                }
                return (this.weakData.Target as TData);
            }
        }
    }
}

