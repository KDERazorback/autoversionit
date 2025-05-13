using Microsoft.Extensions.Logging;

namespace AutoVersionIt.Interop;

public class SimpleShellProcessFactory : IChildProcessFactory
{
    public ILoggerFactory? LoggerFactory { get; set; }

    public SimpleShellProcessFactory(ILoggerFactory? loggerFactory = null)
    {
        LoggerFactory = loggerFactory;
    }
    
    public IChildProcess Create()
    {
        var logger = LoggerFactory?.CreateLogger<ChildProcess>();
        var process = new ChildProcess(logger);
        
        return process;
    }
}