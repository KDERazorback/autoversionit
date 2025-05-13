namespace AutoVersionIt.Interop;

public interface IChildProcess
{
    string BinaryPath { get; }
    string Arguments { get; }
    bool ShouldRunInBackground { get; }
    IChildProcess Binary(string path);
    IChildProcess WithArguments(string args);
    IChildProcess InBackground();
    IChildProcess InForeground();
    IChildProcess Run();
    IChildProcess RunAndWait();
    IChildProcess WaitForExit();
    StreamReader StdOut();
    StreamReader StdErr();
    int ExitCode();
    void Dispose();
}