using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolboxDesigner.Core
{
    public class Property
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Tooltip { get; set; }
        public string CatalogName { get; set; }
        public string DefaultValue { get; set; }
        public string Value { get; set; }
        public bool ReadOnly { get; set; }
        public string EnumVariants { get; set; }
    }
}
