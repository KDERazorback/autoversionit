using System.Text;

namespace AutoVersionIt.Patches;

public static class StreamEncodingHelper
{
    public static Encoding GuessEncodingFor(Stream fs)
    {
        if (fs.Length < 1) return Encoding.Default;
        
        if (HasUtf32Preamble(fs)) return Encoding.UTF32;
        if (HasUtf16Preamble(fs)) return Encoding.Unicode;
        if (HasUtf8Preamble(fs)) return Encoding.UTF8;
        
        if (IsAsciiStream(fs)) return Encoding.ASCII;
        
        return Encoding.UTF8;
    }
    
    public static bool IsUtf8Stream(Stream fs)
    {
        if (HasUtf8Preamble(fs)) return true;
        
        fs.Seek(0, SeekOrigin.Begin);

        while (true)
        {
            int b = fs.ReadByte();
            if (b < 0) break; // End of stream
            if (b == 0x09 || b == 0x0D || b == 0x0A) continue; // TAB or CR or LF
            if (b < 0x20) return true; // Non-ASCII lower part character
            if (b >= 0x7F) return true; // Non-ASCII hi part character
        }

        return false;
    }

    public static bool IsUtf16Stream(Stream fs)
    {
        return HasUtf16Preamble(fs);
    }

    public static bool IsUtf32Stream(Stream fs)
    {
        return HasUtf32Preamble(fs);
    }

    public static bool HasUtf8Preamble(Stream fs)
    {
        if (fs.Length < 3) return false;
        fs.Seek(0, SeekOrigin.Begin);

        var buffer = new byte[3];
        var read = fs.Read(buffer, 0, 3);
        if (read != 3) return false;
        
        return buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF;
    }

    public static bool HasUtf16Preamble(Stream fs)
    {
        if (fs.Length < 2) return false;
        fs.Seek(0, SeekOrigin.Begin);
        
        var buffer = new byte[2];
        var read = fs.Read(buffer, 0, 2);
        if (read != 2) return false;
        
        if (buffer[0] == 0xFF && buffer[1] == 0xFE) return true; // Little Endian
        if (buffer[0] == 0xFE && buffer[1] == 0xFF) return true; // Big Endian

        return false;
    }

    public static bool HasUtf32Preamble(Stream fs)
    {
        if (fs.Length < 4) return false;
        fs.Seek(0, SeekOrigin.Begin);
        
        var buffer = new byte[4];
        var read = fs.Read(buffer, 0, 4);
        if (read != 4) return false;
        
        if (buffer[0] == 0xFF && buffer[1] == 0xFE && buffer[2] == 0x00 && buffer[3] == 0x00) return true; // Little Endian
        if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xFE && buffer[3] == 0xFF) return true; // Big Endian

        return false;
    }

    public static bool IsAsciiStream(Stream fs)
    {
        fs.Seek(0, SeekOrigin.Begin);
        
        while (true)
        {
            int b = fs.ReadByte();
            if (b < 0) break; // End of stream
            if (b == 0x09 || b == 0x0D || b == 0x0A) continue; // TAB or CR or LF
            if (b < 0x20) return false; // Non-ASCII character (0x20 = Space)
            if (b >= 0x7F) return false; // Non-ASCII character (0x7F = DEL)
        }

        return true;
    }

    public static bool IsUnicodeStream(Stream fs)
    {
        return HasUtf32Preamble(fs) || HasUtf16Preamble(fs) || IsUtf8Stream(fs);
    }
}