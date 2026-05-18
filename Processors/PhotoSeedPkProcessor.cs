using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailParser
{
    public sealed class PhotoSeedPkProcessor
    {
        public string? ResultDir => null;

        public PhotoSeedPkProcessor(AppSettings settings, Action<string> log) { }

        public Task RunAsync(CancellationToken ct, Action<ScrapeProgress> reportProgress)
            => Task.CompletedTask;
    }
}
