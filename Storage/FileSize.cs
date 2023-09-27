using System.Globalization;

namespace SyncDirectory.Storage;

public class FileSize
{
    public long Bytes { get; }
    public double KiloBytes => Bytes / 1024.0;
    public double MegaBytes => KiloBytes / 1024.0;
    public double GigaBytes => MegaBytes / 1024.0;

    public FileSize(long bytes) => Bytes = bytes;
    
    public static FileSize Get(IOPath filePath) => new(File.GetSize(filePath));

    public static FileSize Parse(string str)
    {
        var caseInvariant = str.ToLowerInvariant().AsSpan();
        long multi = 1;
        int skipLast = 0;
        if (caseInvariant[^1] == 'b')
            skipLast++;
        switch (caseInvariant[^(1 + skipLast)])
        {
            case 'k':
                multi = 1024;
                skipLast++;
                break;
            case 'm':
                multi = 1024*1024;
                skipLast++;
                break;
            case 'b':
                multi = 1024*1024*1024;
                skipLast++;
                break;
        }

        var numberStr = caseInvariant[..^skipLast].ToString();
        double number = Convert.ToDouble(numberStr);
        return new FileSize((long)(number * multi + 0.5));
    }

    public override string ToString()
    {
        if (GigaBytes > 1) return GigaBytes.ToString(CultureInfo.InvariantCulture) + "G";
        if (MegaBytes > 1) return MegaBytes.ToString(CultureInfo.InvariantCulture) + "M";
        if (KiloBytes > 1) return KiloBytes.ToString(CultureInfo.InvariantCulture) + "K";
        return Bytes.ToString(CultureInfo.InvariantCulture);
    }
}