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
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
            System.Diagnostics.PresentationTraceSources.ResourceDictionarySource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
#endif
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
