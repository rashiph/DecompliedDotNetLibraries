namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class HtmlElement
    {
        internal static readonly object EventClick = new object();
        internal static readonly object EventDoubleClick = new object();
        internal static readonly object EventDrag = new object();
        internal static readonly object EventDragEnd = new object();
        internal static readonly object EventDragLeave = new object();
        internal static readonly object EventDragOver = new object();
        internal static readonly object EventFocusing = new object();
        internal static readonly object EventGotFocus = new object();
        internal static readonly object EventKeyDown = new object();
        internal static readonly object EventKeyPress = new object();
        internal static readonly object EventKeyUp = new object();
        internal static readonly object EventLosingFocus = new object();
        internal static readonly object EventLostFocus = new object();
        internal static readonly object EventMouseDown = new object();
        internal static readonly object EventMouseEnter = new object();
        internal static readonly object EventMouseLeave = new object();
        internal static readonly object EventMouseMove = new object();
        internal static readonly object EventMouseOver = new object();
        internal static readonly object EventMouseUp = new object();
        private System.Windows.Forms.UnsafeNativeMethods.IHTMLElement htmlElement;
        private HtmlShimManager shimManager;

        public event HtmlElementEventHandler Click
        {
            add
            {
                this.ElementShim.AddHandler(EventClick, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventClick, value);
            }
        }

        public event HtmlElementEventHandler DoubleClick
        {
            add
            {
                this.ElementShim.AddHandler(EventDoubleClick, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventDoubleClick, value);
            }
        }

        public event HtmlElementEventHandler Drag
        {
            add
            {
                this.ElementShim.AddHandler(EventDrag, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventDrag, value);
            }
        }

        public event HtmlElementEventHandler DragEnd
        {
            add
            {
                this.ElementShim.AddHandler(EventDragEnd, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventDragEnd, value);
            }
        }

        public event HtmlElementEventHandler DragLeave
        {
            add
            {
                this.ElementShim.AddHandler(EventDragLeave, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventDragLeave, value);
            }
        }

        public event HtmlElementEventHandler DragOver
        {
            add
            {
                this.ElementShim.AddHandler(EventDragOver, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventDragOver, value);
            }
        }

        public event HtmlElementEventHandler Focusing
        {
            add
            {
                this.ElementShim.AddHandler(EventFocusing, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventFocusing, value);
            }
        }

        public event HtmlElementEventHandler GotFocus
        {
            add
            {
                this.ElementShim.AddHandler(EventGotFocus, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventGotFocus, value);
            }
        }

        public event HtmlElementEventHandler KeyDown
        {
            add
            {
                this.ElementShim.AddHandler(EventKeyDown, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventKeyDown, value);
            }
        }

        public event HtmlElementEventHandler KeyPress
        {
            add
            {
                this.ElementShim.AddHandler(EventKeyPress, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventKeyPress, value);
            }
        }

        public event HtmlElementEventHandler KeyUp
        {
            add
            {
                this.ElementShim.AddHandler(EventKeyUp, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventKeyUp, value);
            }
        }

        public event HtmlElementEventHandler LosingFocus
        {
            add
            {
                this.ElementShim.AddHandler(EventLosingFocus, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventLosingFocus, value);
            }
        }

        public event HtmlElementEventHandler LostFocus
        {
            add
            {
                this.ElementShim.AddHandler(EventLostFocus, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventLostFocus, value);
            }
        }

        public event HtmlElementEventHandler MouseDown
        {
            add
            {
                this.ElementShim.AddHandler(EventMouseDown, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventMouseDown, value);
            }
        }

        public event HtmlElementEventHandler MouseEnter
        {
            add
            {
                this.ElementShim.AddHandler(EventMouseEnter, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventMouseEnter, value);
            }
        }

        public event HtmlElementEventHandler MouseLeave
        {
            add
            {
                this.ElementShim.AddHandler(EventMouseLeave, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventMouseLeave, value);
            }
        }

        public event HtmlElementEventHandler MouseMove
        {
            add
            {
                this.ElementShim.AddHandler(EventMouseMove, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventMouseMove, value);
            }
        }

        public event HtmlElementEventHandler MouseOver
        {
            add
            {
                this.ElementShim.AddHandler(EventMouseOver, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventMouseOver, value);
            }
        }

        public event HtmlElementEventHandler MouseUp
        {
            add
            {
                this.ElementShim.AddHandler(EventMouseUp, value);
            }
            remove
            {
                this.ElementShim.RemoveHandler(EventMouseUp, value);
            }
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        internal HtmlElement(HtmlShimManager shimManager, System.Windows.Forms.UnsafeNativeMethods.IHTMLElement element)
        {
            this.htmlElement = element;
            this.shimManager = shimManager;
        }

        public HtmlElement AppendChild(HtmlElement newElement)
        {
            return this.InsertAdjacentElement(HtmlElementInsertionOrientation.BeforeEnd, newElement);
        }

        public void AttachEventHandler(string eventName, EventHandler eventHandler)
        {
            this.ElementShim.AttachEventHandler(eventName, eventHandler);
        }

        public void DetachEventHandler(string eventName, EventHandler eventHandler)
        {
            this.ElementShim.DetachEventHandler(eventName, eventHandler);
        }

        public override bool Equals(object obj)
        {
            return (this == (obj as HtmlElement));
        }

        public void Focus()
        {
            try
            {
                ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).Focus();
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2146826178)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("HtmlElementMethodNotSupported"));
                }
                throw;
            }
        }

        public string GetAttribute(string attributeName)
        {
            object attribute = this.NativeHtmlElement.GetAttribute(attributeName, 0);
            if (attribute != null)
            {
                return attribute.ToString();
            }
            return "";
        }

        public HtmlElementCollection GetElementsByTagName(string tagName)
        {
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection elementsByTagName = ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).GetElementsByTagName(tagName);
            if (elementsByTagName == null)
            {
                return new HtmlElementCollection(this.shimManager);
            }
            return new HtmlElementCollection(this.shimManager, elementsByTagName);
        }

        public override int GetHashCode()
        {
            if (this.htmlElement != null)
            {
                return this.htmlElement.GetHashCode();
            }
            return 0;
        }

        public HtmlElement InsertAdjacentElement(HtmlElementInsertionOrientation orient, HtmlElement newElement)
        {
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement element = ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).InsertAdjacentElement(orient.ToString(), (System.Windows.Forms.UnsafeNativeMethods.IHTMLElement) newElement.DomElement);
            if (element == null)
            {
                return null;
            }
            return new HtmlElement(this.shimManager, element);
        }

        public object InvokeMember(string methodName)
        {
            return this.InvokeMember(methodName, null);
        }

        public object InvokeMember(string methodName, params object[] parameter)
        {
            object obj2 = null;
            System.Windows.Forms.NativeMethods.tagDISPPARAMS pDispParams = new System.Windows.Forms.NativeMethods.tagDISPPARAMS {
                rgvarg = IntPtr.Zero
            };
            try
            {
                System.Windows.Forms.UnsafeNativeMethods.IDispatch nativeHtmlElement = this.NativeHtmlElement as System.Windows.Forms.UnsafeNativeMethods.IDispatch;
                if (nativeHtmlElement != null)
                {
                    Guid empty = Guid.Empty;
                    string[] rgszNames = new string[] { methodName };
                    int[] rgDispId = new int[] { -1 };
                    if (!System.Windows.Forms.NativeMethods.Succeeded(nativeHtmlElement.GetIDsOfNames(ref empty, rgszNames, 1, System.Windows.Forms.SafeNativeMethods.GetThreadLCID(), rgDispId)) || (rgDispId[0] == -1))
                    {
                        return obj2;
                    }
                    if (parameter != null)
                    {
                        Array.Reverse(parameter);
                    }
                    pDispParams.rgvarg = (parameter == null) ? IntPtr.Zero : HtmlDocument.ArrayToVARIANTVector(parameter);
                    pDispParams.cArgs = (parameter == null) ? 0 : parameter.Length;
                    pDispParams.rgdispidNamedArgs = IntPtr.Zero;
                    pDispParams.cNamedArgs = 0;
                    object[] pVarResult = new object[1];
                    if (nativeHtmlElement.Invoke(rgDispId[0], ref empty, System.Windows.Forms.SafeNativeMethods.GetThreadLCID(), 1, pDispParams, pVarResult, new System.Windows.Forms.NativeMethods.tagEXCEPINFO(), null) == 0)
                    {
                        obj2 = pVarResult[0];
                    }
                }
                return obj2;
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
            finally
            {
                if (pDispParams.rgvarg != IntPtr.Zero)
                {
                    HtmlDocument.FreeVARIANTVector(pDispParams.rgvarg, parameter.Length);
                }
            }
            return obj2;
        }

        public static bool operator ==(HtmlElement left, HtmlElement right)
        {
            bool flag;
            if (object.ReferenceEquals(left, null) != object.ReferenceEquals(right, null))
            {
                return false;
            }
            if (object.ReferenceEquals(left, null))
            {
                return true;
            }
            IntPtr zero = IntPtr.Zero;
            IntPtr pUnk = IntPtr.Zero;
            try
            {
                zero = Marshal.GetIUnknownForObject(left.NativeHtmlElement);
                pUnk = Marshal.GetIUnknownForObject(right.NativeHtmlElement);
                flag = zero == pUnk;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.Release(zero);
                }
                if (pUnk != IntPtr.Zero)
                {
                    Marshal.Release(pUnk);
                }
            }
            return flag;
        }

        public static bool operator !=(HtmlElement left, HtmlElement right)
        {
            return !(left == right);
        }

        public void RaiseEvent(string eventName)
        {
            ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement3) this.NativeHtmlElement).FireEvent(eventName, null);
        }

        public void RemoveFocus()
        {
            ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).Blur();
        }

        public void ScrollIntoView(bool alignWithTop)
        {
            this.NativeHtmlElement.ScrollIntoView(alignWithTop);
        }

        public void SetAttribute(string attributeName, string value)
        {
            try
            {
                this.NativeHtmlElement.SetAttribute(attributeName, value, 0);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147352567)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("HtmlElementAttributeNotSupported"));
                }
                throw;
            }
        }

        public HtmlElementCollection All
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection all = this.NativeHtmlElement.GetAll() as System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection;
                if (all == null)
                {
                    return new HtmlElementCollection(this.shimManager);
                }
                return new HtmlElementCollection(this.shimManager, all);
            }
        }

        public bool CanHaveChildren
        {
            get
            {
                return ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).CanHaveChildren();
            }
        }

        public HtmlElementCollection Children
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection children = this.NativeHtmlElement.GetChildren() as System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection;
                if (children == null)
                {
                    return new HtmlElementCollection(this.shimManager);
                }
                return new HtmlElementCollection(this.shimManager, children);
            }
        }

        public Rectangle ClientRectangle
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2 nativeHtmlElement = (System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement;
                return new Rectangle(nativeHtmlElement.ClientLeft(), nativeHtmlElement.ClientTop(), nativeHtmlElement.ClientWidth(), nativeHtmlElement.ClientHeight());
            }
        }

        public HtmlDocument Document
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument doc = this.NativeHtmlElement.GetDocument() as System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument;
                if (doc == null)
                {
                    return null;
                }
                return new HtmlDocument(this.shimManager, doc);
            }
        }

        public object DomElement
        {
            get
            {
                return this.NativeHtmlElement;
            }
        }

        private HtmlElementShim ElementShim
        {
            get
            {
                if (this.ShimManager == null)
                {
                    return null;
                }
                HtmlElementShim elementShim = this.ShimManager.GetElementShim(this);
                if (elementShim == null)
                {
                    this.shimManager.AddElementShim(this);
                    elementShim = this.ShimManager.GetElementShim(this);
                }
                return elementShim;
            }
        }

        public bool Enabled
        {
            get
            {
                return !((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement3) this.NativeHtmlElement).GetDisabled();
            }
            set
            {
                ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement3) this.NativeHtmlElement).SetDisabled(!value);
            }
        }

        public HtmlElement FirstChild
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement element = null;
                System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode nativeHtmlElement = this.NativeHtmlElement as System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode;
                if (nativeHtmlElement != null)
                {
                    element = nativeHtmlElement.FirstChild() as System.Windows.Forms.UnsafeNativeMethods.IHTMLElement;
                }
                if (element == null)
                {
                    return null;
                }
                return new HtmlElement(this.shimManager, element);
            }
        }

        public string Id
        {
            get
            {
                return this.NativeHtmlElement.GetId();
            }
            set
            {
                this.NativeHtmlElement.SetId(value);
            }
        }

        public string InnerHtml
        {
            get
            {
                return this.NativeHtmlElement.GetInnerHTML();
            }
            set
            {
                try
                {
                    this.NativeHtmlElement.SetInnerHTML(value);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode == -2146827688)
                    {
                        throw new NotSupportedException(System.Windows.Forms.SR.GetString("HtmlElementPropertyNotSupported"));
                    }
                    throw;
                }
            }
        }

        public string InnerText
        {
            get
            {
                return this.NativeHtmlElement.GetInnerText();
            }
            set
            {
                try
                {
                    this.NativeHtmlElement.SetInnerText(value);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode == -2146827688)
                    {
                        throw new NotSupportedException(System.Windows.Forms.SR.GetString("HtmlElementPropertyNotSupported"));
                    }
                    throw;
                }
            }
        }

        public string Name
        {
            get
            {
                return this.GetAttribute("Name");
            }
            set
            {
                this.SetAttribute("Name", value);
            }
        }

        private System.Windows.Forms.UnsafeNativeMethods.IHTMLElement NativeHtmlElement
        {
            get
            {
                return this.htmlElement;
            }
        }

        public HtmlElement NextSibling
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement element = null;
                System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode nativeHtmlElement = this.NativeHtmlElement as System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode;
                if (nativeHtmlElement != null)
                {
                    element = nativeHtmlElement.NextSibling() as System.Windows.Forms.UnsafeNativeMethods.IHTMLElement;
                }
                if (element == null)
                {
                    return null;
                }
                return new HtmlElement(this.shimManager, element);
            }
        }

        public HtmlElement OffsetParent
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement offsetParent = this.NativeHtmlElement.GetOffsetParent();
                if (offsetParent == null)
                {
                    return null;
                }
                return new HtmlElement(this.shimManager, offsetParent);
            }
        }

        public Rectangle OffsetRectangle
        {
            get
            {
                return new Rectangle(this.NativeHtmlElement.GetOffsetLeft(), this.NativeHtmlElement.GetOffsetTop(), this.NativeHtmlElement.GetOffsetWidth(), this.NativeHtmlElement.GetOffsetHeight());
            }
        }

        public string OuterHtml
        {
            get
            {
                return this.NativeHtmlElement.GetOuterHTML();
            }
            set
            {
                try
                {
                    this.NativeHtmlElement.SetOuterHTML(value);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode == -2146827688)
                    {
                        throw new NotSupportedException(System.Windows.Forms.SR.GetString("HtmlElementPropertyNotSupported"));
                    }
                    throw;
                }
            }
        }

        public string OuterText
        {
            get
            {
                return this.NativeHtmlElement.GetOuterText();
            }
            set
            {
                try
                {
                    this.NativeHtmlElement.SetOuterText(value);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode == -2146827688)
                    {
                        throw new NotSupportedException(System.Windows.Forms.SR.GetString("HtmlElementPropertyNotSupported"));
                    }
                    throw;
                }
            }
        }

        public HtmlElement Parent
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement parentElement = this.NativeHtmlElement.GetParentElement();
                if (parentElement == null)
                {
                    return null;
                }
                return new HtmlElement(this.shimManager, parentElement);
            }
        }

        public int ScrollLeft
        {
            get
            {
                return ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).GetScrollLeft();
            }
            set
            {
                ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).SetScrollLeft(value);
            }
        }

        public Rectangle ScrollRectangle
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2 nativeHtmlElement = (System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement;
                return new Rectangle(nativeHtmlElement.GetScrollLeft(), nativeHtmlElement.GetScrollTop(), nativeHtmlElement.GetScrollWidth(), nativeHtmlElement.GetScrollHeight());
            }
        }

        public int ScrollTop
        {
            get
            {
                return ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).GetScrollTop();
            }
            set
            {
                ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).SetScrollTop(value);
            }
        }

        private HtmlShimManager ShimManager
        {
            get
            {
                return this.shimManager;
            }
        }

        public string Style
        {
            get
            {
                return this.NativeHtmlElement.GetStyle().GetCssText();
            }
            set
            {
                this.NativeHtmlElement.GetStyle().SetCssText(value);
            }
        }

        public short TabIndex
        {
            get
            {
                return ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).GetTabIndex();
            }
            set
            {
                ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).SetTabIndex(value);
            }
        }

        public string TagName
        {
            get
            {
                return this.NativeHtmlElement.GetTagName();
            }
        }

        [ClassInterface(ClassInterfaceType.None)]
        private class HTMLElementEvents2 : StandardOleMarshalObject, System.Windows.Forms.UnsafeNativeMethods.DHTMLElementEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLAnchorEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLAreaEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLButtonElementEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLControlElementEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLFormElementEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLFrameSiteEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLImgEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLInputFileElementEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLInputImageEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLInputTextElementEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLLabelEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLLinkElementEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLMapEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLMarqueeElementEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLOptionButtonElementEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLSelectElementEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLStyleElementEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLTableEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLTextContainerEvents2, System.Windows.Forms.UnsafeNativeMethods.DHTMLScriptEvents2
        {
            private HtmlElement parent;

            public HTMLElementEvents2(HtmlElement htmlElement)
            {
                this.parent = htmlElement;
            }

            private void FireEvent(object key, EventArgs e)
            {
                if (this.parent != null)
                {
                    this.parent.ElementShim.FireEvent(key, e);
                }
            }

            public void onabort(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventLostFocus, e);
            }

            public void onbounce(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onchange_void(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventClick, e);
                return e.ReturnValue;
            }

            public bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventDoubleClick, e);
                return e.ReturnValue;
            }

            public void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventDrag, e);
                return e.ReturnValue;
            }

            public void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventDragEnd, e);
            }

            public bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventDragLeave, e);
            }

            public bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventDragOver, e);
                return e.ReturnValue;
            }

            public bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onerror(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onfinish(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventGotFocus, e);
            }

            public void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventFocusing, e);
            }

            public void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventLosingFocus, e);
            }

            public bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventKeyDown, e);
            }

            public bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventKeyPress, e);
                return e.ReturnValue;
            }

            public void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventKeyUp, e);
            }

            public void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventMouseDown, e);
            }

            public void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventMouseEnter, e);
            }

            public void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventMouseLeave, e);
            }

            public void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventMouseMove, e);
            }

            public void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventMouseOver, e);
            }

            public void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlElement.EventMouseUp, e);
            }

            public bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onreset(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onsubmit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }
        }

        internal class HtmlElementShim : HtmlShim
        {
            private System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 associatedWindow;
            private AxHost.ConnectionPointCookie cookie;
            private static System.Type[] dispInterfaceTypes = new System.Type[] { 
                typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLElementEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLAnchorEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLAreaEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLButtonElementEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLControlElementEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLFormElementEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLFrameSiteEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLImgEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLInputFileElementEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLInputImageEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLInputTextElementEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLLabelEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLLinkElementEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLMapEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLMarqueeElementEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLOptionButtonElementEvents2), 
                typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLSelectElementEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLStyleElementEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLTableEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLTextContainerEvents2), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLScriptEvents2)
             };
            private HtmlElement htmlElement;

            public HtmlElementShim(HtmlElement element)
            {
                this.htmlElement = element;
                if (this.htmlElement != null)
                {
                    HtmlDocument document = this.htmlElement.Document;
                    if (document != null)
                    {
                        HtmlWindow window = document.Window;
                        if (window != null)
                        {
                            this.associatedWindow = window.NativeHtmlWindow;
                        }
                    }
                }
            }

            public override void AttachEventHandler(string eventName, EventHandler eventHandler)
            {
                HtmlToClrEventProxy pdisp = base.AddEventProxy(eventName, eventHandler);
                ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).AttachEvent(eventName, pdisp);
            }

            public override void ConnectToEvents()
            {
                if ((this.cookie == null) || !this.cookie.Connected)
                {
                    for (int i = 0; (i < dispInterfaceTypes.Length) && (this.cookie == null); i++)
                    {
                        this.cookie = new AxHost.ConnectionPointCookie(this.NativeHtmlElement, new HtmlElement.HTMLElementEvents2(this.htmlElement), dispInterfaceTypes[i], false);
                        if (!this.cookie.Connected)
                        {
                            this.cookie = null;
                        }
                    }
                }
            }

            public override void DetachEventHandler(string eventName, EventHandler eventHandler)
            {
                HtmlToClrEventProxy pdisp = base.RemoveEventProxy(eventHandler);
                if (pdisp != null)
                {
                    ((System.Windows.Forms.UnsafeNativeMethods.IHTMLElement2) this.NativeHtmlElement).DetachEvent(eventName, pdisp);
                }
            }

            public override void DisconnectFromEvents()
            {
                if (this.cookie != null)
                {
                    this.cookie.Disconnect();
                    this.cookie = null;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (this.htmlElement != null)
                {
                    Marshal.FinalReleaseComObject(this.htmlElement.NativeHtmlElement);
                }
                this.htmlElement = null;
            }

            protected override object GetEventSender()
            {
                return this.htmlElement;
            }

            public override System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 AssociatedWindow
            {
                get
                {
                    return this.associatedWindow;
                }
            }

            internal HtmlElement Element
            {
                get
                {
                    return this.htmlElement;
                }
            }

            public System.Windows.Forms.UnsafeNativeMethods.IHTMLElement NativeHtmlElement
            {
                get
                {
                    return this.htmlElement.NativeHtmlElement;
                }
            }
        }
    }
}

