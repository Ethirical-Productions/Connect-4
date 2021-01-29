using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LiveCharts;
using LiveCharts.Wpf;

namespace Connect4Game
{
    public partial class ShowStatistics : Window
    {
        private readonly double _resWidth = SystemParameters.PrimaryScreenWidth;
        private readonly double _resHeight = SystemParameters.PrimaryScreenHeight;
        private readonly MainWindow _mW;
        private readonly int _uId;
        //private string _username;
        private readonly string _sqlConn;

        private string[] _plrStats = { "-1", "-1", "-1", "-1", "-1" };
        private readonly string[] _sqlStringNames = { "O_Wins", "O_Loses", "O_GamesPlayed", "O_TimesPlayedAsRed", "O_TimesPlayedAsYellow" };

        private Dictionary<string, double[]> _buttonDictionary = new Dictionary<string, double[]>();
        private bool _countGraphLoaded = false;
        private bool _countPieLoaded = false;
        private bool _winPieLoaded = false;
        private bool _winGraphLoaded = false;

        public ShowStatistics(MainWindow mw, int uId, string sqlCon)
        {
            _uId = uId;
            _mW = mw;
            _sqlConn = sqlCon;
            DataContext = this;

            PointLabel = chartPoint => $"{chartPoint.Y} ({chartPoint.Participation:P})";

            InitializeComponent();

            Width *= (_resWidth / 1920);
            Height *= (_resHeight / 1080);
            Left = (_resWidth - Width) / 2 + SystemParameters.WorkArea.Left;
            Top = (_resHeight - Height) / 2 + SystemParameters.WorkArea.Top;

            DoPlayerStatLoading();
        }

        public Func<ChartPoint, string> PointLabel { get; set; }

        private void DoPlayerStatLoading()
        {
            LoadingPopup.Visibility = Visibility.Visible;

            new Thread(() => {
                Thread.Sleep(1000);
                string user = LoginAPI.GetUsernameFromUserID(_uId.ToString(), _sqlConn,
                    "SELECT Username FROM UserDB Where UserID = ?id", "?id", "Username");
                if (!user.Contains("/")) {
                    //_username = user;
                    if (user[user.Length - 1] == 's' || user[user.Length - 1] == 'S') {
                        Dispatcher.Invoke(() => UsernameTitle.Text = user + "' Statistics");
                    } else {
                        Dispatcher.Invoke(() => UsernameTitle.Text = user + "'s Statistics");
                    }

                    _plrStats = LoginAPI.GetPlayerStats(_uId.ToString(), _sqlConn,
                        "SELECT * FROM UserData WHERE UserID = ?uID", "?uID", _sqlStringNames);

                    Dispatcher.Invoke(() => {
                        RedPieChart.Values = new ChartValues<int> { int.Parse(_plrStats[3]) };
                        YellowPieChart.Values = new ChartValues<int> { int.Parse(_plrStats[4]) };

                        SolidColorBrush colorBrush = new SolidColorBrush(Color.FromRgb(227,227,227));

                        RedPercent.Content = "Red: " + (Math.Round(double.Parse(_plrStats[3]) / double.Parse(_plrStats[2]), 3) * 100) + "%";
                        YellowPercent.Content = "Yellow: " + (Math.Round(double.Parse(_plrStats[4]) / double.Parse(_plrStats[2]), 3) * 100) + "%";

                        WinPercent.Content = "Win: " + (Math.Round(double.Parse(_plrStats[0]) / double.Parse(_plrStats[2]), 3) * 100) + "%";
                        LosePercent.Content = "Lose: " + (Math.Round(double.Parse(_plrStats[1]) / double.Parse(_plrStats[2]), 3) * 100) + "%";

                        CartesianChart gamesPlayerCH = new CartesianChart {
                            LegendLocation = LegendLocation.Right,
                            Foreground = colorBrush,
                            Height = 150,
                            Width = 300,
                            DisableAnimations = true,
                            DataTooltip = null,

                            AxisX = new AxesCollection {
                                new Axis { Title = "Colour", Foreground = colorBrush }
                            },
                            AxisY = new AxesCollection {
                                new Axis { Title = "Game Count", Foreground = colorBrush }
                            },

                            Series = new SeriesCollection {
                                new ColumnSeries {
                                    Title = "Total",
                                    Values = new ChartValues<int> { int.Parse(_plrStats[2]) },
                                    Fill = new SolidColorBrush(Color.FromRgb(0,227,0))
                                },

                                new ColumnSeries {
                                    Title = "Total Red",
                                    Values = new ChartValues<int> { int.Parse(_plrStats[3]) },
                                    Fill = new SolidColorBrush(Color.FromRgb(227,0,0))
                                },

                                new ColumnSeries {
                                    Title = "Total Yellow",
                                    Values = new ChartValues<int> { int.Parse(_plrStats[4]) },
                                    Fill = new SolidColorBrush(Color.FromRgb(227,227,0))
                                },
                            }
                        };

                        Canvas.SetRight(gamesPlayerCH, 45);
                        Canvas.SetBottom(gamesPlayerCH, 45);
                        gamesPlayerCH.Loaded += CountGraphLoaded;
                        BaseCanvas.Children.Add(gamesPlayerCH);

                        PieChart winLosePC = new PieChart {
                            LegendLocation = LegendLocation.Left,
                            Foreground = colorBrush,
                            Height = 105,
                            Width = 180,
                            DisableAnimations = true,
                            DataTooltip = null,

                            Series = {
                                new PieSeries {
                                    Title = "Win",
                                    Fill = new SolidColorBrush(Color.FromRgb(0,227,0)),
                                    Values = new ChartValues<int> { int.Parse(_plrStats[0]) },
                                    Stroke = Brushes.Black
                                },
                                new PieSeries {
                                Title = "Lose",
                                Fill = Brushes.Red,
                                Values = new ChartValues<int> { int.Parse(_plrStats[1]) },
                                Stroke = Brushes.Black
                                }
                            }
                        };

                        Canvas.SetLeft(winLosePC, 45);
                        Canvas.SetBottom(winLosePC, 200);
                        winLosePC.Loaded += WinPieChartLoaded;
                        BaseCanvas.Children.Add(winLosePC);

                        CartesianChart winsCH = new CartesianChart
                        {
                            LegendLocation = LegendLocation.Left,
                            Foreground = colorBrush,
                            Height = 150,
                            Width = 300,
                            DisableAnimations = true,
                            DataTooltip = null,

                            AxisX = new AxesCollection {
                                new Axis { Title = "Win to Loss", Foreground = colorBrush }
                            },
                            AxisY = new AxesCollection {
                                new Axis { Title = "Count", Foreground = colorBrush }
                            },

                            Series = new SeriesCollection {
                                new ColumnSeries {
                                    Title = "Total",
                                    Values = new ChartValues<int> { int.Parse(_plrStats[2]) },
                                    Fill = new SolidColorBrush(Color.FromRgb(0,227,227))
                                },

                                new ColumnSeries {
                                    Title = "Total Wins",
                                    Values = new ChartValues<int> { int.Parse(_plrStats[0]) },
                                    Fill = new SolidColorBrush(Color.FromRgb(0,227,0))
                                },

                                new ColumnSeries {
                                    Title = "Total Losses",
                                    Values = new ChartValues<int> { int.Parse(_plrStats[1]) },
                                    Fill = new SolidColorBrush(Color.FromRgb(227,0,0))
                                },
                            }
                        };

                        Canvas.SetLeft(winsCH, 45);
                        Canvas.SetBottom(winsCH, 45);
                        winsCH.Loaded += WinGraphLoaded;
                        BaseCanvas.Children.Add(winsCH);

                        DataContext = this;
                    });
                }
                else {
                    Dispatcher.Invoke(() => PrintError(int.Parse(user.Split('/')[0])));
                    return;
                }

                while (!_countPieLoaded && !_countGraphLoaded && !_winPieLoaded && !_winGraphLoaded) {
                    Thread.Sleep(1000);
                }

                Dispatcher.Invoke(() => LoadingPopup.Visibility = Visibility.Hidden);
            }).Start();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            animation.Completed += Window_OnClosed;
            Window.BeginAnimation(OpacityProperty, animation);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mW.Show();
        }

        private void Window_OnClosed(object sender, EventArgs e)
        {
            Close();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            Window.WindowState = WindowState.Minimized;
        }

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
            QuitButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        }

        private void CountPieChartLoaded(object s, RoutedEventArgs e)
        {
            _countPieLoaded = true;
        }

        private void CountGraphLoaded(object s, RoutedEventArgs e)
        {
            _countGraphLoaded = true;
        }

        private void WinPieChartLoaded(object s, RoutedEventArgs e)
        {
            _winPieLoaded = true;
        }

        private void WinGraphLoaded(object sender, RoutedEventArgs e)
        {
            _winGraphLoaded = true;
        }
    }
}
