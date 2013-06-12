namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    internal sealed class CompilerScope
    {
        private HoistedLocals _closureHoistedLocals;
        private HoistedLocals _hoistedLocals;
        private readonly Dictionary<ParameterExpression, Storage> _locals = new Dictionary<ParameterExpression, Storage>();
        private CompilerScope _parent;
        internal readonly Dictionary<ParameterExpression, VariableStorageKind> Definitions = new Dictionary<ParameterExpression, VariableStorageKind>();
        internal readonly bool IsMethod;
        internal Set<object> MergedScopes;
        internal bool NeedsClosure;
        internal readonly object Node;
        internal Dictionary<ParameterExpression, int> ReferenceCount;

        internal CompilerScope(object node, bool isMethod)
        {
            this.Node = node;
            this.IsMethod = isMethod;
            IList<ParameterExpression> variables = GetVariables(node);
            this.Definitions = new Dictionary<ParameterExpression, VariableStorageKind>(variables.Count);
            foreach (ParameterExpression expression in variables)
            {
                this.Definitions.Add(expression, VariableStorageKind.Local);
            }
        }

        internal void AddLocal(LambdaCompiler gen, ParameterExpression variable)
        {
            this._locals.Add(variable, new LocalStorage(gen, variable));
        }

        private void AllocateLocals(LambdaCompiler lc)
        {
            foreach (ParameterExpression expression in this.GetVariables())
            {
                if (((VariableStorageKind) this.Definitions[expression]) == VariableStorageKind.Local)
                {
                    Storage storage;
                    if (this.IsMethod && lc.Parameters.Contains(expression))
                    {
                        storage = new ArgumentStorage(lc, expression);
                    }
                    else
                    {
                        storage = new LocalStorage(lc, expression);
                    }
                    this._locals.Add(expression, storage);
                }
            }
        }

        private void CacheBoxToLocal(LambdaCompiler lc, ParameterExpression v)
        {
            LocalBoxStorage storage = new LocalBoxStorage(lc, v);
            storage.EmitStoreBox();
            this._locals.Add(v, storage);
        }

        internal void EmitAddressOf(ParameterExpression variable)
        {
            this.ResolveVariable(variable).EmitAddress();
        }

        private void EmitCachedVariables()
        {
            if (this.ReferenceCount != null)
            {
                foreach (KeyValuePair<ParameterExpression, int> pair in this.ReferenceCount)
                {
                    if (this.ShouldCache(pair.Key, pair.Value))
                    {
                        ElementBoxStorage storage = this.ResolveVariable(pair.Key) as ElementBoxStorage;
                        if (storage != null)
                        {
                            storage.EmitLoadBox();
                            this.CacheBoxToLocal(storage.Compiler, pair.Key);
                        }
                    }
                }
            }
        }

        private void EmitClosureAccess(LambdaCompiler lc, HoistedLocals locals)
        {
            if (locals != null)
            {
                this.EmitClosureToVariable(lc, locals);
                while ((locals = locals.Parent) != null)
                {
                    ParameterExpression selfVariable = locals.SelfVariable;
                    LocalStorage storage = new LocalStorage(lc, selfVariable);
                    storage.EmitStore(this.ResolveVariable(selfVariable));
                    this._locals.Add(selfVariable, storage);
                }
            }
        }

        private void EmitClosureToVariable(LambdaCompiler lc, HoistedLocals locals)
        {
            lc.EmitClosureArgument();
            lc.IL.Emit(OpCodes.Ldfld, typeof(Closure).GetField("Locals"));
            this.AddLocal(lc, locals.SelfVariable);
            this.EmitSet(locals.SelfVariable);
        }

        internal void EmitGet(ParameterExpression variable)
        {
            this.ResolveVariable(variable).EmitLoad();
        }

        private void EmitNewHoistedLocals(LambdaCompiler lc)
        {
            if (this._hoistedLocals != null)
            {
                lc.IL.EmitInt(this._hoistedLocals.Variables.Count);
                lc.IL.Emit(OpCodes.Newarr, typeof(object));
                int num = 0;
                foreach (ParameterExpression expression in this._hoistedLocals.Variables)
                {
                    lc.IL.Emit(OpCodes.Dup);
                    lc.IL.EmitInt(num++);
                    Type type = typeof(StrongBox<>).MakeGenericType(new Type[] { expression.Type });
                    if (this.IsMethod && lc.Parameters.Contains(expression))
                    {
                        int index = lc.Parameters.IndexOf(expression);
                        lc.EmitLambdaArgument(index);
                        lc.IL.Emit(OpCodes.Newobj, type.GetConstructor(new Type[] { expression.Type }));
                    }
                    else if (expression == this._hoistedLocals.ParentVariable)
                    {
                        this.ResolveVariable(expression, this._closureHoistedLocals).EmitLoad();
                        lc.IL.Emit(OpCodes.Newobj, type.GetConstructor(new Type[] { expression.Type }));
                    }
                    else
                    {
                        lc.IL.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
                    }
                    if (this.ShouldCache(expression))
                    {
                        lc.IL.Emit(OpCodes.Dup);
                        this.CacheBoxToLocal(lc, expression);
                    }
                    lc.IL.Emit(OpCodes.Stelem_Ref);
                }
                this.EmitSet(this._hoistedLocals.SelfVariable);
            }
        }

        internal void EmitSet(ParameterExpression variable)
        {
            this.ResolveVariable(variable).EmitStore();
        }

        internal void EmitVariableAccess(LambdaCompiler lc, ReadOnlyCollection<ParameterExpression> vars)
        {
            if (this.NearestHoistedLocals != null)
            {
                List<long> list = new List<long>(vars.Count);
                foreach (ParameterExpression expression in vars)
                {
                    ulong num = 0L;
                    HoistedLocals nearestHoistedLocals = this.NearestHoistedLocals;
                    while (!nearestHoistedLocals.Indexes.ContainsKey(expression))
                    {
                        num += (ulong) 1L;
                        nearestHoistedLocals = nearestHoistedLocals.Parent;
                    }
                    ulong num2 = (num << 0x20) | ((ulong) nearestHoistedLocals.Indexes[expression]);
                    list.Add((long) num2);
                }
                if (list.Count > 0)
                {
                    this.EmitGet(this.NearestHoistedLocals.SelfVariable);
                    lc.EmitConstantArray<long>(list.ToArray());
                    lc.IL.Emit(OpCodes.Call, typeof(RuntimeOps).GetMethod("CreateRuntimeVariables", new Type[] { typeof(object[]), typeof(long[]) }));
                    return;
                }
            }
            lc.IL.Emit(OpCodes.Call, typeof(RuntimeOps).GetMethod("CreateRuntimeVariables", Type.EmptyTypes));
        }

        internal CompilerScope Enter(LambdaCompiler lc, CompilerScope parent)
        {
            this.SetParent(lc, parent);
            this.AllocateLocals(lc);
            if (this.IsMethod && (this._closureHoistedLocals != null))
            {
                this.EmitClosureAccess(lc, this._closureHoistedLocals);
            }
            this.EmitNewHoistedLocals(lc);
            if (this.IsMethod)
            {
                this.EmitCachedVariables();
            }
            return this;
        }

        internal CompilerScope Exit()
        {
            if (!this.IsMethod)
            {
                foreach (Storage storage in this._locals.Values)
                {
                    storage.FreeLocal();
                }
            }
            CompilerScope scope = this._parent;
            this._parent = null;
            this._hoistedLocals = null;
            this._closureHoistedLocals = null;
            this._locals.Clear();
            return scope;
        }

        private IList<ParameterExpression> GetVariables()
        {
            IList<ParameterExpression> variables = GetVariables(this.Node);
            if (this.MergedScopes == null)
            {
                return variables;
            }
            List<ParameterExpression> list2 = new List<ParameterExpression>(variables);
            foreach (object obj2 in this.MergedScopes)
            {
                list2.AddRange(GetVariables(obj2));
            }
            return list2;
        }

        private static IList<ParameterExpression> GetVariables(object scope)
        {
            LambdaExpression expression = scope as LambdaExpression;
            if (expression != null)
            {
                return expression.Parameters;
            }
            BlockExpression expression2 = scope as BlockExpression;
            if (expression2 != null)
            {
                return expression2.Variables;
            }
            return new ParameterExpression[] { ((CatchBlock) scope).Variable };
        }

        private Storage ResolveVariable(ParameterExpression variable)
        {
            return this.ResolveVariable(variable, this.NearestHoistedLocals);
        }

        private Storage ResolveVariable(ParameterExpression variable, HoistedLocals hoistedLocals)
        {
            for (CompilerScope scope = this; scope != null; scope = scope._parent)
            {
                Storage storage;
                if (scope._locals.TryGetValue(variable, out storage))
                {
                    return storage;
                }
                if (scope.IsMethod)
                {
                    break;
                }
            }
            for (HoistedLocals locals = hoistedLocals; locals != null; locals = locals.Parent)
            {
                int num;
                if (locals.Indexes.TryGetValue(variable, out num))
                {
                    return new ElementBoxStorage(this.ResolveVariable(locals.SelfVariable, hoistedLocals), num, variable);
                }
            }
            throw Error.UndefinedVariable(variable.Name, variable.Type, this.CurrentLambdaName);
        }

        private void SetParent(LambdaCompiler lc, CompilerScope parent)
        {
            this._parent = parent;
            if (this.NeedsClosure && (this._parent != null))
            {
                this._closureHoistedLocals = this._parent.NearestHoistedLocals;
            }
            ReadOnlyCollection<ParameterExpression> vars = (from p in this.GetVariables()
                where ((VariableStorageKind) this.Definitions[p]) == VariableStorageKind.Hoisted
                select p).ToReadOnly<ParameterExpression>();
            if (vars.Count > 0)
            {
                this._hoistedLocals = new HoistedLocals(this._closureHoistedLocals, vars);
                this.AddLocal(lc, this._hoistedLocals.SelfVariable);
            }
        }

        private bool ShouldCache(ParameterExpression v)
        {
            int num;
            if (this.ReferenceCount == null)
            {
                return false;
            }
            return (this.ReferenceCount.TryGetValue(v, out num) && this.ShouldCache(v, num));
        }

        private bool ShouldCache(ParameterExpression v, int refCount)
        {
            return ((refCount > 2) && !this._locals.ContainsKey(v));
        }

        private string CurrentLambdaName
        {
            get
            {
                LambdaExpression node;
                CompilerScope scope = this;
                do
                {
                    node = scope.Node as LambdaExpression;
                }
                while (node == null);
                return node.Name;
            }
        }

        internal HoistedLocals NearestHoistedLocals
        {
            get
            {
                return (this._hoistedLocals ?? this._closureHoistedLocals);
            }
        }

        private sealed class ArgumentStorage : CompilerScope.Storage
        {
            private readonly int _argument;

            internal ArgumentStorage(LambdaCompiler compiler, ParameterExpression p) : base(compiler, p)
            {
                this._argument = compiler.GetLambdaArgument(compiler.Parameters.IndexOf(p));
            }

            internal override void EmitAddress()
            {
                base.Compiler.IL.EmitLoadArgAddress(this._argument);
            }

            internal override void EmitLoad()
            {
                base.Compiler.IL.EmitLoadArg(this._argument);
            }

            internal override void EmitStore()
            {
                base.Compiler.IL.EmitStoreArg(this._argument);
            }
        }

        private sealed class ElementBoxStorage : CompilerScope.Storage
        {
            private readonly CompilerScope.Storage _array;
            private readonly Type _boxType;
            private readonly FieldInfo _boxValueField;
            private readonly int _index;

            internal ElementBoxStorage(CompilerScope.Storage array, int index, ParameterExpression variable) : base(array.Compiler, variable)
            {
                this._array = array;
                this._index = index;
                this._boxType = typeof(StrongBox<>).MakeGenericType(new Type[] { variable.Type });
                this._boxValueField = this._boxType.GetField("Value");
            }

            internal override void EmitAddress()
            {
                this.EmitLoadBox();
                base.Compiler.IL.Emit(OpCodes.Ldflda, this._boxValueField);
            }

            internal override void EmitLoad()
            {
                this.EmitLoadBox();
                base.Compiler.IL.Emit(OpCodes.Ldfld, this._boxValueField);
            }

            internal void EmitLoadBox()
            {
                this._array.EmitLoad();
                base.Compiler.IL.EmitInt(this._index);
                base.Compiler.IL.Emit(OpCodes.Ldelem_Ref);
                base.Compiler.IL.Emit(OpCodes.Castclass, this._boxType);
            }

            internal override void EmitStore()
            {
                LocalBuilder local = base.Compiler.GetLocal(base.Variable.Type);
                base.Compiler.IL.Emit(OpCodes.Stloc, local);
                this.EmitLoadBox();
                base.Compiler.IL.Emit(OpCodes.Ldloc, local);
                base.Compiler.FreeLocal(local);
                base.Compiler.IL.Emit(OpCodes.Stfld, this._boxValueField);
            }

            internal override void EmitStore(CompilerScope.Storage value)
            {
                this.EmitLoadBox();
                value.EmitLoad();
                base.Compiler.IL.Emit(OpCodes.Stfld, this._boxValueField);
            }
        }

        private sealed class LocalBoxStorage : CompilerScope.Storage
        {
            private readonly LocalBuilder _boxLocal;
            private readonly Type _boxType;
            private readonly FieldInfo _boxValueField;

            internal LocalBoxStorage(LambdaCompiler compiler, ParameterExpression variable) : base(compiler, variable)
            {
                this._boxType = typeof(StrongBox<>).MakeGenericType(new Type[] { variable.Type });
                this._boxValueField = this._boxType.GetField("Value");
                this._boxLocal = compiler.GetNamedLocal(this._boxType, variable);
            }

            internal override void EmitAddress()
            {
                base.Compiler.IL.Emit(OpCodes.Ldloc, this._boxLocal);
                base.Compiler.IL.Emit(OpCodes.Ldflda, this._boxValueField);
            }

            internal override void EmitLoad()
            {
                base.Compiler.IL.Emit(OpCodes.Ldloc, this._boxLocal);
                base.Compiler.IL.Emit(OpCodes.Ldfld, this._boxValueField);
            }

            internal override void EmitStore()
            {
                LocalBuilder local = base.Compiler.GetLocal(base.Variable.Type);
                base.Compiler.IL.Emit(OpCodes.Stloc, local);
                base.Compiler.IL.Emit(OpCodes.Ldloc, this._boxLocal);
                base.Compiler.IL.Emit(OpCodes.Ldloc, local);
                base.Compiler.FreeLocal(local);
                base.Compiler.IL.Emit(OpCodes.Stfld, this._boxValueField);
            }

            internal override void EmitStore(CompilerScope.Storage value)
            {
                base.Compiler.IL.Emit(OpCodes.Ldloc, this._boxLocal);
                value.EmitLoad();
                base.Compiler.IL.Emit(OpCodes.Stfld, this._boxValueField);
            }

            internal void EmitStoreBox()
            {
                base.Compiler.IL.Emit(OpCodes.Stloc, this._boxLocal);
            }
        }

        private sealed class LocalStorage : CompilerScope.Storage
        {
            private readonly LocalBuilder _local;

            internal LocalStorage(LambdaCompiler compiler, ParameterExpression variable) : base(compiler, variable)
            {
                this._local = compiler.GetNamedLocal(variable.IsByRef ? variable.Type.MakeByRefType() : variable.Type, variable);
            }

            internal override void EmitAddress()
            {
                base.Compiler.IL.Emit(OpCodes.Ldloca, this._local);
            }

            internal override void EmitLoad()
            {
                base.Compiler.IL.Emit(OpCodes.Ldloc, this._local);
            }

            internal override void EmitStore()
            {
                base.Compiler.IL.Emit(OpCodes.Stloc, this._local);
            }
        }

        private abstract class Storage
        {
            internal readonly LambdaCompiler Compiler;
            internal readonly ParameterExpression Variable;

            internal Storage(LambdaCompiler compiler, ParameterExpression variable)
            {
                this.Compiler = compiler;
                this.Variable = variable;
            }

            internal abstract void EmitAddress();
            internal abstract void EmitLoad();
            internal abstract void EmitStore();
            internal virtual void EmitStore(CompilerScope.Storage value)
            {
                value.EmitLoad();
                this.EmitStore();
            }

            internal virtual void FreeLocal()
            {
            }
        }
    }
}

