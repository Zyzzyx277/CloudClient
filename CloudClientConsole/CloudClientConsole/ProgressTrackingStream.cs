namespace CloudClientConsole;

using System;
using System.IO;

public class ProgressTrackingStream : Stream
{
    private Stream _baseStream;
    private long _totalBytesRead;
    private long _totalBytesWritten;
    private long _totalLength;

    public ProgressTrackingStream(Stream baseStream)
    {
        _baseStream = baseStream;
        _totalLength = baseStream.Length;
        Console.WriteLine($"Size: {_baseStream.Length / (1024 * 1024)} MB");
    }

    public override bool CanRead => _baseStream.CanRead;
    public override bool CanWrite => _baseStream.CanWrite;
    public override bool CanSeek => _baseStream.CanSeek;

    public override long Length => _totalLength;

    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    public override void Flush()
    {
        _baseStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesRead = _baseStream.Read(buffer, offset, count);
        _totalBytesRead += bytesRead;

        // Print read progress here (e.g., percentage completed)
        double progressPercentage = (_totalBytesRead / (double)_totalLength) * 100;
        Console.Write($"Read Progress: {progressPercentage:F2}%\r");

        return bytesRead;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _baseStream.Write(buffer, offset, count);
        _totalBytesWritten += count;

        // Print write progress here (e.g., percentage completed)
        double progressPercentage = (_totalBytesWritten / (double)_totalLength) * 100;
        Console.Write($"Write Progress: {progressPercentage:F2}%\r");
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _baseStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _baseStream.SetLength(value);
        _totalLength = value;
    }

    protected override void Dispose(bool disposing)
    {
        _baseStream.Dispose();
        base.Dispose(disposing);
    }
}
