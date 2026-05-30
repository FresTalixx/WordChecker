using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WordCheckerWPF.ViewModels
{
    public class DriveProgressViewModel : INotifyPropertyChanged
    {
        private string driveName = string.Empty;
        private double progress;
        private string status = string.Empty;

        public string DriveName
        {
            get => driveName;
            set
            {
                if (driveName != value)
                {
                    driveName = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Progress
        {
            get => progress;
            set
            {
                if (progress != value)
                {
                    progress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Status
        {
            get => status;
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
