namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Text;

    [Serializable]
    internal sealed class DirectoryGroupQuery : IDirectoryOperation
    {
        private static string BuildUri(string propValue)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("LDAP://");
            for (int i = 0; i < propValue.Length; i++)
            {
                if (propValue[i] == '/')
                {
                    builder.Append(@"\/");
                }
                else
                {
                    builder.Append(propValue[i]);
                }
            }
            return builder.ToString();
        }

        private static bool Contains(ICollection propertyNames, string testPropertyName)
        {
            foreach (string str in propertyNames)
            {
                if (string.Compare(str, testPropertyName, StringComparison.Ordinal) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void GetResult(DirectoryEntry rootEntry, DirectoryEntry currentEntry, List<DirectoryEntry> response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            Stack<DirectoryEntry> stack = new Stack<DirectoryEntry>();
            stack.Push(currentEntry);
            while (stack.Count != 0)
            {
                DirectoryEntry item = stack.Pop();
                bool flag = false;
                if (Contains(item.Properties.PropertyNames, "objectClass"))
                {
                    foreach (string str in item.Properties["objectClass"])
                    {
                        if (string.Compare(str, ActiveDirectoryRoleFactory.Configuration.Group, StringComparison.Ordinal) == 0)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        if (Contains(item.Properties.PropertyNames, ActiveDirectoryRoleFactory.Configuration.Member))
                        {
                            foreach (string str2 in item.Properties[ActiveDirectoryRoleFactory.Configuration.Member])
                            {
                                stack.Push(new DirectoryEntry(BuildUri(str2)));
                            }
                        }
                    }
                    else
                    {
                        response.Add(item);
                    }
                }
            }
        }
    }
}

