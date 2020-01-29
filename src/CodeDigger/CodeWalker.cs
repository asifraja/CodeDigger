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
        protected CodeWalker() { }

        public CodeWalker(string filePath, string fielName, IDictionary<string, Node> nodes, IDictionary<string, EdgeNode> edges)
        {
            FilePath = filePath;
            FileName = fielName;
            CodeVisitor = new CodeVisitor(FilePath, FileName, nodes, edges);
        }

        public string FilePath { get; }
        public string FileName { get; }
        public string SolutionName { get; }
        public CodeVisitor CodeVisitor { get; }

        public readonly List<InterfaceDeclarationSyntax> Interfaces = new List<InterfaceDeclarationSyntax>();
        public readonly List<TypeParameterSyntax> TypeParameters = new List<TypeParameterSyntax>();
        public readonly List<ConstructorDeclarationSyntax> Constructors = new List<ConstructorDeclarationSyntax>();
        public readonly List<ConstantPatternSyntax> ConstantPatterns = new List<ConstantPatternSyntax>();
        public readonly List<PropertyDeclarationSyntax> Peroperties = new List<PropertyDeclarationSyntax>();
        public readonly List<UsingDirectiveSyntax> Usings = new List<UsingDirectiveSyntax>();
        public readonly List<ClassDeclarationSyntax> Classes = new List<ClassDeclarationSyntax>();
        public readonly List<NamespaceDeclarationSyntax> Namespances = new List<NamespaceDeclarationSyntax>();
        public readonly List<MethodSignature> Methods = new List<MethodSignature>();

        //public override void VisitTypeParameter(TypeParameterSyntax node)
        //{
        //    TypeParameters.Add(node);
        //    base.VisitTypeParameter(node);
        //}

        //public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        //{
        //    Constructors.Add(node);
        //    base.VisitConstructorDeclaration(node);
        //}

        //public override void VisitUsingDirective(UsingDirectiveSyntax node)
        //{
        //    Usings.Add(node);
        //    base.VisitUsingDirective(node);
        //}

        //public override void VisitConstantPattern(ConstantPatternSyntax node)
        //{
        //    ConstantPatterns.Add(node);
        //    base.VisitConstantPattern(node);
        //}

        //public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        //{
        //    Peroperties.Add(node);
        //    base.VisitPropertyDeclaration(node);
        //}

        //public override void VisitThisExpression(ThisExpressionSyntax nodeSyntax)
        //{
        //    CodeVisitor.Visit(nodeSyntax);
        //    base.VisitThisExpression(nodeSyntax);
        //}

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax nodeSyntax)
        {
            CodeVisitor.Visit(nodeSyntax);
            base.VisitInterfaceDeclaration(nodeSyntax);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax nodeSyntax)
        {
            CodeVisitor.Visit(nodeSyntax);
            base.VisitClassDeclaration(nodeSyntax);
        }

        public override void VisitClassOrStructConstraint(ClassOrStructConstraintSyntax nodeSyntax)
        {
            CodeVisitor.Visit(nodeSyntax);
            base.VisitClassOrStructConstraint(nodeSyntax);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax nodeSyntax)
        {
            CodeVisitor.Visit(nodeSyntax);
            base.VisitStructDeclaration(nodeSyntax);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax nodeSyntax)
        {
            {
                CodeVisitor.Visit(nodeSyntax);
                base.VisitEnumDeclaration(nodeSyntax);
            }

            //public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            //{
            //    this.Namespances.Add(node);
            //    base.VisitNamespaceDeclaration(node);
            //}

            //public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            //{
            //    var paraList = new List<TypeSignature>();
            //    var classNode = node.Parent as ClassDeclarationSyntax;
            //    var interfaceNode = node.Parent as InterfaceDeclarationSyntax;

            //    foreach (var para in node.ParameterList.Parameters)
            //    {
            //        paraList.Add(new TypeSignature(TypeSignatureEnum.MethodParameter, para.Modifiers.ToString(), para.Type.ToString(), para.Identifier.Text));
            //    }
            //    var methodCall = new MethodSignature(node.Modifiers.ToString(), node.ReturnType.ToString(), node.Identifier.Text, paraList);

            //    //var paras = node.ParameterList.Parameters.ToString(); // node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            //    this.Methods.Add(methodCall);

            //    base.VisitMethodDeclaration(node);
            //}
        }
    }
}


 
