// ----------------------------------------------------------------------------
// The MIT License
// Ugui bindings https://github.com/Leopotam/ecslite-unity-ugui
// for LeoECS Lite https://github.com/Leopotam/ecslite
// Copyright (c) 2021 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using TMPro;

namespace Leopotam.EcsLite.Unity.Ugui {
    public struct EcsUguiTmpDropdownChangeEvent {
        public string WidgetName;
        public TMP_Dropdown Sender;
        public int Value;
    }
}