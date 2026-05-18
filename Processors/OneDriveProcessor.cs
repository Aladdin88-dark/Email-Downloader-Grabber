using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailParser
{
    public sealed class OneDriveProcessor
    {
        public string? ResultDir => null;

        public OneDriveProcessor(AppSettings settings, Action<string> log) { }

        public Task RunAsync(CancellationToken ct, Action<ScrapeProgress> reportProgress)
            => Task.CompletedTask;
    }
}
