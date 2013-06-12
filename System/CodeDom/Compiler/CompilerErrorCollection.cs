namespace System.CodeDom.Compiler
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;

    [Serializable, PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class CompilerErrorCollection : CollectionBase
    {
        public CompilerErrorCollection()
        {
        }

        public CompilerErrorCollection(CompilerErrorCollection value)
        {
            this.AddRange(value);
        }

        public CompilerErrorCollection(CompilerError[] value)
        {
            this.AddRange(value);
        }

        public int Add(CompilerError value)
        {
            return base.List.Add(value);
        }

        public void AddRange(CompilerError[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(CompilerErrorCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int count = value.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(CompilerError value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(CompilerError[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(CompilerError value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, CompilerError value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(CompilerError value)
        {
            base.List.Remove(value);
        }

        public bool HasErrors
        {
            get
            {
                if (base.Count > 0)
                {
                    foreach (CompilerError error in this)
                    {
                        if (!error.IsWarning)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool HasWarnings
        {
            get
            {
                if (base.Count > 0)
                {
                    foreach (CompilerError error in this)
                    {
                        if (error.IsWarning)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public CompilerError this[int index]
        {
            get
            {
                return (CompilerError) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

