using Caliburn.Micro;

namespace Sample_WP81.ViewModels
{
    public class TargetViewModel : PropertyChangedBase
    {
        public TargetViewModel()
        {
            Name = "Hi from Target view model";
        }

        public string Name { get; set; }
    }
}