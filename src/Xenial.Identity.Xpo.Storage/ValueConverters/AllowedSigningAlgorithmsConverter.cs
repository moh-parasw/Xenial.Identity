using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DevExpress.Xpo.Metadata;

namespace Xenial.Identity.Xpo.Storage.ValueConverters
{
    public class AllowedSigningAlgorithmsConverter : ValueConverter
    {
        public override Type StorageType => typeof(string);

        public override object ConvertFromStorageType(object value)
        {
            var list = new HashSet<string>();
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                str = str.Trim();
                foreach (var item in str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct())
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public override object ConvertToStorageType(object value)
        {
            if (value is ICollection<string> collection)
            {
                if (!collection.Any())
                {
                    return null;
                }
                return collection.Aggregate((x, y) => $"{x},{y}");
            }
            return null;
        }
    }
}
