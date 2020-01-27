using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;
using MiraUI.ViewModels;

namespace MiraUI
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
#if DEBUG
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
            PresentationTraceSources.ResourceDictionarySource.Switch.Level = SourceLevels.Critical;
#endif
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
