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
			private set;
		}

        public abstract ToolLocation PreferredLocation { get; }
        public virtual double PreferredWidth
        {
            get { return 200; }
        }

        public virtual double PreferredHeight
        {
            get { return 200; }
        }

        #region IsVisible

        private bool _isVisible = true;
		public bool IsVisible
		{
			get { return _isVisible; }
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
