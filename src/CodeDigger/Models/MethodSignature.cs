using System.Collections.Generic;
using System.Text;

namespace CodeDigger.Models
{
    public class MethodSignature
    {
        private MethodSignature() { }

        public MethodSignature(TypeSignature method, IList<TypeSignature> properties)
        {
            Method = method;
            Properties = properties;
        }
        public TypeSignature Method { get; }
        
        public IEnumerable<TypeSignature> Properties
        {
            get;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Method.ToString());
            foreach (var para in Properties)
            {
                sb.Append( " " + para.ToString() + " ");
            }
            return sb.ToString();
        }
    }
}

