using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace AICardMod.Scripts;

public class ProphetCharacter : PlaceholderCharacterModel
{
    // ── 外觀 ────────────────────────────────────────────────────────────────

    // 角色名稱顏色（金色）
    public override Color NameColor => new(1.0f, 0.84f, 0.2f);

    // 能量數字外框顏色（深金）
    public override Color EnergyLabelOutlineColor => new(0.5f, 0.35f, 0.0f);

    // ── 基本設定 ─────────────────────────────────────────────────────────────

    // 少女（女性）
    public override CharacterGender Gender => CharacterGender.Feminine;

    // 初始 HP
    public override int StartingHp => 70;

    // ── 場景路徑 ─────────────────────────────────────────────────────────────

    // 戰鬥立繪（暫用 icon.svg，之後替換為真實圖片）
    public override string CustomVisualPath => "res://aiCardMod/scenes/prophet_character.tscn";

    // 能量表盤
    public override string CustomEnergyCounterPath => "res://aiCardMod/scenes/prophet_energy_counter.tscn";

    // 角色選擇頁頭像
    public override string? CustomIconTexturePath => "res://images/char_icon_prophet.png";

    // 人物頭像2號
    public override string CustomIconPath => "res://aiCardMod/scenes/char_icon_prophet.tscn";

    // 角色選擇背景
    public override string CustomCharacterSelectBg => "res://aiCardMod/scenes/prophet_bg.tscn";

    // 角色選擇圖示
    public override string? CustomCharacterSelectIconPath => "res://images/char_select_prophet.png";

    // 角色選擇圖示（鎖定版）
    public override string? CustomCharacterSelectLockedIconPath => "res://images/char_select_prophet_locked.png";

    // 過渡音效（必填，暫用 ironclad）
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    // ── 池子 ─────────────────────────────────────────────────────────────────

    public override CardPoolModel CardPool => ModelDb.CardPool<ProphetCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<ProphetRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<ProphetPotionPool>();

    // ── 起始卡組（5x 神聖打擊 + 4x 虔誠防禦 + 1x 祈禱） ───────────────────

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<HolyStrikeCard>(),
        ModelDb.Card<HolyStrikeCard>(),
        ModelDb.Card<HolyStrikeCard>(),
        ModelDb.Card<HolyStrikeCard>(),
        ModelDb.Card<HolyStrikeCard>(),
        ModelDb.Card<PiousDefendCard>(),
        ModelDb.Card<PiousDefendCard>(),
        ModelDb.Card<PiousDefendCard>(),
        ModelDb.Card<PiousDefendCard>(),
        ModelDb.Card<PrayCard>(),
    ];

    // ── 起始遺物（神諭石板） ──────────────────────────────────────────────────

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<OracleTabletRelic>(),
    ];

    // ── 建築師 Boss 攻擊特效 ──────────────────────────────────────────────────

    public override List<string> GetArchitectAttackVfx() =>
    [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter",
    ];
}
