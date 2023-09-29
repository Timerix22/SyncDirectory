global using System;
global using System.Text;
global using System.Collections.Generic;
global using DTLib.Filesystem;
global using File = DTLib.Filesystem.File;
global using Directory = DTLib.Filesystem.Directory;
using DTLib.Ben.Demystifier;
using DTLib.Extensions;
using SyncDirectory.Storage;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
Console.Title = "SyncDirectory";

try
{
    var storage = new StorageController(
#if DEBUG
        "SyncDirectory-data-debug"
#endif
    );
    var s1=storage.CreateSnapshot("tmp", "tmp");
    var s2=storage.CreateSnapshot("tmp2", "tmp2");
    var diff = DirectorySnapshotDiff.Diff(s1, s2);
    Console.WriteLine(diff.FileDiffs.MergeToString('\n'));
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex.ToStringDemystified());
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine();
}
