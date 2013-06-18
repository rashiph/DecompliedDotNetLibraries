namespace Microsoft.JScript
{
    using System;
    using System.Collections;

    internal class DebugArrayFieldEnumerator
    {
        private ArrayObject arrayObject;
        private int count;
        private ScriptObjectPropertyEnumerator enumerator;

        internal DebugArrayFieldEnumerator(ScriptObjectPropertyEnumerator enumerator, ArrayObject arrayObject)
        {
            this.enumerator = enumerator;
            this.arrayObject = arrayObject;
            this.EnsureCount();
        }

        internal void EnsureCount()
        {
            this.enumerator.Reset();
            this.count = 0;
            while (this.enumerator.MoveNext())
            {
                this.count++;
            }
            this.enumerator.Reset();
        }

        internal int GetCount()
        {
            return this.count;
        }

        internal DynamicFieldInfo[] Next(int count)
        {
            try
            {
                ArrayList list = new ArrayList();
                while ((count > 0) && this.enumerator.MoveNext())
                {
                    string current = (string) this.enumerator.Current;
                    list.Add(new DynamicFieldInfo(current, this.arrayObject.GetMemberValue(current)));
                    count--;
                }
                DynamicFieldInfo[] array = new DynamicFieldInfo[list.Count];
                list.CopyTo(array);
                return array;
            }
            catch
            {
                return new DynamicFieldInfo[0];
            }
        }

        internal void Reset()
        {
            this.enumerator.Reset();
        }

        internal void Skip(int count)
        {
            while ((count > 0) && this.enumerator.MoveNext())
            {
                count--;
            }
        }
    }
}

