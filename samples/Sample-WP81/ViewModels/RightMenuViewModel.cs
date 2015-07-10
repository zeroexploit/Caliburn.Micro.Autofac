using Caliburn.Micro;
using Sample_WP81.Events;

namespace Sample_WP81.ViewModels
{
    public class RightMenuViewModel : Screen
    {
        private readonly IEventAggregator _messenger;

        public RightMenuViewModel(IEventAggregator eventAggregator)
        {
            _messenger = eventAggregator;
            _messenger.Subscribe(this);
        }

        private string _testMessage;
        public string TestMessage
        {
            get { return _testMessage; }
            set
            {
                _testMessage = value;
                NotifyOfPropertyChange(() => TestMessage);
            }
        }

        public void ShowTestMessage()
        {
            TestMessage = "Test Message";
            _messenger.Publish(new ClearListEvent(), x => x());
        }
    }
}