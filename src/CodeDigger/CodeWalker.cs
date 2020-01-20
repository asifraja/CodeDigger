using System;
using System.Collections.Generic;
using System.Reflection;
using CodeDigger.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace UsingCollectorCS
{
    class CodeWalker : CSharpSyntaxWalker
    {
        public readonly List<UsingDirectiveSyntax> Usings = new List<UsingDirectiveSyntax>();
        public readonly List<ClassDeclarationSyntax> Classes = new List<ClassDeclarationSyntax>();
        public readonly List<NamespaceDeclarationSyntax> Namespances = new List<NamespaceDeclarationSyntax>();
        public readonly List<MethodSignature> Methods = new List<MethodSignature>();

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Name.ToString().Contains("easyjet"))
            {
                this.Usings.Add(node);
            }
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            this.Classes.Add(node);
            base.VisitClassDeclaration(node);
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            this.Namespances.Add(node);
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var paraList = new List<TypeSignature>();
            var classNode = node.Parent;
            var namespaceNode = classNode.Parent;

            foreach (var para in node.ParameterList.Parameters)
            {
                paraList.Add(TypeSignature.Create(para.Modifiers.ToString(), para.Type.ToString(), para.Identifier.Text));
            }
            var methodCall = new MethodSignature(TypeSignature.Create(node.Modifiers.ToFullString(), node.ReturnType.ToString(), node.Identifier.Text), paraList);

            //var paras = node.ParameterList.Parameters.ToString(); // node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            this.Methods.Add(methodCall);

            base.VisitMethodDeclaration(node);
        }
    }
}


 
