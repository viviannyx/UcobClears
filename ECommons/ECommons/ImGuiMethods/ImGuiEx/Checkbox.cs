﻿using Dalamud.Bindings.ImGui;
using System;

namespace ECommons.ImGuiMethods;
public static unsafe partial class ImGuiEx
{
    /// <summary>
    /// <see cref="ImGui.Checkbox"/> that has bullet marker instead of normal check mark when enabled.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool CheckboxBullet(string label, ref bool value)
    {
        var flags = value ? 1 : 0;
        if(ImGui.CheckboxFlags(label, ref flags, int.MaxValue))
        {
            value = !value;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Inverted <see cref="ImGui.Checkbox"/>
    /// </summary>
    /// <param name="label"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool CheckboxInverted(string label, ref bool value)
    {
        var inv = !value;
        if(ImGui.Checkbox(label, ref inv))
        {
            value = !inv;
            return true;
        }
        return false;
    }

    /// <summary>
    /// <see cref="ImGui.Checkbox"/> that accepts int as a bool. 0 is false, 1 is true.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool Checkbox(string label, ref int value)
    {
        var b = value != 0;
        if(ImGui.Checkbox(label, ref b))
        {
            value = b ? 1 : 0;
            return true;
        }
        return false;
    }

    public static bool Checkbox<T>(string id, ref T? value, T? defaultValue = null) where T : struct
    {
        var enabled = value != null;
        if(ImGui.Checkbox(id, ref enabled))
        {
            value = enabled ? defaultValue ?? default : null;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Tri-way <see cref="ImGui.Checkbox"/>. Null will be displayed as a bullet. Switching order: false -> null -> true.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool Checkbox(string label, ref bool? value)
    {
        if(value != null)
        {
            var b = value.Value;
            if(ImGui.Checkbox(label, ref b))
            {
                if(b)
                {
                    value = null;
                }
                else
                {
                    value = false;
                }
                return true;
            }
        }
        else
        {
            var b = true;
            if(ImGuiEx.CheckboxBullet(label, ref b))
            {
                value = true;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Draws a checkbox that will be on the same line as previous if there is space, otherwise will move to the next line.
    /// </summary>
    /// <param name="label">Checkbox label</param>
    /// <param name="v">Boolean to toggle</param>
    /// <remarks><see cref="ImGui.SameLine()"/> does not need to be called just before using this.</remarks>
    /// <returns></returns>
    public static bool CheckboxWrapped(string label, ref bool v)
    {
        ImGui.SameLine();
        var labelW = ImGui.CalcTextSize(label);
        var finishPos = ImGui.GetCursorPosX() + labelW.X + ImGui.GetStyle().ItemSpacing.X + ImGui.GetStyle().ItemInnerSpacing.X + ImGui.GetStyle().FramePadding.Length() + ImGui.GetCursorStartPos().X;
        if(finishPos >= ImGui.GetContentRegionMax().X)
            ImGui.NewLine();

        return ImGui.Checkbox(label, ref v);
    }
}
