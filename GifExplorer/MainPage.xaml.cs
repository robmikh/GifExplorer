using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.Graphics.DirectX;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace GifExplorer
{
    class GifFrame
    {
        public InteropBrush ImageBrush { get; }
        public string DisplayName { get; }
        public BitmapPropertySet Properties { get; }

        public GifFrame(CompositionGraphicsDevice compGraphics, CanvasBitmap bitmap, string displayName, BitmapPropertySet properties)
        {
            var size = bitmap.SizeInPixels;
            var surface = compGraphics.CreateDrawingSurface2(new SizeInt32() { Width = (int)size.Width, Height = (int)size.Height }, DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            using (var session = CanvasComposition.CreateDrawingSession(surface))
            {
                session.Clear(Colors.Transparent);
                session.DrawImage(bitmap);
            }
            var compositor = compGraphics.Compositor;
            var compBrush = compositor.CreateSurfaceBrush(surface);
            compBrush.BitmapInterpolationMode = CompositionBitmapInterpolationMode.NearestNeighbor;
            ImageBrush = new InteropBrush(compBrush);
            DisplayName = displayName;
            Properties = properties;
        }
    }

    public sealed partial class MainPage : Page
    {
        private CanvasDevice _device;

        private Compositor _compositor;
        private CompositionGraphicsDevice _compGraphics;

        public MainPage()
        {
            this.InitializeComponent();

            _device = new CanvasDevice();

            _compositor = Window.Current.Compositor;
            _compGraphics = CanvasComposition.CreateCompositionGraphicsDevice(_compositor, _device);

            MainNavigationView.SelectedItem = FramesTab;
        }

        private void MainNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var item = (NavigationViewItem)args.SelectedItem;
            var tag = item.Tag as string;
            switch (tag)
            {
                case "Frames":
                    FramesListView.Visibility = Visibility.Visible;
                    FrameInfoGrid.Visibility = Visibility.Collapsed;
                    break;
                case "FrameInfo":
                    FramesListView.Visibility = Visibility.Collapsed;
                    FrameInfoGrid.Visibility = Visibility.Visible;
                    break;
            }
        }

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".gif");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await OpenFileAsync(file);
            }
        }

        private async Task OpenFileAsync(StorageFile file)
        {
            var frames = new List<GifFrame>();
            using (var stream = await file.OpenReadAsync())
            {
                var decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.GifDecoderId, stream);
                var numFrames = decoder.FrameCount;
                for (uint i = 0; i < numFrames; i++)
                {
                    var frame = await decoder.GetFrameAsync(i);
                    var bitmap = await DecodeBitmapFrameAsync(frame);

                    var properties = await frame.BitmapProperties.GetPropertiesAsync(new string[] 
                    {
                        "/grctlext/Delay",
                        "/imgdesc/Left",
                        "/imgdesc/Top",
                        "/imgdesc/Width",
                        "/imgdesc/Height"
                    });
                    //var delay = (ushort)properties["/grctlext/Delay"].Value;
                    //var rect = new RectInt32()
                    //{
                    //    X = (ushort)properties["/imgdesc/Left"].Value,
                    //    Y = (ushort)properties["/imgdesc/Top"].Value,
                    //    Width = (ushort)properties["/imgdesc/Width"].Value,
                    //    Height = (ushort)properties["/imgdesc/Height"].Value,
                    //};


                    var gifFrame = new GifFrame(_compGraphics, bitmap, $"{i}", properties);
                    frames.Add(gifFrame);
                }
            }

            FramesListView.ItemsSource = frames;
        }

        private async Task<CanvasBitmap> DecodeBitmapFrameAsync(BitmapFrame frame)
        {
            var width = frame.PixelWidth;
            var height = frame.PixelHeight;
            var pixels = await frame.GetPixelDataAsync();
            var bytes = pixels.DetachPixelData();

            var format = frame.BitmapPixelFormat;
            switch (format)
            {
                case BitmapPixelFormat.Bgra8:
                    // Do nothing, it's in a format we like
                    break;
                case BitmapPixelFormat.Rgba8:
                    // Swizzle the bits
                    for (var i = 0; i < bytes.Length; i += 4)
                    {
                        var r = bytes[i + 0];
                        bytes[i + 0] = bytes[i + 2];
                        bytes[i + 2] = r;
                    }
                    break;
                default:
                    throw new Exception($"Unknown pixel format ({format})!");
            }
            
            return CanvasBitmap.CreateFromBytes(_device, bytes, (int)width, (int)height, DirectXPixelFormat.B8G8R8A8UIntNormalized);
        }

        private void FramesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var frame = (GifFrame)((ListView)sender).SelectedItem;
            MainFrameView.Fill = frame.ImageBrush;
        }
    }
}
