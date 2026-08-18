namespace YouTube.Uwp.Models
{
    public sealed class VideoDetails : VideoSummary
    {
        public string Duration { get; set; }

        public ulong ViewCount { get; set; }

        public ulong LikeCount { get; set; }

        public ulong CommentCount { get; set; }

        public string PrivacyStatus { get; set; }

        public bool Embeddable { get; set; }
    }
}
