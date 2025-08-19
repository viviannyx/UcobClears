﻿using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace ECommons.ImGuiMethods;
public abstract class EzFullscreenOverlayWindow : Window
{
    public EzFullscreenOverlayWindow(string name) : base(name, ImGuiEx.OverlayFlags, true)
    {
        RespectCloseHotkey = false;
        IsOpen = true;
    }

    public virtual void PreDrawAction() { }

    public sealed override void PreDraw()
    {
        PreDrawAction();
        ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero);
    }
}
