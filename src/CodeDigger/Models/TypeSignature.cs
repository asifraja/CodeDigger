using System;

namespace CodeDigger.Models
{
    public class TypeSignature
    {
        public string Modifier { get; set; }
        public string Type { get; set; }
        public String Name { get; set; }
        public static TypeSignature Create(string modifier, string type, string name)
        {
            return new TypeSignature { Modifier = modifier, Type = type, Name = name };
        }

        public override string ToString()
        {
            return $"{Modifier} {Type} {Name}";
        }
    }
}