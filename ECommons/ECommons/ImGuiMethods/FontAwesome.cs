﻿using Dalamud.Interface;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace ECommons.ImGuiMethods;
public static class FontAwesome
{
    public static readonly string Save = "\uf0c7";
    public static readonly string Cross = "\uf00d";
    public static readonly string Check = "\uF00C";
    public static readonly string Info = "\uf05a";
    public static readonly string Trash = "\uF1F8";
    public static readonly string Plus = "\uF067";
    public static readonly string Layers = "\uf5fd";

    public static void Print(Vector4? col, string icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(col, icon);
        ImGui.PopFont();
    }
    public static void PrintV(Vector4? col, string icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.TextV(col, icon);
        ImGui.PopFont();
    }

    public static void Print(Vector4? col, FontAwesomeIcon icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(col, icon.ToIconString());
        ImGui.PopFont();
    }

    public static void PrintV(Vector4? col, FontAwesomeIcon icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.TextV(col, icon.ToIconString());
        ImGui.PopFont();
    }
}
