using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WordChecker;

public class WordsSearcher
{
    public List<string> SearchWordsInDirectory(string directoryPath,
        HashSet<string> forbiddenWords, string outputDirectory)
    {
        var foundFiles = new List<string>();
        var filePatterns = new[] { "*.txt", "*.md" };
        var files = filePatterns.SelectMany(pattern =>
            Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories));

        // Create a single regex for all forbidden words for efficiency.
        // \b ensures we match whole words only.
        Console.WriteLine($"Searching for forbidden words in directory '{directoryPath}'...");
        var forbiddenWordsPattern = string.Join("|", forbiddenWords.Select(Regex.Escape));
        var wordRegex = new Regex($@"\b({forbiddenWordsPattern})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        Console.WriteLine("Searching finished!");
        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file);
                if (wordRegex.IsMatch(content))
                {
                    foundFiles.Add(file);

                    // Copy original file
                    var destFileName = Path.Combine(outputDirectory, Path.GetFileName(file));
                    File.Copy(file, destFileName, true);
                    Console.WriteLine($"Copied file: {Path.GetFileName(file)}");

                    // Create modified file with replaced words
                    var modifiedContent = wordRegex.Replace(content, "*******");
                    var newFileName = Path.GetFileNameWithoutExtension(file) + "_modified" + Path.GetExtension(file);
                    var modifiedFilePath = Path.Combine(outputDirectory, newFileName);
                    File.WriteAllText(modifiedFilePath, modifiedContent);
                    Console.WriteLine($"Created modified file: {newFileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing file '{file}': {ex.Message}");
            }
        }

        Console.WriteLine($"Finished searching in directory '{directoryPath}'");
        return foundFiles;
    }
}
