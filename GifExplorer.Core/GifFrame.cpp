#include "pch.h"
#include "GifFrame.h"
#include "GifPropertiesView.h"
#include "GifPalette.h"
#include "GifFrame.g.cpp"

namespace winrt
{
    using namespace Windows::Graphics::Imaging;
}

namespace util
{
    using namespace robmikh::common::uwp;
}

namespace winrt::GifExplorer::Core::implementation
{
    GifFrame::GifFrame(
        winrt::com_ptr<IWICImagingFactory2> const& wicFactory,
        winrt::com_ptr<IWICBitmapFrameDecode> const& wicFrame)
    {
        winrt::com_ptr<IWICMetadataQueryReader> frameMetadataQueryReader;
        winrt::check_hresult(wicFrame->GetMetadataQueryReader(frameMetadataQueryReader.put()));
        m_properties = winrt::make<GifPropertiesView>(frameMetadataQueryReader);

        winrt::com_ptr<IWICPalette> wicPalette;
        winrt::check_hresult(wicFactory->CreatePalette(wicPalette.put()));
        winrt::check_hresult(wicFrame->CopyPalette(wicPalette.get()));
        m_palette = winrt::make<GifPalette>(wicPalette);

        m_wicFactory = wicFactory;
        m_wicFrame = wicFrame;
    }
    winrt::SoftwareBitmap GifFrame::GetSoftwareBitmap()
    {
        winrt::com_ptr<IWICFormatConverter> wicConverter;
        winrt::check_hresult(m_wicFactory->CreateFormatConverter(wicConverter.put()));
        winrt::check_hresult(wicConverter->Initialize(
            m_wicFrame.get(),
            GUID_WICPixelFormat32bppBGRA,
            WICBitmapDitherTypeNone,
            nullptr,
            0.0,
            WICBitmapPaletteTypeCustom));

        uint32_t width = 0;
        uint32_t height = 0;
        winrt::check_hresult(wicConverter->GetSize(&width, &height));

        auto bytesPerPixel = 4;
        auto stride = width * bytesPerPixel;
        std::vector<uint8_t> bytes(stride * height, 0);
        winrt::check_hresult(wicConverter->CopyPixels(nullptr, stride, bytes.size(), bytes.data()));

        auto buffer = winrt::make<util::ComArrayBuffer>(std::move(winrt::com_array(bytes)));
        auto bitmap = winrt::SoftwareBitmap::CreateCopyFromBuffer(
            buffer, winrt::BitmapPixelFormat::Bgra8, width, height, winrt::BitmapAlphaMode::Straight);

        return bitmap;
    }
}
