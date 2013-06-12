namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class SinkProviderData
    {
        private ArrayList _children = new ArrayList();
        private string _name;
        private Hashtable _properties = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        public SinkProviderData(string name)
        {
            this._name = name;
        }

        public IList Children
        {
            get
            {
                return this._children;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public IDictionary Properties
        {
            get
            {
                return this._properties;
            }
        }
    }
}

