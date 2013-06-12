namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable, DebuggerTypeProxy(typeof(SortedSetDebugView<>)), DebuggerDisplay("Count = {Count}")]
    public class SortedSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable, ISerializable, IDeserializationCallback
    {
        private object _syncRoot;
        private IComparer<T> comparer;
        private const string ComparerName = "Comparer";
        private int count;
        private const string CountName = "Count";
        private const string EnumStartName = "EnumStarted";
        private const string EnumVersionName = "EnumVersion";
        private const string ItemsName = "Items";
        private static string lBoundActiveName;
        private static string maxName;
        private static string minName;
        private const string NodeValueName = "Item";
        private const string ReverseName = "Reverse";
        private Node<T> root;
        private SerializationInfo siInfo;
        internal const int StackAllocThreshold = 100;
        private const string TreeName = "Tree";
        private static string uBoundActiveName;
        private int version;
        private const string VersionName = "Version";

        static SortedSet()
        {
            SortedSet<T>.minName = "Min";
            SortedSet<T>.maxName = "Max";
            SortedSet<T>.lBoundActiveName = "lBoundActive";
            SortedSet<T>.uBoundActiveName = "uBoundActive";
        }

        public SortedSet()
        {
            this.comparer = Comparer<T>.Default;
        }

        public SortedSet(IComparer<T> comparer)
        {
            if (comparer == null)
            {
                this.comparer = Comparer<T>.Default;
            }
            else
            {
                this.comparer = comparer;
            }
        }

        public SortedSet(IEnumerable<T> collection) : this(collection, Comparer<T>.Default)
        {
        }

        protected SortedSet(SerializationInfo info, StreamingContext context)
        {
            this.siInfo = info;
        }

        public SortedSet(IEnumerable<T> collection, IComparer<T> comparer) : this(comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            SortedSet<T> set = collection as SortedSet<T>;
            SortedSet<T> set2 = collection as TreeSubSet<T>;
            if (((set != null) && (set2 == null)) && SortedSet<T>.AreComparersEqual((SortedSet<T>) this, set))
            {
                if (set.Count == 0)
                {
                    this.count = 0;
                    this.version = 0;
                    this.root = null;
                }
                else
                {
                    Stack<Node<T>> stack = new Stack<Node<T>>((2 * SortedSet<T>.log2(set.Count)) + 2);
                    Stack<Node<T>> stack2 = new Stack<Node<T>>((2 * SortedSet<T>.log2(set.Count)) + 2);
                    Node<T> root = set.root;
                    Node<T> item = (root != null) ? new Node<T>(root.Item, root.IsRed) : null;
                    this.root = item;
                    while (root != null)
                    {
                        stack.Push(root);
                        stack2.Push(item);
                        item.Left = (root.Left != null) ? new Node<T>(root.Left.Item, root.Left.IsRed) : null;
                        root = root.Left;
                        item = item.Left;
                    }
                    while (stack.Count != 0)
                    {
                        root = stack.Pop();
                        item = stack2.Pop();
                        Node<T> right = root.Right;
                        Node<T> left = null;
                        if (right != null)
                        {
                            left = new Node<T>(right.Item, right.IsRed);
                        }
                        item.Right = left;
                        while (right != null)
                        {
                            stack.Push(right);
                            stack2.Push(left);
                            left.Left = (right.Left != null) ? new Node<T>(right.Left.Item, right.Left.IsRed) : null;
                            right = right.Left;
                            left = left.Left;
                        }
                    }
                    this.count = set.count;
                    this.version = 0;
                }
            }
            else
            {
                List<T> list = new List<T>(collection);
                list.Sort(this.comparer);
                for (int i = 1; i < list.Count; i++)
                {
                    if (comparer.Compare(list[i], list[i - 1]) == 0)
                    {
                        list.RemoveAt(i);
                        i--;
                    }
                }
                this.root = SortedSet<T>.ConstructRootFromSortedArray(list.ToArray(), 0, list.Count - 1, null);
                this.count = list.Count;
                this.version = 0;
            }
        }

        public bool Add(T item)
        {
            return this.AddIfNotPresent(item);
        }

        private void AddAllElements(IEnumerable<T> collection)
        {
            foreach (T local in collection)
            {
                if (!this.Contains(local))
                {
                    this.Add(local);
                }
            }
        }

        internal virtual bool AddIfNotPresent(T item)
        {
            if (this.root == null)
            {
                this.root = new Node<T>(item, false);
                this.count = 1;
                this.version++;
                return true;
            }
            Node<T> root = this.root;
            Node<T> node = null;
            Node<T> grandParent = null;
            Node<T> greatGrandParent = null;
            this.version++;
            int num = 0;
            while (root != null)
            {
                num = this.comparer.Compare(item, root.Item);
                if (num == 0)
                {
                    this.root.IsRed = false;
                    return false;
                }
                if (SortedSet<T>.Is4Node(root))
                {
                    SortedSet<T>.Split4Node(root);
                    if (SortedSet<T>.IsRed(node))
                    {
                        this.InsertionBalance(root, ref node, grandParent, greatGrandParent);
                    }
                }
                greatGrandParent = grandParent;
                grandParent = node;
                node = root;
                root = (num < 0) ? root.Left : root.Right;
            }
            Node<T> current = new Node<T>(item);
            if (num > 0)
            {
                node.Right = current;
            }
            else
            {
                node.Left = current;
            }
            if (node.IsRed)
            {
                this.InsertionBalance(current, ref node, grandParent, greatGrandParent);
            }
            this.root.IsRed = false;
            this.count++;
            return true;
        }

        private static bool AreComparersEqual(SortedSet<T> set1, SortedSet<T> set2)
        {
            return set1.Comparer.Equals(set2.Comparer);
        }

        internal virtual bool BreadthFirstTreeWalk(TreeWalkPredicate<T> action)
        {
            if (this.root != null)
            {
                List<Node<T>> list = new List<Node<T>> {
                    this.root
                };
                while (list.Count != 0)
                {
                    Node<T> node = list[0];
                    list.RemoveAt(0);
                    if (!action(node))
                    {
                        return false;
                    }
                    if (node.Left != null)
                    {
                        list.Add(node.Left);
                    }
                    if (node.Right != null)
                    {
                        list.Add(node.Right);
                    }
                }
            }
            return true;
        }

        [SecurityCritical]
        private unsafe ElementCount<T> CheckUniqueAndUnfoundElements(IEnumerable<T> other, bool returnIfUnfound)
        {
            ElementCount<T> count;
            if (this.Count != 0)
            {
                BitHelper helper;
                int length = BitHelper.ToIntArrayLength(this.Count);
                if (length <= 100)
                {
                    int* bitArrayPtr = (int*) stackalloc byte[(((IntPtr) length) * 4)];
                    helper = new BitHelper(bitArrayPtr, length);
                }
                else
                {
                    int[] bitArray = new int[length];
                    helper = new BitHelper(bitArray, length);
                }
                int num4 = 0;
                int num5 = 0;
                foreach (T local in other)
                {
                    int bitPosition = this.InternalIndexOf(local);
                    if (bitPosition >= 0)
                    {
                        if (!helper.IsMarked(bitPosition))
                        {
                            helper.MarkBit(bitPosition);
                            num5++;
                        }
                    }
                    else
                    {
                        num4++;
                        if (returnIfUnfound)
                        {
                            break;
                        }
                    }
                }
                count.uniqueCount = num5;
                count.unfoundCount = num4;
                return count;
            }
            int num = 0;
            using (IEnumerator<T> enumerator = other.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    num++;
                    goto Label_0039;
                }
            }
        Label_0039:
            count.uniqueCount = 0;
            count.unfoundCount = num;
            return count;
        }

        public virtual void Clear()
        {
            this.root = null;
            this.count = 0;
            this.version++;
        }

        private static Node<T> ConstructRootFromSortedArray(T[] arr, int startIndex, int endIndex, Node<T> redNode)
        {
            int num = (endIndex - startIndex) + 1;
            if (num == 0)
            {
                return null;
            }
            Node<T> node = null;
            switch (num)
            {
                case 1:
                    node = new Node<T>(arr[startIndex], false);
                    if (redNode != null)
                    {
                        node.Left = redNode;
                    }
                    return node;

                case 2:
                    node = new Node<T>(arr[startIndex], false) {
                        Right = new Node<T>(arr[endIndex], false)
                    };
                    node.Right.IsRed = true;
                    if (redNode != null)
                    {
                        node.Left = redNode;
                    }
                    return node;

                case 3:
                    node = new Node<T>(arr[startIndex + 1], false) {
                        Left = new Node<T>(arr[startIndex], false),
                        Right = new Node<T>(arr[endIndex], false)
                    };
                    if (redNode != null)
                    {
                        node.Left.Left = redNode;
                    }
                    return node;
            }
            int index = (startIndex + endIndex) / 2;
            node = new Node<T>(arr[index], false) {
                Left = SortedSet<T>.ConstructRootFromSortedArray(arr, startIndex, index - 1, redNode)
            };
            if ((num % 2) == 0)
            {
                node.Right = SortedSet<T>.ConstructRootFromSortedArray(arr, index + 2, endIndex, new Node<T>(arr[index + 1], true));
                return node;
            }
            node.Right = SortedSet<T>.ConstructRootFromSortedArray(arr, index + 1, endIndex, null);
            return node;
        }

        public virtual bool Contains(T item)
        {
            return (this.FindNode(item) != null);
        }

        private bool ContainsAllElements(IEnumerable<T> collection)
        {
            foreach (T local in collection)
            {
                if (!this.Contains(local))
                {
                    return false;
                }
            }
            return true;
        }

        public void CopyTo(T[] array)
        {
            this.CopyTo(array, 0, this.Count);
        }

        public void CopyTo(T[] array, int index)
        {
            this.CopyTo(array, index, this.Count);
        }

        public void CopyTo(T[] array, int index, int count)
        {
            if (array == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.array);
            }
            if (index < 0)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.index);
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((index > array.Length) || (count > (array.Length - index)))
            {
                throw new ArgumentException(SR.GetString("Arg_ArrayPlusOffTooSmall"));
            }
            count += index;
            this.InOrderTreeWalk(delegate (Node<T> node) {
                if (index >= count)
                {
                    return false;
                }
                array[index++] = node.Item;
                return true;
            });
        }

        public static IEqualityComparer<SortedSet<T>> CreateSetComparer()
        {
            return new SortedSetEqualityComparer<T>();
        }

        public static IEqualityComparer<SortedSet<T>> CreateSetComparer(IEqualityComparer<T> memberEqualityComparer)
        {
            return new SortedSetEqualityComparer<T>(memberEqualityComparer);
        }

        internal virtual bool DoRemove(T item)
        {
            if (this.root == null)
            {
                return false;
            }
            this.version++;
            Node<T> root = this.root;
            Node<T> parent = null;
            Node<T> node3 = null;
            Node<T> match = null;
            Node<T> parentOfMatch = null;
            bool flag = false;
            while (root != null)
            {
                if (SortedSet<T>.Is2Node(root))
                {
                    if (parent == null)
                    {
                        root.IsRed = true;
                    }
                    else
                    {
                        Node<T> sibling = SortedSet<T>.GetSibling(root, parent);
                        if (sibling.IsRed)
                        {
                            if (parent.Right == sibling)
                            {
                                SortedSet<T>.RotateLeft(parent);
                            }
                            else
                            {
                                SortedSet<T>.RotateRight(parent);
                            }
                            parent.IsRed = true;
                            sibling.IsRed = false;
                            this.ReplaceChildOfNodeOrRoot(node3, parent, sibling);
                            node3 = sibling;
                            if (parent == match)
                            {
                                parentOfMatch = sibling;
                            }
                            sibling = (parent.Left == root) ? parent.Right : parent.Left;
                        }
                        if (SortedSet<T>.Is2Node(sibling))
                        {
                            SortedSet<T>.Merge2Nodes(parent, root, sibling);
                        }
                        else
                        {
                            TreeRotation rotation = SortedSet<T>.RotationNeeded(parent, root, sibling);
                            Node<T> newChild = null;
                            switch (rotation)
                            {
                                case TreeRotation.LeftRotation:
                                    sibling.Right.IsRed = false;
                                    newChild = SortedSet<T>.RotateLeft(parent);
                                    break;

                                case TreeRotation.RightRotation:
                                    sibling.Left.IsRed = false;
                                    newChild = SortedSet<T>.RotateRight(parent);
                                    break;

                                case TreeRotation.RightLeftRotation:
                                    newChild = SortedSet<T>.RotateRightLeft(parent);
                                    break;

                                case TreeRotation.LeftRightRotation:
                                    newChild = SortedSet<T>.RotateLeftRight(parent);
                                    break;
                            }
                            newChild.IsRed = parent.IsRed;
                            parent.IsRed = false;
                            root.IsRed = true;
                            this.ReplaceChildOfNodeOrRoot(node3, parent, newChild);
                            if (parent == match)
                            {
                                parentOfMatch = newChild;
                            }
                            node3 = newChild;
                        }
                    }
                }
                int num = flag ? -1 : this.comparer.Compare(item, root.Item);
                if (num == 0)
                {
                    flag = true;
                    match = root;
                    parentOfMatch = parent;
                }
                node3 = parent;
                parent = root;
                if (num < 0)
                {
                    root = root.Left;
                }
                else
                {
                    root = root.Right;
                }
            }
            if (match != null)
            {
                this.ReplaceNode(match, parentOfMatch, parent, node3);
                this.count--;
            }
            if (this.root != null)
            {
                this.root.IsRed = false;
            }
            return flag;
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (this.count != 0)
            {
                if (other == this)
                {
                    this.Clear();
                }
                else
                {
                    SortedSet<T> set = other as SortedSet<T>;
                    if ((set != null) && SortedSet<T>.AreComparersEqual((SortedSet<T>) this, set))
                    {
                        if ((this.comparer.Compare(set.Max, this.Min) >= 0) && (this.comparer.Compare(set.Min, this.Max) <= 0))
                        {
                            T min = this.Min;
                            T max = this.Max;
                            foreach (T local3 in other)
                            {
                                if (this.comparer.Compare(local3, min) >= 0)
                                {
                                    if (this.comparer.Compare(local3, max) > 0)
                                    {
                                        break;
                                    }
                                    this.Remove(local3);
                                }
                            }
                        }
                    }
                    else
                    {
                        this.RemoveAllElements(other);
                    }
                }
            }
        }

        internal virtual Node<T> FindNode(T item)
        {
            int num;
            for (Node<T> node = this.root; node != null; node = (num < 0) ? node.Left : node.Right)
            {
                num = this.comparer.Compare(item, node.Item);
                if (num == 0)
                {
                    return node;
                }
            }
            return null;
        }

        internal Node<T> FindRange(T from, T to)
        {
            return this.FindRange(from, to, true, true);
        }

        internal Node<T> FindRange(T from, T to, bool lowerBoundActive, bool upperBoundActive)
        {
            Node<T> root = this.root;
            while (root != null)
            {
                if (lowerBoundActive && (this.comparer.Compare(from, root.Item) > 0))
                {
                    root = root.Right;
                }
                else
                {
                    if (upperBoundActive && (this.comparer.Compare(to, root.Item) < 0))
                    {
                        root = root.Left;
                        continue;
                    }
                    return root;
                }
            }
            return null;
        }

        public Enumerator<T> GetEnumerator()
        {
            return new Enumerator<T>((SortedSet<T>) this);
        }

        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.info);
            }
            info.AddValue("Count", this.count);
            info.AddValue("Comparer", this.comparer, typeof(IComparer<T>));
            info.AddValue("Version", this.version);
            if (this.root != null)
            {
                T[] array = new T[this.Count];
                this.CopyTo(array, 0);
                info.AddValue("Items", array, typeof(T[]));
            }
        }

        private static Node<T> GetSibling(Node<T> node, Node<T> parent)
        {
            if (parent.Left == node)
            {
                return parent.Right;
            }
            return parent.Left;
        }

        public virtual SortedSet<T> GetViewBetween(T lowerValue, T upperValue)
        {
            if (this.Comparer.Compare(lowerValue, upperValue) > 0)
            {
                throw new ArgumentException("lowerBound is greater than upperBound");
            }
            return new TreeSubSet<T>((SortedSet<T>) this, lowerValue, upperValue, true, true);
        }

        internal bool InOrderTreeWalk(TreeWalkPredicate<T> action)
        {
            return this.InOrderTreeWalk(action, false);
        }

        internal virtual bool InOrderTreeWalk(TreeWalkPredicate<T> action, bool reverse)
        {
            if (this.root != null)
            {
                Stack<Node<T>> stack = new Stack<Node<T>>(2 * SortedSet<T>.log2(this.Count + 1));
                Node<T> root = this.root;
                while (root != null)
                {
                    stack.Push(root);
                    root = reverse ? root.Right : root.Left;
                }
                while (stack.Count != 0)
                {
                    root = stack.Pop();
                    if (!action(root))
                    {
                        return false;
                    }
                    for (Node<T> node2 = reverse ? root.Left : root.Right; node2 != null; node2 = reverse ? node2.Right : node2.Left)
                    {
                        stack.Push(node2);
                    }
                }
            }
            return true;
        }

        private void InsertionBalance(Node<T> current, ref Node<T> parent, Node<T> grandParent, Node<T> greatGrandParent)
        {
            Node<T> node;
            bool flag = grandParent.Right == parent;
            bool flag2 = parent.Right == current;
            if (flag == flag2)
            {
                node = flag2 ? SortedSet<T>.RotateLeft(grandParent) : SortedSet<T>.RotateRight(grandParent);
            }
            else
            {
                node = flag2 ? SortedSet<T>.RotateLeftRight(grandParent) : SortedSet<T>.RotateRightLeft(grandParent);
                parent = greatGrandParent;
            }
            grandParent.IsRed = true;
            node.IsRed = false;
            this.ReplaceChildOfNodeOrRoot(greatGrandParent, grandParent, node);
        }

        internal virtual int InternalIndexOf(T item)
        {
            int num2;
            Node<T> root = this.root;
            for (int i = 0; root != null; i = (num2 < 0) ? ((2 * i) + 1) : ((2 * i) + 2))
            {
                num2 = this.comparer.Compare(item, root.Item);
                if (num2 == 0)
                {
                    return i;
                }
                root = (num2 < 0) ? root.Left : root.Right;
            }
            return -1;
        }

        public virtual void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (this.Count != 0)
            {
                SortedSet<T> set = other as SortedSet<T>;
                TreeSubSet<T> set2 = this as TreeSubSet<T>;
                if (set2 != null)
                {
                    this.VersionCheck();
                }
                if (((set == null) || (set2 != null)) || !SortedSet<T>.AreComparersEqual((SortedSet<T>) this, set))
                {
                    this.IntersectWithEnumerable(other);
                }
                else
                {
                    T[] arr = new T[this.Count];
                    int num = 0;
                    Enumerator<T> enumerator = this.GetEnumerator();
                    Enumerator<T> enumerator2 = set.GetEnumerator();
                    bool flag = !enumerator.MoveNext();
                    bool flag2 = !enumerator2.MoveNext();
                    T max = this.Max;
                    T min = this.Min;
                    while ((!flag && !flag2) && (this.Comparer.Compare(enumerator2.Current, max) <= 0))
                    {
                        int num2 = this.Comparer.Compare(enumerator.Current, enumerator2.Current);
                        if (num2 < 0)
                        {
                            flag = !enumerator.MoveNext();
                        }
                        else
                        {
                            if (num2 == 0)
                            {
                                arr[num++] = enumerator2.Current;
                                flag = !enumerator.MoveNext();
                                flag2 = !enumerator2.MoveNext();
                                continue;
                            }
                            flag2 = !enumerator2.MoveNext();
                        }
                    }
                    this.root = null;
                    this.root = SortedSet<T>.ConstructRootFromSortedArray(arr, 0, num - 1, null);
                    this.count = num;
                    this.version++;
                }
            }
        }

        internal virtual void IntersectWithEnumerable(IEnumerable<T> other)
        {
            List<T> collection = new List<T>(this.Count);
            foreach (T local in other)
            {
                if (this.Contains(local))
                {
                    collection.Add(local);
                    this.Remove(local);
                }
            }
            this.Clear();
            this.AddAllElements(collection);
        }

        private static bool Is2Node(Node<T> node)
        {
            return ((SortedSet<T>.IsBlack(node) && SortedSet<T>.IsNullOrBlack(node.Left)) && SortedSet<T>.IsNullOrBlack(node.Right));
        }

        private static bool Is4Node(Node<T> node)
        {
            return (SortedSet<T>.IsRed(node.Left) && SortedSet<T>.IsRed(node.Right));
        }

        private static bool IsBlack(Node<T> node)
        {
            return ((node != null) && !node.IsRed);
        }

        private static bool IsNullOrBlack(Node<T> node)
        {
            if (node != null)
            {
                return !node.IsRed;
            }
            return true;
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if ((other is ICollection) && (this.Count == 0))
            {
                return ((other as ICollection).Count > 0);
            }
            SortedSet<T> set = other as SortedSet<T>;
            if ((set != null) && SortedSet<T>.AreComparersEqual((SortedSet<T>) this, set))
            {
                if (this.Count >= set.Count)
                {
                    return false;
                }
                return this.IsSubsetOfSortedSetWithSameEC(set);
            }
            ElementCount<T> count = this.CheckUniqueAndUnfoundElements(other, false);
            return ((count.uniqueCount == this.Count) && (count.unfoundCount > 0));
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (this.Count == 0)
            {
                return false;
            }
            if ((other is ICollection) && ((other as ICollection).Count == 0))
            {
                return true;
            }
            SortedSet<T> set = other as SortedSet<T>;
            if ((set != null) && SortedSet<T>.AreComparersEqual(set, (SortedSet<T>) this))
            {
                if (set.Count >= this.Count)
                {
                    return false;
                }
                SortedSet<T> viewBetween = this.GetViewBetween(set.Min, set.Max);
                foreach (T local in set)
                {
                    if (!viewBetween.Contains(local))
                    {
                        return false;
                    }
                }
                return true;
            }
            ElementCount<T> count = this.CheckUniqueAndUnfoundElements(other, true);
            return ((count.uniqueCount < this.Count) && (count.unfoundCount == 0));
        }

        private static bool IsRed(Node<T> node)
        {
            return ((node != null) && node.IsRed);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (this.Count == 0)
            {
                return true;
            }
            SortedSet<T> set = other as SortedSet<T>;
            if ((set != null) && SortedSet<T>.AreComparersEqual((SortedSet<T>) this, set))
            {
                if (this.Count > set.Count)
                {
                    return false;
                }
                return this.IsSubsetOfSortedSetWithSameEC(set);
            }
            ElementCount<T> count = this.CheckUniqueAndUnfoundElements(other, false);
            return ((count.uniqueCount == this.Count) && (count.unfoundCount >= 0));
        }

        private bool IsSubsetOfSortedSetWithSameEC(SortedSet<T> asSorted)
        {
            SortedSet<T> viewBetween = asSorted.GetViewBetween(this.Min, this.Max);
            foreach (T local in this)
            {
                if (!viewBetween.Contains(local))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (!(other is ICollection) || ((other as ICollection).Count != 0))
            {
                SortedSet<T> set = other as SortedSet<T>;
                if ((set == null) || !SortedSet<T>.AreComparersEqual((SortedSet<T>) this, set))
                {
                    return this.ContainsAllElements(other);
                }
                if (this.Count < set.Count)
                {
                    return false;
                }
                SortedSet<T> viewBetween = this.GetViewBetween(set.Min, set.Max);
                foreach (T local in set)
                {
                    if (!viewBetween.Contains(local))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal virtual bool IsWithinRange(T item)
        {
            return true;
        }

        private static int log2(int value)
        {
            int num = 0;
            while (value > 0)
            {
                num++;
                value = value >> 1;
            }
            return num;
        }

        private static void Merge2Nodes(Node<T> parent, Node<T> child1, Node<T> child2)
        {
            parent.IsRed = false;
            child1.IsRed = true;
            child2.IsRed = true;
        }

        protected virtual void OnDeserialization(object sender)
        {
            if (this.comparer == null)
            {
                if (this.siInfo == null)
                {
                    System.ThrowHelper.ThrowSerializationException(System.ExceptionResource.Serialization_InvalidOnDeser);
                }
                this.comparer = (IComparer<T>) this.siInfo.GetValue("Comparer", typeof(IComparer<T>));
                int num = this.siInfo.GetInt32("Count");
                if (num != 0)
                {
                    T[] localArray = (T[]) this.siInfo.GetValue("Items", typeof(T[]));
                    if (localArray == null)
                    {
                        System.ThrowHelper.ThrowSerializationException(System.ExceptionResource.Serialization_MissingValues);
                    }
                    for (int i = 0; i < localArray.Length; i++)
                    {
                        this.Add(localArray[i]);
                    }
                }
                this.version = this.siInfo.GetInt32("Version");
                if (this.count != num)
                {
                    System.ThrowHelper.ThrowSerializationException(System.ExceptionResource.Serialization_MismatchedCount);
                }
                this.siInfo = null;
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (this.Count != 0)
            {
                if ((other is ICollection<T>) && ((other as ICollection<T>).Count == 0))
                {
                    return false;
                }
                SortedSet<T> set = other as SortedSet<T>;
                if (((set != null) && SortedSet<T>.AreComparersEqual((SortedSet<T>) this, set)) && ((this.comparer.Compare(this.Min, set.Max) > 0) || (this.comparer.Compare(this.Max, set.Min) < 0)))
                {
                    return false;
                }
                foreach (T local in other)
                {
                    if (this.Contains(local))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Remove(T item)
        {
            return this.DoRemove(item);
        }

        private void RemoveAllElements(IEnumerable<T> collection)
        {
            T min = this.Min;
            T max = this.Max;
            foreach (T local3 in collection)
            {
                if (((this.comparer.Compare(local3, min) >= 0) && (this.comparer.Compare(local3, max) <= 0)) && this.Contains(local3))
                {
                    this.Remove(local3);
                }
            }
        }

        public int RemoveWhere(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            List<T> matches = new List<T>(this.Count);
            this.BreadthFirstTreeWalk(delegate (Node<T> n) {
                if (match(n.Item))
                {
                    matches.Add(n.Item);
                }
                return true;
            });
            int num = 0;
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                if (this.Remove(matches[i]))
                {
                    num++;
                }
            }
            return num;
        }

        private void ReplaceChildOfNodeOrRoot(Node<T> parent, Node<T> child, Node<T> newChild)
        {
            if (parent != null)
            {
                if (parent.Left == child)
                {
                    parent.Left = newChild;
                }
                else
                {
                    parent.Right = newChild;
                }
            }
            else
            {
                this.root = newChild;
            }
        }

        private void ReplaceNode(Node<T> match, Node<T> parentOfMatch, Node<T> succesor, Node<T> parentOfSuccesor)
        {
            if (succesor == match)
            {
                succesor = match.Left;
            }
            else
            {
                if (succesor.Right != null)
                {
                    succesor.Right.IsRed = false;
                }
                if (parentOfSuccesor != match)
                {
                    parentOfSuccesor.Left = succesor.Right;
                    succesor.Right = match.Right;
                }
                succesor.Left = match.Left;
            }
            if (succesor != null)
            {
                succesor.IsRed = match.IsRed;
            }
            this.ReplaceChildOfNodeOrRoot(parentOfMatch, match, succesor);
        }

        public IEnumerable<T> Reverse()
        {
            Enumerator<T> iteratorVariable0 = new Enumerator<T>((SortedSet<T>) this, true);
            while (true)
            {
                if (!iteratorVariable0.MoveNext())
                {
                    yield break;
                }
                yield return iteratorVariable0.Current;
            }
        }

        private static Node<T> RotateLeft(Node<T> node)
        {
            Node<T> right = node.Right;
            node.Right = right.Left;
            right.Left = node;
            return right;
        }

        private static Node<T> RotateLeftRight(Node<T> node)
        {
            Node<T> left = node.Left;
            Node<T> right = left.Right;
            node.Left = right.Right;
            right.Right = node;
            left.Right = right.Left;
            right.Left = left;
            return right;
        }

        private static Node<T> RotateRight(Node<T> node)
        {
            Node<T> left = node.Left;
            node.Left = left.Right;
            left.Right = node;
            return left;
        }

        private static Node<T> RotateRightLeft(Node<T> node)
        {
            Node<T> right = node.Right;
            Node<T> left = right.Left;
            node.Right = left.Left;
            left.Left = node;
            right.Left = left.Right;
            left.Right = right;
            return left;
        }

        private static TreeRotation RotationNeeded(Node<T> parent, Node<T> current, Node<T> sibling)
        {
            if (SortedSet<T>.IsRed(sibling.Left))
            {
                if (parent.Left == current)
                {
                    return TreeRotation.RightLeftRotation;
                }
                return TreeRotation.RightRotation;
            }
            if (parent.Left == current)
            {
                return TreeRotation.LeftRotation;
            }
            return TreeRotation.LeftRightRotation;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            SortedSet<T> set = other as SortedSet<T>;
            if ((set != null) && SortedSet<T>.AreComparersEqual((SortedSet<T>) this, set))
            {
                IEnumerator<T> enumerator = this.GetEnumerator();
                IEnumerator<T> enumerator2 = set.GetEnumerator();
                bool flag = !enumerator.MoveNext();
                bool flag2 = !enumerator2.MoveNext();
                while (!flag && !flag2)
                {
                    if (this.Comparer.Compare(enumerator.Current, enumerator2.Current) != 0)
                    {
                        return false;
                    }
                    flag = !enumerator.MoveNext();
                    flag2 = !enumerator2.MoveNext();
                }
                return (flag && flag2);
            }
            ElementCount<T> count = this.CheckUniqueAndUnfoundElements(other, true);
            return ((count.uniqueCount == this.Count) && (count.unfoundCount == 0));
        }

        internal static bool SortedSetEquals(SortedSet<T> set1, SortedSet<T> set2, IComparer<T> comparer)
        {
            if (set1 == null)
            {
                return (set2 == null);
            }
            if (set2 == null)
            {
                return false;
            }
            if (SortedSet<T>.AreComparersEqual(set1, set2))
            {
                if (set1.Count != set2.Count)
                {
                    return false;
                }
                return set1.SetEquals(set2);
            }
            bool flag = false;
            foreach (T local in set1)
            {
                flag = false;
                foreach (T local2 in set2)
                {
                    if (comparer.Compare(local, local2) == 0)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    return false;
                }
            }
            return true;
        }

        private static void Split4Node(Node<T> node)
        {
            node.IsRed = true;
            node.Left.IsRed = false;
            node.Right.IsRed = false;
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (this.Count == 0)
            {
                this.UnionWith(other);
            }
            else if (other == this)
            {
                this.Clear();
            }
            else
            {
                SortedSet<T> set = other as SortedSet<T>;
                if ((set != null) && SortedSet<T>.AreComparersEqual((SortedSet<T>) this, set))
                {
                    this.SymmetricExceptWithSameEC(set);
                }
                else
                {
                    T[] array = new List<T>(other).ToArray();
                    Array.Sort<T>(array, this.Comparer);
                    this.SymmetricExceptWithSameEC(array);
                }
            }
        }

        internal void SymmetricExceptWithSameEC(ISet<T> other)
        {
            foreach (T local in other)
            {
                if (this.Contains(local))
                {
                    this.Remove(local);
                }
                else
                {
                    this.Add(local);
                }
            }
        }

        internal void SymmetricExceptWithSameEC(T[] other)
        {
            if (other.Length != 0)
            {
                T y = other[0];
                for (int i = 0; i < other.Length; i++)
                {
                    while (((i < other.Length) && (i != 0)) && (this.comparer.Compare(other[i], y) == 0))
                    {
                        i++;
                    }
                    if (i >= other.Length)
                    {
                        return;
                    }
                    if (this.Contains(other[i]))
                    {
                        this.Remove(other[i]);
                    }
                    else
                    {
                        this.Add(other[i]);
                    }
                    y = other[i];
                }
            }
        }

        void ICollection<T>.Add(T item)
        {
            this.AddIfNotPresent(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator<T>((SortedSet<T>) this);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.array);
            }
            if (array.Rank != 1)
            {
                System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_RankMultiDimNotSupported);
            }
            if (array.GetLowerBound(0) != 0)
            {
                System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_NonZeroLowerBound);
            }
            if (index < 0)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.arrayIndex, System.ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((array.Length - index) < this.Count)
            {
                System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }
            T[] localArray = array as T[];
            if (localArray != null)
            {
                this.CopyTo(localArray, index);
            }
            else
            {
                TreeWalkPredicate<T> action = null;
                object[] objects = array as object[];
                if (objects == null)
                {
                    System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Argument_InvalidArrayType);
                }
                try
                {
                    if (action == null)
                    {
                        action = delegate (Node<T> node) {
                            objects[index++] = node.Item;
                            return true;
                        };
                    }
                    this.InOrderTreeWalk(action);
                }
                catch (ArrayTypeMismatchException)
                {
                    System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Argument_InvalidArrayType);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<T>((SortedSet<T>) this);
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            this.OnDeserialization(sender);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.GetObjectData(info, context);
        }

        internal T[] ToArray()
        {
            T[] array = new T[this.Count];
            this.CopyTo(array);
            return array;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            SortedSet<T> collection = other as SortedSet<T>;
            TreeSubSet<T> set2 = this as TreeSubSet<T>;
            if (set2 != null)
            {
                this.VersionCheck();
            }
            if (((collection != null) && (set2 == null)) && (this.count == 0))
            {
                SortedSet<T> set3 = new SortedSet<T>(collection, this.comparer);
                this.root = set3.root;
                this.count = set3.count;
                this.version++;
            }
            else if (((collection == null) || (set2 != null)) || (!SortedSet<T>.AreComparersEqual((SortedSet<T>) this, collection) || (collection.Count <= (this.Count / 2))))
            {
                this.AddAllElements(other);
            }
            else
            {
                T[] arr = new T[collection.Count + this.Count];
                int num = 0;
                Enumerator<T> enumerator = this.GetEnumerator();
                Enumerator<T> enumerator2 = collection.GetEnumerator();
                bool flag = !enumerator.MoveNext();
                bool flag2 = !enumerator2.MoveNext();
                while (!flag && !flag2)
                {
                    int num2 = this.Comparer.Compare(enumerator.Current, enumerator2.Current);
                    if (num2 < 0)
                    {
                        arr[num++] = enumerator.Current;
                        flag = !enumerator.MoveNext();
                    }
                    else
                    {
                        if (num2 == 0)
                        {
                            arr[num++] = enumerator2.Current;
                            flag = !enumerator.MoveNext();
                            flag2 = !enumerator2.MoveNext();
                            continue;
                        }
                        arr[num++] = enumerator2.Current;
                        flag2 = !enumerator2.MoveNext();
                    }
                }
                if (!flag || !flag2)
                {
                    Enumerator<T> enumerator3 = flag ? enumerator2 : enumerator;
                    do
                    {
                        arr[num++] = enumerator3.Current;
                    }
                    while (enumerator3.MoveNext());
                }
                this.root = null;
                this.root = SortedSet<T>.ConstructRootFromSortedArray(arr, 0, num - 1, null);
                this.count = num;
                this.version++;
            }
        }

        internal void UpdateVersion()
        {
            this.version++;
        }

        internal virtual void VersionCheck()
        {
        }

        public IComparer<T> Comparer
        {
            get
            {
                return this.comparer;
            }
        }

        public int Count
        {
            get
            {
                this.VersionCheck();
                return this.count;
            }
        }

        public T Max
        {
            get
            {
                T ret = default(T);
                this.InOrderTreeWalk(delegate (Node<T> n) {
                    ret = n.Item;
                    return false;
                }, true);
                return ret;
            }
        }

        public T Min
        {
            get
            {
                T ret = default(T);
                this.InOrderTreeWalk(delegate (Node<T> n) {
                    ret = n.Item;
                    return false;
                });
                return ret;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
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
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        [CompilerGenerated]
        private sealed class <Reverse>d__12 : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private T <>2__current;
            public SortedSet<T> <>4__this;
            private int <>l__initialThreadId;
            public SortedSet<T>.Enumerator <e>5__13;

            [DebuggerHidden]
            public <Reverse>d__12(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<e>5__13 = new SortedSet<T>.Enumerator(this.<>4__this, true);
                        break;

                    case 1:
                        this.<>1__state = -1;
                        break;

                    default:
                        goto Label_0065;
                }
                if (this.<e>5__13.MoveNext())
                {
                    this.<>2__current = this.<e>5__13.Current;
                    this.<>1__state = 1;
                    return true;
                }
            Label_0065:
                return false;
            }

            [DebuggerHidden]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    return (SortedSet<T>.<Reverse>d__12) this;
                }
                return new SortedSet<T>.<Reverse>d__12(0) { <>4__this = this.<>4__this };
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            T IEnumerator<T>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ElementCount
        {
            internal int uniqueCount;
            internal int unfoundCount;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator, ISerializable, IDeserializationCallback
        {
            private SortedSet<T> tree;
            private int version;
            private Stack<SortedSet<T>.Node> stack;
            private SortedSet<T>.Node current;
            private static SortedSet<T>.Node dummyNode;
            private bool reverse;
            private SerializationInfo siInfo;
            internal Enumerator(SortedSet<T> set)
            {
                this.tree = set;
                this.tree.VersionCheck();
                this.version = this.tree.version;
                this.stack = new Stack<SortedSet<T>.Node>(2 * SortedSet<T>.log2(set.Count + 1));
                this.current = null;
                this.reverse = false;
                this.siInfo = null;
                this.Intialize();
            }

            internal Enumerator(SortedSet<T> set, bool reverse)
            {
                this.tree = set;
                this.tree.VersionCheck();
                this.version = this.tree.version;
                this.stack = new Stack<SortedSet<T>.Node>(2 * SortedSet<T>.log2(set.Count + 1));
                this.current = null;
                this.reverse = reverse;
                this.siInfo = null;
                this.Intialize();
            }

            private Enumerator(SerializationInfo info, StreamingContext context)
            {
                this.tree = null;
                this.version = -1;
                this.current = null;
                this.reverse = false;
                this.stack = null;
                this.siInfo = info;
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                this.GetObjectData(info, context);
            }

            private void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.info);
                }
                info.AddValue("Tree", this.tree, typeof(SortedSet<T>));
                info.AddValue("EnumVersion", this.version);
                info.AddValue("Reverse", this.reverse);
                info.AddValue("EnumStarted", !this.NotStartedOrEnded);
                info.AddValue("Item", (this.current == null) ? SortedSet<T>.Enumerator.dummyNode.Item : this.current.Item, typeof(T));
            }

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                this.OnDeserialization(sender);
            }

            private void OnDeserialization(object sender)
            {
                if (this.siInfo == null)
                {
                    System.ThrowHelper.ThrowSerializationException(System.ExceptionResource.Serialization_InvalidOnDeser);
                }
                this.tree = (SortedSet<T>) this.siInfo.GetValue("Tree", typeof(SortedSet<T>));
                this.version = this.siInfo.GetInt32("EnumVersion");
                this.reverse = this.siInfo.GetBoolean("Reverse");
                bool boolean = this.siInfo.GetBoolean("EnumStarted");
                this.stack = new Stack<SortedSet<T>.Node>(2 * SortedSet<T>.log2(this.tree.Count + 1));
                this.current = null;
                if (boolean)
                {
                    T y = (T) this.siInfo.GetValue("Item", typeof(T));
                    this.Intialize();
                    while (this.MoveNext())
                    {
                        if (this.tree.Comparer.Compare(this.Current, y) == 0)
                        {
                            return;
                        }
                    }
                }
            }

            private void Intialize()
            {
                this.current = null;
                SortedSet<T>.Node root = this.tree.root;
                SortedSet<T>.Node node2 = null;
                SortedSet<T>.Node node3 = null;
                while (root != null)
                {
                    node2 = this.reverse ? root.Right : root.Left;
                    node3 = this.reverse ? root.Left : root.Right;
                    if (this.tree.IsWithinRange(root.Item))
                    {
                        this.stack.Push(root);
                        root = node2;
                    }
                    else
                    {
                        if ((node2 == null) || !this.tree.IsWithinRange(node2.Item))
                        {
                            root = node3;
                            continue;
                        }
                        root = node2;
                    }
                }
            }

            public bool MoveNext()
            {
                this.tree.VersionCheck();
                if (this.version != this.tree.version)
                {
                    System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                if (this.stack.Count == 0)
                {
                    this.current = null;
                    return false;
                }
                this.current = this.stack.Pop();
                SortedSet<T>.Node item = this.reverse ? this.current.Left : this.current.Right;
                SortedSet<T>.Node node2 = null;
                SortedSet<T>.Node node3 = null;
                while (item != null)
                {
                    node2 = this.reverse ? item.Right : item.Left;
                    node3 = this.reverse ? item.Left : item.Right;
                    if (this.tree.IsWithinRange(item.Item))
                    {
                        this.stack.Push(item);
                        item = node2;
                    }
                    else
                    {
                        if ((node3 == null) || !this.tree.IsWithinRange(node3.Item))
                        {
                            item = node2;
                            continue;
                        }
                        item = node3;
                    }
                }
                return true;
            }

            public void Dispose()
            {
            }

            public T Current
            {
                get
                {
                    if (this.current != null)
                    {
                        return this.current.Item;
                    }
                    return default(T);
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    if (this.current == null)
                    {
                        System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return this.current.Item;
                }
            }
            internal bool NotStartedOrEnded
            {
                get
                {
                    return (this.current == null);
                }
            }
            internal void Reset()
            {
                if (this.version != this.tree.version)
                {
                    System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                this.stack.Clear();
                this.Intialize();
            }

            void IEnumerator.Reset()
            {
                this.Reset();
            }

            static Enumerator()
            {
                SortedSet<T>.Enumerator.dummyNode = new SortedSet<T>.Node(default(T));
            }
        }

        internal class Node
        {
            public bool IsRed;
            public T Item;
            public SortedSet<T>.Node Left;
            public SortedSet<T>.Node Right;

            public Node(T item)
            {
                this.Item = item;
                this.IsRed = true;
            }

            public Node(T item, bool isRed)
            {
                this.Item = item;
                this.IsRed = isRed;
            }
        }

        [Serializable]
        internal sealed class TreeSubSet : SortedSet<T>, ISerializable, IDeserializationCallback
        {
            private bool lBoundActive;
            private T max;
            private T min;
            private bool uBoundActive;
            private SortedSet<T> underlying;

            private TreeSubSet()
            {
                base.comparer = null;
            }

            private TreeSubSet(SerializationInfo info, StreamingContext context)
            {
                base.siInfo = info;
                this.OnDeserializationImpl(info);
            }

            public TreeSubSet(SortedSet<T> Underlying, T Min, T Max, bool lowerBoundActive, bool upperBoundActive) : base(Underlying.Comparer)
            {
                this.underlying = Underlying;
                this.min = Min;
                this.max = Max;
                this.lBoundActive = lowerBoundActive;
                this.uBoundActive = upperBoundActive;
                base.root = this.underlying.FindRange(this.min, this.max, this.lBoundActive, this.uBoundActive);
                base.count = 0;
                base.version = -1;
                this.VersionCheckImpl();
            }

            internal override bool AddIfNotPresent(T item)
            {
                if (!this.IsWithinRange(item))
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.collection);
                }
                bool flag = this.underlying.AddIfNotPresent(item);
                this.VersionCheck();
                return flag;
            }

            internal override bool BreadthFirstTreeWalk(TreeWalkPredicate<T> action)
            {
                this.VersionCheck();
                if (base.root != null)
                {
                    List<SortedSet<T>.Node> list = new List<SortedSet<T>.Node> {
                        base.root
                    };
                    while (list.Count != 0)
                    {
                        SortedSet<T>.Node node = list[0];
                        list.RemoveAt(0);
                        if (this.IsWithinRange(node.Item) && !action(node))
                        {
                            return false;
                        }
                        if ((node.Left != null) && (!this.lBoundActive || (base.Comparer.Compare(this.min, node.Item) < 0)))
                        {
                            list.Add(node.Left);
                        }
                        if ((node.Right != null) && (!this.uBoundActive || (base.Comparer.Compare(this.max, node.Item) > 0)))
                        {
                            list.Add(node.Right);
                        }
                    }
                }
                return true;
            }

            public override void Clear()
            {
                List<T> toRemove;
                if (base.count != 0)
                {
                    toRemove = new List<T>();
                    this.BreadthFirstTreeWalk(delegate (SortedSet<T>.Node n) {
                        toRemove.Add(n.Item);
                        return true;
                    });
                    while (toRemove.Count != 0)
                    {
                        this.underlying.Remove(toRemove[toRemove.Count - 1]);
                        toRemove.RemoveAt(toRemove.Count - 1);
                    }
                    base.root = null;
                    base.count = 0;
                    base.version = this.underlying.version;
                }
            }

            public override bool Contains(T item)
            {
                this.VersionCheck();
                return base.Contains(item);
            }

            internal override bool DoRemove(T item)
            {
                if (!this.IsWithinRange(item))
                {
                    return false;
                }
                bool flag = this.underlying.Remove(item);
                this.VersionCheck();
                return flag;
            }

            internal override SortedSet<T>.Node FindNode(T item)
            {
                if (!this.IsWithinRange(item))
                {
                    return null;
                }
                this.VersionCheck();
                return base.FindNode(item);
            }

            protected override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.info);
                }
                info.AddValue(SortedSet<T>.maxName, this.max, typeof(T));
                info.AddValue(SortedSet<T>.minName, this.min, typeof(T));
                info.AddValue(SortedSet<T>.lBoundActiveName, this.lBoundActive);
                info.AddValue(SortedSet<T>.uBoundActiveName, this.uBoundActive);
                base.GetObjectData(info, context);
            }

            public override SortedSet<T> GetViewBetween(T lowerValue, T upperValue)
            {
                if (this.lBoundActive && (base.Comparer.Compare(this.min, lowerValue) > 0))
                {
                    throw new ArgumentOutOfRangeException("lowerValue");
                }
                if (this.uBoundActive && (base.Comparer.Compare(this.max, upperValue) < 0))
                {
                    throw new ArgumentOutOfRangeException("upperValue");
                }
                return (SortedSet<T>.TreeSubSet) this.underlying.GetViewBetween(lowerValue, upperValue);
            }

            internal override bool InOrderTreeWalk(TreeWalkPredicate<T> action, bool reverse)
            {
                this.VersionCheck();
                if (base.root != null)
                {
                    Stack<SortedSet<T>.Node> stack = new Stack<SortedSet<T>.Node>(2 * SortedSet<T>.log2(base.count + 1));
                    SortedSet<T>.Node root = base.root;
                    while (root != null)
                    {
                        if (this.IsWithinRange(root.Item))
                        {
                            stack.Push(root);
                            root = reverse ? root.Right : root.Left;
                        }
                        else
                        {
                            if (this.lBoundActive && (base.Comparer.Compare(this.min, root.Item) > 0))
                            {
                                root = root.Right;
                                continue;
                            }
                            root = root.Left;
                        }
                    }
                    while (stack.Count != 0)
                    {
                        root = stack.Pop();
                        if (!action(root))
                        {
                            return false;
                        }
                        SortedSet<T>.Node item = reverse ? root.Left : root.Right;
                        while (item != null)
                        {
                            if (this.IsWithinRange(item.Item))
                            {
                                stack.Push(item);
                                item = reverse ? item.Right : item.Left;
                            }
                            else
                            {
                                if (this.lBoundActive && (base.Comparer.Compare(this.min, item.Item) > 0))
                                {
                                    item = item.Right;
                                    continue;
                                }
                                item = item.Left;
                            }
                        }
                    }
                }
                return true;
            }

            internal override int InternalIndexOf(T item)
            {
                int num = -1;
                foreach (T local in this)
                {
                    num++;
                    if (base.Comparer.Compare(item, local) == 0)
                    {
                        return num;
                    }
                }
                return -1;
            }

            internal override void IntersectWithEnumerable(IEnumerable<T> other)
            {
                List<T> collection = new List<T>(base.Count);
                foreach (T local in other)
                {
                    if (this.Contains(local))
                    {
                        collection.Add(local);
                        base.Remove(local);
                    }
                }
                this.Clear();
                base.AddAllElements(collection);
            }

            internal override bool IsWithinRange(T item)
            {
                int num = this.lBoundActive ? base.Comparer.Compare(this.min, item) : -1;
                if (num > 0)
                {
                    return false;
                }
                num = this.uBoundActive ? base.Comparer.Compare(this.max, item) : 1;
                if (num < 0)
                {
                    return false;
                }
                return true;
            }

            protected override void OnDeserialization(object sender)
            {
                this.OnDeserializationImpl(sender);
            }

            private void OnDeserializationImpl(object sender)
            {
                if (base.siInfo == null)
                {
                    System.ThrowHelper.ThrowSerializationException(System.ExceptionResource.Serialization_InvalidOnDeser);
                }
                base.comparer = (IComparer<T>) base.siInfo.GetValue("Comparer", typeof(IComparer<T>));
                int num = base.siInfo.GetInt32("Count");
                this.max = (T) base.siInfo.GetValue(SortedSet<T>.maxName, typeof(T));
                this.min = (T) base.siInfo.GetValue(SortedSet<T>.minName, typeof(T));
                this.lBoundActive = base.siInfo.GetBoolean(SortedSet<T>.lBoundActiveName);
                this.uBoundActive = base.siInfo.GetBoolean(SortedSet<T>.uBoundActiveName);
                this.underlying = new SortedSet<T>();
                if (num != 0)
                {
                    T[] localArray = (T[]) base.siInfo.GetValue("Items", typeof(T[]));
                    if (localArray == null)
                    {
                        System.ThrowHelper.ThrowSerializationException(System.ExceptionResource.Serialization_MissingValues);
                    }
                    for (int i = 0; i < localArray.Length; i++)
                    {
                        this.underlying.Add(localArray[i]);
                    }
                }
                this.underlying.version = base.siInfo.GetInt32("Version");
                base.count = this.underlying.count;
                base.version = this.underlying.version - 1;
                this.VersionCheck();
                if (base.count != num)
                {
                    System.ThrowHelper.ThrowSerializationException(System.ExceptionResource.Serialization_MismatchedCount);
                }
                base.siInfo = null;
            }

            void IDeserializationCallback.OnDeserialization(object sender)
            {
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                this.GetObjectData(info, context);
            }

            internal override void VersionCheck()
            {
                this.VersionCheckImpl();
            }

            private void VersionCheckImpl()
            {
                TreeWalkPredicate<T> action = null;
                if (base.version != this.underlying.version)
                {
                    base.root = this.underlying.FindRange(this.min, this.max, this.lBoundActive, this.uBoundActive);
                    base.version = this.underlying.version;
                    base.count = 0;
                    if (action == null)
                    {
                        action = delegate (SortedSet<T>.Node n) {
                            base.count++;
                            return true;
                        };
                    }
                    base.InOrderTreeWalk(action);
                }
            }
        }
    }
}

