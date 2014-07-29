using Caliburn.Micro;

namespace Sample_WP7
{
    public class TargetViewModel : PropertyChangedBase
    {
        private string _name;

        public TargetViewModel()
        {
            Name = "Hello from a target";
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }
    }
}