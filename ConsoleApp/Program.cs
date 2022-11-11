using ConsoleApp;
using TestGenerator.Core;


        string fileListPath = null;
        string testFilesPath = null;
        
        int maxReadingTasks;
        int maxProcessingTasks;
        int maxWritingTasks; 
        
        bool success = false;
        
        while (!success)
        {
            Console.WriteLine("Введите путь к файлу со списком исходных файлов для генерации тестов:");
            fileListPath = Console.ReadLine();
            success = Directory.Exists(fileListPath);
            if(!success) Console.WriteLine("Директории не существует. Повторите попытку!"); 
        }
        
        success = false;
        while (!success)
        {
            Console.WriteLine("Введите путь к директории, где будут храниться тестовые файлы:");
            testFilesPath = Console.ReadLine();
            success = Directory.Exists(testFilesPath);
            if(!success) Console.WriteLine("Директории не существует. Повторите попытку!"); 
        }

        Console.WriteLine("Введите максимальное количество файлов, загружаемых за раз:"); 
        maxReadingTasks = Convert.ToInt32(Console.ReadLine());
        
        Console.WriteLine("Введите максимальное количество одновременно обрабатываемых задач:"); 
        maxProcessingTasks = Convert.ToInt32(Console.ReadLine());
        
        Console.WriteLine("Введите максимальное количество одновременно записываемых файлов:"); 
        maxWritingTasks = Convert.ToInt32(Console.ReadLine());
        
        var pipeline = 
            new Pipeline(maxReadingTasks,maxProcessingTasks,maxWritingTasks,fileListPath, testFilesPath);
        await pipeline.PerformProcessing();




