namespace CodeDigger.Models
{
    public class Node
    {
        private static long _idIndex = 0;
        public Node()
        {
            _idIndex++;
            Id = _idIndex;
        }
        public long Id { get; }
        public string Key { get; set; }
        public string Name { get; set; }
        public EnumKInd Kind { get; set; }
        public string KindOf { get; set; }
        public string Properties { get; set; }
    }

    public class EdgeNode
    {
        private static int _idIndex = 0;
        public EdgeNode()
        {
            _idIndex++;
            Id = _idIndex;
        }
        public long Id { get; set; }
        public long SourceId { get; set; }
        public long TargetId { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public EnumRelations Related { get; set; }
        public string RelatedAs { get; set; }
        public string Properties { get; set; }
    }

    public enum EnumKInd
    {
        File=10,
        Namespace = 20,
        Using = 30,
        Class = 40,
        Struct = 50,
        Interface = 60,
        Enum = 70,
        EnumField = 80,
        Method = 90,
        Constructor = 100,
        Property = 110,
        Field = 120,
        Parameter = 130,
        Operator = 140,
        ImplicitOperator = 150,
        ExplicitOperator = 160,
        Delegate = 170,
        AccessorList = 180
    }

    public enum EnumRelations
    {
        DefinedIn = 500,
        Of = 600,
        Uses = 700,
        Implements = 800,
        inherited = 900,
    }
}
