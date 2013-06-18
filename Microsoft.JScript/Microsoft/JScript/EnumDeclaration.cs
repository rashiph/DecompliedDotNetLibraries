namespace Microsoft.JScript
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal sealed class EnumDeclaration : Class
    {
        internal TypeExpression baseType;

        internal EnumDeclaration(Context context, IdentifierLiteral id, TypeExpression baseType, Block body, FieldAttributes attributes, CustomAttributeList customAttributes) : base(context, id, new TypeExpression(new ConstantWrapper(Typeob.Enum, null)), new TypeExpression[0], body, attributes, false, false, true, false, customAttributes)
        {
            this.baseType = (baseType != null) ? baseType : new TypeExpression(new ConstantWrapper(Typeob.Int32, null));
            base.needsEngine = false;
            base.attributes &= TypeAttributes.NestedFamORAssem;
            TypeExpression expression = new TypeExpression(new ConstantWrapper(base.classob, base.context));
            AST ast = new ConstantWrapper(-1, null);
            AST ast2 = new ConstantWrapper(1, null);
            JSMemberField[] fields = base.fields;
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo info = fields[i];
                JSVariableField field = (JSVariableField) info;
                field.attributeFlags = FieldAttributes.Literal | FieldAttributes.Static | FieldAttributes.Public;
                field.type = expression;
                if (field.value == null)
                {
                    field.value = ast = new Plus(ast.context, ast, ast2);
                }
                else
                {
                    ast = (AST) field.value;
                }
                field.value = new DeclaredEnumValue(field.value, field.Name, base.classob);
            }
        }

        internal override Type GetTypeBuilderOrEnumBuilder()
        {
            if (base.classob.classwriter != null)
            {
                return base.classob.classwriter;
            }
            this.PartiallyEvaluate();
            ClassScope enclosingScope = base.enclosingScope as ClassScope;
            if (enclosingScope != null)
            {
                TypeBuilder builder = ((TypeBuilder) enclosingScope.classwriter).DefineNestedType(base.name, base.attributes | TypeAttributes.Sealed, Typeob.Enum, (Type[]) null);
                base.classob.classwriter = builder;
                Type type = this.baseType.ToType();
                FieldBuilder builder2 = builder.DefineField("value__", type, FieldAttributes.SpecialName | FieldAttributes.Private);
                if (base.customAttributes != null)
                {
                    CustomAttributeBuilder[] customAttributeBuilders = base.customAttributes.GetCustomAttributeBuilders(false);
                    for (int k = 0; k < customAttributeBuilders.Length; k++)
                    {
                        builder.SetCustomAttribute(customAttributeBuilders[k]);
                    }
                }
                JSMemberField[] fieldArray = base.fields;
                for (int j = 0; j < fieldArray.Length; j++)
                {
                    FieldInfo info = fieldArray[j];
                    (((JSMemberField) info).metaData = builder.DefineField(info.Name, builder, FieldAttributes.Literal | FieldAttributes.Static | FieldAttributes.Public)).SetConstant(((EnumWrapper) info.GetValue(null)).ToNumericValue());
                }
                return builder;
            }
            EnumBuilder builder3 = base.compilerGlobals.module.DefineEnum(base.name, base.attributes, this.baseType.ToType());
            base.classob.classwriter = builder3;
            if (base.customAttributes != null)
            {
                CustomAttributeBuilder[] builderArray2 = base.customAttributes.GetCustomAttributeBuilders(false);
                for (int m = 0; m < builderArray2.Length; m++)
                {
                    builder3.SetCustomAttribute(builderArray2[m]);
                }
            }
            JSMemberField[] fields = base.fields;
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo info2 = fields[i];
                ((JSMemberField) info2).metaData = builder3.DefineLiteral(info2.Name, ((EnumWrapper) info2.GetValue(null)).ToNumericValue());
            }
            return builder3;
        }

        internal override AST PartiallyEvaluate()
        {
            if (base.classob.GetParent() is GlobalScope)
            {
                this.baseType.PartiallyEvaluate();
                IReflect reflect = this.baseType.ToIReflect();
                Type bt = null;
                if (!(reflect is Type) || !Microsoft.JScript.Convert.IsPrimitiveIntegerType(bt = (Type) reflect))
                {
                    this.baseType.context.HandleError(JSError.InvalidBaseTypeForEnum);
                    this.baseType = new TypeExpression(new ConstantWrapper(Typeob.Int32, null));
                    bt = Typeob.Int32;
                }
                if (base.customAttributes != null)
                {
                    base.customAttributes.PartiallyEvaluate();
                }
                if (base.NeedsToBeCheckedForCLSCompliance())
                {
                    if (!TypeExpression.TypeIsCLSCompliant(reflect))
                    {
                        this.baseType.context.HandleError(JSError.NonCLSCompliantType);
                    }
                    base.CheckMemberNamesForCLSCompliance();
                }
                ScriptObject enclosingScope = base.enclosingScope;
                while (!(enclosingScope is GlobalScope) && !(enclosingScope is PackageScope))
                {
                    enclosingScope = enclosingScope.GetParent();
                }
                base.classob.SetParent(new WithObject(enclosingScope, Typeob.Enum, true));
                base.Globals.ScopeStack.Push(base.classob);
                try
                {
                    JSMemberField[] fields = base.fields;
                    for (int i = 0; i < fields.Length; i++)
                    {
                        FieldInfo info = fields[i];
                        JSMemberField field = (JSMemberField) info;
                        ((DeclaredEnumValue) field.value).CoerceToBaseType(bt, field.originalContext);
                    }
                }
                finally
                {
                    base.Globals.ScopeStack.Pop();
                }
            }
            return this;
        }
    }
}

