using System;

namespace YouTube.Uwp.Models
{
    // Kept platform-independent so it can replace the WP8 ResultItem-based model safely.
    public class VideoSummary
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ChannelId { get; set; }

        public string ChannelTitle { get; set; }

        public string ThumbnailUrl { get; set; }

        public DateTimeOffset? PublishedAt { get; set; }
    }
}
