using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AutoVersionIt.Interop;

public class ChildProcess : IDisposable, IChildProcess
{
    protected ILogger? Logger { get; }
    protected bool StreamsRead = false;
    private bool _disposed;
    
    protected MemoryStream StdOutStream { get; } = new MemoryStream();
    protected MemoryStream StdErrStream { get; } = new MemoryStream();
    
    public string BinaryPath { get; protected set; } = string.Empty;
    public string Arguments { get; protected set; } = string.Empty;

    public bool ShouldRunInBackground { get; protected set; } = false;
    
    protected Process? ChildProcessObject { get; set; } = null;

    public ChildProcess()
    { }
    
    public ChildProcess(ILogger? logger)
    {
        Logger = logger;
    }

    public IChildProcess Binary(string path)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ChildProcess));
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
        BinaryPath = path;
        return this;
    }

    public IChildProcess WithArguments(string args)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ChildProcess));
        Arguments = args;
        return this;
    }

    public IChildProcess InBackground()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ChildProcess));
        ShouldRunInBackground = true;
        return this;
    }

    public IChildProcess InForeground()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ChildProcess));
        ShouldRunInBackground = false;
        return this;
    }

    public virtual IChildProcess Run()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ChildProcess));
        if (ChildProcessObject is not null) throw new InvalidOperationException("Child process is already running.");

        var psi = new ProcessStartInfo(BinaryPath, Arguments);

        if (ShouldRunInBackground)
        {
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
        }
        else
        {
            psi.CreateNoWindow = false;
            psi.WindowStyle = ProcessWindowStyle.Normal;
        }

        psi.UseShellExecute = false;
        
        ChildProcessObject = new Process();
        ChildProcessObject.StartInfo = psi;
        ChildProcessObject.Start();
        ChildProcessObject.PriorityClass = ProcessPriorityClass.BelowNormal;
        ChildProcessObject.StandardInput.Close();

        return this;
    }

    public virtual IChildProcess RunAndWait()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ChildProcess));
        if (ChildProcessObject is not null) throw new InvalidOperationException("Child process is already running.");

        Run();
        WaitForExit();
        
        return this;
    }
    public virtual IChildProcess WaitForExit()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ChildProcess));
        if (ChildProcessObject is null) throw new InvalidOperationException("Child process is not started.");
        
        ChildProcessObject.WaitForExit();

        return this;
    }

    public virtual StreamReader StdOut()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ChildProcess));
        if (ChildProcessObject is null) throw new InvalidOperationException("Child process is not started.");
        if (!ChildProcessObject.HasExited) throw new InvalidOperationException("Child process is still running.");

        if (!StreamsRead)
            ReadStandardStreams();
        
        StdOutStream.Seek(0, SeekOrigin.Begin);
        return new StreamReader(StdOutStream, Encoding.UTF8, leaveOpen: true);
    }
    
    public virtual StreamReader StdErr()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ChildProcess));
        if (ChildProcessObject is null) throw new InvalidOperationException("Child process is not started.");
        if (!ChildProcessObject.HasExited) throw new InvalidOperationException("Child process is still running.");

        if (!StreamsRead)
            ReadStandardStreams();
        
        StdErrStream.Seek(0, SeekOrigin.Begin);
        return new StreamReader(StdErrStream, Encoding.UTF8, leaveOpen: true);
    }
    
    public virtual int ExitCode()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ChildProcess));
        if (ChildProcessObject is null) throw new InvalidOperationException("Child process is not started.");
        if (!ChildProcessObject.HasExited) throw new InvalidOperationException("Child process is still running.");
        
        return ChildProcessObject.ExitCode;
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            StdOutStream.Dispose();
            StdErrStream.Dispose();
            ChildProcessObject?.Dispose();
        }
    
        _disposed = true;
    }
    
    ~ChildProcess()
    {
        Dispose(false);
    }
    
    protected virtual void ReadStandardStreams()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ChildProcess));
        if (StreamsRead) throw new InvalidOperationException("Standard streams are already read.");
        if (ChildProcessObject is null) throw new InvalidOperationException("Child process is not started.");
        if (!ChildProcessObject.HasExited) throw new InvalidOperationException("Child process is still running.");
        
        ChildProcessObject.StandardOutput.BaseStream.CopyTo(StdOutStream);
        ChildProcessObject.StandardOutput.Close();
        
        ChildProcessObject.StandardError.BaseStream.CopyTo(StdErrStream);
        ChildProcessObject.StandardError.Close();
        
        StdOutStream.Seek(0, SeekOrigin.Begin);
        StdErrStream.Seek(0, SeekOrigin.Begin);
        
        StreamsRead = true;
    }
}