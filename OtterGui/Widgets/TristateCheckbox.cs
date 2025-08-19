using Dalamud.Bindings.ImGui;
using OtterGui.Text.Widget;
using OtterGuiInternal.Utility;

namespace OtterGui.Widgets;

public class TristateCheckbox(uint crossColor = 0xFF0000FF, uint checkColor = 0xFF00FF00, uint dotColor = 0xFFD0D0D0)
    : MultiStateCheckbox<sbyte>
{
    public readonly uint CrossColor = MergeAlpha(crossColor);
    public readonly uint CheckColor = MergeAlpha(checkColor);
    public readonly uint DotColor   = MergeAlpha(dotColor);

    private static uint MergeAlpha(uint color)
        => (color & 0x00FFFFFF) | ((uint)((color >> 24) * ImGui.GetStyle().Alpha) << 24);

    protected override void RenderSymbol(sbyte value, Vector2 position, float size)
    {
        switch (value)
        {
            case -1:
                SymbolHelpers.RenderCross(ImGui.GetWindowDrawList(), position, CrossColor, size);
                break;
            case 1:
                SymbolHelpers.RenderCheckmark(ImGui.GetWindowDrawList(), position, CheckColor, size);
                break;
            default:
                SymbolHelpers.RenderDot(ImGui.GetWindowDrawList(), position, DotColor, size);
                break;
        }
    }

    protected override sbyte NextValue(sbyte value)
        => value switch
        {
            0 => 1,
            1 => -1,
            _ => 0,
        };

    protected override sbyte PreviousValue(sbyte value)
        => value switch
        {
            0 => -1,
            1 => 0,
            _ => 1,
        };
}
