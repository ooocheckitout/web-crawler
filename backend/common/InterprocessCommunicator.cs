using System.IO.Pipes;
using Microsoft.Extensions.Logging;

namespace common;

public class InterprocessCommunicator
{
    readonly ILogger<InterprocessCommunicator> _logger;
    AnonymousPipeServerStream? _writerPipe;
    AnonymousPipeClientStream? _readerPipe;

    public InterprocessCommunicator(ILogger<InterprocessCommunicator> logger)
    {
        _logger = logger;
    }

    public string InitializeWriter()
    {
        _writerPipe ??= new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
        string clientHandleAsString = _writerPipe.GetClientHandleAsString();
        _logger.LogInformation("Writer initialized with handle {handle}", clientHandleAsString);
        return clientHandleAsString;
    }

    public void InitializeReader(string handleString)
    {
        _readerPipe ??= new AnonymousPipeClientStream(PipeDirection.In, handleString);
        _logger.LogInformation("Reader initialized using handle {handle}", handleString);
    }


    public void Write(string message)
    {
        if (_writerPipe is null) throw new Exception("Writer was not initialized.");

        using var stream = new StreamWriter(_writerPipe) { AutoFlush = true };
        stream.WriteLine(message);
        _logger.LogInformation("Write message {message}", message);
    }

    public string Read()
    {
        if (_readerPipe is null) throw new Exception("Read was not initialized.");

        using var stream = new StreamReader(_readerPipe);

        string? message = null;
        while (message == null)
            message = stream.ReadLine();

        _logger.LogInformation("Read message {message}", message);
        return message;
    }
}
