#pragma once
#include "GifFrame.g.h"

namespace winrt::GifExplorer::Core::implementation
{
    struct GifFrame : GifFrameT<GifFrame>
    {
        GifFrame(
            winrt::com_ptr<IWICImagingFactory2> const& wicFactory,
            winrt::com_ptr<IWICBitmapFrameDecode> const& wicFrame);

        winrt::GifExplorer::Core::GifPropertiesView BitmapProperties() { return m_properties; }
        winrt::GifExplorer::Core::GifPalette Palette() { return m_palette; }
        winrt::Windows::Graphics::Imaging::SoftwareBitmap GetSoftwareBitmap();

    private:
        winrt::GifExplorer::Core::GifPalette m_palette{ nullptr };
        winrt::GifExplorer::Core::GifPropertiesView m_properties{ nullptr };
        winrt::com_ptr<IWICImagingFactory2> m_wicFactory;
        winrt::com_ptr<IWICBitmapFrameDecode> m_wicFrame;
    };
}
