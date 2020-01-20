using Microsoft.Kinect;
using System;
using System.Collections.Generic;
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


namespace KinectAppEjercicio2
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor sensor;
        private byte[] colorPixels;
        private WriteableBitmap colorBitmap;
        private DepthImagePixel[] depthPixels;
        public MainWindow()
        {
            InitializeComponent();
            radioButtonColor.IsChecked = true;
            radioButtonDepth.IsChecked = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    sensor = potentialSensor;
                    if(sensor != null)
                    {
                        colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                        colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth,
                            sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                        sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                        sensor.ColorFrameReady += SensorColorFrameReady;
                    }
                    break;
                }
            }
            startSensor();            
        }
        
        private void startSensor()
        {
            try
            {
                sensor.Start();
            }
            catch (NullReferenceException)
            {
                sensor = null;
                this.text.Text = "Kinect no preparada";
            }
        }


        private void radioButtonColor_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    sensor = potentialSensor;
                    if (sensor != null)
                    {
                        sensor.DepthStream.Disable();
                        colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                        colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth,
                            sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                        sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                        sensor.ColorFrameReady += SensorColorFrameReady;
                    }
                    break;
                }
            }
            startSensor();
        }

        private void radioButtonDepth_Checked(object sender, RoutedEventArgs e)
        {
            
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    sensor = potentialSensor;
                    if (sensor != null)
                    {
                        sensor.ColorStream.Disable();
                        colorPixels = new byte[sensor.DepthStream.FramePixelDataLength * sizeof(int)];
                        depthPixels = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
                        colorBitmap = new WriteableBitmap(sensor.DepthStream.FrameWidth,
                            sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                        sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                        sensor.DepthFrameReady += SensorDepthFrameReady;
                    }
                    break;
                }
            }
            startSensor();
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

        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    depthFrame.CopyDepthImagePixelDataTo(depthPixels);

                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;

                    int colorPixelIndex = 0;
                    for (int i = 0; i < depthPixels.Length; i++)
                    {
                        short depth = depthPixels[i].Depth;
                        byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);
                        colorPixels[colorPixelIndex++] = intensity;
                        colorPixels[colorPixelIndex++] = intensity;
                        colorPixels[colorPixelIndex++] = intensity;
                        ++colorPixelIndex;
                    }

                    colorBitmap.WritePixels(new Int32Rect(0, 0,
                        colorBitmap.PixelWidth, colorBitmap.PixelHeight),
                        colorPixels, colorBitmap.PixelWidth * sizeof(int), 0);

                    image.Source = colorBitmap;
                }
            }
        }
    }
}
