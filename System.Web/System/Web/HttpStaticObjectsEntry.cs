namespace System.Web
{
    using System;

    internal class HttpStaticObjectsEntry
    {
        private object _instance;
        private bool _lateBound;
        private string _name;
        private Type _type;

        internal HttpStaticObjectsEntry(string name, object instance, int dummy)
        {
            this._name = name;
            this._type = instance.GetType();
            this._instance = instance;
        }

        internal HttpStaticObjectsEntry(string name, Type t, bool lateBound)
        {
            this._name = name;
            this._type = t;
            this._lateBound = lateBound;
            this._instance = null;
        }

        internal Type DeclaredType
        {
            get
            {
                if (!this._lateBound)
                {
                    return this.ObjectType;
                }
                return typeof(object);
            }
        }

        internal bool HasInstance
        {
            get
            {
                return (this._instance != null);
            }
        }

        internal object Instance
        {
            get
            {
                if (this._instance == null)
                {
                    lock (this)
                    {
                        if (this._instance == null)
                        {
                            this._instance = Activator.CreateInstance(this._type);
                        }
                    }
                }
                return this._instance;
            }
        }

        internal bool LateBound
        {
            get
            {
                return this._lateBound;
            }
        }

        internal string Name
        {
            get
            {
                return this._name;
            }
        }

        internal Type ObjectType
        {
            get
            {
                return this._type;
            }
        }
    }
}

