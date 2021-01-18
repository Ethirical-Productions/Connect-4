using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Definitions.Series;
using LiveCharts.Wpf;

namespace Connect4Game
{
    public partial class ShowStatistics : Window
    {
        private readonly double _resWidth = SystemParameters.PrimaryScreenWidth;
        private readonly double _resHeight = SystemParameters.PrimaryScreenHeight;
        private readonly MainWindow _mW;
        private readonly int _uId;
        private string _username;
        private readonly string _sqlConn;

        private string[] _plrStats = { "-1", "-1", "-1", "-1", "-1" };
        private readonly string[] _sqlStringNames = { "O_Wins", "O_Loses", "O_GamesPlayed", "O_TimesPlayedAsRed", "O_TimesPlayedAsYellow" };

        private Dictionary<string, double[]> _buttonDictionary = new Dictionary<string, double[]>();

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
                string user = LoginAPI.GetUsernameFromUserID(_uId.ToString(), _sqlConn,
                    "SELECT Username FROM UserDB Where UserID = ?id", "?id", "Username");
                if (!user.Contains("/")) {
                    _username = user;
                    if (user[user.Length - 1] == 's') {
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
                        string[] labels = {"", "", ""};

                        CartesianChart cH = new CartesianChart {
                            LegendLocation = LegendLocation.Left,
                            Foreground = colorBrush,
                            Height = 185,
                            Width = 300,
                            DisableAnimations = true,
                            DataTooltip = null,

                            AxisX = new AxesCollection {
                                new Axis { Title = "Colour", Foreground = colorBrush, Labels = labels}
                            },
                            AxisY = new AxesCollection {
                                new Axis { Title = "Game Count", Foreground = colorBrush }
                            },

                            Series = new SeriesCollection {
                                new ColumnSeries {
                                    Title = "Total",
                                    Values = new ChartValues<int> { int.Parse(_plrStats[2]) },
                                    Fill = new SolidColorBrush(Color.FromRgb(98,255,87))
                                },

                                new ColumnSeries {
                                    Title = "Total Red",
                                    Values = new ChartValues<int> { int.Parse(_plrStats[3]) },
                                    Fill = new SolidColorBrush(Color.FromRgb(255,87,87))
                                },

                                new ColumnSeries {
                                    Title = "Total Yellow",
                                    Values = new ChartValues<int> { int.Parse(_plrStats[4]) },
                                    Fill = new SolidColorBrush(Color.FromRgb(255,233,87))
                                },
                            }
                        };

                        Canvas.SetRight(cH, 45);
                        Canvas.SetBottom(cH, 45);

                        BaseCanvas.Children.Add(cH);

                        DataContext = this;
                    });
                }
                else {
                    Dispatcher.Invoke(() => PrintError(int.Parse(user.Split('/')[0])));
                    return;
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
    }
}
