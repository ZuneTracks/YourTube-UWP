using System;
using System.Net;
using System.Text;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using YouTube.Uwp.Models;

namespace YouTube.Uwp.Services
{
    // Keeps tile rendering separate from the public-data request and the page UI.
    public sealed class TrendingTileService
    {
        public void Update(VideoSummary video, string regionCode)
        {
            if (video == null)
            {
                throw new ArgumentNullException("video");
            }

            if (string.IsNullOrWhiteSpace(video.Title))
            {
                throw new ArgumentException("A trending video title is required.", "video");
            }

            XmlDocument tileXml = new XmlDocument();
            tileXml.LoadXml(CreateTileXml(video, regionCode));

            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Clear();
            updater.Update(new TileNotification(tileXml));
        }

        private static string CreateTileXml(VideoSummary video, string regionCode)
        {
            string title = Encode(TrimForTile(video.Title, 96));
            string channel = Encode(TrimForTile(video.ChannelTitle, 48));
            string heading = Encode(CreateHeading(regionCode));
            string image = CreateImageElement(video.ThumbnailUrl);

            StringBuilder tile = new StringBuilder();
            tile.Append("<tile><visual>");
            AppendBinding(tile, "TileMedium", heading, title, channel, image);
            AppendBinding(tile, "TileWide", heading, title, channel, image);
            tile.Append("</visual></tile>");
            return tile.ToString();
        }

        private static void AppendBinding(StringBuilder tile, string template, string heading, string title, string channel, string image)
        {
            tile.Append("<binding template=\"").Append(template).Append("\" branding=\"name\">");
            tile.Append(image);
            tile.Append("<text hint-style=\"subtitle\">").Append(heading).Append("</text>");
            tile.Append("<text hint-style=\"captionSubtle\" hint-wrap=\"true\">").Append(title).Append("</text>");
            if (!string.IsNullOrWhiteSpace(channel))
            {
                tile.Append("<text hint-style=\"captionSubtle\">").Append(channel).Append("</text>");
            }

            tile.Append("</binding>");
        }

        private static string CreateImageElement(string thumbnailUrl)
        {
            Uri thumbnailUri;
            if (!Uri.TryCreate(thumbnailUrl, UriKind.Absolute, out thumbnailUri) ||
                (thumbnailUri.Scheme != "http" && thumbnailUri.Scheme != "https"))
            {
                return string.Empty;
            }

            return "<image placement=\"peek\" hint-overlay=\"28\" src=\"" + Encode(thumbnailUri.AbsoluteUri) + "\" />";
        }

        private static string CreateHeading(string regionCode)
        {
            string region = string.IsNullOrWhiteSpace(regionCode)
                ? string.Empty
                : regionCode.Trim().ToUpperInvariant();
            return string.IsNullOrWhiteSpace(region)
                ? "Trending now"
                : "Trending now (" + region + ")";
        }

        private static string TrimForTile(string value, int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string trimmed = value.Trim();
            return trimmed.Length <= maximumLength
                ? trimmed
                : trimmed.Substring(0, maximumLength - 3) + "...";
        }

        private static string Encode(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
