using CodeDigger.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace UsingCollectorCS
{
    public class CodeVisitor
    {
        //    // CREATE (Email:Client {id:'1', description:'email client', repo:'localgit:emailClient'})
        //    // "CREATE (:{kind} {id:'{id}', description:'email client', repo:'localgit:emailClient'})"
        protected CodeVisitor()
        {
        }

        public CodeVisitor(string filePath, string fileName, IDictionary<string, Node> nodes, IDictionary<string, EdgeNode> edges)
        {
            FilePath = filePath;
            FileName = fileName;
            _nodes = nodes;
            _edges = edges;
            Register(EnumKInd.File, FilePath, FileName);
            //System.Console.WriteLine(FilePath);
        }

        public string FilePath { get; }
        public string FileName { get; }

        private IDictionary<string, Node> _nodes;
        private IDictionary<string, EdgeNode> _edges;

        private Node Register(EnumKInd kind, string key, string name)
        {
            if (!_nodes.ContainsKey(key))
            {
                var node = new Node { Name = name, Key = key, Kind = kind};
                _nodes.Add(key, node);
                return node;
            }
            return null;
        }

        private EdgeNode Relate(string fromKey, string toKey, EnumRelations relatedAs)
        {
            var key = fromKey + "." + toKey + "." + relatedAs.ToString();
            if (!_edges.ContainsKey(key))
            {
                var related = new EdgeNode { Source = fromKey, Target = toKey, Related = relatedAs};
                if(_nodes.ContainsKey(fromKey))
                {
                    related.SourceId = _nodes[fromKey].Id;
                }
                if (_nodes.ContainsKey(toKey))
                {
                    related.TargetId = _nodes[toKey].Id;
                }
                _edges.Add(key, related);
                return related;
            }
            return null;
        }

        public void Visit(InterfaceDeclarationSyntax syntaxNode)
        {
            var namespaceNode = syntaxNode.Parent as NamespaceDeclarationSyntax;
            if (namespaceNode == null)
            {
                namespaceNode = syntaxNode.Parent.Parent as NamespaceDeclarationSyntax;
            }
            var key = namespaceNode.Name.ToString()+"."+syntaxNode.Identifier.Text;
            Register(EnumKInd.Interface, key, syntaxNode.Identifier.Text);
            Relate(namespaceNode.Name.ToString(), key, EnumRelations.DefinedIn);
            Visit(key, namespaceNode);
            Visit(key, syntaxNode.Members);
        }

        public void Visit(EnumDeclarationSyntax syntaxNode)
        {
            var namespaceNode = syntaxNode.Parent as NamespaceDeclarationSyntax;
            if (namespaceNode == null)
            {
                namespaceNode = syntaxNode.Parent.Parent as NamespaceDeclarationSyntax;
            }
            var key = namespaceNode.Name.ToString() + "." + syntaxNode.Identifier.Text;
            Register(EnumKInd.Enum, key, syntaxNode.Identifier.Text);
            Relate(namespaceNode.Name.ToString(), key, EnumRelations.DefinedIn);
            Visit(key, namespaceNode);
            Visit(key, syntaxNode.Members);
        }

        public void Visit(ClassOrStructConstraintSyntax syntaxNode)
        {
            var namespaceNode = syntaxNode.Parent as NamespaceDeclarationSyntax;
            if (namespaceNode == null)
            {
                namespaceNode = syntaxNode.Parent.Parent as NamespaceDeclarationSyntax;
            }

            //var key = namespaceNode.Name.ToString() + "." + node.Identifier.Text;
            //Register(EnumNodeKind.Class, key, node.Identifier.Text);
            //Relate(namespaceNode.Name.ToString(), key, EnumRelated.DefinedIn);
            //Visit(FilePath, namespaceNode);
            //Visit(key, node.Members);
        }

        // TODO: Add support for Generics, Interface and inheritence
        public void Visit(ClassDeclarationSyntax syntaxNode)
        {
            var namespaceNode = syntaxNode.Parent as NamespaceDeclarationSyntax;
            if (namespaceNode == null)
            {
                namespaceNode = syntaxNode.Parent.Parent as NamespaceDeclarationSyntax;
            }

            var key = namespaceNode.Name.ToString() + "." + syntaxNode.Identifier.Text;
            Register(EnumKInd.Class, key, syntaxNode.Identifier.Text);
            Relate(namespaceNode.Name.ToString(), key, EnumRelations.DefinedIn);
            Visit(FilePath, namespaceNode);
            Visit(key, syntaxNode.Members);
        }

        private void Visit(string containerKey, NamespaceDeclarationSyntax syntaxNode)
        {
            if (syntaxNode == null) return;

            var key = syntaxNode.Name.ToString();
            Register(EnumKInd.Namespace, key, syntaxNode.Name.ToString());
            Relate(containerKey, key, EnumRelations.DefinedIn);
            Visit(key, syntaxNode.Usings);
        }

        private void Visit(string containerKey, SyntaxList<UsingDirectiveSyntax> usings)
        {
            foreach (var usingNode in usings)
            {
                var key = usingNode.Name.ToString();
                Register(EnumKInd.Using, key, usingNode.Name.ToString());
                Relate(containerKey, key, EnumRelations.Uses);
            }
        }

        private void Visit(string containerKey, SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        {
            foreach (var enumMemberNode in members)
            {
                var key = containerKey+"."+enumMemberNode.ToString();
                Register(EnumKInd.EnumField, key, enumMemberNode.ToString());
                Relate(containerKey, key, EnumRelations.Of);
            }
        }

        public void Visit(StructDeclarationSyntax node,string containerKey = "")
        {
            var namespaceNode = node.Parent as NamespaceDeclarationSyntax;
            if (namespaceNode == null)
            {
                namespaceNode = node.Parent.Parent as NamespaceDeclarationSyntax;
            }
            var key = containerKey.Length > 0? containerKey + "." + node.Identifier.Text : namespaceNode.Name.ToString() + "." + node.Identifier.Text;
            Register(EnumKInd.Struct, key, node.Identifier.Text);
            Relate(containerKey.Length > 0 ? containerKey : namespaceNode.Name.ToString(), key, EnumRelations.DefinedIn);
            Visit(FilePath, namespaceNode);
            Visit(key, node.Members);
        }

        private void Visit(string containerKey, SyntaxList<MemberDeclarationSyntax> members)
        {
            foreach (var syntaxNode in members)
            {
                if (syntaxNode is MethodDeclarationSyntax)
                    Visit(containerKey, syntaxNode as MethodDeclarationSyntax);
                else if (syntaxNode is PropertyDeclarationSyntax)
                    Visit(containerKey, syntaxNode as PropertyDeclarationSyntax);
                else if (syntaxNode is FieldDeclarationSyntax)
                    Visit(containerKey, syntaxNode as FieldDeclarationSyntax);
                else if (syntaxNode is ClassDeclarationSyntax)
                    Visit(syntaxNode as ClassDeclarationSyntax);
                else if (syntaxNode is InterfaceDeclarationSyntax)
                    Visit(syntaxNode as InterfaceDeclarationSyntax);
                else if (syntaxNode is EnumDeclarationSyntax)
                    Visit(syntaxNode as EnumDeclarationSyntax);
                else if (syntaxNode is ConstructorDeclarationSyntax)
                    Visit(containerKey, syntaxNode as ConstructorDeclarationSyntax);
                else if (syntaxNode is StructDeclarationSyntax)
                    Visit(syntaxNode as StructDeclarationSyntax, containerKey);
                else if (syntaxNode is OperatorDeclarationSyntax)
                    Visit(containerKey, syntaxNode as OperatorDeclarationSyntax);
                else
                {
                    System.Console.WriteLine("\tmember XXX : " + syntaxNode.ToString());
                }
            }
        }

        private void Visit(string containerKey, OperatorDeclarationSyntax syntaxNode)
        {
            var key = containerKey + "." + syntaxNode.OperatorToken.Text;
            Register(EnumKInd.Operator, key, syntaxNode.OperatorToken.Text);
            Relate(containerKey, key, EnumRelations.DefinedIn);
            Visit(key, syntaxNode.ParameterList.Parameters);
        }

        private void Visit(string containerKey, ConstructorDeclarationSyntax syntaxNode)
        {
            var key = containerKey + "." + syntaxNode.Identifier.Text;
            Register(EnumKInd.Constructor, key, syntaxNode.Identifier.Text);
            Relate(containerKey, key, EnumRelations.DefinedIn);
            Visit(key, syntaxNode.ParameterList.Parameters);
        }

        private void Visit(string containerKey, MethodDeclarationSyntax syntaxNode)
        {
            var key = containerKey + "." + syntaxNode.Identifier.Text;
            Register(EnumKInd.Method, key, syntaxNode.Identifier.Text);
            Relate(containerKey, key, EnumRelations.DefinedIn);
            Visit(key, syntaxNode.ParameterList.Parameters);
        }

        private void Visit(string containerKey, PropertyDeclarationSyntax syntaxNode)
        {
            var key = containerKey + "." + syntaxNode.Identifier.Text;
            Register(EnumKInd.Property, key, syntaxNode.Identifier.Text);
            Relate(containerKey, key, EnumRelations.DefinedIn);
        }

        private void Visit(string containerKey, FieldDeclarationSyntax syntaxNode)
        {
            var name = syntaxNode.Declaration.Variables.First().Identifier.Text;
            var key = containerKey + "." + name;
            //System.Console.WriteLine("\tfield: " + name);
            Register(EnumKInd.Field, key, name);
            Relate(containerKey, key, EnumRelations.DefinedIn);
        }

        private void Visit(string containerKey, SeparatedSyntaxList<ParameterSyntax> parameters)
        {
            foreach (var para in parameters)
            {
                var key = containerKey + "." + para.Identifier.Text;
                Register(EnumKInd.Parameter, key, para.Identifier.Text);
                Relate(containerKey, key, EnumRelations.DefinedIn);
                //System.Console.WriteLine(string.Format("\t\tpara: {0} {1} {2}", para.Modifiers.ToString(), para.Type.ToString(), para.Identifier.Text));
            }
        }
    }
}


 
