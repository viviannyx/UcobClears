using Dalamud.Bindings.ImGui;
using OtterGui.Text.HelperObjects;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace OtterGui.Text;

public static partial class ImUtf8
{
    /// <summary> Draw text with an additional label as a single item. </summary>
    /// <param name="label"> The text label as a UTF8 string. HAS to be null-terminated. </param>
    /// <param name="text"> The text itself as a UTF8 string. HAS to be null-terminated. </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LabelText(ReadOnlySpan<byte> label, ReadOnlySpan<byte> text)
        => ImGui.LabelText(label, text);

    /// <param name="text"> The text itself as a UTF16 string. HAS to be null-terminated. </param>
    /// <inheritdoc cref="LabelText(ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LabelText(ReadOnlySpan<byte> label, ReadOnlySpan<char> text)
        => LabelText(label, text.Span<TextStringHandlerBuffer>());

    /// <param name="text"> The text itself as a formatted string. HAS to be null-terminated. </param>
    /// <inheritdoc cref="LabelText(ReadOnlySpan{byte},ReadOnlySpan{char})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LabelText(ReadOnlySpan<byte> label, ref Utf8StringHandler<TextStringHandlerBuffer> text)
        => LabelText(label, text.Span());


    /// <param name="label"> The text label as a UTF16 string. </param>
    /// <inheritdoc cref="LabelText(ReadOnlySpan{byte},ReadOnlySpan{byte})"/>
    /// <exception cref="ImUtf8FormatException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LabelText(ReadOnlySpan<char> label, ReadOnlySpan<byte> text)
        => LabelText(label.Span<LabelStringHandlerBuffer>(), text);

    /// <param name="label"> The text label as a UTF16 string. </param>
    /// <inheritdoc cref="LabelText(ReadOnlySpan{byte},ReadOnlySpan{char})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LabelText(ReadOnlySpan<char> label, ReadOnlySpan<char> text)
        => LabelText(label.Span<LabelStringHandlerBuffer>(), text.Span<TextStringHandlerBuffer>());

    /// <param name="label"> The text label as a UTF16 string. </param>
    /// <inheritdoc cref="LabelText(ReadOnlySpan{byte}, ref Utf8StringHandler{TextStringHandlerBuffer})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LabelText(ReadOnlySpan<char> label, ref Utf8StringHandler<TextStringHandlerBuffer> text)
        => LabelText(label.Span<LabelStringHandlerBuffer>(), text.Span());


    /// <param name="label"> The text label as a formatted string. </param>
    /// <inheritdoc cref="LabelText(ReadOnlySpan{char},ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LabelText(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<byte> text)
        => LabelText(label.Span(), text);

    /// <param name="label"> The text label as a formatted string. </param>
    /// <inheritdoc cref="LabelText(ReadOnlySpan{char},ReadOnlySpan{char})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LabelText(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ReadOnlySpan<char> text)
        => LabelText(label.Span(), text.Span<TextStringHandlerBuffer>());

    /// <param name="label"> The text label as a formatted string. </param>
    /// <inheritdoc cref="LabelText(ReadOnlySpan{char}, ref Utf8StringHandler{TextStringHandlerBuffer})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LabelText(ref Utf8StringHandler<LabelStringHandlerBuffer> label, ref Utf8StringHandler<TextStringHandlerBuffer> text)
        => LabelText(label.Span(), text.Span());
}
