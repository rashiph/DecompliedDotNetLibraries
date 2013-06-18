namespace Microsoft.JScript
{
    using System;

    public sealed class LenientStringPrototype : StringPrototype
    {
        public object anchor;
        public object big;
        public object blink;
        public object bold;
        public object charAt;
        public object charCodeAt;
        public object concat;
        public object constructor;
        public object @fixed;
        public object fontcolor;
        public object fontsize;
        public object indexOf;
        public object italics;
        public object lastIndexOf;
        public object link;
        public object localeCompare;
        public object match;
        public object replace;
        public object search;
        public object slice;
        public object small;
        public object split;
        public object strike;
        public object sub;
        [NotRecommended("substr")]
        public object substr;
        public object substring;
        public object sup;
        public object toLocaleLowerCase;
        public object toLocaleUpperCase;
        public object toLowerCase;
        public object toString;
        public object toUpperCase;
        public object valueOf;

        internal LenientStringPrototype(LenientFunctionPrototype funcprot, LenientObjectPrototype parent) : base(funcprot, parent)
        {
            base.noExpando = false;
            Type type = typeof(StringPrototype);
            this.anchor = new BuiltinFunction("anchor", this, type.GetMethod("anchor"), funcprot);
            this.big = new BuiltinFunction("big", this, type.GetMethod("big"), funcprot);
            this.blink = new BuiltinFunction("blink", this, type.GetMethod("blink"), funcprot);
            this.bold = new BuiltinFunction("bold", this, type.GetMethod("bold"), funcprot);
            this.charAt = new BuiltinFunction("charAt", this, type.GetMethod("charAt"), funcprot);
            this.charCodeAt = new BuiltinFunction("charCodeAt", this, type.GetMethod("charCodeAt"), funcprot);
            this.concat = new BuiltinFunction("concat", this, type.GetMethod("concat"), funcprot);
            this.@fixed = new BuiltinFunction("fixed", this, type.GetMethod("fixed"), funcprot);
            this.fontcolor = new BuiltinFunction("fontcolor", this, type.GetMethod("fontcolor"), funcprot);
            this.fontsize = new BuiltinFunction("fontsize", this, type.GetMethod("fontsize"), funcprot);
            this.indexOf = new BuiltinFunction("indexOf", this, type.GetMethod("indexOf"), funcprot);
            this.italics = new BuiltinFunction("italics", this, type.GetMethod("italics"), funcprot);
            this.lastIndexOf = new BuiltinFunction("lastIndexOf", this, type.GetMethod("lastIndexOf"), funcprot);
            this.link = new BuiltinFunction("link", this, type.GetMethod("link"), funcprot);
            this.localeCompare = new BuiltinFunction("localeCompare", this, type.GetMethod("localeCompare"), funcprot);
            this.match = new BuiltinFunction("match", this, type.GetMethod("match"), funcprot);
            this.replace = new BuiltinFunction("replace", this, type.GetMethod("replace"), funcprot);
            this.search = new BuiltinFunction("search", this, type.GetMethod("search"), funcprot);
            this.slice = new BuiltinFunction("slice", this, type.GetMethod("slice"), funcprot);
            this.small = new BuiltinFunction("small", this, type.GetMethod("small"), funcprot);
            this.split = new BuiltinFunction("split", this, type.GetMethod("split"), funcprot);
            this.strike = new BuiltinFunction("strike", this, type.GetMethod("strike"), funcprot);
            this.sub = new BuiltinFunction("sub", this, type.GetMethod("sub"), funcprot);
            this.substr = new BuiltinFunction("substr", this, type.GetMethod("substr"), funcprot);
            this.substring = new BuiltinFunction("substring", this, type.GetMethod("substring"), funcprot);
            this.sup = new BuiltinFunction("sup", this, type.GetMethod("sup"), funcprot);
            this.toLocaleLowerCase = new BuiltinFunction("toLocaleLowerCase", this, type.GetMethod("toLocaleLowerCase"), funcprot);
            this.toLocaleUpperCase = new BuiltinFunction("toLocaleUpperCase", this, type.GetMethod("toLocaleUpperCase"), funcprot);
            this.toLowerCase = new BuiltinFunction("toLowerCase", this, type.GetMethod("toLowerCase"), funcprot);
            this.toString = new BuiltinFunction("toString", this, type.GetMethod("toString"), funcprot);
            this.toUpperCase = new BuiltinFunction("toUpperCase", this, type.GetMethod("toUpperCase"), funcprot);
            this.valueOf = new BuiltinFunction("valueOf", this, type.GetMethod("valueOf"), funcprot);
        }
    }
}

