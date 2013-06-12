namespace System.Linq.Expressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class BlockExpressionList : IList<Expression>, ICollection<Expression>, IEnumerable<Expression>, IEnumerable
    {
        private readonly Expression _arg0;
        private readonly BlockExpression _block;

        internal BlockExpressionList(BlockExpression provider, Expression arg0)
        {
            this._block = provider;
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
            for (int i = 1; i < this._block.ExpressionCount; i++)
            {
                array[arrayIndex++] = this._block.GetExpression(i);
            }
        }

        public IEnumerator<Expression> GetEnumerator()
        {
            yield return this._arg0;
            int index = 1;
            while (true)
            {
                if (index >= this._block.ExpressionCount)
                {
                    yield break;
                }
                yield return this._block.GetExpression(index);
                index++;
            }
        }

        public int IndexOf(Expression item)
        {
            if (this._arg0 == item)
            {
                return 0;
            }
            for (int i = 1; i < this._block.ExpressionCount; i++)
            {
                if (this._block.GetExpression(i) == item)
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
                if (index >= this._block.ExpressionCount)
                {
                    yield break;
                }
                yield return this._block.GetExpression(index);
                index++;
            }
        }

        public int Count
        {
            get
            {
                return this._block.ExpressionCount;
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
                return this._block.GetExpression(index);
            }
            set
            {
                throw ContractUtils.Unreachable;
            }
        }


    }
}

