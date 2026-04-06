using Godot;
using MegaCrit.Sts2.Core.Localization;

namespace AICardMod.UI;

/// <summary>
/// A modal input dialog that asks the player what they want the AI Card to become.
/// Call ShowAsync() to display it and await the player's text input.
/// </summary>
public partial class AiInputDialog : CanvasLayer
{
    private TaskCompletionSource<string>? _tcs;
    private LineEdit _lineEdit = null!;

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Show the dialog. Returns the player's input, or empty string if dismissed.
    /// </summary>
    public static async Task<string> ShowAsync()
    {
        if (Engine.GetMainLoop() is not SceneTree tree)
        {
            GD.PrintErr("[AICard] Cannot get SceneTree — dialog skipped");
            return "";
        }

        var dialog = new AiInputDialog();
        tree.Root.AddChild(dialog);

        try
        {
            return await dialog._tcs!.Task;
        }
        finally
        {
            if (IsInstanceValid(dialog))
                dialog.QueueFree();
        }
    }

    // ── Godot lifecycle ─────────────────────────────────────────────────────

    public override void _Ready()
    {
        _tcs = new TaskCompletionSource<string>();
        Layer = 128; // render on top of everything

        _BuildUI();
    }

    public override void _Input(InputEvent @event)
    {
        // Press Escape to cancel (submit empty → caller treats as cancel)
        if (@event is InputEventKey { Keycode: Key.Escape, Pressed: true })
        {
            _Submit("");
        }
    }

    // ── UI construction ──────────────────────────────────────────────────────

    private void _BuildUI()
    {
        // Semi-transparent black overlay
        var overlay = new ColorRect
        {
            Color = new Color(0f, 0f, 0f, 0.72f)
        };
        overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(overlay);

        // ── Centered panel ──────────────────────────────────────────────────
        var panelContainer = new PanelContainer();
        panelContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        panelContainer.CustomMinimumSize = new Vector2(620, 0);
        panelContainer.Position = new Vector2(-310, -140);
        AddChild(panelContainer);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_top", 24);
        margin.AddThemeConstantOverride("margin_bottom", 24);
        margin.AddThemeConstantOverride("margin_left", 28);
        margin.AddThemeConstantOverride("margin_right", 28);
        panelContainer.AddChild(margin);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 14);
        margin.AddChild(vbox);

        // Title — localized
        var title = new Label
        {
            Text = new LocString("cards", "AICARDMOD-UI_DIALOG.title").GetFormattedText(),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 22);
        vbox.AddChild(title);

        // Separator line
        vbox.AddChild(new HSeparator());

        // Battle stats row (kept as placeholder)
        vbox.AddChild(_BattleStatsLabel());

        // Instructions — localized
        var hint = new Label
        {
            Text = new LocString("cards", "AICARDMOD-UI_DIALOG.instructions").GetFormattedText(),
            AutowrapMode = TextServer.AutowrapMode.Word,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        hint.AddThemeFontSizeOverride("font_size", 14);
        vbox.AddChild(hint);

        // Text input — localized placeholder
        _lineEdit = new LineEdit
        {
            PlaceholderText = new LocString("cards", "AICARDMOD-UI_DIALOG.input_placeholder").GetFormattedText(),
            CaretBlink = true,
            ExpandToTextLength = false
        };
        _lineEdit.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(_lineEdit);

        // Confirm button — localized
        var btn = new Button
        {
            Text = new LocString("cards", "AICARDMOD-UI_DIALOG.button").GetFormattedText()
        };
        btn.AddThemeFontSizeOverride("font_size", 15);
        btn.Pressed += () => _Submit(_lineEdit.Text.Trim());
        vbox.AddChild(btn);

        // Cancel hint — localized
        var cancel = new Label
        {
            Text = new LocString("cards", "AICARDMOD-UI_DIALOG.cancel").GetFormattedText(),
            HorizontalAlignment = HorizontalAlignment.Center,
            Modulate = new Color(1, 1, 1, 0.45f)
        };
        cancel.AddThemeFontSizeOverride("font_size", 12);
        vbox.AddChild(cancel);

        // Enter key submits
        _lineEdit.TextSubmitted += text => _Submit(text.Trim());

        _lineEdit.GrabFocus();
    }

    private static Label _BattleStatsLabel()
    {
        // Placeholder — actual stats are set after _Ready via SetBattleStats()
        return new Label
        {
            Name = "StatsLabel",
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center,
            Modulate = new Color(0.8f, 0.9f, 1f, 0.85f)
        };
    }

    /// <summary>Optionally inject live battle context into the dialog.</summary>
    public void SetBattleStats(string stats)
    {
        if (GetNodeOrNull<Label>("StatsLabel") is { } lbl)
            lbl.Text = stats;
    }

    // ── Public awaitable ─────────────────────────────────────────────────────

    /// <summary>
    /// Await this after adding the dialog to the scene tree to get the player's input.
    /// </summary>
    public Task<string> GetInputAsync() => _tcs!.Task;

    // ── Submit ───────────────────────────────────────────────────────────────

    private void _Submit(string text)
    {
        _tcs?.TrySetResult(text);
    }
}
