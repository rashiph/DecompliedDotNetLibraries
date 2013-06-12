namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class BoundConstants
    {
        private readonly Dictionary<TypedConstant, LocalBuilder> _cache = new Dictionary<TypedConstant, LocalBuilder>();
        private readonly Dictionary<object, int> _indexes = new Dictionary<object, int>(ReferenceEqualityComparer<object>.Instance);
        private readonly Dictionary<TypedConstant, int> _references = new Dictionary<TypedConstant, int>();
        private readonly List<object> _values = new List<object>();

        internal void AddReference(object value, Type type)
        {
            if (!this._indexes.ContainsKey(value))
            {
                this._indexes.Add(value, this._values.Count);
                this._values.Add(value);
            }
            Helpers.IncrementCount<TypedConstant>(new TypedConstant(value, type), this._references);
        }

        internal void EmitCacheConstants(LambdaCompiler lc)
        {
            int num = 0;
            foreach (KeyValuePair<TypedConstant, int> pair in this._references)
            {
                if (!lc.CanEmitBoundConstants)
                {
                    throw Error.CannotCompileConstant(pair.Key.Value);
                }
                if (ShouldCache(pair.Value))
                {
                    num++;
                }
            }
            if (num != 0)
            {
                EmitConstantsArray(lc);
                this._cache.Clear();
                foreach (KeyValuePair<TypedConstant, int> pair2 in this._references)
                {
                    if (ShouldCache(pair2.Value))
                    {
                        if (--num > 0)
                        {
                            lc.IL.Emit(OpCodes.Dup);
                        }
                        LocalBuilder local = lc.IL.DeclareLocal(pair2.Key.Type);
                        this.EmitConstantFromArray(lc, pair2.Key.Value, local.LocalType);
                        lc.IL.Emit(OpCodes.Stloc, local);
                        this._cache.Add(pair2.Key, local);
                    }
                }
            }
        }

        internal void EmitConstant(LambdaCompiler lc, object value, Type type)
        {
            LocalBuilder builder;
            if (!lc.CanEmitBoundConstants)
            {
                throw Error.CannotCompileConstant(value);
            }
            if (this._cache.TryGetValue(new TypedConstant(value, type), out builder))
            {
                lc.IL.Emit(OpCodes.Ldloc, builder);
            }
            else
            {
                EmitConstantsArray(lc);
                this.EmitConstantFromArray(lc, value, type);
            }
        }

        private void EmitConstantFromArray(LambdaCompiler lc, object value, Type type)
        {
            int num;
            if (!this._indexes.TryGetValue(value, out num))
            {
                this._indexes.Add(value, num = this._values.Count);
                this._values.Add(value);
            }
            lc.IL.EmitInt(num);
            lc.IL.Emit(OpCodes.Ldelem_Ref);
            if (type.IsValueType)
            {
                lc.IL.Emit(OpCodes.Unbox_Any, type);
            }
            else if (type != typeof(object))
            {
                lc.IL.Emit(OpCodes.Castclass, type);
            }
        }

        private static void EmitConstantsArray(LambdaCompiler lc)
        {
            lc.EmitClosureArgument();
            lc.IL.Emit(OpCodes.Ldfld, typeof(Closure).GetField("Constants"));
        }

        private static bool ShouldCache(int refCount)
        {
            return (refCount > 2);
        }

        internal object[] ToArray()
        {
            return this._values.ToArray();
        }

        internal int Count
        {
            get
            {
                return this._values.Count;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TypedConstant : IEquatable<BoundConstants.TypedConstant>
        {
            internal readonly object Value;
            internal readonly System.Type Type;
            internal TypedConstant(object value, System.Type type)
            {
                this.Value = value;
                this.Type = type;
            }

            public override int GetHashCode()
            {
                return (RuntimeHelpers.GetHashCode(this.Value) ^ this.Type.GetHashCode());
            }

            public bool Equals(BoundConstants.TypedConstant other)
            {
                return (object.ReferenceEquals(this.Value, other.Value) && this.Type.Equals(other.Type));
            }

            public override bool Equals(object obj)
            {
                return ((obj is BoundConstants.TypedConstant) && this.Equals((BoundConstants.TypedConstant) obj));
            }
        }
    }
}

