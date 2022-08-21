#pragma once
#include <Unknwn.h>
#include <winrt/base.h>
#include <wincodec.h>
#include <string>

namespace robmikh::common::uwp
{
    template <typename T>
    T GetMetadataByName(
        winrt::com_ptr<IWICMetadataQueryReader> const& metadataQueryReader,
        std::wstring const& propertyName);

    template <>
    inline uint16_t GetMetadataByName<uint16_t>(
        winrt::com_ptr<IWICMetadataQueryReader> const& metadataQueryReader,
        std::wstring const& propertyName)
    {
        PROPVARIANT propValue = {};
        winrt::check_hresult(metadataQueryReader->GetMetadataByName(
            propertyName.c_str(),
            &propValue));
        if (propValue.vt != VT_UI2)
        {
            throw winrt::hresult_error(E_UNEXPECTED, L"Unexpected property value type.");
        }
        return propValue.uiVal;
    }
}
