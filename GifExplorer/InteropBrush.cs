using Windows.UI.Composition;
using Windows.UI.Xaml.Media;

namespace GifExplorer
{
    class InteropBrush : XamlCompositionBrushBase
    {
        public InteropBrush(CompositionBrush brush)
        {
            CompositionBrush = brush;
        }

        public void SetBrush(CompositionBrush brush)
        {
            CompositionBrush = brush;
        }
    }
}
