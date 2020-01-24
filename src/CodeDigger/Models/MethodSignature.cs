using System.Collections.Generic;
using System.Text;

namespace CodeDigger.Models
{
    public class MethodSignature : TypeSignature
    {
        public MethodSignature(string modifier, string returnType, string name, IEnumerable<TypeSignature> parameters) 
            : base(TypeSignatureEnum.Method, modifier, returnType, name)
        {
            Parameters = parameters;
        }

        public IEnumerable<TypeSignature> Parameters { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            foreach (var para in Parameters)
            {
                sb.Append( " " + para.ToString() + " ");
            }
            return sb.ToString();
        }
    }
}

