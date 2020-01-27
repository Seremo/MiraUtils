using MiraUI.Docker;

namespace MiraUI.ViewModels
{
    public abstract class ToolViewModel : PaneViewModel
    {
        public ToolViewModel(string name)
        {
            Name = name;
            Title = name;
        }

        public string Name
        {
            get;
        }

        public abstract ToolLocation PreferredLocation { get; }
        public virtual double PreferredWidth => 200;

        public virtual double PreferredHeight => 200;

        #region IsVisible

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    NotifyOfPropertyChange(() => IsVisible);
                }
            }
        }

        #endregion

    }
}
