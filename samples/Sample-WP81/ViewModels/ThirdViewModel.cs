using System.Windows.Input;
using Caliburn.Micro;
using Sample_WP81.Infrastructure;

namespace Sample_WP81.ViewModels
{
    public class ThirdViewModel : Screen
    {
        public ThirdViewModel(INavigationService navigationService)
        {
            GoBackCommand = new ActionCommand(x => navigationService.GoBack(), x => navigationService.CanGoBack);
        }

        public ICommand GoBackCommand { get; set; }
    }
}