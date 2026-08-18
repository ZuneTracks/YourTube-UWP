using System;

namespace YouTube.Uwp.Models
{
    public sealed class ChannelDetails
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string CustomUrl { get; set; }

        public string ThumbnailUrl { get; set; }

        public DateTimeOffset? PublishedAt { get; set; }

        public string UploadsPlaylistId { get; set; }

        public ulong ViewCount { get; set; }

        public ulong SubscriberCount { get; set; }

        public ulong VideoCount { get; set; }
    }
}
