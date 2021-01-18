using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Connect4Game
{
    public partial class LoginDialog
    {
        private readonly double _resWidth = SystemParameters.MaximizedPrimaryScreenWidth;
        private readonly double _resHeight = SystemParameters.MaximizedPrimaryScreenHeight;

        public LoginDialog(int ls, string user)
        {
            InitializeComponent();
            MakeGuiVisible(ls, user);

            this.Width = Width * (_resWidth / 1920);
            this.Height = Height * (_resHeight / 1080);
            this.Left = (_resWidth - this.Width) / 2 + SystemParameters.WorkArea.Left;
            this.Top = (_resHeight - this.Height) / 2 + SystemParameters.WorkArea.Top;
        }

        private void MakeGuiVisible(int ls, string user)
        {
            if (ls == 0) {
                LC_Text.Text = user + " has successfully logged in. \n Welcome to Connect 4!";
                LoginCanvas.Visibility = Visibility.Visible;
            } else {
                SC_Text.Text = user + " has successfully signed up. \n Welcome to Connect 4!";
                SignupCanvas.Visibility = Visibility.Visible;
            }
        }

        private void QuitButton_OnClick(object sender, RoutedEventArgs e)
        {
            QuitButton.IsEnabled = false;

            DoubleAnimation animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            animation.Completed += LoginDialog_OnClosed;
            Window.BeginAnimation(OpacityProperty, animation);
        }

        private void LoginDialog_OnClosed(object sender, EventArgs e)
        {
            Close();
        }
    }
}
