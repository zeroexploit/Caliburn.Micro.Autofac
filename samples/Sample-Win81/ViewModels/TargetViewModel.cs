namespace Caliburn.Micro.WinRT.Autofac.Sample.ViewModels
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