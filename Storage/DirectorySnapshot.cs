using System.Collections;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using DTLib;
using DTLib.Dtsod;

namespace SyncDirectory.Storage;

public record DirectorySnapshot(
    string Name,
    IOPath Path,
    DateTime SnapshotTimeUTC,
    FileSnapshot[] Files)
{
    public Lazy<Dictionary<string, FileSnapshot>> PathMap = new(() =>
        new Dictionary<string, FileSnapshot>(Files.ToDictionary(f=>f.Path.Str)));
    
    public static DirectorySnapshot Parse(DtsodV23 dtsod) =>
        new DirectorySnapshot(
            dtsod["name"],
            dtsod["path"],
            DateTime.ParseExact(
                    (string)dtsod["snapshot_time"],
                    MyTimeFormat.ForText,
                    CultureInfo.InvariantCulture),
            ((List<object>)dtsod["files"])
                    .Select(fd=>FileSnapshot.Parse((DtsodV23)fd))
                    .ToArray()
        );
    
    public DtsodV23 ToDtsod() =>
        new()
        {
            {"name", Name},
            {"path", Path.ToString()},
            {"snapshot_time", SnapshotTimeUTC.ToString(MyTimeFormat.ForText)},
            {"files", Files.Select(fs=>fs.ToDtsod()).ToImmutableList()}
        };

    public static DirectorySnapshot Create(string name, IOPath dirPath)
    {
        var fileSnapshots = new List<FileSnapshot>();
        foreach (var filePath in Directory.GetAllFiles(dirPath))
            fileSnapshots.Add(FileSnapshot.Create(filePath, dirPath));
        return new DirectorySnapshot(name, dirPath, DateTime.UtcNow, fileSnapshots.ToArray());
    }
}