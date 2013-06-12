namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using System.Xml;

    [Obsolete("This class has been deprecated.  http://go.microsoft.com/fwlink/?linkid=14202")]
    public class DiagnosticsConfigurationHandler : IConfigurationSectionHandler
    {
        public virtual object Create(object parent, object configContext, XmlNode section)
        {
            Hashtable hashtable2;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            HandlerBase.CheckForUnrecognizedAttributes(section);
            Hashtable hashtable = (Hashtable) parent;
            if (hashtable == null)
            {
                hashtable2 = new Hashtable();
            }
            else
            {
                hashtable2 = (Hashtable) hashtable.Clone();
            }
            foreach (XmlNode node in section.ChildNodes)
            {
                if (HandlerBase.IsIgnorableAlsoCheckForNonElement(node))
                {
                    continue;
                }
                string name = node.Name;
                if (name == null)
                {
                    goto Label_0188;
                }
                if (!(name == "switches"))
                {
                    if (name == "assert")
                    {
                        goto Label_00E6;
                    }
                    if (name == "trace")
                    {
                        goto Label_011A;
                    }
                    if (name == "performanceCounters")
                    {
                        goto Label_014E;
                    }
                    goto Label_0188;
                }
                if (flag)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("ConfigSectionsUnique", new object[] { "switches" }));
                }
                flag = true;
                HandleSwitches(hashtable2, node, configContext);
                goto Label_018F;
            Label_00E6:
                if (flag2)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("ConfigSectionsUnique", new object[] { "assert" }));
                }
                flag2 = true;
                HandleAssert(hashtable2, node, configContext);
                goto Label_018F;
            Label_011A:
                if (flag3)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("ConfigSectionsUnique", new object[] { "trace" }));
                }
                flag3 = true;
                HandleTrace(hashtable2, node, configContext);
                goto Label_018F;
            Label_014E:
                if (flag4)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("ConfigSectionsUnique", new object[] { "performanceCounters" }));
                }
                flag4 = true;
                HandleCounters((Hashtable) parent, hashtable2, node, configContext);
                goto Label_018F;
            Label_0188:
                HandlerBase.ThrowUnrecognizedElement(node);
            Label_018F:
                HandlerBase.CheckForUnrecognizedAttributes(node);
            }
            return hashtable2;
        }

        private static void HandleAssert(Hashtable config, XmlNode assertNode, object context)
        {
            bool val = false;
            if (HandlerBase.GetAndRemoveBooleanAttribute(assertNode, "assertuienabled", ref val) != null)
            {
                config["assertuienabled"] = val;
            }
            string str = null;
            if (HandlerBase.GetAndRemoveStringAttribute(assertNode, "logfilename", ref str) != null)
            {
                config["logfilename"] = str;
            }
            HandlerBase.CheckForChildNodes(assertNode);
        }

        private static void HandleCounters(Hashtable parent, Hashtable config, XmlNode countersNode, object context)
        {
            int val = 0;
            if ((HandlerBase.GetAndRemoveIntegerAttribute(countersNode, "filemappingsize", ref val) != null) && (parent == null))
            {
                config["filemappingsize"] = val;
            }
            HandlerBase.CheckForChildNodes(countersNode);
        }

        private static void HandleListeners(Hashtable config, XmlNode listenersNode, object context)
        {
            HandlerBase.CheckForUnrecognizedAttributes(listenersNode);
            foreach (XmlNode node in listenersNode.ChildNodes)
            {
                string str5;
                if (HandlerBase.IsIgnorableAlsoCheckForNonElement(node))
                {
                    continue;
                }
                string val = null;
                string str2 = null;
                string str3 = null;
                string name = node.Name;
                if (((str5 = name) == null) || (((str5 != "add") && (str5 != "remove")) && (str5 != "clear")))
                {
                    HandlerBase.ThrowUnrecognizedElement(node);
                }
                HandlerBase.GetAndRemoveStringAttribute(node, "name", ref val);
                HandlerBase.GetAndRemoveStringAttribute(node, "type", ref str2);
                HandlerBase.GetAndRemoveStringAttribute(node, "initializeData", ref str3);
                HandlerBase.CheckForUnrecognizedAttributes(node);
                HandlerBase.CheckForChildNodes(node);
                TraceListener listener = null;
                if (str2 != null)
                {
                    Type c = Type.GetType(str2);
                    if (c == null)
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("Could_not_find_type", new object[] { str2 }));
                    }
                    if (!typeof(TraceListener).IsAssignableFrom(c))
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("Type_isnt_tracelistener", new object[] { str2 }));
                    }
                    if (str3 == null)
                    {
                        ConstructorInfo constructor = c.GetConstructor(new Type[0]);
                        if (constructor == null)
                        {
                            throw new ConfigurationErrorsException(System.SR.GetString("Could_not_get_constructor", new object[] { str2 }));
                        }
                        listener = (TraceListener) SecurityUtils.ConstructorInfoInvoke(constructor, new object[0]);
                    }
                    else
                    {
                        ConstructorInfo ctor = c.GetConstructor(new Type[] { typeof(string) });
                        if (ctor == null)
                        {
                            throw new ConfigurationErrorsException(System.SR.GetString("Could_not_get_constructor", new object[] { str2 }));
                        }
                        listener = (TraceListener) SecurityUtils.ConstructorInfoInvoke(ctor, new object[] { str3 });
                    }
                    if (val != null)
                    {
                        listener.Name = val;
                    }
                }
                switch (name[0])
                {
                    case 'a':
                        if (listener == null)
                        {
                            throw new ConfigurationErrorsException(System.SR.GetString("Could_not_create_listener", new object[] { val }));
                        }
                        break;

                    case 'c':
                    {
                        Trace.Listeners.Clear();
                        continue;
                    }
                    case 'r':
                    {
                        if (listener != null)
                        {
                            goto Label_0258;
                        }
                        if (val == null)
                        {
                            throw new ConfigurationErrorsException(System.SR.GetString("Cannot_remove_with_null"));
                        }
                        Trace.Listeners.Remove(val);
                        continue;
                    }
                    default:
                        goto Label_0272;
                }
                Trace.Listeners.Add(listener);
                continue;
            Label_0258:
                Trace.Listeners.Remove(listener);
                continue;
            Label_0272:
                HandlerBase.ThrowUnrecognizedElement(node);
            }
        }

        private static void HandleSwitches(Hashtable config, XmlNode switchesNode, object context)
        {
            Hashtable hashtable = (Hashtable) new SwitchesDictionarySectionHandler().Create(config["switches"], context, switchesNode);
            IDictionaryEnumerator enumerator = hashtable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                try
                {
                    int.Parse((string) enumerator.Value, CultureInfo.InvariantCulture);
                    continue;
                }
                catch
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("Value_must_be_numeric", new object[] { enumerator.Key }));
                }
            }
            config["switches"] = hashtable;
        }

        private static void HandleTrace(Hashtable config, XmlNode traceNode, object context)
        {
            bool flag = false;
            bool val = false;
            if (HandlerBase.GetAndRemoveBooleanAttribute(traceNode, "autoflush", ref val) != null)
            {
                config["autoflush"] = val;
            }
            int num = 0;
            if (HandlerBase.GetAndRemoveIntegerAttribute(traceNode, "indentsize", ref num) != null)
            {
                config["indentsize"] = num;
            }
            foreach (XmlNode node in traceNode.ChildNodes)
            {
                if (!HandlerBase.IsIgnorableAlsoCheckForNonElement(node))
                {
                    if (node.Name == "listeners")
                    {
                        if (flag)
                        {
                            throw new ConfigurationErrorsException(System.SR.GetString("ConfigSectionsUnique", new object[] { "listeners" }));
                        }
                        flag = true;
                        HandleListeners(config, node, context);
                    }
                    else
                    {
                        HandlerBase.ThrowUnrecognizedElement(node);
                    }
                }
            }
        }
    }
}

