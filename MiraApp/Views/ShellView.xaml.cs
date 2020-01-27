using System;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;

namespace MiraUI.Views
{
    /// <summary>
    ///     Logika interakcji dla klasy ShellView.xaml
    /// </summary>
    public partial class ShellView : MetroWindow
    {
        public ShellView()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/Images/icon.png"));
        }
    }
}
