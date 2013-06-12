namespace System.Dynamic
{
    using System.Linq.Expressions;

    public interface IDynamicMetaObjectProvider
    {
        DynamicMetaObject GetMetaObject(Expression parameter);
    }
}

