using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WordChecker;

public class SearchResult
{
    public List<string> FoundFiles { get; set; } = new();
    public Dictionary<string, int> WordCounts { get; set; } = new();
}

public class WordsSearcher(ILogger logger, CancellationTokenSource cancellationTokenSource)
{
    object lockObj = new object();
    volatile bool isPaused = false;
    bool isCancelled = false;
    ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(true);
    private SearchResult? lastSearchResult;

    public SearchResult? GetLastSearchResult() => lastSearchResult;

    public void Pause()
    {
        if (!isPaused)
        {
            manualResetEvent.Reset();
            isPaused = true;
        }
        
    }

    public void Stop()
    {
        cancellationTokenSource.Cancel();
        manualResetEvent.Set();
    }

    public void Resume()
    {
        if (isPaused)
        {
            manualResetEvent.Set();
            isPaused = false;
        }
    }

    public IEnumerable<string> GetFilePathsFromDirectory(string directoryPath)
    {
        var filePatterns = new[] { "*.txt", "*.md" };

        var files = Enumerable.Empty<string>();
        try
        {
            var enumOptions = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };
            Console.WriteLine("Looking for files...");
            files = filePatterns.SelectMany(pattern =>
                Directory.EnumerateFiles(directoryPath, pattern, enumOptions));
            //foreach (var file in files)
            //{
            //    logger.Information($"Found file {file}");
            //}
            
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while enumerating files in '{directoryPath}': {ex.Message}");
        }

        return files;

    }


    public string CopyFile(string sourceFilePath, string destinationDirectory, int amountOfMatches)
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
            var fileInfo = new FileInfo(destFileName);
            logger.Information($"Copied file: {Path.GetFileName(sourceFilePath)} to {Path.GetFullPath(destFileName)}" +
                $" \t Size: {fileInfo.Length} bytes \t Matches: {amountOfMatches}");
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while copying file '{sourceFilePath}': {ex.Message}");
        }

        return destFileName;
    }


    public void CreateModifiedFile(string sourceFilePath, string destinationDirectory, int amountOfMatches, string modifiedContent)
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
            logger.Information($"Created modified file: {Path.GetFileName(destFileName)}");
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while creating modified file for '{sourceFilePath}': {ex.Message}");
        }
    }


    public List<string> SearchWordsInDirectory(string directoryPath,
        HashSet<string> forbiddenWords, string outputDirectory)
    {
        var sw = Stopwatch.StartNew();
        var foundFiles = new ConcurrentBag<string>();
        var files = GetFilePathsFromDirectory(directoryPath);

        logger.Information($"Searching for forbidden words in directory '{directoryPath}'...");
        var forbiddenWordsPattern = string.Join("|", forbiddenWords.Select(Regex.Escape));
        var wordRegex = new Regex($@"\b({forbiddenWordsPattern})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        var wordCountDict = new Dictionary<string, int>();

        var parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = cancellationTokenSource.Token
        };
        try
        {
            Parallel.ForEach(files, parallelOptions, (file, state) =>
            {
                try
                {
                    manualResetEvent.Wait(cancellationTokenSource.Token);

                    var content = File.ReadAllText(file);
                    var matches = wordRegex.Matches(content);

                    if (matches.Count > 0)
                    {
                        lock (lockObj)
                        {
                            foreach (Match match in matches)
                            {
                                var word = match.Value.ToLower();
                                if (wordCountDict.ContainsKey(word))
                                    wordCountDict[word]++;
                                else
                                    wordCountDict[word] = 1;
                            }
                        }
                        foundFiles.Add(file);

                        var destFileName = CopyFile(file, outputDirectory, matches.Count);
                        var modifiedContent = wordRegex.Replace(content, "*******");
                        CreateModifiedFile(file, outputDirectory, matches.Count, modifiedContent);
                    }
                }
                catch (OperationCanceledException)
                {
                    state.Break();
                }
                catch (Exception ex)
                {
                    logger.Error($"An error occurred while processing file '{file}': {ex.Message}");
                }
            });
        }
        catch (OperationCanceledException)
        {
            logger.Information("Search cancelled by user.");
        }

        Task.Delay(500).Wait();

        if (isPaused)
        {
            logger.Information("Search is currently paused. Please resume to see results.");
        }
        if (isCancelled)
        {
            logger.Information("Search was cancelled. No results will be displayed.");
        }

        logger.Information("Searching finished!");
        logger.Information($"Finished searching in directory '{directoryPath}'");

        lastSearchResult = new SearchResult 
        { 
            FoundFiles = foundFiles.ToList(),
            WordCounts = wordCountDict
        };

        if (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            var topWords = wordCountDict.OrderByDescending(kv => kv.Value).Take(10);
            logger.Information("Top 10 most frequent forbidden words:");

            foreach (var kv in topWords)
            {
                logger.Information($"{kv.Key}: {kv.Value} occurrences");
            }
            sw.Stop();
            logger.Information($" Total time taken: {sw.Elapsed.TotalSeconds} seconds");

            return foundFiles.ToList();
        }

        sw.Stop();
        logger.Information($" Total time taken: {sw.Elapsed.TotalSeconds} seconds");

        return new List<string>();
    }
}
