namespace System.Xml.Linq
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Inserter
    {
        private XContainer parent;
        private XNode previous;
        private string text;
        public Inserter(XContainer parent, XNode anchor)
        {
            this.parent = parent;
            this.previous = anchor;
            this.text = null;
        }

        public void Add(object content)
        {
            this.AddContent(content);
            if (this.text != null)
            {
                if (this.parent.content == null)
                {
                    if (this.parent.SkipNotify())
                    {
                        this.parent.content = this.text;
                    }
                    else if (this.text.Length > 0)
                    {
                        this.InsertNode(new XText(this.text));
                    }
                    else if (this.parent is XElement)
                    {
                        this.parent.NotifyChanging(this.parent, XObjectChangeEventArgs.Value);
                        if (this.parent.content != null)
                        {
                            throw new InvalidOperationException(Res.GetString("InvalidOperation_ExternalCode"));
                        }
                        this.parent.content = this.text;
                        this.parent.NotifyChanged(this.parent, XObjectChangeEventArgs.Value);
                    }
                    else
                    {
                        this.parent.content = this.text;
                    }
                }
                else if (this.text.Length > 0)
                {
                    if ((this.previous is XText) && !(this.previous is XCData))
                    {
                        XText previous = (XText) this.previous;
                        previous.Value = previous.Value + this.text;
                    }
                    else
                    {
                        this.parent.ConvertTextToNode();
                        this.InsertNode(new XText(this.text));
                    }
                }
            }
        }

        private void AddContent(object content)
        {
            if (content != null)
            {
                XNode n = content as XNode;
                if (n != null)
                {
                    this.AddNode(n);
                }
                else
                {
                    string s = content as string;
                    if (s != null)
                    {
                        this.AddString(s);
                    }
                    else
                    {
                        XStreamingElement other = content as XStreamingElement;
                        if (other != null)
                        {
                            this.AddNode(new XElement(other));
                        }
                        else
                        {
                            object[] objArray = content as object[];
                            if (objArray != null)
                            {
                                foreach (object obj2 in objArray)
                                {
                                    this.AddContent(obj2);
                                }
                            }
                            else
                            {
                                IEnumerable enumerable = content as IEnumerable;
                                if (enumerable != null)
                                {
                                    foreach (object obj3 in enumerable)
                                    {
                                        this.AddContent(obj3);
                                    }
                                }
                                else
                                {
                                    if (content is XAttribute)
                                    {
                                        throw new ArgumentException(Res.GetString("Argument_AddAttribute"));
                                    }
                                    this.AddString(XContainer.GetStringValue(content));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddNode(XNode n)
        {
            this.parent.ValidateNode(n, this.previous);
            if (n.parent != null)
            {
                n = n.CloneNode();
            }
            else
            {
                XNode parent = this.parent;
                while (parent.parent != null)
                {
                    parent = parent.parent;
                }
                if (n == parent)
                {
                    n = n.CloneNode();
                }
            }
            this.parent.ConvertTextToNode();
            if (this.text != null)
            {
                if (this.text.Length > 0)
                {
                    if ((this.previous is XText) && !(this.previous is XCData))
                    {
                        XText previous = (XText) this.previous;
                        previous.Value = previous.Value + this.text;
                    }
                    else
                    {
                        this.InsertNode(new XText(this.text));
                    }
                }
                this.text = null;
            }
            this.InsertNode(n);
        }

        private void AddString(string s)
        {
            this.parent.ValidateString(s);
            this.text = this.text + s;
        }

        private void InsertNode(XNode n)
        {
            bool flag = this.parent.NotifyChanging(n, XObjectChangeEventArgs.Add);
            if (n.parent != null)
            {
                throw new InvalidOperationException(Res.GetString("InvalidOperation_ExternalCode"));
            }
            n.parent = this.parent;
            if ((this.parent.content == null) || (this.parent.content is string))
            {
                n.next = n;
                this.parent.content = n;
            }
            else if (this.previous == null)
            {
                XNode content = (XNode) this.parent.content;
                n.next = content.next;
                content.next = n;
            }
            else
            {
                n.next = this.previous.next;
                this.previous.next = n;
                if (this.parent.content == this.previous)
                {
                    this.parent.content = n;
                }
            }
            this.previous = n;
            if (flag)
            {
                this.parent.NotifyChanged(n, XObjectChangeEventArgs.Add);
            }
        }
    }
}

