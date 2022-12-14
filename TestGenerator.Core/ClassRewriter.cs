using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGenerator.Core;

public class ClassRewriter:CSharpSyntaxRewriter
{

    private  ConcurrentBag<string> _uniqueMethods = new();

    private string _currentNamespace="";

    public string MakeTestFromClass(string code)
    {
        _uniqueMethods = new();
        
        var tree = CSharpSyntaxTree.ParseText(code);

        var node = tree.GetRoot();
        
        var testCode = Visit(node).NormalizeWhitespace().ToFullString();
        
        return testCode;
        
    }

    
   

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.Modifiers.First().Text.Equals("public"))
        {
            var assert = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("Assert"),
                        SyntaxFactory.IdentifierName("Fail")))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal("autogenerated")))))));


            var methodBody = SyntaxFactory.Block(assert);

            var methodDeclarationWithNewBody = node.WithBody(methodBody).NormalizeWhitespace();

            var returnType = SyntaxFactory.ParseTypeName("void");



            var methodDeclarationWithNewReturnType =
                methodDeclarationWithNewBody.WithReturnType(returnType).NormalizeWhitespace();

            var emptyBlock = SyntaxFactory.ParameterList();

            var methodDeclarationWithoutParameters = methodDeclarationWithNewReturnType.WithParameterList(emptyBlock)
                .NormalizeWhitespace();

            var identifierText = node.Identifier.Text;

            var className = ((ClassDeclarationSyntax)node.Parent).Identifier.Text;

            int index = 0;

            while (_uniqueMethods.Contains(identifierText + (index == 0 ? "" : index)))
            {
                index++;
            }

            _uniqueMethods.Add(identifierText + (index == 0 ? "" : index));

            var newIdentifier = SyntaxFactory.Identifier(identifierText + (index == 0 ? "" : index) + "Test");

            var methodDeclarationWithNewName =
                methodDeclarationWithoutParameters.WithIdentifier(newIdentifier).NormalizeWhitespace();

            var attributeName = SyntaxFactory.ParseName("Test");

            var attribute = SyntaxFactory.Attribute(attributeName);

            var separatedSyntaxList = new SeparatedSyntaxList<AttributeSyntax>();

            separatedSyntaxList = separatedSyntaxList.Add(attribute);

            var attributeList = SyntaxFactory.AttributeList(separatedSyntaxList);

            var syntaxList = new SyntaxList<AttributeListSyntax>();

            syntaxList = syntaxList.Add(attributeList);

            var methodDeclarationWithTestAttribute =
                methodDeclarationWithNewName.WithAttributeLists(syntaxList).NormalizeWhitespace();

            return base.VisitMethodDeclaration(methodDeclarationWithTestAttribute);
        }
        else
        {
            return null;
        }

    }
    
    
    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        _currentNamespace = node.Name.ToFullString();
        
        var newNamespaceName = SyntaxFactory.ParseName(node.Name.ToFullString()+".Test");
        
        var namespaceDeclaration = node.WithName(newNamespaceName);

        return  base.VisitNamespaceDeclaration(namespaceDeclaration);
        
    }

    
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        
        var identifierText = node.Identifier.Text;
        
        var newIdentifier = SyntaxFactory.Identifier(identifierText+"Tests");
        
        var classDeclarationWithNewName = node.WithIdentifier(newIdentifier).NormalizeWhitespace();

        var attributeName = SyntaxFactory.ParseName("TestFixture");
        
        var attribute = SyntaxFactory.Attribute(attributeName);
        
        var separatedSyntaxList = new SeparatedSyntaxList<AttributeSyntax>();
        
        separatedSyntaxList = separatedSyntaxList.Add(attribute);
        
        var attributeList = SyntaxFactory.AttributeList(separatedSyntaxList);
        
        var syntaxList = new SyntaxList<AttributeListSyntax>();

        syntaxList = syntaxList.Add(attributeList);
        
        var classDeclarationWithTestAttribute = classDeclarationWithNewName.WithAttributeLists(syntaxList).NormalizeWhitespace();
        
        return base.VisitClassDeclaration(classDeclarationWithTestAttribute);
    }

    
    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        return null;
    }

    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {

      node = (CompilationUnitSyntax?) base.VisitCompilationUnit(node);
        
    var defaultLoadDirectiveList = new List<UsingDirectiveSyntax>()
    {
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("NUnit.Framework")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(_currentNamespace))
    };
        
        node = defaultLoadDirectiveList.Aggregate(node,
            (current, loadDirective) => current.AddUsings(loadDirective));
        
        return node;
    }
}