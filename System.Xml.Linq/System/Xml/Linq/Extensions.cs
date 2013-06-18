namespace System.Xml.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public static class Extensions
    {
        public static IEnumerable<XElement> Ancestors<T>(this IEnumerable<T> source) where T: XNode
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return GetAncestors<T>(source, null, false);
        }

        public static IEnumerable<XElement> Ancestors<T>(this IEnumerable<T> source, XName name) where T: XNode
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (name == null)
            {
                return XElement.EmptySequence;
            }
            return GetAncestors<T>(source, name, false);
        }

        public static IEnumerable<XElement> AncestorsAndSelf(this IEnumerable<XElement> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return GetAncestors<XElement>(source, null, true);
        }

        public static IEnumerable<XElement> AncestorsAndSelf(this IEnumerable<XElement> source, XName name)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (name == null)
            {
                return XElement.EmptySequence;
            }
            return GetAncestors<XElement>(source, name, true);
        }

        public static IEnumerable<XAttribute> Attributes(this IEnumerable<XElement> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return GetAttributes(source, null);
        }

        public static IEnumerable<XAttribute> Attributes(this IEnumerable<XElement> source, XName name)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (name == null)
            {
                return XAttribute.EmptySequence;
            }
            return GetAttributes(source, name);
        }

        public static IEnumerable<XNode> DescendantNodes<T>(this IEnumerable<T> source) where T: XContainer
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return GetDescendantNodes<T>(source, false);
        }

        public static IEnumerable<XNode> DescendantNodesAndSelf(this IEnumerable<XElement> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return GetDescendantNodes<XElement>(source, true);
        }

        public static IEnumerable<XElement> Descendants<T>(this IEnumerable<T> source) where T: XContainer
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return GetDescendants<T>(source, null, false);
        }

        public static IEnumerable<XElement> Descendants<T>(this IEnumerable<T> source, XName name) where T: XContainer
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (name == null)
            {
                return XElement.EmptySequence;
            }
            return GetDescendants<T>(source, name, false);
        }

        public static IEnumerable<XElement> DescendantsAndSelf(this IEnumerable<XElement> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return GetDescendants<XElement>(source, null, true);
        }

        public static IEnumerable<XElement> DescendantsAndSelf(this IEnumerable<XElement> source, XName name)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (name == null)
            {
                return XElement.EmptySequence;
            }
            return GetDescendants<XElement>(source, name, true);
        }

        public static IEnumerable<XElement> Elements<T>(this IEnumerable<T> source) where T: XContainer
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return GetElements<T>(source, null);
        }

        public static IEnumerable<XElement> Elements<T>(this IEnumerable<T> source, XName name) where T: XContainer
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (name == null)
            {
                return XElement.EmptySequence;
            }
            return GetElements<T>(source, name);
        }

        private static IEnumerable<XElement> GetAncestors<T>(IEnumerable<T> source, XName name, bool self) where T: XNode
        {
            foreach (XNode iteratorVariable0 in source)
            {
                if (iteratorVariable0 != null)
                {
                    for (XElement iteratorVariable1 = (self ? ((XElement) iteratorVariable0) : ((XElement) iteratorVariable0.parent)) as XElement; iteratorVariable1 != null; iteratorVariable1 = iteratorVariable1.parent as XElement)
                    {
                        if ((name == null) || (iteratorVariable1.name == name))
                        {
                            yield return iteratorVariable1;
                        }
                    }
                }
            }
        }

        private static IEnumerable<XAttribute> GetAttributes(IEnumerable<XElement> source, XName name)
        {
            foreach (XElement iteratorVariable0 in source)
            {
                if (iteratorVariable0 != null)
                {
                    XAttribute lastAttr = iteratorVariable0.lastAttr;
                    if (lastAttr != null)
                    {
                        do
                        {
                            lastAttr = lastAttr.next;
                            if ((name == null) || (lastAttr.name == name))
                            {
                                yield return lastAttr;
                            }
                        }
                        while ((lastAttr.parent == iteratorVariable0) && (lastAttr != iteratorVariable0.lastAttr));
                    }
                }
            }
        }

        private static IEnumerable<XNode> GetDescendantNodes<T>(IEnumerable<T> source, bool self) where T: XContainer
        {
            foreach (XContainer iteratorVariable0 in source)
            {
                XNode iteratorVariable3;
                XContainer iteratorVariable2;
                if (iteratorVariable0 == null)
                {
                    continue;
                }
                if (self)
                {
                    yield return iteratorVariable0;
                }
                XNode next = iteratorVariable0;
            Label_009C:
                iteratorVariable2 = next as XContainer;
                if ((iteratorVariable2 == null) || ((iteratorVariable3 = iteratorVariable2.FirstNode) == null))
                {
                    goto Label_00EA;
                }
                next = iteratorVariable3;
                goto Label_013F;
            Label_00D9:
                next = next.parent;
            Label_00EA:
                if (((next != null) && (next != iteratorVariable0)) && (next == next.parent.content))
                {
                    goto Label_00D9;
                }
                if ((next == null) || (next == iteratorVariable0))
                {
                    continue;
                }
                next = next.next;
            Label_013F:
                yield return next;
                goto Label_009C;
            }
        }

        private static IEnumerable<XElement> GetDescendants<T>(IEnumerable<T> source, XName name, bool self) where T: XContainer
        {
            foreach (XContainer iteratorVariable0 in source)
            {
                XElement iteratorVariable4;
                if (iteratorVariable0 == null)
                {
                    continue;
                }
                if (self)
                {
                    XElement iteratorVariable1 = (XElement) iteratorVariable0;
                    if ((name == null) || (iteratorVariable1.name == name))
                    {
                        yield return iteratorVariable1;
                    }
                }
                XNode next = iteratorVariable0;
                XContainer iteratorVariable3 = iteratorVariable0;
            Label_00DF:
                if ((iteratorVariable3 == null) || !(iteratorVariable3.content is XNode))
                {
                    goto Label_0127;
                }
                next = ((XNode) iteratorVariable3.content).next;
                goto Label_017F;
            Label_0116:
                next = next.parent;
            Label_0127:
                if (((next != null) && (next != iteratorVariable0)) && (next == next.parent.content))
                {
                    goto Label_0116;
                }
                if ((next == null) || (next == iteratorVariable0))
                {
                    continue;
                }
                next = next.next;
            Label_017F:
                iteratorVariable4 = next as XElement;
                if ((iteratorVariable4 != null) && ((name == null) || (iteratorVariable4.name == name)))
                {
                    yield return iteratorVariable4;
                }
                iteratorVariable3 = iteratorVariable4;
                goto Label_00DF;
            }
        }

        private static IEnumerable<XElement> GetElements<T>(IEnumerable<T> source, XName name) where T: XContainer
        {
            foreach (XContainer iteratorVariable0 in source)
            {
                if (iteratorVariable0 != null)
                {
                    XNode content = iteratorVariable0.content as XNode;
                    if (content != null)
                    {
                        do
                        {
                            content = content.next;
                            XElement iteratorVariable2 = content as XElement;
                            if ((iteratorVariable2 != null) && ((name == null) || (iteratorVariable2.name == name)))
                            {
                                yield return iteratorVariable2;
                            }
                        }
                        while ((content.parent == iteratorVariable0) && (content != iteratorVariable0.content));
                    }
                }
            }
        }

        public static IEnumerable<T> InDocumentOrder<T>(this IEnumerable<T> source) where T: XNode
        {
            return source.OrderBy<T, XNode>(n => n, XNode.DocumentOrderComparer);
        }

        public static IEnumerable<XNode> Nodes<T>(this IEnumerable<T> source) where T: XContainer
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            foreach (XContainer iteratorVariable0 in source)
            {
                if (iteratorVariable0 == null)
                {
                    continue;
                }
                XNode lastNode = iteratorVariable0.LastNode;
                if (lastNode != null)
                {
                    do
                    {
                        lastNode = lastNode.next;
                        yield return lastNode;
                    }
                    while ((lastNode.parent == iteratorVariable0) && (lastNode != iteratorVariable0.content));
                }
            }
        }

        public static void Remove(this IEnumerable<XAttribute> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            foreach (XAttribute attribute in new List<XAttribute>(source))
            {
                if (attribute != null)
                {
                    attribute.Remove();
                }
            }
        }

        public static void Remove<T>(this IEnumerable<T> source) where T: XNode
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            foreach (T local in new List<T>(source))
            {
                if (local != null)
                {
                    local.Remove();
                }
            }
        }

        [CompilerGenerated]
        private sealed class <GetAncestors>d__f<T> : IEnumerable<XElement>, IEnumerable, IEnumerator<XElement>, IEnumerator, IDisposable where T: XNode
        {
            private int <>1__state;
            private XElement <>2__current;
            public XName <>3__name;
            public bool <>3__self;
            public IEnumerable<T> <>3__source;
            public IEnumerator<T> <>7__wrap12;
            private int <>l__initialThreadId;
            public XElement <e>5__11;
            public XNode <node>5__10;
            public XName name;
            public bool self;
            public IEnumerable<T> source;

            [DebuggerHidden]
            public <GetAncestors>d__f(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally13()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap12 != null)
                {
                    this.<>7__wrap12.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>7__wrap12 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_00ED;

                        case 2:
                            goto Label_00C8;

                        default:
                            goto Label_0103;
                    }
                Label_0042:
                    this.<node>5__10 = this.<>7__wrap12.Current;
                    if (this.<node>5__10 != null)
                    {
                        this.<e>5__11 = (this.self ? ((XElement) this.<node>5__10) : ((XElement) this.<node>5__10.parent)) as XElement;
                        while (this.<e>5__11 != null)
                        {
                            if ((this.name != null) && !(this.<e>5__11.name == this.name))
                            {
                                goto Label_00CF;
                            }
                            this.<>2__current = this.<e>5__11;
                            this.<>1__state = 2;
                            return true;
                        Label_00C8:
                            this.<>1__state = 1;
                        Label_00CF:
                            this.<e>5__11 = this.<e>5__11.parent as XElement;
                        }
                    }
                Label_00ED:
                    if (this.<>7__wrap12.MoveNext())
                    {
                        goto Label_0042;
                    }
                    this.<>m__Finally13();
                Label_0103:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<XElement> IEnumerable<XElement>.GetEnumerator()
            {
                Extensions.<GetAncestors>d__f<T> _f;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    _f = (Extensions.<GetAncestors>d__f<T>) this;
                }
                else
                {
                    _f = new Extensions.<GetAncestors>d__f<T>(0);
                }
                _f.source = this.<>3__source;
                _f.name = this.<>3__name;
                _f.self = this.<>3__self;
                return _f;
            }

            [DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 1:
                    case 2:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally13();
                        }
                        return;
                }
            }

            XElement IEnumerator<XElement>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }


        [CompilerGenerated]
        private sealed class <GetDescendantNodes>d__16<T> : IEnumerable<XNode>, IEnumerable, IEnumerator<XNode>, IEnumerator, IDisposable where T: XContainer
        {
            private int <>1__state;
            private XNode <>2__current;
            public bool <>3__self;
            public IEnumerable<T> <>3__source;
            public IEnumerator<T> <>7__wrap1b;
            private int <>l__initialThreadId;
            public XContainer <c>5__19;
            public XNode <first>5__1a;
            public XNode <n>5__18;
            public XContainer <root>5__17;
            public bool self;
            public IEnumerable<T> source;

            [DebuggerHidden]
            public <GetDescendantNodes>d__16(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally1c()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1b != null)
                {
                    this.<>7__wrap1b.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>7__wrap1b = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_0162;

                        case 2:
                            goto Label_0089;

                        case 3:
                            this.<>1__state = 1;
                            goto Label_009C;

                        default:
                            goto Label_0178;
                    }
                Label_0046:
                    this.<root>5__17 = this.<>7__wrap1b.Current;
                    if (this.<root>5__17 == null)
                    {
                        goto Label_0162;
                    }
                    if (!this.self)
                    {
                        goto Label_0090;
                    }
                    this.<>2__current = this.<root>5__17;
                    this.<>1__state = 2;
                    return true;
                Label_0089:
                    this.<>1__state = 1;
                Label_0090:
                    this.<n>5__18 = this.<root>5__17;
                Label_009C:
                    this.<c>5__19 = this.<n>5__18 as XContainer;
                    if ((this.<c>5__19 == null) || ((this.<first>5__1a = this.<c>5__19.FirstNode) == null))
                    {
                        goto Label_00EA;
                    }
                    this.<n>5__18 = this.<first>5__1a;
                    goto Label_013F;
                Label_00D9:
                    this.<n>5__18 = this.<n>5__18.parent;
                Label_00EA:
                    if (((this.<n>5__18 != null) && (this.<n>5__18 != this.<root>5__17)) && (this.<n>5__18 == this.<n>5__18.parent.content))
                    {
                        goto Label_00D9;
                    }
                    if ((this.<n>5__18 == null) || (this.<n>5__18 == this.<root>5__17))
                    {
                        goto Label_0162;
                    }
                    this.<n>5__18 = this.<n>5__18.next;
                Label_013F:
                    this.<>2__current = this.<n>5__18;
                    this.<>1__state = 3;
                    return true;
                Label_0162:
                    if (this.<>7__wrap1b.MoveNext())
                    {
                        goto Label_0046;
                    }
                    this.<>m__Finally1c();
                Label_0178:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<XNode> IEnumerable<XNode>.GetEnumerator()
            {
                Extensions.<GetDescendantNodes>d__16<T> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Extensions.<GetDescendantNodes>d__16<T>) this;
                }
                else
                {
                    d__ = new Extensions.<GetDescendantNodes>d__16<T>(0);
                }
                d__.source = this.<>3__source;
                d__.self = this.<>3__self;
                return d__;
            }

            [DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Xml.Linq.XNode>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 1:
                    case 2:
                    case 3:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally1c();
                        }
                        return;
                }
            }

            XNode IEnumerator<XNode>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <GetDescendants>d__1f<T> : IEnumerable<XElement>, IEnumerable, IEnumerator<XElement>, IEnumerator, IDisposable where T: XContainer
        {
            private int <>1__state;
            private XElement <>2__current;
            public XName <>3__name;
            public bool <>3__self;
            public IEnumerable<T> <>3__source;
            public IEnumerator<T> <>7__wrap25;
            private int <>l__initialThreadId;
            public XContainer <c>5__23;
            public XElement <e>5__21;
            public XElement <e>5__24;
            public XNode <n>5__22;
            public XContainer <root>5__20;
            public XName name;
            public bool self;
            public IEnumerable<T> source;

            [DebuggerHidden]
            public <GetDescendants>d__1f(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally26()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap25 != null)
                {
                    this.<>7__wrap25.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>7__wrap25 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_01ED;

                        case 2:
                            goto Label_00C0;

                        case 3:
                            goto Label_01D5;

                        default:
                            goto Label_0203;
                    }
                Label_0046:
                    this.<root>5__20 = this.<>7__wrap25.Current;
                    if (this.<root>5__20 == null)
                    {
                        goto Label_01ED;
                    }
                    if (!this.self)
                    {
                        goto Label_00C7;
                    }
                    this.<e>5__21 = (XElement) this.<root>5__20;
                    if ((this.name != null) && !(this.<e>5__21.name == this.name))
                    {
                        goto Label_00C7;
                    }
                    this.<>2__current = this.<e>5__21;
                    this.<>1__state = 2;
                    return true;
                Label_00C0:
                    this.<>1__state = 1;
                Label_00C7:
                    this.<n>5__22 = this.<root>5__20;
                    this.<c>5__23 = this.<root>5__20;
                Label_00DF:
                    if ((this.<c>5__23 == null) || !(this.<c>5__23.content is XNode))
                    {
                        goto Label_0127;
                    }
                    this.<n>5__22 = ((XNode) this.<c>5__23.content).next;
                    goto Label_017F;
                Label_0116:
                    this.<n>5__22 = this.<n>5__22.parent;
                Label_0127:
                    if (((this.<n>5__22 != null) && (this.<n>5__22 != this.<root>5__20)) && (this.<n>5__22 == this.<n>5__22.parent.content))
                    {
                        goto Label_0116;
                    }
                    if ((this.<n>5__22 == null) || (this.<n>5__22 == this.<root>5__20))
                    {
                        goto Label_01ED;
                    }
                    this.<n>5__22 = this.<n>5__22.next;
                Label_017F:
                    this.<e>5__24 = this.<n>5__22 as XElement;
                    if ((this.<e>5__24 == null) || ((this.name != null) && !(this.<e>5__24.name == this.name)))
                    {
                        goto Label_01DC;
                    }
                    this.<>2__current = this.<e>5__24;
                    this.<>1__state = 3;
                    return true;
                Label_01D5:
                    this.<>1__state = 1;
                Label_01DC:
                    this.<c>5__23 = this.<e>5__24;
                    goto Label_00DF;
                Label_01ED:
                    if (this.<>7__wrap25.MoveNext())
                    {
                        goto Label_0046;
                    }
                    this.<>m__Finally26();
                Label_0203:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<XElement> IEnumerable<XElement>.GetEnumerator()
            {
                Extensions.<GetDescendants>d__1f<T> d__f;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__f = (Extensions.<GetDescendants>d__1f<T>) this;
                }
                else
                {
                    d__f = new Extensions.<GetDescendants>d__1f<T>(0);
                }
                d__f.source = this.<>3__source;
                d__f.name = this.<>3__name;
                d__f.self = this.<>3__self;
                return d__f;
            }

            [DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 1:
                    case 2:
                    case 3:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally26();
                        }
                        return;
                }
            }

            XElement IEnumerator<XElement>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <GetElements>d__29<T> : IEnumerable<XElement>, IEnumerable, IEnumerator<XElement>, IEnumerator, IDisposable where T: XContainer
        {
            private int <>1__state;
            private XElement <>2__current;
            public XName <>3__name;
            public IEnumerable<T> <>3__source;
            public IEnumerator<T> <>7__wrap2d;
            private int <>l__initialThreadId;
            public XElement <e>5__2c;
            public XNode <n>5__2b;
            public XContainer <root>5__2a;
            public XName name;
            public IEnumerable<T> source;

            [DebuggerHidden]
            public <GetElements>d__29(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally2e()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap2d != null)
                {
                    this.<>7__wrap2d.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>7__wrap2d = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_011B;

                        case 2:
                            goto Label_00EB;

                        default:
                            goto Label_0131;
                    }
                Label_0042:
                    this.<root>5__2a = this.<>7__wrap2d.Current;
                    if (this.<root>5__2a == null)
                    {
                        goto Label_011B;
                    }
                    this.<n>5__2b = this.<root>5__2a.content as XNode;
                    if (this.<n>5__2b == null)
                    {
                        goto Label_011B;
                    }
                Label_0084:
                    this.<n>5__2b = this.<n>5__2b.next;
                    this.<e>5__2c = this.<n>5__2b as XElement;
                    if ((this.<e>5__2c == null) || ((this.name != null) && !(this.<e>5__2c.name == this.name)))
                    {
                        goto Label_00F2;
                    }
                    this.<>2__current = this.<e>5__2c;
                    this.<>1__state = 2;
                    return true;
                Label_00EB:
                    this.<>1__state = 1;
                Label_00F2:
                    if ((this.<n>5__2b.parent == this.<root>5__2a) && (this.<n>5__2b != this.<root>5__2a.content))
                    {
                        goto Label_0084;
                    }
                Label_011B:
                    if (this.<>7__wrap2d.MoveNext())
                    {
                        goto Label_0042;
                    }
                    this.<>m__Finally2e();
                Label_0131:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<XElement> IEnumerable<XElement>.GetEnumerator()
            {
                Extensions.<GetElements>d__29<T> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Extensions.<GetElements>d__29<T>) this;
                }
                else
                {
                    d__ = new Extensions.<GetElements>d__29<T>(0);
                }
                d__.source = this.<>3__source;
                d__.name = this.<>3__name;
                return d__;
            }

            [DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Xml.Linq.XElement>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 1:
                    case 2:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally2e();
                        }
                        return;
                }
            }

            XElement IEnumerator<XElement>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <Nodes>d__0<T> : IEnumerable<XNode>, IEnumerable, IEnumerator<XNode>, IEnumerator, IDisposable where T: XContainer
        {
            private int <>1__state;
            private XNode <>2__current;
            public IEnumerable<T> <>3__source;
            public IEnumerator<T> <>7__wrap3;
            private int <>l__initialThreadId;
            public XNode <n>5__2;
            public XContainer <root>5__1;
            public IEnumerable<T> source;

            [DebuggerHidden]
            public <Nodes>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally4()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap3 != null)
                {
                    this.<>7__wrap3.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            if (this.source == null)
                            {
                                throw new ArgumentNullException("source");
                            }
                            break;

                        case 2:
                            goto Label_00B4;

                        default:
                            goto Label_00F7;
                    }
                    this.<>7__wrap3 = this.source.GetEnumerator();
                    this.<>1__state = 1;
                    while (this.<>7__wrap3.MoveNext())
                    {
                        this.<root>5__1 = this.<>7__wrap3.Current;
                        if (this.<root>5__1 == null)
                        {
                            continue;
                        }
                        this.<n>5__2 = this.<root>5__1.LastNode;
                        if (this.<n>5__2 == null)
                        {
                            continue;
                        }
                    Label_008C:
                        this.<n>5__2 = this.<n>5__2.next;
                        this.<>2__current = this.<n>5__2;
                        this.<>1__state = 2;
                        return true;
                    Label_00B4:
                        this.<>1__state = 1;
                        if ((this.<n>5__2.parent == this.<root>5__1) && (this.<n>5__2 != this.<root>5__1.content))
                        {
                            goto Label_008C;
                        }
                    }
                    this.<>m__Finally4();
                Label_00F7:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<XNode> IEnumerable<XNode>.GetEnumerator()
            {
                Extensions.<Nodes>d__0<T> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Extensions.<Nodes>d__0<T>) this;
                }
                else
                {
                    d__ = new Extensions.<Nodes>d__0<T>(0);
                }
                d__.source = this.<>3__source;
                return d__;
            }

            [DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Xml.Linq.XNode>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 1:
                    case 2:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally4();
                        }
                        return;
                }
            }

            XNode IEnumerator<XNode>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}

