using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Serilog;
using WordChecker;
using WordCheckerWPF.ViewModels;

namespace WordCheckerWPF
{
    public partial class MainWindow : Window
    {
        private WordsSearcher? wordsSearcher;
        private CancellationTokenSource? cancellationTokenSource;
        private ILogger? logger;
        private Config? config;
        private Task? searchTask;
        private ObservableCollection<DriveProgressViewModel> driveProgress = new();
        private Dictionary<string, int> wordCountDict = new();
        private int totalFilesFound = 0;
        private DateTime searchStartTime;
        private List<string> allFoundFiles = new();

        private object lockObj = new object();

        public MainWindow()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            DriveProgressItemsControl.ItemsSource = driveProgress;
            LoadConfig();
            UpdateUI();
        }

        private void LoadConfig()
        {
            try
            {
                var json = File.ReadAllText("config.json");
                config = System.Text.Json.JsonSerializer.Deserialize<Config>(json);

                if (config != null)
                {
                    InputFileTextBox.Text = config.InputDirectory;
                    OutputDirTextBox.Text = config.OutputDirectory;
                    LogDirTextBox.Text = config.LogDirectory;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Could not load config: {ex.Message}", "Configuration Error");
            }
        }

        private void SaveConfig()
        {
            try
            {
                config = new Config
                {
                    InputDirectory = InputFileTextBox.Text,
                    OutputDirectory = OutputDirTextBox.Text,
                    LogDirectory = LogDirTextBox.Text
                };
                var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("config.json", json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Could not save config: {ex.Message}", "Configuration Error");
            }
        }

        private void BrowseInputFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Select Forbidden Words File"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InputFileTextBox.Text = dialog.FileName;
            }
        }

        private void BrowseOutputDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Output Directory"
            };
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                OutputDirTextBox.Text = dialog.SelectedPath;
            }
            dialog.Dispose();
        }

        private void BrowseLogDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Log Directory"
            };
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                LogDirTextBox.Text = dialog.SelectedPath;
            }
            dialog.Dispose();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputFileTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please select an input file with forbidden words.", "Validation Error");
                return;
            }
            if (!File.Exists(InputFileTextBox.Text))
            {
                System.Windows.MessageBox.Show("Input file does not exist.", "Validation Error");
                return;
            }
            if (string.IsNullOrWhiteSpace(OutputDirTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please select an output directory.", "Validation Error");
                return;
            }
            if (string.IsNullOrWhiteSpace(LogDirTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please select a log directory.", "Validation Error");
                return;
            }

            try
            {
                // Ensure output directory exists
                if (!Directory.Exists(OutputDirTextBox.Text))
                {
                    Directory.CreateDirectory(OutputDirTextBox.Text);
                }
                // Ensure log directory exists
                if (!Directory.Exists(LogDirTextBox.Text))
                {
                    Directory.CreateDirectory(LogDirTextBox.Text);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to create directories: {ex.Message}", "Directory Creation Error");
                return;
            }

            // Disable Start button immediately to prevent double-click
            StartButton.IsEnabled = false;
            PauseButton.IsEnabled = true;
            ResumeButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            SaveConfig();
            StartSearch();
        }

        private void StartSearch()
        {
            try
            {
                // Read UI values on the UI thread BEFORE starting background task
                var inputFilePath = InputFileTextBox.Text;
                var outputDirectory = OutputDirTextBox.Text;

                cancellationTokenSource = new CancellationTokenSource();
                logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File(Path.Combine(outputDirectory, "search-.log"), rollingInterval: RollingInterval.Hour)
                    .CreateLogger();

                logger.Information("Application started");
                logger.Information($"Output Directory: {outputDirectory}");
                logger.Information($"Log Directory: {outputDirectory}");

                wordsSearcher = new WordsSearcher(logger, cancellationTokenSource);

                ResetUI();
                // DO NOT call UpdateUI() here - button states are already set in Start_Click()

                searchStartTime = DateTime.Now;
                searchTask = Task.Run(async () => await PerformSearch(inputFilePath, outputDirectory));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to start search: {ex.Message}\n\n{ex.StackTrace}", "Startup Error");
                logger?.Error($"Failed to start search: {ex}");
                UpdateUI(); // Only call UpdateUI on error to reset buttons
            }
        }

        private async Task PerformSearch(string inputFilePath, string outputDirectory)
        {
            try
            {
                logger?.Information("Reading forbidden words from file...");
                var forbiddenWords = new HashSet<string>(
                    File.ReadAllLines(inputFilePath)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                );

                logger?.Information($"Loaded {forbiddenWords.Count} forbidden words");

                if (forbiddenWords.Count == 0)
                {
                    logger?.Information("No forbidden words found in the input file. Aborting search.");
                    Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show("No forbidden words found in the input file. Please add some words and try again.", "No Forbidden Words");
                        UpdateUI();
                    });
                    return;
                }

                var drives = DriveInfo.GetDrives();
                var readyDrives = drives.Where(d => d.IsReady).ToList();
                logger?.Information($"Found {readyDrives.Count} ready drives");

                Dispatcher.Invoke(() =>
                {
                    driveProgress.Clear();
                    foreach (var drive in readyDrives)
                    {
                        driveProgress.Add(new DriveProgressViewModel { DriveName = drive.Name, Progress = 0, Status = "Waiting..." });
                    }
                });

                if (readyDrives.Count == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show("No ready drives found to search.", "No Drives");
                        UpdateUI();
                    });
                    return;
                }

                var tasks = readyDrives.Select((drive, index) => Task.Run(() =>
                {
                    var vm = driveProgress[index];
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            vm.Status = "Searching...";
                            vm.Progress = 50; // Show partial progress while searching
                        });

                        logger?.Information($"Starting search on drive {drive.Name}");
                        var foundFiles = wordsSearcher.SearchWordsInDirectory(
                            drive.RootDirectory.FullName,
                            forbiddenWords,
                            outputDirectory
                        );

                        var result = wordsSearcher.GetLastSearchResult();
                        Dispatcher.Invoke(() =>
                        {
                            vm.Progress = 100;
                            vm.Status = $"Completed - {foundFiles.Count} files found";
                            totalFilesFound += foundFiles.Count;
                            allFoundFiles.AddRange(foundFiles);

                            if (result != null)
                            {
                                foreach (var kvp in result.WordCounts)
                                {
                                    if (wordCountDict.ContainsKey(kvp.Key))
                                        wordCountDict[kvp.Key] += kvp.Value;
                                    else
                                        wordCountDict[kvp.Key] = kvp.Value;
                                }
                            }

                            TotalFilesTextBlock.Text = totalFilesFound.ToString();
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            vm.Progress = 0;
                            vm.Status = "Cancelled";
                        });
                        logger?.Information($"Search cancelled on drive {readyDrives[index].Name}");
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            vm.Progress = 0;
                            vm.Status = $"Error: {ex.Message}";
                            logger?.Error($"Error searching drive {readyDrives[index].Name}: {ex}");
                        });
                    }
                })).ToList();

                await Task.WhenAll(tasks);

                Dispatcher.Invoke(() =>
                {
                    DisplayResults();
                    UpdateUI();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show($"Search failed: {ex.Message}\n\n{ex.StackTrace}", "Search Error");
                    logger?.Error($"Search failed: {ex}");
                    UpdateUI();
                });
            }
        }

        private void ResetUI()
        {
            ResultsTextBlock.Text = "Searching...";
            TotalFilesTextBlock.Text = "0";
            TotalMatchesTextBlock.Text = "0";
            UniqueWordsTextBlock.Text = "0";
            ExecutionTimeTextBlock.Text = "0.0 sec";
            TopWordsListBox.ItemsSource = null;
            ResultsDataGrid.ItemsSource = null;
            wordCountDict.Clear();
            totalFilesFound = 0;
            allFoundFiles.Clear();
        }

        private void DisplayResults()
        {
            if (cancellationTokenSource?.Token.IsCancellationRequested ?? false)
            {
                ResultsTextBlock.Text = "Search was cancelled. No results to display.";
                return;
            }

            var timeElapsed = DateTime.Now - searchStartTime;
            ExecutionTimeTextBlock.Text = $"{timeElapsed.TotalSeconds:F2} sec";

            var totalMatches = wordCountDict.Values.Sum();
            TotalMatchesTextBlock.Text = totalMatches.ToString();
            UniqueWordsTextBlock.Text = wordCountDict.Count.ToString();

            var topWords = wordCountDict.OrderByDescending(kv => kv.Value).Take(10).ToList();
            TopWordsListBox.ItemsSource = topWords;

            var resultLines = new List<string>
            {
                $"Search completed successfully!",
                $"Total files found: {totalFilesFound}",
                $"Total forbidden words found: {totalMatches}",
                $"Unique forbidden words: {wordCountDict.Count}",
                $"Execution time: {timeElapsed.TotalSeconds:F2} seconds",
                "",
                "Files found:"
            };

            foreach (var file in allFoundFiles)
            {
                resultLines.Add($"  - {file}");
            }

            ResultsTextBlock.Text = string.Join("\n", resultLines);

            var fileData = allFoundFiles.Select(f => new { File = f, FileInfo = new FileInfo(f) }).ToList();
            ResultsDataGrid.ItemsSource = fileData;

        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            wordsSearcher?.Pause();
            PauseButton.IsEnabled = false;
            ResumeButton.IsEnabled = true;
            logger?.Information("Search paused by user.");
        }

        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            wordsSearcher?.Resume();
            PauseButton.IsEnabled = true;
            ResumeButton.IsEnabled = false;
            logger?.Information("Search resumed by user.");
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            wordsSearcher?.Stop();
            cancellationTokenSource?.Cancel();
            UpdateUI();
            logger?.Information("Search stopped by user.");
        }

        private void UpdateUI()
        {
            var isSearching = searchTask != null && !searchTask.IsCompleted;

            if (isSearching)
            {
                StartButton.IsEnabled = false;
                PauseButton.IsEnabled = true;
                ResumeButton.IsEnabled = true;
                StopButton.IsEnabled = true;
                InputFileTextBox.IsEnabled = false;
                OutputDirTextBox.IsEnabled = false;
                LogDirTextBox.IsEnabled = false;
            }
            else
            {
                StartButton.IsEnabled = true;
                PauseButton.IsEnabled = false;
                ResumeButton.IsEnabled = false;
                StopButton.IsEnabled = false;
                InputFileTextBox.IsEnabled = true;
                OutputDirTextBox.IsEnabled = true;
                LogDirTextBox.IsEnabled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            wordsSearcher?.Stop();
            cancellationTokenSource?.Cancel();
            logger?.Information("Application closed.");
        }
    }
}
