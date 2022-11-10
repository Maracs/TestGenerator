using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGenerator.Core;

public class FileWalker:CSharpSyntaxWalker
{
    private List<UsingDirectiveSyntax> _classDirectives = new();
    
    private List<string> _classFiles = new();

    private string _namespace = "";

    public List<string> GetClassesFromFile(string code)
    {

        var tree = CSharpSyntaxTree.ParseText(code);

        var node = tree.GetRoot();
        
        Visit(node);
        
        return _classFiles;
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        if (_namespace.Equals(""))
        {
            _namespace = node.Name.NormalizeWhitespace().ToFullString();
        }
        else
        {
            _namespace +="."+ node.Name.NormalizeWhitespace().ToFullString();
        }
        
        base.VisitNamespaceDeclaration(node);
        
        if (_namespace.Equals(""))
        {
            _namespace.Replace( node.Name.NormalizeWhitespace().ToFullString(),"");
        }
        else
        {
            _namespace.Replace( "."+ node.Name.NormalizeWhitespace().ToFullString(),"");
        }
        
    }


    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        if (_namespace.Equals(""))
        {
            _namespace = node.Name.NormalizeWhitespace().ToFullString();
        }
        else
        {
            _namespace +="."+ node.Name.NormalizeWhitespace().ToFullString();
        }
        
        base.VisitFileScopedNamespaceDeclaration(node);
        
        if (_namespace.Equals(""))
        {
            _namespace.Replace( node.Name.NormalizeWhitespace().ToFullString(),"");
        }
        else
        {
            _namespace.Replace( "."+ node.Name.NormalizeWhitespace().ToFullString(),"");
        }
    }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        _classDirectives.Add(node);
        base.VisitUsingDirective(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        
         if (_namespace.Equals(""))
         {
             var newNode = SyntaxFactory.CompilationUnit()
                 .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(node));
             
             newNode = _classDirectives.Aggregate(newNode,
                 (current, loadDirective) => current.AddUsings(loadDirective));
             
             _classFiles.Add(
                 newNode.NormalizeWhitespace().ToFullString()
             );
         }
         else
         {
             var newNode =SyntaxFactory.CompilationUnit()
                 .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                     SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(_namespace))
                         .WithMembers(new SyntaxList<MemberDeclarationSyntax>(node))));
             
             newNode = _classDirectives.Aggregate(newNode,
                 (current, loadDirective) => current.AddUsings(loadDirective));
             
             _classFiles.Add(
                    newNode.NormalizeWhitespace().ToFullString()
             );
         }

         base.VisitClassDeclaration(node);
    }
}