namespace System.Diagnostics.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Design;

    internal class StringDictionaryEditor : CollectionEditor
    {
        public StringDictionaryEditor(Type type) : base(type)
        {
        }

        protected override CollectionEditor.CollectionForm CreateCollectionForm()
        {
            CollectionEditor.CollectionForm form = base.CreateCollectionForm();
            form.Text = System.Design.SR.GetString("StringDictionaryEditorTitle");
            form.CollectionEditable = true;
            return form;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(EditableDictionaryEntry);
        }

        protected override object CreateInstance(Type itemType)
        {
            return new EditableDictionaryEntry("name", "value");
        }

        protected override object[] GetItems(object editValue)
        {
            if (editValue == null)
            {
                return new object[0];
            }
            StringDictionary dictionary = editValue as StringDictionary;
            if (dictionary == null)
            {
                throw new ArgumentNullException("editValue");
            }
            object[] objArray = new object[dictionary.Count];
            int num = 0;
            foreach (DictionaryEntry entry in dictionary)
            {
                EditableDictionaryEntry entry2 = new EditableDictionaryEntry((string) entry.Key, (string) entry.Value);
                objArray[num++] = entry2;
            }
            return objArray;
        }

        protected override object SetItems(object editValue, object[] value)
        {
            StringDictionary dictionary = editValue as StringDictionary;
            if (dictionary == null)
            {
                throw new ArgumentNullException("editValue");
            }
            dictionary.Clear();
            foreach (EditableDictionaryEntry entry in value)
            {
                dictionary[entry.Name] = entry.Value;
            }
            return dictionary;
        }
    }
}

