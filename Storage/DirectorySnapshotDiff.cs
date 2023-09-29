using System.Linq;

namespace SyncDirectory.Storage;

public record DirectorySnapshotDiff(ICollection<FileSnapshotDiff> FileDiffs)
{
    public static DirectorySnapshotDiff Diff(DirectorySnapshot firstLocal, DirectorySnapshot secondLocal)
    {
        List<FileSnapshotDiff> files = new();
        foreach (var file in secondLocal.Files)
        {
            if(firstLocal.PathMap.Value.TryGetValue(file.Path.Str, out var filePrev))
            {
                files.Add(new FileSnapshotDiff(file,
                    file.ModifyTimeUTC == filePrev.ModifyTimeUTC ? FileStatus.Unchanged : FileStatus.Modified));
            }
            else  files.Add(new FileSnapshotDiff(file, FileStatus.Created));
        }

        foreach (var filePrev in firstLocal.Files)
        {
            if(!secondLocal.PathMap.Value.ContainsKey(filePrev.Path.Str))
                files.Add(new FileSnapshotDiff(filePrev, FileStatus.Deleted));
        }

        return new DirectorySnapshotDiff(files);
    }
}