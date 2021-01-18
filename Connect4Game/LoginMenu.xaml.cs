using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;

namespace Connect4Game
{
    public partial class LoginMenu
    {
        private readonly double _resWidth = SystemParameters.PrimaryScreenWidth;
        private readonly double _resHeight = SystemParameters.PrimaryScreenHeight;
        private readonly MainWindow _mW;
        private Thread _runLogin = new Thread(() => { });
        private static string _sqlConn;
        private readonly int _plrNo;
        private readonly Tuple<int, int> _uId;
        private Dictionary<string, double[]> _buttonDictionary = new Dictionary<string, double[]>();

        public LoginMenu(MainWindow parent, int plrNo, Tuple<int, int> uIDs, string sqlCon)
        {
            _mW = parent;
            _plrNo = plrNo;
            _sqlConn = sqlCon;
            InitializeComponent();
            if (Application.Current.MainWindow != null) {
                this.Width = Width * (_resWidth / 1920);
                this.Height = Height * (_resHeight / 1080);
                this.Left = (_resWidth - this.Width) / 2 + SystemParameters.WorkArea.Left;
                this.Top = (_resHeight - this.Height) / 2 + SystemParameters.WorkArea.Top;
            }

            _uId = uIDs;

            InternetCheck();
            CheckDbExists();
        }

        private static void CheckDbExists()
        {
            string doesExist = LoginAPI.DoesTableExist(_sqlConn, "SHOW TABLES LIKE 'UserDB';");
            if (doesExist == "0") {
                LoginAPI.CreateTable(_sqlConn, "CREATE TABLE IF NOT EXISTS sql2367778.UserDB (UserID INT AUTO_INCREMENT PRIMARY KEY, Username VARCHAR(64) NOT NULL, PasswordHash VARCHAR(128) NOT NULL, SignupDate DATETIME NOT NULL)");
            }

            doesExist = LoginAPI.DoesTableExist(_sqlConn, "SHOW TABLES LIKE 'UserData';");
            if (doesExist == "0") {
                LoginAPI.CreateTable(_sqlConn, "CREATE TABLE IF NOT EXISTS sql2367778.UserData (UserID int, O_Wins int DEFAULT 0 NOT NULL, O_Loses int DEFAULT 0 NOT NULL, O_GamesPlayed int DEFAULT 0 NOT NULL, O_TimesPlayedAsRed int DEFAULT 0 NOT NULL, O_TimesPlayedAsYellow int DEFAULT 0 NOT NULL, FOREIGN KEY (UserID) REFERENCES sql2367778.UserDB(UserID))");
            }
        }

        private void InternetCheck()
        {
            new Thread(() => {
                while (true) {
                    try {
                        if (InternetAPI.CheckForInternetConnection()) {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                                LoginButton.IsEnabled = true;
                                UsernameBox.IsEnabled = true;
                                PasswordBox.IsEnabled = true;

                                ConnectionBar.Fill = Brushes.LimeGreen;
                                ConnectionBar.ToolTip = "Connection Status: Connected";
                            }));
                        } else {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                                LoginButton.IsEnabled = false;
                                UsernameBox.IsEnabled = false;
                                PasswordBox.IsEnabled = false;

                                ConnectionBar.Fill = Brushes.Red;
                                ConnectionBar.ToolTip = "Connection Status: Not Connected";
                            }));
                        }
                    } catch (Exception e) { Console.WriteLine(e.ToString()); }
                    Thread.Sleep(5000);
                }
                // ReSharper disable once FunctionNeverReturns
            }).Start();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            try { _runLogin.Abort(); } catch (Exception exception) { Console.WriteLine(exception); }

            DoubleAnimation animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            animation.Completed += LoginMenu_OnClosed;
            Window.BeginAnimation(OpacityProperty, animation);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mW.Show();
        }

        private void LoginMenu_OnClosed(object sender, EventArgs e)
        {
            Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string pwHash = null;
            string userId = "-1";

            if (PasswordBox.Password.Length < 8) {
                PrintError(2);
            } else if (UsernameBox.Text.Length == 0) {
                PrintError(1);
            } else {
                string username = UsernameBox.Text;
                LoginButton.IsEnabled = false;
                PasswordBox.IsEnabled = false;
                UsernameBox.IsEnabled = false;
                _runLogin = new Thread(() => {
                    Thread pwThread = new Thread(() => pwHash = LoginAPI.GeneratePasswordHash(PasswordBox.Password));
                    pwThread.Start();
                    string userExistsString = LoginAPI.DoesUsernameExist(username, _sqlConn, "SELECT Username FROM UserDB WHERE Username = ?user", "?user", "Username");
                    if (userExistsString == "1") {
                        pwThread.Join();
                        string pwCorrectString = LoginAPI.IsPasswordCorrect(pwHash, _sqlConn, "SELECT PasswordHash FROM UserDB WHERE PasswordHash = ?pass", "?pass", "PasswordHash");
                        if (pwCorrectString == "1") {
                            userId = LoginAPI.Login(pwHash, username, _sqlConn, "SELECT UserID FROM UserDB WHERE Username = ?user AND PasswordHash = ?pw", "?user", "?pw", "UserID");

                            if (userId.Contains("/")) { PrintError(int.Parse(userId[1].ToString())); }
                        } else {
                            if (pwCorrectString.Contains("/")) { PrintError(int.Parse(pwCorrectString[1].ToString())); }
                            Dispatcher.Invoke(() => {
                                LoginButton.IsEnabled = true;
                                PasswordBox.IsEnabled = true;
                                UsernameBox.IsEnabled = true;
                            });
                            Thread.CurrentThread.Abort();
                        }
                    } else {
                        PrintError(3);
                        pwThread.Abort();
                        Dispatcher.Invoke(() => {
                            LoginButton.IsEnabled = true;
                            PasswordBox.IsEnabled = true;
                            UsernameBox.IsEnabled = true;
                        });
                        Thread.CurrentThread.Abort();
                    }

                    if (_uId.Item1 == int.Parse(userId) || _uId.Item2 == int.Parse(userId)) {
                        PrintError(4);
                        pwThread.Abort();
                        Dispatcher.Invoke(() => {
                            LoginButton.IsEnabled = true;
                            PasswordBox.IsEnabled = true;
                            UsernameBox.IsEnabled = true;
                        });
                        Thread.CurrentThread.Abort();
                    }

                    if (!userId.Contains("/")) {
                        Thread newThread = new Thread(() => {
                            LoginDialog lD = new LoginDialog(0, username);
                            lD.Show();
                            Dispatcher.Run();
                        });
                        newThread.SetApartmentState(ApartmentState.STA);
                        newThread.Start();
                        MainWindow.PassUserId(int.Parse(userId), username, _plrNo, _mW);
                        Dispatcher.Invoke(() => QuitButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)));
                    }
                });
                _runLogin.Start();
            }
        }

        private void PrintError(int errorId) {
            Dispatcher.Invoke(() => {
                OutputBox.Foreground = Brushes.Red;
                switch (errorId) {
                    case 0:
                        OutputBox.Text = "Connection Failed... Please try again.";
                        break;
                    case 1:
                        OutputBox.Text = "Username Box must contain a Username!";
                        break;
                    case 2:
                        OutputBox.Text = "Password Box must contain a Password longer than 8 Characters!";
                        break;
                    case 3:
                        OutputBox.Text = "Username and/or Password is Incorrect!";
                        break;
                    case 4:
                        OutputBox.Text = "User is already logged in!";
                        break;
                    case 9:
                        OutputBox.Text = "An Unknown Error has occured while logging in...";
                        break;
                }
                CleanupOutput();
            });
        }

        private void CleanupOutput() {
            new Thread(() => {
                for (int i = 0; i < 100; i++) {
                    Thread.Sleep(50);
                    string text = "";
                    Dispatcher.Invoke(() => text = OutputBox.Text);
                    if (text?.Length == 0) { Thread.CurrentThread.Abort(); }
                }
                Dispatcher.Invoke(() => OutputBox.Text = "");
            }).Start();
        }

        private void EnterKey_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                LoginButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }

        private void Button_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
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
    }
}
