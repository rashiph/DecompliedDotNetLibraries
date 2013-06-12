namespace System
{
    public interface IComparable<in T>
    {
        int CompareTo(T other);
    }
}

