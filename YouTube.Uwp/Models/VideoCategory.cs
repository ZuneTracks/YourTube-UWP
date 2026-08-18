using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace YouTube.Uwp.Models
{
    public sealed class VideoCategory : INotifyPropertyChanged
    {
        private bool hasLoaded;
        private bool isExpanded;
        private string statusMessage;

        public VideoCategory()
        {
            Videos = new ObservableCollection<VideoSummary>();
        }

        public string Id { get; set; }

        public string Title { get; set; }

        public ObservableCollection<VideoSummary> Videos { get; private set; }

        public bool HasLoaded
        {
            get { return hasLoaded; }
            private set
            {
                if (hasLoaded == value)
                {
                    return;
                }

                hasLoaded = value;
                OnPropertyChanged("HasLoaded");
            }
        }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (isExpanded == value)
                {
                    return;
                }

                isExpanded = value;
                OnPropertyChanged("IsExpanded");
                OnPropertyChanged("ExpandedContentVisibility");
                OnPropertyChanged("ToggleGlyph");
            }
        }

        public Visibility ExpandedContentVisibility
        {
            get { return IsExpanded ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string ToggleGlyph
        {
            get { return IsExpanded ? "-" : "+"; }
        }

        public string StatusMessage
        {
            get { return statusMessage; }
            set
            {
                if (statusMessage == value)
                {
                    return;
                }

                statusMessage = value;
                OnPropertyChanged("StatusMessage");
            }
        }

        public void SetVideos(IEnumerable<VideoSummary> videos)
        {
            Videos.Clear();
            foreach (VideoSummary video in videos)
            {
                Videos.Add(video);
            }

            HasLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
