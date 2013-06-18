namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    public class CompositeActivityCodeGenerator : ActivityCodeGenerator
    {
        public override void GenerateCode(CodeGenerationManager manager, object obj)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            CompositeActivity compositeActivity = obj as CompositeActivity;
            if (compositeActivity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(CompositeActivity).FullName }), "obj");
            }
            base.GenerateCode(manager, obj);
            foreach (Activity activity2 in Helpers.GetAllEnabledActivities(compositeActivity))
            {
                foreach (ActivityCodeGenerator generator in manager.GetCodeGenerators(activity2.GetType()))
                {
                    generator.GenerateCode(manager, activity2);
                }
            }
        }
    }
}

