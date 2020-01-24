using System;

namespace CodeDigger.Models
{
    public enum TypeSignatureEnum
    {
        Namespace,
        Class,
        Method,
        MethodParameter
    }

    public class TypeSignature
    {
        public string Modifier { get; }
        public string ReturnType { get; }
        public String Name { get;  }

        public TypeSignatureEnum TypeSignatureEnum { get;  }

        private TypeSignature() {}

        public TypeSignature(TypeSignatureEnum signatureEnum, string modifier, string returnType, string name)
        {
            TypeSignatureEnum = signatureEnum;
            Modifier = modifier; 
            ReturnType = returnType;
            Name = name ;
        }

        public override string ToString()
        {
            return $"{Modifier} {ReturnType} {Name}";
        }
    }
}