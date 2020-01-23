using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiraUI.Docker
{
    public enum ToolLocation
    {
        Left,
        Right,
        Bottom,
        Top
    }

    public class PaneViewModel : Screen
    {
        public PaneViewModel()
        { }


        #region Title

        private string _title = null;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    NotifyOfPropertyChange(() => Title);
                }
            }
        }

        #endregion

        #region ContentId

        private string _contentId = null;
        public string ContentId
        {
            get { return _contentId; }
            set
            {
                if (_contentId != value)
                {
                    _contentId = value;
                    NotifyOfPropertyChange(() => ContentId);
                }
            }
        }

        #endregion

        #region IsSelected

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyOfPropertyChange(() => IsSelected);
                }
            }
        }

        #endregion

        #region IsActive

        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    NotifyOfPropertyChange(() => IsActive);
                }
            }
        }
        #endregion
    }
}
