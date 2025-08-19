using Dalamud.Bindings.ImGui;
using OtterGui.Text.HelperObjects;

namespace OtterGui.Text.EndObjects;

public unsafe ref struct Id
{
    private int _counter;

    /// <inheritdoc cref="ImUtf8.PushId(int)"/>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Id Push(int id)
    {
        ++_counter;
        ImGui.PushID(id);
        return this;
    }

    /// <inheritdoc cref="ImUtf8.PushId(nint)"/>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Id Push(nint ptr)
    {
        ++_counter;
        ImGui.PushID(ptr);
        return this;
    }

    /// <inheritdoc cref="ImUtf8.PushId(ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Id Push(scoped ReadOnlySpan<byte> label)
    {
        ++_counter;
        ImGui.PushID(label);
        return this;
    }

    /// <inheritdoc cref="ImUtf8.PushId(ReadOnlySpan{char})"/>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Id Push(scoped ReadOnlySpan<char> label)
        => Push(label.Span<LabelStringHandlerBuffer>());

    /// <inheritdoc cref="ImUtf8.PushId(ref Utf8StringHandler{LabelStringHandlerBuffer})"/>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Id Push(scoped ref Utf8StringHandler<LabelStringHandlerBuffer> label)
        => Push(label.Span());

    /// <summary> Pop a number of IDs from the ID stack, but at most as many as this object pushed. </summary>
    /// <param name="count"> The number of IDs to pop. </param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public void Pop(int count = 1)
    {
        if (count > _counter)
            count = _counter;
        _counter -= count;
        while (count-- > 0)
            ImGui.PopID();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public void Dispose()
        => Pop(_counter);
}
