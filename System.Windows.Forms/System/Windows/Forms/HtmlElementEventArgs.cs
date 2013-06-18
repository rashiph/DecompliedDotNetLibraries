namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    public sealed class HtmlElementEventArgs : EventArgs
    {
        private System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj htmlEventObj;
        private HtmlShimManager shimManager;

        internal HtmlElementEventArgs(HtmlShimManager shimManager, System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj eventObj)
        {
            this.htmlEventObj = eventObj;
            this.shimManager = shimManager;
        }

        public bool AltKeyPressed
        {
            get
            {
                return this.NativeHTMLEventObj.GetAltKey();
            }
        }

        public bool BubbleEvent
        {
            get
            {
                return !this.NativeHTMLEventObj.GetCancelBubble();
            }
            set
            {
                this.NativeHTMLEventObj.SetCancelBubble(!value);
            }
        }

        public Point ClientMousePosition
        {
            get
            {
                return new Point(this.NativeHTMLEventObj.GetClientX(), this.NativeHTMLEventObj.GetClientY());
            }
        }

        public bool CtrlKeyPressed
        {
            get
            {
                return this.NativeHTMLEventObj.GetCtrlKey();
            }
        }

        public string EventType
        {
            get
            {
                return this.NativeHTMLEventObj.GetEventType();
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public HtmlElement FromElement
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement fromElement = this.NativeHTMLEventObj.GetFromElement();
                if (fromElement != null)
                {
                    return new HtmlElement(this.shimManager, fromElement);
                }
                return null;
            }
        }

        public int KeyPressedCode
        {
            get
            {
                return this.NativeHTMLEventObj.GetKeyCode();
            }
        }

        public MouseButtons MouseButtonsPressed
        {
            get
            {
                MouseButtons none = MouseButtons.None;
                int button = this.NativeHTMLEventObj.GetButton();
                if ((button & 1) != 0)
                {
                    none |= MouseButtons.Left;
                }
                if ((button & 2) != 0)
                {
                    none |= MouseButtons.Right;
                }
                if ((button & 4) != 0)
                {
                    none |= MouseButtons.Middle;
                }
                return none;
            }
        }

        public Point MousePosition
        {
            get
            {
                return new Point(this.NativeHTMLEventObj.GetX(), this.NativeHTMLEventObj.GetY());
            }
        }

        private System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj NativeHTMLEventObj
        {
            get
            {
                return this.htmlEventObj;
            }
        }

        public Point OffsetMousePosition
        {
            get
            {
                return new Point(this.NativeHTMLEventObj.GetOffsetX(), this.NativeHTMLEventObj.GetOffsetY());
            }
        }

        public bool ReturnValue
        {
            get
            {
                object returnValue = this.NativeHTMLEventObj.GetReturnValue();
                if (returnValue != null)
                {
                    return (bool) returnValue;
                }
                return true;
            }
            set
            {
                object p = value;
                this.NativeHTMLEventObj.SetReturnValue(p);
            }
        }

        public bool ShiftKeyPressed
        {
            get
            {
                return this.NativeHTMLEventObj.GetShiftKey();
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public HtmlElement ToElement
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement toElement = this.NativeHTMLEventObj.GetToElement();
                if (toElement != null)
                {
                    return new HtmlElement(this.shimManager, toElement);
                }
                return null;
            }
        }
    }
}

