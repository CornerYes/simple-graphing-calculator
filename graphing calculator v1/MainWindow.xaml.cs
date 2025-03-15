
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Linq.Expressions;
using Parlot.Fluent;
using System.Windows.Controls;
namespace graphing_calculator_v1
{

    public partial class MainWindow : Window
    {
        private double scale = 40f;
        private double zoomrate = 2.5f;

        private double xoffset = 0;
        private double yoffset = 0;

        public double speed = 4f;
        private HashSet<Key> pressedkeys = new();

        double actualwindowX = 0;
        double actualwindowY = 0;
        private string equation = "x * x";
        private Boolean failedlasttime = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void drawgrid()
        {
            double griscale = scale;
            for (double x = (actualwindowX + xoffset) % griscale; x < drawboard.ActualWidth + actualwindowX; x += griscale)
            {
                Line vl = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = drawboard.ActualHeight,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                drawboard.Children.Add(vl);
            }

            for (double y = (actualwindowY + yoffset) % griscale; y < drawboard.ActualHeight; y += griscale)
            {
                Line hl = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = drawboard.ActualWidth,
                    Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                drawboard.Children.Add(hl);
            }
        }

        private void updategraph()
        {
            drawboard.Children.Clear();
            scalename.Content = "Scale: " + scale.ToString() + "x";
            if (failedlasttime == false) {
                Polyline polyline = new Polyline();
                polyline.Stroke = Brushes.Black;

                drawgrid();

                double minX = (-actualwindowX - xoffset) / scale;
                double maxX = (drawboard.ActualWidth - actualwindowX - xoffset) / scale;

                double step = (maxX - minX) / drawboard.ActualWidth;

                var exp = new NCalc.Expression(equation);
                
                for (double x = minX; x <= maxX; x += step)
                {
                    exp.Parameters["x"] = x;
                    exp.Parameters["pi"] = Math.PI;
                    exp.Parameters["e"] = Math.E;
                    double y = 0;
                    try
                    {
                        y = Convert.ToDouble(exp.Evaluate());
                    }
                    catch (Exception ex)
                    {
                        errortext.Text = ex.Message;
                        failedlasttime = true;
                        break;
                    }
                    if (!double.IsNaN(y))
                    {
                        double screenX = actualwindowX + x * scale + xoffset;
                        double screenY = actualwindowY - y * scale + yoffset;
                        polyline.Points.Add(new Point(screenX, screenY));
                    }
                }
                drawboard.Children.Add(polyline);

                Line xa = new Line
                {
                    X1 = 0,
                    Y1 = actualwindowY + yoffset,
                    X2 = drawboard.ActualWidth,
                    Y2 = actualwindowY + yoffset,
                    Stroke = Brushes.Gray,
                    Name = "XAxis"
                };

                Line ya = new Line
                {
                    X1 = actualwindowX + xoffset,
                    Y1 = 0,
                    X2 = actualwindowX + xoffset,
                    Y2 = drawboard.ActualHeight,
                    Stroke = Brushes.Gray,
                    Name = "YAxis"
                };

                drawboard.Children.Insert(1, xa);
                drawboard.Children.Insert(2, ya);
            }
        }

        private void update(object? sender, EventArgs e)
        {
            actualwindowX = this.ActualWidth / 2f;
            actualwindowY = this.ActualHeight / 2f;

            if (input.IsFocused == false)
            {
                if (pressedkeys.Contains(Key.W)) yoffset += speed;
                if (pressedkeys.Contains(Key.S)) yoffset -= speed;
                if (pressedkeys.Contains(Key.A)) xoffset += speed;
                if (pressedkeys.Contains(Key.D)) xoffset -= speed;
            }

            updategraph();
        }
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += update;
        }

        private void Window_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double rate = 0.5f;
            if (e.Delta > 0)
            {
                scale += (zoomrate + (scale / 10));
                xoffset += (actualwindowX - Mouse.GetPosition(drawboard).X) * rate;
                yoffset += (actualwindowY - Mouse.GetPosition(drawboard).Y) * rate;
            }
            else
            {
                scale = Math.Clamp(scale - (zoomrate + (scale / 10)), 10f, 10000);
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!pressedkeys.Contains(e.Key))
            {
                pressedkeys.Add(e.Key);
            }

        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (pressedkeys.Contains(e.Key))
            {
                pressedkeys.Remove(e.Key);
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                if (failedlasttime == true) {
                    errortext.Text = " ";
                    failedlasttime = false;
                }
                string equationstring = input.Text;
                drawboard.Focus();
                equation = equationstring;
            }
        }
    }
}