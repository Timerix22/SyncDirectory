using System.Linq;
using DTLib;
using DTLib.Dtsod;

namespace SyncDirectory.Storage;

public class StorageController
{
    public readonly IOPath DataDir;
    public readonly IOPath SnapshotsDir;
    private Dictionary<string, List<DirectorySnapshot>> Snapshots;

    /// <summary>
    /// creates StorageController and parses the program data
    /// </summary>
    /// <param name="dataDir">directory where program data is stored (default=$LocalAppData/SyncDirectory)</param>
    public StorageController(string? dataDir = null)
    {
        if (dataDir == null)
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DataDir = Path.Concat(localAppData, "SyncDirectory");
        }
        else DataDir = dataDir;
        SnapshotsDir = Path.Concat(DataDir, "snapshots");
        Snapshots = new Dictionary<string, List<DirectorySnapshot>>();

        // reading snapshots from SnapshotsDir
        Directory.Create(SnapshotsDir);
        foreach (var snapshotFile in Directory.GetAllFiles(SnapshotsDir))
        {
            string content = File.ReadAllText(snapshotFile);
            DtsodV23 dtsod = new DtsodV23(content);
            var snapshot = DirectorySnapshot.Parse(dtsod);
            if (!Snapshots.TryAdd(snapshot.Name, new List<DirectorySnapshot> { snapshot }))
                Snapshots[snapshot.Name].Add(snapshot);
        }

        // sorting snapshots by time
        foreach (var snapshotCollection in Snapshots)
        {
            Snapshots[snapshotCollection.Key] = snapshotCollection.Value
                .OrderBy(s => s.SnapshotTimeUTC)
                .AsParallel()
                .ToList();
        }
    }

    public bool TryGetSnapshots(string name, out List<DirectorySnapshot>? snapshots)
        => Snapshots.TryGetValue(name, out snapshots!);

    public bool TryGetLatestSnapshot(string name, out DirectorySnapshot? snapshot)
    {
        if (TryGetSnapshots(name, out var snapshots))
        {
            snapshot = snapshots![^1];
            return true;
        }

        snapshot = null;
        return false;
    }

    public DirectorySnapshot CreateSnapshot(string dirName, IOPath dirPath)
    {
        var snapshot = DirectorySnapshot.Create(dirName, dirPath);
        if (!Snapshots.TryAdd(snapshot.Name, new List<DirectorySnapshot> { snapshot }))
            Snapshots[snapshot.Name].Add(snapshot);
        
        // saving to file
        string fileName = $"{Path.ReplaceRestrictedChars(dirName)}-{snapshot.SnapshotTimeUTC.ToString(MyTimeFormat.ForFileNames)}.dtsod";    
        IOPath filePath = Path.Concat(SnapshotsDir, dirName, fileName);
        var dtsod = snapshot.ToDtsod();
        var text = dtsod.ToString();
        File.WriteAllText(filePath, text);
        
        return snapshot;
    }
}