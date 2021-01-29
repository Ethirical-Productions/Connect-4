using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Connect4Game
{
    public partial class MainWindow : Window
    {
        private readonly double _resWidth = SystemParameters.PrimaryScreenWidth;
        private readonly double _resHeight = SystemParameters.PrimaryScreenHeight;
        private readonly string _sqlConn;
        private readonly string[] _sqlStringNames = { "O_Wins", "O_Loses", "O_GamesPlayed", "O_TimesPlayedAsRed", "O_TimesPlayedAsYellow" };

        private Tuple<int, int> _uId = new Tuple<int, int>(-1, -1);
        private Dictionary<string, double[]> _buttonDictionary = new Dictionary<string, double[]>();

        public MainWindow()
        {
            InitializeComponent();

            string sqlC;
            using (Stream embeddedFile = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Connect4Game..env"))
            using (StreamReader reader = new StreamReader(embeddedFile)) {
                sqlC = reader.ReadToEnd();
            }

            if (sqlC.Contains("SQL_CONNECT_STRING")) {
                sqlC = sqlC.Remove(0, "SQL_CONNECT_STRING=".Length);
                _sqlConn = sqlC;
            } else {
                throw new Exception("No SQL Connection String");
            }

            if (Application.Current.MainWindow != null) {
                Width *= _resWidth / 1920;
                Height *= _resHeight / 1080;
                Left = (_resWidth - Width) / 2 + SystemParameters.WorkArea.Left;
                Top = (_resHeight - Height) / 2 + SystemParameters.WorkArea.Top;
            }
            InternetCheck();
        }

        private void InternetCheck()
        {
            new Thread(() => {
                while (true) {
                    try {
                        if (InternetAPI.CheckForInternetConnection()) {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                                ConnectionBar.Fill = Brushes.LimeGreen;
                                ConnectionBar.ToolTip = "Connection Status: Connected";
                                Login_Button1.IsEnabled = true;
                                Signup_Button1.IsEnabled = true;
                                Login_Button2.IsEnabled = true;
                                Signup_Button2.IsEnabled = true;
                                if (_uId.Item1 != -1 && _uId.Item2 != -1) {
                                    OMP_Button.IsEnabled = true;
                                    SwitchColoursButton.IsEnabled = true;
                                }
                            }));
                        } else {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                                ConnectionBar.Fill = Brushes.Red;
                                ConnectionBar.ToolTip = "Connection Status: Not Connected";
                                Signup_Button1.IsEnabled = false;
                                Login_Button1.IsEnabled = false;
                                Signup_Button2.IsEnabled = false;
                                Login_Button2.IsEnabled = false;
                                OMP_Button.IsEnabled = false;
                                SwitchColoursButton.IsEnabled = false;
                            }));
                        }
                    } catch (Exception e) { Console.WriteLine(e.ToString()); }
                    Thread.Sleep(1000);
                }
            }).Start();
        }

        public static void PassUserId(int userId, string user, int plrNo, MainWindow mW)
        {
            //MessageBox.Show(userId.ToString());
            mW.UpdateLoginUI(userId, user, plrNo, mW);
        }

        private void UpdateLoginUI(int userId, string user, int playerNo, MainWindow mW)
        {
            Dispatcher.Invoke(() => {
                switch (playerNo) {
                    case 1:
                        mW.LoginSignupCanvas1.Visibility = Visibility.Hidden;
                        mW.UserDataCanvas1.Visibility = Visibility.Visible;

                        mW.UsernameText1.Text = user;
                        mW._uId = new Tuple<int, int>(userId, _uId.Item2);
                        mW.UserIDText1.Text = "User ID: " + userId;
                        new Thread(() =>
                        {
                            string[] stats = LoginAPI.GetPlayerStats(userId.ToString(), _sqlConn,
                                "SELECT * FROM UserData WHERE UserID = ?uID", "?uID", _sqlStringNames);
                            if (int.Parse(stats[0]) > 0) {
                                Dispatcher.Invoke(() => {
                                    mW.StatisticsButton1.IsEnabled = true;
                                    mW.StatisticsButton1.ToolTip = "Open Player Statistics";
                                    mW.StatisticsButton1.Click += LoadStatisticsWindow;
                                });
                            } else {
                                Dispatcher.Invoke(() => {
                                    mW.StatisticsButton1.IsEnabled = true;
                                    mW.StatisticsButton1.ToolTip = "Play A Game to Unlock Statistics";
                                    mW.StatisticsButton1.Click -= LoadStatisticsWindow;
                                });
                            }
                        }).Start();
                        break;
                    case 2:
                        mW.LoginSignupCanvas2.Visibility = Visibility.Hidden;
                        mW.UserDataCanvas2.Visibility = Visibility.Visible;

                        mW.UsernameText2.Text = user;
                        mW._uId = new Tuple<int, int>(_uId.Item1, userId);
                        mW.UserIDText2.Text = "User ID: " + userId;
                        new Thread(() => {
                            string[] stats = LoginAPI.GetPlayerStats(userId.ToString(), _sqlConn,
                                "SELECT * FROM UserData WHERE UserID = ?uID", "?uID", _sqlStringNames);
                            if (int.Parse(stats[0]) > 0) {
                                Dispatcher.Invoke(() => {
                                    mW.StatisticsButton2.IsEnabled = true;
                                    mW.StatisticsButton2.ToolTip = "Open Player Statistics";
                                    mW.StatisticsButton2.Click += LoadStatisticsWindow;
                                });
                            } else {
                                Dispatcher.Invoke(() => {
                                    mW.StatisticsButton2.IsEnabled = true;
                                    mW.StatisticsButton2.ToolTip = "Play A Game to Unlock Statistics";
                                    mW.StatisticsButton2.Click -= LoadStatisticsWindow;
                                });
                            }
                        }).Start();
                        break;
                }

                if (_uId.Item1 != -1 && _uId.Item2 != -1) {
                    OMP_Button.IsEnabled = true;
                    SwitchColoursButton.IsEnabled = true;
                }
            });
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            DoubleAnimation animation = new DoubleAnimation() {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };
            animation.Completed += Window_Closed;
            Window.BeginAnimation(OpacityProperty, animation);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Window.Hide();
            Environment.Exit(0);
        }

        private void LMP_Button_OnClick(object sender, RoutedEventArgs e)
        {
            LocalMultiplayerGameWindow lMGW = new LocalMultiplayerGameWindow(this);
            lMGW.Show();

            MainWindow mW = this;
            mW.Hide();
        }

        private void OMP_Button_OnClick(object sender, RoutedEventArgs e)
        {
            LocalMultiplayerLoggedInWindow lMLIW = new LocalMultiplayerLoggedInWindow(this, _uId, _sqlConn);
            lMLIW.Show();

            MainWindow mW = this;
            mW.Hide();
        }

        private void Login_Button_OnClick(object sender, RoutedEventArgs e)
        {
            int plrNo = 0;

            Button uiSender = (Button)sender;
            switch (uiSender.Name) {
                case "Login_Button1":
                    plrNo = 1;
                    break;
                case "Login_Button2":
                    plrNo = 2;
                    break;
            }

            LoginMenu lM = new LoginMenu(this, plrNo, _uId, _sqlConn);
            lM.Show();

            MainWindow mW = this;
            mW.Hide();
        }

        private void Signup_Button_OnClick(object sender, RoutedEventArgs e)
        {
            int plrNo = 0;

            Button uiSender = (Button)sender;
            switch (uiSender.Name) {
                case "Signup_Button1":
                    plrNo = 1;
                    break;
                case "Signup_Button2":
                    plrNo = 2;
                    break;
            }

            SignupMenu sM = new SignupMenu(this, plrNo, _sqlConn);
            sM.Show();

            MainWindow mW = this;
            mW.Hide();
        }

        private void QuitButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            Window.WindowState = WindowState.Minimized;
        }

        private void SignoutButton_OnClick(object sender, RoutedEventArgs e)
        {
            Button uiSender = (Button) sender;
            switch (uiSender.Name) {
                case "SignoutButton1":
                    LoginSignupCanvas1.Visibility = Visibility.Visible;
                    UserDataCanvas1.Visibility = Visibility.Hidden;
                    UsernameText1.Text = "DefaultUsername";
                    _uId = new Tuple<int, int>(-1, _uId.Item2);
                    UserIDText1.Text = "User ID: -1";
                    break;
                case "SignoutButton2":
                    LoginSignupCanvas2.Visibility = Visibility.Visible;
                    UserDataCanvas2.Visibility = Visibility.Hidden;
                    UsernameText2.Text = "DefaultUsername";
                    _uId = new Tuple<int, int>(_uId.Item1, -1);
                    UserIDText2.Text = "User ID: -1";
                    break;
            }

            if (_uId.Item1 == -1 || _uId.Item2 == -1) {
                    OMP_Button.IsEnabled = false;
                    SwitchColoursButton.IsEnabled = false;
            }
        }

        private void SwitchColorButton_Click(object sender, RoutedEventArgs e)
        {
            _uId = new Tuple<int, int>(_uId.Item2, _uId.Item1);

            new Thread(() => {
                string output1 = LoginAPI.GetUsernameFromUserID(_uId.Item1.ToString(), _sqlConn,
                    "SELECT Username FROM UserDB Where UserID = ?id", "?id", "Username");

                string output2 = LoginAPI.GetUsernameFromUserID(_uId.Item2.ToString(), _sqlConn,
                    "SELECT Username FROM UserDB Where UserID = ?id", "?id", "Username");

                Dispatcher.Invoke(() => {
                    UsernameText1.Text = output1;
                    UserIDText1.Text = "User ID: " + _uId.Item1;

                    UsernameText2.Text = output2;
                    UserIDText2.Text = "User ID: " + _uId.Item2;
                });
            }).Start();
        }

        private void Button_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Button button = (Button) sender;
            SharedUICode uiEvents = new SharedUICode();
            uiEvents.DoEventExpand(button, ref _buttonDictionary);
        }

        private void Button_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Button button = (Button)sender;
            SharedUICode uiEvents = new SharedUICode();
            uiEvents.DoEventExpand(button, ref _buttonDictionary);
        }

        private void Button_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            SharedUICode uiEvents = new SharedUICode();
            uiEvents.DoEventShrink(button, ref _buttonDictionary);
        }

        private void Button_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Button button = (Button)sender;
            SharedUICode uiEvents = new SharedUICode();
            uiEvents.DoEventShrink(button, ref _buttonDictionary);
        }

        private void LoadStatisticsWindow(object sender, RoutedEventArgs e)
        {
            Button sendButton = (Button) sender;

            int uId = 0;

            if (sendButton.Name == "StatisticsButton1") {
                uId = _uId.Item1;
            } else if (sendButton.Name == "StatisticsButton2") {
                uId = _uId.Item2;
            }

            ShowStatistics sS = new ShowStatistics(this, uId, _sqlConn);
            sS.Show();
            MainWindow mW = this;
            mW.Hide();
        }
    }
}