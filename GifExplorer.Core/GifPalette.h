#pragma once
#include "GifPalette.g.h"

namespace winrt::GifExplorer::Core::implementation
{
    struct GifPalette : GifPaletteT<GifPalette>
    {
        GifPalette(winrt::com_ptr<IWICPalette> const& wicPalette);

        winrt::Windows::Foundation::Collections::IVectorView<winrt::Windows::UI::Color> Colors() { return m_colors.GetView(); }
        bool HasAlpha() { return m_hasAlpha; }

    private:
        winrt::Windows::Foundation::Collections::IVector<winrt::Windows::UI::Color> m_colors;
        bool m_hasAlpha = false;
    };
}
