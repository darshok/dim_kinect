using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Microsoft.Kinect;

namespace KinectApp
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor sensor;
        private byte[] colorPixels;
        private WriteableBitmap colorBitmap;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    sensor = potentialSensor;
                    colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                    colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth,
                        sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                    break;
                }
            }

            if (sensor != null)
            {
                sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                sensor.ColorFrameReady += SensorColorFrameReady;
            }

            try
            {
                sensor.Start();
            }
            catch (NullReferenceException)
            {
                sensor = null;
                text.Text = "Kinect no preparada";
            }
        }
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    colorFrame.CopyPixelDataTo(colorPixels);

                    colorBitmap.WritePixels(new Int32Rect(0, 0,
                        colorBitmap.PixelWidth, colorBitmap.PixelHeight),
                        colorPixels, colorBitmap.PixelWidth * sizeof(int), 0);

                    image.Source = colorBitmap;
                }
            }
        }
    }
}
