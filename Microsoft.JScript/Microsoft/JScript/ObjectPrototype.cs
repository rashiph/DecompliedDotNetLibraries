namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;

    public class ObjectPrototype : JSObject
    {
        internal static ObjectConstructor _constructor;
        internal static readonly ObjectPrototype ob = new ObjectPrototype();

        internal ObjectPrototype() : base(null)
        {
            if (Globals.contextEngine == null)
            {
                base.engine = new VsaEngine(true);
                base.engine.InitVsaEngine("JS7://Microsoft.JScript.Vsa.VsaEngine", new DefaultVsaSite());
            }
            else
            {
                base.engine = Globals.contextEngine;
            }
        }

        internal static ObjectPrototype CommonInstance()
        {
            return ob;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Object_hasOwnProperty)]
        public static bool hasOwnProperty(object thisob, object name)
        {
            string str = Microsoft.JScript.Convert.ToString(name);
            if (thisob is ArrayObject)
            {
                long num = ArrayObject.Array_index_for(str);
                if (num >= 0L)
                {
                    object valueAtIndex = ((ArrayObject) thisob).GetValueAtIndex((uint) num);
                    return ((valueAtIndex != null) && (valueAtIndex != Microsoft.JScript.Missing.Value));
                }
            }
            if (!(thisob is JSObject))
            {
                return !(LateBinding.GetMemberValue(thisob, str) is Microsoft.JScript.Missing);
            }
            MemberInfo[] member = ((JSObject) thisob).GetMember(str, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            int length = member.Length;
            if (length <= 1)
            {
                if (length < 1)
                {
                    return false;
                }
                if (member[0] is JSPrototypeField)
                {
                    return !(((JSPrototypeField) member[0]).value is Microsoft.JScript.Missing);
                }
            }
            return true;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Object_isPrototypeOf)]
        public static bool isPrototypeOf(object thisob, object ob)
        {
            if ((thisob is ScriptObject) && (ob is ScriptObject))
            {
                while (ob != null)
                {
                    if (ob == thisob)
                    {
                        return true;
                    }
                    ob = ((ScriptObject) ob).GetParent();
                }
            }
            return false;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Object_propertyIsEnumerable)]
        public static bool propertyIsEnumerable(object thisob, object name)
        {
            string str = Microsoft.JScript.Convert.ToString(name);
            if (thisob is ArrayObject)
            {
                long num = ArrayObject.Array_index_for(str);
                if (num >= 0L)
                {
                    object valueAtIndex = ((ArrayObject) thisob).GetValueAtIndex((uint) num);
                    return ((valueAtIndex != null) && (valueAtIndex != Microsoft.JScript.Missing.Value));
                }
            }
            if (!(thisob is JSObject))
            {
                return false;
            }
            FieldInfo field = ((JSObject) thisob).GetField(str, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            return ((field != null) && (field is JSExpandoField));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Object_toLocaleString)]
        public static string toLocaleString(object thisob)
        {
            return Microsoft.JScript.Convert.ToString(thisob);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Object_toString)]
        public static string toString(object thisob)
        {
            if (thisob is JSObject)
            {
                return ("[object " + ((JSObject) thisob).GetClassName() + "]");
            }
            return ("[object " + thisob.GetType().Name + "]");
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Object_valueOf)]
        public static object valueOf(object thisob)
        {
            return thisob;
        }

        public static ObjectConstructor constructor
        {
            get
            {
                return _constructor;
            }
        }
    }
}

