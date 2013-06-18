namespace System.Data.Design
{
    using System;

    internal class SimpleNamedObject : INamedObject
    {
        private object _obj;

        public SimpleNamedObject(object obj)
        {
            this._obj = obj;
        }

        public string Name
        {
            get
            {
                if (this._obj is INamedObject)
                {
                    return (this._obj as INamedObject).Name;
                }
                if (this._obj is string)
                {
                    return (this._obj as string);
                }
                return this._obj.ToString();
            }
            set
            {
                if (this._obj is INamedObject)
                {
                    (this._obj as INamedObject).Name = value;
                }
                else if (this._obj is string)
                {
                    this._obj = value;
                }
            }
        }
    }
}

