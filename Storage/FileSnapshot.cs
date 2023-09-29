using System.Globalization;
using DTLib;
using DTLib.Dtsod;

namespace SyncDirectory.Storage;

public record FileSnapshot(
    IOPath Path,
    FileSize Size,
    DateTime ModifyTimeUTC,
    DateTime SnapshotTimeUTC)
{
    /// <summary>
    /// Creates new FileSnapshot from an exnsting file
    /// </summary>
    /// <param name="fullFilePath">baseDirPath+filePathInBaseDir</param>
    /// <param name="baseDirPath">will be removed from fullFilePath in the rezult FileSnapshot.Path</param>
    /// <returns></returns>
    public static FileSnapshot Create(IOPath fullFilePath, IOPath baseDirPath)
    {
        var creationTime = System.IO.File.GetCreationTimeUtc(fullFilePath.Str);
        var modifyTime = System.IO.File.GetLastWriteTimeUtc(fullFilePath.Str);
        // sometimes LastWriteTime can be less then CreationTime when unpacking archives
        var latest = modifyTime > creationTime ? modifyTime : creationTime;
        return new FileSnapshot(fullFilePath.RemoveBase(baseDirPath), FileSize.Get(fullFilePath), latest, DateTime.UtcNow);
    }

    public static FileSnapshot Parse(DtsodV23 dtsod) =>
        new FileSnapshot(
            dtsod["path"],
            FileSize.Parse(dtsod["size"]),
            DateTime.ParseExact((string)dtsod["modify_time"], MyTimeFormat.ForText, CultureInfo.InvariantCulture),
            DateTime.ParseExact((string)dtsod["snapshot_time"], MyTimeFormat.ForText, CultureInfo.InvariantCulture)
        );

    public DtsodV23 ToDtsod() =>
        new()
        {
            {"path", Path.ToString()},
            {"size", Size.ToString()},
            {"modify_time", ModifyTimeUTC.ToString(MyTimeFormat.ForText)},
            {"snapshot_time", SnapshotTimeUTC.ToString(MyTimeFormat.ForText)}
        };
}