namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections;
    using System.Xml;
    using System.Xml.XPath;

    internal class XPathMultyIterator : ResetableIterator
    {
        protected ResetableIterator[] arr;
        protected int firstNotEmpty;
        protected int position;

        public XPathMultyIterator(XPathMultyIterator it)
        {
            this.arr = (ResetableIterator[]) it.arr.Clone();
            this.firstNotEmpty = it.firstNotEmpty;
            this.position = it.position;
        }

        public XPathMultyIterator(ArrayList inputArray)
        {
            this.arr = new ResetableIterator[inputArray.Count];
            for (int i = 0; i < this.arr.Length; i++)
            {
                this.arr[i] = new XPathArrayIterator((ArrayList) inputArray[i]);
            }
            this.Init();
        }

        private bool Advance(int pos)
        {
            if (this.arr[pos].MoveNext())
            {
                return true;
            }
            if (this.firstNotEmpty != pos)
            {
                ResetableIterator iterator = this.arr[pos];
                Array.Copy(this.arr, this.firstNotEmpty, this.arr, this.firstNotEmpty + 1, pos - this.firstNotEmpty);
                this.arr[this.firstNotEmpty] = iterator;
            }
            this.firstNotEmpty++;
            return false;
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathMultyIterator(this);
        }

        private void Init()
        {
            for (int i = 0; i < this.arr.Length; i++)
            {
                this.Advance(i);
            }
            int item = this.arr.Length - 2;
            while (this.firstNotEmpty <= item)
            {
                if (this.SiftItem(item))
                {
                    item--;
                }
            }
        }

        public override bool MoveNext()
        {
            if (this.firstNotEmpty >= this.arr.Length)
            {
                return false;
            }
            if (this.position != 0)
            {
                if (this.Advance(this.firstNotEmpty))
                {
                    this.SiftItem(this.firstNotEmpty);
                }
                if (this.firstNotEmpty >= this.arr.Length)
                {
                    return false;
                }
            }
            this.position++;
            return true;
        }

        public override void Reset()
        {
            this.firstNotEmpty = 0;
            this.position = 0;
            for (int i = 0; i < this.arr.Length; i++)
            {
                this.arr[i].Reset();
            }
            this.Init();
        }

        private bool SiftItem(int item)
        {
            ResetableIterator iterator = this.arr[item];
            while ((item + 1) < this.arr.Length)
            {
                XmlNodeOrder order = Query.CompareNodes(iterator.Current, this.arr[item + 1].Current);
                if (order == XmlNodeOrder.Before)
                {
                    break;
                }
                if (order == XmlNodeOrder.After)
                {
                    this.arr[item] = this.arr[item + 1];
                    item++;
                }
                else
                {
                    this.arr[item] = iterator;
                    if (!this.Advance(item))
                    {
                        return false;
                    }
                    iterator = this.arr[item];
                }
            }
            this.arr[item] = iterator;
            return true;
        }

        public override XPathNavigator Current
        {
            get
            {
                return this.arr[this.firstNotEmpty].Current;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return this.position;
            }
        }
    }
}

