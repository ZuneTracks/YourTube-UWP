using System;

namespace YouTube.Uwp.Models
{
    public sealed class PlaylistSummary
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ThumbnailUrl { get; set; }

        public DateTimeOffset? PublishedAt { get; set; }

        public ulong VideoCount { get; set; }

        public string VideoCountLabel
        {
            get { return VideoCount.ToString("N0") + " videos"; }
        }
    }
}
