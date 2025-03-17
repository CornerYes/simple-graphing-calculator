
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Linq.Expressions;
using Parlot.Fluent;
using System.Windows.Controls;
using System.Reflection.Metadata;
using System.Security.Cryptography;
namespace graphing_calculator_v1
{

    public partial class MainWindow : Window
    {
        private double scale = 40f;
        private double zoomrate = 2.5f;

        private double xoffset = 0;
        private double yoffset = 0;

        public double speed = 4f;

        double actualwindowmidX = 0;
        double actualwindowmidY = 0;
        private string equation = "x * x";
        private Boolean failedlasttime = false;

        private Dictionary<String, double> parameters = new();
        private HashSet<Key> pressedkeys = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void drawgrid()
        {
            for (double x = (actualwindowmidX + xoffset) % scale; x < drawboard.ActualWidth + actualwindowmidX; x += scale)
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

            for (double y = (actualwindowmidY + yoffset) % scale; y < drawboard.ActualHeight; y += scale)
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

                double minX = (-actualwindowmidX - xoffset) / scale;
                double maxX = (actualwindowmidX - xoffset) / scale;

                double step = (maxX - minX) / drawboard.ActualWidth * 0.5;
                stepname.Content = "Step Count: " + step.ToString();

                var exp = new NCalc.Expression(equation);

                for (double x = minX; x <= maxX; x += step)
                {
                    exp.Parameters["x"] = x;
                   foreach (var param in parameters)
                    {
                        exp.Parameters[param.Key] = param.Value;
                    }

                    double y = 0;
                    double nexty = 0;

                    try
                    {
                        y = Convert.ToDouble(exp.Evaluate());
                        exp.Parameters["x"] = x + step;
                        nexty = Convert.ToDouble(exp.Evaluate());
                    }
                    catch (Exception ex)
                    {
                        errortext.Text = ex.Message;
                        failedlasttime = true;
                        break;
                    }

                    if ((actualwindowmidY - y * scale + yoffset) - (actualwindowmidY - nexty * scale + yoffset) > drawboard.ActualHeight)
                    {
                        drawboard.Children.Add(polyline);
                        polyline = new Polyline();
                        polyline.Stroke = Brushes.Black;
                    }

                    if (!double.IsNaN(y))
                    {
                        double screenX = actualwindowmidX + x * scale + xoffset;
                        double screenY = actualwindowmidY - y * scale + yoffset;
                        polyline.Points.Add(new Point(screenX, screenY));
                    }                  
                }
                drawboard.Children.Add(polyline);

                Line xa = new Line
                {
                    X1 = 0,
                    Y1 = actualwindowmidY + yoffset,
                    X2 = drawboard.ActualWidth,
                    Y2 = actualwindowmidY + yoffset,
                    Stroke = Brushes.Gray,
                    Name = "XAxis",
                    StrokeThickness = 2
                };

                Line ya = new Line
                {
                    X1 = actualwindowmidX + xoffset,
                    Y1 = 0,
                    X2 = actualwindowmidX + xoffset,
                    Y2 = drawboard.ActualHeight,
                    Stroke = Brushes.Gray,
                    Name = "YAxis",
                    StrokeThickness = 2
                };

                drawboard.Children.Insert(1, xa);
                drawboard.Children.Insert(2, ya);
            }
        }


        private void update(object? sender, EventArgs e)
        {
            actualwindowmidX = this.ActualWidth / 2f;
            actualwindowmidY = this.ActualHeight / 2f;

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
            parameters.Add("pi", Math.PI);
            parameters.Add("e", Math.E);
            CompositionTarget.Rendering += update;
        }

        private void Window_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double rate = 0.5f;
            if (e.Delta > 0)
            {
                scale += (zoomrate + (scale / 10));
                xoffset += (actualwindowmidX - Mouse.GetPosition(drawboard).X) * rate;
                yoffset += (actualwindowmidY - Mouse.GetPosition(drawboard).Y) * rate;
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