namespace System.Xml.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Xml;

    public abstract class XObject : IXmlLineInfo
    {
        internal object annotations;
        internal XContainer parent;

        public event EventHandler<XObjectChangeEventArgs> Changed
        {
            add
            {
                if (value != null)
                {
                    XObjectChangeAnnotation annotation = this.Annotation<XObjectChangeAnnotation>();
                    if (annotation == null)
                    {
                        annotation = new XObjectChangeAnnotation();
                        this.AddAnnotation(annotation);
                    }
                    annotation.changed = (EventHandler<XObjectChangeEventArgs>) Delegate.Combine(annotation.changed, value);
                }
            }
            remove
            {
                if (value != null)
                {
                    XObjectChangeAnnotation annotation = this.Annotation<XObjectChangeAnnotation>();
                    if (annotation != null)
                    {
                        annotation.changed = (EventHandler<XObjectChangeEventArgs>) Delegate.Remove(annotation.changed, value);
                        if ((annotation.changing == null) && (annotation.changed == null))
                        {
                            this.RemoveAnnotations<XObjectChangeAnnotation>();
                        }
                    }
                }
            }
        }

        public event EventHandler<XObjectChangeEventArgs> Changing
        {
            add
            {
                if (value != null)
                {
                    XObjectChangeAnnotation annotation = this.Annotation<XObjectChangeAnnotation>();
                    if (annotation == null)
                    {
                        annotation = new XObjectChangeAnnotation();
                        this.AddAnnotation(annotation);
                    }
                    annotation.changing = (EventHandler<XObjectChangeEventArgs>) Delegate.Combine(annotation.changing, value);
                }
            }
            remove
            {
                if (value != null)
                {
                    XObjectChangeAnnotation annotation = this.Annotation<XObjectChangeAnnotation>();
                    if (annotation != null)
                    {
                        annotation.changing = (EventHandler<XObjectChangeEventArgs>) Delegate.Remove(annotation.changing, value);
                        if ((annotation.changing == null) && (annotation.changed == null))
                        {
                            this.RemoveAnnotations<XObjectChangeAnnotation>();
                        }
                    }
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal XObject()
        {
        }

        public void AddAnnotation(object annotation)
        {
            if (annotation == null)
            {
                throw new ArgumentNullException("annotation");
            }
            if (this.annotations == null)
            {
                this.annotations = (annotation is object[]) ? new object[] { annotation } : annotation;
            }
            else
            {
                object[] annotations = this.annotations as object[];
                if (annotations == null)
                {
                    this.annotations = new object[] { this.annotations, annotation };
                }
                else
                {
                    int index = 0;
                    while ((index < annotations.Length) && (annotations[index] != null))
                    {
                        index++;
                    }
                    if (index == annotations.Length)
                    {
                        Array.Resize<object>(ref annotations, index * 2);
                        this.annotations = annotations;
                    }
                    annotations[index] = annotation;
                }
            }
        }

        public T Annotation<T>() where T: class
        {
            if (this.annotations != null)
            {
                object[] annotations = this.annotations as object[];
                if (annotations == null)
                {
                    return (this.annotations as T);
                }
                for (int i = 0; i < annotations.Length; i++)
                {
                    object obj2 = annotations[i];
                    if (obj2 == null)
                    {
                        break;
                    }
                    T local = obj2 as T;
                    if (local != null)
                    {
                        return local;
                    }
                }
            }
            return default(T);
        }

        public object Annotation(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (this.annotations != null)
            {
                object[] annotations = this.annotations as object[];
                if (annotations == null)
                {
                    if (type.IsInstanceOfType(this.annotations))
                    {
                        return this.annotations;
                    }
                }
                else
                {
                    for (int i = 0; i < annotations.Length; i++)
                    {
                        object o = annotations[i];
                        if (o == null)
                        {
                            break;
                        }
                        if (type.IsInstanceOfType(o))
                        {
                            return o;
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<T> Annotations<T>() where T: class
        {
            if (this.annotations == null)
            {
                yield break;
            }
            object[] annotations = this.annotations as object[];
            if (annotations != null)
            {
                for (int i = 0; i < annotations.Length; i++)
                {
                    object iteratorVariable3 = annotations[i];
                    if (iteratorVariable3 == null)
                    {
                        break;
                    }
                    T iteratorVariable4 = iteratorVariable3 as T;
                    if (iteratorVariable4 != null)
                    {
                        yield return iteratorVariable4;
                    }
                }
                yield break;
            }
            T iteratorVariable1 = this.annotations as T;
            if (iteratorVariable1 == null)
            {
                yield break;
            }
            yield return iteratorVariable1;
        }

        public IEnumerable<object> Annotations(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return this.AnnotationsIterator(type);
        }

        private IEnumerable<object> AnnotationsIterator(Type type)
        {
            if (this.annotations == null)
            {
                yield break;
            }
            object[] annotations = this.annotations as object[];
            if (annotations != null)
            {
                for (int i = 0; i < annotations.Length; i++)
                {
                    object o = annotations[i];
                    if (o == null)
                    {
                        break;
                    }
                    if (type.IsInstanceOfType(o))
                    {
                        yield return o;
                    }
                }
                yield break;
            }
            if (!type.IsInstanceOfType(this.annotations))
            {
                yield break;
            }
            yield return this.annotations;
        }

        internal SaveOptions GetSaveOptionsFromAnnotations()
        {
            XObject parent = this;
            while (true)
            {
                while ((parent != null) && (parent.annotations == null))
                {
                    parent = parent.parent;
                }
                if (parent == null)
                {
                    return SaveOptions.None;
                }
                object obj3 = parent.Annotation(typeof(SaveOptions));
                if (obj3 != null)
                {
                    return (SaveOptions) obj3;
                }
                parent = parent.parent;
            }
        }

        internal bool NotifyChanged(object sender, XObjectChangeEventArgs e)
        {
            bool flag = false;
            XObject parent = this;
            while (true)
            {
                while ((parent != null) && (parent.annotations == null))
                {
                    parent = parent.parent;
                }
                if (parent == null)
                {
                    return flag;
                }
                XObjectChangeAnnotation annotation = parent.Annotation<XObjectChangeAnnotation>();
                if (annotation != null)
                {
                    flag = true;
                    if (annotation.changed != null)
                    {
                        annotation.changed(sender, e);
                    }
                }
                parent = parent.parent;
            }
        }

        internal bool NotifyChanging(object sender, XObjectChangeEventArgs e)
        {
            bool flag = false;
            XObject parent = this;
            while (true)
            {
                while ((parent != null) && (parent.annotations == null))
                {
                    parent = parent.parent;
                }
                if (parent == null)
                {
                    return flag;
                }
                XObjectChangeAnnotation annotation = parent.Annotation<XObjectChangeAnnotation>();
                if (annotation != null)
                {
                    flag = true;
                    if (annotation.changing != null)
                    {
                        annotation.changing(sender, e);
                    }
                }
                parent = parent.parent;
            }
        }

        public void RemoveAnnotations<T>() where T: class
        {
            if (this.annotations != null)
            {
                object[] annotations = this.annotations as object[];
                if (annotations == null)
                {
                    if (this.annotations is T)
                    {
                        this.annotations = null;
                    }
                }
                else
                {
                    int index = 0;
                    int num2 = 0;
                    while (index < annotations.Length)
                    {
                        object obj2 = annotations[index];
                        if (obj2 == null)
                        {
                            break;
                        }
                        if (!(obj2 is T))
                        {
                            annotations[num2++] = obj2;
                        }
                        index++;
                    }
                    if (num2 != 0)
                    {
                        while (num2 < index)
                        {
                            annotations[num2++] = null;
                        }
                    }
                    else
                    {
                        this.annotations = null;
                    }
                }
            }
        }

        public void RemoveAnnotations(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (this.annotations != null)
            {
                object[] annotations = this.annotations as object[];
                if (annotations == null)
                {
                    if (type.IsInstanceOfType(this.annotations))
                    {
                        this.annotations = null;
                    }
                }
                else
                {
                    int index = 0;
                    int num2 = 0;
                    while (index < annotations.Length)
                    {
                        object o = annotations[index];
                        if (o == null)
                        {
                            break;
                        }
                        if (!type.IsInstanceOfType(o))
                        {
                            annotations[num2++] = o;
                        }
                        index++;
                    }
                    if (num2 != 0)
                    {
                        while (num2 < index)
                        {
                            annotations[num2++] = null;
                        }
                    }
                    else
                    {
                        this.annotations = null;
                    }
                }
            }
        }

        internal void SetBaseUri(string baseUri)
        {
            this.AddAnnotation(new BaseUriAnnotation(baseUri));
        }

        internal void SetLineInfo(int lineNumber, int linePosition)
        {
            this.AddAnnotation(new LineInfoAnnotation(lineNumber, linePosition));
        }

        internal bool SkipNotify()
        {
            XObject parent = this;
            while (true)
            {
                while ((parent != null) && (parent.annotations == null))
                {
                    parent = parent.parent;
                }
                if (parent == null)
                {
                    return true;
                }
                if (parent.Annotations<XObjectChangeAnnotation>() != null)
                {
                    return false;
                }
                parent = parent.parent;
            }
        }

        bool IXmlLineInfo.HasLineInfo()
        {
            return (this.Annotation<LineInfoAnnotation>() != null);
        }

        public string BaseUri
        {
            get
            {
                XObject parent = this;
                while (true)
                {
                    while ((parent != null) && (parent.annotations == null))
                    {
                        parent = parent.parent;
                    }
                    if (parent == null)
                    {
                        return string.Empty;
                    }
                    BaseUriAnnotation annotation = parent.Annotation<BaseUriAnnotation>();
                    if (annotation != null)
                    {
                        return annotation.baseUri;
                    }
                    parent = parent.parent;
                }
            }
        }

        public XDocument Document
        {
            get
            {
                XObject parent = this;
                while (parent.parent != null)
                {
                    parent = parent.parent;
                }
                return (parent as XDocument);
            }
        }

        internal bool HasBaseUri
        {
            get
            {
                return (this.Annotation<BaseUriAnnotation>() != null);
            }
        }

        public abstract XmlNodeType NodeType { get; }

        public XElement Parent
        {
            get
            {
                return (this.parent as XElement);
            }
        }

        int IXmlLineInfo.LineNumber
        {
            get
            {
                LineInfoAnnotation annotation = this.Annotation<LineInfoAnnotation>();
                if (annotation != null)
                {
                    return annotation.lineNumber;
                }
                return 0;
            }
        }

        int IXmlLineInfo.LinePosition
        {
            get
            {
                LineInfoAnnotation annotation = this.Annotation<LineInfoAnnotation>();
                if (annotation != null)
                {
                    return annotation.linePosition;
                }
                return 0;
            }
        }

        [CompilerGenerated]
        private sealed class <Annotations>d__6<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T: class
        {
            private int <>1__state;
            private T <>2__current;
            public XObject <>4__this;
            private int <>l__initialThreadId;
            public object[] <a>5__7;
            public int <i>5__9;
            public object <obj>5__a;
            public T <result>5__8;
            public T <result>5__b;

            [DebuggerHidden]
            public <Annotations>d__6(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        if (this.<>4__this.annotations == null)
                        {
                            break;
                        }
                        this.<a>5__7 = this.<>4__this.annotations as object[];
                        if (this.<a>5__7 != null)
                        {
                            this.<i>5__9 = 0;
                            while (this.<i>5__9 < this.<a>5__7.Length)
                            {
                                this.<obj>5__a = this.<a>5__7[this.<i>5__9];
                                if (this.<obj>5__a == null)
                                {
                                    break;
                                }
                                this.<result>5__b = this.<obj>5__a as T;
                                if (this.<result>5__b == null)
                                {
                                    goto Label_0102;
                                }
                                this.<>2__current = this.<result>5__b;
                                this.<>1__state = 2;
                                return true;
                            Label_00FB:
                                this.<>1__state = -1;
                            Label_0102:
                                this.<i>5__9++;
                            }
                            break;
                        }
                        this.<result>5__8 = this.<>4__this.annotations as T;
                        if (this.<result>5__8 == null)
                        {
                            break;
                        }
                        this.<>2__current = this.<result>5__8;
                        this.<>1__state = 1;
                        return true;

                    case 1:
                        this.<>1__state = -1;
                        break;

                    case 2:
                        goto Label_00FB;
                }
                return false;
            }

            [DebuggerHidden]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    return (XObject.<Annotations>d__6<T>) this;
                }
                return new XObject.<Annotations>d__6<T>(0) { <>4__this = this.<>4__this };
            }

            [DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            T IEnumerator<T>.Current
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

