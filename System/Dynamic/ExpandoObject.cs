namespace System.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public sealed class ExpandoObject : IDynamicMetaObjectProvider, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, INotifyPropertyChanged
    {
        private int _count;
        private ExpandoData _data = ExpandoData.Empty;
        internal const int AmbiguousMatchFound = -2;
        internal readonly object LockObject = new object();
        internal const int NoMatch = -1;
        internal static readonly object Uninitialized = new object();

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged;

        private bool ExpandoContainsKey(string key)
        {
            return (this._data.Class.GetValueIndexCaseSensitive(key) >= 0);
        }

        private IEnumerator<KeyValuePair<string, object>> GetExpandoEnumerator(ExpandoData data, int version)
        {
            for (int i = 0; i < data.Class.Keys.Length; i++)
            {
                if ((this._data.Version != version) || (data != this._data))
                {
                    throw Error.CollectionModifiedWhileEnumerating();
                }
                object iteratorVariable1 = data[i];
                if (iteratorVariable1 != Uninitialized)
                {
                    yield return new KeyValuePair<string, object>(data.Class.Keys[i], iteratorVariable1);
                }
            }
        }

        internal bool IsDeletedMember(int index)
        {
            if (index == this._data.Length)
            {
                return false;
            }
            return (this._data[index] == Uninitialized);
        }

        internal void PromoteClass(object oldClass, object newClass)
        {
            this.PromoteClassCore((ExpandoClass) oldClass, (ExpandoClass) newClass);
        }

        private ExpandoData PromoteClassCore(ExpandoClass oldClass, ExpandoClass newClass)
        {
            lock (this.LockObject)
            {
                if (this._data.Class == oldClass)
                {
                    this._data = this._data.UpdateClass(newClass);
                }
                return this._data;
            }
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            this.TryAddMember(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            ExpandoData data;
            lock (this.LockObject)
            {
                data = this._data;
                this._data = ExpandoData.Empty;
                this._count = 0;
            }
            PropertyChangedEventHandler handler = this._propertyChanged;
            if (handler != null)
            {
                int index = 0;
                int length = data.Class.Keys.Length;
                while (index < length)
                {
                    if (data[index] != Uninitialized)
                    {
                        handler(this, new PropertyChangedEventArgs(data.Class.Keys[index]));
                    }
                    index++;
                }
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            object obj2;
            if (!this.TryGetValueForKey(item.Key, out obj2))
            {
                return false;
            }
            return object.Equals(obj2, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresArrayRange<KeyValuePair<string, object>>(array, arrayIndex, this._count, "arrayIndex", "Count");
            lock (this.LockObject)
            {
                foreach (KeyValuePair<string, object> pair in (IEnumerable<KeyValuePair<string, object>>) this)
                {
                    array[arrayIndex++] = pair;
                }
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return this.TryDeleteValue(null, -1, item.Key, false, item.Value);
        }

        void IDictionary<string, object>.Add(string key, object value)
        {
            this.TryAddMember(key, value);
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            ContractUtils.RequiresNotNull(key, "key");
            ExpandoData data = this._data;
            int valueIndexCaseSensitive = data.Class.GetValueIndexCaseSensitive(key);
            return ((valueIndexCaseSensitive >= 0) && (data[valueIndexCaseSensitive] != Uninitialized));
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            ContractUtils.RequiresNotNull(key, "key");
            return this.TryDeleteValue(null, -1, key, false, Uninitialized);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            return this.TryGetValueForKey(key, out value);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            ExpandoData data = this._data;
            return this.GetExpandoEnumerator(data, data.Version);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            ExpandoData data = this._data;
            return this.GetExpandoEnumerator(data, data.Version);
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new MetaExpando(parameter, this);
        }

        private void TryAddMember(string key, object value)
        {
            ContractUtils.RequiresNotNull(key, "key");
            this.TrySetValue(null, -1, value, key, false, true);
        }

        internal bool TryDeleteValue(object indexClass, int index, string name, bool ignoreCase, object deleteValue)
        {
            ExpandoData data;
            lock (this.LockObject)
            {
                data = this._data;
                if ((data.Class != indexClass) || ignoreCase)
                {
                    index = data.Class.GetValueIndex(name, ignoreCase, this);
                    if (index == -2)
                    {
                        throw Error.AmbiguousMatchInExpandoObject(name);
                    }
                }
                if (index == -1)
                {
                    return false;
                }
                object objA = data[index];
                if (objA == Uninitialized)
                {
                    return false;
                }
                if ((deleteValue != Uninitialized) && !object.Equals(objA, deleteValue))
                {
                    return false;
                }
                data[index] = Uninitialized;
                this._count--;
            }
            PropertyChangedEventHandler handler = this._propertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(data.Class.Keys[index]));
            }
            return true;
        }

        internal bool TryGetValue(object indexClass, int index, string name, bool ignoreCase, out object value)
        {
            ExpandoData data = this._data;
            if ((data.Class != indexClass) || ignoreCase)
            {
                index = data.Class.GetValueIndex(name, ignoreCase, this);
                if (index == -2)
                {
                    throw Error.AmbiguousMatchInExpandoObject(name);
                }
            }
            if (index == -1)
            {
                value = null;
                return false;
            }
            object obj2 = data[index];
            if (obj2 == Uninitialized)
            {
                value = null;
                return false;
            }
            value = obj2;
            return true;
        }

        private bool TryGetValueForKey(string key, out object value)
        {
            return this.TryGetValue(null, -1, key, false, out value);
        }

        internal void TrySetValue(object indexClass, int index, object value, string name, bool ignoreCase, bool add)
        {
            ExpandoData data;
            object obj2;
            lock (this.LockObject)
            {
                data = this._data;
                if ((data.Class != indexClass) || ignoreCase)
                {
                    index = data.Class.GetValueIndex(name, ignoreCase, this);
                    if (index == -2)
                    {
                        throw Error.AmbiguousMatchInExpandoObject(name);
                    }
                    if (index == -1)
                    {
                        int num = ignoreCase ? data.Class.GetValueIndexCaseSensitive(name) : index;
                        if (num != -1)
                        {
                            index = num;
                        }
                        else
                        {
                            ExpandoClass newClass = data.Class.FindNewClass(name);
                            data = this.PromoteClassCore(data.Class, newClass);
                            index = data.Class.GetValueIndexCaseSensitive(name);
                        }
                    }
                }
                obj2 = data[index];
                if (obj2 == Uninitialized)
                {
                    this._count++;
                }
                else if (add)
                {
                    throw Error.SameKeyExistsInExpando(name);
                }
                data[index] = value;
            }
            PropertyChangedEventHandler handler = this._propertyChanged;
            if ((handler != null) && (value != obj2))
            {
                handler(this, new PropertyChangedEventArgs(data.Class.Keys[index]));
            }
        }

        internal ExpandoClass Class
        {
            get
            {
                return this._data.Class;
            }
        }

        int ICollection<KeyValuePair<string, object>>.Count
        {
            get
            {
                return this._count;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IDictionary<string, object>.this[string key]
        {
            get
            {
                object obj2;
                if (!this.TryGetValueForKey(key, out obj2))
                {
                    throw Error.KeyDoesNotExistInExpando(key);
                }
                return obj2;
            }
            set
            {
                ContractUtils.RequiresNotNull(key, "key");
                this.TrySetValue(null, -1, value, key, false, false);
            }
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                return new KeyCollection(this);
            }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                return new ValueCollection(this);
            }
        }


        private class ExpandoData
        {
            private readonly object[] _dataArray;
            private int _version;
            internal readonly ExpandoClass Class;
            internal static ExpandoObject.ExpandoData Empty = new ExpandoObject.ExpandoData();

            private ExpandoData()
            {
                this.Class = ExpandoClass.Empty;
                this._dataArray = new object[0];
            }

            internal ExpandoData(ExpandoClass klass, object[] data, int version)
            {
                this.Class = klass;
                this._dataArray = data;
                this._version = version;
            }

            private static int GetAlignedSize(int len)
            {
                return ((len + 7) & -8);
            }

            internal ExpandoObject.ExpandoData UpdateClass(ExpandoClass newClass)
            {
                if (this._dataArray.Length >= newClass.Keys.Length)
                {
                    this[newClass.Keys.Length - 1] = ExpandoObject.Uninitialized;
                    return new ExpandoObject.ExpandoData(newClass, this._dataArray, this._version);
                }
                int length = this._dataArray.Length;
                object[] destinationArray = new object[GetAlignedSize(newClass.Keys.Length)];
                Array.Copy(this._dataArray, destinationArray, this._dataArray.Length);
                ExpandoObject.ExpandoData data = new ExpandoObject.ExpandoData(newClass, destinationArray, this._version);
                data[length] = ExpandoObject.Uninitialized;
                return data;
            }

            internal object this[int index]
            {
                get
                {
                    return this._dataArray[index];
                }
                set
                {
                    this._version++;
                    this._dataArray[index] = value;
                }
            }

            internal int Length
            {
                get
                {
                    return this._dataArray.Length;
                }
            }

            internal int Version
            {
                get
                {
                    return this._version;
                }
            }
        }

        [DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ExpandoObject.KeyCollectionDebugView))]
        private class KeyCollection : ICollection<string>, IEnumerable<string>, IEnumerable
        {
            private readonly ExpandoObject _expando;
            private readonly int _expandoCount;
            private readonly ExpandoObject.ExpandoData _expandoData;
            private readonly int _expandoVersion;

            internal KeyCollection(ExpandoObject expando)
            {
                lock (expando.LockObject)
                {
                    this._expando = expando;
                    this._expandoVersion = expando._data.Version;
                    this._expandoCount = expando._count;
                    this._expandoData = expando._data;
                }
            }

            public void Add(string item)
            {
                throw Error.CollectionReadOnly();
            }

            private void CheckVersion()
            {
                if ((this._expando._data.Version != this._expandoVersion) || (this._expandoData != this._expando._data))
                {
                    throw Error.CollectionModifiedWhileEnumerating();
                }
            }

            public void Clear()
            {
                throw Error.CollectionReadOnly();
            }

            public bool Contains(string item)
            {
                lock (this._expando.LockObject)
                {
                    this.CheckVersion();
                    return this._expando.ExpandoContainsKey(item);
                }
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                ContractUtils.RequiresNotNull(array, "array");
                ContractUtils.RequiresArrayRange<string>(array, arrayIndex, this._expandoCount, "arrayIndex", "Count");
                lock (this._expando.LockObject)
                {
                    this.CheckVersion();
                    ExpandoObject.ExpandoData data = this._expando._data;
                    for (int i = 0; i < data.Class.Keys.Length; i++)
                    {
                        if (data[i] != ExpandoObject.Uninitialized)
                        {
                            array[arrayIndex++] = data.Class.Keys[i];
                        }
                    }
                }
            }

            public IEnumerator<string> GetEnumerator()
            {
                int index = 0;
                int length = this._expandoData.Class.Keys.Length;
                while (index < length)
                {
                    this.CheckVersion();
                    if (this._expandoData[index] != ExpandoObject.Uninitialized)
                    {
                        yield return this._expandoData.Class.Keys[index];
                    }
                    index++;
                }
            }

            public bool Remove(string item)
            {
                throw Error.CollectionReadOnly();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    this.CheckVersion();
                    return this._expandoCount;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

        }

        private sealed class KeyCollectionDebugView
        {
            private ICollection<string> collection;

            public KeyCollectionDebugView(ICollection<string> collection)
            {
                this.collection = collection;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public string[] Items
            {
                get
                {
                    string[] array = new string[this.collection.Count];
                    this.collection.CopyTo(array, 0);
                    return array;
                }
            }
        }

        private class MetaExpando : DynamicMetaObject
        {
            public MetaExpando(Expression expression, ExpandoObject value) : base(expression, BindingRestrictions.Empty, value)
            {
            }

            private DynamicMetaObject AddDynamicTestAndDefer(DynamicMetaObjectBinder binder, ExpandoClass klass, ExpandoClass originalClass, DynamicMetaObject succeeds)
            {
                Expression ifTrue = succeeds.Expression;
                if (originalClass != null)
                {
                    ifTrue = Expression.Block(Expression.Call(null, typeof(RuntimeOps).GetMethod("ExpandoPromoteClass"), this.GetLimitedSelf(), Expression.Constant(originalClass, typeof(object)), Expression.Constant(klass, typeof(object))), succeeds.Expression);
                }
                return new DynamicMetaObject(Expression.Condition(Expression.Call(null, typeof(RuntimeOps).GetMethod("ExpandoCheckVersion"), this.GetLimitedSelf(), Expression.Constant(originalClass ?? klass, typeof(object))), ifTrue, binder.GetUpdateExpression(ifTrue.Type)), this.GetRestrictions().Merge(succeeds.Restrictions));
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
            {
                ContractUtils.RequiresNotNull(binder, "binder");
                int num = this.Value.Class.GetValueIndex(binder.Name, binder.IgnoreCase, this.Value);
                Expression expression = Expression.Call(typeof(RuntimeOps).GetMethod("ExpandoTryDeleteValue"), this.GetLimitedSelf(), Expression.Constant(this.Value.Class, typeof(object)), Expression.Constant(num), Expression.Constant(binder.Name), Expression.Constant(binder.IgnoreCase));
                DynamicMetaObject obj2 = binder.FallbackDeleteMember(this);
                DynamicMetaObject succeeds = new DynamicMetaObject(Expression.IfThen(Expression.Not(expression), obj2.Expression), obj2.Restrictions);
                return this.AddDynamicTestAndDefer(binder, this.Value.Class, null, succeeds);
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                ContractUtils.RequiresNotNull(binder, "binder");
                return this.BindGetOrInvokeMember(binder, binder.Name, binder.IgnoreCase, binder.FallbackGetMember(this), null);
            }

            private DynamicMetaObject BindGetOrInvokeMember(DynamicMetaObjectBinder binder, string name, bool ignoreCase, DynamicMetaObject fallback, Func<DynamicMetaObject, DynamicMetaObject> fallbackInvoke)
            {
                ParameterExpression expression;
                ExpandoClass class2 = this.Value.Class;
                int num = class2.GetValueIndex(name, ignoreCase, this.Value);
                Expression test = Expression.Call(typeof(RuntimeOps).GetMethod("ExpandoTryGetValue"), new Expression[] { this.GetLimitedSelf(), Expression.Constant(class2, typeof(object)), Expression.Constant(num), Expression.Constant(name), Expression.Constant(ignoreCase), expression = Expression.Parameter(typeof(object), "value") });
                DynamicMetaObject arg = new DynamicMetaObject(expression, BindingRestrictions.Empty);
                if (fallbackInvoke != null)
                {
                    arg = fallbackInvoke(arg);
                }
                arg = new DynamicMetaObject(Expression.Block(new ParameterExpression[] { expression }, new Expression[] { Expression.Condition(test, arg.Expression, fallback.Expression, typeof(object)) }), arg.Restrictions.Merge(fallback.Restrictions));
                return this.AddDynamicTestAndDefer(binder, this.Value.Class, null, arg);
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                ContractUtils.RequiresNotNull(binder, "binder");
                return this.BindGetOrInvokeMember(binder, binder.Name, binder.IgnoreCase, binder.FallbackInvokeMember(this, args), value => binder.FallbackInvoke(value, args, null));
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                ExpandoClass class2;
                int num;
                ContractUtils.RequiresNotNull(binder, "binder");
                ContractUtils.RequiresNotNull(value, "value");
                ExpandoClass originalClass = this.GetClassEnsureIndex(binder.Name, binder.IgnoreCase, this.Value, out class2, out num);
                return this.AddDynamicTestAndDefer(binder, class2, originalClass, new DynamicMetaObject(Expression.Call(typeof(RuntimeOps).GetMethod("ExpandoTrySetValue"), new Expression[] { this.GetLimitedSelf(), Expression.Constant(class2, typeof(object)), Expression.Constant(num), Expression.Convert(value.Expression, typeof(object)), Expression.Constant(binder.Name), Expression.Constant(binder.IgnoreCase) }), BindingRestrictions.Empty));
            }

            private ExpandoClass GetClassEnsureIndex(string name, bool caseInsensitive, ExpandoObject obj, out ExpandoClass klass, out int index)
            {
                ExpandoClass class2 = this.Value.Class;
                index = class2.GetValueIndex(name, caseInsensitive, obj);
                if (index == -2)
                {
                    klass = class2;
                    return null;
                }
                if (index == -1)
                {
                    ExpandoClass class3 = class2.FindNewClass(name);
                    klass = class3;
                    index = class3.GetValueIndexCaseSensitive(name);
                    return class2;
                }
                klass = class2;
                return null;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                ExpandoObject.ExpandoData iteratorVariable0 = this.Value._data;
                ExpandoClass iteratorVariable1 = iteratorVariable0.Class;
                for (int i = 0; i < iteratorVariable1.Keys.Length; i++)
                {
                    object iteratorVariable3 = iteratorVariable0[i];
                    if (iteratorVariable3 != ExpandoObject.Uninitialized)
                    {
                        yield return iteratorVariable1.Keys[i];
                    }
                }
            }

            private Expression GetLimitedSelf()
            {
                if (TypeUtils.AreEquivalent(base.Expression.Type, base.LimitType))
                {
                    return base.Expression;
                }
                return Expression.Convert(base.Expression, base.LimitType);
            }

            private BindingRestrictions GetRestrictions()
            {
                return BindingRestrictions.GetTypeRestriction(this);
            }

            public ExpandoObject Value
            {
                get
                {
                    return (ExpandoObject) base.Value;
                }
            }

        }

        [DebuggerTypeProxy(typeof(ExpandoObject.ValueCollectionDebugView)), DebuggerDisplay("Count = {Count}")]
        private class ValueCollection : ICollection<object>, IEnumerable<object>, IEnumerable
        {
            private readonly ExpandoObject _expando;
            private readonly int _expandoCount;
            private readonly ExpandoObject.ExpandoData _expandoData;
            private readonly int _expandoVersion;

            internal ValueCollection(ExpandoObject expando)
            {
                lock (expando.LockObject)
                {
                    this._expando = expando;
                    this._expandoVersion = expando._data.Version;
                    this._expandoCount = expando._count;
                    this._expandoData = expando._data;
                }
            }

            public void Add(object item)
            {
                throw Error.CollectionReadOnly();
            }

            private void CheckVersion()
            {
                if ((this._expando._data.Version != this._expandoVersion) || (this._expandoData != this._expando._data))
                {
                    throw Error.CollectionModifiedWhileEnumerating();
                }
            }

            public void Clear()
            {
                throw Error.CollectionReadOnly();
            }

            public bool Contains(object item)
            {
                lock (this._expando.LockObject)
                {
                    this.CheckVersion();
                    ExpandoObject.ExpandoData data = this._expando._data;
                    for (int i = 0; i < data.Class.Keys.Length; i++)
                    {
                        if (object.Equals(data[i], item))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            public void CopyTo(object[] array, int arrayIndex)
            {
                ContractUtils.RequiresNotNull(array, "array");
                ContractUtils.RequiresArrayRange<object>(array, arrayIndex, this._expandoCount, "arrayIndex", "Count");
                lock (this._expando.LockObject)
                {
                    this.CheckVersion();
                    ExpandoObject.ExpandoData data = this._expando._data;
                    for (int i = 0; i < data.Class.Keys.Length; i++)
                    {
                        if (data[i] != ExpandoObject.Uninitialized)
                        {
                            array[arrayIndex++] = data[i];
                        }
                    }
                }
            }

            public IEnumerator<object> GetEnumerator()
            {
                ExpandoObject.ExpandoData iteratorVariable0 = this._expando._data;
                for (int i = 0; i < iteratorVariable0.Class.Keys.Length; i++)
                {
                    this.CheckVersion();
                    object iteratorVariable2 = iteratorVariable0[i];
                    if (iteratorVariable2 != ExpandoObject.Uninitialized)
                    {
                        yield return iteratorVariable2;
                    }
                }
            }

            public bool Remove(object item)
            {
                throw Error.CollectionReadOnly();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    this.CheckVersion();
                    return this._expandoCount;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

        }

        private sealed class ValueCollectionDebugView
        {
            private ICollection<object> collection;

            public ValueCollectionDebugView(ICollection<object> collection)
            {
                this.collection = collection;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object[] Items
            {
                get
                {
                    object[] array = new object[this.collection.Count];
                    this.collection.CopyTo(array, 0);
                    return array;
                }
            }
        }
    }
}

