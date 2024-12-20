﻿
using System.Reflection.Metadata;
using System.CommandLine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;

void printExeption(string exeption)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(exeption);
    Console.ResetColor();
    Environment.Exit(1);
}

var rootCommand = new RootCommand("Root command for File Bundle CLI");

//Bundle command
var bundleCommand = new Command("bundle", "Bundle code files to a single file");

//Response file command
var responseFileCommand = new Command("create-rsp", "Create adapted response file");


//Output option
var bundleOutput = new Option<FileInfo>("--output", "File path and name");
bundleOutput.AddAlias("-o");
bundleCommand.AddOption(bundleOutput);

//Language option
var languages = new Dictionary<string, List<string>>
{
    ["Python"] = new List<string> { ".py" },
    ["Java"] = new List<string> { ".java", ".class", ".jar" },
    ["C++"] = new List<string> { ".cpp", ".h", ".hpp" },
    ["JavaScript"] = new List<string> { ".js" },
    ["C#"] = new List<string> { ".cs" },
    ["Ruby"] = new List<string> { ".rb" },
    ["Swift"] = new List<string> { ".swift" },
    ["HTML"] = new List<string> { ".html", ".htm" },
    ["CSS"] = new List<string> { ".css" },
    ["PHP"] = new List<string> { ".php" },
    ["Go"] = new List<string> { ".go" },
    ["TypeScript"] = new List<string> { ".ts" },
    ["Rust"] = new List<string> { ".rs" },
    ["Kotlin"] = new List<string> { ".kt" },
    ["Bash"] = new List<string> { ".sh" },
    ["SQL"] = new List<string> { ".sql" },
    ["ObjectiveC"] = new List<string> { ".m" },
    ["Scala"] = new List<string> { ".scala" },
    ["Groovy"] = new List<string> { ".groovy" },
    ["Perl"] = new List<string> { ".pl" }
};
List<string> languageFiles = new List<string>();
var bundleLanguageOption = new Option<string>("--language", "The languages in the bundle file");
bundleLanguageOption.AddAlias("-l");
bundleCommand.AddOption(bundleLanguageOption);


//Note option
var bundleNoteOption = new Option<bool>("--note", "File location for any file in the bundle file");
bundleNoteOption.AddAlias("-n");
bundleCommand.AddOption(bundleNoteOption);

//Sort option
var bundleSortOption = new Option<string>("--sort", "Sort the files acording to their names");
bundleSortOption.SetDefaultValue("AB");
bundleSortOption.AddAlias("-s");
bundleCommand.AddOption(bundleSortOption);

//Remove-empty-lines option
var bundleRemoveEmptyLinesOption = new Option<bool>("--remove-empty-lines", "Remove empty lines in the files");
bundleRemoveEmptyLinesOption.AddAlias("-r");
bundleCommand.AddOption(bundleRemoveEmptyLinesOption);

//author option
var authorOption = new Option<bool>("--author", "Recording the file creator's name");
authorOption.AddAlias("-a");
bundleCommand.AddOption(authorOption);

// languages option function
void createLaguagesList(string language)
{
    // all languages
    if (language == "all")
    {
        foreach (var l in languages)
        {
            foreach (var file in l.Value)
            {
                languageFiles.Add(file);
            }
        }
    }
    else
    {
        string[] choosedLanguage = language.Split(" ");
        for (int i = 0; i < choosedLanguage.Length; i++)
        {
            try
            {
                List<string> files = languages[choosedLanguage[i]];
                foreach (var file in files)
                {
                    languageFiles.Add(file);
                }
            }
            catch (KeyNotFoundException ex)
            {
                printExeption("Error: \"" + choosedLanguage[i] + "\" not exist in the language option \nYou can choose only: Python, Java, C++, JavaScript, C#, Ruby, Swift, HTML, CSS, Go, TypeScript, PHP, Rust, Kotlin, Bash, SQL, ObjectiveC, Scala, Groovy, Perl");
            }
        }
    }
    // without duplicate
    HashSet<string> uniqueSet = new HashSet<string>(languageFiles);
    languageFiles = uniqueSet.ToList();
}

List<string> allFiles = new List<string>();
void getAllFiles(string directory)
{
    if (directory.Contains("bin") || directory.Contains("Debug") || directory.Contains("Lib") ) 
        return;
    string[] files = Directory.GetFiles(directory);
    for (int i = 0; i < files.Length; i++)
    {
        allFiles.Add(files[i]);
    }
    
    string[] subDirectories = Directory.GetDirectories(directory);
    foreach (var subDirectory in subDirectories)
    {
        getAllFiles(subDirectory);
    }
}

bundleCommand.SetHandler((output, language ,note, sort, remove, author) => {

    // languages option(the information->languageFiles)
    try
    {
        createLaguagesList(language);
    }
    catch (NullReferenceException ex)
    {
        printExeption("Error: Language option must be given");
    }

    // all the files
    string currentDirectory = Directory.GetCurrentDirectory();
    getAllFiles(currentDirectory);

    // only files from choosed languages
    List<string> bundleFiles = allFiles
            .Where(file => languageFiles.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            .ToList();


    // sort option
    if(sort == "AB")
    {
        List<string> sortedFilePaths = bundleFiles
            .OrderBy(filePath => Path.GetFileName(filePath))
            .ToList();
        bundleFiles = sortedFilePaths;
    }
    else if(sort == "code")
    {
        List<string> bundleFiles2 = new List<string>();
        foreach (var l in languageFiles)
            foreach (var f in bundleFiles)
                if (f.EndsWith(l))
                    bundleFiles2.Add(f);
        bundleFiles = bundleFiles2;
    }
    else
    {
        printExeption("Error: can't sort by \"" + sort + "\"");
    }

    
    // remove option
    if (remove)
    {
        foreach(var filePath in bundleFiles)
        {
            string[] lines = File.ReadAllLines(filePath);
            string[] nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            File.WriteAllLines(filePath, nonEmptyLines);
        }   
    }

    // author option
    string authorName = string.Empty;
    if (author)
    {
        Console.WriteLine("Enter the author name:");
        authorName = Console.ReadLine();
    }

    //  write to bundle file
    // output option
    try
    {
        if (File.Exists(output.FullName))
        {
            try
            {
                string fileContent = File.ReadAllText(output.FullName);
                bool full = !string.IsNullOrWhiteSpace(fileContent);
                if (full)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(output.FullName, FileMode.Truncate))
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        printExeption("Error deleting file content");
                    }
                }

            }
            catch (Exception ex)
            {
                printExeption($"Error reading the file");
            }
            
        }
        using (StreamWriter writer = new StreamWriter(output.FullName, true))
        {
            if (!bundleFiles.Any())
                printExeption("There are no files to write");
            // Write the author name
            if (!string.IsNullOrEmpty(authorName))
                writer.WriteLine("Created by: " + authorName + Environment.NewLine);
            // Write all requiered files
            foreach (string filePath in bundleFiles)
            {
                if (note)
                {
                    string relativePath = Path.GetRelativePath(currentDirectory, filePath);
                    writer.WriteLine("This file is located at: \n" + relativePath + Environment.NewLine);
                }

                // Read the content of the file
                string[] fileLines = File.ReadAllLines(filePath);

                // Write the file content to the output file
                foreach (string line in fileLines)
                    writer.WriteLine(line);

                // Add a separator between each file
                writer.WriteLine(new string('-', 30));
            }
        }
        Console.WriteLine("Files written to: " + output.FullName);
    }
    catch (NullReferenceException ex)
    {
        printExeption("Error: Missing path or file name");
    }
    catch (DirectoryNotFoundException ex)
    {
        printExeption("Error: Invalid file path");
    }
    catch (IOException ex)
    {
        printExeption("Error: Some problem with this file name. Please choose anothor name");
    }

    
}, bundleOutput, bundleLanguageOption, bundleNoteOption, bundleSortOption, bundleRemoveEmptyLinesOption, authorOption);



responseFileCommand.SetHandler(() =>
{
    // output, language ,note, sort, remove, author
    Console.WriteLine("Hello we are here to create a response file!");
    // output
    Console.WriteLine("1) Please enter a name to the bundle file or path with name\nfor example you can enter \"bundleFile\" or \"E:\\bundleFile\"");
    string output = Console.ReadLine();
    Console.Clear();
    // language
    Console.WriteLine("2) Please enter the programming languages to include in the bundle file\nThe language option:\n Python, Java, C++, JavaScript, C#, Ruby, Swift, HTML, CSS, Go, TypeScript, PHP, Rust, Kotlin, Bash, SQL, ObjectiveC, Scala, Groovy, Perl");
    string language = Console.ReadLine();
    Console.Clear();
    // note
    Console.WriteLine("3) Do you want to write the relative path of any file in the bundle?\nIf you want enter \"y\" if not enter any key");
    var note = Console.ReadKey();
    bool noteFlag = false;
    if (note.Equals("y") || note.Equals("Y"))
        noteFlag = true;
    Console.Clear();
    // sort
    Console.WriteLine("4) We will sort the bundle files in ABC order acording to their name \nIf you want to sort acording to code enter \"c\" if not enter any key");
    string sort = "AB";
    var sortKey = Console.ReadKey();
    if (sortKey.Equals("C") || sortKey.Equals("c"))
        sort = "code";
    Console.Clear();
    // remove empty lines
    Console.WriteLine("Remove empty lines from any file in the bundle\nIf you want enter \"y\" if not enter any key");
    var remove = Console.ReadKey();
    bool removeFlag = false;
    if (remove.Equals("Y") || remove.Equals("y"))
        noteFlag = true;
    Console.Clear();
    // author
    Console.WriteLine("Do you want to write in the head of the bundle file who create the file \nIf you want enter \"y\" if not enter any key");
    var author = Console.ReadKey();
    bool authorFlag = false;
    if (author.Equals("Y") || author.Equals("y"))   
        authorFlag = true;
    Console.Clear();

    Console.WriteLine("Preper response file...");
    // write the response file
    // output, language ,note, sort, remove, author
    string filePath = "responseFile";
    using (StreamWriter writer = new StreamWriter(filePath))
    {     
        writer.WriteLine("bundle");
        // output
        writer.WriteLine("--output " + output);
        // language
        writer.WriteLine("--language " + language);
        // note
        writer.WriteLine("--note " + noteFlag);
        //sort
        writer.WriteLine("--sort "+ sort);
        // remove
        writer.WriteLine("--remove-empty-lines " + removeFlag);
        // author
        if(authorFlag == true)
        writer.WriteLine("--author");
    }
    Console.Clear();
    Console.WriteLine("We create for you a response file named \"responseFile\" in the current directory");
    Console.WriteLine("To run the command you need to write \"fib @responseFile\" ");


});


rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(responseFileCommand);

rootCommand.InvokeAsync(args);

//publish

//fib bundle --language C#

//fib bundle --language C# --output bundle

//fib bundle --language C# -o bundle
// 
//fib bundle --language C# -o bundle -a 

//myriam elkouby

// output, language ,note, sort, remove, author


// fib create-rsp