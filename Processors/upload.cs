using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailParser
{
    public sealed class ImapAttachmentDownloader
    {
        public string? ResultDir => null;

        public ImapAttachmentDownloader(AppSettings settings, Action<string> log) { }

        public Task<string?> RunAsync(CancellationToken ct, Action<ScrapeProgress> reportProgress, Action<string> onDirCreated)
            => Task.FromResult<string?>(null);
    }
}
