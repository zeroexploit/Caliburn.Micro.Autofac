using Caliburn.Micro;

namespace Sample_WPF {
    using System.ComponentModel.Composition;

    [Export(typeof (IShell))]
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        public ShellViewModel()
        {
        }
    }
}
