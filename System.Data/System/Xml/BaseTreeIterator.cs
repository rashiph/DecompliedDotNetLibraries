namespace System.Xml
{
    using System;

    internal abstract class BaseTreeIterator
    {
        protected DataSetMapper mapper;

        internal BaseTreeIterator(DataSetMapper mapper)
        {
            this.mapper = mapper;
        }

        internal abstract bool Next();
        internal abstract bool NextRight();
        internal bool NextRightRowElement()
        {
            if (!this.NextRight())
            {
                return false;
            }
            return (this.OnRowElement() || this.NextRowElement());
        }

        internal bool NextRowElement()
        {
            while (this.Next())
            {
                if (this.OnRowElement())
                {
                    return true;
                }
            }
            return false;
        }

        internal bool OnRowElement()
        {
            XmlBoundElement currentNode = this.CurrentNode as XmlBoundElement;
            return ((currentNode != null) && (currentNode.Row != null));
        }

        internal abstract void Reset();

        internal abstract XmlNode CurrentNode { get; }
    }
}

