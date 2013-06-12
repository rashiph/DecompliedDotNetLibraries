namespace System.Web.UI
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Web.Compilation;
    using System.Web.Handlers;
    using System.Web.Util;

    [Serializable]
    internal class ScriptKey
    {
        private bool _isInclude;
        private bool _isResource;
        private string _key;
        [NonSerialized]
        private Type _type;
        private string _typeNameForSerialization;

        internal ScriptKey(Type type, string key) : this(type, key, false, false)
        {
        }

        internal ScriptKey(Type type, string key, bool isInclude, bool isResource)
        {
            this._type = type;
            if (string.IsNullOrEmpty(key))
            {
                key = null;
            }
            this._key = key;
            this._isInclude = isInclude;
            this._isResource = isResource;
        }

        public override bool Equals(object o)
        {
            ScriptKey key = (ScriptKey) o;
            return (((key._type == this._type) && (key._key == this._key)) && (key._isInclude == this._isInclude));
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes(this._type.GetHashCode(), this._key.GetHashCode(), this._isInclude.GetHashCode());
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            this._type = BuildManager.GetType(this._typeNameForSerialization, true, false);
        }

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            this._typeNameForSerialization = Util.GetAssemblyQualifiedTypeName(this._type);
        }

        public System.Reflection.Assembly Assembly
        {
            get
            {
                if (this._type != null)
                {
                    return AssemblyResourceLoader.GetAssemblyFromType(this._type);
                }
                return null;
            }
        }

        public bool IsResource
        {
            get
            {
                return this._isResource;
            }
        }

        public string Key
        {
            get
            {
                return this._key;
            }
        }
    }
}

