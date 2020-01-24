using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeDigger.Models
{
    public class Node
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public EnumNodeKind Kind { get; set; }
    }

    public class Relation
    {
        public string FromKey { get; set; }
        public string ToKey { get; set; }
        public EnumRelated RelatedAs { get; set; }
    }

    public enum EnumNodeKind
    {
        File,
        Namespace,
        Using,
        Class,
        Interface,
        Enum,
        EnumField,
        Method,
        Constructor,
        Property,
        Field,
        Parameter
    }

    public enum EnumRelated
    {
        DefinedIn,
        Of,
        Uses,
        Implements,
        inherited
    }
}
