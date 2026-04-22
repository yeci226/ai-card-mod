using Godot;

namespace AICardMod.UI;

public sealed record TemplateCardDialogResult(
    int PrefixIndex,
    int MiddleIndex,
    int SuffixIndex,
    int PietySpend,
    bool Ethereal);

public partial class TemplateCardDialog : CanvasLayer
{
    private TaskCompletionSource<TemplateCardDialogResult?>? _tcs;
    private OptionButton _prefixOption = null!;
    private OptionButton _middleOption = null!;
    private OptionButton _suffixOption = null!;
    private HSlider _pietySlider = null!;
    private Label _pietyValueLabel = null!;
    private CheckBox _etherealCheck = null!;
    private Label _previewTitleLabel = null!;
    private Label _previewDescriptionLabel = null!;
    private Label _previewAffixLabel = null!;
    private Label _previewMetaLabel = null!;

    private Func<int, int, int, int, bool, (string title, string description, string affixText, string metaText)> _previewFactory = null!;

    public static async Task<TemplateCardDialogResult?> ShowAsync(
        string[] prefixNames,
        string[] middleNames,
        string[] suffixNames,
        int defaultPrefix,
        int defaultMiddle,
        int defaultSuffix,
        int defaultPietySpend,
        int maxPietySpend,
        bool defaultEthereal,
        Func<int, int, int, int, bool, (string title, string description, string affixText, string metaText)> previewFactory)
    {
        if (Engine.GetMainLoop() is not SceneTree tree)
        {
            GD.PrintErr("[AICard] Cannot get SceneTree for template dialog.");
            return null;
        }

        var dialog = new TemplateCardDialog();
        tree.Root.AddChild(dialog);

        dialog.Initialize(
            prefixNames,
            middleNames,
            suffixNames,
            defaultPrefix,
            defaultMiddle,
            defaultSuffix,
            defaultPietySpend,
            maxPietySpend,
            defaultEthereal,
            previewFactory);

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

    public override void _Ready()
    {
        _tcs = new TaskCompletionSource<TemplateCardDialogResult?>();
        Layer = 128;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Keycode: Key.Escape, Pressed: true })
            SubmitCancel();
    }

    private void Initialize(
        string[] prefixNames,
        string[] middleNames,
        string[] suffixNames,
        int defaultPrefix,
        int defaultMiddle,
        int defaultSuffix,
        int defaultPietySpend,
        int maxPietySpend,
        bool defaultEthereal,
        Func<int, int, int, int, bool, (string title, string description, string affixText, string metaText)> previewFactory)
    {
        _previewFactory = previewFactory;
        BuildUi(prefixNames, middleNames, suffixNames, maxPietySpend);

        _prefixOption.Select(Math.Clamp(defaultPrefix, 0, Math.Max(0, _prefixOption.ItemCount - 1)));
        _middleOption.Select(Math.Clamp(defaultMiddle, 0, Math.Max(0, _middleOption.ItemCount - 1)));
        _suffixOption.Select(Math.Clamp(defaultSuffix, 0, Math.Max(0, _suffixOption.ItemCount - 1)));
        _pietySlider.Value = Math.Clamp(defaultPietySpend, 0, Math.Max(0, maxPietySpend));
        _etherealCheck.ButtonPressed = defaultEthereal;

        RefreshPreview();
    }

    private void BuildUi(
        string[] prefixNames,
        string[] middleNames,
        string[] suffixNames,
        int maxPietySpend)
    {
        var overlay = new ColorRect { Color = new Color(0f, 0f, 0f, 0.72f) };
        overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(overlay);

        var panelContainer = new PanelContainer();
        panelContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        panelContainer.CustomMinimumSize = new Vector2(760, 0);
        panelContainer.Position = new Vector2(-380, -250);
        AddChild(panelContainer);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_top", 20);
        margin.AddThemeConstantOverride("margin_bottom", 20);
        margin.AddThemeConstantOverride("margin_left", 24);
        margin.AddThemeConstantOverride("margin_right", 24);
        panelContainer.AddChild(margin);

        var rootVBox = new VBoxContainer();
        rootVBox.AddThemeConstantOverride("separation", 12);
        margin.AddChild(rootVBox);

        var title = new Label
        {
            Text = "神諭詞條配置",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 22);
        rootVBox.AddChild(title);

        rootVBox.AddChild(new HSeparator());

        var previewPanel = new PanelContainer();
        rootVBox.AddChild(previewPanel);

        var previewMargin = new MarginContainer();
        previewMargin.AddThemeConstantOverride("margin_top", 12);
        previewMargin.AddThemeConstantOverride("margin_bottom", 12);
        previewMargin.AddThemeConstantOverride("margin_left", 14);
        previewMargin.AddThemeConstantOverride("margin_right", 14);
        previewPanel.AddChild(previewMargin);

        var previewVBox = new VBoxContainer();
        previewVBox.AddThemeConstantOverride("separation", 6);
        previewMargin.AddChild(previewVBox);

        _previewTitleLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = ""
        };
        _previewTitleLabel.AddThemeFontSizeOverride("font_size", 19);
        previewVBox.AddChild(_previewTitleLabel);

        _previewDescriptionLabel = new Label
        {
            Text = "",
            AutowrapMode = TextServer.AutowrapMode.Word
        };
        previewVBox.AddChild(_previewDescriptionLabel);

        _previewAffixLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center,
            Modulate = new Color(1f, 1f, 1f, 0.8f)
        };
        _previewAffixLabel.AddThemeFontSizeOverride("font_size", 13);
        previewVBox.AddChild(_previewAffixLabel);

        _previewMetaLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center,
            Modulate = new Color(0.9f, 0.95f, 1f, 0.9f)
        };
        _previewMetaLabel.AddThemeFontSizeOverride("font_size", 12);
        previewVBox.AddChild(_previewMetaLabel);

        var configGrid = new GridContainer { Columns = 2 };
        configGrid.AddThemeConstantOverride("h_separation", 14);
        configGrid.AddThemeConstantOverride("v_separation", 8);
        rootVBox.AddChild(configGrid);

        configGrid.AddChild(new Label { Text = "前綴" });
        _prefixOption = CreateOption(prefixNames);
        configGrid.AddChild(_prefixOption);

        configGrid.AddChild(new Label { Text = "中綴" });
        _middleOption = CreateOption(middleNames);
        configGrid.AddChild(_middleOption);

        configGrid.AddChild(new Label { Text = "後綴" });
        _suffixOption = CreateOption(suffixNames);
        configGrid.AddChild(_suffixOption);

        configGrid.AddChild(new Label { Text = "投入虔誠" });
        var pietyBox = new HBoxContainer();
        pietyBox.AddThemeConstantOverride("separation", 10);
        _pietySlider = new HSlider
        {
            MinValue = 0,
            MaxValue = Math.Max(0, maxPietySpend),
            Step = 1,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            Editable = maxPietySpend > 0
        };
        _pietyValueLabel = new Label { Text = "0", CustomMinimumSize = new Vector2(42, 0) };
        pietyBox.AddChild(_pietySlider);
        pietyBox.AddChild(_pietyValueLabel);
        configGrid.AddChild(pietyBox);

        configGrid.AddChild(new Label { Text = "虛無" });
        _etherealCheck = new CheckBox { Text = "啟用" };
        configGrid.AddChild(_etherealCheck);

        var btnRow = new HBoxContainer();
        btnRow.Alignment = BoxContainer.AlignmentMode.End;
        btnRow.AddThemeConstantOverride("separation", 10);
        rootVBox.AddChild(btnRow);

        var cancelBtn = new Button { Text = "取消" };
        cancelBtn.Pressed += SubmitCancel;
        btnRow.AddChild(cancelBtn);

        var confirmBtn = new Button { Text = "套用並生成" };
        confirmBtn.Pressed += SubmitConfirm;
        btnRow.AddChild(confirmBtn);

        _prefixOption.ItemSelected += _ => RefreshPreview();
        _middleOption.ItemSelected += _ => RefreshPreview();
        _suffixOption.ItemSelected += _ => RefreshPreview();
        _etherealCheck.Toggled += _ => RefreshPreview();
        _pietySlider.ValueChanged += _ => RefreshPreview();

        _prefixOption.GrabFocus();
    }

    private static OptionButton CreateOption(IEnumerable<string> items)
    {
        var option = new OptionButton();
        foreach (var item in items)
            option.AddItem(item);
        return option;
    }

    private void RefreshPreview()
    {
        int piety = (int)_pietySlider.Value;
        _pietyValueLabel.Text = piety.ToString();

        var preview = _previewFactory(
            _prefixOption.GetSelectedId(),
            _middleOption.GetSelectedId(),
            _suffixOption.GetSelectedId(),
            piety,
            _etherealCheck.ButtonPressed);

        _previewTitleLabel.Text = preview.title;
        _previewDescriptionLabel.Text = preview.description;
        _previewAffixLabel.Text = preview.affixText;
        _previewMetaLabel.Text = preview.metaText;
    }

    private void SubmitConfirm()
    {
        _tcs?.TrySetResult(new TemplateCardDialogResult(
            _prefixOption.GetSelectedId(),
            _middleOption.GetSelectedId(),
            _suffixOption.GetSelectedId(),
            (int)_pietySlider.Value,
            _etherealCheck.ButtonPressed));
    }

    private void SubmitCancel()
    {
        _tcs?.TrySetResult(null);
    }
}
