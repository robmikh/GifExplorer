#include "pch.h"
#include "GifPropertiesView.h"
#include "GifPropertiesView.g.cpp"

namespace winrt
{
    using namespace Windows::Foundation;
    using namespace Windows::Foundation::Collections;
    using namespace Windows::Graphics::Imaging;
}

namespace util
{
    using namespace robmikh::common::uwp;
}

namespace winrt::GifExplorer::Core::implementation
{
    winrt::BitmapTypedValue CreateBitmapTypedValue(
        PROPVARIANT const& propValue)
    {
        winrt::IInspectable object{ nullptr };
        auto propertyType = winrt::PropertyType::Empty;

        switch (propValue.vt)
        {
        case VT_BOOL:
            object = winrt::box_value(propValue.boolVal != 0);
            propertyType = winrt::PropertyType::Boolean;
            break;
        case VT_UI1:
            object = winrt::box_value(propValue.bVal);
            propertyType = winrt::PropertyType::UInt8;
            break;
        case VT_UI1 | VT_VECTOR:
        {
            std::vector<uint8_t> values(propValue.caub.cElems, 0);
            memcpy_s(values.data(), values.size(), propValue.caub.pElems, propValue.caub.cElems);
            object = winrt::box_value(winrt::com_array(std::move(values)));
            propertyType = winrt::PropertyType::UInt8Array;
        }
            break;
        case VT_UI2:
            object = winrt::box_value(propValue.uiVal);
            propertyType = winrt::PropertyType::UInt16;
            break;
        case VT_LPSTR:
        {
            std::string string(propValue.pszVal);
            std::wstring wstring(string.begin(), string.end());
            object = winrt::box_value(winrt::hstring(wstring));
            propertyType = winrt::PropertyType::String;
        }
            break;
        default:
            throw winrt::hresult_error(E_UNEXPECTED, L"Unexpected property value type.");
        }

        return winrt::BitmapTypedValue(object, propertyType);
    }

    winrt::BitmapPropertySet GifPropertiesView::GetProperties(winrt::IIterable<hstring> const& propertiesToRetrieve)
    {
        auto properties = winrt::BitmapPropertySet();
        for (auto&& propertyName : propertiesToRetrieve)
        {
            wil::unique_prop_variant propValue;
            HRESULT hr = m_wicMetadataReader->GetMetadataByName(
                propertyName.c_str(),
                propValue.addressof());
            if (SUCCEEDED(hr))
            {
                auto value = CreateBitmapTypedValue(propValue);

                properties.Insert(propertyName, value);
            }
            else if (hr == WINCODEC_ERR_PROPERTYNOTFOUND)
            {
                // Skip the property
            }
            else
            {
                winrt::check_hresult(hr);
            }
        }
        return properties;
    }
}
