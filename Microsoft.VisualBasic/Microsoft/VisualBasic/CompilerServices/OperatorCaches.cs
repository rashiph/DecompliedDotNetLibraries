namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    internal class OperatorCaches
    {
        internal static readonly FixedList ConversionCache = new FixedList();
        internal static readonly FixedExistanceList UnconvertibleTypeCache = new FixedExistanceList();

        private OperatorCaches()
        {
        }

        internal sealed class FixedExistanceList
        {
            private const int DefaultSize = 50;
            private int m_Count;
            private int m_First;
            private int m_Last;
            private readonly Entry[] m_List;
            private readonly int m_Size;

            internal FixedExistanceList() : this(50)
            {
            }

            internal FixedExistanceList(int Size)
            {
                this.m_Size = Size;
                this.m_List = new Entry[(this.m_Size - 1) + 1];
                int num3 = this.m_Size - 2;
                for (int i = 0; i <= num3; i++)
                {
                    this.m_List[i].Next = i + 1;
                }
                for (int j = this.m_Size - 1; j >= 1; j += -1)
                {
                    this.m_List[j].Previous = j - 1;
                }
                this.m_List[0].Previous = this.m_Size - 1;
                this.m_Last = this.m_Size - 1;
            }

            internal void Insert(Type Type)
            {
                if (this.m_Count < this.m_Size)
                {
                    this.m_Count++;
                }
                int last = this.m_Last;
                this.m_First = last;
                this.m_Last = this.m_List[this.m_Last].Previous;
                this.m_List[last].Type = Type;
            }

            internal bool Lookup(Type Type)
            {
                int first = this.m_First;
                for (int i = 0; i < this.m_Count; i++)
                {
                    if (Type == this.m_List[first].Type)
                    {
                        this.MoveToFront(first);
                        return true;
                    }
                    first = this.m_List[first].Next;
                }
                return false;
            }

            private void MoveToFront(int Item)
            {
                if (Item != this.m_First)
                {
                    int next = this.m_List[Item].Next;
                    int previous = this.m_List[Item].Previous;
                    this.m_List[previous].Next = next;
                    this.m_List[next].Previous = previous;
                    this.m_List[this.m_First].Previous = Item;
                    this.m_List[this.m_Last].Next = Item;
                    this.m_List[Item].Next = this.m_First;
                    this.m_List[Item].Previous = this.m_Last;
                    this.m_First = Item;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct Entry
            {
                internal System.Type Type;
                internal int Next;
                internal int Previous;
            }
        }

        internal sealed class FixedList
        {
            private const int DefaultSize = 50;
            private int m_Count;
            private int m_First;
            private int m_Last;
            private readonly Entry[] m_List;
            private readonly int m_Size;

            internal FixedList() : this(50)
            {
            }

            internal FixedList(int Size)
            {
                this.m_Size = Size;
                this.m_List = new Entry[(this.m_Size - 1) + 1];
                int num3 = this.m_Size - 2;
                for (int i = 0; i <= num3; i++)
                {
                    this.m_List[i].Next = i + 1;
                }
                for (int j = this.m_Size - 1; j >= 1; j += -1)
                {
                    this.m_List[j].Previous = j - 1;
                }
                this.m_List[0].Previous = this.m_Size - 1;
                this.m_Last = this.m_Size - 1;
            }

            internal void Insert(Type TargetType, Type SourceType, ConversionResolution.ConversionClass Classification, Symbols.Method OperatorMethod)
            {
                if (this.m_Count < this.m_Size)
                {
                    this.m_Count++;
                }
                int last = this.m_Last;
                this.m_First = last;
                this.m_Last = this.m_List[this.m_Last].Previous;
                this.m_List[last].TargetType = TargetType;
                this.m_List[last].SourceType = SourceType;
                this.m_List[last].Classification = Classification;
                this.m_List[last].OperatorMethod = OperatorMethod;
            }

            internal bool Lookup(Type TargetType, Type SourceType, ref ConversionResolution.ConversionClass Classification, ref Symbols.Method OperatorMethod)
            {
                int first = this.m_First;
                for (int i = 0; i < this.m_Count; i++)
                {
                    if ((TargetType == this.m_List[first].TargetType) && (SourceType == this.m_List[first].SourceType))
                    {
                        Classification = this.m_List[first].Classification;
                        OperatorMethod = this.m_List[first].OperatorMethod;
                        this.MoveToFront(first);
                        return true;
                    }
                    first = this.m_List[first].Next;
                }
                Classification = ConversionResolution.ConversionClass.Bad;
                OperatorMethod = null;
                return false;
            }

            private void MoveToFront(int Item)
            {
                if (Item != this.m_First)
                {
                    int next = this.m_List[Item].Next;
                    int previous = this.m_List[Item].Previous;
                    this.m_List[previous].Next = next;
                    this.m_List[next].Previous = previous;
                    this.m_List[this.m_First].Previous = Item;
                    this.m_List[this.m_Last].Next = Item;
                    this.m_List[Item].Next = this.m_First;
                    this.m_List[Item].Previous = this.m_Last;
                    this.m_First = Item;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct Entry
            {
                internal Type TargetType;
                internal Type SourceType;
                internal ConversionResolution.ConversionClass Classification;
                internal Symbols.Method OperatorMethod;
                internal int Next;
                internal int Previous;
            }
        }
    }
}

