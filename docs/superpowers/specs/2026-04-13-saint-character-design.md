# 聖女角色實作白皮書

**日期：** 2026-04-13  
**版本：** 1.0  
**範圍：** 全 80 張卡牌、3 大核心機制（信仰、業力、受難）、FaithManager、遺物

---

## 一、系統架構總覽

```
FaithManager.CheckAndTriggerRevelation()
        │
        ├─ 讀取 FaithPower（信仰層數）
        ├─ while faith >= 10:
        │       RevelationPower += 1           ← 先計數
        │       if DivineSummonPower exists:
        │           faith -= 10               ← 不減半時固定消耗
        │       else:
        │           faith = floor(faith / 2)  ← 正常減半
        │       DamageCmd.NonAttack(6) ALL    ← 啟示 AoE（非攻擊傷害）
        │       PlayerCmd.GainEnergy(1)
        └─ 結束

傷害攔截優先級（高 → 低）：
  [1] Block（引擎原生）
  [2] ForbiddenIconPower  → 穿透格擋的傷害改為 GainBlock
  [3] KarmaTransferPower  → 剩餘傷害轉 KarmaPower，本回合末結算
  [4] HpLostTrackerPower  → 在 OnModifyDamageTaken 計入（方案A：業力轉化亦計入）

KarmaPower 回合末結算：
  BeforeTurnEnd → CreatureCmd.LoseHP(owner, KarmaPower) → KarmaPower = 0
```

---

## 二、核心 Power 規格

### 2.1 FaithPower（信仰）

| 屬性 | 值 |
|---|---|
| Type | `PowerType.Buff` |
| StackType | `PowerStackType.Counter` |
| 衰退 | 無 |
| 職責 | 純數值儲存，不自行觸發啟示 |
| 規則 | 每次 Apply 後，**呼叫方**負責呼叫 `FaithManager.CheckAndTriggerRevelation()` |

---

### 2.2 RevelationPower（啟示）

| 屬性 | 值 |
|---|---|
| Type | `PowerType.Buff` |
| StackType | `PowerStackType.Counter` |
| 移除 | 不可移除（`CanBeRemoved = false`）|
| 衰退 | 無 |
| 職責 | 計啟示次數，供牌讀取（光輝連擊等）|
| 描述 | 「本場戰鬥已引發 {Amount} 次啟示。下一次啟示將對全體敵人造成 6 點傷害。」|

---

### 2.3 KarmaPower（業力）

| 屬性 | 值 |
|---|---|
| Type | `PowerType.Buff` |
| StackType | `PowerStackType.Counter` |
| 職責 | 儲存「已轉移但未結算」的傷害量 |
| 結算 | `BeforeTurnEnd`：若無 KarmaTransferPower 保護，`CreatureCmd.LoseHP → 清零` |

---

### 2.4 HpLostTrackerPower（傷害追蹤）

| 屬性 | 值 |
|---|---|
| 職責 | 追蹤本回合與本場戰鬥的 HP 損失（含業力轉化量，方案A）|
| `ThisTurnTotal` | `OnTurnStart` 重置為 0 |
| `ThisCombatTotal` | 全場累積，不重置 |
| 掛載時機 | `OnModifyDamageTaken`（先於實際扣血）|

---

### 2.5 特定牌用 Power（快速列表）

| Power 類別 | 服務的牌 | 核心行為 |
|---|---|---|
| `SaintLegacyPower` | Card 22 | `AfterPlayerTurnStart` 獲 1/2 信仰 |
| `DivineSummonPower` | Card 20 | FaithManager 跳過減半邏輯 |
| `MartyrdomPower` | Card 37 | `OnHpLost` 對隨機敵人造成 10/14 傷 |
| `ThornTrialPower` | Card 46 | `OnAttacked` 給敵人 3/5 反傷層 |
| `SoulLinkPower` | Card 48 | `OnHpLost` 對連結目標造成等量傷 |
| `KarmaTransferPower` | Card 31 | `OnModifyDamageTaken` → KarmaPower |
| `ForbiddenIconPower` | Card 42 | `OnModifyDamageTaken` → GainBlock |
| `DomainAbsolutePower` | Card 60 | `OnModifyDamageTaken` → min(damage,1) |
| `RefugePower` | Card 57 | 阻止 `OnTurnStart` 清除格擋 |
| `HolySpiritPregnancyPower` | Card 59 | `OnTurnStart` + `OnCardDraw` 升級打擊/防禦且 0 費 |
| `GatesOfParadisePower` | Card 58 | `AfterPlayerTurnStart` +2 能量 |
| `QuietMindPower` | Card 18 | `BeforeTurnEnd` 若本回未攻擊，+3/5 信仰 |
| `HolyGuidancePower` | Card 15 | `AfterAttackCard` +1 信仰 |
| `InfiniteGloryPower` | Card 23 | `OnRevelation` +2 力量 +2 敏捷 |
| `PrecepPower` | Card 79 | 本回合打牌上限 5，`AfterPlayerTurnStart` +2 能量 |

---

## 三、FaithManager 完整規格

```csharp
public static class FaithManager
{
    public static async Task CheckAndTriggerRevelation(
        Creature owner,
        PlayerChoiceContext ctx,
        CustomCardModel? source = null)
    {
        while (true)
        {
            var faithPower = owner.Powers?.OfType<FaithPower>().FirstOrDefault();
            if (faithPower == null || faithPower.Amount < 10) break;

            bool skipHalving = owner.Powers?.OfType<DivineSummonPower>().Any() == true;

            // 1. 先計啟示次數
            await PowerCmd.Apply<RevelationPower>(owner, 1, owner, source);

            // 2. 消耗信仰
            int currentFaith = (int)faithPower.Amount;
            int newFaith = skipHalving
                ? currentFaith - 10
                : currentFaith / 2;
            int faithDelta = newFaith - currentFaith;
            await PowerCmd.Apply<FaithPower>(owner, faithDelta, owner, source);

            // 3. AoE 非攻擊傷害
            foreach (var enemy in owner.CombatState?.Enemies?.Where(e => e.IsAlive) ?? [])
                await DamageCmd.NonAttack(6).FromPower(source).Targeting(enemy).Execute(ctx);

            // 4. 能量
            await PlayerCmd.GainEnergy(1, owner.Player);
        }
    }
}
```

### 標準信仰增加呼叫模式

```csharp
// 每張涉及信仰增加的牌，OnPlay 結尾統一：
await PowerCmd.Apply<FaithPower>(Owner.Creature, amount, Owner.Creature, this);
await FaithManager.CheckAndTriggerRevelation(Owner.Creature, choiceContext, this);
```

---

## 四、刪除清單

執行實作前，刪除以下檔案（保留 `AiCard.cs`）：

```
Cards/*.cs              （全部，除 AiCard.cs）
Powers/PietyPower.cs
```

更新：
- `AiCard.cs`：`PietyPower` 引用改為 `FaithPower`

保留但重構：
- `Relics/OracleTabletRelic.cs`、`PrayerBeadsRelic.cs` — 移除 PietyPower 引用，後續依新遺物設計調整

---

## 五、全 80 張卡牌清單

### 命名慣例
- 類別名：英文語義 + `Card`（如 `StrikeCard`、`PrayerCard`）
- 本地化 key：`AICARDMOD-<UPPER_SNAKE>_CARD.title/description`

---

### 第一批：初始牌組（Cards 1–10）

| # | 類別名 | 中文名 | 類型 | 稀有度 | 費 | 效果 | 升級效果 |
|---|---|---|---|---|---|---|---|
| 1–5 | `StrikeCard` | 打擊 | Attack | Basic | 1 | 造成 6 傷 | 造成 9 傷 |
| 6–9 | `DefendCard` | 防禦 | Skill | Basic | 1 | 獲得 5 格擋 | 獲得 8 格擋 |
| 10 | `PrayerCard` | 祈禱 | Skill | Basic | 1 | 獲得 3 信仰，抽 1 牌 | 獲得 5 信仰，抽 1 牌 |

---

### 第二批：信仰與啟示流（Cards 11–30）

| # | 類別名 | 中文名 | 類型 | 稀有度 | 費 | 效果 | 升級效果 |
|---|---|---|---|---|---|---|---|
| 11 | `LightFlowCard` | 光流 | Attack | Common | 1 | 造成 8 傷；有信仰額外 4 傷 | 造成 11 傷；有信仰額外 6 傷 |
| 12 | `FaithGatherCard` | 集氣 | Skill | Common | 0 | 獲得 2 信仰；Exhaust | 獲得 3 信仰；不再 Exhaust |
| 13 | `BlindFaithCard` | 盲信 | Skill | Common | 1 | 獲得 7 格擋，1 信仰 | 獲得 10 格擋，2 信仰 |
| 14 | `RefractionCard` | 折射 | Attack | Uncommon | 1 | 造成 5 傷；下 2 次獲信仰時層數 +1 | 造成 7 傷；下 3 次獲信仰時 +1 |
| 15 | `HolyGuidanceCard` | 神聖導引 | Power | Uncommon | 1 | 每打出攻擊牌獲 1 信仰（固有）| 固有，效果相同 |
| 16 | `FaithResonanceCard` | 信仰共振 | Skill | Uncommon | 2 | 獲得等同信仰層數的格擋 | 耗能 1，效果相同 |
| 17 | `BrillianceStrikeCard` | 光輝連擊 | Attack | Uncommon | 1 | 造成 3 傷 X 次（X=啟示次數+1）| 造成 5 傷 X 次 |
| 18 | `QuietMindCard` | 靜思 | Power | Uncommon | 1 | 回合結束未攻擊獲 3 信仰 | 回合結束未攻擊獲 5 信仰 |
| 19 | `VisionCard` | 異象 | Attack | Uncommon | 2 | 14 全體傷；信仰>10 返還 1 能量 | 18 全體傷；信仰>7 返還 1 能量 |
| 20 | `DivineSummonCard` | 天神下凡 | Power | Rare | 3 | 信仰不再因啟示而減半 | 耗能 2，效果相同 |
| 21 | `FinalJudgmentCard` | 最終審判 | Attack | Rare | 3 | Exhaust；造成 信仰×3 傷 | Exhaust；造成 信仰×4 傷 |
| 22 | `SaintLegacyCard` | 聖徒遺產 | Skill | Rare | 1 | 獲 10 信仰；施加 SaintLegacyPower(1) | 獲 14 信仰；SaintLegacyPower(2) |
| 23 | `InfiniteGloryCard` | 無限榮光 | Power | Rare | 2 | 每次啟示獲 2 力量與 2 敏捷（固有）| 固有，效果相同 |
| 24 | `OracleEyeCard` | 神諭之眼 | Skill | Uncommon | X | 抽 X 牌，獲 X 信仰 | 抽 X+1 牌，獲 X+1 信仰 |
| 25 | `VoidDevotionCard` | 虛空虔誠 | Skill | Rare | 0 | 信仰翻倍；Exhaust | 信仰翻倍；Exhaust；**Retain** |
| 26 | `EvangelismCard` | 傳教 | Attack | Common | 1 | 造成 7 傷；手中打擊升級為光流 | 造成 7 傷；打擊升級為光流+ |
| 27 | `IconBreakerCard` | 聖像破壞者 | Attack | Uncommon | 1 | 失所有信仰；每層 4 傷 | 失所有信仰；每層 6 傷 |
| 28 | `MiracleChainCard` | 連鎖奇蹟 | Attack | Uncommon | 1 | 手中每張「奇蹟」牌造成 5 傷 | 每張奇蹟造成 8 傷 |
| 29 | `HymnalChorusCard` | 聖歌合唱 | Power | Rare | 2 | 每有一張奇蹟，回合開始多抽 1（固有）| 固有，效果相同 |
| 30 | `FaithCollapseCard` | 信仰崩塌 | Skill | Rare | 1 | 失所有信仰；每層使本回數值 +10 耗能 | 效果相同 |

---

### 第三批：神聖受難與業力流（Cards 31–50）

| # | 類別名 | 中文名 | 類型 | 稀有度 | 費 | 效果 | 升級效果 |
|---|---|---|---|---|---|---|---|
| 31 | `KarmaTransferCard` | 業力轉移 | Skill | Uncommon | 1 | 本回傷害轉業力；Retain | Retain，效果相同 |
| 32 | `HolyLightRecompenseCard` | 聖光回饋 | Attack | Uncommon | 1 | 造成 10 傷 + 等同業力加成 | 造成 14 傷 + 等同業力加成 |
| 33 | `SpiritRestCard` | 靈魂安息 | Skill | Rare | 2 | Exhaust；回復業力量 50% HP | Exhaust；回復 75% HP |
| 34 | `BloodSacrificeCard` | 血祭 | Skill | Common | 0 | 失去 3 HP，獲得 2 能量 | 失去 2 HP，獲得 2 能量 |
| 35 | `SorrowfulDefenseCard` | 刺痛防禦 | Skill | Common | 1 | 獲得 9 格擋，失去 2 HP | 獲得 13 格擋，失去 2 HP |
| 36 | `RepentanceCard` | 懺悔 | Attack | Common | 1 | 造成 9 傷；本回失血則獲 2 信仰 | 造成 12 傷；獲 4 信仰 |
| 37 | `MartyrdomCard` | 受難 | Power | Rare | 2 | 每當失血，對隨機敵人造成 10 傷 | 造成 14 傷 |
| 38 | `HolyBloodCultivationCard` | 聖血灌溉 | Skill | Rare | 1 | Exhaust；每失 1 HP 增 2 格擋 | Exhaust；每失 1 HP 增 3 格擋 |
| 39 | `SinnerCard` | 負罪者 | Attack | Uncommon | 1 | 10 傷；本場每失 10 HP +10 傷 | 本場每失 8 HP +10 傷 |
| 40 | `NirvanaCard` | 涅槃 | Power | Rare | 3 | Exhaust；死亡時不減員，回 20% HP + 20 信仰；復活時清零業力 | 耗能 2；回 30% HP |
| 41 | `AtonementBanquetCard` | 贖罪聖餐 | Attack | Rare | 2 | 造成 20 傷；回本場失血 15% HP | 造成 28 傷；回 20% HP |
| 42 | `ForbiddenIconCard` | 禁忌聖像 | Power | Rare | 1 | 固有；失血時改獲等量格擋 | 固有，效果相同 |
| 43 | `CrimsonPyreCard` | 紅蓮火刑 | Attack | Rare | 2 | 18 全體傷；每有一負面效果 +10 | 24 全體傷；每有一負面效果 +14 |
| 44 | `AbandonmentCard` | 割捨 | Skill | Common | 1 | 丟 1 手牌，獲 5 信仰 | 丟 1 手牌，獲 8 信仰 |
| 45 | `MartyrCard` | 殉道 | Skill | Rare | 2 | 此牌被消耗時，獲 15 格擋 + 2 能量 | 獲 20 格擋 + 3 能量 |
| 46 | `ThornTrialCard` | 荊棘試煉 | Power | Uncommon | 1 | 每當被攻擊，給敵 3 層反傷 | 給敵 5 層反傷 |
| 47 | `GoodForEvilCard` | 以德報怨 | Skill | Uncommon | 1 | 受傷時獲等量格擋，持續 1 回合 | Retain，效果相同 |
| 48 | `SoulLinkCard` | 靈魂連結 | Skill | Uncommon | 1 | 賦 1 層共振（妳受傷敵受同傷）| 賦 2 層共振 |
| 49 | `FleshDedicationCard` | 肉體奉獻 | Skill | Rare | 1 | 失 5 HP；挑一張能力牌入手 | 失 5 HP；挑一張能力牌 0 費 |
| 50 | `BloodWashCard` | 血色洗禮 | Skill | Uncommon | 1 | 失 4 HP；本回信仰翻倍 | Retain，效果相同 |

---

### 第四批：領域與輔助（Cards 51–70）

| # | 類別名 | 中文名 | 類型 | 稀有度 | 費 | 效果 | 升級效果 |
|---|---|---|---|---|---|---|---|
| 51 | `HolyBarrierCard` | 神聖屏障 | Skill | Uncommon | 1 | 8 格擋；有信仰則賦敵 1 虛弱 | 11 格擋；賦 2 虛弱 |
| 52 | `HolyWaterCard` | 灑水 | Skill | Common | 1 | 解除所有負面狀態 | 耗能 0，效果相同 |
| 53 | `PurificationCard` | 淨化 | Skill | Common | 1 | Exhaust 一張狀態/詛咒牌，抽 1 牌 | 效果相同 |
| 54 | `HolyMilitiaCard` | 聖教軍 | Skill | Uncommon | 2 | 12 格擋；下回獲得 1 能量 | 16 格擋；下回獲得 2 能量 |
| 55 | `PraiseCard` | 讚美詩 | Power | Rare | 1 | 每打出 3 技能牌，抽 1 牌 | 耗能 0，效果相同 |
| 56 | `CathedralBellCard` | 教堂鐘聲 | Attack | Uncommon | 2 | 10 全體傷，賦 2 易傷 | 14 全體傷，賦 3 易傷 |
| 57 | `RefugeCard` | 避難所 | Power | Rare | 2 | 格擋不再消失 | 耗能 1，效果相同 |
| 58 | `GatesOfParadiseCard` | 天堂之門 | Power | Rare | 3 | 每回合獲得 2 能量 | 耗能 2，效果相同 |
| 59 | `HolySpiritPregnancyCard` | 聖靈感孕 | Power | Rare | 1 | 固有；打擊/防禦變升級版且 0 費（`OnTurnStart` + `OnCardDraw`）| 固有，效果相同 |
| 60 | `DomainAbsoluteCard` | 領域：無敵 | Skill | Rare | 2 | Exhaust；本回受傷不超 1 | Exhaust + **Retain**，效果相同 |
| 61 | `OmniscienceCard` | 全知全能 | Skill | Rare | 4 | Exhaust；挑 2 牌打兩次 | 耗能 3，效果相同 |
| 62 | `DivineInterventionCard` | 神聖干預 | Skill | Uncommon | 0 | 本回每消耗一牌獲 5 格擋 | 獲 8 格擋 |
| 63 | `ProphetDanceCard` | 祈神舞 | Skill | Rare | 2 | Exhaust；拿回消耗堆 2 牌 | 耗能 1，效果相同 |
| 64 | `HumilityCard` | 謙卑 | Skill | Common | 1 | 本回不造成傷害，獲 12 格擋 | 獲 16 格擋 |
| 65 | `TelekinesisCard` | 心靈感應 | Skill | Uncommon | 1 | 看抽牌堆前 5，拿 1 張 | 看前 8，拿 1 張 |
| 66 | `RelicGatherCard` | 聖物收集 | Skill | Rare | 1 | Exhaust；獲一隨機藥水 | 耗能 0，效果相同 |
| 67 | `GargoyleCard` | 石像鬼化 | Skill | Uncommon | 2 | 15 格擋；下回少 1 能量 | 20 格擋；下回少 1 能量 |
| 68 | `CleansingSinCard` | 洗罪 | Skill | Uncommon | 1 | 每消耗一牌回 2 HP | 每消耗一牌回 3 HP |
| 69 | `HolyDomainResonanceCard` | 聖域共鳴 | Skill | Rare | 1 | 本回每消耗一牌獲 2 信仰；Retain | Retain，效果相同 |
| 70 | `TabulaCard` | 石板刻字 | Skill | Uncommon | 1 | 擇一手牌加「Retain」| 耗能 0，效果相同 |

---

### 第五批：限制與奇蹟（Cards 71–80）

| # | 類別名 | 中文名 | 類型 | 稀有度 | 費 | 效果 | 升級效果 |
|---|---|---|---|---|---|---|---|
| 71 | `DivineCovenantCard` | 神聖契約 | Skill | Rare | 2 | Exhaust；消耗所有信仰，以信仰層數為預算呼叫 AI 生成效果 | 耗能 1，效果相同 |
| 72 | `SevenVirtuesCard` | 七美德 | Skill | Rare | 3 | Exhaust；獲 7 點全屬性 Buff | 耗能 2，效果相同 |
| 73 | `GenesisCard` | 創世記 | Power | Rare | 3 | Exhaust；牌組合併，剩餘牌本場耗能 -1（固有）| 固有，效果相同 |
| 74 | `ApocalypseProphecyCard` | 末日預言 | Power | Rare | 2 | 3 回後敵失 50% 當前 HP | 耗能 1，效果相同 |
| 75 | `SaintSmileCard` | 聖女的微笑 | Skill | Rare | 0 | Exhaust；獲能量/抽牌/信仰/力量 | 獲能量/抽牌/2信仰/2力量 |
| 76 | `StigmataCard` | 聖痕 | Skill | Uncommon | 0 | Exhaust；獲 2 能量，生命上限 -5（本戰）| 獲 2 能量，生命上限 -3 |
| 77 | `BlindTrustCard` | 盲信能力 | Power | Uncommon | 1 | 獲 3 信仰；無法看敵人意圖 | 耗能 0，效果相同 |
| 78 | `SoulStripCard` | 靈魂剝離 | Skill | Rare | 2 | Exhaust；敵失 15% 當前 HP（Boss 有上限）| 耗能 1；敵失 20% HP |
| 79 | `PrecepCard` | 戒律 | Power | Rare | 1 | 每回限打 5 牌，回合開始獲 2 能量（固有）| 固有，效果相同 |
| 80 | `FinalPrayerCard` | 最終祈禱 | Skill | Rare | 0 | Exhaust；獲 1 能量；回合末死亡 | Retain，效果相同 |

---

## 六、遺物規格

### 破損的經文（初始遺物）
- 每場戰鬥開始：獲得 3 信仰 + `CheckAndTriggerRevelation`

### 聖女的黃金經文（升級遺物）
- 每場戰鬥開始：獲得 6 信仰 + `CheckAndTriggerRevelation`
- 每次觸發啟示：抽 1 張牌

---

## 七、實作階段規劃

### Phase 0（核心引擎）
1. 刪除舊卡牌與 PietyPower
2. 建立 `FaithPower`、`RevelationPower`、`KarmaPower`、`HpLostTrackerPower`
3. 建立 `FaithManager.CheckAndTriggerRevelation()`
4. 更新 `AiCard` 引用改為 `FaithPower`

### Phase 1（初始牌組）
- Cards 1–10：`StrikeCard`、`DefendCard`、`PrayerCard`

### Phase 2（信仰流）
- Cards 11–30

### Phase 3（受難流）
- Cards 31–50

### Phase 4（領域流）
- Cards 51–70

### Phase 5（奇蹟流 + 遺物）
- Cards 71–80 + 兩個遺物 + 本地化（ENG + zhs）

---

## 八、特殊邊際情況備忘

| 情況 | 處理方式 |
|---|---|
| DivineSummonPower + 信仰翻倍 | FaithManager 減 10（非減半），避免死循環 |
| 涅槃復活 | 清零 KarmaPower，避免回合末業力結算二次死亡 |
| ForbiddenIcon + KarmaTransfer 同存 | ForbiddenIcon 優先，穿透格擋後才進業力池 |
| HolySpiritPregnancy | `OnTurnStart` + `OnCardDraw` 雙重掛載 |
| 最終祈禱（Card 80）+ Retain 升級 | Retain 讓玩家可留著作為「必殺開關」 |
| 聖域共鳴（Card 69）消耗大量牌 | 每次 `OnExhaust` 都呼叫 `CheckAndTriggerRevelation` |

---

*本文件由設計討論自動彙整。實作前請以此為基準，逐節對照已確認的機制設計。*
