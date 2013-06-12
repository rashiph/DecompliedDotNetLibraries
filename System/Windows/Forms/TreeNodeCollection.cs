namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;

    [Editor("System.Windows.Forms.Design.TreeNodeCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public class TreeNodeCollection : IList, ICollection, IEnumerable
    {
        private int fixedIndex = -1;
        private int lastAccessedIndex = -1;
        private TreeNode owner;

        internal TreeNodeCollection(TreeNode owner)
        {
            this.owner = owner;
        }

        public virtual TreeNode Add(string text)
        {
            TreeNode node = new TreeNode(text);
            this.Add(node);
            return node;
        }

        public virtual int Add(TreeNode node)
        {
            return this.AddInternal(node, 0);
        }

        public virtual TreeNode Add(string key, string text)
        {
            TreeNode node = new TreeNode(text) {
                Name = key
            };
            this.Add(node);
            return node;
        }

        public virtual TreeNode Add(string key, string text, int imageIndex)
        {
            TreeNode node = new TreeNode(text) {
                Name = key,
                ImageIndex = imageIndex
            };
            this.Add(node);
            return node;
        }

        public virtual TreeNode Add(string key, string text, string imageKey)
        {
            TreeNode node = new TreeNode(text) {
                Name = key,
                ImageKey = imageKey
            };
            this.Add(node);
            return node;
        }

        public virtual TreeNode Add(string key, string text, int imageIndex, int selectedImageIndex)
        {
            TreeNode node = new TreeNode(text, imageIndex, selectedImageIndex) {
                Name = key
            };
            this.Add(node);
            return node;
        }

        public virtual TreeNode Add(string key, string text, string imageKey, string selectedImageKey)
        {
            TreeNode node = new TreeNode(text) {
                Name = key,
                ImageKey = imageKey,
                SelectedImageKey = selectedImageKey
            };
            this.Add(node);
            return node;
        }

        private int AddInternal(TreeNode node, int delta)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (node.handle != IntPtr.Zero)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("OnlyOneControl", new object[] { node.Text }), "node");
            }
            TreeView treeView = this.owner.TreeView;
            if ((treeView != null) && treeView.Sorted)
            {
                return this.owner.AddSorted(node);
            }
            node.parent = this.owner;
            int fixedIndex = this.owner.Nodes.FixedIndex;
            if (fixedIndex != -1)
            {
                node.index = fixedIndex + delta;
            }
            else
            {
                this.owner.EnsureCapacity(1);
                node.index = this.owner.childCount;
            }
            this.owner.children[node.index] = node;
            this.owner.childCount++;
            node.Realize(false);
            if ((treeView != null) && (node == treeView.selectedNode))
            {
                treeView.SelectedNode = node;
            }
            if ((treeView != null) && (treeView.TreeViewNodeSorter != null))
            {
                treeView.Sort();
            }
            return node.index;
        }

        public virtual void AddRange(TreeNode[] nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("nodes");
            }
            if (nodes.Length != 0)
            {
                TreeView treeView = this.owner.TreeView;
                if ((treeView != null) && (nodes.Length > 200))
                {
                    treeView.BeginUpdate();
                }
                this.owner.Nodes.FixedIndex = this.owner.childCount;
                this.owner.EnsureCapacity(nodes.Length);
                for (int i = nodes.Length - 1; i >= 0; i--)
                {
                    this.AddInternal(nodes[i], i);
                }
                this.owner.Nodes.FixedIndex = -1;
                if ((treeView != null) && (nodes.Length > 200))
                {
                    treeView.EndUpdate();
                }
            }
        }

        public virtual void Clear()
        {
            this.owner.Clear();
        }

        public bool Contains(TreeNode node)
        {
            return (this.IndexOf(node) != -1);
        }

        public virtual bool ContainsKey(string key)
        {
            return this.IsValidIndex(this.IndexOfKey(key));
        }

        public void CopyTo(Array dest, int index)
        {
            if (this.owner.childCount > 0)
            {
                Array.Copy(this.owner.children, 0, dest, index, this.owner.childCount);
            }
        }

        public TreeNode[] Find(string key, bool searchAllChildren)
        {
            ArrayList list = this.FindInternal(key, searchAllChildren, this, new ArrayList());
            TreeNode[] array = new TreeNode[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        private ArrayList FindInternal(string key, bool searchAllChildren, TreeNodeCollection treeNodeCollectionToLookIn, ArrayList foundTreeNodes)
        {
            if ((treeNodeCollectionToLookIn == null) || (foundTreeNodes == null))
            {
                return null;
            }
            for (int i = 0; i < treeNodeCollectionToLookIn.Count; i++)
            {
                if ((treeNodeCollectionToLookIn[i] != null) && WindowsFormsUtils.SafeCompareStrings(treeNodeCollectionToLookIn[i].Name, key, true))
                {
                    foundTreeNodes.Add(treeNodeCollectionToLookIn[i]);
                }
            }
            if (searchAllChildren)
            {
                for (int j = 0; j < treeNodeCollectionToLookIn.Count; j++)
                {
                    if (((treeNodeCollectionToLookIn[j] != null) && (treeNodeCollectionToLookIn[j].Nodes != null)) && (treeNodeCollectionToLookIn[j].Nodes.Count > 0))
                    {
                        foundTreeNodes = this.FindInternal(key, searchAllChildren, treeNodeCollectionToLookIn[j].Nodes, foundTreeNodes);
                    }
                }
            }
            return foundTreeNodes;
        }

        public IEnumerator GetEnumerator()
        {
            return new WindowsFormsUtils.ArraySubsetEnumerator(this.owner.children, this.owner.childCount);
        }

        public int IndexOf(TreeNode node)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i] == node)
                {
                    return i;
                }
            }
            return -1;
        }

        public virtual int IndexOfKey(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (this.IsValidIndex(this.lastAccessedIndex) && WindowsFormsUtils.SafeCompareStrings(this[this.lastAccessedIndex].Name, key, true))
                {
                    return this.lastAccessedIndex;
                }
                for (int i = 0; i < this.Count; i++)
                {
                    if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, true))
                    {
                        this.lastAccessedIndex = i;
                        return i;
                    }
                }
                this.lastAccessedIndex = -1;
            }
            return -1;
        }

        public virtual TreeNode Insert(int index, string text)
        {
            TreeNode node = new TreeNode(text);
            this.Insert(index, node);
            return node;
        }

        public virtual void Insert(int index, TreeNode node)
        {
            if (node.handle != IntPtr.Zero)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("OnlyOneControl", new object[] { node.Text }), "node");
            }
            TreeView treeView = this.owner.TreeView;
            if ((treeView != null) && treeView.Sorted)
            {
                this.owner.AddSorted(node);
            }
            else
            {
                if (index < 0)
                {
                    index = 0;
                }
                if (index > this.owner.childCount)
                {
                    index = this.owner.childCount;
                }
                this.owner.InsertNodeAt(index, node);
            }
        }

        public virtual TreeNode Insert(int index, string key, string text)
        {
            TreeNode node = new TreeNode(text) {
                Name = key
            };
            this.Insert(index, node);
            return node;
        }

        public virtual TreeNode Insert(int index, string key, string text, int imageIndex)
        {
            TreeNode node = new TreeNode(text) {
                Name = key,
                ImageIndex = imageIndex
            };
            this.Insert(index, node);
            return node;
        }

        public virtual TreeNode Insert(int index, string key, string text, string imageKey)
        {
            TreeNode node = new TreeNode(text) {
                Name = key,
                ImageKey = imageKey
            };
            this.Insert(index, node);
            return node;
        }

        public virtual TreeNode Insert(int index, string key, string text, int imageIndex, int selectedImageIndex)
        {
            TreeNode node = new TreeNode(text, imageIndex, selectedImageIndex) {
                Name = key
            };
            this.Insert(index, node);
            return node;
        }

        public virtual TreeNode Insert(int index, string key, string text, string imageKey, string selectedImageKey)
        {
            TreeNode node = new TreeNode(text) {
                Name = key,
                ImageKey = imageKey,
                SelectedImageKey = selectedImageKey
            };
            this.Insert(index, node);
            return node;
        }

        private bool IsValidIndex(int index)
        {
            return ((index >= 0) && (index < this.Count));
        }

        public void Remove(TreeNode node)
        {
            node.Remove();
        }

        public virtual void RemoveAt(int index)
        {
            this[index].Remove();
        }

        public virtual void RemoveByKey(string key)
        {
            int index = this.IndexOfKey(key);
            if (this.IsValidIndex(index))
            {
                this.RemoveAt(index);
            }
        }

        int IList.Add(object node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (node is TreeNode)
            {
                return this.Add((TreeNode) node);
            }
            return this.Add(node.ToString()).index;
        }

        bool IList.Contains(object node)
        {
            return ((node is TreeNode) && this.Contains((TreeNode) node));
        }

        int IList.IndexOf(object node)
        {
            if (node is TreeNode)
            {
                return this.IndexOf((TreeNode) node);
            }
            return -1;
        }

        void IList.Insert(int index, object node)
        {
            if (!(node is TreeNode))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("TreeNodeCollectionBadTreeNode"), "node");
            }
            this.Insert(index, (TreeNode) node);
        }

        void IList.Remove(object node)
        {
            if (node is TreeNode)
            {
                this.Remove((TreeNode) node);
            }
        }

        [Browsable(false)]
        public int Count
        {
            get
            {
                return this.owner.childCount;
            }
        }

        internal int FixedIndex
        {
            get
            {
                return this.fixedIndex;
            }
            set
            {
                this.fixedIndex = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual TreeNode this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.owner.childCount))
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return this.owner.children[index];
            }
            set
            {
                if ((index < 0) || (index >= this.owner.childCount))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                value.parent = this.owner;
                value.index = index;
                this.owner.children[index] = value;
                value.Realize(false);
            }
        }

        public virtual TreeNode this[string key]
        {
            get
            {
                if (!string.IsNullOrEmpty(key))
                {
                    int index = this.IndexOfKey(key);
                    if (this.IsValidIndex(index))
                    {
                        return this[index];
                    }
                }
                return null;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (!(value is TreeNode))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("TreeNodeCollectionBadTreeNode"), "value");
                }
                this[index] = (TreeNode) value;
            }
        }
    }
}

