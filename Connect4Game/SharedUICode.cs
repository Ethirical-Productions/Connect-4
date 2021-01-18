using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Connect4Game
{
    internal class SharedUICode
    {
        public void DoEventExpand(Button currentButton, ref Dictionary<string, double[]> buttonDictionary)
        {
            string horiString = GetHoriString(currentButton);
            string vertString = GetVertString(currentButton);

            if (!buttonDictionary.ContainsKey(currentButton.Name)) {
                double verticalValue = 0.0d;
                double horizontalValue = 0.0d;

                if (!double.IsNaN((double) currentButton.GetValue(Canvas.TopProperty))) {
                    verticalValue = (double) currentButton.GetValue(Canvas.TopProperty);
                } else if (!double.IsNaN((double) currentButton.GetValue(Canvas.BottomProperty))) {
                    verticalValue = (double) currentButton.GetValue(Canvas.BottomProperty);
                }

                if (!double.IsNaN((double) currentButton.GetValue(Canvas.RightProperty))) {
                    horizontalValue = (double) currentButton.GetValue(Canvas.RightProperty);
                } else if (!double.IsNaN((double) currentButton.GetValue(Canvas.LeftProperty))) {
                    horizontalValue = (double) currentButton.GetValue(Canvas.LeftProperty);
                }

                double[] doubles =
                    {currentButton.Width, currentButton.Height, horizontalValue, verticalValue};
                buttonDictionary.Add(currentButton.Name, doubles);
            }

            IEasingFunction eF = new CubicEase {EasingMode = EasingMode.EaseInOut};

            DoubleAnimation width = new DoubleAnimation {
                To = buttonDictionary[currentButton.Name][0] * 1.1,
                EasingFunction = eF,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };
            DoubleAnimation height = new DoubleAnimation {
                To = buttonDictionary[currentButton.Name][1] * 1.1,
                EasingFunction = eF,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };
            DoubleAnimation cHorizonatal = new DoubleAnimation {
                To = buttonDictionary[currentButton.Name][2] - buttonDictionary[currentButton.Name][0] * 0.05,
                EasingFunction = eF,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };
            DoubleAnimation cVertical = new DoubleAnimation {
                To = buttonDictionary[currentButton.Name][3] - buttonDictionary[currentButton.Name][1] * 0.05,
                EasingFunction = eF,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            SetStoryboardTargets(currentButton, width, height, cHorizonatal, cVertical, horiString, vertString);

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(width);
            storyboard.Children.Add(height);
            storyboard.Children.Add(cHorizonatal);
            storyboard.Children.Add(cVertical);

            storyboard.Begin(currentButton);
        }

        public void DoEventShrink(Button currentButton, ref Dictionary<string, double[]> buttonDictionary)
        {
            string horiString = GetHoriString(currentButton);
            string vertString = GetVertString(currentButton);

            if (!buttonDictionary.ContainsKey(currentButton.Name)) {
                double verticalValue = 0.0d;
                double horizontalValue = 0.0d;

                if (!double.IsNaN((double)currentButton.GetValue(Canvas.TopProperty))) {
                    verticalValue = (double)currentButton.GetValue(Canvas.TopProperty);
                } else if (!double.IsNaN((double)currentButton.GetValue(Canvas.BottomProperty))) {
                    verticalValue = (double)currentButton.GetValue(Canvas.BottomProperty);
                }

                if (!double.IsNaN((double)currentButton.GetValue(Canvas.RightProperty))) {
                    horizontalValue = (double)currentButton.GetValue(Canvas.RightProperty);
                } else if (!double.IsNaN((double)currentButton.GetValue(Canvas.LeftProperty))) {
                    horizontalValue = (double)currentButton.GetValue(Canvas.LeftProperty);
                }

                double[] doubles =
                    {currentButton.Width, currentButton.Height, horizontalValue, verticalValue};
                buttonDictionary.Add(currentButton.Name, doubles);
            }

            IEasingFunction eF = new CubicEase { EasingMode = EasingMode.EaseInOut };

            DoubleAnimation width = new DoubleAnimation
            {
                To = buttonDictionary[currentButton.Name][0],
                EasingFunction = eF,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };
            DoubleAnimation height = new DoubleAnimation
            {
                To = buttonDictionary[currentButton.Name][1],
                EasingFunction = eF,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };
            DoubleAnimation cHori = new DoubleAnimation()
            {
                To = buttonDictionary[currentButton.Name][2],
                EasingFunction = eF,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };
            DoubleAnimation cVert = new DoubleAnimation()
            {
                To = buttonDictionary[currentButton.Name][3],
                EasingFunction = eF,
                Duration = new Duration(TimeSpan.FromSeconds(0.25))
            };

            SetStoryboardTargets(currentButton, width, height, cHori, cVert, horiString, vertString);

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(width);
            storyboard.Children.Add(height);
            storyboard.Children.Add(cHori);
            storyboard.Children.Add(cVert);

            storyboard.Begin(currentButton);
        }

        private static void SetStoryboardTargets(Button currentButton, DoubleAnimation width, DoubleAnimation height, DoubleAnimation cHori, DoubleAnimation cVert, string hor, string ver)
        {
            Storyboard.SetTargetName(width, currentButton.Name);
            Storyboard.SetTargetName(height, currentButton.Name);
            Storyboard.SetTargetName(cHori, currentButton.Name);
            Storyboard.SetTargetName(cVert, currentButton.Name);

            Storyboard.SetTargetProperty(width, new PropertyPath("Width"));
            Storyboard.SetTargetProperty(height, new PropertyPath("Height"));
            Storyboard.SetTargetProperty(cHori, new PropertyPath(hor));
            Storyboard.SetTargetProperty(cVert, new PropertyPath(ver));
        }

        private static string GetHoriString(Button currentButton)
        {
            return !double.IsNaN((double)currentButton.GetValue(Canvas.RightProperty)) ? "(Canvas.Right)" : "(Canvas.Left)";
        }

        private static string GetVertString(Button currentButton)
        {
            return !double.IsNaN((double)currentButton.GetValue(Canvas.TopProperty)) ? "(Canvas.Top)" : "(Canvas.Bottom)";
        }
    }
}
