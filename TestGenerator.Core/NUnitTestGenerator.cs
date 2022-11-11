namespace TestGenerator.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

 public class NUnitTestgenerator
{
    
    public List<string> MakeTests(string code)
    {

        var fileWalker= new FileWalker();
            
        var classList = fileWalker.GetClassesFromFile(code);
            
        var classRewriter = new ClassRewriter();


        var tests = new List<string>();
            
        foreach (var c in classList)
        {
            tests.Add(classRewriter.MakeTestFromClass(c));
        }
        
        return tests;
    }

   
    
}

       

        

   
        
        
       

    
    
