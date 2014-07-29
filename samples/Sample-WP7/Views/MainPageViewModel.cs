using Caliburn.Micro;

namespace Sample_WP7.Views {
    public class MainPageViewModel : Screen
    {
        private readonly INavigationService _navigationService;

        public MainPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            DisplayName = "Hello!";
        }

        public void GoToSecondPage()
        {
            var uri = _navigationService.UriFor<SecondViewModel>().BuildUri();
            _navigationService.Navigate(uri);
        }
    }
}
