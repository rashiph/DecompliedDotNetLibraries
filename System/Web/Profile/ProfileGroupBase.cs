namespace System.Web.Profile
{
    using System;
    using System.Reflection;

    public class ProfileGroupBase
    {
        private string _MyName = null;
        private ProfileBase _Parent = null;

        public object GetPropertyValue(string propertyName)
        {
            return this._Parent[this._MyName + propertyName];
        }

        public void Init(ProfileBase parent, string myName)
        {
            if (this._Parent == null)
            {
                this._Parent = parent;
                this._MyName = myName + ".";
            }
        }

        public void SetPropertyValue(string propertyName, object propertyValue)
        {
            this._Parent[this._MyName + propertyName] = propertyValue;
        }

        public object this[string propertyName]
        {
            get
            {
                return this._Parent[this._MyName + propertyName];
            }
            set
            {
                this._Parent[this._MyName + propertyName] = value;
            }
        }
    }
}

