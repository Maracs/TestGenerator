using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGenerator.Core;

namespace ConsoleApp;

public class Pipeline
{

   
    public int MaxReadingTasks { get; }

    public int MaxProcessingTasks { get; }

    public int MaxWritingTasks { get; }
    
    private readonly string writingPath;
    
    private readonly string inputPath;

    public Pipeline(int maxReadingTasks, int maxProcessingTasks, int maxWritingTasks,string inputPath, string writingPath)
    {
        this.writingPath = writingPath;
        this.inputPath = inputPath;
        MaxReadingTasks = maxReadingTasks;
        MaxProcessingTasks = maxProcessingTasks;
        MaxWritingTasks = maxWritingTasks;
    }

    public async Task PerformProcessing()
    {

        var readDirectoryBlock = new TransformManyBlock<string, string>(
            async path=> await ReadDirectory(path),new ExecutionDataflowBlockOptions{}
            );
        
        var linkOptions = new DataflowLinkOptions {PropagateCompletion = true};
        
        var readingBlock = new TransformBlock<string, string>(
            async path => await ReadFile(path),
            new ExecutionDataflowBlockOptions{MaxDegreeOfParallelism = MaxReadingTasks});
        
        var processingBlock = new TransformBlock<string, List<string>>(
            content => ProcessFile(content),
            new ExecutionDataflowBlockOptions{MaxDegreeOfParallelism = MaxProcessingTasks});
        
        var writingBlock = new ActionBlock<List<string>>(async fwc => await WriteFile(fwc),
            new ExecutionDataflowBlockOptions{MaxDegreeOfParallelism = MaxWritingTasks});

        readDirectoryBlock.LinkTo(readingBlock, linkOptions);
        readingBlock.LinkTo(processingBlock, linkOptions);
        processingBlock.LinkTo(writingBlock, linkOptions);

        readDirectoryBlock.Post(inputPath);
        
        readDirectoryBlock.Complete();

        await writingBlock.Completion;
    }

    private async Task<string> ReadFile(string filePath)
    {

        string result;
        using (var streamReader = new StreamReader(filePath))
        {
            result = await streamReader.ReadToEndAsync();
        }
        
        return result;
    }
    
    private async Task<string[]> ReadDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new ArgumentException("Directory doesn't exist");
        }

        return Directory.EnumerateFiles(path).ToArray();
    }

    private List<string> ProcessFile(string code)
    {
        var testGenerator = new NUnitTestgenerator();
        
        var tests =testGenerator.MakeTests(code) ;
        
        return tests;
    }

    private async Task WriteFile(List<string> list)
    {
        
        foreach (var str in list)
        {

            var fileName =  CSharpSyntaxTree.ParseText(str).GetRoot()
                .DescendantNodes().OfType<ClassDeclarationSyntax>().First().Identifier.Text;
            
            using (var streamWriter = new StreamWriter(writingPath+"\\"+fileName+".cs"))
            {
                await streamWriter.WriteAsync(str);
            }
        }
    }
    
}