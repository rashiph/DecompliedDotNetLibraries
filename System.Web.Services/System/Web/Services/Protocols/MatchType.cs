namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class MatchType
    {
        private MatchMember[] fields;
        private System.Type type;

        internal object Match(string text)
        {
            object target = Activator.CreateInstance(this.type);
            for (int i = 0; i < this.fields.Length; i++)
            {
                this.fields[i].Match(target, text);
            }
            return target;
        }

        internal static MatchType Reflect(System.Type type)
        {
            MatchType type2 = new MatchType {
                type = type
            };
            MemberInfo[] members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            ArrayList list = new ArrayList();
            for (int i = 0; i < members.Length; i++)
            {
                MatchMember member = MatchMember.Reflect(members[i]);
                if (member != null)
                {
                    list.Add(member);
                }
            }
            type2.fields = (MatchMember[]) list.ToArray(typeof(MatchMember));
            return type2;
        }

        internal System.Type Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

