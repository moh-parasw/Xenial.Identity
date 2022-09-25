using System;
using System.Collections.Generic;
using System.Linq;

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
                    _ = list.Add(item);
                }
            }
            return list;
        }

        public override object ConvertToStorageType(object value) => value is ICollection<string> collection ? !collection.Any() ? null : (object)collection.Aggregate((x, y) => $"{x},{y}") : null;
    }
}
