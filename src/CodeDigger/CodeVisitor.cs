using CodeDigger.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace UsingCollectorCS
{
    public class CodeVisitor
    {
        protected CodeVisitor()
        {
        }

        public CodeVisitor(string filePath, string fileName, IDictionary<string, Node> nodes, IDictionary<string, Relation> relations)
        {
            FilePath = filePath;
            FileName = fileName;
            _nodes = nodes;
            _relations = relations;
            Register(EnumNodeKind.File, FilePath, FileName);
            System.Console.WriteLine(FilePath);
        }

        public string FilePath { get; }
        public string FileName { get; }

        private IDictionary<string, Node> _nodes;
        private IDictionary<string, Relation> _relations;

        private void Register(EnumNodeKind kind, string key, string name)
        {
            if (!_nodes.ContainsKey(key))
            {
                var node = new Node { Name = name, Key = key, Kind = kind };
                _nodes.Add(key, node);
            }
        }

        private void Relate(string fromKey, string toKey, EnumRelated relatedAs)
        {
            var key = fromKey + "." + toKey + "." + relatedAs.ToString();
            if (!_relations.ContainsKey(key))
            {
                var related = new Relation { FromKey = fromKey, ToKey = toKey, RelatedAs = relatedAs};
                _relations.Add(key, related);
            }
        }

        //private void Register(string container, EnumNodeKind kind, string name, EnumRelated related)
        //{
        //    // CREATE (john:Person {name: 'John'})
        //    // CREATE (Email:Client {id:'1', description:'email client', repo:'localgit:emailClient'})
        //    // "CREATE (:{kind} {id:'{id}', description:'email client', repo:'localgit:emailClient'})"
        //    var containserey = container;

        //    if (!_nodes.ContainsKey(key))
        //    {
        //        var node = new Node { Name = name, Key = key, Kind = kind };
        //        _nodes.Add(key, node);
        //    }

        //    key = container + "." + name;

        //    if (!_nodes.ContainsKey(key))
        //    {
        //        var node = new Node {Name = name,Key = key, Kind = kind };
        //        _nodes.Add(key, node);
        //    }

        //    var relatedKey = key + "." + related.ToString();

        //    if (!_relations.ContainsKey(relatedKey))
        //    {
        //        var relayed = new Relation { FromKey = container, ToKey = key, RelatedAs = related };
        //        _relations.Add(relatedKey, relayed);
        //    }
        //}

        public void Visit(InterfaceDeclarationSyntax node)
        {
            var namespaceNode = node.Parent as NamespaceDeclarationSyntax;
            if (namespaceNode == null)
            {
                namespaceNode = node.Parent.Parent as NamespaceDeclarationSyntax;
            }
            var key = namespaceNode.Name.ToString()+"."+node.Identifier.Text;
            Register(EnumNodeKind.Interface, key, node.Identifier.Text);
            Relate(namespaceNode.Name.ToString(), key, EnumRelated.DefinedIn);
            Visit(key, namespaceNode);
            Visit(key, node.Members);
        }

        public void Visit(EnumDeclarationSyntax node)
        {
            var namespaceNode = node.Parent as NamespaceDeclarationSyntax;
            
            var key = namespaceNode.Name.ToString() + "." + node.Identifier.Text;
            Register(EnumNodeKind.Enum, key, node.Identifier.Text);
            Relate(namespaceNode.Name.ToString(), key, EnumRelated.DefinedIn);
            Visit(key, namespaceNode);
            Visit(key, node.Members);
        }

        // TODO: Add support for Generics, Interface and inheritence
        public void Visit(ClassDeclarationSyntax node)
        {
            var namespaceNode = node.Parent as NamespaceDeclarationSyntax;
            if (namespaceNode == null)
            {
                namespaceNode = node.Parent.Parent as NamespaceDeclarationSyntax;
            }

            var key = namespaceNode.Name.ToString() + "." + node.Identifier.Text;
            Register(EnumNodeKind.Class, key, node.Identifier.Text);
            Relate(namespaceNode.Name.ToString(), key, EnumRelated.DefinedIn);
            Visit(FilePath, namespaceNode);
            Visit(key, node.Members);
        }

        private void Visit(string containerKey, NamespaceDeclarationSyntax node)
        {
            if (node == null) return;

            var key = node.Name.ToString();
            Register(EnumNodeKind.Namespace, key, node.Name.ToString());
            Relate(containerKey, key, EnumRelated.DefinedIn);
            Visit(key, node.Usings);
        }

        private void Visit(string containerKey, SyntaxList<UsingDirectiveSyntax> usings)
        {
            foreach (var usingNode in usings)
            {
                var key = usingNode.Name.ToString();
                Register(EnumNodeKind.Using, key, usingNode.Name.ToString());
                Relate(containerKey, key, EnumRelated.Uses);
            }
        }

        private void Visit(string containerKey, SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        {
            foreach (var enumMemberNode in members)
            {
                var key = containerKey+"."+enumMemberNode.ToString();
                Register(EnumNodeKind.EnumField, key, enumMemberNode.ToString());
                Relate(containerKey, key, EnumRelated.Of);
            }
        }

        private void Visit(string containerKey, SyntaxList<MemberDeclarationSyntax> members)
        {
            foreach (var node in members)
            {
                if (node is MethodDeclarationSyntax)
                    Visit(containerKey, node as MethodDeclarationSyntax);
                else if (node is PropertyDeclarationSyntax)
                    Visit(containerKey, node as PropertyDeclarationSyntax);
                else if (node is FieldDeclarationSyntax)
                    Visit(containerKey, node as FieldDeclarationSyntax);
                else if (node is ClassDeclarationSyntax)
                    Visit(node as ClassDeclarationSyntax);
                else if (node is InterfaceDeclarationSyntax)
                    Visit(node as InterfaceDeclarationSyntax);
                else if (node is EnumDeclarationSyntax)
                    Visit(node as EnumDeclarationSyntax);
                else if (node is ConstructorDeclarationSyntax)
                    Visit(containerKey, node as ConstructorDeclarationSyntax);
                else
                {
                    System.Console.WriteLine("\tmember XXX : " + node.ToString());
                }
            }
        }

        private void Visit(string containerKey, ConstructorDeclarationSyntax node)
        {
            var key = containerKey + "." + node.Identifier.Text;
            Register(EnumNodeKind.Constructor, key, node.Identifier.Text);
            Relate(containerKey, key, EnumRelated.DefinedIn);
            Visit(key, node.ParameterList.Parameters);
        }

        private void Visit(string containerKey, MethodDeclarationSyntax node)
        {
            var key = containerKey + "." + node.Identifier.Text;
            Register(EnumNodeKind.Method, key, node.Identifier.Text);
            Relate(containerKey, key, EnumRelated.DefinedIn);
            Visit(key, node.ParameterList.Parameters);
        }

        private void Visit(string containerKey, PropertyDeclarationSyntax node)
        {
            var key = containerKey + "." + node.Identifier.Text;
            Register(EnumNodeKind.Property, key, node.Identifier.Text);
            Relate(containerKey, key, EnumRelated.DefinedIn);
        }

        private void Visit(string containerKey, FieldDeclarationSyntax node)
        {
            var name = node.Declaration.Variables.First().Identifier.Text;
            var key = containerKey + "." + name;
            System.Console.WriteLine("\tfield: " + name);
            Register(EnumNodeKind.Field, key, name);
            Relate(containerKey, key, EnumRelated.DefinedIn);
        }

        private void Visit(string containerKey, SeparatedSyntaxList<ParameterSyntax> parameters)
        {
            foreach (var para in parameters)
            {
                var key = containerKey + "." + para.Identifier.Text;
                Register(EnumNodeKind.Parameter, key, para.Identifier.Text);
                Relate(containerKey, key, EnumRelated.DefinedIn);
                //System.Console.WriteLine(string.Format("\t\tpara: {0} {1} {2}", para.Modifiers.ToString(), para.Type.ToString(), para.Identifier.Text));
            }
        }
    }
}


 
