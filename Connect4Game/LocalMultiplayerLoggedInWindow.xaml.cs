using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Connect4Game
{
    public partial class LocalMultiplayerLoggedInWindow : Window
    {
        private readonly MainWindow _mW;
        private int _playerTurn = 0;
        private readonly double _resWidth = SystemParameters.PrimaryScreenWidth;
        private readonly double _resHeight = SystemParameters.PrimaryScreenHeight;
        private static string _sqlConn;
        private readonly string[] _sqlStringNames = { "O_Wins", "O_Loses", "O_GamesPlayed", "O_TimesPlayedAsRed", "O_TimesPlayedAsYellow" };
        private string[] _plr1Stats = { "-1", "-1", "-1", "-1", "-1"};
        private string[] _plr2Stats = { "-1", "-1", "-1", "-1", "-1" };

        private readonly Tuple<int, int> _uIDs;
        private Tuple<string, string> _usernames = new Tuple<string, string>("", "");

        private delegate void WinEvent(int plrwon);
        private WinEvent _winEvent;

        private int[,] _c4Board = new int[7, 7];

        public LocalMultiplayerLoggedInWindow(MainWindow mW, Tuple<int, int> pIDs, string sqlCon)
        {
            _uIDs = pIDs;
            _mW = mW;
            _sqlConn = sqlCon;
            InitializeComponent();
            if (Application.Current.MainWindow != null) {
                Width *= _resWidth / 1920;
                Height *= _resHeight / 1080;
                Left = ((_resWidth - Width) / 2) + SystemParameters.WorkArea.Left;
                Top = ((_resHeight - Height) / 2) + SystemParameters.WorkArea.Top;
            }
            InitBoard();
            DisableHitboxes();
            GetUsernames();
            InternetCheck();

            _winEvent += WinOutput;
        }
        private Dictionary<string, double[]> _buttonDictionary = new Dictionary<string, double[]>();

        private void GetUsernames()
        {
            LoadingPopup.Visibility = Visibility.Visible;

            new Thread(() => {
                string output = LoginAPI.GetUsernameFromUserID(_uIDs.Item1.ToString(), _sqlConn,
                    "SELECT Username FROM UserDB Where UserID = ?id", "?id", "Username");
                if (!output.Contains("/")) {
                    _usernames = new Tuple<string, string>(output, _usernames.Item2);
                } else {
                    Dispatcher.Invoke(() => PrintError(int.Parse(output.Split('/')[0])));
                    return;
                }

                output = LoginAPI.GetUsernameFromUserID(_uIDs.Item2.ToString(), _sqlConn,
                    "SELECT Username FROM UserDB Where UserID = ?id", "?id", "Username");
                if (!output.Contains("/")) {
                    _usernames = new Tuple<string, string>(_usernames.Item1, output);
                } else {
                    Dispatcher.Invoke(() => PrintError(int.Parse(output.Split('/')[0])));
                    return;
                }

                _plr1Stats = LoginAPI.GetPlayerStats(_uIDs.Item1.ToString(), _sqlConn,
                    "SELECT * FROM UserData WHERE UserID = ?uID", "?uID", _sqlStringNames);
                if (_plr1Stats[0].Contains("/")) {
                    Dispatcher.Invoke(() => PrintError(int.Parse(_plr1Stats[0].Split('/')[0])));
                    return;
                }

                _plr2Stats = LoginAPI.GetPlayerStats(_uIDs.Item2.ToString(), _sqlConn,
                    "SELECT * FROM UserData WHERE UserID = ?uID", "?uID", _sqlStringNames);
                if (_plr2Stats[0].Contains("/")) {
                    Dispatcher.Invoke(() => PrintError(int.Parse(_plr2Stats[0].Split('/')[0])));
                    return;
                }

                Dispatcher.Invoke(() => {
                    EnableHitboxes();
                    LoadingPopup.Visibility = Visibility.Hidden;
                    Player1NameLabel.Text = _usernames.Item1;
                    Player2NameLabel.Text = _usernames.Item2;
                });
                Thread.Sleep(1000);
            }).Start();
        }

        private void InternetCheck()
        {
            new Thread(() => {
                while (true) {
                    try {
                        if (InternetAPI.CheckForInternetConnection()) {
                            Dispatcher.Invoke(() => {
                                if (NoConnectionPopup.Visibility == Visibility.Visible) {
                                    EnableHitboxes();
                                    NoConnectionPopup.Visibility = Visibility.Hidden;
                                }
                            });
                        } else {
                            Dispatcher.Invoke(() => {
                                if (NoConnectionPopup.Visibility == Visibility.Hidden) {
                                    DisableHitboxes();
                                    NoConnectionPopup.Visibility = Visibility.Visible;
                                }
                            });
                        }
                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                    Thread.Sleep(1000);
                }
            }).Start();
        }

        private void InitBoard()
        {
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    _c4Board[i, j] = -1;
                }
            }
            _playerTurn = 0;
        }

        private void DoPlayerMove(object sender, MouseButtonEventArgs e)
        {
            Rectangle hitbox = (Rectangle)e.Source;
            Grid gameGrid = (Grid)this.FindName("GameGrid");
            string b = string.Join("", hitbox.Name.Where(char.IsDigit));
            int c = int.Parse(b);

            bool hasPlayed = false;
            int rowPlayed = -1;

            for (int i = 6; i >= 0; i--) {
                if (_c4Board[i, c] == -1) {
                    _c4Board[i, c] = _playerTurn;
                    hasPlayed = true;
                    rowPlayed = i;
                    break;
                }
            }

            if (hasPlayed && rowPlayed != -1) {
                if (_playerTurn == 0) {
                    PlacePlayerMoveCounter(_playerTurn, c, rowPlayed, gameGrid);
                    RemoveHoverEffect(c, gameGrid);

                    if (Connect4API.HasPlayerWon(_c4Board, _playerTurn)) {
                        DisableHitboxes();
                        _winEvent.Invoke(_playerTurn);
                    } else {
                        PlaceHoverEffect(1, c, gameGrid);
                    }
                    _playerTurn = 1;
                } else if (_playerTurn == 1) {
                    PlacePlayerMoveCounter(_playerTurn, c, rowPlayed, gameGrid);
                    RemoveHoverEffect(c, gameGrid);
                    if (Connect4API.HasPlayerWon(_c4Board, _playerTurn)) {
                        DisableHitboxes();
                        _winEvent.Invoke(_playerTurn);
                    } else {
                        PlaceHoverEffect(0, c, gameGrid);
                    }
                    _playerTurn = 0;
                }
            }

            if (Connect4API.IsDraw(_c4Board)) {
                DrawPopup.Opacity = 0d;
                DrawPopup.Visibility = Visibility.Visible;

                DoubleAnimation animation = new DoubleAnimation()
                {
                    From = 0.0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.25))
                };
                DrawPopup.BeginAnimation(OpacityProperty, animation);
            }
            //if (!gameWin) { Connect4API.SaveBoard(C4Board, _playerTurn, "/LMC4SF"); }
        }

        private void DisableHitboxes()
        {
            HitboxColumn0.IsEnabled = false;
            HitboxColumn1.IsEnabled = false;
            HitboxColumn2.IsEnabled = false;
            HitboxColumn3.IsEnabled = false;
            HitboxColumn4.IsEnabled = false;
            HitboxColumn5.IsEnabled = false;
            HitboxColumn6.IsEnabled = false;
        }

        private void PlacePlayerMoveCounter(int pM, int c, int r, Grid gameGrid)
        {
            if (pM == 0) {
                Ellipse playerMove = new Ellipse { Fill = Brushes.Red, Margin = new Thickness(2.5, 2.5, 2.5, 2.5) };
                gameGrid.Children.Add(playerMove);
                Grid.SetColumn(playerMove, c);
                Grid.SetRow(playerMove, r + 1);
            } else {
                Ellipse playerMove = new Ellipse { Fill = Brushes.Yellow, Margin = new Thickness(2.5, 2.5, 2.5, 2.5) };
                gameGrid.Children.Add(playerMove);
                Grid.SetColumn(playerMove, c);
                Grid.SetRow(playerMove, r + 1);
            }
        }

        private void PlaceHoverEffect(int pM, int c, Grid gameGrid)
        {
            if (pM == 0) {
                Ellipse hoverCircle = new Ellipse { Fill = Brushes.Red, Opacity = 0.25, Margin = new Thickness(2.5, 2.5, 2.5, 2.5) };
                gameGrid.Children.Add(hoverCircle);
                Grid.SetColumn(hoverCircle, c);
                Grid.SetRow(hoverCircle, 0);
            } else if (pM == 1) {
                Ellipse hoverCircle = new Ellipse { Fill = Brushes.Yellow, Opacity = 0.25, Margin = new Thickness(2.5, 2.5, 2.5, 2.5) };
                gameGrid.Children.Add(hoverCircle);
                Grid.SetColumn(hoverCircle, c);
                Grid.SetRow(hoverCircle, 0);
            }
        }

        private void RemoveHoverEffect(int c, Grid gameGrid)
        {
            try {
                UIElement old = gameGrid.Children.Cast<UIElement>().First(ellipse => Grid.GetRow(ellipse) == 0 && Grid.GetColumn(ellipse) == c);
                gameGrid.Children.Remove(old);
            } catch {
                // ignored
            }
        }

        private void HoverColumnEffect(object sender, MouseEventArgs e)
        {
            Rectangle hitbox = (Rectangle)e.Source;
            Grid gameGrid = (Grid)this.FindName("GameGrid");
            string b = string.Join("", hitbox.Name.Where(char.IsDigit));
            int c = int.Parse(b);

            PlaceHoverEffect(_playerTurn, c, gameGrid);
        }

        private void HoverColumnEffectFin(object sender, MouseEventArgs e)
        {
            Rectangle hitbox = (Rectangle)e.Source;
            Grid gameGrid = (Grid)this.FindName("GameGrid");
            string b = string.Join("", hitbox.Name.Where(char.IsDigit));
            int c = int.Parse(b);
            RemoveHoverEffect(c, gameGrid);
        }

        private void WinOutput(int plrWon)
        {
            new Thread(() => {
                switch (plrWon) {
                    case 0:
                        Dispatcher.Invoke(() => UsernameWinsMsg.Content = "- " + _usernames.Item1 + " Wins! -");

                        string[] pl1Upd = { "1", "0", "1", "1", "0" };
                        string error = LoginAPI.UpdatePlayerStats(_plr1Stats, pl1Upd, _sqlConn,
                            "UPDATE UserData SET O_Wins = ?oW, O_Loses = ?oL, O_GamesPlayed = ?oGP, O_TimesPlayedAsRed = ?oTPR, O_TimesPlayedAsYellow = ?oTPY WHERE UserID = ?uID",
                            _uIDs.Item1.ToString(), "?oW", "?oL", "?oGP", "?oTPR", "?oTPY", "?uID");

                        if (error.Contains("/")) {
                            Dispatcher.Invoke(() => PrintError(int.Parse(error.Split('/')[0])));
                            return;
                        }

                        string[] pl2Upd = { "0", "1", "1", "0", "1" };
                        error = LoginAPI.UpdatePlayerStats(_plr2Stats, pl2Upd, _sqlConn,
                            "UPDATE UserData SET O_Wins = ?oW, O_Loses = ?oL, O_GamesPlayed = ?oGP, O_TimesPlayedAsRed = ?oTPR, O_TimesPlayedAsYellow = ?oTPY WHERE UserID = ?uID",
                            _uIDs.Item2.ToString(), "?oW", "?oL", "?oGP", "?oTPR", "?oTPY", "?uID");

                        if (error.Contains("/")) {
                            Dispatcher.Invoke(() => PrintError(int.Parse(error.Split('/')[0])));
                            return;
                        }
                        break;
                    case 1:
                        Dispatcher.Invoke(() => UsernameWinsMsg.Content = "- " + _usernames.Item2 + " Wins! -");

                        string[] pl2UpdB = { "1", "0", "1", "0", "1" };
                        error = LoginAPI.UpdatePlayerStats(_plr2Stats, pl2UpdB, _sqlConn,
                            "UPDATE UserData SET O_Wins = ?oW, O_Loses = ?oL, O_GamesPlayed = ?oGP, O_TimesPlayedAsRed = ?oTPR, O_TimesPlayedAsYellow = ?oTPY WHERE UserID = ?uID",
                            _uIDs.Item2.ToString(), "?oW", "?oL", "?oGP", "?oTPR", "?oTPY", "?uID");

                        if (error.Contains("/")) {
                            Dispatcher.Invoke(() => PrintError(int.Parse(error.Split('/')[0])));
                            return;
                        }

                        string[] pl1UpdB = { "0", "1", "1", "1", "0" };
                        error = LoginAPI.UpdatePlayerStats(_plr1Stats, pl1UpdB, _sqlConn,
                            "UPDATE UserData SET O_Wins = ?oW, O_Loses = ?oL, O_GamesPlayed = ?oGP, O_TimesPlayedAsRed = ?oTPR, O_TimesPlayedAsYellow = ?oTPY WHERE UserID = ?uID",
                            _uIDs.Item1.ToString(), "?oW", "?oL", "?oGP", "?oTPR", "?oTPY", "?uID");

                        if (error.Contains("/")) {
                            Dispatcher.Invoke(() => PrintError(int.Parse(error.Split('/')[0])));
                            return;
                        }
                        break;
                }

                Dispatcher.Invoke(() => {
                    WinPopup.Opacity = 0d;
                    WinPopup.Visibility = Visibility.Visible;

                    //Connect4API.SetFileAsWon(C4Board, _playerTurn, "/LMC4SF");

                    DoubleAnimation animation = new DoubleAnimation() {
                        From = 0.0,
                        To = 0.8,
                        Duration = new Duration(TimeSpan.FromSeconds(0.25))
                    };

                    WinPopup.BeginAnimation(OpacityProperty, animation);
                    _playerTurn = 0;
                });
            }).Start();
        }

        private void QuitButton_OnClick(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            _winEvent -= WinOutput;

            animation.Completed += CloseEvent;
            Window.BeginAnimation(OpacityProperty, animation);
        }

        private void CloseEvent(object s, EventArgs e)
        {
            Close();
        }

        private void PopupClose(object s, EventArgs e)
        {
            QuitButton1.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            Window.WindowState = WindowState.Minimized;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            animation.Completed += Animation_Completed;
            WinPopup.BeginAnimation(OpacityProperty, animation);

            RestartGame();
        }

        private void EnableHitboxes()
        {
            HitboxColumn0.IsEnabled = true;
            HitboxColumn1.IsEnabled = true;
            HitboxColumn2.IsEnabled = true;
            HitboxColumn3.IsEnabled = true;
            HitboxColumn4.IsEnabled = true;
            HitboxColumn5.IsEnabled = true;
            HitboxColumn6.IsEnabled = true;
        }

        private void Animation_Completed(object sender, EventArgs e)
        {
            WinPopup.Visibility = Visibility.Hidden;
            WinPopup.Opacity = 1.0d;
        }

        private void Animation_CompletedB(object sender, EventArgs e)
        {
            DrawPopup.Visibility = Visibility.Hidden;
            DrawPopup.Opacity = 1.0d;
        }

        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            animation.Completed += PopupClose;
            WinPopup.BeginAnimation(OpacityProperty, animation);
        }

        private void RestartButton_ClickB(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            animation.Completed += Animation_CompletedB;
            DrawPopup.BeginAnimation(OpacityProperty, animation);

            RestartGame();
        }

        private void CloseMenu_ClickB(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            animation.Completed += PopupClose;
            DrawPopup.BeginAnimation(OpacityProperty, animation);
        }

        private void RestartGame()
        {
            InitBoard();

            Grid gameGrid = (Grid) this.FindName("GameGrid");
            List<Ellipse> arr = new List<Ellipse>();

            if (gameGrid != null) {
                arr.AddRange(gameGrid.Children.Cast<UIElement>().Where(e1 => e1.GetType().Name == "Ellipse").Cast<Ellipse>());
                //LINQ, Takes all Elements in gameGrid.Children of CAST UIElement, where the Type = "Ellipse" and Casts them to Ellipse.

                foreach (Ellipse t in arr) {
                    gameGrid.Children.Remove(t);
                }
            }

            GetUsernames();

        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            int pTurn;
            (_c4Board, pTurn) = Connect4API.LoadBoard("/LMC4SF");
            if (_playerTurn == -1) { throw new Exception("Player Move is -1"); }

            _playerTurn = pTurn;
            DrawC4Board();

            DoubleAnimation animation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };
            animation.Completed += (o, args) => LoadGamePopup.Visibility = Visibility.Hidden;
            LoadGamePopup.BeginAnimation(OpacityProperty, animation);
        }

        private void DrawC4Board()
        {
            Grid gameGrid = (Grid)this.FindName("GameGrid");
            if (gameGrid != null) {
                for (int i = 0; i < 7; i++) {
                    for (int j = 0; j < 7; j++) {
                        if (_c4Board[i, j] != -1) { PlacePlayerMoveCounter(_c4Board[i, j], j, i, gameGrid); }
                    }
                }
            }
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            animation.Completed += PopupCloseL;
            LoadGamePopup.BeginAnimation(OpacityProperty, animation);
        }

        private void PopupCloseL(object s, EventArgs e)
        {
            LoadGamePopup.Visibility = Visibility.Hidden;
            LoadGamePopup.Opacity = 0.0d;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _mW.Show();
        }

        private void Window_OnClosing(object sender, CancelEventArgs e) { }

        private void PrintError(int errorId) {
            Dispatcher.Invoke(() => {
                OutputBox.Visibility = Visibility.Visible;

                DoubleAnimation animation = new DoubleAnimation() {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.25))
                };
                OutputBox.BeginAnimation(OpacityProperty, animation);

                switch (errorId) {
                    case 0:
                        ErrorLabel.Content = "Connection Failed - No Internet Connection";
                        break;
                    case 1:
                        ErrorLabel.Content = "User ID doesn't exist!";
                        break;
                    case 9:
                        ErrorLabel.Content = "An Unknown Error has occured while logging in...";
                        break;
                }
            });
        }

        private void CloseError_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation() {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            animation.Completed += PopupCloseA;
            OutputBox.BeginAnimation(OpacityProperty, animation);
        }

        private void PopupCloseA(object s, EventArgs e)
        {
            OutputBox.Visibility = Visibility.Hidden;
            QuitButton1.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
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
