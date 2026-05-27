using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WordChecker;

public class WordsSearcher
{

    public FilesAndInfo GetFilePathsFromDirectory(string directoryPath, ILogger logger)
    {
        var foundFiles = new ConcurrentBag<string>();
        var filePatterns = new[] { "*.txt", "*.md" };

        var files = new List<string>();
        try
        {
            var enumOptions = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            files = filePatterns.SelectMany(pattern =>
                Directory.EnumerateFiles(directoryPath, pattern, enumOptions)).ToList();
            foreach (var file in files)
            {
                logger.Information($"Found file {file}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while enumerating files in '{directoryPath}': {ex.Message}");
        }

        var returnInfo = new FilesAndInfo
        {
            TotalFiles = files.Count,
            Files = files
        };
        return returnInfo;

    }

    public string CopyFile(string sourceFilePath, string destinationDirectory, ILogger logger)
    {
        var destFileName = string.Empty;
        try
        {
            destFileName = Path.Combine(destinationDirectory, Path.GetFileName(sourceFilePath));
            var number = 1;
            var newFileName = string.Empty;
            var oldFileNameNoExtension = $"{Path.GetFileNameWithoutExtension(destFileName)}";
            while (File.Exists(destFileName))
            {
                newFileName = $"{oldFileNameNoExtension}({number}){Path.GetExtension(destFileName)}";
                destFileName = Path.Combine(destinationDirectory, newFileName);
                number++;
            }
            File.Copy(sourceFilePath, destFileName, true);
            logger.Information($"Copied file: {Path.GetFileName(sourceFilePath)}");
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while copying file '{sourceFilePath}': {ex.Message}");
        }

        return destFileName;
    }

    public void CreateModifiedFile(string sourceFilePath, string destinationDirectory, string modifiedContent, ILogger logger)
    {
        var destFileName = string.Empty;
        try
        {
            destFileName = Path.Combine(destinationDirectory, Path.GetFileName(sourceFilePath));
            var number = 1;
            var newFileName = string.Empty;
            var oldFileNameNoExtension = $"{Path.GetFileNameWithoutExtension(destFileName)}";
            while (File.Exists(destFileName))
            {
                newFileName = $"{oldFileNameNoExtension}({number}){Path.GetExtension(destFileName)}";
                destFileName = Path.Combine(destinationDirectory, newFileName);
                number++;
            }
            File.WriteAllText(destFileName, modifiedContent);
            logger.Information($"Created modified file: {newFileName}");
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while creating modified file for '{sourceFilePath}': {ex.Message}");
        }
    }
    public List<string> SearchWordsInDirectory(string directoryPath,
        HashSet<string> forbiddenWords, string outputDirectory, ILogger logger)
    {
        var info = GetFilePathsFromDirectory(directoryPath, logger);
        var foundFiles = new ConcurrentBag<string>();
        var files = info.Files;

        // Create a single regex for all forbidden words for efficiency.
        // \b ensures we match whole words only.
        logger.Information($"Searching for forbidden words in directory '{directoryPath}'...");
        var forbiddenWordsPattern = string.Join("|", forbiddenWords.Select(Regex.Escape));
        var wordRegex = new Regex($@"\b({forbiddenWordsPattern})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        Parallel.ForEach(files, file =>
        {
            try
            {
                var content = File.ReadAllText(file);
                var matches = wordRegex.Matches(content);

                if (matches.Count > 0)
                {
                    foundFiles.Add(file);
                    
                    // Copy original
                    var destFileName = CopyFile(file, outputDirectory, logger);

                    // Create modified version
                    var modifiedContent = wordRegex.Replace(content, "*******");
                    CreateModifiedFile(file, outputDirectory, modifiedContent, logger);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while processing file '{file}': {ex.Message}");
            }
        });

        logger.Information("Searching finished!");
        logger.Information($"Finished searching in directory '{directoryPath}'");
        return foundFiles.ToList();
    }
}
