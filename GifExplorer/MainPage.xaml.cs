using GifExplorer.Core;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.Graphics.DirectX;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
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
        public RectInt32 Rect { get; }
        public CanvasBitmap Bitmap { get; }

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
            Rect = new RectInt32()
            {
                X = (ushort)properties["/imgdesc/Left"].Value,
                Y = (ushort)properties["/imgdesc/Top"].Value,
                Width = (ushort)properties["/imgdesc/Width"].Value,
                Height = (ushort)properties["/imgdesc/Height"].Value,
            };
            Bitmap = bitmap;
        }
    }

    public sealed partial class MainPage : Page
    {
        private CanvasDevice _device;

        private Compositor _compositor;
        private CompositionGraphicsDevice _compGraphics;

        private GifFrame _rightClickedFrame;
        private string _rightClickedInfo;

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
                    ContainerInfoGrid.Visibility = Visibility.Collapsed;
                    break;
                case "FrameInfo":
                    FramesListView.Visibility = Visibility.Collapsed;
                    FrameInfoGrid.Visibility = Visibility.Visible;
                    ContainerInfoGrid.Visibility = Visibility.Collapsed;
                    break;
                case "ContainerInfo":
                    FramesListView.Visibility = Visibility.Collapsed;
                    FrameInfoGrid.Visibility = Visibility.Collapsed;
                    ContainerInfoGrid.Visibility = Visibility.Visible;
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

        public async Task OpenFileAsync(StorageFile file)
        {
            _rightClickedFrame = null;
            _rightClickedInfo = null;

            BitmapPropertySet containerProperties = null;
            var frames = new List<GifFrame>();
            using (var stream = await file.OpenReadAsync())
            {
                var decoder = new GifDecoder(stream);
                var width = decoder.PixelWidth;
                var height = decoder.PixelHeight;
                MainFrameCanvas.Width = width;
                MainFrameCanvas.Height = height;

                // Information on gif metadata can be found here:
                // https://docs.microsoft.com/en-us/windows/win32/wic/-wic-native-image-format-metadata-queries#gif-metadata

                // Extract container info
                containerProperties = decoder.BitmapContainerProperties.GetProperties(new string[]
                {
                    "/logscrdesc/Width",
                    "/logscrdesc/Height",
                    "/commentext/TextEntry",
                    "/logscrdesc/Signature",
                    "/logscrdesc/GlobalColorTableFlag",
                    "/logscrdesc/ColorResolution",
                    "/logscrdesc/SortFlag",
                    "/logscrdesc/GlobalColorTableSize",
                    "/logscrdesc/BackgroundColorIndex",
                    "/logscrdesc/PixelAspectRatio",
                    "/appext/Application",
                    "/appext/Data"
                });

                // Extract frames
                var numFrames = decoder.FrameCount;
                for (uint i = 0; i < numFrames; i++)
                {
                    var frame = decoder.GetFrame(i);
                    var bitmap = DecodeBitmapFrame(frame);

                    var properties = frame.BitmapProperties.GetProperties(new string[] 
                    {
                        "/grctlext/Delay",
                        "/imgdesc/Left",
                        "/imgdesc/Top",
                        "/imgdesc/Width",
                        "/imgdesc/Height",
                        "/grctlext/Disposal",
                        "/grctlext/UserInputFlag",
                        "/grctlext/TransparencyFlag",
                        "/grctlext/TransparentColorIndex",
                        "/imgdesc/LocalColorTableFlag",
                        "/imgdesc/InterlaceFlag",
                        "/imgdesc/SortFlag",
                        "/imgdesc/LocalColorTableSize"
                    });

                    var gifFrame = new GifFrame(_compGraphics, bitmap, $"{i}", properties);
                    frames.Add(gifFrame);
                }
            }

            FramesListView.ItemsSource = frames;
            FramesListView.SelectedIndex = 0;
            ContainerInfoLisView.ItemsSource = containerProperties;
        }

        private CanvasBitmap DecodeBitmapFrame(Core.GifFrame frame)
        {
            var softwareBitmap = frame.GetSoftwareBitmap();

            SoftwareBitmap convertedBitmap;

            var pixelFormat = softwareBitmap.BitmapPixelFormat;
            var alphaMode = softwareBitmap.BitmapAlphaMode;
            if (pixelFormat == BitmapPixelFormat.Bgra8 && alphaMode == BitmapAlphaMode.Premultiplied)
            {
                convertedBitmap = softwareBitmap;
            }
            else
            {
                convertedBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            return CanvasBitmap.CreateFromSoftwareBitmap(_device, convertedBitmap);
        }

        private void FramesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var frame = ((ListView)sender).SelectedItem as GifFrame;
            if (frame != null)
            {
                MainFrameView.Width = frame.Rect.Width;
                MainFrameView.Height = frame.Rect.Height;
                MainFrameView.Fill = frame.ImageBrush;
                Canvas.SetLeft(MainFrameView, frame.Rect.X);
                Canvas.SetTop(MainFrameView, frame.Rect.Y);
            }
            else
            {
                MainFrameView.Width = 0;
                MainFrameView.Height = 0;
                MainFrameView.Fill = null;
                Canvas.SetLeft(MainFrameView, 0);
                Canvas.SetTop(MainFrameView, 0);
            }
        }

        private void FramesListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var listView = (ListView)sender;
            FrameMenuFlyout.ShowAt(listView, e.GetPosition(listView));
            _rightClickedFrame = ((FrameworkElement)e.OriginalSource).DataContext as GifFrame;
        }

        private async void CopyFrameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;

            var stream = new InMemoryRandomAccessStream();
            await _rightClickedFrame.Bitmap.SaveAsync(stream, CanvasBitmapFileFormat.Png, 1.0f);

            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
            Clipboard.SetContent(dataPackage);
        }

        private async void SaveFrameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.SuggestedFileName = $"frame{_rightClickedFrame.DisplayName}";
            picker.DefaultFileExtension = ".png";
            picker.FileTypeChoices.Add("PNG Image", new List<string> { ".png" });

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await _rightClickedFrame.Bitmap.SaveAsync(stream, CanvasBitmapFileFormat.Png, 1.0f);
                }
            }
        }

        private void FrameInfoListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var listView = (ListView)sender;
            FrameInfoMenuFlyout.ShowAt(listView, e.GetPosition(listView));
            var pair = (KeyValuePair<string, BitmapTypedValue>)((FrameworkElement)e.OriginalSource).DataContext;
            _rightClickedInfo = pair.Value.FormatValue();
        }

        private void ContainerInfoLisView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var listView = (ListView)sender;
            ContainerInfoMenuFlyout.ShowAt(listView, e.GetPosition(listView));
            var pair = (KeyValuePair<string, BitmapTypedValue>)((FrameworkElement)e.OriginalSource).DataContext;
            _rightClickedInfo = pair.Value.FormatValue();
        }

        private void CopyInfoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;

            dataPackage.SetText(_rightClickedInfo);
            Clipboard.SetContent(dataPackage);
        }

        private async void Grid_DragOver(object sender, DragEventArgs e)
        {
            var deferral = e.GetDeferral();
            bool valid = false;
            string caption = "";
            var view = e.DataView;

            // First check for storage items
            if (view.Contains(StandardDataFormats.StorageItems))
            {
                var items = await view.GetStorageItemsAsync();

                if (items.Count > 0)
                {
                    // We'll only open the first one
                    var item = items.First();
                    if (item is StorageFile file)
                    {
                        var extension = file.FileType.ToLower();
                        if (extension == ".gif")
                        {
                            valid = true;
                            caption = "Open Gif";
                        }
                    }
                }
            }
            else if (view.Contains(StandardDataFormats.Bitmap))
            {
                valid = true;
            }

            if (valid)
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = caption;
            }
            deferral.Complete();
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            var view = e.DataView;
            if (view.Contains(StandardDataFormats.StorageItems))
            {
                var items = await view.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    // We'll only open the first one
                    var item = items.First();
                    if (item is StorageFile file)
                    {
                        await OpenFileAsync(file);
                    }
                }
            }
        }
    }
}
