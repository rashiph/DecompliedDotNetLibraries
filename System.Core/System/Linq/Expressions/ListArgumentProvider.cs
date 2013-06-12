namespace System.Linq.Expressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class ListArgumentProvider : IList<Expression>, ICollection<Expression>, IEnumerable<Expression>, IEnumerable
    {
        private readonly Expression _arg0;
        private readonly IArgumentProvider _provider;

        internal ListArgumentProvider(IArgumentProvider provider, Expression arg0)
        {
            this._provider = provider;
            this._arg0 = arg0;
        }

        public void Add(Expression item)
        {
            throw ContractUtils.Unreachable;
        }

        public void Clear()
        {
            throw ContractUtils.Unreachable;
        }

        public bool Contains(Expression item)
        {
            return (this.IndexOf(item) != -1);
        }

        public void CopyTo(Expression[] array, int arrayIndex)
        {
            array[arrayIndex++] = this._arg0;
            for (int i = 1; i < this._provider.ArgumentCount; i++)
            {
                array[arrayIndex++] = this._provider.GetArgument(i);
            }
        }

        public IEnumerator<Expression> GetEnumerator()
        {
            yield return this._arg0;
            int index = 1;
            while (true)
            {
                if (index >= this._provider.ArgumentCount)
                {
                    yield break;
                }
                yield return this._provider.GetArgument(index);
                index++;
            }
        }

        public int IndexOf(Expression item)
        {
            if (this._arg0 == item)
            {
                return 0;
            }
            for (int i = 1; i < this._provider.ArgumentCount; i++)
            {
                if (this._provider.GetArgument(i) == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, Expression item)
        {
            throw ContractUtils.Unreachable;
        }

        public bool Remove(Expression item)
        {
            throw ContractUtils.Unreachable;
        }

        public void RemoveAt(int index)
        {
            throw ContractUtils.Unreachable;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this._arg0;
            int index = 1;
            while (true)
            {
                if (index >= this._provider.ArgumentCount)
                {
                    yield break;
                }
                yield return this._provider.GetArgument(index);
                index++;
            }
        }

        public int Count
        {
            get
            {
                return this._provider.ArgumentCount;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public Expression this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return this._arg0;
                }
                return this._provider.GetArgument(index);
            }
            set
            {
                throw ContractUtils.Unreachable;
            }
        }


    }
}

