using System.Text.Json;
using LmuStatsViewer.Domain;

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("Welcome to the LMU Stats Viewer...");
Console.ResetColor();
FindPathToResultsFolder();

var allDrivingSessions = new List<DrivingSession>();
FindProcessedDrivingSessionsAndLoad();
FindAllUnprocessedFilesAndProcess();
PrintMainStatistics();

var menu = new List<string> { "Overview", "Favourite tracks", "Favourite cars", "Quit LMU Stats Viewer" };
var menuChoices = new List<char> { 'o', 't', 'c', 'q' };
char menuChoice = 'o';
while (menuChoice != 'q' && menuChoice != 'Q')
{
    PrintMenu();
    menuChoice = Console.ReadKey().KeyChar;
    Console.WriteLine();
    switch (menuChoice)
    {
        case 'o':
        case 'O':
            PrintMainStatistics();
            break;
        case 't':
        case 'T':
            PrintTrackOverview();
            break;
        case 'c':
        case 'C':
            PrintCarOverview();
            break;
        case 'q':
        case 'Q':
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Thanks for using LMU Stats Viewer!");
            Console.ResetColor();
            break;
    }
}

#region Printing methods

void PrintMenu()
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("\nPlease choose an option:");
    Console.ResetColor();
    for (int i = 0; i < menu.Count; i++)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"[{menuChoices[i]}] ");
        Console.ResetColor();
        Console.WriteLine($"{menu[i]}");
    }
    Console.Write("\nYour choice: ");
}

void PrintTrackOverview()
{
    Print.Header("Favourite tracks");
    var favouriteTrackCoursesDistance = allDrivingSessions.GroupBy(ds => ds.TrackCourse).Select(g => new { TrackCourse = g.Key, Distance = g.Sum(ds => ds.DrivenDistanceInMeters) / 1000 }).OrderByDescending(d => d.Distance).ToList();
    for (int i = 0; i < favouriteTrackCoursesDistance.Count; i++)
    {
        if (favouriteTrackCoursesDistance[i].Distance < 0.01) break;
        Print.Line($"{i + 1}. {favouriteTrackCoursesDistance[i].TrackCourse}", favouriteTrackCoursesDistance[i].Distance.ToString("0.00"), "km");   
    }

}

void PrintCarOverview()
{
    Print.Header("Favourite cars");
    var favouriteCarsDistance = allDrivingSessions.GroupBy(ds => ds.CarType).Select(g => new { CarType = g.Key, Distance = g.Sum(ds => ds.DrivenDistanceInMeters) / 1000 }).OrderByDescending(d => d.Distance).ToList();
    for (int i = 0; i < favouriteCarsDistance.Count; i++)
    {
        if (favouriteCarsDistance[i].Distance < 0.01) break;
        Print.Line($"{i + 1}. {favouriteCarsDistance[i].CarType}", favouriteCarsDistance[i].Distance.ToString("0.00"), "km");
    }

}

void PrintMainStatistics()
{
    Console.WriteLine("\n\nAll these values are estimates, since data collected from the game is not super accurate.\n");
    
    Print.Header("Total distance");
    var totalDistanceInKm = allDrivingSessions.Sum(ds => ds.DrivenDistanceInMeters) / 1000;
    Print.Line("Total distance", totalDistanceInKm.ToString("0.00"), "km");
    
    Print.Header("Distance per class");
    var distancePerClass = allDrivingSessions.GroupBy(ds => ds.CarClass)
        .Select(g => new { Class = g.Key, Distance = g.Sum(ds => ds.DrivenDistanceInMeters) / 1000 })
        .OrderByDescending(d => d.Distance)
        .ToList();
    foreach (var distancePerClassItem in distancePerClass)
    {
        if (distancePerClassItem.Distance < 0.01) break;
        Print.Line(distancePerClassItem.Class, distancePerClassItem.Distance.ToString("0.00"), "km");
    }
    
    Print.Header("Favourite cars");
    var favouriteCarsDistance = allDrivingSessions.GroupBy(ds => ds.CarType).Select(g => new { CarType = g.Key, Distance = g.Sum(ds => ds.DrivenDistanceInMeters) / 1000 }).OrderByDescending(d => d.Distance).ToList();
    for (int i = 0; i < 3; i++)
    {
        Print.Line($"{i + 1}. {favouriteCarsDistance[i].CarType}", favouriteCarsDistance[i].Distance.ToString("0.00"), "km");
    }
    
    Print.Header("Favourite tracks");
    var favouriteTrackCoursesDistance = allDrivingSessions.GroupBy(ds => ds.TrackCourse).Select(g => new { TrackCourse = g.Key, Distance = g.Sum(ds => ds.DrivenDistanceInMeters) / 1000 }).OrderByDescending(d => d.Distance).ToList();
    for (int i = 0; i < 3; i++)
    {
        Print.Line($"{i + 1}. {favouriteTrackCoursesDistance[i].TrackCourse}", favouriteTrackCoursesDistance[i].Distance.ToString("0.00"), "km");   
    }
}
#endregion

#region Helper methods
void FindProcessedDrivingSessionsAndLoad()
{
    if (Settings.PathToStatisticsFile == null)
    {
        Console.WriteLine("Path to statistics file not found!");
        return;
    }
    
    if (File.Exists(Settings.PathToStatisticsFile))
    {
        var statistics = File.ReadAllText(Settings.PathToStatisticsFile);
        allDrivingSessions = JsonSerializer.Deserialize<List<DrivingSession>>(statistics);
    }
}

void SaveStatistics()
{
    if (Settings.PathToStatisticsFile == null)
    {
        Console.WriteLine("Path to statistics file not found!");
        return;
    }
    
    File.WriteAllText(Settings.PathToStatisticsFile, JsonSerializer.Serialize(allDrivingSessions));
}

void FindAllUnprocessedFilesAndProcess()
{
    if (Settings.PathToResultsFolder == null)
    {
        Console.WriteLine("Path to results folder not found!");
        return;
    }
    
    var allFiles = Directory.GetFiles(Path.Combine(Settings.PathToResultsFolder, "UserData\\Log\\Results"), "*.xml");
    var failedFiles = new List<string>();
    var newFiles = new List<string>();
    foreach (var file in allFiles)
    {
        if (allDrivingSessions.Any(ds => ds.SessionFile == Path.GetFileName(file))) continue;
        
        Console.WriteLine($"Processing {file}...");
        var session = DrivingSession.MakeFromFile(file);
        if (session.SessionType == SessionType.Unknown)
        {
            failedFiles.Add(file);
            continue;
        }
        
        newFiles.Add(file);
        allDrivingSessions.Add(session);
    }
    Console.WriteLine($"Failed files: {failedFiles.Count}");
    Console.WriteLine($"New sessions added: {newFiles.Count}");
    if (newFiles.Count > 0) SaveStatistics();
}

static void FindPathToResultsFolder()
{
    if (!File.Exists("settings.ini"))
    {
        Console.WriteLine("Please enter the FULL (absolute) path to your steam folder containing Le Mans Ultimate (e.g. C:\\Program Files (x86)\\Steam\\steamapps\\common\\Le Mans Ultimate) and hit Enter: ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Path: ");
        Console.ForegroundColor = ConsoleColor.Gray;
        var fullPath = Console.ReadLine();
        
        if (Directory.Exists(fullPath) && Directory.Exists(Path.Combine(fullPath, "UserData\\Log\\Results")))
        {
            Console.WriteLine("Path found & Results folder found!");
        }
        
        Console.WriteLine("Please enter your driver's name as you use it in LMU, with correct capitalization:");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Name: ");
        Console.ForegroundColor = ConsoleColor.Gray;
        var playerName = Console.ReadLine();

        Console.WriteLine("Please enter the FULL (absolute, start with C:\\...) path to the directory where you wish to store your statistics file: ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Name: ");
        Console.ForegroundColor = ConsoleColor.Gray;
        var statisticsPath = Console.ReadLine();

        if (Directory.Exists(statisticsPath))
        {
            Console.WriteLine("Statistics path found!");
        }

        if (statisticsPath != null)
        {
            File.WriteAllText("settings.ini", $"PATH_TO_LMU={fullPath}\n");
            File.AppendAllText("settings.ini", $"PLAYER_NAME={playerName}\n");
            File.AppendAllText("settings.ini", $"PATH_TO_STATISTICS_FILE={Path.Combine(statisticsPath, $"{playerName}_LMU.log")}\n");;
        }
    }
    else
    {
        var settingsContents = File.ReadAllText("settings.ini");
        Dictionary<string, string> settings = new Dictionary<string, string>();
        foreach (var setting in settingsContents.Split('\n'))
        {
            if (!setting.Contains('=')) continue;
            settings.Add(setting.Split('=')[0], setting.Split('=')[1]);

            if (settings.ContainsKey("PATH_TO_LMU"))
            {
                Settings.PathToResultsFolder = settings["PATH_TO_LMU"];
            }
            
            if (settings.ContainsKey("PATH_TO_STATISTICS_FILE"))
            {
                Settings.PathToStatisticsFile = settings["PATH_TO_STATISTICS_FILE"];
            }

            if (settings.ContainsKey("PLAYER_NAME"))
            {
                Settings.PlayerName = settings["PLAYER_NAME"];
            }
        }
    }
}
#endregion