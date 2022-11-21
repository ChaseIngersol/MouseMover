using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Cursor = System.Windows.Forms.Cursor;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;

namespace MouseMover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        bool shouldContinue = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(() => RunMover()));
            thread.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            shouldContinue = false;

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                Status.Content = "Stopped";
                Status.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xF4, 0x43, 0x36));
            }), DispatcherPriority.Render);
        }

        private void RunMover() 
        {
            shouldContinue = true;
            Mover mouseMover = new Mover();

            Console.WriteLine("Starting mover...");

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                Status.Content = "Running...";
                Status.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x4C, 0xAF, 0x50));
            }), DispatcherPriority.Render);

            while (shouldContinue)
            {
                Point currPos = System.Windows.Forms.Control.MousePosition;
                Point newPos = mouseMover.GeneratePos(true);
                mouseMover.LinearSmoothMove(currPos, newPos, 50);
                Thread.Sleep(5000);
            }
        }
    }
    public class Mover
    {
        private Rectangle ScreenBounds { get; set; }
        private int MouseStepDelay { get; set; }

        public Mover()
        {
            this.ScreenBounds = Screen.PrimaryScreen.Bounds;
            this.MouseStepDelay = 1;
        }

        public Point GeneratePos(bool shouldConstrain)
        {
            int constrainFactor = 4; // Limit area of screen bounds used for random pos generation by factor of n
            int minX = shouldConstrain ? ScreenBounds.Width / constrainFactor + ScreenBounds.Width / (constrainFactor * 2) : ScreenBounds.Width;
            int minY = shouldConstrain ? ScreenBounds.Height / constrainFactor + ScreenBounds.Height / (constrainFactor * 2) : ScreenBounds.Height;
            int maxX = shouldConstrain ? ScreenBounds.Width - minX : 0;
            int maxY = shouldConstrain ? ScreenBounds.Height - minY : 0;
            int randX = new Random().Next(minX, maxX);
            int randY = new Random().Next(minY, maxY);
            return new Point(randX, randY);
        }

        public void LinearSmoothMove(Point currPosition, Point newPosition, int steps)
        {
            PointF iterPoint = currPosition;

            // Find the slope of the line segment defined by start and newPosition
            PointF slope = new PointF(newPosition.X - currPosition.X, newPosition.Y - currPosition.Y);

            // Divide by the number of steps
            slope.X = slope.X / steps;
            slope.Y = slope.Y / steps;

            // Move the mouse to each iterative point.
            for (int i = 0; i < steps; i++)
            {
                iterPoint = new PointF(iterPoint.X + slope.X, iterPoint.Y + slope.Y);
                Win32.SetCursorPos((int)iterPoint.X, (int)iterPoint.Y);
                Thread.Sleep(MouseStepDelay);
            }

            // Move the mouse to the final destination.
            Win32.SetCursorPos(newPosition.X, newPosition.Y);
        }
    }
    public class Win32
    {
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("User32.Dll")]
        public static extern System.Drawing.Point GetCursorPos();

        [DllImport("User32.Dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(int X, int Y)
            {
                x = X;
                y = Y;
            }
        }
    }
}