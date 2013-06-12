namespace System.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Runtime.Remoting;

    public class DynamicMetaObject
    {
        private readonly System.Linq.Expressions.Expression _expression;
        private readonly bool _hasValue;
        private readonly BindingRestrictions _restrictions;
        private readonly object _value;
        public static readonly DynamicMetaObject[] EmptyMetaObjects = new DynamicMetaObject[0];

        public DynamicMetaObject(System.Linq.Expressions.Expression expression, BindingRestrictions restrictions)
        {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(restrictions, "restrictions");
            this._expression = expression;
            this._restrictions = restrictions;
        }

        public DynamicMetaObject(System.Linq.Expressions.Expression expression, BindingRestrictions restrictions, object value) : this(expression, restrictions)
        {
            this._value = value;
            this._hasValue = true;
        }

        public virtual DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackBinaryOperation(this, arg);
        }

        public virtual DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackConvert(this);
        }

        public virtual DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackCreateInstance(this, args);
        }

        public virtual DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackDeleteIndex(this, indexes);
        }

        public virtual DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackDeleteMember(this);
        }

        public virtual DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetIndex(this, indexes);
        }

        public virtual DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetMember(this);
        }

        public virtual DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvoke(this, args);
        }

        public virtual DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvokeMember(this, args);
        }

        public virtual DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetIndex(this, indexes, value);
        }

        public virtual DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetMember(this, value);
        }

        public virtual DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackUnaryOperation(this);
        }

        public static DynamicMetaObject Create(object value, System.Linq.Expressions.Expression expression)
        {
            ContractUtils.RequiresNotNull(expression, "expression");
            IDynamicMetaObjectProvider provider = value as IDynamicMetaObjectProvider;
            if ((provider == null) || RemotingServices.IsObjectOutOfAppDomain(value))
            {
                return new DynamicMetaObject(expression, BindingRestrictions.Empty, value);
            }
            DynamicMetaObject metaObject = provider.GetMetaObject(expression);
            if (((metaObject == null) || !metaObject.HasValue) || ((metaObject.Value == null) || (metaObject.Expression != expression)))
            {
                throw Error.InvalidMetaObjectCreated(provider.GetType());
            }
            return metaObject;
        }

        public virtual IEnumerable<string> GetDynamicMemberNames()
        {
            return new string[0];
        }

        internal static System.Linq.Expressions.Expression[] GetExpressions(DynamicMetaObject[] objects)
        {
            ContractUtils.RequiresNotNull(objects, "objects");
            System.Linq.Expressions.Expression[] expressionArray = new System.Linq.Expressions.Expression[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                DynamicMetaObject obj2 = objects[i];
                ContractUtils.RequiresNotNull(obj2, "objects");
                System.Linq.Expressions.Expression expression = obj2.Expression;
                ContractUtils.RequiresNotNull(expression, "objects");
                expressionArray[i] = expression;
            }
            return expressionArray;
        }

        public System.Linq.Expressions.Expression Expression
        {
            get
            {
                return this._expression;
            }
        }

        public bool HasValue
        {
            get
            {
                return this._hasValue;
            }
        }

        public Type LimitType
        {
            get
            {
                return (this.RuntimeType ?? this.Expression.Type);
            }
        }

        public BindingRestrictions Restrictions
        {
            get
            {
                return this._restrictions;
            }
        }

        public Type RuntimeType
        {
            get
            {
                if (this._hasValue)
                {
                    Type type = this.Expression.Type;
                    if (type.IsValueType)
                    {
                        return type;
                    }
                    if (this._value != null)
                    {
                        return this._value.GetType();
                    }
                }
                return null;
            }
        }

        public object Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

