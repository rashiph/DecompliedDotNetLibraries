namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable, DebuggerTypeProxy(typeof(Collection.CollectionDebugView)), DebuggerDisplay("Count = {Count}")]
    public sealed class Collection : ICollection, IList, ISerializable, IDeserializationCallback
    {
        private CultureInfo m_CultureInfo;
        private SerializationInfo m_DeserializationInfo;
        private FastList m_ItemsList;
        private ArrayList m_Iterators;
        private Dictionary<string, Node> m_KeyedNodesHash;
        private const string SERIALIZATIONKEY_CULTUREINFO = "CultureInfo";
        private const string SERIALIZATIONKEY_KEYS = "Keys";
        private const string SERIALIZATIONKEY_KEYSCOUNT = "KeysCount";
        private const string SERIALIZATIONKEY_VALUES = "Values";

        public Collection()
        {
            this.Initialize(Utils.GetCultureInfo(), 0);
        }

        private Collection(SerializationInfo info, StreamingContext context)
        {
            this.m_DeserializationInfo = info;
        }

        public void Add(object Item, string Key = null, object Before = null, object After = null)
        {
            if ((Before != null) && (After != null))
            {
                throw new ArgumentException(Utils.GetResourceString("Collection_BeforeAfterExclusive"));
            }
            Node node = new Node(Key, Item);
            if (Key != null)
            {
                try
                {
                    this.m_KeyedNodesHash.Add(Key, node);
                }
                catch (ArgumentException)
                {
                    throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Collection_DuplicateKey")), 0x1c9);
                }
            }
            try
            {
                if ((Before == null) && (After == null))
                {
                    this.m_ItemsList.Add(node);
                }
                else if (Before != null)
                {
                    string key = Before as string;
                    if (key != null)
                    {
                        Node node2 = null;
                        if (!this.m_KeyedNodesHash.TryGetValue(key, out node2))
                        {
                            throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Before" }));
                        }
                        this.m_ItemsList.InsertBefore(node, node2);
                    }
                    else
                    {
                        this.m_ItemsList.Insert(Conversions.ToInteger(Before) - 1, node);
                    }
                }
                else
                {
                    string str2 = After as string;
                    if (str2 != null)
                    {
                        Node node3 = null;
                        if (!this.m_KeyedNodesHash.TryGetValue(str2, out node3))
                        {
                            throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "After" }));
                        }
                        this.m_ItemsList.InsertAfter(node, node3);
                    }
                    else
                    {
                        this.m_ItemsList.Insert(Conversions.ToInteger(After), node);
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (StackOverflowException)
            {
                throw;
            }
            catch (Exception)
            {
                if (Key != null)
                {
                    this.m_KeyedNodesHash.Remove(Key);
                }
                throw;
            }
            this.AdjustEnumeratorsOnNodeInserted(node);
        }

        internal void AddIterator(WeakReference weakref)
        {
            this.m_Iterators.Add(weakref);
        }

        private void AdjustEnumeratorsHelper(Node NewOrRemovedNode, ForEachEnum.AdjustIndexType Type)
        {
            for (int i = this.m_Iterators.Count - 1; i >= 0; i--)
            {
                WeakReference reference = (WeakReference) this.m_Iterators[i];
                if (reference.IsAlive)
                {
                    ForEachEnum target = (ForEachEnum) reference.Target;
                    if (target != null)
                    {
                        target.Adjust(NewOrRemovedNode, Type);
                    }
                }
                else
                {
                    this.m_Iterators.RemoveAt(i);
                }
            }
        }

        private void AdjustEnumeratorsOnNodeInserted(Node NewNode)
        {
            this.AdjustEnumeratorsHelper(NewNode, ForEachEnum.AdjustIndexType.Insert);
        }

        private void AdjustEnumeratorsOnNodeRemoved(Node RemovedNode)
        {
            this.AdjustEnumeratorsHelper(RemovedNode, ForEachEnum.AdjustIndexType.Remove);
        }

        public void Clear()
        {
            this.m_KeyedNodesHash.Clear();
            this.m_ItemsList.Clear();
            for (int i = this.m_Iterators.Count - 1; i >= 0; i--)
            {
                WeakReference reference = (WeakReference) this.m_Iterators[i];
                if (reference.IsAlive)
                {
                    ForEachEnum target = (ForEachEnum) reference.Target;
                    if (target != null)
                    {
                        target.AdjustOnListCleared();
                    }
                }
                else
                {
                    this.m_Iterators.RemoveAt(i);
                }
            }
        }

        public bool Contains(string Key)
        {
            if (Key == null)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Key" }));
            }
            return this.m_KeyedNodesHash.ContainsKey(Key);
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = this.m_Iterators.Count - 1; i >= 0; i--)
            {
                WeakReference reference = (WeakReference) this.m_Iterators[i];
                if (!reference.IsAlive)
                {
                    this.m_Iterators.RemoveAt(i);
                }
            }
            ForEachEnum target = new ForEachEnum(this);
            WeakReference reference2 = new WeakReference(target);
            target.WeakRef = reference2;
            this.m_Iterators.Add(reference2);
            return target;
        }

        internal Node GetFirstListNode()
        {
            return this.m_ItemsList.GetFirstListNode();
        }

        [SecurityCritical]
        private void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            string[] strArray = new string[(this.Count - 1) + 1];
            object[] objArray = new object[(this.Count - 1) + 1];
            Node firstListNode = this.GetFirstListNode();
            int num = 0;
            while (firstListNode != null)
            {
                int num2;
                if (firstListNode.m_Key != null)
                {
                    num++;
                }
                strArray[num2] = firstListNode.m_Key;
                objArray[num2] = firstListNode.m_Value;
                num2++;
                firstListNode = firstListNode.m_Next;
            }
            info.AddValue("Keys", strArray, typeof(string[]));
            info.AddValue("KeysCount", num, typeof(int));
            info.AddValue("Values", objArray, typeof(object[]));
            info.AddValue("CultureInfo", this.m_CultureInfo);
        }

        private void ICollectionCopyTo(Array array, int index)
        {
            int num;
            if (array == null)
            {
                throw new ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "array" }));
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_RankEQOne1", new string[] { "array" }));
            }
            if ((index < 0) || ((array.Length - index) < this.Count))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "index" }));
            }
            object[] objArray = array as object[];
            if (objArray != null)
            {
                int count = this.Count;
                for (num = 1; num <= count; num++)
                {
                    objArray[(index + num) - 1] = this[num];
                }
            }
            else
            {
                int num3 = this.Count;
                for (num = 1; num <= num3; num++)
                {
                    array.SetValue(this[num], (int) ((index + num) - 1));
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private IEnumerator ICollectionGetEnumerator()
        {
            return this.GetEnumerator();
        }

        private int IListAdd(object value)
        {
            this.Add(value, null, null, null);
            return (this.m_ItemsList.Count() - 1);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private void IListClear()
        {
            this.Clear();
        }

        private bool IListContains(object value)
        {
            return (this.IListIndexOf(value) != -1);
        }

        private int IListIndexOf(object value)
        {
            return this.m_ItemsList.IndexOfValue(value);
        }

        private void IListInsert(int index, object value)
        {
            Node node = new Node(null, value);
            this.m_ItemsList.Insert(index, node);
            this.AdjustEnumeratorsOnNodeInserted(node);
        }

        private void IListRemove(object value)
        {
            int index = this.IListIndexOf(value);
            if (index != -1)
            {
                this.IListRemoveAt(index);
            }
        }

        private void IListRemoveAt(int index)
        {
            Node removedNode = this.m_ItemsList.RemoveAt(index);
            this.AdjustEnumeratorsOnNodeRemoved(removedNode);
            if (removedNode.m_Key != null)
            {
                this.m_KeyedNodesHash.Remove(removedNode.m_Key);
            }
            removedNode.m_Prev = null;
            removedNode.m_Next = null;
        }

        private void IndexCheck(int Index)
        {
            if ((Index < 1) || (Index > this.m_ItemsList.Count()))
            {
                throw new IndexOutOfRangeException(Utils.GetResourceString("Argument_CollectionIndex"));
            }
        }

        private void Initialize(CultureInfo CultureInfo, int StartingHashCapacity = 0)
        {
            if (StartingHashCapacity > 0)
            {
                this.m_KeyedNodesHash = new Dictionary<string, Node>(StartingHashCapacity, StringComparer.Create(CultureInfo, true));
            }
            else
            {
                this.m_KeyedNodesHash = new Dictionary<string, Node>(StringComparer.Create(CultureInfo, true));
            }
            this.m_ItemsList = new FastList();
            this.m_Iterators = new ArrayList();
            this.m_CultureInfo = CultureInfo;
        }

        private FastList InternalItemsList()
        {
            return this.m_ItemsList;
        }

        private void OnDeserialization(object sender)
        {
            try
            {
                CultureInfo cultureInfo = (CultureInfo) this.m_DeserializationInfo.GetValue("CultureInfo", typeof(CultureInfo));
                if (cultureInfo == null)
                {
                    throw new SerializationException(Utils.GetResourceString("Serialization_MissingCultureInfo"));
                }
                string[] strArray = (string[]) this.m_DeserializationInfo.GetValue("Keys", typeof(string[]));
                object[] objArray = (object[]) this.m_DeserializationInfo.GetValue("Values", typeof(object[]));
                if (strArray == null)
                {
                    throw new SerializationException(Utils.GetResourceString("Serialization_MissingKeys"));
                }
                if (objArray == null)
                {
                    throw new SerializationException(Utils.GetResourceString("Serialization_MissingValues"));
                }
                if (strArray.Length != objArray.Length)
                {
                    throw new SerializationException(Utils.GetResourceString("Serialization_KeyValueDifferentSizes"));
                }
                int startingHashCapacity = this.m_DeserializationInfo.GetInt32("KeysCount");
                if ((startingHashCapacity < 0) || (startingHashCapacity > strArray.Length))
                {
                    startingHashCapacity = 0;
                }
                this.Initialize(cultureInfo, startingHashCapacity);
                int num3 = strArray.Length - 1;
                for (int i = 0; i <= num3; i++)
                {
                    this.Add(objArray[i], strArray[i], null, null);
                }
                this.m_DeserializationInfo = null;
            }
            finally
            {
                if (this.m_DeserializationInfo != null)
                {
                    this.m_DeserializationInfo = null;
                    this.Initialize(Utils.GetCultureInfo(), 0);
                }
            }
        }

        public void Remove(int Index)
        {
            this.IndexCheck(Index);
            Node removedNode = this.m_ItemsList.RemoveAt(Index - 1);
            this.AdjustEnumeratorsOnNodeRemoved(removedNode);
            if (removedNode.m_Key != null)
            {
                this.m_KeyedNodesHash.Remove(removedNode.m_Key);
            }
            removedNode.m_Prev = null;
            removedNode.m_Next = null;
        }

        public void Remove(string Key)
        {
            Node node = null;
            if (!this.m_KeyedNodesHash.TryGetValue(Key, out node))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Key" }));
            }
            this.AdjustEnumeratorsOnNodeRemoved(node);
            this.m_KeyedNodesHash.Remove(Key);
            this.m_ItemsList.RemoveNode(node);
            node.m_Prev = null;
            node.m_Next = null;
        }

        internal void RemoveIterator(WeakReference weakref)
        {
            this.m_Iterators.Remove(weakref);
        }

        public int Count
        {
            get
            {
                return this.m_ItemsList.Count();
            }
        }

        private int ICollectionCount
        {
            get
            {
                return this.m_ItemsList.Count();
            }
        }

        private bool ICollectionIsSynchronized
        {
            get
            {
                return false;
            }
        }

        private object ICollectionSyncRoot
        {
            get
            {
                return this;
            }
        }

        private bool IListIsFixedSize
        {
            get
            {
                return false;
            }
        }

        private bool IListIsReadOnly
        {
            get
            {
                return false;
            }
        }

        private object this[int index]
        {
            get
            {
                return this.m_ItemsList.get_Item(index).m_Value;
            }
            set
            {
                this.m_ItemsList.get_Item(index).m_Value = value;
            }
        }

        public object this[int Index]
        {
            get
            {
                this.IndexCheck(Index);
                return this.m_ItemsList.get_Item(Index - 1).m_Value;
            }
        }

        public object this[string Key]
        {
            get
            {
                if (Key == null)
                {
                    throw new IndexOutOfRangeException(Utils.GetResourceString("Argument_CollectionIndex"));
                }
                Node node = null;
                if (!this.m_KeyedNodesHash.TryGetValue(Key, out node))
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Index" }));
                }
                return node.m_Value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public object this[object Index]
        {
            get
            {
                int num;
                if (((Index is string) || (Index is char)) || (Index is char[]))
                {
                    string str = Conversions.ToString(Index);
                    return this[str];
                }
                try
                {
                    num = Conversions.ToInteger(Index);
                }
                catch (StackOverflowException exception)
                {
                    throw exception;
                }
                catch (OutOfMemoryException exception2)
                {
                    throw exception2;
                }
                catch (ThreadAbortException exception3)
                {
                    throw exception3;
                }
                catch (Exception)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Index" }));
                }
                return this[num];
            }
        }

        private int System.Collections.ICollection.Count
        {
            get
            {
                return this.m_ItemsList.Count();
            }
        }

        private bool System.Collections.ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        private object System.Collections.ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        private bool System.Collections.IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        private bool System.Collections.IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        private object this[int index]
        {
            get
            {
                return this.m_ItemsList.get_Item(index).m_Value;
            }
            set
            {
                this.m_ItemsList.get_Item(index).m_Value = value;
            }
        }

        internal sealed class CollectionDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private Collection m_InstanceBeingWatched;

            public CollectionDebugView(Collection RealClass)
            {
                this.m_InstanceBeingWatched = RealClass;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object[] Items
            {
                get
                {
                    int count = this.m_InstanceBeingWatched.Count;
                    if (count == 0)
                    {
                        return null;
                    }
                    object[] objArray2 = new object[count + 1];
                    objArray2[0] = Utils.GetResourceString("EmptyPlaceHolderMessage");
                    int num3 = count;
                    for (int i = 1; i <= num3; i++)
                    {
                        Collection.Node node = this.m_InstanceBeingWatched.InternalItemsList().get_Item(i - 1);
                        objArray2[i] = new Collection.KeyValuePair(node.m_Key, node.m_Value);
                    }
                    return objArray2;
                }
            }
        }

        private sealed class FastList
        {
            private int m_Count = 0;
            private Collection.Node m_EndOfList;
            private Collection.Node m_StartOfList;

            internal FastList()
            {
            }

            internal void Add(Collection.Node Node)
            {
                if (this.m_StartOfList == null)
                {
                    this.m_StartOfList = Node;
                }
                else
                {
                    this.m_EndOfList.m_Next = Node;
                    Node.m_Prev = this.m_EndOfList;
                }
                this.m_EndOfList = Node;
                this.m_Count++;
            }

            internal void Clear()
            {
                this.m_StartOfList = null;
                this.m_EndOfList = null;
                this.m_Count = 0;
            }

            internal int Count()
            {
                return this.m_Count;
            }

            private bool DataIsEqual(object obj1, object obj2)
            {
                return ((obj1 == obj2) || ((obj1.GetType() == obj2.GetType()) && object.Equals(obj1, obj2)));
            }

            private void DeleteNode(Collection.Node NodeToBeDeleted, Collection.Node PrevNode)
            {
                if (PrevNode == null)
                {
                    this.m_StartOfList = this.m_StartOfList.m_Next;
                    if (this.m_StartOfList == null)
                    {
                        this.m_EndOfList = null;
                    }
                    else
                    {
                        this.m_StartOfList.m_Prev = null;
                    }
                }
                else
                {
                    PrevNode.m_Next = NodeToBeDeleted.m_Next;
                    if (PrevNode.m_Next == null)
                    {
                        this.m_EndOfList = PrevNode;
                    }
                    else
                    {
                        PrevNode.m_Next.m_Prev = PrevNode;
                    }
                }
                this.m_Count--;
            }

            internal Collection.Node GetFirstListNode()
            {
                return this.m_StartOfList;
            }

            private Collection.Node GetNodeAtIndex(int Index, ref Collection.Node PrevNode = null)
            {
                Collection.Node startOfList = this.m_StartOfList;
                int num = 0;
                PrevNode = null;
                while ((num < Index) && (startOfList != null))
                {
                    PrevNode = startOfList;
                    startOfList = startOfList.m_Next;
                    num++;
                }
                return startOfList;
            }

            internal int IndexOfValue(object Value)
            {
                Collection.Node startOfList = this.m_StartOfList;
                for (int i = 0; startOfList != null; i++)
                {
                    if (this.DataIsEqual(startOfList.m_Value, Value))
                    {
                        return i;
                    }
                    startOfList = startOfList.m_Next;
                }
                return -1;
            }

            internal void Insert(int Index, Collection.Node Node)
            {
                Collection.Node prevNode = null;
                if ((Index < 0) || (Index > this.m_Count))
                {
                    throw new ArgumentOutOfRangeException("Index");
                }
                Collection.Node nodeAtIndex = this.GetNodeAtIndex(Index, ref prevNode);
                this.Insert(Node, prevNode, nodeAtIndex);
            }

            private void Insert(Collection.Node Node, Collection.Node PrevNode, Collection.Node CurrentNode)
            {
                Node.m_Next = CurrentNode;
                if (CurrentNode != null)
                {
                    CurrentNode.m_Prev = Node;
                }
                if (PrevNode == null)
                {
                    this.m_StartOfList = Node;
                }
                else
                {
                    PrevNode.m_Next = Node;
                    Node.m_Prev = PrevNode;
                }
                if (Node.m_Next == null)
                {
                    this.m_EndOfList = Node;
                }
                this.m_Count++;
            }

            internal void InsertAfter(Collection.Node Node, Collection.Node NodeToInsertAfter)
            {
                this.Insert(Node, NodeToInsertAfter, NodeToInsertAfter.m_Next);
            }

            internal void InsertBefore(Collection.Node Node, Collection.Node NodeToInsertBefore)
            {
                this.Insert(Node, NodeToInsertBefore.m_Prev, NodeToInsertBefore);
            }

            internal Collection.Node RemoveAt(int Index)
            {
                Collection.Node startOfList = this.m_StartOfList;
                int num = 0;
                Collection.Node prevNode = null;
                while ((num < Index) && (startOfList != null))
                {
                    prevNode = startOfList;
                    startOfList = startOfList.m_Next;
                    num++;
                }
                if (startOfList == null)
                {
                    throw new ArgumentOutOfRangeException("Index");
                }
                this.DeleteNode(startOfList, prevNode);
                return startOfList;
            }

            internal void RemoveNode(Collection.Node NodeToBeDeleted)
            {
                this.DeleteNode(NodeToBeDeleted, NodeToBeDeleted.m_Prev);
            }

            internal Collection.Node this[int Index]
            {
                get
                {
                    Collection.Node prevNode = null;
                    Collection.Node nodeAtIndex = this.GetNodeAtIndex(Index, ref prevNode);
                    if (nodeAtIndex == null)
                    {
                        throw new ArgumentOutOfRangeException("Index");
                    }
                    return nodeAtIndex;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyValuePair
        {
            private object m_Key;
            private object m_Value;
            internal KeyValuePair(object NewKey, object NewValue)
            {
                this = new Collection.KeyValuePair();
                this.m_Key = NewKey;
                this.m_Value = NewValue;
            }

            public object Key
            {
                get
                {
                    return this.m_Key;
                }
            }
            public object Value
            {
                get
                {
                    return this.m_Value;
                }
            }
        }

        internal sealed class Node
        {
            internal string m_Key;
            internal Collection.Node m_Next;
            internal Collection.Node m_Prev;
            internal object m_Value;

            internal Node(string Key, object Value)
            {
                this.m_Value = Value;
                this.m_Key = Key;
            }
        }
    }
}

