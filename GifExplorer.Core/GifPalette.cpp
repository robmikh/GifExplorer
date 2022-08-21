#include "pch.h"
#include "GifPalette.h"
#include "GifPalette.g.cpp"

namespace winrt
{
    using namespace Windows::UI;
}

namespace winrt::GifExplorer::Core::implementation
{
    GifPalette::GifPalette(winrt::com_ptr<IWICPalette> const& wicPalette)
    {
        uint32_t numColors = 0;
        winrt::check_hresult(wicPalette->GetColorCount(&numColors));
        std::vector<WICColor> wicColors(numColors, 0);
        winrt::check_hresult(wicPalette->GetColors(numColors, wicColors.data(), &numColors));

        std::vector<winrt::Color> colors;
        for (auto&& wicColor : wicColors)
        {
            auto alpha = (0xFF000000 & wicColor) >> 24;
            auto red = (0x00FF0000 & wicColor) >> 16;
            auto green = (0x0000FF00 & wicColor) >> 8;
            auto blue = 0x000000FF & wicColor;

            colors.push_back(winrt::Color
                {
                    static_cast<uint8_t>(alpha), 
                    static_cast<uint8_t>(red),
                    static_cast<uint8_t>(green),
                    static_cast<uint8_t>(blue)
                });
        }
        m_colors = winrt::single_threaded_vector(std::move(colors));

        BOOL hasAlpha = FALSE;
        winrt::check_hresult(wicPalette->HasAlpha(&hasAlpha));
        m_hasAlpha = !!hasAlpha;
    }
}
