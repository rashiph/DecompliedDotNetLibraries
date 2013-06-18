namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Reflection;

    internal sealed class BuiltinFunction : ScriptFunction
    {
        private JSBuiltin biFunc;
        internal MethodInfo method;

        internal BuiltinFunction(object obj, MethodInfo method) : this(method.Name, obj, method, FunctionPrototype.ob)
        {
        }

        internal BuiltinFunction(string name, object obj, MethodInfo method, ScriptFunction parent) : base(parent, name)
        {
            base.noExpando = false;
            ParameterInfo[] parameters = method.GetParameters();
            base.ilength = parameters.Length;
            object[] objArray = Microsoft.JScript.CustomAttribute.GetCustomAttributes(method, typeof(JSFunctionAttribute), false);
            JSFunctionAttribute attribute = (objArray.Length > 0) ? ((JSFunctionAttribute) objArray[0]) : new JSFunctionAttribute(JSFunctionAttributeEnum.None);
            JSFunctionAttributeEnum attributeValue = attribute.attributeValue;
            if ((attributeValue & JSFunctionAttributeEnum.HasThisObject) != JSFunctionAttributeEnum.None)
            {
                base.ilength--;
            }
            if ((attributeValue & JSFunctionAttributeEnum.HasEngine) != JSFunctionAttributeEnum.None)
            {
                base.ilength--;
            }
            if ((attributeValue & JSFunctionAttributeEnum.HasVarArgs) != JSFunctionAttributeEnum.None)
            {
                base.ilength--;
            }
            this.biFunc = attribute.builtinFunction;
            if (this.biFunc == JSBuiltin.None)
            {
                this.method = new JSNativeMethod(method, obj, base.engine);
            }
            else
            {
                this.method = null;
            }
        }

        internal override object Call(object[] args, object thisob)
        {
            return QuickCall(args, thisob, this.biFunc, this.method, base.engine);
        }

        private static object GetArg(object[] args, int i, int n)
        {
            if (i >= n)
            {
                return Microsoft.JScript.Missing.Value;
            }
            return args[i];
        }

        internal static object QuickCall(object[] args, object thisob, JSBuiltin biFunc, MethodInfo method, VsaEngine engine)
        {
            int length = args.Length;
            switch (biFunc)
            {
                case JSBuiltin.Array_concat:
                    return ArrayPrototype.concat(thisob, engine, args);

                case JSBuiltin.Array_join:
                    return ArrayPrototype.join(thisob, GetArg(args, 0, length));

                case JSBuiltin.Array_pop:
                    return ArrayPrototype.pop(thisob);

                case JSBuiltin.Array_push:
                    return ArrayPrototype.push(thisob, args);

                case JSBuiltin.Array_reverse:
                    return ArrayPrototype.reverse(thisob);

                case JSBuiltin.Array_shift:
                    return ArrayPrototype.shift(thisob);

                case JSBuiltin.Array_slice:
                    return ArrayPrototype.slice(thisob, engine, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length));

                case JSBuiltin.Array_sort:
                    return ArrayPrototype.sort(thisob, GetArg(args, 0, length));

                case JSBuiltin.Array_splice:
                    return ArrayPrototype.splice(thisob, engine, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), Microsoft.JScript.Convert.ToNumber(GetArg(args, 1, length)), VarArgs(args, 2, length));

                case JSBuiltin.Array_toLocaleString:
                    return ArrayPrototype.toLocaleString(thisob);

                case JSBuiltin.Array_toString:
                    return ArrayPrototype.toString(thisob);

                case JSBuiltin.Array_unshift:
                    return ArrayPrototype.unshift(thisob, args);

                case JSBuiltin.Boolean_toString:
                    return BooleanPrototype.toString(thisob);

                case JSBuiltin.Boolean_valueOf:
                    return BooleanPrototype.valueOf(thisob);

                case JSBuiltin.Date_getDate:
                    return DatePrototype.getDate(thisob);

                case JSBuiltin.Date_getDay:
                    return DatePrototype.getDay(thisob);

                case JSBuiltin.Date_getFullYear:
                    return DatePrototype.getFullYear(thisob);

                case JSBuiltin.Date_getHours:
                    return DatePrototype.getHours(thisob);

                case JSBuiltin.Date_getMilliseconds:
                    return DatePrototype.getMilliseconds(thisob);

                case JSBuiltin.Date_getMinutes:
                    return DatePrototype.getMinutes(thisob);

                case JSBuiltin.Date_getMonth:
                    return DatePrototype.getMonth(thisob);

                case JSBuiltin.Date_getSeconds:
                    return DatePrototype.getSeconds(thisob);

                case JSBuiltin.Date_getTime:
                    return DatePrototype.getTime(thisob);

                case JSBuiltin.Date_getTimezoneOffset:
                    return DatePrototype.getTimezoneOffset(thisob);

                case JSBuiltin.Date_getUTCDate:
                    return DatePrototype.getUTCDate(thisob);

                case JSBuiltin.Date_getUTCDay:
                    return DatePrototype.getUTCDay(thisob);

                case JSBuiltin.Date_getUTCFullYear:
                    return DatePrototype.getUTCFullYear(thisob);

                case JSBuiltin.Date_getUTCHours:
                    return DatePrototype.getUTCHours(thisob);

                case JSBuiltin.Date_getUTCMilliseconds:
                    return DatePrototype.getUTCMilliseconds(thisob);

                case JSBuiltin.Date_getUTCMinutes:
                    return DatePrototype.getUTCMinutes(thisob);

                case JSBuiltin.Date_getUTCMonth:
                    return DatePrototype.getUTCMonth(thisob);

                case JSBuiltin.Date_getUTCSeconds:
                    return DatePrototype.getUTCSeconds(thisob);

                case JSBuiltin.Date_getVarDate:
                    return DatePrototype.getVarDate(thisob);

                case JSBuiltin.Date_getYear:
                    return DatePrototype.getYear(thisob);

                case JSBuiltin.Date_parse:
                    return DateConstructor.parse(Microsoft.JScript.Convert.ToString(GetArg(args, 0, length)));

                case JSBuiltin.Date_setDate:
                    return DatePrototype.setDate(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Date_setFullYear:
                    return DatePrototype.setFullYear(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length), GetArg(args, 2, length));

                case JSBuiltin.Date_setHours:
                    return DatePrototype.setHours(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length), GetArg(args, 2, length), GetArg(args, 3, length));

                case JSBuiltin.Date_setMinutes:
                    return DatePrototype.setMinutes(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length), GetArg(args, 2, length));

                case JSBuiltin.Date_setMilliseconds:
                    return DatePrototype.setMilliseconds(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Date_setMonth:
                    return DatePrototype.setMonth(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length));

                case JSBuiltin.Date_setSeconds:
                    return DatePrototype.setSeconds(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length));

                case JSBuiltin.Date_setTime:
                    return DatePrototype.setTime(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Date_setUTCDate:
                    return DatePrototype.setUTCDate(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Date_setUTCFullYear:
                    return DatePrototype.setUTCFullYear(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length), GetArg(args, 2, length));

                case JSBuiltin.Date_setUTCHours:
                    return DatePrototype.setUTCHours(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length), GetArg(args, 2, length), GetArg(args, 3, length));

                case JSBuiltin.Date_setUTCMinutes:
                    return DatePrototype.setUTCMinutes(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length), GetArg(args, 2, length));

                case JSBuiltin.Date_setUTCMilliseconds:
                    return DatePrototype.setUTCMilliseconds(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Date_setUTCMonth:
                    return DatePrototype.setUTCMonth(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length));

                case JSBuiltin.Date_setUTCSeconds:
                    return DatePrototype.setUTCSeconds(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length));

                case JSBuiltin.Date_setYear:
                    return DatePrototype.setYear(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Date_toDateString:
                    return DatePrototype.toDateString(thisob);

                case JSBuiltin.Date_toGMTString:
                    return DatePrototype.toGMTString(thisob);

                case JSBuiltin.Date_toLocaleDateString:
                    return DatePrototype.toLocaleDateString(thisob);

                case JSBuiltin.Date_toLocaleString:
                    return DatePrototype.toLocaleString(thisob);

                case JSBuiltin.Date_toLocaleTimeString:
                    return DatePrototype.toLocaleTimeString(thisob);

                case JSBuiltin.Date_toString:
                    return DatePrototype.toString(thisob);

                case JSBuiltin.Date_toTimeString:
                    return DatePrototype.toTimeString(thisob);

                case JSBuiltin.Date_toUTCString:
                    return DatePrototype.toUTCString(thisob);

                case JSBuiltin.Date_UTC:
                    return DateConstructor.UTC(GetArg(args, 0, length), GetArg(args, 1, length), GetArg(args, 2, length), GetArg(args, 3, length), GetArg(args, 4, length), GetArg(args, 5, length), GetArg(args, 6, length));

                case JSBuiltin.Date_valueOf:
                    return DatePrototype.valueOf(thisob);

                case JSBuiltin.Enumerator_atEnd:
                    return EnumeratorPrototype.atEnd(thisob);

                case JSBuiltin.Enumerator_item:
                    return EnumeratorPrototype.item(thisob);

                case JSBuiltin.Enumerator_moveFirst:
                    EnumeratorPrototype.moveFirst(thisob);
                    return null;

                case JSBuiltin.Enumerator_moveNext:
                    EnumeratorPrototype.moveNext(thisob);
                    return null;

                case JSBuiltin.Error_toString:
                    return ErrorPrototype.toString(thisob);

                case JSBuiltin.Function_apply:
                    return FunctionPrototype.apply(thisob, GetArg(args, 0, length), GetArg(args, 1, length));

                case JSBuiltin.Function_call:
                    return FunctionPrototype.call(thisob, GetArg(args, 0, length), VarArgs(args, 1, length));

                case JSBuiltin.Function_toString:
                    return FunctionPrototype.toString(thisob);

                case JSBuiltin.Global_CollectGarbage:
                    GlobalObject.CollectGarbage();
                    return null;

                case JSBuiltin.Global_decodeURI:
                    return GlobalObject.decodeURI(GetArg(args, 0, length));

                case JSBuiltin.Global_decodeURIComponent:
                    return GlobalObject.decodeURIComponent(GetArg(args, 0, length));

                case JSBuiltin.Global_encodeURI:
                    return GlobalObject.encodeURI(GetArg(args, 0, length));

                case JSBuiltin.Global_encodeURIComponent:
                    return GlobalObject.encodeURIComponent(GetArg(args, 0, length));

                case JSBuiltin.Global_escape:
                    return GlobalObject.escape(GetArg(args, 0, length));

                case JSBuiltin.Global_eval:
                    return GlobalObject.eval(GetArg(args, 0, length));

                case JSBuiltin.Global_GetObject:
                    return GlobalObject.GetObject(GetArg(args, 0, length), GetArg(args, 1, length));

                case JSBuiltin.Global_isNaN:
                    return GlobalObject.isNaN(GetArg(args, 0, length));

                case JSBuiltin.Global_isFinite:
                    return GlobalObject.isFinite(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Global_parseFloat:
                    return GlobalObject.parseFloat(GetArg(args, 0, length));

                case JSBuiltin.Global_parseInt:
                    return GlobalObject.parseInt(GetArg(args, 0, length), GetArg(args, 1, length));

                case JSBuiltin.Global_ScriptEngine:
                    return GlobalObject.ScriptEngine();

                case JSBuiltin.Global_ScriptEngineBuildVersion:
                    return GlobalObject.ScriptEngineBuildVersion();

                case JSBuiltin.Global_ScriptEngineMajorVersion:
                    return GlobalObject.ScriptEngineMajorVersion();

                case JSBuiltin.Global_ScriptEngineMinorVersion:
                    return GlobalObject.ScriptEngineMinorVersion();

                case JSBuiltin.Global_unescape:
                    return GlobalObject.unescape(GetArg(args, 0, length));

                case JSBuiltin.Math_abs:
                    return MathObject.abs(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_acos:
                    return MathObject.acos(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_asin:
                    return MathObject.asin(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_atan:
                    return MathObject.atan(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_atan2:
                    return MathObject.atan2(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), Microsoft.JScript.Convert.ToNumber(GetArg(args, 1, length)));

                case JSBuiltin.Math_ceil:
                    return MathObject.ceil(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_cos:
                    return MathObject.cos(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_exp:
                    return MathObject.exp(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_floor:
                    return MathObject.floor(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_log:
                    return MathObject.log(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_max:
                    return MathObject.max(GetArg(args, 0, length), GetArg(args, 1, length), VarArgs(args, 2, length));

                case JSBuiltin.Math_min:
                    return MathObject.min(GetArg(args, 0, length), GetArg(args, 1, length), VarArgs(args, 2, length));

                case JSBuiltin.Math_pow:
                    return MathObject.pow(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), Microsoft.JScript.Convert.ToNumber(GetArg(args, 1, length)));

                case JSBuiltin.Math_random:
                    return MathObject.random();

                case JSBuiltin.Math_round:
                    return MathObject.round(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_sin:
                    return MathObject.sin(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_sqrt:
                    return MathObject.sqrt(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Math_tan:
                    return MathObject.tan(Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Number_toExponential:
                    return NumberPrototype.toExponential(thisob, GetArg(args, 0, length));

                case JSBuiltin.Number_toFixed:
                    return NumberPrototype.toFixed(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.Number_toLocaleString:
                    return NumberPrototype.toLocaleString(thisob);

                case JSBuiltin.Number_toPrecision:
                    return NumberPrototype.toPrecision(thisob, GetArg(args, 0, length));

                case JSBuiltin.Number_toString:
                    return NumberPrototype.toString(thisob, GetArg(args, 0, length));

                case JSBuiltin.Number_valueOf:
                    return NumberPrototype.valueOf(thisob);

                case JSBuiltin.Object_hasOwnProperty:
                    return ObjectPrototype.hasOwnProperty(thisob, GetArg(args, 0, length));

                case JSBuiltin.Object_isPrototypeOf:
                    return ObjectPrototype.isPrototypeOf(thisob, GetArg(args, 0, length));

                case JSBuiltin.Object_propertyIsEnumerable:
                    return ObjectPrototype.propertyIsEnumerable(thisob, GetArg(args, 0, length));

                case JSBuiltin.Object_toLocaleString:
                    return ObjectPrototype.toLocaleString(thisob);

                case JSBuiltin.Object_toString:
                    return ObjectPrototype.toString(thisob);

                case JSBuiltin.Object_valueOf:
                    return ObjectPrototype.valueOf(thisob);

                case JSBuiltin.RegExp_compile:
                    return RegExpPrototype.compile(thisob, GetArg(args, 0, length), GetArg(args, 1, length));

                case JSBuiltin.RegExp_exec:
                    return RegExpPrototype.exec(thisob, GetArg(args, 0, length));

                case JSBuiltin.RegExp_test:
                    return RegExpPrototype.test(thisob, GetArg(args, 0, length));

                case JSBuiltin.RegExp_toString:
                    return RegExpPrototype.toString(thisob);

                case JSBuiltin.String_anchor:
                    return StringPrototype.anchor(thisob, GetArg(args, 0, length));

                case JSBuiltin.String_big:
                    return StringPrototype.big(thisob);

                case JSBuiltin.String_blink:
                    return StringPrototype.blink(thisob);

                case JSBuiltin.String_bold:
                    return StringPrototype.bold(thisob);

                case JSBuiltin.String_charAt:
                    return StringPrototype.charAt(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.String_charCodeAt:
                    return StringPrototype.charCodeAt(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)));

                case JSBuiltin.String_concat:
                    return StringPrototype.concat(thisob, args);

                case JSBuiltin.String_fixed:
                    return StringPrototype.@fixed(thisob);

                case JSBuiltin.String_fontcolor:
                    return StringPrototype.fontcolor(thisob, GetArg(args, 0, length));

                case JSBuiltin.String_fontsize:
                    return StringPrototype.fontsize(thisob, GetArg(args, 0, length));

                case JSBuiltin.String_fromCharCode:
                    return StringConstructor.fromCharCode(args);

                case JSBuiltin.String_indexOf:
                    return StringPrototype.indexOf(thisob, GetArg(args, 0, length), Microsoft.JScript.Convert.ToNumber(GetArg(args, 1, length)));

                case JSBuiltin.String_italics:
                    return StringPrototype.italics(thisob);

                case JSBuiltin.String_lastIndexOf:
                    return StringPrototype.lastIndexOf(thisob, GetArg(args, 0, length), Microsoft.JScript.Convert.ToNumber(GetArg(args, 1, length)));

                case JSBuiltin.String_link:
                    return StringPrototype.link(thisob, GetArg(args, 0, length));

                case JSBuiltin.String_localeCompare:
                    return StringPrototype.localeCompare(thisob, GetArg(args, 0, length));

                case JSBuiltin.String_match:
                    return StringPrototype.match(thisob, engine, GetArg(args, 0, length));

                case JSBuiltin.String_replace:
                    return StringPrototype.replace(thisob, GetArg(args, 0, length), GetArg(args, 1, length));

                case JSBuiltin.String_search:
                    return StringPrototype.search(thisob, engine, GetArg(args, 0, length));

                case JSBuiltin.String_slice:
                    return StringPrototype.slice(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length));

                case JSBuiltin.String_small:
                    return StringPrototype.small(thisob);

                case JSBuiltin.String_split:
                    return StringPrototype.split(thisob, engine, GetArg(args, 0, length), GetArg(args, 1, length));

                case JSBuiltin.String_strike:
                    return StringPrototype.strike(thisob);

                case JSBuiltin.String_sub:
                    return StringPrototype.sub(thisob);

                case JSBuiltin.String_substr:
                    return StringPrototype.substr(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length));

                case JSBuiltin.String_substring:
                    return StringPrototype.substring(thisob, Microsoft.JScript.Convert.ToNumber(GetArg(args, 0, length)), GetArg(args, 1, length));

                case JSBuiltin.String_sup:
                    return StringPrototype.sup(thisob);

                case JSBuiltin.String_toLocaleLowerCase:
                    return StringPrototype.toLocaleLowerCase(thisob);

                case JSBuiltin.String_toLocaleUpperCase:
                    return StringPrototype.toLocaleUpperCase(thisob);

                case JSBuiltin.String_toLowerCase:
                    return StringPrototype.toLowerCase(thisob);

                case JSBuiltin.String_toString:
                    return StringPrototype.toString(thisob);

                case JSBuiltin.String_toUpperCase:
                    return StringPrototype.toUpperCase(thisob);

                case JSBuiltin.String_valueOf:
                    return StringPrototype.valueOf(thisob);

                case JSBuiltin.VBArray_dimensions:
                    return VBArrayPrototype.dimensions(thisob);

                case JSBuiltin.VBArray_getItem:
                    return VBArrayPrototype.getItem(thisob, args);

                case JSBuiltin.VBArray_lbound:
                    return VBArrayPrototype.lbound(thisob, GetArg(args, 0, length));

                case JSBuiltin.VBArray_toArray:
                    return VBArrayPrototype.toArray(thisob, engine);

                case JSBuiltin.VBArray_ubound:
                    return VBArrayPrototype.ubound(thisob, GetArg(args, 0, length));
            }
            return method.Invoke(thisob, BindingFlags.Default, JSBinder.ob, args, null);
        }

        public override string ToString()
        {
            return ("function " + base.name + "() {\n    [native code]\n}");
        }

        private static object[] VarArgs(object[] args, int offset, int n)
        {
            object[] objArray = new object[(n >= offset) ? (n - offset) : 0];
            for (int i = offset; i < n; i++)
            {
                objArray[i - offset] = args[i];
            }
            return objArray;
        }
    }
}

