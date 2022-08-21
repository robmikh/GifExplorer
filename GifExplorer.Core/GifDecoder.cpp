#include "pch.h"
#include "GifDecoder.h"
#include "GifPropertiesView.h"
#include "GifFrame.h"
#include "GifDecoder.g.cpp"

namespace util
{
    using namespace robmikh::common::uwp;
}

namespace winrt::GifExplorer::Core::implementation
{
    GifDecoder::GifDecoder(winrt::Windows::Storage::Streams::IRandomAccessStream const& stream)
    {
        m_stream = util::CreateStreamFromRandomAccessStream(stream);

        // Create WIC Decoder
        m_wicFactory = winrt::create_instance<IWICImagingFactory2>(CLSID_WICImagingFactory2, CLSCTX_INPROC_SERVER);
        winrt::check_hresult(m_wicFactory->CreateDecoder(GUID_ContainerFormatGif, nullptr, m_wicDecoder.put()));
        winrt::check_hresult(m_wicDecoder->Initialize(m_stream.get(), WICDecodeMetadataCacheOnLoad));

        // Read properties
        winrt::com_ptr<IWICMetadataQueryReader> metadataQueryReader;
        winrt::check_hresult(m_wicDecoder->GetMetadataQueryReader(
            metadataQueryReader.put()));
        
        m_wicDecoder->GetFrameCount(&m_frameCount);
        m_width = util::GetMetadataByName<uint16_t>(
            metadataQueryReader,
            L"/logscrdesc/Width");
        m_height = util::GetMetadataByName<uint16_t>(
            metadataQueryReader,
            L"/logscrdesc/Height");

        // Creat container property view
        m_containerProperties = winrt::make<GifPropertiesView>(metadataQueryReader);
    }
    winrt::GifExplorer::Core::GifFrame implementation::GifDecoder::GetFrame(uint32_t index)
    {
        winrt::com_ptr<IWICBitmapFrameDecode> wicFrame;
        winrt::check_hresult(m_wicDecoder->GetFrame(index, wicFrame.put()));
        return winrt::make<GifFrame>(m_wicFactory, wicFrame);
    }
    void GifDecoder::Close()
    {
        throw hresult_not_implemented();
    }
}
