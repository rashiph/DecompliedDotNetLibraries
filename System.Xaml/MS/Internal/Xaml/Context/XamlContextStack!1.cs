namespace MS.Internal.Xaml.Context
{
    using System;
    using System.Globalization;
    using System.Text;

    internal class XamlContextStack<T> where T: XamlFrame
    {
        private Func<T> _creationDelegate;
        private T _currentFrame;
        private int _depth;
        private T _recycledFrame;

        public XamlContextStack(Func<T> creationDelegate)
        {
            this._depth = -1;
            this._currentFrame = default(T);
            this._recycledFrame = default(T);
            this._creationDelegate = creationDelegate;
            this.Grow();
            this._depth = 0;
        }

        public XamlContextStack(XamlContextStack<T> source, bool copy)
        {
            this._depth = -1;
            this._currentFrame = default(T);
            this._recycledFrame = default(T);
            this._creationDelegate = source._creationDelegate;
            this._depth = source.Depth;
            if (!copy)
            {
                this._currentFrame = source.CurrentFrame;
            }
            else
            {
                T currentFrame = source.CurrentFrame;
                T local2 = default(T);
                while (currentFrame != null)
                {
                    T local3 = (T) currentFrame.Clone();
                    if (this._currentFrame == null)
                    {
                        this._currentFrame = local3;
                    }
                    if (local2 != null)
                    {
                        local2.Previous = local3;
                    }
                    local2 = local3;
                    currentFrame = (T) currentFrame.Previous;
                }
            }
        }

        public T GetFrame(int depth)
        {
            T previous = this._currentFrame;
            while (previous.Depth > depth)
            {
                previous = (T) previous.Previous;
            }
            return previous;
        }

        private void Grow()
        {
            T local = this._currentFrame;
            this._currentFrame = this._creationDelegate();
            this._currentFrame.Previous = local;
        }

        public void PopScope()
        {
            this._depth--;
            T local = this._currentFrame;
            this._currentFrame = (T) this._currentFrame.Previous;
            local.Previous = this._recycledFrame;
            this._recycledFrame = local;
            local.Reset();
        }

        public void PushScope()
        {
            if (this._recycledFrame == null)
            {
                this.Grow();
            }
            else
            {
                T local = this._currentFrame;
                this._currentFrame = this._recycledFrame;
                this._recycledFrame = (T) this._recycledFrame.Previous;
                this._currentFrame.Previous = local;
            }
            this._depth++;
        }

        private void ShowFrame(StringBuilder sb, T iteratorFrame)
        {
            if (iteratorFrame != null)
            {
                if (iteratorFrame.Previous != null)
                {
                    this.ShowFrame(sb, (T) iteratorFrame.Previous);
                }
                sb.AppendLine(string.Concat(new object[] { "  ", iteratorFrame.Depth, " ", iteratorFrame.ToString() }));
            }
        }

        public void Trim()
        {
            this._recycledFrame = default(T);
        }

        public T CurrentFrame
        {
            get
            {
                return this._currentFrame;
            }
        }

        public int Depth
        {
            get
            {
                return this._depth;
            }
            set
            {
                this._depth = value;
            }
        }

        public string Frames
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Stack: " + ((this._currentFrame == null) ? -1 : (this._currentFrame.Depth + 1)).ToString(CultureInfo.InvariantCulture) + " frames");
                this.ShowFrame(sb, this._currentFrame);
                return sb.ToString();
            }
        }

        public T PreviousFrame
        {
            get
            {
                return (T) this._currentFrame.Previous;
            }
        }

        public T PreviousPreviousFrame
        {
            get
            {
                return (T) this._currentFrame.Previous.Previous;
            }
        }
    }
}

