namespace System.Xml
{
    using System;

    internal class XmlElementListListener
    {
        private XmlDocument doc;
        private WeakReference elemList;
        private XmlNodeChangedEventHandler nodeChangeHandler;

        internal XmlElementListListener(XmlDocument doc, XmlElementList elemList)
        {
            this.doc = doc;
            this.elemList = new WeakReference(elemList);
            this.nodeChangeHandler = new XmlNodeChangedEventHandler(this.OnListChanged);
            doc.NodeInserted += this.nodeChangeHandler;
            doc.NodeRemoved += this.nodeChangeHandler;
        }

        private void OnListChanged(object sender, XmlNodeChangedEventArgs args)
        {
            lock (this)
            {
                if (this.elemList != null)
                {
                    XmlElementList target = (XmlElementList) this.elemList.Target;
                    if (target != null)
                    {
                        target.ConcurrencyCheck(args);
                    }
                    else
                    {
                        this.doc.NodeInserted -= this.nodeChangeHandler;
                        this.doc.NodeRemoved -= this.nodeChangeHandler;
                        this.elemList = null;
                    }
                }
            }
        }

        internal void Unregister()
        {
            lock (this)
            {
                if (this.elemList != null)
                {
                    this.doc.NodeInserted -= this.nodeChangeHandler;
                    this.doc.NodeRemoved -= this.nodeChangeHandler;
                    this.elemList = null;
                }
            }
        }
    }
}

