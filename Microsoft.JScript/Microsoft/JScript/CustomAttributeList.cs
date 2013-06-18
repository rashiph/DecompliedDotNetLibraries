namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection.Emit;

    internal sealed class CustomAttributeList : AST
    {
        private bool alreadyPartiallyEvaluated;
        private ArrayList customAttributes;
        private ArrayList list;

        internal CustomAttributeList(Context context) : base(context)
        {
            this.list = new ArrayList();
            this.customAttributes = null;
            this.alreadyPartiallyEvaluated = false;
        }

        internal void Append(CustomAttribute elem)
        {
            this.list.Add(elem);
            base.context.UpdateWith(elem.context);
        }

        internal bool ContainsExpandoAttribute()
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                CustomAttribute attribute = (CustomAttribute) this.list[num];
                if ((attribute != null) && attribute.IsExpandoAttribute())
                {
                    return true;
                }
                num++;
            }
            return false;
        }

        internal override object Evaluate()
        {
            return this.Evaluate(false);
        }

        internal object Evaluate(bool getForProperty)
        {
            int count = this.list.Count;
            ArrayList list = new ArrayList(count);
            for (int i = 0; i < count; i++)
            {
                CustomAttribute attribute = (CustomAttribute) this.list[i];
                if (attribute == null)
                {
                    continue;
                }
                if (attribute.raiseToPropertyLevel)
                {
                    if (getForProperty)
                    {
                        goto Label_003C;
                    }
                    continue;
                }
                if (getForProperty)
                {
                    continue;
                }
            Label_003C:
                list.Add(attribute.Evaluate());
            }
            object[] array = new object[list.Count];
            list.CopyTo(array);
            return array;
        }

        internal CustomAttribute GetAttribute(Type attributeClass)
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                CustomAttribute attribute = (CustomAttribute) this.list[num];
                if (attribute != null)
                {
                    object type = attribute.type;
                    if ((type is Type) && (((Type) type) == attributeClass))
                    {
                        return (CustomAttribute) this.list[num];
                    }
                }
                num++;
            }
            return null;
        }

        internal CustomAttributeBuilder[] GetCustomAttributeBuilders(bool getForProperty)
        {
            this.customAttributes = new ArrayList(this.list.Count);
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                CustomAttributeBuilder builder;
                CustomAttribute attribute = (CustomAttribute) this.list[num];
                if (attribute == null)
                {
                    goto Label_0062;
                }
                if (attribute.raiseToPropertyLevel)
                {
                    if (getForProperty)
                    {
                        goto Label_004B;
                    }
                    goto Label_0062;
                }
                if (getForProperty)
                {
                    goto Label_0062;
                }
            Label_004B:
                builder = attribute.GetCustomAttribute();
                if (builder != null)
                {
                    this.customAttributes.Add(builder);
                }
            Label_0062:
                num++;
            }
            CustomAttributeBuilder[] array = new CustomAttributeBuilder[this.customAttributes.Count];
            this.customAttributes.CopyTo(array);
            return array;
        }

        internal override AST PartiallyEvaluate()
        {
            if (!this.alreadyPartiallyEvaluated)
            {
                this.alreadyPartiallyEvaluated = true;
                int num = 0;
                int count = this.list.Count;
                while (num < count)
                {
                    this.list[num] = ((CustomAttribute) this.list[num]).PartiallyEvaluate();
                    num++;
                }
                int num3 = 0;
                int num4 = this.list.Count;
                while (num3 < num4)
                {
                    CustomAttribute attribute = (CustomAttribute) this.list[num3];
                    if (attribute != null)
                    {
                        object typeIfAttributeHasToBeUnique = attribute.GetTypeIfAttributeHasToBeUnique();
                        if (typeIfAttributeHasToBeUnique != null)
                        {
                            for (int i = num3 + 1; i < num4; i++)
                            {
                                CustomAttribute attribute2 = (CustomAttribute) this.list[i];
                                if ((attribute2 != null) && (typeIfAttributeHasToBeUnique == attribute2.type))
                                {
                                    attribute2.context.HandleError(JSError.CustomAttributeUsedMoreThanOnce);
                                    this.list[i] = null;
                                }
                            }
                        }
                    }
                    num3++;
                }
            }
            return this;
        }

        internal void Remove(CustomAttribute elem)
        {
            this.list.Remove(elem);
        }

        internal void SetTarget(AST target)
        {
            int num = 0;
            int count = this.list.Count;
            while (num < count)
            {
                ((CustomAttribute) this.list[num]).SetTarget(target);
                num++;
            }
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
        }
    }
}

