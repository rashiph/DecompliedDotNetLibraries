namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class VariableDeclaration : AST
    {
        private Completion completion;
        internal JSVariableField field;
        internal Lookup identifier;
        internal AST initializer;
        private TypeExpression type;

        internal VariableDeclaration(Context context, Lookup identifier, TypeExpression type, AST initializer, FieldAttributes attributes, CustomAttributeList customAttributes) : base(context)
        {
            if (initializer != null)
            {
                base.context.UpdateWith(initializer.context);
            }
            else if (type != null)
            {
                base.context.UpdateWith(type.context);
            }
            this.identifier = identifier;
            this.type = type;
            this.initializer = initializer;
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            while (parent is WithObject)
            {
                parent = parent.GetParent();
            }
            string name = this.identifier.ToString();
            if (parent is ClassScope)
            {
                if (name == ((ClassScope) parent).name)
                {
                    identifier.context.HandleError(JSError.CannotUseNameOfClass);
                    name = name + " var";
                }
            }
            else if (attributes != FieldAttributes.PrivateScope)
            {
                base.context.HandleError(JSError.NotInsideClass);
                attributes = FieldAttributes.Public;
            }
            else
            {
                attributes |= FieldAttributes.Public;
            }
            FieldInfo localField = ((IActivationObject) parent).GetLocalField(name);
            if (localField != null)
            {
                if ((localField.IsLiteral || (parent is ClassScope)) || (type != null))
                {
                    identifier.context.HandleError(JSError.DuplicateName, true);
                }
                this.type = (TypeExpression) (type = null);
            }
            if (parent is ActivationObject)
            {
                if ((localField == null) || (localField is JSVariableField))
                {
                    this.field = ((ActivationObject) parent).AddFieldOrUseExistingField(this.identifier.ToString(), Microsoft.JScript.Missing.Value, attributes);
                }
                else
                {
                    this.field = ((ActivationObject) parent).AddNewField(this.identifier.ToString(), null, attributes);
                }
            }
            else
            {
                this.field = ((StackFrame) parent).AddNewField(this.identifier.ToString(), null, attributes | FieldAttributes.Static);
            }
            this.field.type = type;
            this.field.customAttributes = customAttributes;
            this.field.originalContext = context;
            if (this.field is JSLocalField)
            {
                ((JSLocalField) this.field).debugOn = this.identifier.context.document.debugOn;
            }
            this.completion = new Completion();
        }

        internal override object Evaluate()
        {
            ScriptObject parent = base.Globals.ScopeStack.Peek();
            object obj3 = null;
            if (this.initializer != null)
            {
                obj3 = this.initializer.Evaluate();
            }
            if (this.type == null)
            {
                while (parent is BlockScope)
                {
                    parent = parent.GetParent();
                }
                if (parent is WithObject)
                {
                    this.identifier.SetWithValue((WithObject) parent, obj3);
                }
                while ((parent is WithObject) || (parent is BlockScope))
                {
                    parent = parent.GetParent();
                }
                if ((this.initializer == null) && !(this.field.value is Microsoft.JScript.Missing))
                {
                    this.completion.value = this.field.value;
                    return this.completion;
                }
            }
            else
            {
                obj3 = Microsoft.JScript.Convert.Coerce(obj3, this.type);
            }
            this.field.SetValue(parent, this.completion.value = obj3);
            return this.completion;
        }

        internal override Context GetFirstExecutableContext()
        {
            if (this.initializer == null)
            {
                return null;
            }
            return base.context;
        }

        internal override AST PartiallyEvaluate()
        {
            AST ast = this.identifier = (Lookup) this.identifier.PartiallyEvaluateAsReference();
            if (this.type != null)
            {
                this.field.type = this.type = (TypeExpression) this.type.PartiallyEvaluate();
            }
            else if (((this.initializer == null) && !(this.field is JSLocalField)) && (this.field.value is Microsoft.JScript.Missing))
            {
                ast.context.HandleError(JSError.VariableLeftUninitialized);
                this.field.type = this.type = new TypeExpression(new ConstantWrapper(Typeob.Object, ast.context));
            }
            if (this.initializer != null)
            {
                if (this.field.IsStatic)
                {
                    ScriptObject parent = base.Engine.ScriptObjectStackTop();
                    ClassScope scope = null;
                    while ((parent != null) && ((scope = parent as ClassScope) == null))
                    {
                        parent = parent.GetParent();
                    }
                    if (scope != null)
                    {
                        scope.inStaticInitializerCode = true;
                    }
                    this.initializer = this.initializer.PartiallyEvaluate();
                    if (scope != null)
                    {
                        scope.inStaticInitializerCode = false;
                    }
                }
                else
                {
                    this.initializer = this.initializer.PartiallyEvaluate();
                }
                ast.SetPartialValue(this.initializer);
            }
            if ((this.field != null) && (this.field.customAttributes != null))
            {
                this.field.customAttributes.PartiallyEvaluate();
            }
            return this;
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if (this.initializer != null)
            {
                if (base.context.document.debugOn && (this.initializer.context != null))
                {
                    base.context.EmitLineInfo(il);
                }
                Lookup identifier = this.identifier;
                identifier.TranslateToILPreSet(il, true);
                identifier.TranslateToILSet(il, true, this.initializer);
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
            if (this.type != null)
            {
                this.type.TranslateToILInitializer(il);
            }
            if (this.initializer != null)
            {
                this.initializer.TranslateToILInitializer(il);
            }
        }
    }
}

