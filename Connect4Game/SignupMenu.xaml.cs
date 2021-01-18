using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Connect4Game
{
    public partial class SignupMenu
    {
        private readonly double _resWidth = SystemParameters.PrimaryScreenWidth;
        private readonly double _resHeight = SystemParameters.PrimaryScreenHeight;
        private readonly MainWindow _mW;
        private Thread _runSignup = new Thread(() => { });
        private static string _sqlConn;
        private readonly int _plrNo;

        private Dictionary<string, double[]> _buttonDictionary = new Dictionary<string, double[]>();

        public SignupMenu(MainWindow parent, int plrNo, string sqlCon)
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
                                SignupButton.IsEnabled = true;
                                UsernameBox.IsEnabled = true;
                                PasswordBox.IsEnabled = true;
                                PasswordBoxConfirm.IsEnabled = true;

                                ConnectionBar.Fill = Brushes.LimeGreen;
                                ConnectionBar.ToolTip = "Connection Status: Connected";
                            }));
                        } else {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                                SignupButton.IsEnabled = false;
                                UsernameBox.IsEnabled = false;
                                PasswordBox.IsEnabled = false;
                                PasswordBoxConfirm.IsEnabled = true;

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

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            string pwHash = null;
            string userId = "-1";

            if (PasswordBox.Password.Length < 8) {
                PrintError(2);
            } else if (UsernameBox.Text.Length == 0) {
                PrintError(1);
            } else if (!Regex.IsMatch(PasswordBox.Password, "[A-Za-z0-9][^A-Za-z0-9]")) {
                if (!Regex.IsMatch(PasswordBox.Password, "[A-Z]")) {
                    PrintError(6);
                } else if (!Regex.IsMatch(PasswordBox.Password, "[a-z]")) {
                    PrintError(5);
                } else if (!Regex.IsMatch(PasswordBox.Password, "[^a-zA-Z0-9]")) {
                    PrintError(3);
                } else if (!Regex.IsMatch(PasswordBox.Password, "[0-9]")) {
                    PrintError(4);
                }
            } else if (PasswordBox.Password != PasswordBoxConfirm.Password) {
                PrintError(7);
            } else {
                string username = UsernameBox.Text;
                Dispatcher.Invoke(() => {
                    SignupButton.IsEnabled = false;
                    PasswordBox.IsEnabled = false;
                    PasswordBoxConfirm.IsEnabled = false;
                    UsernameBox.IsEnabled = false;
                });
                _runSignup = new Thread(() => {
                    Thread pwThread = new Thread(() => pwHash = LoginAPI.GeneratePasswordHash(PasswordBox.Password));
                    pwThread.Start();
                    if (LoginAPI.DoesUsernameExist(username, _sqlConn, "SELECT Username FROM UserDB WHERE Username = ?user", "?user", "Username") == "0") {
                        pwThread.Join();
                        string signupOutput = LoginAPI.Signup(pwHash, username, _sqlConn, "INSERT INTO UserDB (Username, PasswordHash, SignupDate) VALUES (?user, ?pass, ?dt)", "?user", "?pass", "?dt");
                        if (signupOutput != "1") {
                            if (signupOutput.Contains("/")) { PrintError(int.Parse(signupOutput[1].ToString())); }
                            Dispatcher.Invoke(() => {
                                SignupButton.IsEnabled = true;
                                PasswordBox.IsEnabled = true;
                                PasswordBoxConfirm.IsEnabled = true;
                                UsernameBox.IsEnabled = true;
                            });
                            Thread.CurrentThread.Abort();
                        } else {
                            userId = LoginAPI.Login(pwHash, username, _sqlConn, "SELECT UserID FROM UserDB WHERE Username = ?user AND PasswordHash = ?pw", "?user", "?pw", "UserID");
                            if (userId.Contains("/")) {
                                PrintError(int.Parse(userId[1].ToString()));
                                Dispatcher.Invoke(() => {
                                    SignupButton.IsEnabled = true;
                                    PasswordBox.IsEnabled = true;
                                    PasswordBoxConfirm.IsEnabled = true;
                                    UsernameBox.IsEnabled = true;
                                });
                                Thread.CurrentThread.Abort();
                            } else {
                                string addOutput = LoginAPI.AddIdToAnotherTable(int.Parse(userId), _sqlConn, "INSERT INTO UserData (UserID) VALUES (?id)", "?id");
                                if (addOutput.Contains("/")) {
                                    PrintError(int.Parse(addOutput[1].ToString()));
                                    Dispatcher.Invoke(() => {
                                        SignupButton.IsEnabled = true;
                                        PasswordBox.IsEnabled = true;
                                        PasswordBoxConfirm.IsEnabled = true;
                                        UsernameBox.IsEnabled = true;
                                    });
                                    Thread.CurrentThread.Abort();
                                }
                            }
                        }
                    } else {
                        PrintError(8);
                        pwThread.Abort();
                        Dispatcher.Invoke(() => {
                            SignupButton.IsEnabled = true;
                            PasswordBox.IsEnabled = true;
                            PasswordBoxConfirm.IsEnabled = true;
                            UsernameBox.IsEnabled = true;
                        });
                        Thread.CurrentThread.Abort();
                    }

                    if (!userId.Contains("/"))
                    {
                        Thread newThread = new Thread(() => {
                            LoginDialog lD = new LoginDialog(1, username);
                            lD.Show();
                            Dispatcher.Run();
                        });
                        newThread.SetApartmentState(ApartmentState.STA);
                        newThread.Start();
                        MainWindow.PassUserId(int.Parse(userId), username, _plrNo, _mW);
                        Dispatcher.Invoke(() => QuitButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)));
                    }
                });
                _runSignup.Start();
            }
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                SignupButton.IsEnabled = false;
                PasswordBox.IsEnabled = false;
                PasswordBoxConfirm.IsEnabled = false;
                UsernameBox.IsEnabled = false;
                QuitButton.IsEnabled = false;
            });
            try { _runSignup.Abort(); } catch (Exception exception) { Console.WriteLine(exception); }

            DoubleAnimation animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            animation.Completed += SignupMenu_OnClosed;
            Window.BeginAnimation(OpacityProperty, animation);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mW.Show();
        }

        private void SignupMenu_OnClosed(object sender, EventArgs e)
        {
            Close();
        }

        private void PrintError(int errorId) {
            Dispatcher.Invoke(() => {
                OutputBox.Foreground = Brushes.Red;
                if (errorId == 0) {
                    OutputBox.Text = "Connection Failed... Please try again!";
                } else if (errorId == 1) {
                    OutputBox.Text = "Username Box must contain a Username!";
                } else if (errorId == 2) {
                    OutputBox.Text = "Password must contain at least 8 Characters!";
                } else if (errorId == 3) {
                    OutputBox.Text = "Password must contain a Special Character (Non-Alphanumerical Characters)!";
                } else if (errorId == 4) {
                    OutputBox.Text = "Password must contain a Number!";
                } else if (errorId == 5) {
                    OutputBox.Text = "Password must contain a Lower-Case Character!";
                } else if (errorId == 6) {
                    OutputBox.Text = "Password must contain a Capital/Upper-Case Character!";
                } else if (errorId == 7) {
                    OutputBox.Text = "Passwords do not match!";
                } else if (errorId == 8) {
                    OutputBox.Text = "Username already Exists!";
                } else if (errorId == 9) {
                    OutputBox.Text = "An Unknown Error has occured while logging in...";
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
                SignupButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
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
