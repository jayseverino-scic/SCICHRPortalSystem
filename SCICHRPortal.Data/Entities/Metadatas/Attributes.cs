using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace SCICHRPortal.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IncludeIndexAttribute : Attribute
    {
        public string Name { get; set; }
        public string[] PropertyNames { get; set; }
        public string[] IncludeProperties { get; set; }
        public bool IsUnique { get; set; }
        public bool IsClustered { get; set; }

        public IncludeIndexAttribute(string name, string[] propertyNames, string[] includeProperties = null)
        {
            Name = name;
            PropertyNames = propertyNames;
            IncludeProperties = includeProperties ?? Array.Empty<string>();
            IsUnique = false;
            IsClustered = false;
        }
    }
}