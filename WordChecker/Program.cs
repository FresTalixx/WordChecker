//Фінальне завдання:

//Реалізуйте додаток, який дозволить відшукати певний набір заборонених
//слів у файлах.
//Інтерфейс користувача додатка має дозволити ввести або завантажити з файлу набір заборонених слів. 

//Натиснувши на кнопку «Старт», додаток має почати
//шукати ці слова на всіх доступних накопичувачах інформації (жорсткі диски,
//флешки).
//Файли, що містять заборонені слова, мають бути скопійовані у задану папку.
//Крім файлу-оригіналу, створіть новий файл із вмістом файлу-оригіналу,
//в якому заборонені слова замінені на 7 повторюваних символів зірочок (*******).

//Також створіть файл звіту. Він має містити інформацію про всі знайдені файли
//із забороненими словами, шляхи до цих файлів, розмір файлів, інформацію про
//кількість замін тощо. У файлі звіту відобразіть також Топ-10 найпопулярніших
//заборонених слів.
//Інтерфейс програми має показувати прогрес роботи додатка за допомогою
//індикаторів (progress bars). Користувач через інтерфейс додатка може призупинити роботу алгоритму,
//відновити, повністю зупинити.
//За підсумками роботи програми, виведіть результати роботи в елементи користувацького
// інтерфейсу (продумайте, які елементи керування можуть знадобитися).
//Програма обов’язково має використовувати механізми багатопотоковості та синхронізації!
//Програма може бути запущена лише в одній копії. 

//ШІ юзати можна? - ТАК
//додаток має шукати заборонені слова по txt та md файлах? - Можно додати і підримку md файлів

using System;
using System.Collections.Generic;
using WordChecker;
using Serilog;


using var mutex = new Mutex(true, "WordCheckerMutex", out bool createdNew);

if (!createdNew)
{
    Console.WriteLine("Application is already running.");
    return;
}

var json = new StreamReader("config.json").ReadToEnd();
var config = System.Text.Json.JsonSerializer.Deserialize<Config>(json);

if (config == null)
{
    Console.WriteLine("Failed to load configuration.");
    return;
}

var inputFilePath = config.InputDirectory;
var outputDirectory = config.OutputDirectory;
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(config.LogDirectory, rollingInterval: RollingInterval.Hour)
    .CreateLogger();

var cancellationTokenSource = new CancellationTokenSource();

var forbiddenWords = new HashSet<string>(File.ReadAllLines(inputFilePath));

var drives = DriveInfo.GetDrives();

var wordsSearcher = new WordsSearcher(logger, cancellationTokenSource);
var totalForbWordsCount = 0;
var tasks = drives.Where(d => d.IsReady).Select(drive => Task.Run(() =>
{
    var words = wordsSearcher.SearchWordsInDirectory(drive.RootDirectory.FullName, forbiddenWords, outputDirectory);
    logger.Information($"Drive {drive.Name}: Found {words.Count} files with forbidden words.");
    Interlocked.Add(ref totalForbWordsCount, words.Count);
})).ToList();

var inputTask = Task.Run(() =>
{
    while (!cancellationTokenSource.Token.IsCancellationRequested)
    {
        var key = Console.ReadKey(true).Key;
        if (key == ConsoleKey.P)
        {
            wordsSearcher.Pause();
            logger.Information("Search paused.");
        }
        else if (key == ConsoleKey.R)
        {
            wordsSearcher.Resume();
            logger.Information("Search resumed.");
        }
        else if (key == ConsoleKey.S)
        {
            wordsSearcher.Stop();
            logger.Information("Search stopped.");
            break;
        }
    }
});

tasks.Add(inputTask);


await Task.WhenAll(tasks);

logger.Information($"Found {totalForbWordsCount} files with forbidden words.");
//foreach (var drive in drives)
//{
//    if (drive.IsReady)
//    {
//var words = wordsSearcher.SearchWordsInDirectory("D:\\IT STEP", forbiddenWords, outputDirectory);
//        Console.WriteLine(string.Join(' ', words));
//    }
//}



