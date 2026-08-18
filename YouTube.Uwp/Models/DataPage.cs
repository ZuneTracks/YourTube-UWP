using System.Collections.Generic;

namespace YouTube.Uwp.Models
{
    public sealed class DataPage<T>
    {
        public DataPage(IReadOnlyList<T> items, string nextPageToken)
        {
            Items = items;
            NextPageToken = nextPageToken;
        }

        public IReadOnlyList<T> Items { get; private set; }

        public string NextPageToken { get; private set; }
    }
}
