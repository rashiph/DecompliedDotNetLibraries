namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class ScriptObjectPropertyEnumerator : IEnumerator
    {
        private ArrayList enumerators;
        private int index;
        private ArrayList objects;
        private SimpleHashtable visited_names;

        internal ScriptObjectPropertyEnumerator(ScriptObject obj)
        {
            obj.GetPropertyEnumerator(this.enumerators = new ArrayList(), this.objects = new ArrayList());
            this.index = 0;
            this.visited_names = new SimpleHashtable(0x10);
        }

        public virtual bool MoveNext()
        {
            string name;
            if (this.index >= this.enumerators.Count)
            {
                return false;
            }
            IEnumerator enumerator = (IEnumerator) this.enumerators[this.index];
            if (!enumerator.MoveNext())
            {
                this.index++;
                return this.MoveNext();
            }
            object current = enumerator.Current;
            FieldInfo info = current as FieldInfo;
            if (info != null)
            {
                JSPrototypeField field = current as JSPrototypeField;
                if ((field != null) && (field.value is Microsoft.JScript.Missing))
                {
                    return this.MoveNext();
                }
                name = info.Name;
                if (info.GetValue(this.objects[this.index]) is Microsoft.JScript.Missing)
                {
                    return this.MoveNext();
                }
            }
            else if (current is string)
            {
                name = (string) current;
            }
            else if (current is MemberInfo)
            {
                name = ((MemberInfo) current).Name;
            }
            else
            {
                name = current.ToString();
            }
            if (this.visited_names[name] != null)
            {
                return this.MoveNext();
            }
            this.visited_names[name] = name;
            return true;
        }

        public virtual void Reset()
        {
            this.index = 0;
            foreach (IEnumerator enumerator in this.enumerators)
            {
                enumerator.Reset();
            }
            this.visited_names = new SimpleHashtable(0x10);
        }

        public virtual object Current
        {
            get
            {
                object current = ((IEnumerator) this.enumerators[this.index]).Current;
                if (current is MemberInfo)
                {
                    return ((MemberInfo) current).Name;
                }
                return current.ToString();
            }
        }
    }
}

