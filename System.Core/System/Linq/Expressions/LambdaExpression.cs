namespace System.Linq.Expressions
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Linq.Expressions.Compiler;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    [DebuggerTypeProxy(typeof(Expression.LambdaExpressionProxy))]
    public abstract class LambdaExpression : Expression
    {
        private readonly Expression _body;
        private readonly System.Type _delegateType;
        private readonly string _name;
        private readonly ReadOnlyCollection<ParameterExpression> _parameters;
        private readonly bool _tailCall;

        internal LambdaExpression(System.Type delegateType, string name, Expression body, bool tailCall, ReadOnlyCollection<ParameterExpression> parameters)
        {
            this._name = name;
            this._body = body;
            this._parameters = parameters;
            this._delegateType = delegateType;
            this._tailCall = tailCall;
        }

        internal abstract LambdaExpression Accept(StackSpiller spiller);
        public Delegate Compile()
        {
            return LambdaCompiler.Compile(this, null);
        }

        public Delegate Compile(DebugInfoGenerator debugInfoGenerator)
        {
            ContractUtils.RequiresNotNull(debugInfoGenerator, "debugInfoGenerator");
            return LambdaCompiler.Compile(this, debugInfoGenerator);
        }

        public void CompileToMethod(MethodBuilder method)
        {
            this.CompileToMethodInternal(method, null);
        }

        public void CompileToMethod(MethodBuilder method, DebugInfoGenerator debugInfoGenerator)
        {
            ContractUtils.RequiresNotNull(debugInfoGenerator, "debugInfoGenerator");
            this.CompileToMethodInternal(method, debugInfoGenerator);
        }

        private void CompileToMethodInternal(MethodBuilder method, DebugInfoGenerator debugInfoGenerator)
        {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.Requires(method.IsStatic, "method");
            TypeBuilder declaringType = method.DeclaringType as TypeBuilder;
            if (declaringType == null)
            {
                throw Error.MethodBuilderDoesNotHaveTypeBuilder();
            }
            LambdaCompiler.Compile(this, method, debugInfoGenerator);
        }

        public Expression Body
        {
            get
            {
                return this._body;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Lambda;
            }
        }

        public ReadOnlyCollection<ParameterExpression> Parameters
        {
            get
            {
                return this._parameters;
            }
        }

        public System.Type ReturnType
        {
            get
            {
                return this.Type.GetMethod("Invoke").ReturnType;
            }
        }

        public bool TailCall
        {
            get
            {
                return this._tailCall;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._delegateType;
            }
        }
    }
}

