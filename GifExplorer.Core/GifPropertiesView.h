#pragma once
#include "GifPropertiesView.g.h"

namespace winrt::GifExplorer::Core::implementation
{
    struct GifPropertiesView : GifPropertiesViewT<GifPropertiesView>
    {
        GifPropertiesView(winrt::com_ptr<IWICMetadataQueryReader> const& wicMetadataReader) : m_wicMetadataReader(wicMetadataReader) {}

        winrt::Windows::Graphics::Imaging::BitmapPropertySet GetProperties(winrt::Windows::Foundation::Collections::IIterable<hstring> const& propertiesToRetrieve);
    
    private:
        winrt::com_ptr<IWICMetadataQueryReader> m_wicMetadataReader;
    };
}
