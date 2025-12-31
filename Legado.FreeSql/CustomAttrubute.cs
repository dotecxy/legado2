using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.FreeSql
{

    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string Name { get; set; }

        public bool WithoutRowId { get; set; }

        public TableAttribute(string name)
        {
            Name = name;
        }
    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IndexedAttribute : Attribute
    {
        public string Name { get; set; }
        public string Field { get; set; }

        public int Order { get; set; }

        public virtual bool Unique { get; set; }

        public IndexedAttribute()
        {
        }

        public IndexedAttribute(string name, int order)
        {
            Name = name;
            Order = order;
        }
        public IndexedAttribute(string name,string field, int order)
        {
            Name = name;
            Order = order;
            Field = field;
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; set; }

        public ColumnAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : IndexedAttribute
    {
        public override bool Unique
        {
            get
            {
                return true;
            }
            set
            {
            }
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AutoIncrementAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NotNullAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
    }

    //[AttributeUsage(AttributeTargets.Enum)]
    //public class StoreAsTextAttribute : Attribute
    //{
    //}
     
}
