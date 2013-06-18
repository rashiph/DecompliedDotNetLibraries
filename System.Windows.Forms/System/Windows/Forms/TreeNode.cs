namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, TypeConverter(typeof(TreeNodeConverter)), DefaultProperty("Text")]
    public class TreeNode : MarshalByRefObject, ICloneable, ISerializable
    {
        private const int ALLOWEDIMAGES = 14;
        private const int CHECKED = 0x2000;
        internal int childCount;
        internal TreeNode[] children;
        private bool collapseOnRealization;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private bool expandOnRealization;
        internal IntPtr handle;
        private TreeNodeImageIndexer imageIndexer;
        internal int index;
        private static readonly int insertMask = 0x23;
        internal const int MAX_TREENODES_OPS = 200;
        internal string name;
        private TreeNodeCollection nodes;
        internal bool nodesCleared;
        internal TreeNode parent;
        internal OwnerDrawPropertyBag propBag;
        private TreeNodeImageIndexer selectedImageIndexer;
        private const int SHIFTVAL = 12;
        private TreeNodeImageIndexer stateImageIndexer;
        internal string text;
        private string toolTipText;
        private BitVector32 treeNodeState;
        private const int TREENODESTATE_isChecked = 1;
        internal System.Windows.Forms.TreeView treeView;
        private const int UNCHECKED = 0x1000;
        private object userData;

        public TreeNode()
        {
            this.toolTipText = "";
            this.treeNodeState = new BitVector32();
        }

        public TreeNode(string text) : this()
        {
            this.text = text;
        }

        internal TreeNode(System.Windows.Forms.TreeView treeView) : this()
        {
            this.treeView = treeView;
        }

        protected TreeNode(SerializationInfo serializationInfo, StreamingContext context) : this()
        {
            this.Deserialize(serializationInfo, context);
        }

        public TreeNode(string text, TreeNode[] children) : this()
        {
            this.text = text;
            this.Nodes.AddRange(children);
        }

        public TreeNode(string text, int imageIndex, int selectedImageIndex) : this()
        {
            this.text = text;
            this.ImageIndexer.Index = imageIndex;
            this.SelectedImageIndexer.Index = selectedImageIndex;
        }

        public TreeNode(string text, int imageIndex, int selectedImageIndex, TreeNode[] children) : this()
        {
            this.text = text;
            this.ImageIndexer.Index = imageIndex;
            this.SelectedImageIndexer.Index = selectedImageIndex;
            this.Nodes.AddRange(children);
        }

        internal int AddSorted(TreeNode node)
        {
            int index = 0;
            string text = node.Text;
            System.Windows.Forms.TreeView treeView = this.TreeView;
            if (this.childCount > 0)
            {
                int num2;
                int childCount;
                int num4;
                if (treeView.TreeViewNodeSorter == null)
                {
                    CompareInfo compareInfo = Application.CurrentCulture.CompareInfo;
                    if (compareInfo.Compare(this.children[this.childCount - 1].Text, text) <= 0)
                    {
                        index = this.childCount;
                    }
                    else
                    {
                        num2 = 0;
                        childCount = this.childCount;
                        while (num2 < childCount)
                        {
                            num4 = (num2 + childCount) / 2;
                            if (compareInfo.Compare(this.children[num4].Text, text) <= 0)
                            {
                                num2 = num4 + 1;
                            }
                            else
                            {
                                childCount = num4;
                            }
                        }
                        index = num2;
                    }
                }
                else
                {
                    IComparer treeViewNodeSorter = treeView.TreeViewNodeSorter;
                    num2 = 0;
                    childCount = this.childCount;
                    while (num2 < childCount)
                    {
                        num4 = (num2 + childCount) / 2;
                        if (treeViewNodeSorter.Compare(this.children[num4], node) <= 0)
                        {
                            num2 = num4 + 1;
                        }
                        else
                        {
                            childCount = num4;
                        }
                    }
                    index = num2;
                }
            }
            node.SortChildren(treeView);
            this.InsertNodeAt(index, node);
            return index;
        }

        public void BeginEdit()
        {
            if (this.handle != IntPtr.Zero)
            {
                System.Windows.Forms.TreeView treeView = this.TreeView;
                if (!treeView.LabelEdit)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("TreeNodeBeginEditFailed"));
                }
                if (!treeView.Focused)
                {
                    treeView.FocusInternal();
                }
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), System.Windows.Forms.NativeMethods.TVM_EDITLABEL, 0, this.handle);
            }
        }

        internal void Clear()
        {
            bool flag = false;
            System.Windows.Forms.TreeView treeView = this.TreeView;
            try
            {
                if (treeView != null)
                {
                    treeView.nodesCollectionClear = true;
                    if ((treeView != null) && (this.childCount > 200))
                    {
                        flag = true;
                        treeView.BeginUpdate();
                    }
                }
                while (this.childCount > 0)
                {
                    this.children[this.childCount - 1].Remove(true);
                }
                this.children = null;
                if ((treeView != null) && flag)
                {
                    treeView.EndUpdate();
                }
            }
            finally
            {
                if (treeView != null)
                {
                    treeView.nodesCollectionClear = false;
                }
                this.nodesCleared = true;
            }
        }

        public virtual object Clone()
        {
            System.Type type = base.GetType();
            TreeNode node = null;
            if (type == typeof(TreeNode))
            {
                node = new TreeNode(this.text, this.ImageIndexer.Index, this.SelectedImageIndexer.Index);
            }
            else
            {
                node = (TreeNode) Activator.CreateInstance(type);
            }
            node.Text = this.text;
            node.Name = this.name;
            node.ImageIndexer.Index = this.ImageIndexer.Index;
            node.SelectedImageIndexer.Index = this.SelectedImageIndexer.Index;
            node.StateImageIndexer.Index = this.StateImageIndexer.Index;
            node.ToolTipText = this.toolTipText;
            node.ContextMenu = this.contextMenu;
            node.ContextMenuStrip = this.contextMenuStrip;
            if (!string.IsNullOrEmpty(this.ImageIndexer.Key))
            {
                node.ImageIndexer.Key = this.ImageIndexer.Key;
            }
            if (!string.IsNullOrEmpty(this.SelectedImageIndexer.Key))
            {
                node.SelectedImageIndexer.Key = this.SelectedImageIndexer.Key;
            }
            if (!string.IsNullOrEmpty(this.StateImageIndexer.Key))
            {
                node.StateImageIndexer.Key = this.StateImageIndexer.Key;
            }
            if (this.childCount > 0)
            {
                node.children = new TreeNode[this.childCount];
                for (int i = 0; i < this.childCount; i++)
                {
                    node.Nodes.Add((TreeNode) this.children[i].Clone());
                }
            }
            if (this.propBag != null)
            {
                node.propBag = OwnerDrawPropertyBag.Copy(this.propBag);
            }
            node.Checked = this.Checked;
            node.Tag = this.Tag;
            return node;
        }

        public void Collapse()
        {
            this.CollapseInternal(false);
        }

        public void Collapse(bool ignoreChildren)
        {
            this.CollapseInternal(ignoreChildren);
        }

        private void CollapseInternal(bool ignoreChildren)
        {
            System.Windows.Forms.TreeView treeView = this.TreeView;
            bool flag = false;
            this.collapseOnRealization = false;
            this.expandOnRealization = false;
            if ((treeView == null) || !treeView.IsHandleCreated)
            {
                this.collapseOnRealization = true;
            }
            else
            {
                if (ignoreChildren)
                {
                    this.DoCollapse(treeView);
                }
                else
                {
                    if (!ignoreChildren && (this.childCount > 0))
                    {
                        for (int i = 0; i < this.childCount; i++)
                        {
                            if (treeView.SelectedNode == this.children[i])
                            {
                                flag = true;
                            }
                            this.children[i].DoCollapse(treeView);
                            this.children[i].Collapse();
                        }
                    }
                    this.DoCollapse(treeView);
                }
                if (flag)
                {
                    treeView.SelectedNode = this;
                }
                treeView.Invalidate();
                this.collapseOnRealization = false;
            }
        }

        protected virtual void Deserialize(SerializationInfo serializationInfo, StreamingContext context)
        {
            int num = 0;
            int num2 = -1;
            string str = null;
            int num3 = -1;
            string str2 = null;
            int num4 = -1;
            string str3 = null;
            SerializationInfoEnumerator enumerator = serializationInfo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SerializationEntry current = enumerator.Current;
                switch (current.Name)
                {
                    case "PropBag":
                        this.propBag = (OwnerDrawPropertyBag) serializationInfo.GetValue(current.Name, typeof(OwnerDrawPropertyBag));
                        break;

                    case "Text":
                        this.Text = serializationInfo.GetString(current.Name);
                        break;

                    case "Name":
                        this.Name = serializationInfo.GetString(current.Name);
                        break;

                    case "IsChecked":
                        this.CheckedStateInternal = serializationInfo.GetBoolean(current.Name);
                        break;

                    case "ImageIndex":
                        num2 = serializationInfo.GetInt32(current.Name);
                        break;

                    case "SelectedImageIndex":
                        num3 = serializationInfo.GetInt32(current.Name);
                        break;

                    case "ImageKey":
                        str = serializationInfo.GetString(current.Name);
                        break;

                    case "SelectedImageKey":
                        str2 = serializationInfo.GetString(current.Name);
                        break;

                    case "StateImageKey":
                        str3 = serializationInfo.GetString(current.Name);
                        break;

                    case "StateImageIndex":
                        num4 = serializationInfo.GetInt32(current.Name);
                        break;

                    case "ChildCount":
                        num = serializationInfo.GetInt32(current.Name);
                        break;

                    case "UserData":
                        this.userData = current.Value;
                        break;
                }
            }
            if (str != null)
            {
                this.ImageKey = str;
            }
            else if (num2 != -1)
            {
                this.ImageIndex = num2;
            }
            if (str2 != null)
            {
                this.SelectedImageKey = str2;
            }
            else if (num3 != -1)
            {
                this.SelectedImageIndex = num3;
            }
            if (str3 != null)
            {
                this.StateImageKey = str3;
            }
            else if (num4 != -1)
            {
                this.StateImageIndex = num4;
            }
            if (num > 0)
            {
                TreeNode[] nodes = new TreeNode[num];
                for (int i = 0; i < num; i++)
                {
                    nodes[i] = (TreeNode) serializationInfo.GetValue("children" + i, typeof(TreeNode));
                }
                this.Nodes.AddRange(nodes);
            }
        }

        private void DoCollapse(System.Windows.Forms.TreeView tv)
        {
            if ((this.State & 0x20) != 0)
            {
                TreeViewCancelEventArgs e = new TreeViewCancelEventArgs(this, false, TreeViewAction.Collapse);
                tv.OnBeforeCollapse(e);
                if (!e.Cancel)
                {
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(tv, tv.Handle), 0x1102, 1, this.Handle);
                    tv.OnAfterCollapse(new TreeViewEventArgs(this));
                }
            }
        }

        public void EndEdit(bool cancel)
        {
            if (this.TreeView != null)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), 0x1116, cancel ? 1 : 0, 0);
            }
        }

        internal void EnsureCapacity(int num)
        {
            int num2 = num;
            if (num2 < 4)
            {
                num2 = 4;
            }
            if (this.children == null)
            {
                this.children = new TreeNode[num2];
            }
            else if ((this.childCount + num) > this.children.Length)
            {
                int num3 = this.childCount + num;
                if (num == 1)
                {
                    num3 = this.childCount * 2;
                }
                TreeNode[] destinationArray = new TreeNode[num3];
                Array.Copy(this.children, 0, destinationArray, 0, this.childCount);
                this.children = destinationArray;
            }
        }

        private void EnsureStateImageValue()
        {
            if ((this.treeView != null) && (this.treeView.CheckBoxes && (this.treeView.StateImageList != null)))
            {
                if (!string.IsNullOrEmpty(this.StateImageKey))
                {
                    this.StateImageIndex = this.Checked ? 1 : 0;
                    this.StateImageKey = this.treeView.StateImageList.Images.Keys[this.StateImageIndex];
                }
                else
                {
                    this.StateImageIndex = this.Checked ? 1 : 0;
                }
            }
        }

        public void EnsureVisible()
        {
            if (this.TreeView != null)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), 0x1114, 0, this.Handle);
            }
        }

        public void Expand()
        {
            System.Windows.Forms.TreeView treeView = this.TreeView;
            if ((treeView == null) || !treeView.IsHandleCreated)
            {
                this.expandOnRealization = true;
            }
            else
            {
                this.ResetExpandedState(treeView);
                if (!this.IsExpanded)
                {
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 0x1102, 2, this.Handle);
                }
                this.expandOnRealization = false;
            }
        }

        public void ExpandAll()
        {
            this.Expand();
            for (int i = 0; i < this.childCount; i++)
            {
                this.children[i].ExpandAll();
            }
        }

        internal System.Windows.Forms.TreeView FindTreeView()
        {
            TreeNode parent = this;
            while (parent.parent != null)
            {
                parent = parent.parent;
            }
            return parent.treeView;
        }

        public static TreeNode FromHandle(System.Windows.Forms.TreeView tree, IntPtr handle)
        {
            System.Windows.Forms.IntSecurity.ControlFromHandleOrLocation.Demand();
            return tree.NodeFromHandle(handle);
        }

        private void GetFullPath(StringBuilder path, string pathSeparator)
        {
            if (this.parent != null)
            {
                this.parent.GetFullPath(path, pathSeparator);
                if (this.parent.parent != null)
                {
                    path.Append(pathSeparator);
                }
                path.Append(this.text);
            }
        }

        public int GetNodeCount(bool includeSubTrees)
        {
            int childCount = this.childCount;
            if (includeSubTrees)
            {
                for (int i = 0; i < this.childCount; i++)
                {
                    childCount += this.children[i].GetNodeCount(true);
                }
            }
            return childCount;
        }

        internal void InsertNodeAt(int index, TreeNode node)
        {
            this.EnsureCapacity(1);
            node.parent = this;
            node.index = index;
            for (int i = this.childCount; i > index; i--)
            {
                TreeNode node2;
                this.children[i] = node2 = this.children[i - 1];
                node2.index = i;
            }
            this.children[index] = node;
            this.childCount++;
            node.Realize(false);
            if ((this.TreeView != null) && (node == this.TreeView.selectedNode))
            {
                this.TreeView.SelectedNode = node;
            }
        }

        private void InvalidateHostTree()
        {
            if ((this.treeView != null) && this.treeView.IsHandleCreated)
            {
                this.treeView.Invalidate();
            }
        }

        internal void Realize(bool insertFirst)
        {
            System.Windows.Forms.TreeView treeView = this.TreeView;
            if ((treeView != null) && treeView.IsHandleCreated)
            {
                if (this.parent != null)
                {
                    if (treeView.InvokeRequired)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("InvalidCrossThreadControlCall"));
                    }
                    System.Windows.Forms.NativeMethods.TV_INSERTSTRUCT lParam = new System.Windows.Forms.NativeMethods.TV_INSERTSTRUCT {
                        item_mask = insertMask,
                        hParent = this.parent.handle
                    };
                    TreeNode prevNode = this.PrevNode;
                    if (insertFirst || (prevNode == null))
                    {
                        lParam.hInsertAfter = (IntPtr) (-65535);
                    }
                    else
                    {
                        lParam.hInsertAfter = prevNode.handle;
                    }
                    lParam.item_pszText = Marshal.StringToHGlobalAuto(this.text);
                    lParam.item_iImage = (this.ImageIndexer.ActualIndex == -1) ? treeView.ImageIndexer.ActualIndex : this.ImageIndexer.ActualIndex;
                    lParam.item_iSelectedImage = (this.SelectedImageIndexer.ActualIndex == -1) ? treeView.SelectedImageIndexer.ActualIndex : this.SelectedImageIndexer.ActualIndex;
                    lParam.item_mask = 1;
                    lParam.item_stateMask = 0;
                    lParam.item_state = 0;
                    if (treeView.CheckBoxes)
                    {
                        lParam.item_mask |= 8;
                        lParam.item_stateMask |= 0xf000;
                        lParam.item_state |= this.CheckedInternal ? 0x2000 : 0x1000;
                    }
                    else if ((treeView.StateImageList != null) && (this.StateImageIndexer.ActualIndex >= 0))
                    {
                        lParam.item_mask |= 8;
                        lParam.item_stateMask = 0xf000;
                        lParam.item_state = (this.StateImageIndexer.ActualIndex + 1) << 12;
                    }
                    if (lParam.item_iImage >= 0)
                    {
                        lParam.item_mask |= 2;
                    }
                    if (lParam.item_iSelectedImage >= 0)
                    {
                        lParam.item_mask |= 0x20;
                    }
                    bool flag = false;
                    if (System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), 0x110f, 0, 0) != IntPtr.Zero)
                    {
                        flag = true;
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), 0x1116, 0, 0);
                    }
                    this.handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), System.Windows.Forms.NativeMethods.TVM_INSERTITEM, 0, ref lParam);
                    treeView.nodeTable[this.handle] = this;
                    this.UpdateNode(4);
                    Marshal.FreeHGlobal(lParam.item_pszText);
                    if (flag)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this.TreeView, this.TreeView.Handle), System.Windows.Forms.NativeMethods.TVM_EDITLABEL, IntPtr.Zero, this.handle);
                    }
                    System.Windows.Forms.SafeNativeMethods.InvalidateRect(new HandleRef(treeView, treeView.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, false);
                    if ((this.parent.nodesCleared && (insertFirst || (prevNode == null))) && !treeView.Scrollable)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), 11, 1, 0);
                        this.nodesCleared = false;
                    }
                }
                for (int i = this.childCount - 1; i >= 0; i--)
                {
                    this.children[i].Realize(true);
                }
                if (this.expandOnRealization)
                {
                    this.Expand();
                }
                if (this.collapseOnRealization)
                {
                    this.Collapse();
                }
            }
        }

        public void Remove()
        {
            this.Remove(true);
        }

        internal void Remove(bool notify)
        {
            bool isExpanded = this.IsExpanded;
            for (int i = 0; i < this.childCount; i++)
            {
                this.children[i].Remove(false);
            }
            if (notify && (this.parent != null))
            {
                for (int j = this.index; j < (this.parent.childCount - 1); j++)
                {
                    TreeNode node;
                    this.parent.children[j] = node = this.parent.children[j + 1];
                    node.index = j;
                }
                this.parent.children[this.parent.childCount - 1] = null;
                this.parent.childCount--;
                this.parent = null;
            }
            this.expandOnRealization = isExpanded;
            if (this.TreeView != null)
            {
                if (this.handle != IntPtr.Zero)
                {
                    if (notify && this.TreeView.IsHandleCreated)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), 0x1101, 0, this.handle);
                    }
                    this.treeView.nodeTable.Remove(this.handle);
                    this.handle = IntPtr.Zero;
                }
                this.treeView = null;
            }
        }

        private void RemovePropBagIfEmpty()
        {
            if ((this.propBag != null) && this.propBag.IsEmpty())
            {
                this.propBag = null;
            }
        }

        private void ResetExpandedState(System.Windows.Forms.TreeView tv)
        {
            System.Windows.Forms.NativeMethods.TV_ITEM lParam = new System.Windows.Forms.NativeMethods.TV_ITEM {
                mask = 0x18,
                hItem = this.handle,
                stateMask = 0x40,
                state = 0
            };
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(tv, tv.Handle), System.Windows.Forms.NativeMethods.TVM_SETITEM, 0, ref lParam);
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.SerializationFormatter), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        protected virtual void Serialize(SerializationInfo si, StreamingContext context)
        {
            if (this.propBag != null)
            {
                si.AddValue("PropBag", this.propBag, typeof(OwnerDrawPropertyBag));
            }
            si.AddValue("Text", this.text);
            si.AddValue("Name", this.Name);
            si.AddValue("IsChecked", this.treeNodeState[1]);
            si.AddValue("ImageIndex", this.ImageIndexer.Index);
            si.AddValue("ImageKey", this.ImageIndexer.Key);
            si.AddValue("SelectedImageIndex", this.SelectedImageIndexer.Index);
            si.AddValue("SelectedImageKey", this.SelectedImageIndexer.Key);
            if ((this.treeView != null) && (this.treeView.StateImageList != null))
            {
                si.AddValue("StateImageIndex", this.StateImageIndexer.Index);
            }
            if ((this.treeView != null) && (this.treeView.StateImageList != null))
            {
                si.AddValue("StateImageKey", this.StateImageIndexer.Key);
            }
            si.AddValue("ChildCount", this.childCount);
            if (this.childCount > 0)
            {
                for (int i = 0; i < this.childCount; i++)
                {
                    si.AddValue("children" + i, this.children[i], typeof(TreeNode));
                }
            }
            if ((this.userData != null) && this.userData.GetType().IsSerializable)
            {
                si.AddValue("UserData", this.userData, this.userData.GetType());
            }
        }

        private bool ShouldSerializeBackColor()
        {
            return (this.BackColor != Color.Empty);
        }

        private bool ShouldSerializeForeColor()
        {
            return (this.ForeColor != Color.Empty);
        }

        private void SortChildren(System.Windows.Forms.TreeView parentTreeView)
        {
            if (this.childCount > 0)
            {
                TreeNode[] nodeArray = new TreeNode[this.childCount];
                if ((parentTreeView == null) || (parentTreeView.TreeViewNodeSorter == null))
                {
                    CompareInfo compareInfo = Application.CurrentCulture.CompareInfo;
                    for (int i = 0; i < this.childCount; i++)
                    {
                        int index = -1;
                        for (int j = 0; j < this.childCount; j++)
                        {
                            if (this.children[j] != null)
                            {
                                if (index == -1)
                                {
                                    index = j;
                                }
                                else if (compareInfo.Compare(this.children[j].Text, this.children[index].Text) <= 0)
                                {
                                    index = j;
                                }
                            }
                        }
                        nodeArray[i] = this.children[index];
                        this.children[index] = null;
                        nodeArray[i].index = i;
                        nodeArray[i].SortChildren(parentTreeView);
                    }
                    this.children = nodeArray;
                }
                else
                {
                    IComparer treeViewNodeSorter = parentTreeView.TreeViewNodeSorter;
                    for (int k = 0; k < this.childCount; k++)
                    {
                        int num5 = -1;
                        for (int m = 0; m < this.childCount; m++)
                        {
                            if (this.children[m] != null)
                            {
                                if (num5 == -1)
                                {
                                    num5 = m;
                                }
                                else if (treeViewNodeSorter.Compare(this.children[m], this.children[num5]) <= 0)
                                {
                                    num5 = m;
                                }
                            }
                        }
                        nodeArray[k] = this.children[num5];
                        this.children[num5] = null;
                        nodeArray[k].index = k;
                        nodeArray[k].SortChildren(parentTreeView);
                    }
                    this.children = nodeArray;
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            this.Serialize(si, context);
        }

        public void Toggle()
        {
            if (this.IsExpanded)
            {
                this.Collapse();
            }
            else
            {
                this.Expand();
            }
        }

        public override string ToString()
        {
            return ("TreeNode: " + ((this.text == null) ? "" : this.text));
        }

        internal void UpdateImage()
        {
            System.Windows.Forms.NativeMethods.TV_ITEM lParam = new System.Windows.Forms.NativeMethods.TV_ITEM {
                mask = 0x12,
                hItem = this.Handle,
                iImage = Math.Max(0, (this.ImageIndexer.ActualIndex >= this.TreeView.ImageList.Images.Count) ? (this.TreeView.ImageList.Images.Count - 1) : this.ImageIndexer.ActualIndex)
            };
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), System.Windows.Forms.NativeMethods.TVM_SETITEM, 0, ref lParam);
        }

        private void UpdateNode(int mask)
        {
            if (this.handle != IntPtr.Zero)
            {
                System.Windows.Forms.TreeView treeView = this.TreeView;
                System.Windows.Forms.NativeMethods.TV_ITEM lParam = new System.Windows.Forms.NativeMethods.TV_ITEM {
                    mask = 0x10 | mask,
                    hItem = this.handle
                };
                if ((mask & 1) != 0)
                {
                    lParam.pszText = Marshal.StringToHGlobalAuto(this.text);
                }
                if ((mask & 2) != 0)
                {
                    lParam.iImage = (this.ImageIndexer.ActualIndex == -1) ? treeView.ImageIndexer.ActualIndex : this.ImageIndexer.ActualIndex;
                }
                if ((mask & 0x20) != 0)
                {
                    lParam.iSelectedImage = (this.SelectedImageIndexer.ActualIndex == -1) ? treeView.SelectedImageIndexer.ActualIndex : this.SelectedImageIndexer.ActualIndex;
                }
                if ((mask & 8) != 0)
                {
                    lParam.stateMask = 0xf000;
                    if (this.StateImageIndexer.ActualIndex != -1)
                    {
                        lParam.state = (this.StateImageIndexer.ActualIndex + 1) << 12;
                    }
                }
                if ((mask & 4) != 0)
                {
                    lParam.lParam = this.handle;
                }
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), System.Windows.Forms.NativeMethods.TVM_SETITEM, 0, ref lParam);
                if ((mask & 1) != 0)
                {
                    Marshal.FreeHGlobal(lParam.pszText);
                    if (treeView.Scrollable)
                    {
                        treeView.ForceScrollbarUpdate(false);
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TreeNodeBackColorDescr")]
        public Color BackColor
        {
            get
            {
                if (this.propBag == null)
                {
                    return Color.Empty;
                }
                return this.propBag.BackColor;
            }
            set
            {
                Color backColor = this.BackColor;
                if (value.IsEmpty)
                {
                    if (this.propBag != null)
                    {
                        this.propBag.BackColor = Color.Empty;
                        this.RemovePropBagIfEmpty();
                    }
                    if (!backColor.IsEmpty)
                    {
                        this.InvalidateHostTree();
                    }
                }
                else
                {
                    if (this.propBag == null)
                    {
                        this.propBag = new OwnerDrawPropertyBag();
                    }
                    this.propBag.BackColor = value;
                    if (!value.Equals(backColor))
                    {
                        this.InvalidateHostTree();
                    }
                }
            }
        }

        [Browsable(false)]
        public Rectangle Bounds
        {
            get
            {
                if (this.TreeView == null)
                {
                    return Rectangle.Empty;
                }
                System.Windows.Forms.NativeMethods.RECT lParam = new System.Windows.Forms.NativeMethods.RECT();
                (IntPtr) &lParam.left = this.Handle;
                if (((int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), 0x1104, 1, ref lParam)) == 0)
                {
                    return Rectangle.Empty;
                }
                return Rectangle.FromLTRB(lParam.left, lParam.top, lParam.right, lParam.bottom);
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("TreeNodeCheckedDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool Checked
        {
            get
            {
                return this.CheckedInternal;
            }
            set
            {
                System.Windows.Forms.TreeView treeView = this.TreeView;
                if (treeView != null)
                {
                    if (!treeView.TreeViewBeforeCheck(this, TreeViewAction.Unknown))
                    {
                        this.CheckedInternal = value;
                        treeView.TreeViewAfterCheck(this, TreeViewAction.Unknown);
                    }
                }
                else
                {
                    this.CheckedInternal = value;
                }
            }
        }

        internal bool CheckedInternal
        {
            get
            {
                return this.CheckedStateInternal;
            }
            set
            {
                this.CheckedStateInternal = value;
                if (this.handle != IntPtr.Zero)
                {
                    System.Windows.Forms.TreeView treeView = this.TreeView;
                    if ((treeView != null) && treeView.IsHandleCreated)
                    {
                        System.Windows.Forms.NativeMethods.TV_ITEM tv_item;
                        tv_item = new System.Windows.Forms.NativeMethods.TV_ITEM {
                            mask = 0x18,
                            hItem = this.handle,
                            stateMask = 0xf000,
                            state = tv_item.state | (value ? 0x2000 : 0x1000)
                        };
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), System.Windows.Forms.NativeMethods.TVM_SETITEM, 0, ref tv_item);
                    }
                }
            }
        }

        internal bool CheckedStateInternal
        {
            get
            {
                return this.treeNodeState[1];
            }
            set
            {
                this.treeNodeState[1] = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue((string) null), System.Windows.Forms.SRDescription("ControlContextMenuDescr")]
        public virtual System.Windows.Forms.ContextMenu ContextMenu
        {
            get
            {
                return this.contextMenu;
            }
            set
            {
                this.contextMenu = value;
            }
        }

        [System.Windows.Forms.SRDescription("ControlContextMenuDescr"), DefaultValue((string) null), System.Windows.Forms.SRCategory("CatBehavior")]
        public virtual System.Windows.Forms.ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return this.contextMenuStrip;
            }
            set
            {
                this.contextMenuStrip = value;
            }
        }

        [Browsable(false)]
        public TreeNode FirstNode
        {
            get
            {
                if (this.childCount == 0)
                {
                    return null;
                }
                return this.children[0];
            }
        }

        private TreeNode FirstVisibleParent
        {
            get
            {
                TreeNode parent = this;
                while ((parent != null) && parent.Bounds.IsEmpty)
                {
                    parent = parent.Parent;
                }
                return parent;
            }
        }

        [System.Windows.Forms.SRDescription("TreeNodeForeColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Color ForeColor
        {
            get
            {
                if (this.propBag == null)
                {
                    return Color.Empty;
                }
                return this.propBag.ForeColor;
            }
            set
            {
                Color foreColor = this.ForeColor;
                if (value.IsEmpty)
                {
                    if (this.propBag != null)
                    {
                        this.propBag.ForeColor = Color.Empty;
                        this.RemovePropBagIfEmpty();
                    }
                    if (!foreColor.IsEmpty)
                    {
                        this.InvalidateHostTree();
                    }
                }
                else
                {
                    if (this.propBag == null)
                    {
                        this.propBag = new OwnerDrawPropertyBag();
                    }
                    this.propBag.ForeColor = value;
                    if (!value.Equals(foreColor))
                    {
                        this.InvalidateHostTree();
                    }
                }
            }
        }

        [Browsable(false)]
        public string FullPath
        {
            get
            {
                System.Windows.Forms.TreeView treeView = this.TreeView;
                if (treeView == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("TreeNodeNoParent"));
                }
                StringBuilder path = new StringBuilder();
                this.GetFullPath(path, treeView.PathSeparator);
                return path.ToString();
            }
        }

        [Browsable(false)]
        public IntPtr Handle
        {
            get
            {
                if (this.handle == IntPtr.Zero)
                {
                    this.TreeView.CreateControl();
                }
                return this.handle;
            }
        }

        [DefaultValue(-1), RelatedImageList("TreeView.ImageList"), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeNodeImageIndexDescr"), TypeConverter(typeof(TreeViewImageIndexConverter)), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), RefreshProperties(RefreshProperties.Repaint), Localizable(true)]
        public int ImageIndex
        {
            get
            {
                return this.ImageIndexer.Index;
            }
            set
            {
                this.ImageIndexer.Index = value;
                this.UpdateNode(2);
            }
        }

        internal TreeNodeImageIndexer ImageIndexer
        {
            get
            {
                if (this.imageIndexer == null)
                {
                    this.imageIndexer = new TreeNodeImageIndexer(this, TreeNodeImageIndexer.ImageListType.Default);
                }
                return this.imageIndexer;
            }
        }

        [RelatedImageList("TreeView.ImageList"), DefaultValue(""), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), RefreshProperties(RefreshProperties.Repaint), TypeConverter(typeof(TreeViewImageKeyConverter)), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeNodeImageKeyDescr")]
        public string ImageKey
        {
            get
            {
                return this.ImageIndexer.Key;
            }
            set
            {
                this.ImageIndexer.Key = value;
                this.UpdateNode(2);
            }
        }

        [System.Windows.Forms.SRDescription("TreeNodeIndexDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public int Index
        {
            get
            {
                return this.index;
            }
        }

        [Browsable(false)]
        public bool IsEditing
        {
            get
            {
                System.Windows.Forms.TreeView treeView = this.TreeView;
                return ((treeView != null) && (treeView.editNode == this));
            }
        }

        [Browsable(false)]
        public bool IsExpanded
        {
            get
            {
                if (this.handle == IntPtr.Zero)
                {
                    return this.expandOnRealization;
                }
                return ((this.State & 0x20) != 0);
            }
        }

        [Browsable(false)]
        public bool IsSelected
        {
            get
            {
                if (this.handle == IntPtr.Zero)
                {
                    return false;
                }
                return ((this.State & 2) != 0);
            }
        }

        [Browsable(false)]
        public bool IsVisible
        {
            get
            {
                if (this.handle == IntPtr.Zero)
                {
                    return false;
                }
                System.Windows.Forms.TreeView treeView = this.TreeView;
                System.Windows.Forms.NativeMethods.RECT lParam = new System.Windows.Forms.NativeMethods.RECT();
                (IntPtr) &lParam.left = this.Handle;
                bool flag = ((int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 0x1104, 1, ref lParam)) != 0;
                if (flag)
                {
                    Size clientSize = treeView.ClientSize;
                    flag = (((lParam.bottom > 0) && (lParam.right > 0)) && (lParam.top < clientSize.Height)) && (lParam.left < clientSize.Width);
                }
                return flag;
            }
        }

        [Browsable(false)]
        public TreeNode LastNode
        {
            get
            {
                if (this.childCount == 0)
                {
                    return null;
                }
                return this.children[this.childCount - 1];
            }
        }

        [Browsable(false)]
        public int Level
        {
            get
            {
                if (this.Parent == null)
                {
                    return 0;
                }
                return (this.Parent.Level + 1);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TreeNodeNodeNameDescr")]
        public string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return "";
            }
            set
            {
                this.name = value;
            }
        }

        [Browsable(false)]
        public TreeNode NextNode
        {
            get
            {
                if ((this.index + 1) < this.parent.Nodes.Count)
                {
                    return this.parent.Nodes[this.index + 1];
                }
                return null;
            }
        }

        [Browsable(false)]
        public TreeNode NextVisibleNode
        {
            get
            {
                if (this.TreeView != null)
                {
                    TreeNode firstVisibleParent = this.FirstVisibleParent;
                    if (firstVisibleParent != null)
                    {
                        IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), 0x110a, 6, firstVisibleParent.Handle);
                        if (handle != IntPtr.Zero)
                        {
                            return this.TreeView.NodeFromHandle(handle);
                        }
                    }
                }
                return null;
            }
        }

        [Localizable(true), DefaultValue((string) null), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TreeNodeNodeFontDescr")]
        public Font NodeFont
        {
            get
            {
                if (this.propBag == null)
                {
                    return null;
                }
                return this.propBag.Font;
            }
            set
            {
                Font nodeFont = this.NodeFont;
                if (value == null)
                {
                    if (this.propBag != null)
                    {
                        this.propBag.Font = null;
                        this.RemovePropBagIfEmpty();
                    }
                    if (nodeFont != null)
                    {
                        this.InvalidateHostTree();
                    }
                }
                else
                {
                    if (this.propBag == null)
                    {
                        this.propBag = new OwnerDrawPropertyBag();
                    }
                    this.propBag.Font = value;
                    if (!value.Equals(nodeFont))
                    {
                        this.InvalidateHostTree();
                    }
                }
            }
        }

        [ListBindable(false), Browsable(false)]
        public TreeNodeCollection Nodes
        {
            get
            {
                if (this.nodes == null)
                {
                    this.nodes = new TreeNodeCollection(this);
                }
                return this.nodes;
            }
        }

        [Browsable(false)]
        public TreeNode Parent
        {
            get
            {
                System.Windows.Forms.TreeView treeView = this.TreeView;
                if ((treeView != null) && (this.parent == treeView.root))
                {
                    return null;
                }
                return this.parent;
            }
        }

        [Browsable(false)]
        public TreeNode PrevNode
        {
            get
            {
                int index = this.index;
                int fixedIndex = this.parent.Nodes.FixedIndex;
                if (fixedIndex > 0)
                {
                    index = fixedIndex;
                }
                if ((index > 0) && (index <= this.parent.Nodes.Count))
                {
                    return this.parent.Nodes[index - 1];
                }
                return null;
            }
        }

        [Browsable(false)]
        public TreeNode PrevVisibleNode
        {
            get
            {
                TreeNode firstVisibleParent = this.FirstVisibleParent;
                if (firstVisibleParent != null)
                {
                    if (this.TreeView == null)
                    {
                        return null;
                    }
                    IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), 0x110a, 7, firstVisibleParent.Handle);
                    if (handle != IntPtr.Zero)
                    {
                        return this.TreeView.NodeFromHandle(handle);
                    }
                }
                return null;
            }
        }

        internal Rectangle RowBounds
        {
            get
            {
                System.Windows.Forms.NativeMethods.RECT lParam = new System.Windows.Forms.NativeMethods.RECT();
                (IntPtr) &lParam.left = this.Handle;
                if (this.TreeView == null)
                {
                    return Rectangle.Empty;
                }
                if (((int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), 0x1104, 0, ref lParam)) == 0)
                {
                    return Rectangle.Empty;
                }
                return Rectangle.FromLTRB(lParam.left, lParam.top, lParam.right, lParam.bottom);
            }
        }

        [System.Windows.Forms.SRDescription("TreeNodeSelectedImageIndexDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), RelatedImageList("TreeView.ImageList"), TypeConverter(typeof(TreeViewImageIndexConverter)), DefaultValue(-1), RefreshProperties(RefreshProperties.Repaint), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public int SelectedImageIndex
        {
            get
            {
                return this.SelectedImageIndexer.Index;
            }
            set
            {
                this.SelectedImageIndexer.Index = value;
                this.UpdateNode(0x20);
            }
        }

        internal TreeNodeImageIndexer SelectedImageIndexer
        {
            get
            {
                if (this.selectedImageIndexer == null)
                {
                    this.selectedImageIndexer = new TreeNodeImageIndexer(this, TreeNodeImageIndexer.ImageListType.Default);
                }
                return this.selectedImageIndexer;
            }
        }

        [RelatedImageList("TreeView.ImageList"), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeNodeSelectedImageKeyDescr"), TypeConverter(typeof(TreeViewImageKeyConverter)), DefaultValue(""), RefreshProperties(RefreshProperties.Repaint), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string SelectedImageKey
        {
            get
            {
                return this.SelectedImageIndexer.Key;
            }
            set
            {
                this.SelectedImageIndexer.Key = value;
                this.UpdateNode(0x20);
            }
        }

        internal int State
        {
            get
            {
                if (this.handle == IntPtr.Zero)
                {
                    return 0;
                }
                if (this.TreeView == null)
                {
                    return 0;
                }
                System.Windows.Forms.NativeMethods.TV_ITEM lParam = new System.Windows.Forms.NativeMethods.TV_ITEM {
                    hItem = this.Handle,
                    mask = 0x18,
                    stateMask = 0x22
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TreeView, this.TreeView.Handle), System.Windows.Forms.NativeMethods.TVM_GETITEM, 0, ref lParam);
                return lParam.state;
            }
        }

        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Localizable(true), TypeConverter(typeof(NoneExcludedImageIndexConverter)), DefaultValue(-1), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeNodeStateImageIndexDescr"), RefreshProperties(RefreshProperties.Repaint), RelatedImageList("TreeView.StateImageList")]
        public int StateImageIndex
        {
            get
            {
                if ((this.treeView != null) && (this.treeView.StateImageList != null))
                {
                    return this.StateImageIndexer.Index;
                }
                return -1;
            }
            set
            {
                if ((value < -1) || (value > 14))
                {
                    throw new ArgumentOutOfRangeException("StateImageIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "StateImageIndex", value.ToString(CultureInfo.CurrentCulture) }));
                }
                this.StateImageIndexer.Index = value;
                if ((this.treeView != null) && !this.treeView.CheckBoxes)
                {
                    this.UpdateNode(8);
                }
            }
        }

        internal TreeNodeImageIndexer StateImageIndexer
        {
            get
            {
                if (this.stateImageIndexer == null)
                {
                    this.stateImageIndexer = new TreeNodeImageIndexer(this, TreeNodeImageIndexer.ImageListType.State);
                }
                return this.stateImageIndexer;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), System.Windows.Forms.SRDescription("TreeNodeStateImageKeyDescr"), TypeConverter(typeof(ImageKeyConverter)), DefaultValue(""), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), RefreshProperties(RefreshProperties.Repaint), RelatedImageList("TreeView.StateImageList")]
        public string StateImageKey
        {
            get
            {
                return this.StateImageIndexer.Key;
            }
            set
            {
                if (this.StateImageIndexer.Key != value)
                {
                    this.StateImageIndexer.Key = value;
                    if ((this.treeView != null) && !this.treeView.CheckBoxes)
                    {
                        this.UpdateNode(8);
                    }
                }
            }
        }

        [Localizable(false), System.Windows.Forms.SRCategory("CatData"), TypeConverter(typeof(StringConverter)), Bindable(true), System.Windows.Forms.SRDescription("ControlTagDescr"), DefaultValue((string) null)]
        public object Tag
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.userData = value;
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("TreeNodeTextDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public string Text
        {
            get
            {
                if (this.text != null)
                {
                    return this.text;
                }
                return "";
            }
            set
            {
                this.text = value;
                this.UpdateNode(1);
            }
        }

        [Localizable(false), DefaultValue(""), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TreeNodeToolTipTextDescr")]
        public string ToolTipText
        {
            get
            {
                return this.toolTipText;
            }
            set
            {
                this.toolTipText = value;
            }
        }

        [Browsable(false)]
        public System.Windows.Forms.TreeView TreeView
        {
            get
            {
                if (this.treeView == null)
                {
                    this.treeView = this.FindTreeView();
                }
                return this.treeView;
            }
        }

        internal class TreeNodeImageIndexer : System.Windows.Forms.ImageList.Indexer
        {
            private ImageListType imageListType;
            private TreeNode owner;

            public TreeNodeImageIndexer(TreeNode node, ImageListType imageListType)
            {
                this.owner = node;
                this.imageListType = imageListType;
            }

            public override System.Windows.Forms.ImageList ImageList
            {
                get
                {
                    if (this.owner.TreeView == null)
                    {
                        return null;
                    }
                    if (this.imageListType == ImageListType.State)
                    {
                        return this.owner.TreeView.StateImageList;
                    }
                    return this.owner.TreeView.ImageList;
                }
                set
                {
                }
            }

            public enum ImageListType
            {
                Default,
                State
            }
        }
    }
}

