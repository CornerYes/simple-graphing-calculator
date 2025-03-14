
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;
using NCalc;

namespace graphing_calculator_v1
{

    public partial class MainWindow : Window
    {
        public double length = 20f;
        public double scale = 40f;
        public double zoomrate = 2.5f;

        public double xoffset = 0;
        public double yoffset = 0;

        public double speed = 4f;

        double xcanva = 0;
        double ycanva = 0;
        public static double equation(double x)
        {
            double y = (x * x);
            return y;
        }

        public MainWindow()
        {
            InitializeComponent();  
        }

        private void Window_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0) {
                scale += (zoomrate + (scale / 10));
            } else
            {
                scale = Math.Clamp(scale - (zoomrate + (scale / 10)), 10f, 10000);
            }
            updategraph();
        }

        private void drawgrid()
        {
            double griscale = scale;
            for (double x = (xcanva + xoffset) % griscale; x < drawboard.ActualWidth + xcanva; x += griscale)
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

            for (double y = (ycanva + yoffset) % griscale; y < drawboard.ActualHeight; y += griscale)
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
            Polyline polyline = new Polyline();
            polyline.Stroke = Brushes.Black;

            drawgrid();

            double minX = (0 - xcanva - xoffset) / scale;
            double maxX = (drawboard.ActualWidth - xcanva - xoffset) / scale;

            double step = (maxX - minX) / drawboard.ActualWidth;

            for (double x = minX; x <= maxX; x += step)
            {
                double y = equation(x);
                double screenX = xcanva + x * scale + xoffset;
                double screenY = ycanva - y * scale + yoffset;
                polyline.Points.Add(new Point(screenX, screenY));
            }
            drawboard.Children.Add(polyline);

            Line xAxis = new Line
            {
                X1 = 0,
                Y1 = ycanva + yoffset,
                X2 = drawboard.ActualWidth,
                Y2 = ycanva + yoffset,
                Stroke = Brushes.Gray,
                Name = "XAxis"
            };

            Line yAxis = new Line
            {
                X1 = xcanva + xoffset,
                Y1 = 0,
                X2 = xcanva + xoffset,
                Y2 = drawboard.ActualHeight,
                Stroke = Brushes.Gray,
                Name = "YAxis"
            };

            drawboard.Children.Insert(1, xAxis);
            drawboard.Children.Insert(2, yAxis);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            xcanva = this.ActualWidth / 2f;
            ycanva = this.ActualHeight / 2f;
            updategraph();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            xcanva = this.ActualWidth / 2f;
            ycanva = this.ActualHeight / 2f;
            updategraph();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.W)
            {
                yoffset += speed;
            } else if (e.Key == System.Windows.Input.Key.S) {
                yoffset -= speed;
            }
            
            if (e.Key == System.Windows.Input.Key.A) {
                xoffset += speed;
            } else if (e.Key == System.Windows.Input.Key.D)
            { 
                xoffset -= speed;
            }

            updategraph();
        }
    }
}