using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynCSharp.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RoslynCSharp.Example
{
#pragma warning disable 0219

    public class Ex07a_CompileFromSyntaxTree : MonoBehaviour
    {
        private ScriptDomain domain = null;

        public void Start()
        {
            domain = ScriptDomain.CreateDomain("Example Domain");

            //CSharpSyntaxTree syntaxTree = CSharpSyntaxTree.Create();
            
            CompilationUnitSyntax syntax = SyntaxFactory.CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("UnityEngine")))
            .AddMembers(
                SyntaxFactory.ClassDeclaration("Example")
                .AddMembers(                    
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "SayHello")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .AddBodyStatements(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("Debug"), 
                                SyntaxFactory.IdentifierName("Log"))
                        .WithOperatorToken(SyntaxFactory.Token(SyntaxKind.DotToken))
                        ).WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, 
                                            SyntaxFactory.Literal("Hello World"))))))))));

            // Get syntax tree
            CSharpSyntaxTree syntaxTree = CSharpSyntaxTree.Create(syntax) as CSharpSyntaxTree;

            // Compile the syntax tree with references
            ScriptType type = domain.CompileAndLoadMainSyntaxTree(syntaxTree, ScriptSecurityMode.UseSettings, new Compiler.IMetadataReferenceProvider[] { 
                    AssemblyReference.FromAssembly(typeof(object).Assembly), 
                    AssemblyReference.FromAssembly(typeof(UnityEngine.Object).Assembly) });

            // Invoke the say hello method
            type.CallStatic("SayHello");
        }
    }
}
