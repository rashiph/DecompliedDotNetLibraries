namespace System.Activities.Hosting
{
    using System;
    using System.Activities;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public sealed class SymbolResolver : IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
    {
        private Dictionary<string, ExternalLocationReference> symbols = new Dictionary<string, ExternalLocationReference>();

        public void Add(KeyValuePair<string, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(string key, object value)
        {
            this.symbols.Add(key, this.CreateReference(key, value));
        }

        public void Add(string key, Type type)
        {
            if (type == null)
            {
                throw FxTrace.Exception.ArgumentNull("type");
            }
            this.symbols.Add(key, new ExternalLocationReference(key, type, TypeHelper.GetDefaultValueForType(type)));
        }

        public void Add(string key, object value, Type type)
        {
            if (type == null)
            {
                throw FxTrace.Exception.ArgumentNull("type");
            }
            if (!TypeHelper.AreTypesCompatible(value, type))
            {
                throw FxTrace.Exception.Argument("value", System.Activities.SR.ValueMustBeAssignableToType);
            }
            this.symbols.Add(key, new ExternalLocationReference(key, type, value));
        }

        public LocationReferenceEnvironment AsLocationReferenceEnvironment()
        {
            return new SymbolResolverLocationReferenceEnvironment(this);
        }

        public void Clear()
        {
            this.symbols.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            ExternalLocationReference reference;
            return (this.symbols.TryGetValue(item.Key, out reference) && (item.Value == reference.Value));
        }

        public bool ContainsKey(string key)
        {
            return this.symbols.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw FxTrace.Exception.ArgumentNull("array");
            }
            if (arrayIndex < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("arrayIndex", arrayIndex, System.Activities.SR.CopyToIndexOutOfRange);
            }
            if (array.Rank > 1)
            {
                throw FxTrace.Exception.Argument("array", System.Activities.SR.CopyToRankMustBeOne);
            }
            if (this.symbols.Count > (array.Length - arrayIndex))
            {
                throw FxTrace.Exception.Argument("array", System.Activities.SR.CopyToNotEnoughSpaceInArray);
            }
            foreach (KeyValuePair<string, ExternalLocationReference> pair in this.symbols)
            {
                array[arrayIndex] = new KeyValuePair<string, object>(pair.Key, pair.Value.Value);
                arrayIndex++;
            }
        }

        private ExternalLocationReference CreateReference(string name, object value)
        {
            Type objectType = TypeHelper.ObjectType;
            if (value != null)
            {
                objectType = value.GetType();
            }
            return new ExternalLocationReference(name, objectType, value);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (KeyValuePair<string, ExternalLocationReference> iteratorVariable0 in this.symbols)
            {
                yield return new KeyValuePair<string, object>(iteratorVariable0.Key, iteratorVariable0.Value.Value);
            }
        }

        private System.Activities.Location GetLocation(string name, Type type)
        {
            ExternalLocationReference reference;
            if (!this.symbols.TryGetValue(name, out reference) || (reference.Type != type))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.SymbolResolverDoesNotHaveSymbol(name, type)));
            }
            return reference.Location;
        }

        internal IEnumerable<KeyValuePair<string, LocationReference>> GetLocationReferenceEnumerator()
        {
            foreach (KeyValuePair<string, ExternalLocationReference> iteratorVariable0 in this.symbols)
            {
                yield return new KeyValuePair<string, LocationReference>(iteratorVariable0.Key, iteratorVariable0.Value);
            }
        }

        internal bool IsVisible(LocationReference locationReference)
        {
            ExternalLocationReference reference;
            if (locationReference.Name == null)
            {
                return false;
            }
            return (this.symbols.TryGetValue(locationReference.Name, out reference) && (reference.Type == locationReference.Type));
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            ExternalLocationReference reference;
            if (this.symbols.TryGetValue(item.Key, out reference) && (reference.Value == item.Value))
            {
                this.symbols.Remove(item.Key);
                return true;
            }
            return false;
        }

        public bool Remove(string key)
        {
            return this.symbols.Remove(key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal bool TryGetLocationReference(string name, out LocationReference result)
        {
            ExternalLocationReference reference;
            if (this.symbols.TryGetValue(name, out reference))
            {
                result = reference;
                return true;
            }
            result = null;
            return false;
        }

        public bool TryGetValue(string key, out object value)
        {
            ExternalLocationReference reference;
            if (this.symbols.TryGetValue(key, out reference))
            {
                value = reference.Value;
                return true;
            }
            value = null;
            return false;
        }

        public int Count
        {
            get
            {
                return this.symbols.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public object this[string key]
        {
            get
            {
                return this.symbols[key].Value;
            }
            set
            {
                this.symbols[key] = this.CreateReference(key, value);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return this.symbols.Keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                List<object> list = new List<object>(this.symbols.Count);
                foreach (ExternalLocationReference reference in this.symbols.Values)
                {
                    list.Add(reference.Value);
                }
                return list;
            }
        }



        private class ExternalLocationReference : LocationReference
        {
            private ExternalLocation location;
            private string name;
            private Type type;

            public ExternalLocationReference(string name, Type type, object value)
            {
                this.name = name;
                this.type = type;
                this.location = new ExternalLocation(this.type, value);
            }

            public override System.Activities.Location GetLocation(ActivityContext context)
            {
                SymbolResolver extension = context.GetExtension<SymbolResolver>();
                if (extension == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CanNotFindSymbolResolverInWorkflowInstanceExtensions));
                }
                return extension.GetLocation(base.Name, base.Type);
            }

            public System.Activities.Location Location
            {
                get
                {
                    return this.location;
                }
            }

            protected override string NameCore
            {
                get
                {
                    return this.name;
                }
            }

            protected override Type TypeCore
            {
                get
                {
                    return this.type;
                }
            }

            public object Value
            {
                get
                {
                    return this.location.Value;
                }
            }

            private class ExternalLocation : System.Activities.Location
            {
                private Type type;
                private object value;

                public ExternalLocation(Type type, object value)
                {
                    this.type = type;
                    this.value = value;
                }

                public override Type LocationType
                {
                    get
                    {
                        return this.type;
                    }
                }

                protected override object ValueCore
                {
                    get
                    {
                        return this.value;
                    }
                    set
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ExternalLocationsGetOnly));
                    }
                }
            }
        }

        private class SymbolResolverLocationReferenceEnvironment : LocationReferenceEnvironment
        {
            private SymbolResolver symbolResolver;

            public SymbolResolverLocationReferenceEnvironment(SymbolResolver symbolResolver)
            {
                this.symbolResolver = symbolResolver;
            }

            public override IEnumerable<LocationReference> GetLocationReferences()
            {
                List<LocationReference> list = new List<LocationReference>();
                foreach (SymbolResolver.ExternalLocationReference reference in this.symbolResolver.symbols.Values)
                {
                    list.Add(reference);
                }
                return list;
            }

            public override bool IsVisible(LocationReference locationReference)
            {
                if (locationReference == null)
                {
                    throw FxTrace.Exception.ArgumentNull("locationReference");
                }
                return this.symbolResolver.IsVisible(locationReference);
            }

            public override bool TryGetLocationReference(string name, out LocationReference result)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("name");
                }
                return this.symbolResolver.TryGetLocationReference(name, out result);
            }

            public override Activity Root
            {
                get
                {
                    return null;
                }
            }
        }
    }
}

