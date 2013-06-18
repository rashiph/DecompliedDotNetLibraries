namespace System.Activities
{
    using System;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class ActivityLocationReferenceEnvironment : LocationReferenceEnvironment
    {
        private Dictionary<string, LocationReference> declarations;
        private List<LocationReference> unnamedDeclarations;

        public ActivityLocationReferenceEnvironment()
        {
        }

        public ActivityLocationReferenceEnvironment(LocationReferenceEnvironment parent)
        {
            base.Parent = parent;
            if (base.Parent != null)
            {
                this.InternalRoot = parent.Root;
            }
        }

        public void Declare(LocationReference locationReference, Activity owner, ref IList<ValidationError> validationErrors)
        {
            if (locationReference.Name == null)
            {
                if (this.unnamedDeclarations == null)
                {
                    this.unnamedDeclarations = new List<LocationReference>();
                }
                this.unnamedDeclarations.Add(locationReference);
            }
            else if (this.Declarations.ContainsKey(locationReference.Name))
            {
                string id = null;
                if (owner != null)
                {
                    id = owner.Id;
                }
                ValidationError data = new ValidationError(System.Activities.SR.SymbolNamesMustBeUnique(locationReference.Name)) {
                    Source = owner,
                    Id = id
                };
                ActivityUtilities.Add<ValidationError>(ref validationErrors, data);
            }
            else
            {
                this.Declarations.Add(locationReference.Name, locationReference);
            }
        }

        public override IEnumerable<LocationReference> GetLocationReferences()
        {
            return this.Declarations.Values;
        }

        public override bool IsVisible(LocationReference locationReference)
        {
            if (locationReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("locationReference");
            }
            for (LocationReferenceEnvironment environment = this; environment != null; environment = environment.Parent)
            {
                ActivityLocationReferenceEnvironment environment2 = environment as ActivityLocationReferenceEnvironment;
                if (environment2 == null)
                {
                    return environment.IsVisible(locationReference);
                }
                if (environment2.declarations != null)
                {
                    foreach (LocationReference reference in environment2.declarations.Values)
                    {
                        if (locationReference == reference)
                        {
                            return true;
                        }
                    }
                }
                if (environment2.unnamedDeclarations != null)
                {
                    for (int i = 0; i < environment2.unnamedDeclarations.Count; i++)
                    {
                        if (locationReference == environment2.unnamedDeclarations[i])
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override bool TryGetLocationReference(string name, out LocationReference result)
        {
            if (name == null)
            {
                LocationReferenceEnvironment parent = base.Parent;
                while (parent is ActivityLocationReferenceEnvironment)
                {
                    parent = parent.Parent;
                }
                if (parent != null)
                {
                    return parent.TryGetLocationReference(name, out result);
                }
            }
            else
            {
                if ((this.declarations != null) && this.declarations.TryGetValue(name, out result))
                {
                    return true;
                }
                bool flag = false;
                LocationReferenceEnvironment environment2 = base.Parent;
                while ((environment2 != null) && (environment2 is ActivityLocationReferenceEnvironment))
                {
                    ActivityLocationReferenceEnvironment environment3 = (ActivityLocationReferenceEnvironment) environment2;
                    if ((environment3.declarations != null) && environment3.declarations.TryGetValue(name, out result))
                    {
                        return true;
                    }
                    environment2 = environment2.Parent;
                }
                if ((!flag && (environment2 != null)) && environment2.TryGetLocationReference(name, out result))
                {
                    return true;
                }
            }
            result = null;
            return false;
        }

        private Dictionary<string, LocationReference> Declarations
        {
            get
            {
                if (this.declarations == null)
                {
                    this.declarations = new Dictionary<string, LocationReference>();
                }
                return this.declarations;
            }
        }

        public Activity InternalRoot { get; set; }

        public override Activity Root
        {
            get
            {
                return this.InternalRoot;
            }
        }
    }
}

