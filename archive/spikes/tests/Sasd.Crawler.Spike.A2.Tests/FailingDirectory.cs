using Lucene.Net.Store;

namespace Sasd.Crawler.Spike.A2.Tests;

internal sealed class FailingDirectory(Lucene.Net.Store.Directory @delegate) : FilterDirectory(@delegate)
{
    private long remainingBytes = long.MaxValue;

    public void FailAfter(long bytes)
    {
        if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes));
        Interlocked.Exchange(ref remainingBytes, bytes);
    }

    public override IndexOutput CreateOutput(string name, IOContext context) =>
        new FailingIndexOutput(base.CreateOutput(name, context), this);

    private void Consume(int bytes)
    {
        var remaining = Interlocked.Add(ref remainingBytes, -bytes);
        if (remaining < 0) throw new IOException("Simulated disk-full write failure.");
    }

    private sealed class FailingIndexOutput(IndexOutput inner, FailingDirectory owner) : IndexOutput
    {
        public override long Position => inner.Position;
        public override long Checksum => inner.Checksum;
        public override void Flush() => inner.Flush();
        [Obsolete("Lucene 4.8 compatibility override.")]
        public override void Seek(long position) => inner.Seek(position);
        public override void WriteByte(byte value) { owner.Consume(1); inner.WriteByte(value); }
        public override void WriteBytes(byte[] buffer, int offset, int length) { owner.Consume(length); inner.WriteBytes(buffer, offset, length); }
        protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); }
    }
}
