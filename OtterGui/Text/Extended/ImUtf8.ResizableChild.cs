using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using OtterGui.Extensions;
using OtterGui.Text.EndObjects;
using OtterGui.Text.HelperObjects;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace OtterGui.Text;

public static unsafe partial class ImUtf8
{
    /// <summary> Create a bordered child that can be resized in positive X- or Y-direction. </summary>
    /// <param name="label"> The ID of the child as a UTF8 string. HAS to be null-terminated. </param>
    /// <param name="size"> The desired current size of the child. </param>
    /// <param name="currentSize"> The returned current size of the child. </param>
    /// <param name="setSize"> The function to invoke when the size of the child finalizes a change. </param>
    /// <param name="minSize"> The minimum size of the child. </param>
    /// <param name="maxSize"> The maximum size of the child. </param>
    /// <param name="resizeX"> Whether to allow resizing in X-direction. </param>
    /// <param name="resizeY"> Whether to allow resizing in Y-direction. </param>
    /// <param name="flags"> Additional flags for the child. </param>
    /// <returns> A disposable object that evaluates to true if any part of the begun child is currently visible. Use with using. </returns>
    public static Child ResizableChild(ReadOnlySpan<byte> label, Vector2 size, out Vector2 currentSize, Action<Vector2> setSize,
        Vector2 minSize, Vector2 maxSize,
        ImGuiWindowFlags flags = default, bool resizeX = true, bool resizeY = false)
    {
        // Work in the child ID.
        using var idStack = PushId(label);

        // Use two IDs to store state and current resizing value if any.
        var     stateId = GetId("####state"u8);
        var     valueId = GetId("####value"u8);
        ref var state   = ref ImGui.GetStateStorage().GetIntRef(stateId, 0);
        ref var value   = ref ImGui.GetStateStorage().GetFloatRef(valueId, 0f);
        currentSize = state switch
        {
            1 => size with { X = value },
            2 => size with { Y = value },
            _ => size,
        };

        // Fix border width, use regular color and rounding style.
        const float borderWidth     = 1f;
        const float halfBorderWidth = borderWidth / 2f;
        var         borderColor     = ImGui.GetColorU32(ImGuiCol.Border);
        var         rounding        = ImGui.GetStyle().ChildRounding;
        var         onlyInner       = rounding is 0 ? borderWidth : rounding;
        var         hoverExtend     = 5f * GlobalScale;
        const float delay           = 0.1f;

        var rectMin  = ImGui.GetCursorScreenPos() + new Vector2(halfBorderWidth);
        var rectMax  = (ImGui.GetCursorScreenPos() + size).Round();
        var drawList = ImGui.GetWindowDrawList();

        // If resizing in X direction is allowed, handle it.
        if (resizeX)
        {
            var id = GetId("####x"u8);
            // Behaves as a splitter, so second size is the remainder.
            var sizeInc      = size.X;
            var sizeDec      = ImGui.GetContentRegionAvail().X - size.X;
            var remainderMin = ImGui.GetContentRegionAvail().X - maxSize.X;

            using var color = ImRaii.PushColor(ImGuiCol.Separator, borderColor);
            var rect = new ImRect(new Vector2(rectMax.X - halfBorderWidth, rectMin.Y + onlyInner),
                new Vector2(rectMax.X + halfBorderWidth,                   rectMax.Y - onlyInner));
            if (ImGuiP.SplitterBehavior(rect, id, ImGuiAxis.X, &sizeInc, &sizeDec, minSize.X, remainderMin, hoverExtend, delay, 0))
            {
                // Update internal state.
                value   = sizeInc;
                size    = size with { X = sizeInc };
                rectMax = (ImGui.GetCursorScreenPos() + size).Round();
                state   = 1;
            }

            if (ImGui.IsItemDeactivated())
            {
                // Handle updating on deactivation only.
                state = 0;
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    size.X  = value;
                    rectMax = (ImGui.GetCursorScreenPos() + size).Round();
                    setSize(size);
                }
            }
        }

        if (resizeY)
        {
            // Same as X just for the other direction. Y takes priority in length.
            var id = GetId("####y"u8);

            var sizeInc      = size.Y;
            var sizeDec      = ImGui.GetContentRegionAvail().Y - size.Y;
            var remainderMin = ImGui.GetContentRegionAvail().Y - maxSize.Y;

            using var color = ImRaii.PushColor(ImGuiCol.Separator, borderColor);
            var rect = new ImRect(new Vector2(rectMin.X + onlyInner, rectMax.Y - halfBorderWidth),
                new Vector2(rectMax.X - onlyInner,                   rectMax.Y + halfBorderWidth));
            if (ImGuiP.SplitterBehavior(rect, id, ImGuiAxis.Y, &sizeInc, &sizeDec, minSize.X, remainderMin, hoverExtend, delay, 0))
            {
                value   = sizeInc;
                size    = size with { Y = sizeInc };
                rectMax = (ImGui.GetCursorScreenPos() + size).Round();
                state   = 2;
            }

            if (ImGui.IsItemDeactivated())
            {
                state = 0;
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    size.Y  = value;
                    rectMax = (ImGui.GetCursorScreenPos() + size).Round();
                    setSize(size);
                }
            }
        }

        if (rounding is 0)
        {
            // With no rounding, simply draw the lines not dealt with by the resizable lines.
            if (!resizeX)
                drawList.PathLineTo(new Vector2(rectMax.X, rectMax.Y));
            drawList.PathLineTo(new Vector2(rectMax.X, rectMin.Y));
            drawList.PathLineTo(rectMin);
            drawList.PathLineTo(new Vector2(rectMin.X, rectMax.Y));
            if (!resizeY)
                drawList.PathLineTo(rectMax);
            drawList.PathStroke(borderColor, ImDrawFlags.None, borderWidth);
            if (resizeX && resizeY)
                drawList.AddRectFilled(rectMax - new Vector2(halfBorderWidth), rectMax + new Vector2(halfBorderWidth), borderColor);
        }
        else
        {
            // Otherwise, draw all required arcs and lines.
            var centerTopRight    = new Vector2(rectMax.X - rounding, rectMin.Y + rounding);
            var centerTopLeft     = new Vector2(rectMin.X + rounding, centerTopRight.Y);
            var centerBottomRight = new Vector2(centerTopRight.X,     rectMax.Y - rounding);
            var centerBottomLeft  = new Vector2(centerTopLeft.X,      centerBottomRight.Y);
            if (!resizeX)
                drawList.PathArcToFast(centerBottomRight, rounding, 3, 0);
            drawList.PathArcToFast(centerTopRight,   rounding, 12, 9);
            drawList.PathArcToFast(centerTopLeft,    rounding, 9,  6);
            drawList.PathArcToFast(centerBottomLeft, rounding, 6,  3);
            if (resizeY)
            {
                drawList.PathStroke(borderColor, ImDrawFlags.None, borderWidth);
                if (resizeX)
                {
                    drawList.PathArcToFast(centerBottomRight, rounding, 3, 0);
                    drawList.PathStroke(borderColor, ImDrawFlags.None, borderWidth);
                }
            }
            else if (!resizeX)
            {
                drawList.PathStroke(borderColor, ImDrawFlags.Closed, borderWidth);
            }
            else
            {
                drawList.PathArcToFast(centerBottomRight, rounding, 3, 0);
                drawList.PathStroke(borderColor, ImDrawFlags.None, borderWidth);
            }
        }

        idStack.Pop();
        using var c = ImRaii.PushColor(ImGuiCol.Border, 0);
        return Child(label, size, true, flags);
    }

    /// <param name="label"> The ID of the child as a UTF16 string. </param>
    /// <inheritdoc cref="ResizableChild(ReadOnlySpan{byte},Vector2, out Vector2,Action{Vector2},Vector2,Vector2,ImGuiWindowFlags,bool,bool)"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Child ResizableChild(ReadOnlySpan<char> label, Vector2 size, out Vector2 currentSize, Action<Vector2> setSize,
        Vector2 minSize, Vector2 maxSize,
        ImGuiWindowFlags flags = default, bool resizeX = true, bool resizeY = false)
        => ResizableChild(label.Span<LabelStringHandlerBuffer>(), size, out currentSize, setSize, minSize, maxSize, flags, resizeX, resizeY);

    /// <param name="label"> The ID of the child as a format string. </param>
    /// <inheritdoc cref="ResizableChild(ReadOnlySpan{char},Vector2,out Vector2,Action{Vector2},Vector2,Vector2,ImGuiWindowFlags,bool,bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Child ResizableChild(ref Utf8StringHandler<LabelStringHandlerBuffer> label, Vector2 size, out Vector2 currentSize,
        Action<Vector2> setSize,
        Vector2 minSize, Vector2 maxSize, ImGuiWindowFlags flags = default, bool resizeX = true, bool resizeY = false)
        => ResizableChild(label.Span(), size, out currentSize, setSize, minSize, maxSize, flags, resizeX, resizeY);
}
