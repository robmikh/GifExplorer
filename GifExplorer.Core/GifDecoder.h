#pragma once
#include "GifDecoder.g.h"

namespace winrt::GifExplorer::Core::implementation
{
    struct GifDecoder : GifDecoderT<GifDecoder>
    {
        GifDecoder(winrt::Windows::Storage::Streams::IRandomAccessStream const& stream);

        uint32_t PixelWidth() { return m_width; }
        uint32_t PixelHeight() { return m_height; }
        uint32_t FrameCount() { return m_frameCount; }
        winrt::GifExplorer::Core::GifPropertiesView BitmapContainerProperties() { return m_containerProperties; }

        winrt::GifExplorer::Core::GifFrame GetFrame(uint32_t index);

        void Close();

    private:
        uint32_t m_width = 0;
        uint32_t m_height = 0;
        uint32_t m_frameCount = 0;
        winrt::com_ptr<IStream> m_stream;
        winrt::com_ptr<IWICImagingFactory2> m_wicFactory;
        winrt::com_ptr<IWICBitmapDecoder> m_wicDecoder;
        winrt::GifExplorer::Core::GifPropertiesView m_containerProperties{ nullptr };
    };
}
namespace winrt::GifExplorer::Core::factory_implementation
{
    struct GifDecoder : GifDecoderT<GifDecoder, implementation::GifDecoder>
    {
    };
}
