namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;

    public class BlockScope : ActivationObject
    {
        internal bool catchHanderScope;
        private static int counter;
        private ArrayList localFieldsForDebugInfo;
        internal int scopeId;

        internal BlockScope(ScriptObject parent) : base(parent)
        {
            this.scopeId = counter++;
            base.isKnownAtCompileTime = true;
            base.fast = (parent is ActivationObject) ? ((ActivationObject) parent).fast : true;
            this.localFieldsForDebugInfo = new ArrayList();
        }

        public BlockScope(ScriptObject parent, string name, int scopeId) : base(parent)
        {
            this.scopeId = scopeId;
            JSField field = (JSField) base.parent.GetField(name + ":" + this.scopeId, BindingFlags.Public);
            base.name_table[name] = field;
            base.field_table.Add(field);
        }

        internal void AddFieldForLocalScopeDebugInfo(JSLocalField field)
        {
            this.localFieldsForDebugInfo.Add(field);
        }

        protected override JSVariableField CreateField(string name, FieldAttributes attributeFlags, object value)
        {
            if (!(base.parent is ActivationObject))
            {
                return base.CreateField(name, attributeFlags, value);
            }
            JSVariableField field = ((ActivationObject) base.parent).AddNewField(name + ":" + this.scopeId, value, attributeFlags);
            field.debuggerName = name;
            return field;
        }

        internal void EmitLocalInfoForFields(ILGenerator il)
        {
            foreach (JSLocalField field in this.localFieldsForDebugInfo)
            {
                ((LocalBuilder) field.metaData).SetLocalSymInfo(field.debuggerName);
            }
            if (base.parent is GlobalScope)
            {
                LocalBuilder local = il.DeclareLocal(Typeob.Int32);
                local.SetLocalSymInfo("scopeId for catch block");
                ConstantWrapper.TranslateToILInt(il, this.scopeId);
                il.Emit(OpCodes.Stloc, local);
            }
        }
    }
}

