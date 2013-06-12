namespace System.Xml.Serialization
{
    using System;

    public class UnreferencedObjectEventArgs : EventArgs
    {
        private string id;
        private object o;

        public UnreferencedObjectEventArgs(object o, string id)
        {
            this.o = o;
            this.id = id;
        }

        public string UnreferencedId
        {
            get
            {
                return this.id;
            }
        }

        public object UnreferencedObject
        {
            get
            {
                return this.o;
            }
        }
    }
}

