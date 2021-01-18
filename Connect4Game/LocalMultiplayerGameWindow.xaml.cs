using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Connect4Game
{
    public partial class LocalMultiplayerGameWindow
    {
        private readonly MainWindow _mW;
        private int playerTurn = 0;
        private readonly double _resWidth = SystemParameters.PrimaryScreenWidth;
        private readonly double _resHeight = SystemParameters.PrimaryScreenHeight;

        private delegate void WinEvent(int plrwon);
        private WinEvent winEvent;
        private Dictionary<string, double[]> _buttonDictionary = new Dictionary<string, double[]>();

        private int[,] C4Board = new int[7,7];
        public LocalMultiplayerGameWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            if (Application.Current.MainWindow != null) {
                this.Width = Width * (_resWidth / 1920);
                this.Height = Height * (_resHeight / 1080);
                this.Left = (_resWidth - this.Width) / 2 + SystemParameters.WorkArea.Left;
                this.Top = (_resHeight - this.Height) / 2 + SystemParameters.WorkArea.Top;
            }
            InitBoard();
            Connect4API.LoadSaveLoc();

            if (Connect4API.SaveFileExists("/LMC4SF")) {
                LoadGamePopup.Visibility = Visibility.Visible;
            }

            winEvent += WinOutput;
            _mW = mainWindow;
        }

        private void InitBoard()
        {
            for (int i = 0; i < 7; i++) {
                for (int j = 0; j < 7; j++) {
                    C4Board[i, j] = -1;
                }
            }

            playerTurn = 0;

        }

        private void DoPlayerMove(object sender, MouseButtonEventArgs e)
        {
            Rectangle hitbox = (Rectangle)e.Source;
            Grid gameGrid = (Grid)this.FindName("GameGrid");
            string b = string.Join("", hitbox.Name.Where(char.IsDigit));
            int c = int.Parse(b);

            bool gameWin = false;
            bool hasPlayed = false;
            int rowPlayed = -1;

            for (int i = 6; i >= 0; i--) {
                if (C4Board[i, c] == -1) {
                    C4Board[i, c] = playerTurn;
                    hasPlayed = true;
                    rowPlayed = i;
                    break;
                }
            }

            if (hasPlayed && rowPlayed != -1) {
                if (playerTurn == 0) {
                    PlacePlayerMoveCounter(playerTurn, c, rowPlayed, gameGrid);
                    RemoveHoverEffect(c, gameGrid);

                    if (Connect4API.HasPlayerWon(C4Board, playerTurn)) {
                        DisableHitboxes();
                        gameWin = true;
                        winEvent.Invoke(playerTurn);
                    } else {
                        PlaceHoverEffect(1, c, gameGrid);
                    }
                    playerTurn = 1;
                } else if (playerTurn == 1) {
                    PlacePlayerMoveCounter(playerTurn, c, rowPlayed, gameGrid);
                    RemoveHoverEffect(c, gameGrid);
                    if (Connect4API.HasPlayerWon(C4Board, playerTurn)) {
                        DisableHitboxes();
                        gameWin = true;
                        winEvent.Invoke(playerTurn);
                    } else {
                        PlaceHoverEffect(0, c, gameGrid);
                    }
                    playerTurn = 0;
                }
            }

            if (Connect4API.IsDraw(C4Board)) {
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

            if (!gameWin) { Connect4API.SaveBoard(C4Board, playerTurn, "/LMC4SF"); }
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
                Ellipse playerMove = new Ellipse { Fill = Brushes.Red, Margin = new Thickness(2.5, 2.5, 2.5, 2.5)};
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
                var old = gameGrid.Children.Cast<UIElement>().First(ellipse => Grid.GetRow(ellipse) == 0 && Grid.GetColumn(ellipse) == c);
                gameGrid.Children.Remove(old);
            } catch {}
        }

        private void HoverColumnEffect(object sender, MouseEventArgs e)
        {
            Rectangle hitbox = (Rectangle)e.Source;
            Grid gameGrid = (Grid)this.FindName("GameGrid");
            string b = string.Join("", hitbox.Name.Where(char.IsDigit));
            int c = int.Parse(b);

            PlaceHoverEffect(playerTurn, c, gameGrid);
        }

        private void HoverColumnEffectFin(object sender, MouseEventArgs e)
        {
            Rectangle hitbox = (Rectangle)e.Source;
            Grid gameGrid = (Grid)this.FindName("GameGrid");
            string b = string.Join("", hitbox.Name.Where(char.IsDigit));
            int c = int.Parse(b);
            RemoveHoverEffect(c, gameGrid);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _mW.Show();
        }

        private void Window_OnClosing(object sender, CancelEventArgs e) { }

        private void WinOutput(int plrWon)
        {
            UsernameWinsMsg.Content = "- Player " + (plrWon + 1) + " Wins! -";
            WinPopup.Opacity = 0d;
            WinPopup.Visibility = Visibility.Visible;

            Connect4API.SetFileAsWon(C4Board, playerTurn, "/LMC4SF");

            DoubleAnimation animation = new DoubleAnimation() {
                From = 0.0,
                To = 0.8,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            WinPopup.BeginAnimation(OpacityProperty, animation);
            playerTurn = 0;
        }

        private void QuitButton_OnClick(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            winEvent -= WinOutput;

            animation.Completed += CloseEvent;
            Window.BeginAnimation(OpacityProperty, animation);
        }

        private void CloseEvent(object s, EventArgs e)
        {
            Close();
        }

        private void PopupClose(object s, EventArgs e)
        {
            QuitButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
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

            InitBoard();

            Grid gameGrid = (Grid)this.FindName("GameGrid");
            List<Ellipse> arr = new List<Ellipse>();

            if (gameGrid != null) {
                arr.AddRange(gameGrid.Children.Cast<UIElement>().Where(e1 => e1.GetType().Name == "Ellipse").Cast<Ellipse>());
                //LINQ, Takes all Elements in gameGrid.Children of CAST UIElement, where the Type = "Ellipse" and Casts them to Ellipse.

                foreach (Ellipse t in arr) {
                    gameGrid.Children.Remove(t);
                }
            }

            EnableHitboxes();
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

        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {
           DoubleAnimation animation = new DoubleAnimation() {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            animation.Completed += PopupClose;
            WinPopup.BeginAnimation(OpacityProperty, animation);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            int pTurn;
            (C4Board, pTurn) = Connect4API.LoadBoard("/LMC4SF");
            if (playerTurn == -1) { throw new Exception("Player Move is -1"); }

            playerTurn = pTurn;
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

        private void DrawC4Board() {
            Grid gameGrid = (Grid)this.FindName("GameGrid");
            if (gameGrid != null) {
                for (int i = 0; i < 7; i++) {
                    for (int j = 0; j < 7; j++) {
                        if (C4Board[i, j] != -1) { PlacePlayerMoveCounter(C4Board[i, j], j, i, gameGrid); }
                    }
                }
            }
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation {
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

        private void Animation_CompletedB(object sender, EventArgs e)
        {
            DrawPopup.Visibility = Visibility.Hidden;
            DrawPopup.Opacity = 1.0d;
        }

        private void RestartGame()
        {
            InitBoard();

            Grid gameGrid = (Grid)this.FindName("GameGrid");
            List<Ellipse> arr = new List<Ellipse>();

            if (gameGrid != null)
            {
                arr.AddRange(gameGrid.Children.Cast<UIElement>().Where(e1 => e1.GetType().Name == "Ellipse").Cast<Ellipse>());
                //LINQ, Takes all Elements in gameGrid.Children of CAST UIElement, where the Type = "Ellipse" and Casts them to Ellipse.

                foreach (Ellipse t in arr)
                {
                    gameGrid.Children.Remove(t);
                }
            }

            EnableHitboxes();
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

    //class Player
    //{
    //    private string _name;
    //    private int _playerTurn;
    //    public Player(int pT, string name) {
    //        _name = name;
    //        _playerTurn = pT;
    //    }
    //}

}
