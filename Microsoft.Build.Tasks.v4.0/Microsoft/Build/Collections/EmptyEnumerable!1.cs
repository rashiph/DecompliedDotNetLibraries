namespace Microsoft.Build.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal class EmptyEnumerable<T> : IEnumerable<T>, IEnumerable
    {
        private static Microsoft.Build.Collections.EmptyEnumerable<T> instance;

        private EmptyEnumerable()
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new <GetEnumerator>d__0<T>(0) { <>4__this = (Microsoft.Build.Collections.EmptyEnumerable<T>) this };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public static Microsoft.Build.Collections.EmptyEnumerable<T> Instance
        {
            get
            {
                if (Microsoft.Build.Collections.EmptyEnumerable<T>.instance == null)
                {
                    Microsoft.Build.Collections.EmptyEnumerable<T>.instance = new Microsoft.Build.Collections.EmptyEnumerable<T>();
                }
                return Microsoft.Build.Collections.EmptyEnumerable<T>.instance;
            }
        }

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__0 : IEnumerator<T>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private T <>2__current;
            public Microsoft.Build.Collections.EmptyEnumerable<T> <>4__this;

            [DebuggerHidden]
            public <GetEnumerator>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private bool MoveNext()
            {
                if (this.<>1__state == 0)
                {
                    this.<>1__state = -1;
                }
                return false;
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

