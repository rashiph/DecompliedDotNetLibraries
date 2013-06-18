namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class TypeExpression : AST
    {
        private IReflect cachedIR;
        internal AST expression;
        internal bool isArray;
        internal int rank;
        private bool recursive;

        internal TypeExpression(AST expression) : base(expression.context)
        {
            this.expression = expression;
            this.isArray = false;
            this.rank = 0;
            this.recursive = false;
            this.cachedIR = null;
            if (expression is Lookup)
            {
                string typeName = expression.ToString();
                object predefinedType = Globals.TypeRefs.GetPredefinedType(typeName);
                if (predefinedType != null)
                {
                    this.expression = new ConstantWrapper(predefinedType, expression.context);
                }
            }
        }

        internal override object Evaluate()
        {
            return this.ToIReflect();
        }

        internal override IReflect InferType(JSField inference_target)
        {
            return this.ToIReflect();
        }

        internal bool IsCLSCompliant()
        {
            return TypeIsCLSCompliant(this.expression.Evaluate());
        }

        internal override AST PartiallyEvaluate()
        {
            if (this.recursive)
            {
                if (!(this.expression is ConstantWrapper))
                {
                    this.expression = new ConstantWrapper(Typeob.Object, base.context);
                }
                return this;
            }
            Member expression = this.expression as Member;
            if (expression != null)
            {
                object obj2 = expression.EvaluateAsType();
                if (obj2 != null)
                {
                    this.expression = new ConstantWrapper(obj2, expression.context);
                    return this;
                }
            }
            this.recursive = true;
            this.expression = this.expression.PartiallyEvaluate();
            this.recursive = false;
            if (!(this.expression is TypeExpression))
            {
                Type c = null;
                if (this.expression is ConstantWrapper)
                {
                    object obj3 = this.expression.Evaluate();
                    if (obj3 == null)
                    {
                        this.expression.context.HandleError(JSError.NeedType);
                        this.expression = new ConstantWrapper(Typeob.Object, base.context);
                        return this;
                    }
                    c = Globals.TypeRefs.ToReferenceContext(obj3.GetType());
                    Binding.WarnIfObsolete(obj3 as Type, this.expression.context);
                }
                else if (this.expression.OkToUseAsType())
                {
                    c = Globals.TypeRefs.ToReferenceContext(this.expression.Evaluate().GetType());
                }
                else
                {
                    this.expression.context.HandleError(JSError.NeedCompileTimeConstant);
                    this.expression = new ConstantWrapper(Typeob.Object, this.expression.context);
                    return this;
                }
                if ((c == null) || (((c != Typeob.ClassScope) && (c != Typeob.TypedArray)) && !Typeob.Type.IsAssignableFrom(c)))
                {
                    this.expression.context.HandleError(JSError.NeedType);
                    this.expression = new ConstantWrapper(Typeob.Object, this.expression.context);
                }
            }
            return this;
        }

        internal IReflect ToIReflect()
        {
            if (!(this.expression is ConstantWrapper))
            {
                this.PartiallyEvaluate();
            }
            IReflect cachedIR = this.cachedIR;
            if (cachedIR != null)
            {
                return cachedIR;
            }
            object obj2 = this.expression.Evaluate();
            if (((obj2 is ClassScope) || (obj2 is TypedArray)) || (base.context == null))
            {
                cachedIR = (IReflect) obj2;
            }
            else
            {
                cachedIR = Microsoft.JScript.Convert.ToIReflect((Type) obj2, base.Engine);
            }
            if (this.isArray)
            {
                return (this.cachedIR = new TypedArray(cachedIR, this.rank));
            }
            return (this.cachedIR = cachedIR);
        }

        internal Type ToType()
        {
            if (!(this.expression is ConstantWrapper))
            {
                this.PartiallyEvaluate();
            }
            object obj2 = this.expression.Evaluate();
            Type elementType = null;
            if (obj2 is ClassScope)
            {
                elementType = ((ClassScope) obj2).GetTypeBuilderOrEnumBuilder();
            }
            else if (obj2 is TypedArray)
            {
                elementType = Microsoft.JScript.Convert.ToType((TypedArray) obj2);
            }
            else
            {
                elementType = Globals.TypeRefs.ToReferenceContext((Type) obj2);
            }
            if (this.isArray)
            {
                return Microsoft.JScript.Convert.ToType(TypedArray.ToRankString(this.rank), elementType);
            }
            return elementType;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            this.expression.TranslateToIL(il, rtype);
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            this.expression.TranslateToILInitializer(il);
        }

        internal static bool TypeIsCLSCompliant(object type)
        {
            if (type is ClassScope)
            {
                return ((ClassScope) type).IsCLSCompliant();
            }
            if (type is TypedArray)
            {
                object elementType = ((TypedArray) type).elementType;
                return ((!(elementType is TypedArray) && (!(elementType is Type) || !((Type) elementType).IsArray)) && TypeIsCLSCompliant(elementType));
            }
            Type type2 = (Type) type;
            if (type2.IsPrimitive)
            {
                if (((!(type2 == Typeob.Boolean) && !(type2 == Typeob.Byte)) && (!(type2 == Typeob.Char) && !(type2 == Typeob.Double))) && ((!(type2 == Typeob.Int16) && !(type2 == Typeob.Int32)) && (!(type2 == Typeob.Int64) && !(type2 == Typeob.Single))))
                {
                    return false;
                }
                return true;
            }
            if (type2.IsArray)
            {
                if (type2.GetElementType().IsArray)
                {
                    return false;
                }
                return TypeIsCLSCompliant(type2);
            }
            object[] objArray = Microsoft.JScript.CustomAttribute.GetCustomAttributes(type2, typeof(CLSCompliantAttribute), false);
            if (objArray.Length > 0)
            {
                return ((CLSCompliantAttribute) objArray[0]).IsCompliant;
            }
            Module target = type2.Module;
            objArray = Microsoft.JScript.CustomAttribute.GetCustomAttributes(target, typeof(CLSCompliantAttribute), false);
            if (objArray.Length > 0)
            {
                return ((CLSCompliantAttribute) objArray[0]).IsCompliant;
            }
            objArray = Microsoft.JScript.CustomAttribute.GetCustomAttributes(target.Assembly, typeof(CLSCompliantAttribute), false);
            return ((objArray.Length > 0) && ((CLSCompliantAttribute) objArray[0]).IsCompliant);
        }
    }
}

