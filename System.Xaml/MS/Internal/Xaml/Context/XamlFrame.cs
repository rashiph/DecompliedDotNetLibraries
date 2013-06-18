namespace MS.Internal.Xaml.Context
{
    using System;

    internal abstract class XamlFrame
    {
        private int _depth;
        private XamlFrame _previous;

        protected XamlFrame()
        {
            this._depth = -1;
        }

        protected XamlFrame(XamlFrame source)
        {
            this._depth = source._depth;
        }

        public virtual XamlFrame Clone()
        {
            throw new NotImplementedException();
        }

        public abstract void Reset();

        public int Depth
        {
            get
            {
                return this._depth;
            }
        }

        public XamlFrame Previous
        {
            get
            {
                return this._previous;
            }
            set
            {
                this._previous = value;
                this._depth = (this._previous == null) ? 0 : (this._previous._depth + 1);
            }
        }
    }
}

