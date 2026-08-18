using System.ComponentModel;
using Windows.UI.Xaml;

namespace YouTube.Uwp.Models
{
    public sealed class PivotHeader : INotifyPropertyChanged
    {
        private bool isSelected;

        public string Label { get; set; }

        public double Width { get; set; }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected == value)
                {
                    return;
                }

                isSelected = value;
                OnPropertyChanged("IsSelected");
                OnPropertyChanged("UnderlineVisibility");
            }
        }

        public Visibility UnderlineVisibility
        {
            get { return IsSelected ? Visibility.Visible : Visibility.Collapsed; }
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
