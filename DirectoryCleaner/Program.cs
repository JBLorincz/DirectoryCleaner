using System.CommandLine;
using System.CommandLine.Invocation;
using DirectoryCleaner;
using static DirectoryCleaner.OSObjectFactory;

#if RELEASE
     string DEFAULT_WORKING_DIRECTORY = Directory.GetCurrentDirectory();
#endif
#if DEBUG
 string DEFAULT_WORKING_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
#endif

int returnCode = 0;

RootCommand rootCommand = new();

var sourceDirectoryOption = new Option<string>("--source-directory", () => DEFAULT_WORKING_DIRECTORY, "The directory to clean. By default is the working directory");
sourceDirectoryOption.AddAlias("-d");

var timeToLookBackOption = new Option<string>("--time-to-look-back", () => "1m", "How much time to look back before considering the file or directory junk. Example: 3m 5w 2d 5h");
timeToLookBackOption.AddAlias("-t");

var junkFolderNameOption = new Option<string>("--output-directory", () => "Junk", "What the output folder is named.");
junkFolderNameOption.AddAlias("-j");

var verboseModeOption = new Option<bool>("--verbose", () => false, "Whether to show verbose output");
verboseModeOption.AddAlias("-v");

rootCommand.AddOption(sourceDirectoryOption);
rootCommand.AddOption(timeToLookBackOption);
rootCommand.AddOption(junkFolderNameOption);
rootCommand.AddOption(verboseModeOption);

rootCommand.SetHandler((directoryToClean,timeUntilJunk,junkFolderName,verboseMode) => 
{
    if (verboseMode)
    {
        Console.WriteLine($"--source-directory = {directoryToClean}");
        Console.WriteLine($"--t = {timeUntilJunk}");
        Console.WriteLine($"-j = {junkFolderName}");
        Console.WriteLine($"-v = {verboseMode}");
    }

    if (string.IsNullOrWhiteSpace(directoryToClean)) return;

    returnCode = Clean(verboseMode, directoryToClean, timeUntilJunk, junkFolderName);

}, sourceDirectoryOption,timeToLookBackOption,junkFolderNameOption,verboseModeOption);

rootCommand.Invoke(args);
    

Console.WriteLine("");


return returnCode;
List<IOSEntity> GetOSObjectsInDirectory(string sourceDirectory, string outputDirectory)
{
    List<IOSEntity> directoryObjects = new();

    IEnumerable<string> unfilteredObjectsInSourceDirectory = Directory.EnumerateFileSystemEntries(sourceDirectory);
    IEnumerable<string> filteredObjectsInSourceDirectory = from @object in unfilteredObjectsInSourceDirectory where @object != outputDirectory select @object;



    foreach (string item in filteredObjectsInSourceDirectory)
    {
        directoryObjects.Add(CreateIOSEntityFromPath(item));
    }
    return directoryObjects;
}

int Clean(bool verboseMode = false,string sourceDirectory = "",string timeToJunk = "1m",string JunkFolderName = "Junk"
)
{
    string outputDirectory = sourceDirectory + "\\" + JunkFolderName;



    JunkIdentifier identifier = new(timeToJunk);




    if (!Directory.Exists(sourceDirectory))
    {
        Console.WriteLine("Source directory does not exist!");
        Thread.Sleep(1000);
        return 1;
    }

    if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);


    DateTime currentTime = DateTime.Now;
    List<IOSEntity> entities = GetOSObjectsInDirectory(sourceDirectory,outputDirectory);
    foreach (IOSEntity entity in entities)
    {
        if (verboseMode) Console.WriteLine($"Trying {entity.Name}");
        DateTime lastTimeAccessed = entity.GetLastWriteTimeDate();
        if (verboseMode) Console.WriteLine($"{entity.Name}: Last accessed at {lastTimeAccessed.ToString()}");

        if (currentTime > lastTimeAccessed + identifier.DateOffsetToTimespan())
        {
            try
            {
                if (!entity.IsValid() && verboseMode)
                {
                    Console.WriteLine($"Skipping the hidden object {entity.Name}");
                    Console.WriteLine();
                }


                if (!entity.IsValid()) continue;


                entity.MoveToDirectory(outputDirectory);

                if (verboseMode) Console.WriteLine($"Moving {entity.Name} to {outputDirectory}");
                else Console.WriteLine($"Junking {entity.Name}");
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Error moving {entity.Name} to {outputDirectory}!");
                if (verboseMode) Console.WriteLine(exc.Message);
            }
            Console.WriteLine();
        }
    }
    Console.WriteLine("Cleanup finished!");
    Thread.Sleep(1000);
    return 0;

}