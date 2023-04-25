using CommandLine;
using DirectoryCleaner;
using static DirectoryCleaner.OSObjectFactory;

bool verboseMode = false;

string sourceDirectory = "";

string timeToJunk = "1m";

string JunkFolderName = "Junk";

Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       sourceDirectory = o.SourceDirectory;
                       timeToJunk = o.TimeToJunk;
                       JunkFolderName = o.JunkFolderName;
                       if (o.Verbose)
                       {
                           Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}  -s {o.SourceDirectory} -t {o.TimeToJunk} -j {o.JunkFolderName}");
                           verboseMode = true;
                       }

                   });
Console.WriteLine("");

if (string.IsNullOrWhiteSpace(sourceDirectory)) return 0;

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
List<IOSEntity> entities = GetOSObjectsInDirectory(sourceDirectory);
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


List<IOSEntity> GetOSObjectsInDirectory(string path)
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

public class Options
{

    #if RELEASE
    static readonly string DEFAULT_WORKING_DIRECTORY = Directory.GetCurrentDirectory();
    #endif
    #if DEBUG
        static readonly string DEFAULT_WORKING_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    #endif

    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }


    /// <summary>
    /// TODO: Implement prompting the user before moving directories by default and make this flag work
    /// </summary>
    [Option('n', "no-prompt", Required = false, HelpText = "Whether the program should confirm with the user to move files and directories.")]
    public bool NoPrompt { get; set; }

    [Option('s', "source-directory", Required = false, HelpText = "The directory to clean.")]
    public string SourceDirectory { get; set; } = DEFAULT_WORKING_DIRECTORY; // Current working directory by default

    [Option('t', "time-to-look-back", Required = false, HelpText = "How much time to look back before considering the file or directory junk. Default is 1m. Example: 3m 5w 2d 5h")]
    public string TimeToJunk { get; set; } = "1m";

    [Option('j', "junk-folder-name", Required = false, HelpText = "What the output folder is named. Default is 'Junk'.")]
    public string JunkFolderName { get; set; } = "Junk";

}