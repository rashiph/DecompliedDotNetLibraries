namespace System.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal abstract class RBTree<K> : IEnumerable
    {
        private readonly TreeAccessMethod _accessMethod;
        private int _inUseNodeCount;
        private int _inUsePageCount;
        private int _inUseSatelliteTreeCount;
        private TreePage<K>[] _pageTable;
        private int[] _pageTableMap;
        private int _version;
        internal const int DefaultPageSize = 0x20;
        private int nextFreePageLine;
        internal const int NIL = 0;
        public int root;

        protected RBTree(TreeAccessMethod accessMethod)
        {
            this._accessMethod = accessMethod;
            this.InitTree();
        }

        public int Add(K item)
        {
            int newNode = this.GetNewNode(item);
            this.RBInsert(0, newNode, 0, -1, false);
            return newNode;
        }

        private TreePage<K> AllocPage(int size)
        {
            int indexOfPageWithFreeSlot = this.GetIndexOfPageWithFreeSlot(false);
            if (indexOfPageWithFreeSlot != -1)
            {
                this._pageTable[indexOfPageWithFreeSlot] = new TreePage<K>(size);
                this.nextFreePageLine = indexOfPageWithFreeSlot / 0x20;
            }
            else
            {
                TreePage<K>[] destinationArray = new TreePage<K>[this._pageTable.Length * 2];
                Array.Copy(this._pageTable, 0, destinationArray, 0, this._pageTable.Length);
                int[] numArray = new int[((destinationArray.Length + 0x20) - 1) / 0x20];
                Array.Copy(this._pageTableMap, 0, numArray, 0, this._pageTableMap.Length);
                this.nextFreePageLine = this._pageTableMap.Length;
                indexOfPageWithFreeSlot = this._pageTable.Length;
                this._pageTable = destinationArray;
                this._pageTableMap = numArray;
                this._pageTable[indexOfPageWithFreeSlot] = new TreePage<K>(size);
            }
            this._pageTable[indexOfPageWithFreeSlot].PageId = indexOfPageWithFreeSlot;
            this._inUsePageCount++;
            return this._pageTable[indexOfPageWithFreeSlot];
        }

        public void Clear()
        {
            this.InitTree();
            this._version++;
        }

        private NodeColor<K> color(int nodeId)
        {
            return this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].nodeColor;
        }

        protected abstract int CompareNode(K record1, K record2);
        protected abstract int CompareSateliteTreeNode(K record1, K record2);
        private int ComputeIndexByNode(int nodeId)
        {
            int num2 = this.SubTreeSize(this.Left(nodeId));
            while (nodeId != 0)
            {
                int num = this.Parent(nodeId);
                if (nodeId == this.Right(num))
                {
                    num2 += this.SubTreeSize(this.Left(num)) + 1;
                }
                nodeId = num;
            }
            return num2;
        }

        private int ComputeIndexWithSatelliteByNode(int nodeId)
        {
            int num2 = this.SubTreeSize(this.Left(nodeId));
            while (nodeId != 0)
            {
                int num = this.Parent(nodeId);
                if (nodeId == this.Right(num))
                {
                    num2 += this.SubTreeSize(this.Left(num)) + ((this.Next(num) == 0) ? 1 : this.SubTreeSize(this.Next(num)));
                }
                nodeId = num;
            }
            return num2;
        }

        private int ComputeNodeByIndex(int index, out int satelliteRootId)
        {
            index++;
            satelliteRootId = 0;
            int root = this.root;
            int num2 = -1;
            while ((root != 0) && (((num2 = this.SubTreeSize(this.Left(root)) + 1) != index) || (this.Next(root) != 0)))
            {
                if (index < num2)
                {
                    root = this.Left(root);
                }
                else
                {
                    if (((this.Next(root) != 0) && (index >= num2)) && (index <= ((num2 + this.SubTreeSize(this.Next(root))) - 1)))
                    {
                        satelliteRootId = root;
                        index = (index - num2) + 1;
                        return this.ComputeNodeByIndex(this.Next(root), index);
                    }
                    if (this.Next(root) == 0)
                    {
                        index -= num2;
                    }
                    else
                    {
                        index -= (num2 + this.SubTreeSize(this.Next(root))) - 1;
                    }
                    root = this.Right(root);
                }
            }
            return root;
        }

        private int ComputeNodeByIndex(int x_id, int index)
        {
            while (x_id != 0)
            {
                int nodeId = this.Left(x_id);
                int num = this.SubTreeSize(nodeId) + 1;
                if (index < num)
                {
                    x_id = nodeId;
                }
                else
                {
                    if (num >= index)
                    {
                        return x_id;
                    }
                    x_id = this.Right(x_id);
                    index -= num;
                }
            }
            return x_id;
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw ExceptionBuilder.ArgumentNull("array");
            }
            if (index < 0)
            {
                throw ExceptionBuilder.ArgumentOutOfRange("index");
            }
            int count = this.Count;
            if ((array.Length - index) < this.Count)
            {
                throw ExceptionBuilder.InvalidOffsetLength();
            }
            int nodeId = this.Minimum(this.root);
            for (int i = 0; i < count; i++)
            {
                array.SetValue(this.Key(nodeId), (int) (index + i));
                nodeId = this.Successor(nodeId);
            }
        }

        public void CopyTo(K[] array, int index)
        {
            if (array == null)
            {
                throw ExceptionBuilder.ArgumentNull("array");
            }
            if (index < 0)
            {
                throw ExceptionBuilder.ArgumentOutOfRange("index");
            }
            int count = this.Count;
            if ((array.Length - index) < this.Count)
            {
                throw ExceptionBuilder.InvalidOffsetLength();
            }
            int nodeId = this.Minimum(this.root);
            for (int i = 0; i < count; i++)
            {
                array[index + i] = this.Key(nodeId);
                nodeId = this.Successor(nodeId);
            }
        }

        private void DecreaseSize(int nodeId)
        {
            this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].subTreeSize--;
        }

        public K DeleteByIndex(int i)
        {
            NodePath<K> nodeByIndex = this.GetNodeByIndex(i);
            K local = this.Key(nodeByIndex.NodeID);
            this.RBDeleteX(0, nodeByIndex.NodeID, nodeByIndex.MainTreeNodeID);
            return local;
        }

        private void FreeNode(int nodeId)
        {
            TreePage<K> page = this._pageTable[nodeId >> 0x10];
            int index = nodeId & 0xffff;
            page.Slots[index] = new Node<K>();
            page.SlotMap[index / 0x20] &= ~(((int) 1) << (index % 0x20));
            page.InUseCount--;
            this._inUseNodeCount--;
            if (page.InUseCount == 0)
            {
                this.FreePage(page);
            }
            else if (page.InUseCount == (page.Slots.Length - 1))
            {
                this.MarkPageFree(page);
            }
        }

        private void FreePage(TreePage<K> page)
        {
            this.MarkPageFree(page);
            this._pageTable[page.PageId] = null;
            this._inUsePageCount--;
        }

        public IEnumerator GetEnumerator()
        {
            return new RBTreeEnumerator<K>((RBTree<K>) this);
        }

        public int GetIndexByKey(K key)
        {
            int indexByNodePath = -1;
            NodePath<K> nodeByKey = this.GetNodeByKey(key);
            if (nodeByKey.NodeID != 0)
            {
                indexByNodePath = this.GetIndexByNodePath(nodeByKey);
            }
            return indexByNodePath;
        }

        public int GetIndexByNode(int node)
        {
            if (this._inUseSatelliteTreeCount == 0)
            {
                return this.ComputeIndexByNode(node);
            }
            if (this.Next(node) != 0)
            {
                return this.ComputeIndexWithSatelliteByNode(node);
            }
            int nodeId = this.SearchSubTree(0, this.Key(node));
            if (nodeId == node)
            {
                return this.ComputeIndexWithSatelliteByNode(node);
            }
            return (this.ComputeIndexWithSatelliteByNode(nodeId) + this.ComputeIndexByNode(node));
        }

        private int GetIndexByNodePath(NodePath<K> path)
        {
            if (this._inUseSatelliteTreeCount == 0)
            {
                return this.ComputeIndexByNode(path.NodeID);
            }
            if (path.MainTreeNodeID == 0)
            {
                return this.ComputeIndexWithSatelliteByNode(path.NodeID);
            }
            return (this.ComputeIndexWithSatelliteByNode(path.MainTreeNodeID) + this.ComputeIndexByNode(path.NodeID));
        }

        private int GetIndexOfPageWithFreeSlot(bool allocatedPage)
        {
            int nextFreePageLine = this.nextFreePageLine;
            int index = -1;
            while (nextFreePageLine < this._pageTableMap.Length)
            {
                if (this._pageTableMap[nextFreePageLine] < -1)
                {
                    uint num4;
                    for (uint i = (uint) this._pageTableMap[nextFreePageLine]; (i ^ uint.MaxValue) != 0; i |= num4)
                    {
                        num4 = ~i & (i + 1);
                        if ((this._pageTableMap[nextFreePageLine] & num4) != 0L)
                        {
                            throw ExceptionBuilder.InternalRBTreeError(RBTreeError.PagePositionInSlotInUse);
                        }
                        index = (nextFreePageLine * 0x20) + RBTree<K>.GetIntValueFromBitMap(num4);
                        if (allocatedPage)
                        {
                            if (this._pageTable[index] != null)
                            {
                                return index;
                            }
                        }
                        else if (this._pageTable[index] == null)
                        {
                            return index;
                        }
                        index = -1;
                    }
                }
                nextFreePageLine++;
            }
            if (this.nextFreePageLine != 0)
            {
                this.nextFreePageLine = 0;
                index = this.GetIndexOfPageWithFreeSlot(allocatedPage);
            }
            return index;
        }

        private static int GetIntValueFromBitMap(uint bitMap)
        {
            int num = 0;
            if ((bitMap & 0xffff0000) != 0)
            {
                num += 0x10;
                bitMap = bitMap >> 0x10;
            }
            if ((bitMap & 0xff00) != 0)
            {
                num += 8;
                bitMap = bitMap >> 8;
            }
            if ((bitMap & 240) != 0)
            {
                num += 4;
                bitMap = bitMap >> 4;
            }
            if ((bitMap & 12) != 0)
            {
                num += 2;
                bitMap = bitMap >> 2;
            }
            if ((bitMap & 2) != 0)
            {
                num++;
            }
            return num;
        }

        private int GetNewNode(K key)
        {
            TreePage<K> page = null;
            int indexOfPageWithFreeSlot = this.GetIndexOfPageWithFreeSlot(true);
            if (indexOfPageWithFreeSlot != -1)
            {
                page = this._pageTable[indexOfPageWithFreeSlot];
            }
            else if (this._inUsePageCount < 4)
            {
                page = this.AllocPage(0x20);
            }
            else if (this._inUsePageCount < 0x20)
            {
                page = this.AllocPage(0x100);
            }
            else if (this._inUsePageCount < 0x80)
            {
                page = this.AllocPage(0x400);
            }
            else if (this._inUsePageCount < 0x1000)
            {
                page = this.AllocPage(0x1000);
            }
            else if (this._inUsePageCount < 0x8000)
            {
                page = this.AllocPage(0x2000);
            }
            else
            {
                page = this.AllocPage(0x10000);
            }
            int index = page.AllocSlot((RBTree<K>) this);
            if (index == -1)
            {
                throw ExceptionBuilder.InternalRBTreeError(RBTreeError.NoFreeSlots);
            }
            page.Slots[index].selfId = (page.PageId << 0x10) | index;
            page.Slots[index].subTreeSize = 1;
            page.Slots[index].keyOfNode = key;
            return page.Slots[index].selfId;
        }

        private NodePath<K> GetNodeByIndex(int userIndex)
        {
            int num;
            int num2;
            if (this._inUseSatelliteTreeCount == 0)
            {
                num = this.ComputeNodeByIndex(this.root, (int) (userIndex + 1));
                num2 = 0;
            }
            else
            {
                num = this.ComputeNodeByIndex(userIndex, out num2);
            }
            if (num != 0)
            {
                return new NodePath<K>(num, num2);
            }
            if (TreeAccessMethod.INDEX_ONLY == this._accessMethod)
            {
                throw ExceptionBuilder.RowOutOfRange(userIndex);
            }
            throw ExceptionBuilder.InternalRBTreeError(RBTreeError.IndexOutOFRangeinGetNodeByIndex);
        }

        private NodePath<K> GetNodeByKey(K key)
        {
            int nodeId = this.SearchSubTree(0, key);
            if (this.Next(nodeId) != 0)
            {
                return new NodePath<K>(this.SearchSubTree(this.Next(nodeId), key), nodeId);
            }
            if (!this.Key(nodeId).Equals(key))
            {
                nodeId = 0;
            }
            return new NodePath<K>(nodeId, 0);
        }

        private void IncreaseSize(int nodeId)
        {
            this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].subTreeSize++;
        }

        public int IndexOf(int nodeId, K item)
        {
            int index = -1;
            if (nodeId != 0)
            {
                if (this.Key(nodeId) == item)
                {
                    return this.GetIndexByNode(nodeId);
                }
                index = this.IndexOf(this.Left(nodeId), item);
                if (index != -1)
                {
                    return index;
                }
                index = this.IndexOf(this.Right(nodeId), item);
                if (index != -1)
                {
                    return index;
                }
            }
            return index;
        }

        private void InitTree()
        {
            this.root = 0;
            this._pageTable = new TreePage<K>[0x20];
            this._pageTableMap = new int[((this._pageTable.Length + 0x20) - 1) / 0x20];
            this._inUsePageCount = 0;
            this.nextFreePageLine = 0;
            this.AllocPage(0x20);
            this._pageTable[0].Slots[0].nodeColor = NodeColor<K>.black;
            this._pageTable[0].SlotMap[0] = 1;
            this._pageTable[0].InUseCount = 1;
            this._inUseNodeCount = 1;
            this._inUseSatelliteTreeCount = 0;
        }

        public int Insert(K item)
        {
            int newNode = this.GetNewNode(item);
            this.RBInsert(0, newNode, 0, -1, false);
            return newNode;
        }

        public int Insert(int position, K item)
        {
            return this.InsertAt(position, item, false);
        }

        public int InsertAt(int position, K item, bool append)
        {
            int newNode = this.GetNewNode(item);
            this.RBInsert(0, newNode, 0, position, append);
            return newNode;
        }

        public K Key(int nodeId)
        {
            return this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].keyOfNode;
        }

        public int Left(int nodeId)
        {
            return this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].leftId;
        }

        private int LeftRotate(int root_id, int x_id, int mainTreeNode)
        {
            int nodeId = this.Right(x_id);
            this.SetRight(x_id, this.Left(nodeId));
            if (this.Left(nodeId) != 0)
            {
                this.SetParent(this.Left(nodeId), x_id);
            }
            this.SetParent(nodeId, this.Parent(x_id));
            if (this.Parent(x_id) == 0)
            {
                if (root_id == 0)
                {
                    this.root = nodeId;
                }
                else
                {
                    this.SetNext(mainTreeNode, nodeId);
                    this.SetKey(mainTreeNode, this.Key(nodeId));
                    root_id = nodeId;
                }
            }
            else if (x_id == this.Left(this.Parent(x_id)))
            {
                this.SetLeft(this.Parent(x_id), nodeId);
            }
            else
            {
                this.SetRight(this.Parent(x_id), nodeId);
            }
            this.SetLeft(nodeId, x_id);
            this.SetParent(x_id, nodeId);
            if (x_id != 0)
            {
                this.SetSubTreeSize(x_id, (this.SubTreeSize(this.Left(x_id)) + this.SubTreeSize(this.Right(x_id))) + ((this.Next(x_id) == 0) ? 1 : this.SubTreeSize(this.Next(x_id))));
            }
            if (nodeId != 0)
            {
                this.SetSubTreeSize(nodeId, (this.SubTreeSize(this.Left(nodeId)) + this.SubTreeSize(this.Right(nodeId))) + ((this.Next(nodeId) == 0) ? 1 : this.SubTreeSize(this.Next(nodeId))));
            }
            return root_id;
        }

        private void MarkPageFree(TreePage<K> page)
        {
            this._pageTableMap[page.PageId / 0x20] &= ~(((int) 1) << (page.PageId % 0x20));
        }

        private void MarkPageFull(TreePage<K> page)
        {
            this._pageTableMap[page.PageId / 0x20] |= ((int) 1) << (page.PageId % 0x20);
        }

        private int Minimum(int x_id)
        {
            while (this.Left(x_id) != 0)
            {
                x_id = this.Left(x_id);
            }
            return x_id;
        }

        public int Next(int nodeId)
        {
            return this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].nextId;
        }

        public int Parent(int nodeId)
        {
            return this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].parentId;
        }

        public int RBDelete(int z_id)
        {
            return this.RBDeleteX(0, z_id, 0);
        }

        private int RBDeleteFixup(int root_id, int x_id, int px_id, int mainTreeNodeID)
        {
            if ((x_id != 0) || (px_id != 0))
            {
                while ((((root_id == 0) ? this.root : root_id) != x_id) && (this.color(x_id) == NodeColor<K>.black))
                {
                    int num;
                    if (((x_id != 0) && (x_id == this.Left(this.Parent(x_id)))) || ((x_id == 0) && (this.Left(px_id) == 0)))
                    {
                        num = (x_id == 0) ? this.Right(px_id) : this.Right(this.Parent(x_id));
                        if (num == 0)
                        {
                            throw ExceptionBuilder.InternalRBTreeError(RBTreeError.RBDeleteFixup);
                        }
                        if (this.color(num) == NodeColor<K>.red)
                        {
                            this.SetColor(num, NodeColor<K>.black);
                            this.SetColor(px_id, NodeColor<K>.red);
                            root_id = this.LeftRotate(root_id, px_id, mainTreeNodeID);
                            num = (x_id == 0) ? this.Right(px_id) : this.Right(this.Parent(x_id));
                        }
                        if ((this.color(this.Left(num)) == NodeColor<K>.black) && (this.color(this.Right(num)) == NodeColor<K>.black))
                        {
                            this.SetColor(num, NodeColor<K>.red);
                            x_id = px_id;
                            px_id = this.Parent(px_id);
                        }
                        else
                        {
                            if (this.color(this.Right(num)) == NodeColor<K>.black)
                            {
                                this.SetColor(this.Left(num), NodeColor<K>.black);
                                this.SetColor(num, NodeColor<K>.red);
                                root_id = this.RightRotate(root_id, num, mainTreeNodeID);
                                num = (x_id == 0) ? this.Right(px_id) : this.Right(this.Parent(x_id));
                            }
                            this.SetColor(num, this.color(px_id));
                            this.SetColor(px_id, NodeColor<K>.black);
                            this.SetColor(this.Right(num), NodeColor<K>.black);
                            root_id = this.LeftRotate(root_id, px_id, mainTreeNodeID);
                            x_id = (root_id == 0) ? this.root : root_id;
                            px_id = this.Parent(x_id);
                        }
                    }
                    else
                    {
                        num = this.Left(px_id);
                        if (this.color(num) == NodeColor<K>.red)
                        {
                            this.SetColor(num, NodeColor<K>.black);
                            if (x_id != 0)
                            {
                                this.SetColor(px_id, NodeColor<K>.red);
                                root_id = this.RightRotate(root_id, px_id, mainTreeNodeID);
                                num = (x_id == 0) ? this.Left(px_id) : this.Left(this.Parent(x_id));
                            }
                            else
                            {
                                this.SetColor(px_id, NodeColor<K>.red);
                                root_id = this.RightRotate(root_id, px_id, mainTreeNodeID);
                                num = (x_id == 0) ? this.Left(px_id) : this.Left(this.Parent(x_id));
                                if (num == 0)
                                {
                                    throw ExceptionBuilder.InternalRBTreeError(RBTreeError.CannotRotateInvalidsuccessorNodeinDelete);
                                }
                            }
                        }
                        if ((this.color(this.Right(num)) == NodeColor<K>.black) && (this.color(this.Left(num)) == NodeColor<K>.black))
                        {
                            this.SetColor(num, NodeColor<K>.red);
                            x_id = px_id;
                            px_id = this.Parent(px_id);
                        }
                        else
                        {
                            if (this.color(this.Left(num)) == NodeColor<K>.black)
                            {
                                this.SetColor(this.Right(num), NodeColor<K>.black);
                                this.SetColor(num, NodeColor<K>.red);
                                root_id = this.LeftRotate(root_id, num, mainTreeNodeID);
                                num = (x_id == 0) ? this.Left(px_id) : this.Left(this.Parent(x_id));
                            }
                            if (x_id != 0)
                            {
                                this.SetColor(num, this.color(px_id));
                                this.SetColor(px_id, NodeColor<K>.black);
                                this.SetColor(this.Left(num), NodeColor<K>.black);
                                root_id = this.RightRotate(root_id, px_id, mainTreeNodeID);
                                x_id = (root_id == 0) ? this.root : root_id;
                                px_id = this.Parent(x_id);
                            }
                            else
                            {
                                this.SetColor(num, this.color(px_id));
                                this.SetColor(px_id, NodeColor<K>.black);
                                this.SetColor(this.Left(num), NodeColor<K>.black);
                                root_id = this.RightRotate(root_id, px_id, mainTreeNodeID);
                                x_id = (root_id == 0) ? this.root : root_id;
                                px_id = this.Parent(x_id);
                            }
                        }
                    }
                }
                this.SetColor(x_id, NodeColor<K>.black);
                return root_id;
            }
            return 0;
        }

        private int RBDeleteX(int root_id, int z_id, int mainTreeNodeID)
        {
            int num2;
            int nodeId = 0;
            if (this.Next(z_id) != 0)
            {
                return this.RBDeleteX(this.Next(z_id), this.Next(z_id), z_id);
            }
            bool flag = false;
            int num = (this._accessMethod == TreeAccessMethod.KEY_SEARCH_AND_INDEX) ? mainTreeNodeID : z_id;
            if (this.Next(num) != 0)
            {
                root_id = this.Next(num);
            }
            if (this.SubTreeSize(this.Next(num)) == 2)
            {
                flag = true;
            }
            else if (this.SubTreeSize(this.Next(num)) == 1)
            {
                throw ExceptionBuilder.InternalRBTreeError(RBTreeError.InvalidNextSizeInDelete);
            }
            if ((this.Left(z_id) == 0) || (this.Right(z_id) == 0))
            {
                num2 = z_id;
            }
            else
            {
                num2 = this.Successor(z_id);
            }
            if (this.Left(num2) != 0)
            {
                nodeId = this.Left(num2);
            }
            else
            {
                nodeId = this.Right(num2);
            }
            int parentNodeId = this.Parent(num2);
            if (nodeId != 0)
            {
                this.SetParent(nodeId, parentNodeId);
            }
            if (parentNodeId == 0)
            {
                if (root_id == 0)
                {
                    this.root = nodeId;
                }
                else
                {
                    root_id = nodeId;
                }
            }
            else if (num2 == this.Left(parentNodeId))
            {
                this.SetLeft(parentNodeId, nodeId);
            }
            else
            {
                this.SetRight(parentNodeId, nodeId);
            }
            if (num2 != z_id)
            {
                this.SetKey(z_id, this.Key(num2));
                this.SetNext(z_id, this.Next(num2));
            }
            if (this.Next(num) != 0)
            {
                if ((root_id == 0) && (z_id != num))
                {
                    throw ExceptionBuilder.InternalRBTreeError(RBTreeError.InvalidStateinDelete);
                }
                if (root_id != 0)
                {
                    this.SetNext(num, root_id);
                    this.SetKey(num, this.Key(root_id));
                }
            }
            for (int i = parentNodeId; i != 0; i = this.Parent(i))
            {
                this.RecomputeSize(i);
            }
            if (root_id != 0)
            {
                for (int j = num; j != 0; j = this.Parent(j))
                {
                    this.DecreaseSize(j);
                }
            }
            if (this.color(num2) == NodeColor<K>.black)
            {
                root_id = this.RBDeleteFixup(root_id, nodeId, parentNodeId, mainTreeNodeID);
            }
            if (flag)
            {
                if ((num == 0) || (this.SubTreeSize(this.Next(num)) != 1))
                {
                    throw ExceptionBuilder.InternalRBTreeError(RBTreeError.InvalidNodeSizeinDelete);
                }
                this._inUseSatelliteTreeCount--;
                int num3 = this.Next(num);
                this.SetLeft(num3, this.Left(num));
                this.SetRight(num3, this.Right(num));
                this.SetSubTreeSize(num3, this.SubTreeSize(num));
                this.SetColor(num3, this.color(num));
                if (this.Parent(num) != 0)
                {
                    this.SetParent(num3, this.Parent(num));
                    if (this.Left(this.Parent(num)) == num)
                    {
                        this.SetLeft(this.Parent(num), num3);
                    }
                    else
                    {
                        this.SetRight(this.Parent(num), num3);
                    }
                }
                if (this.Left(num) != 0)
                {
                    this.SetParent(this.Left(num), num3);
                }
                if (this.Right(num) != 0)
                {
                    this.SetParent(this.Right(num), num3);
                }
                if (this.root == num)
                {
                    this.root = num3;
                }
                this.FreeNode(num);
                num = 0;
            }
            else if (this.Next(num) != 0)
            {
                if ((root_id == 0) && (z_id != num))
                {
                    throw ExceptionBuilder.InternalRBTreeError(RBTreeError.InvalidStateinEndDelete);
                }
                if (root_id != 0)
                {
                    this.SetNext(num, root_id);
                    this.SetKey(num, this.Key(root_id));
                }
            }
            if (num2 != z_id)
            {
                this.SetLeft(num2, this.Left(z_id));
                this.SetRight(num2, this.Right(z_id));
                this.SetColor(num2, this.color(z_id));
                this.SetSubTreeSize(num2, this.SubTreeSize(z_id));
                if (this.Parent(z_id) != 0)
                {
                    this.SetParent(num2, this.Parent(z_id));
                    if (this.Left(this.Parent(z_id)) == z_id)
                    {
                        this.SetLeft(this.Parent(z_id), num2);
                    }
                    else
                    {
                        this.SetRight(this.Parent(z_id), num2);
                    }
                }
                else
                {
                    this.SetParent(num2, 0);
                }
                if (this.Left(z_id) != 0)
                {
                    this.SetParent(this.Left(z_id), num2);
                }
                if (this.Right(z_id) != 0)
                {
                    this.SetParent(this.Right(z_id), num2);
                }
                if (this.root == z_id)
                {
                    this.root = num2;
                }
                else if (root_id == z_id)
                {
                    root_id = num2;
                }
                if ((num != 0) && (this.Next(num) == z_id))
                {
                    this.SetNext(num, num2);
                }
            }
            this.FreeNode(z_id);
            this._version++;
            return z_id;
        }

        private int RBInsert(int root_id, int x_id, int mainTreeNodeID, int position, bool append)
        {
            this._version++;
            int nodeId = 0;
            int num = (root_id == 0) ? this.root : root_id;
            if ((this._accessMethod != TreeAccessMethod.KEY_SEARCH_AND_INDEX) || append)
            {
                if ((this._accessMethod != TreeAccessMethod.INDEX_ONLY) && !append)
                {
                    throw ExceptionBuilder.InternalRBTreeError(RBTreeError.UnsupportedAccessMethod1);
                }
                if (position == -1)
                {
                    position = this.SubTreeSize(this.root);
                }
                while (num != 0)
                {
                    this.IncreaseSize(num);
                    nodeId = num;
                    int num5 = position - this.SubTreeSize(this.Left(nodeId));
                    if (num5 <= 0)
                    {
                        num = this.Left(num);
                    }
                    else
                    {
                        num = this.Right(num);
                        if (num != 0)
                        {
                            position = num5 - 1;
                        }
                    }
                }
            }
            else
            {
                while (num != 0)
                {
                    this.IncreaseSize(num);
                    nodeId = num;
                    int num6 = (root_id == 0) ? this.CompareNode(this.Key(x_id), this.Key(num)) : this.CompareSateliteTreeNode(this.Key(x_id), this.Key(num));
                    if (num6 < 0)
                    {
                        num = this.Left(num);
                    }
                    else
                    {
                        if (num6 > 0)
                        {
                            num = this.Right(num);
                            continue;
                        }
                        if (root_id != 0)
                        {
                            throw ExceptionBuilder.InternalRBTreeError(RBTreeError.InvalidStateinInsert);
                        }
                        if (this.Next(num) != 0)
                        {
                            root_id = this.RBInsert(this.Next(num), x_id, num, -1, false);
                            this.SetKey(num, this.Key(this.Next(num)));
                            return root_id;
                        }
                        int newNode = 0;
                        newNode = this.GetNewNode(this.Key(num));
                        this._inUseSatelliteTreeCount++;
                        this.SetNext(newNode, num);
                        this.SetColor(newNode, this.color(num));
                        this.SetParent(newNode, this.Parent(num));
                        this.SetLeft(newNode, this.Left(num));
                        this.SetRight(newNode, this.Right(num));
                        if (this.Left(this.Parent(num)) == num)
                        {
                            this.SetLeft(this.Parent(num), newNode);
                        }
                        else if (this.Right(this.Parent(num)) == num)
                        {
                            this.SetRight(this.Parent(num), newNode);
                        }
                        if (this.Left(num) != 0)
                        {
                            this.SetParent(this.Left(num), newNode);
                        }
                        if (this.Right(num) != 0)
                        {
                            this.SetParent(this.Right(num), newNode);
                        }
                        if (this.root == num)
                        {
                            this.root = newNode;
                        }
                        this.SetColor(num, NodeColor<K>.black);
                        this.SetParent(num, 0);
                        this.SetLeft(num, 0);
                        this.SetRight(num, 0);
                        int size = this.SubTreeSize(num);
                        this.SetSubTreeSize(num, 1);
                        root_id = this.RBInsert(num, x_id, newNode, -1, false);
                        this.SetSubTreeSize(newNode, size);
                        return root_id;
                    }
                }
            }
            this.SetParent(x_id, nodeId);
            if (nodeId == 0)
            {
                if (root_id == 0)
                {
                    this.root = x_id;
                }
                else
                {
                    this.SetNext(mainTreeNodeID, x_id);
                    this.SetKey(mainTreeNodeID, this.Key(x_id));
                    root_id = x_id;
                }
            }
            else
            {
                int num4 = 0;
                if (this._accessMethod == TreeAccessMethod.KEY_SEARCH_AND_INDEX)
                {
                    num4 = (root_id == 0) ? this.CompareNode(this.Key(x_id), this.Key(nodeId)) : this.CompareSateliteTreeNode(this.Key(x_id), this.Key(nodeId));
                }
                else
                {
                    if (this._accessMethod != TreeAccessMethod.INDEX_ONLY)
                    {
                        throw ExceptionBuilder.InternalRBTreeError(RBTreeError.UnsupportedAccessMethod2);
                    }
                    num4 = (position <= 0) ? -1 : 1;
                }
                if (num4 < 0)
                {
                    this.SetLeft(nodeId, x_id);
                }
                else
                {
                    this.SetRight(nodeId, x_id);
                }
            }
            this.SetLeft(x_id, 0);
            this.SetRight(x_id, 0);
            this.SetColor(x_id, NodeColor<K>.red);
            num = x_id;
            while (this.color(this.Parent(x_id)) == NodeColor<K>.red)
            {
                if (this.Parent(x_id) == this.Left(this.Parent(this.Parent(x_id))))
                {
                    nodeId = this.Right(this.Parent(this.Parent(x_id)));
                    if (this.color(nodeId) == NodeColor<K>.red)
                    {
                        this.SetColor(this.Parent(x_id), NodeColor<K>.black);
                        this.SetColor(nodeId, NodeColor<K>.black);
                        this.SetColor(this.Parent(this.Parent(x_id)), NodeColor<K>.red);
                        x_id = this.Parent(this.Parent(x_id));
                    }
                    else
                    {
                        if (x_id == this.Right(this.Parent(x_id)))
                        {
                            x_id = this.Parent(x_id);
                            root_id = this.LeftRotate(root_id, x_id, mainTreeNodeID);
                        }
                        this.SetColor(this.Parent(x_id), NodeColor<K>.black);
                        this.SetColor(this.Parent(this.Parent(x_id)), NodeColor<K>.red);
                        root_id = this.RightRotate(root_id, this.Parent(this.Parent(x_id)), mainTreeNodeID);
                    }
                }
                else
                {
                    nodeId = this.Left(this.Parent(this.Parent(x_id)));
                    if (this.color(nodeId) == NodeColor<K>.red)
                    {
                        this.SetColor(this.Parent(x_id), NodeColor<K>.black);
                        this.SetColor(nodeId, NodeColor<K>.black);
                        this.SetColor(this.Parent(this.Parent(x_id)), NodeColor<K>.red);
                        x_id = this.Parent(this.Parent(x_id));
                    }
                    else
                    {
                        if (x_id == this.Left(this.Parent(x_id)))
                        {
                            x_id = this.Parent(x_id);
                            root_id = this.RightRotate(root_id, x_id, mainTreeNodeID);
                        }
                        this.SetColor(this.Parent(x_id), NodeColor<K>.black);
                        this.SetColor(this.Parent(this.Parent(x_id)), NodeColor<K>.red);
                        root_id = this.LeftRotate(root_id, this.Parent(this.Parent(x_id)), mainTreeNodeID);
                    }
                }
            }
            if (root_id == 0)
            {
                this.SetColor(this.root, NodeColor<K>.black);
                return root_id;
            }
            this.SetColor(root_id, NodeColor<K>.black);
            return root_id;
        }

        private void RecomputeSize(int nodeId)
        {
            int num = (this.SubTreeSize(this.Left(nodeId)) + this.SubTreeSize(this.Right(nodeId))) + ((this.Next(nodeId) == 0) ? 1 : this.SubTreeSize(this.Next(nodeId)));
            this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].subTreeSize = num;
        }

        public void RemoveAt(int position)
        {
            this.DeleteByIndex(position);
        }

        public int Right(int nodeId)
        {
            return this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].rightId;
        }

        private int RightRotate(int root_id, int x_id, int mainTreeNode)
        {
            int nodeId = this.Left(x_id);
            this.SetLeft(x_id, this.Right(nodeId));
            if (this.Right(nodeId) != 0)
            {
                this.SetParent(this.Right(nodeId), x_id);
            }
            this.SetParent(nodeId, this.Parent(x_id));
            if (this.Parent(x_id) == 0)
            {
                if (root_id == 0)
                {
                    this.root = nodeId;
                }
                else
                {
                    this.SetNext(mainTreeNode, nodeId);
                    this.SetKey(mainTreeNode, this.Key(nodeId));
                    root_id = nodeId;
                }
            }
            else if (x_id == this.Left(this.Parent(x_id)))
            {
                this.SetLeft(this.Parent(x_id), nodeId);
            }
            else
            {
                this.SetRight(this.Parent(x_id), nodeId);
            }
            this.SetRight(nodeId, x_id);
            this.SetParent(x_id, nodeId);
            if (x_id != 0)
            {
                this.SetSubTreeSize(x_id, (this.SubTreeSize(this.Left(x_id)) + this.SubTreeSize(this.Right(x_id))) + ((this.Next(x_id) == 0) ? 1 : this.SubTreeSize(this.Next(x_id))));
            }
            if (nodeId != 0)
            {
                this.SetSubTreeSize(nodeId, (this.SubTreeSize(this.Left(nodeId)) + this.SubTreeSize(this.Right(nodeId))) + ((this.Next(nodeId) == 0) ? 1 : this.SubTreeSize(this.Next(nodeId))));
            }
            return root_id;
        }

        public int Search(K key)
        {
            int root = this.root;
            while (root != 0)
            {
                int num2 = this.CompareNode(key, this.Key(root));
                if (num2 == 0)
                {
                    return root;
                }
                if (num2 < 0)
                {
                    root = this.Left(root);
                }
                else
                {
                    root = this.Right(root);
                }
            }
            return root;
        }

        private int SearchSubTree(int root_id, K key)
        {
            if ((root_id != 0) && (this._accessMethod != TreeAccessMethod.KEY_SEARCH_AND_INDEX))
            {
                throw ExceptionBuilder.InternalRBTreeError(RBTreeError.UnsupportedAccessMethodInNonNillRootSubtree);
            }
            int nodeId = (root_id == 0) ? this.root : root_id;
            while (nodeId != 0)
            {
                int num2 = (root_id == 0) ? this.CompareNode(key, this.Key(nodeId)) : this.CompareSateliteTreeNode(key, this.Key(nodeId));
                if (num2 == 0)
                {
                    return nodeId;
                }
                if (num2 < 0)
                {
                    nodeId = this.Left(nodeId);
                }
                else
                {
                    nodeId = this.Right(nodeId);
                }
            }
            return nodeId;
        }

        private void SetColor(int nodeId, NodeColor<K> color)
        {
            this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].nodeColor = color;
        }

        private void SetKey(int nodeId, K key)
        {
            this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].keyOfNode = key;
        }

        private void SetLeft(int nodeId, int leftNodeId)
        {
            this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].leftId = leftNodeId;
        }

        private void SetNext(int nodeId, int nextNodeId)
        {
            this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].nextId = nextNodeId;
        }

        private void SetParent(int nodeId, int parentNodeId)
        {
            this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].parentId = parentNodeId;
        }

        private void SetRight(int nodeId, int rightNodeId)
        {
            this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].rightId = rightNodeId;
        }

        private void SetSubTreeSize(int nodeId, int size)
        {
            this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].subTreeSize = size;
        }

        public int SubTreeSize(int nodeId)
        {
            return this._pageTable[nodeId >> 0x10].Slots[nodeId & 0xffff].subTreeSize;
        }

        private int Successor(int x_id)
        {
            if (this.Right(x_id) != 0)
            {
                return this.Minimum(this.Right(x_id));
            }
            int nodeId = this.Parent(x_id);
            while ((nodeId != 0) && (x_id == this.Right(nodeId)))
            {
                x_id = nodeId;
                nodeId = this.Parent(nodeId);
            }
            return nodeId;
        }

        private bool Successor(ref int nodeId, ref int mainTreeNodeId)
        {
            if (nodeId == 0)
            {
                nodeId = this.Minimum(mainTreeNodeId);
                mainTreeNodeId = 0;
            }
            else
            {
                nodeId = this.Successor(nodeId);
                if ((nodeId == 0) && (mainTreeNodeId != 0))
                {
                    nodeId = this.Successor(mainTreeNodeId);
                    mainTreeNodeId = 0;
                }
            }
            if (nodeId == 0)
            {
                return false;
            }
            if (this.Next(nodeId) != 0)
            {
                if (mainTreeNodeId != 0)
                {
                    throw ExceptionBuilder.InternalRBTreeError(RBTreeError.NestedSatelliteTreeEnumerator);
                }
                mainTreeNodeId = nodeId;
                nodeId = this.Minimum(this.Next(nodeId));
            }
            return true;
        }

        public void UpdateNodeKey(K currentKey, K newKey)
        {
            NodePath<K> nodeByKey = this.GetNodeByKey(currentKey);
            if ((this.Parent(nodeByKey.NodeID) == 0) && (nodeByKey.NodeID != this.root))
            {
                this.SetKey(nodeByKey.MainTreeNodeID, newKey);
            }
            this.SetKey(nodeByKey.NodeID, newKey);
        }

        [Conditional("DEBUG")]
        private void VerifySize(int nodeId, int size)
        {
            this.SubTreeSize(this.Left(nodeId));
            this.SubTreeSize(this.Right(nodeId));
            if (this.Next(nodeId) != 0)
            {
                this.SubTreeSize(this.Next(nodeId));
            }
        }

        public int Count
        {
            get
            {
                return (this._inUseNodeCount - 1);
            }
        }

        public bool HasDuplicates
        {
            get
            {
                return (0 != this._inUseSatelliteTreeCount);
            }
        }

        public K this[int index]
        {
            get
            {
                return this.Key(this.GetNodeByIndex(index).NodeID);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Node
        {
            internal int selfId;
            internal int leftId;
            internal int rightId;
            internal int parentId;
            internal int nextId;
            internal int subTreeSize;
            internal K keyOfNode;
            internal RBTree<K>.NodeColor nodeColor;
        }

        private enum NodeColor
        {
            public const RBTree<K>.NodeColor black = RBTree<K>.NodeColor.black;,
            public const RBTree<K>.NodeColor red = RBTree<K>.NodeColor.red;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NodePath
        {
            internal readonly int NodeID;
            internal readonly int MainTreeNodeID;
            internal NodePath(int nodeID, int mainTreeNodeID)
            {
                this.NodeID = nodeID;
                this.MainTreeNodeID = mainTreeNodeID;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RBTreeEnumerator : IEnumerator<K>, IDisposable, IEnumerator
        {
            private readonly RBTree<K> tree;
            private readonly int version;
            private int index;
            private int mainTreeNodeId;
            private K current;
            internal RBTreeEnumerator(RBTree<K> tree)
            {
                this.tree = tree;
                this.version = tree._version;
                this.index = 0;
                this.mainTreeNodeId = tree.root;
                this.current = default(K);
            }

            internal RBTreeEnumerator(RBTree<K> tree, int position)
            {
                this.tree = tree;
                this.version = tree._version;
                if (position == 0)
                {
                    this.index = 0;
                    this.mainTreeNodeId = tree.root;
                }
                else
                {
                    this.index = tree.ComputeNodeByIndex(position - 1, out this.mainTreeNodeId);
                    if (this.index == 0)
                    {
                        throw ExceptionBuilder.InternalRBTreeError(RBTreeError.IndexOutOFRangeinGetNodeByIndex);
                    }
                }
                this.current = default(K);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this.version != this.tree._version)
                {
                    throw ExceptionBuilder.EnumeratorModified();
                }
                bool flag = this.tree.Successor(ref this.index, ref this.mainTreeNodeId);
                this.current = this.tree.Key(this.index);
                return flag;
            }

            public K Current
            {
                get
                {
                    return this.current;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
            void IEnumerator.Reset()
            {
                if (this.version != this.tree._version)
                {
                    throw ExceptionBuilder.EnumeratorModified();
                }
                this.index = 0;
                this.mainTreeNodeId = this.tree.root;
                this.current = default(K);
            }
        }

        private sealed class TreePage
        {
            private int _inUseCount;
            private int _nextFreeSlotLine;
            private int _pageId;
            public const int slotLineSize = 0x20;
            internal readonly int[] SlotMap;
            internal readonly RBTree<K>.Node[] Slots;

            internal TreePage(int size)
            {
                if (size > 0x10000)
                {
                    throw ExceptionBuilder.InternalRBTreeError(RBTreeError.InvalidPageSize);
                }
                this.Slots = new RBTree<K>.Node[size];
                this.SlotMap = new int[((size + 0x20) - 1) / 0x20];
            }

            internal int AllocSlot(RBTree<K> tree)
            {
                int index = 0;
                int num3 = 0;
                int intValueFromBitMap = -1;
                if (this._inUseCount < this.Slots.Length)
                {
                    for (index = this._nextFreeSlotLine; index < this.SlotMap.Length; index++)
                    {
                        if (this.SlotMap[index] < -1)
                        {
                            intValueFromBitMap = 0;
                            num3 = ~this.SlotMap[index] & (this.SlotMap[index] + 1);
                            this.SlotMap[index] |= num3;
                            this._inUseCount++;
                            if (this._inUseCount == this.Slots.Length)
                            {
                                tree.MarkPageFull((RBTree<K>.TreePage) this);
                            }
                            tree._inUseNodeCount++;
                            intValueFromBitMap = RBTree<K>.GetIntValueFromBitMap((uint) num3);
                            this._nextFreeSlotLine = index;
                            intValueFromBitMap = (index * 0x20) + intValueFromBitMap;
                            break;
                        }
                    }
                    if ((intValueFromBitMap == -1) && (this._nextFreeSlotLine != 0))
                    {
                        this._nextFreeSlotLine = 0;
                        intValueFromBitMap = this.AllocSlot(tree);
                    }
                }
                return intValueFromBitMap;
            }

            internal int InUseCount
            {
                get
                {
                    return this._inUseCount;
                }
                set
                {
                    this._inUseCount = value;
                }
            }

            internal int PageId
            {
                get
                {
                    return this._pageId;
                }
                set
                {
                    this._pageId = value;
                }
            }
        }
    }
}

