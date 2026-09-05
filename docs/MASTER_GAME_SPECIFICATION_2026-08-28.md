# 一盤面式能動型インクリメンタルゲーム Master仕様書

文書Version：0.7.2
更新日：2026年9月5日
対象Engine：Unity 6000.3.18f1  
第一対象Platform：Windows 10／11 x64  
最終配布先：Steam  
試作配布先：GitHub Releases  
Project名：未定

---

## 0．この文書の役割

本書は，本作の企画，ゲームロジック，画面構造，Data構造，Unity実装方針，Prototype計画，GitHub運用を一つに統合した唯一の正本である．本書単体を読めば，現時点の採用仕様とPrototype用初期値を実装できる状態を目的とする．

2026年9月5日のUser承認により，最新試作は§32「速度資源型マガジン・ピンボール」へ移る．本試作については§32が旧章の五発循環・非破壊Target・固定速度・配置編集・Visited報酬制限等に優先する．旧章と旧Buildは比較・復元用に保持し，過去Prototypeの仕様を上書きしない．完成版の製品定義は新試作の手応えを見て更新する．

旧監査資料，調査依頼書，調査報告書，経済調査，設計メモ，Chat上の提案は判断履歴であり，実装根拠にしない．それらと本書が競合する場合，本書を優先する．AIへ実装を依頼する時は，本書と対象Prototypeの実装Briefだけを仕様入力とする．Userから本書を変更する明示的指示が出た場合は，その指示を優先し，本書のVersionと変更履歴を更新する．

### 0.1．仕様状態

本書では，各仕様を次の状態で管理する．

| 状態 | 意味 | AI実装時の扱い |
|---|---|---|
| `FIXED` | 現在の基準仕様 | 無断変更しない |
| `INITIAL` | 最初に実装する仮説値 | 指定値で実装し，計測後に変更可能 |
| `TEST` | Prototype結果で決定 | 正式仕様として断定しない |
| `DEFERRED` | 正本には残すが現在のPrototype外 | 明示指示なしに実装しない |

### 0.2．数値の扱い

本書中の時間，倍率，価格，角度，個数のうち`INITIAL`と書かれた値は，完成版の確定Balanceではない．Prototypeで比較可能な初期状態を作るための値である．

### 0.3．AI運用時の原則

AIは次を守る．

1．`FIXED`仕様を変更する場合，変更理由，影響範囲，代替案を先に提示する．  
2．未定義の仕様を勝手に恒久仕様へしない．最小の仮実装と`TEST`項目を提案する．  
3．一つのIssueまたはPull Requestで，無関係な機能を同時に変更しない．  
4．ゲーム上の論理結果とUnity上の描画Objectを分離する．  
5．乱数，Event順，Save形式，Versionを再現可能にする．  
6．処理上限へ到達した時，報酬や効果を無言で消さない．  
7．Sourceにない仕様を，既存仕様であるかのように断定しない．  
8．実装変更時は，本書，Test，Release Notesのうち影響するものを同時に更新する．  
9．`TEST`項目が実装に必要な場合，本書に書かれた現在値だけを使い，新しい候補を追加しない．  
10．判断履歴文書から機能を復活させない．

### 0.4．実装不変条件

次は，Balance値が変わっても維持する不変条件である．Issue，Commit，Pull Requestでは，影響するIDを記載する．

| ID | 不変条件 |
|---|---|
| `INV-001` | ゲーム終了まで一つの盤面を継続成長させる |
| `INV-002` | 通常Targetは敵ではなく，繰り返し利用する収益装置である |
| `INV-003` | Trigger Pull一回につき一Lineageを作る |
| `INV-004` | 子弾は親LineageとVisited集合を継承する |
| `INV-005` | Recipeは物理Magazineと独立した五Slotの循環規則である |
| `INV-006` | PrimerはRecipe確定時に決まり，発射順や着弾時刻で変化しない |
| `INV-007` | Recipe変更は周期途中へ即時反映せず，次周期境界で適用する |
| `INV-008` | 通貨はGoldとMaterialの二種類に限定する |
| `INV-009` | Core DamageはLineage Yieldと共通倍率から算出し，独立Damage経済を作らない |
| `INV-010` | Ascension，Prestige，盤面Resetを進行へ使わない |
| `INV-011` | 論理SimulationとUnity Viewを分離する |
| `INV-012` | 論理Projectile上限をゲーム規則として明示し，描画上限と混同しない |
| `INV-013` | Recipe，盤面，Upgrade編集時はSimulationを停止する |
| `INV-014` | PrototypeはPlayerの明示操作なしにTelemetryを送信しない |
| `INV-015` | 五発Recipeが照準判断を変えない場合，機能追加で延命せず主軸を再設計する |

### 0.5．章構成

```text
0  文書規則とAI運用
1  製品定義
2  非目標
3  用語
4  Game State
5  盤面
6  入力と照準
7  GunとReload
8  五発Recipe
9  Capacity
10 ProjectileとLineage
11 弾種
12 Target
13 経済とDamage
14 CoreとEnding
15 自動化
16 進行
17 UI
18 Tutorial
19 AudioとAccessibility
20 Unity実装
21 SaveとVersion
22 Playtest Log
23 Prototype
24 Test
25 GitHub
26 Steam
27 未確定事項
28 Definition of Done
29 実装開始順
30 変更履歴
31 現在の最終判断
```

---

## 1．製品定義

### 1.1．一文説明

一つの盤面へTargetを配置し，五発の循環レシピと射撃方向を組み合わせて連鎖を作り，盤面奥の巨大Coreを破壊する，約2時間30分から3時間で完結する能動型インクリメンタルゲームである．`FIXED`

### 1.2．想定体験

```text
一発を狙って撃つ
↓
Targetへ命中し，Goldと素材を得る
↓
Target，弾，自動化を解放する
↓
五発レシピとTarget配置を編集する
↓
一発が複数Targetへ連鎖する
↓
自動化へ単純操作を委譲する
↓
手動で高収益経路を作る
↓
盤面全体をCore破壊装置へ変える
```

### 1.3．設計柱

1．**一盤面の継続成長**．最初の木Targetから最終Coreまで，同じ盤面を育てる．`FIXED`  
2．**五発レシピによるBuild**．弾種だけでなく，配置順が結果を変える．`FIXED`  
3．**照準による能動回収**．Buildの潜在能力を，現在弾に応じた照準で回収する．`FIXED`  
4．**見えるIncremental**．倍率だけでなく，Target，軌跡，反射，分裂，音が増える．`FIXED`  
5．**明確な終点**．無限継続を前提にせず，巨大Core破壊でEndingへ到達する．`FIXED`

### 1.4．三つの主要軸

```text
五発レシピ ＝ Buildの設計図
Socket配置 ＝ 設計図が働く物理経路
照準        ＝ 現在弾に応じて経路を選ぶ操作
```

主要Build軸は五発レシピである．Target配置は補助Build軸，銃は射撃感とTempoの軸とする．`FIXED`

### 1.5．想定規模

- 初見Ending：約150分から180分．`TEST`
- 密度が維持できない場合：約120分から150分へ短縮する．`FIXED`
- 無料配布を想定する．`FIXED`
- 個人制作または小規模制作で完成可能な範囲とする．`FIXED`
- Online，対戦，課金，長期運営は行わない．`FIXED`
- Steam実績，Controller，追加言語は完成性確認後に判断する．`DEFERRED`

### 1.6．Theme，物語，言語

- Project名，世界観，主人公，Targetの最終美術Themeは未決定である．`TEST`
- Prototype 0から2は抽象図形と仮称で進めてよい．`FIXED`
- 美術Themeによって物理規則や経済構造を変更しない．`FIXED`
- EndingにはCore破壊の視覚的結末を必須とするが，物語Text量は未決定である．`TEST`
- 開発中の基準UI言語は日本語とする．`INITIAL`
- Steam公開時の英語対応は，Text量と制作期間をPrototype 4後に評価する．`DEFERRED`
- Voiceは想定しない．`FIXED`

---

## 2．非目標

次は現在の本作へ入れない．

- 六Stage制，時間切れ，敗北，敵AI，敵攻撃，敵弾幕．
- 通常区間後の独立Boss戦，防衛Core．
- 複数砲台，30発手動Magazine，完全自由座標配置．
- Ascension，Prestige，盤面Reset，Offline収益．
- 三種類以上の通貨，一発ごとの弾薬消費，Random Affix．
- Run内Perk，自動購入，自動配置，高度なAuto Aim．
- 爆発弾，Gold弾，Chaos Loader，Prism，Relay，Sweep Servo．

これらは現在の製品範囲外であり，過去資料に記述があっても実装しない．追加する場合はMasterのVersionを更新する．

---

## 3．用語

| 用語 | 定義 |
|---|---|
| Board | ゲーム終了まで継続する一つの論理盤面 |
| Gun | 画面下中央に固定された一丁の発射装置 |
| Target | 命中によって報酬または弾道効果を生む盤面装置 |
| Socket | Targetを配置できる固定候補位置 |
| Recipe | 五Slotを循環する自動装填規則 |
| Chamber | 次に発射される一発の状態 |
| Primer | Recipe上で通常弾が後続特殊弾へ与える装填強化 |
| Projectile | 盤面上を移動する一発または子弾の論理状態 |
| Lineage | 一回のTrigger Pullから生じた親弾と全子弾の系譜 |
| Lineage Yield | 一Lineageが獲得した実Goldの合計 |
| Special Effect Visit | 同一LineageがTarget固有効果を起動済みかの記録 |
| Reward Visit | 同一LineageがTarget報酬を取得済みかの記録 |
| Material | 有限解放に使う一種類の素材 |
| Lifetime Gold | 獲得Goldの累積．消費できず，表示解禁条件にだけ使う進行Counter |
| Capacity | 五発Recipeへ入れられる特殊弾Costの合計上限 |
| Overdrive | 盤面をResetせず，終盤の制約を壊す質的解放 |
| Core | 最初から盤面奥に見える最終Target |
| Simulation | 報酬，衝突，Lineageを決定する純粋な論理処理 |
| View | Simulation結果をUnity上で描画，演奏，表示するObject |

---

## 4．Game State

### 4.1．上位State

```text
Boot
↓
MainMenu
↓
Playing
├ PausedByPlayer
├ EditingRecipe
├ EditingBoard
├ ViewingUpgrades
└ Ending
```

`EditingRecipe`，`EditingBoard`，`ViewingUpgrades`中はSimulationを停止する．UI AnimationとAudio fadeのみUnscaled Timeで動かしてよい．`FIXED`

- `EditingRecipe`開始時，飛行中Projectileは保持する．変更RecipeはPending扱いとなるため，既存Lineageへ影響しない．`FIXED`
- `ViewingUpgrades`開始時，飛行中Projectileは保持する．Projectile生成時にSnapshotした倍率は変えない．`FIXED`
- `EditingBoard`開始時，飛行中Projectileを終了し，各Lineageを現在Yieldで完了させる．移動後のTargetを既発射弾へ後置きできないようにする．`FIXED`
- 編集画面を閉じた入力を発射入力へ伝播させない．`FIXED`

### 4.2．Playing State

Playing中は次を進行させる．

- 発射可能時間．
- Projectile移動．
- Target Cooldown．
- Material Gauge．
- Auto-fire．
- Lineage完了判定．
- Gold，Core Damage，進行条件．

### 4.3．Save対象

- Game Version．
- Save Schema Version．
- Gold．
- Lifetime Gold．
- Material所持数とGauge．
- 購入済みGold Upgrade．
- 解放済み有限機能．
- Active Recipe五SlotとPending Recipe五Slot．
- 現在Recipe Slot IndexとRecipe Cycle ID．
- Capacity．
- Target所有数とSocket配置．
- Automation設定．
- Core Shield進行とCore HP．
- Tutorial Flag．
- 設定値．
- Play時間．

盤面上を飛行中のProjectile，未完了Lineage，一時的Floating Numberは保存しない．Save Snapshot作成はLive Runtimeを変更せず，Autosaveによって飛翔中Projectileを消滅させない．Load時だけ飛翔中Projectileと未完了Lineageが存在しない安全状態へ戻し，Chamberを装填済み，発射可能にする．`FIXED`

---

## 5．盤面

### 5.1．論理座標

- 論理解像度は1600×900．`FIXED`
- Aspect Ratioは16:9．`FIXED`
- 物理計算は論理座標で行う．`FIXED`
- Windowが16:9でない場合，盤面は等比Scaleし，余白はLetterboxまたは安全UI領域にする．`FIXED`
- Window ResizeでTarget位置，反射角，弾速を変えない．`FIXED`

### 5.2．Gun位置

- 基準位置は`(800, 820)`．`INITIAL`
- Gunは盤面下中央へ固定する．`FIXED`
- Gun本体はProjectile Colliderと衝突しない．`FIXED`

### 5.3．Core位置

- 基準中心は`(800, 115)`．`INITIAL`
- Coreは開始時からSilhouetteとShield Ringを表示する．`FIXED`
- 未解放時も長期目標であることをUIで示す．`FIXED`

### 5.4．自由配置方式

2026-09-05のUser承認により，固定Socket方式を盤面内の自由配置へ変更する．`FIXED`

- 配置編集はPauseし，ドラッグで移動する．20 logical pxのグリッド吸着を初期値とする．`INITIAL`
- Mirrorは5度単位で回転でき，面法線と散布なしの参考反射経路を表示する．`INITIAL`
- 装置同士の重なり，盤面外，銃とUI領域への配置は禁止する．円は円，回転した板は回転矩形の形状で判定する．`FIXED`
- 禁止位置は赤く表示し，不正な位置で離した場合は移動前の位置・角度へ戻す．`FIXED`
- 配置変更は無料である．配置編集開始時の既発射弾終了は§4.1に従う．`FIXED`
- Prototype 1Aでは配置領域をX=300〜1560，Y=140〜750とする．当たり判定全体が領域内に収まる必要がある．`INITIAL`
- Prototype 1AはCollector二個，Mirror一個，Amplifier一個を支給した実験開始状態とし，Collector Base Goldは2で開始する．購入済み初回Upgrade相当で，製品のNew Game値は変更しない．`INITIAL`
- 配置の明示保存・編集終了・終了時にLocalへ保存する．上書き前提の現行配置とは別に日時付き配置履歴を保持する．`INITIAL`

以下のSocket図と個数・価格参照は旧設計の候補である．本節の自由配置規則が優先し，Prototype 1AへSocket購入や固定位置制限を実装しない．製品の所有数・配置可能数・解禁価格は後続Prototypeで再整理する．

```text
┌──────────────────────────────────────────┐
│                 [ CORE ]                 │
│       o A2       o C1       o A3        │
│   o B2      o U2      o U3      o B3    │
│       o A1       o C2       o A4        │
│   o B1      o U1      o U4      o B4    │
│             \\  射線領域  /              │
│                  [銃]                    │
└──────────────────────────────────────────┘
```

- 完成版候補は14から16 Socket．`TEST`
- Prototype 1は5 Socket．`INITIAL`
- Target移動は無料．`FIXED`
- 配置編集時はPauseする．`FIXED`
- Basic Targetは同種最大3個．`INITIAL`
- Utility Targetは原則同種1個．`INITIAL`
- Socket間隔は最大Target直径の1.5倍以上を目安とする．`INITIAL`
- 配置画面では，Mirrorからの予測反射線を表示する．`TEST`

### 5.5．境界

- 左右と上端は壁として反射可能にする．`INITIAL`
- 画面下端へ到達したProjectileは消滅する．`INITIAL`
- Prototype 0では，壁一枚のみを配置する．`INITIAL`
- 完成版の全周反射壁は，反射Buildが常時最適になる場合に見直す．`TEST`

---

## 6．入力と照準

### 6.1．基本入力

- Mouse移動で照準方向と集中度を指定する．`FIXED`
- Left Click一回で一発撃つ．`FIXED`
- UI上のClickは発射に使わない．`FIXED`
- Reload中のClickは予約しない．`INITIAL`
- 発射方向はClickした瞬間のCursor位置で確定する．`FIXED`
- Recipe，配置，Upgrade画面はButtonまたはKeyboard Shortcutで開く．`TEST`
- Escで編集画面を閉じるかPause Menuを開く．`FIXED`

### 6.2．照準方向

```text
AimVector = CursorLogicalPosition - GunLogicalPosition
AimDirection = normalize(AimVector)
```

CursorがGun中心から`minimumAimRadius`未満にある場合，直前の有効`AimDirection`を維持する．起動後に有効方向がない場合，上方向`(0, -1)`を使う．`FIXED`

### 6.3．Cursor距離と散布角

初期式は線形補間とする．`INITIAL`

```text
t = clamp((distance - minDistance) / (maxDistance - minDistance), 0, 1)
spread = maxSpread + (minSpread - maxSpread) × t
```

初期値は次とする．

```text
minDistance = 100
maxDistance = 600
minSpread = 2度
maxSpread = 14度
```

例としてCursor距離350なら，

```text
t = (350 - 100) / (600 - 100) = 0.5
spread = 14 + (2 - 14) × 0.5 = 8度
```

- 遠いほど集中，近いほど拡散する．`FIXED`
- 武器固有の最小，最大散布角を越えない．`FIXED`
- 発射前に実際の最大散布範囲を半透明Coneで表示する．`FIXED`
- 実弾は表示Cone外へ出ない．`FIXED`
- 非線形補正はPrototype 0比較後に判断する．`TEST`

### 6.4．散布乱数

- Run開始時にSession Seedを作る．`FIXED`
- ShotごとにSeedから偏差を生成し，Logへ記録する．`FIXED`
- 同じSeed，同じ入力Event列なら，同じ偏差列を再現可能にする．`FIXED`
- 初期分布は`[-spread, +spread]`の一様角度分布とする．`INITIAL`
- 中央寄り分布は射撃感比較後に判断する．`TEST`

### 6.5．長押し

- 開始時は無効．`FIXED`
- 60分前後の進行報酬として解放候補とする．`INITIAL`
- 長押し中も各発射時点のCursor方向を使う．`FIXED`

---

## 7．GunとReload

### 7.1．Gun数

完成版の基準は一丁である．第二，第三の独立Gunは入れない．`FIXED`

銃の成長は置換ではなく，次のMode解放で表現する．

- Single Shot．開始時．
- Hold Fire．中盤．
- Overdrive Burst．終盤．

### 7.2．Parameter

| Parameter | 初期値 | 状態 |
|---|---:|---|
| Reload／Cycle Time | 0.65秒 | `INITIAL` |
| Projectile Speed | 900 logical px/s | `INITIAL` |
| Projectile Radius | 8 logical px | `INITIAL` |
| Impact Multiplier | 1.0 | `INITIAL` |
| Min Spread | 2度 | `INITIAL` |
| Max Spread | 14度 | `INITIAL` |

### 7.3．ChamberとReload状態機械

Runtimeは`chamberSlotIndex`，`nextSlotIndex`，`chamberReady`，`reloadRemaining`を別々に持つ．`FIXED`

- New Gameは`chamberSlotIndex = 0`，`chamberReady = true`，`recipeCycleId = 0`で開始する．`FIXED`
- 発射要求は`Playing`かつ`chamberReady`の時だけ成功する．`FIXED`
- 成功時，現在の`chamberSlotIndex`からLineageを生成し，`chamberReady = false`としてReloadを開始する．`FIXED`
- 同時に`nextSlotIndex = (chamberSlotIndex + 1) mod 5`を計算するが，Chamberはまだ進めない．`FIXED`
- Reload完了時，`nextSlotIndex`をChamberへ装填し，`chamberReady = true`にする．`FIXED`
- Slot 4からSlot 0を装填する瞬間にPending RecipeをActive Recipeへ反映し，`recipeCycleId`を一増加する．`FIXED`
- Slot 0から4の五Lineageは同じ`recipeCycleId`を共有する．`FIXED`
- 編集でRecipeを変更しても，現在ChamberとActive Recipeは変えない．`FIXED`
- 編集後のRecipeはPending Recipeとして保持し，Slot 4発射後のReload完了でSlot 0を装填する時に適用する．`FIXED`
- まだ一発も発射していないNew Gameでは，Pending Recipeを即時適用してよい．`FIXED`
- Pending Recipeがある間，UIへ`次周期から適用`と表示する．`FIXED`
- 編集中はPauseするため，編集直後に意図しない発射は起こらない．`FIXED`
- Reload進行は円形または線形Indicatorで常時読めるようにする．`FIXED`
- Reload完了時に小さな発光と音を出す．`INITIAL`

### 7.4．Overdrive Burst

- 五発Recipeを短い間隔で一周期撃つModeである．`INITIAL`
- Burst内でも各Slotは個別Lineageを持つ．`FIXED`
- PrimerはRecipe定義から事前計算されるため，着弾順に影響されない．`FIXED`
- Burst間隔とCooldownはPrototype 4後に決定する．`TEST`

---

## 8．五発Recipe

### 8.1．基本定義

- Recipeは常に五Slotである．`FIXED`
- 五Slotは物理Magazine容量ではない．`FIXED`
- 自動装填機が循環参照する規則である．`FIXED`
- Slot順は決定論的である．`FIXED`
- 五番目の次は一番目である．`FIXED`
- 一周期の五発は同じRecipe Cycle IDを持ち，各発射は別Lineage IDを持つ．`FIXED`
- Recipe編集時はPauseする．`FIXED`
- 現在弾，次弾，残り三発を常時表示する．`FIXED`

### 8.2．Primer方式

PrimerはRecipe編集完了時に計算する装填効果とする．`FIXED`

計算規則は次のとおり．

1．各特殊弾Slotから循環方向へ直前Slotを調べる．  
2．直前に連続する通常弾を数える．  
3．特殊弾または二発目の通常弾へ到達したら終了する．  
4．Primer値は0から2である．  
5．通常弾Slot自身はPrimerを消費しない．  
6．特殊弾のPrimerは発射時にProjectileへCopyする．

例：

```text
[通常][通常][分裂][通常][貫通]
              Primer 2      Primer 1
```

```text
[貫通][分裂][通常][通常][通常]
 Primer 2  Primer 0
```

二つ目の例では，末尾の連続通常弾が循環して一番目の貫通弾を強化する．

### 8.3．Primerの意味

- 通常弾は安定Goldと特殊弾準備を担当する．`FIXED`
- 特殊弾だけで埋めるとPrimer 0となる．`FIXED`
- Primerは命中しなくてもRecipe上の強化値として存在する．`FIXED`
- 通常弾を当てる能動報酬はGoldとLineage Yieldであり，Primer成立条件には混ぜない．`FIXED`
- 着弾Comboを追加する場合は，Primerと別名称，別Stateにする．`DEFERRED`

### 8.4．Recipe UI

- DragまたはClick選択でSlotを入れ替える．`INITIAL`
- 弾を選び，全Slotへ一括設定できる．`INITIAL`
- Undoを一回用意する．`INITIAL`
- 各特殊Slot上にPrimer値を表示する．`FIXED`
- Capacity超過時は確定できず，超過SlotとCostを強調する．`FIXED`
- 確定した変更はPending Recipeへ保存し，Active Recipeを途中で書き換えない．`FIXED`
- Pending Recipeを再編集した場合，最新の有効内容で置き換える．`FIXED`
- 複数Presetは初期版へ入れない．`DEFERRED`

---

## 9．特殊装填Capacity

### 9.1．基本規則

- 通常弾Costは0．`FIXED`
- 特殊弾Cost合計はCapacity以下でなければならない．`FIXED`
- 発射によってGoldまたはMaterialを消費しない．`FIXED`
- 同種重複追加Costは設けない．`FIXED`
- 同種制限は弾固有に必要な場合だけ設定する．`FIXED`

### 9.2．初期値

| 項目 | 値 | 状態 |
|---|---:|---|
| 初期Capacity | 4 | `INITIAL` |
| 中盤Capacity | 5，6 | `INITIAL` |
| Overdrive Capacity | 8 | `INITIAL` |
| 貫通弾Cost | 2 | `INITIAL` |
| 分裂弾Cost | 3 | `INITIAL` |

初回解放時は，貫通弾二発または分裂弾一発を選べる．残りSlotへ通常弾を必要とする構成になる．

---

## 10．ProjectileとLineage

### 10.1．Lineage生成

- Trigger Pull一回ごとに新しいLineage IDを作る．`FIXED`
- Recipeの各発射は別Lineageである．`FIXED`
- 分裂した子弾は親Lineage IDを継承する．`FIXED`
- Lineageは全Projectile消滅時に完了する．`FIXED`
- 完了時にLineage Yieldを一回表示する．`FIXED`

### 10.2．Projectile State

最低限，次を持つ．

```text
ProjectileId
LineageId
ParentProjectileId
RecipeCycleId
RecipeSlotIndex
ProjectileType
PrimerLevel
Position
Velocity
Radius
GenerationDepth
RemainingReflections
RemainingPierces
RewardMultiplier
SpawnSequence
GunMultiplierSnapshot
ProjectileMultiplierSnapshot
Alive
```

### 10.3．Lineage State

```text
LineageId
RootProjectileId
RecipeCycleId
RecipeSlotIndex
RootProjectileType
PrimerLevel
YieldGold
CoreDamageDealt
GenerationBudgetRemaining
ActiveProjectileCount
EffectVisitedTargetIds
RewardVisitedTargetIds
CreatedTick
CompletedTick
```

Visited集合はLineageだけが所有する．Projectileは`LineageId`から同じ集合を参照し，Snapshotや複製を持たない．子弾間で同じTarget効果を重複起動せず，同期不良を防ぐためである．`FIXED`

### 10.4．Simulation Tick

- 論理Simulationは60 tick/sを初期値とする．`INITIAL`
- 描画Frame Rateと分離する．`FIXED`
- Projectile位置はView側で補間してよい．`FIXED`
- 一Tickの長時間停止後に無制限Catch-upしない．最大Catch-up Tick数を設定する．`INITIAL`
- Background中はSimulationを停止する．`INITIAL`

### 10.5．移動と衝突検索

Rigidbody2DのCollision Callbackを論理正本にしない．`FIXED`

Projectileは各Tickで次を行う．

```text
1．予定移動距離を計算する
2．現在位置から予定位置までCircleCastする
3．最も近い有効衝突を選ぶ
4．衝突点まで移動する
5．衝突Eventを処理する
6．反射または貫通後に残距離があれば再検索する
7．残距離終了またはProjectile消滅で終了する
```

- 高速弾のすり抜けを許容しない．`FIXED`
- 一Tick内衝突反復上限は8回．`INITIAL`
- 上限到達時は残移動を翌Tickへ持ち越す．効果を消さない．`INITIAL`
- 同距離の衝突はCollider Priority，Target IDの順で安定決定する．`FIXED`
- 衝突距離差が0.001 logical px以下なら同距離とみなす．`INITIAL`

### 10.6．Collider Priority

同一点と判断される場合の優先順は次とする．`INITIAL`

```text
Core
Target
Wall
Boundary Exit
```

Target同士を重ねて配置できないため，通常はTarget同士の同時衝突は起こらない．

### 10.7．Event処理順

```text
1．衝突対象と衝突点を確定する
2．Effect VisitとReward Visitを判定する
3．GoldまたはMaterial報酬を計算し，Lineage Yieldへ加算する
4．Target固有効果を現在Projectileへ適用する
5．Projectile固有効果を実行し，必要なら子弾を生成する
6．適用後のLineage ID，倍率，残回数を子弾へ継承する
7．親弾の消滅，反射，貫通を確定する
8．論理結果をEvent Priority，Target ID，Projectile Spawn Sequence順に確定する
9．確定済み結果からView Eventを発行する
```

AmplifierへSplit Projectileが命中した場合，手順4で倍率を付与し，手順5で分裂し，手順6で付与済み倍率を各子弾へCopyする．`FIXED`

論理結果が確定する前にParticle，Audio，Floating NumberからGame Stateを変更してはならない．`FIXED`

### 10.8．反射

```text
reflected = velocity - 2 × dot(velocity, normal) × normal
```

- Mirrorまたは壁の法線で反射する．`FIXED`
- 初期版は速度減衰なし．`INITIAL`
- 反射回数を一消費する．`FIXED`
- 残り反射0で反射Targetへ当たった場合，Projectileは消滅する．`INITIAL`
- 衝突直後は法線方向へ小さなEpsilonだけ離し，再衝突を防ぐ．`FIXED`

### 10.9．生成上限

| 上限 | 初期値 | 状態 |
|---|---:|---|
| 分裂Generation Depth | 3 | `INITIAL` |
| 一Projectile反射回数 | 6 | `INITIAL` |
| 一Projectile貫通回数 | 8 | `INITIAL` |
| 一Lineage生成Budget | 64 | `INITIAL` |
| 同時描画Projectile | 250 | `INITIAL` |

Generation BudgetはRootを含む論理Projectile生成数である．Budget不足時は可能な数だけ生成し，超過数を`LIMIT` View Eventへ記録する．方向が異なる未生成Projectileの将来報酬は計算できないため，超過分へ疑似報酬を与えない．これは明示されたBalance上限である．`FIXED`

### 10.10．論理Batchと描画集約

- 論理Projectileと描画Projectileを分離する．`FIXED`
- 論理上存在するProjectileは，Viewが省略されても個別にSimulationする．`FIXED`
- 描画上限を超えた同Lineage，同方向Bucket，同0.1秒窓の子弾はSwarm Viewへ束ねる．`INITIAL`
- Swarmは太い軌跡と`×N`で表示するが，論理報酬を代理計算しない．`FIXED`
- 描画上限はGame Logicへ影響しない．`FIXED`

---

## 11．弾種

### 11.1．通常弾

状態：`FIXED`

- Capacity Cost 0．
- 基礎弾倍率1.0．`INITIAL`
- Target固有の通常処理に従う．
- Recipe上で直後の特殊弾へPrimerを与える．
- 特殊効果を持たない代わりに，安定GoldとPrimerを担当する．
- 終盤も下位互換にしない．

### 11.2．貫通弾

状態：Prototype 2で`INITIAL`

- Capacity Cost 2．
- 基礎弾倍率1.25．`INITIAL`
- 基礎Remaining Pierceは2．`INITIAL`
- Primer 1ごとにRemaining Pierce＋1．`FIXED`
- Target通過時にRemaining Pierceを一消費する．
- Mirror反射はPierceを消費しない．
- Core命中で必ず消滅する．
- 同一Lineageの同Target報酬は原則一回である．

### 11.3．分裂弾

Prototype 2で未詳細部分を具体化する：子も分裂弾としてPrimer・残反射回数・反射履歴・倍率を継承する．深度3では分裂要求を抑止し親を終了，未生成数をLIMIT表示する．貫通残数0では次のCollector報酬後に吸収する．`INITIAL`

状態：Prototype 2で`INITIAL`

- Capacity Cost 3．
- 基礎弾倍率1.0．`INITIAL`
- 最初の分裂可能Target命中時に親弾を消滅させ，子弾を生成する．`INITIAL`
- 基礎子弾数は2．`INITIAL`
- Primer 1ごとに子弾＋1．`FIXED`
- 子弾角度は親方向を中心に均等配置する．`INITIAL`
- 基礎開き角は合計30度．`INITIAL`
- 子弾はLineage，Visited集合，Reward Multiplierを継承する．`FIXED`
- 子弾Generation Depthを一増加する．`FIXED`
- Generation Budget不足時は許可された数だけ生成し，超過数を`LIMIT`表示する．`FIXED`

完成版の弾種は，通常，貫通，分裂の三種類を現在の正本とする．新弾種はPrototype 2でこの三種類だけでは有効なBuild差が作れないと確認されるまで追加しない．`FIXED`

---

## 12．Target

### 12.1．共通Data

```text
TargetId
TargetTypeId
SocketId
BaseGold
CollisionShape
CollisionRadiusOrSize
CooldownDuration
CooldownRemaining
OwnedCount
Unlocked
VisualState
```

### 12.2．共通規則

- 通常Targetは敵ではなく収益装置である．`FIXED`
- 通常TargetはGame Logic上のHPを持たない．命中時に破壊と再生を見せる場合も，購入済みTarget StateとTarget IDは維持する．`FIXED`
- 通常Targetは盤面に残り，繰り返し利用できる．`FIXED`
- Reward VisitとSpecial Effect Visitを分ける．`FIXED`
- 同じLineageが同じTargetの特殊効果を原則一回だけ起動する．`FIXED`
- Reward再取得はTarget固有規則で許可する．`FIXED`
- MirrorとWallの物理反射はSpecial Effect Visitの対象外であり，反射上限内なら同一Lineageで再利用できる．`FIXED`
- Targetには基礎Goldを常時表示する．`FIXED`
- 命中時には倍率適用後の実Goldを表示する．`FIXED`

### 12.3．Collector

状態：`FIXED`

- 開始時Target．
- 基礎Gold 1．`INITIAL`
- 有効命中時に短い破壊Animationを再生し，0.35秒後に同じTarget ID，同じSocketで表示を戻す．Game Logic上は消滅しない．`INITIAL`
- 最初の10回は破壊回数を`0/10`で表示する．`INITIAL`
- 通常弾と分裂前の親弾を吸収する．`INITIAL`
- 貫通弾は報酬後に通過する．
- 一Lineage一回報酬．
- Cooldownなし．
- 終盤はPrimer用通常弾の安定収益起点として残る．

### 12.4．Stone Bank

状態：`INITIAL`

- 基礎Gold 5または10．`TEST`
- 命中後もProjectileを通過させる．
- 通過時に速度を20％低下させる案．`TEST`
- 一Lineage一回報酬．
- 個体Cooldown 0.25秒候補．
- 直線貫通Buildの主要収益源．

### 12.5．Mirror

状態：Prototype 0から`INITIAL`

- 直接Gold 0．
- Projectileを反射する．
- 同一Lineageでも，Remaining Reflectionsが残っていれば同じMirrorで再反射できる．`FIXED`
- 一回の反射ごとにRemaining Reflectionsを一消費する．`FIXED`
- 見た目は反射面と法線方向を明示する．

### 12.6．Amplifier

状態：Prototype 1で`INITIAL`

- 直接Gold 0．
- Projectileを通過させる．
- 次に得る一Target分のReward Multiplierを×2にする．`INITIAL`
- MultiplierはLineage共有値ではなくProjectileごとのTokenである．`FIXED`
- 同一Lineageで同じAmplifierを一回だけ起動する．`FIXED`
- 複数Amplifierの積算上限は×4とする．`INITIAL`
- Amplifier命中後に分裂した場合，各子弾が残っているTokenを継承する．`FIXED`
- 各Projectileが次にGold報酬を得た時，そのProjectileのTokenを消費する．`FIXED`

### 12.7．Material Node

状態：Prototype 1で`INITIAL`

- 少額GoldとMaterial Gaugeを与える．
- 基礎Gauge＋1．
- Gauge報酬は一Lineage一回．
- 支給時はReadyで開始する．Ready時の有効命中後，4.0秒のRechargeへ入る．`INITIAL`
- Recharge中の命中はGaugeとGoldを与えない．強いReady発光，消灯，残り時間Ringで状態を示す．`INITIAL`
- 同一Lineage一回制限と個体Rechargeの両方を満たした時だけGaugeを得る．`FIXED`
- Projectileは原則吸収し，貫通弾のみ通す．`INITIAL`

### 12.8．Core

状態：`FIXED`

- 最初から見える最終Target．
- Goldを直接出さない．
- ProjectileとLineageを終端する．
- 三枚のShield Ringと本体HPを持つ．
- 通常Target収益と同じLineage YieldでDamageを与える．

### 12.9．ProjectileとTargetの衝突Matrix

Targetの物理結果は次を基準とする．`FIXED`

| 衝突先 | 通常弾 | 貫通弾 | 分裂弾 | 備考 |
|---|---|---|---|---|
| Collector | 報酬後に吸収 | Pierceを一消費して通過 | 報酬後に分裂し親消滅 | 代表的な吸収Target |
| Stone Bank | 報酬後に通過 | 報酬後に通過 | 報酬後に分裂し親消滅 | 速度低下候補 |
| Mirror | 反射 | 反射 | 分裂せず反射 | Reflectionを一消費 |
| Amplifier | 倍率付与後に通過 | 倍率付与後に通過 | 倍率付与後に分裂し子へ継承 | 直接Goldなし |
| Material Node | Ready判定後に吸収 | Pierceを一消費して通過 | Ready判定後に分裂し親消滅 | Recharge中も物理結果は同じ |
| Core | Lineage終端 | Lineage終端 | 分裂せずLineage終端 | Shieldまたは本体を処理 |
| Wall | 反射 | 反射 | 分裂せず反射 | Reflectionを一消費 |

貫通弾の`RemainingPierces`は，CollectorとMaterial Nodeのような本来吸収するTargetを通過する時だけ消費する．StoneとAmplifierの自然通過では消費しない．`FIXED`

分裂可能TargetはCollector，Stone Bank，Amplifier，Material Nodeである．Mirror，Wall，Coreでは分裂しない．`FIXED`

TargetがCooldown中でもColliderは有効である．報酬と固有効果だけを抑止し，吸収，通過，反射という物理結果は変えない．`FIXED`

---

## 13．Gold，Material，Damage

### 13.1．通貨

| 値 | 入手 | 用途 | 状態 |
|---|---|---|---|
| Gold | Targetの有効報酬 | 反復Upgrade，同種Target追加，有限解禁のGold部分 | `FIXED` |
| Material | Material Nodeの確定Gauge | 新Target Type，新弾種，Socket，Capacity，自動化 | `FIXED` |
| Lifetime Gold | 獲得Goldの累積．消費しない | 店項目の表示，Material Node支給，Tutorial進行 | `FIXED` |

通貨はGoldとMaterialの二種類であり，Lifetime Goldは所持・消費できない進行Counterである．第三通貨，確率Drop，発射Cost，Prestige通貨は作らない．`FIXED`

### 13.2．Gold式

```text
RewardGold = floor(
    TargetBaseGold
    × GunMultiplier
    × ProjectileMultiplier
    × RouteMultiplier
)

GunMultiplier = 2 ^ GunUpgradeLevel
```

```text
RouteMultiplier = ReflectionMultiplier × RewardTokenMultiplier
```

`ReflectionMultiplier`は，そのProjectileが現在のGold報酬までに成功させた有効反射で決まる．反射0回で1.0，初回有効反射で1.5，二回以上で2.0とし，上限2.0とする．同じMirrorの往復だけで増やさず，二回目は異なるMirrorまたはWall反射を要求する．`INITIAL`

`RewardTokenMultiplier`はAmplifier Tokenがなければ1.0，あれば2.0または積算上限4.0である．Gold報酬計算後，使用したProjectileのTokenを1.0へ戻す．`FIXED`

式の適用順は固定し，途中丸めを行わず，最後にfloorする．Gold関連値は`double`で計算し，`NaN`，`Infinity`，負値を拒否する．表示時だけ整数へ整形する．`FIXED`

例：

```text
TargetBaseGold = 10
GunUpgradeLevel = 2
ProjectileMultiplier = 1.25
RouteMultiplier = 2.0

GunMultiplier = 2 ^ 2 = 4.0
RewardGold = floor(10 × 4.0 × 1.25 × 2.0) = 100
```

### 13.3．Lineage Yield

- 一Lineage内の実獲得Goldを加算する．`FIXED`
- MaterialはLineage Yieldへ含めない．`FIXED`
- 表示用に一Lineage完了時の合計を保持する．`FIXED`
- Core命中時，それまでのLineage YieldをDamageへ変換する．`FIXED`

### 13.4．Core Damage

```text
CoreDamage = DirectCoreValue
           + floor(LineageYield × CoreConversionRate)
```

- 初期Core Conversion Rateは0.5．`INITIAL`
- `DirectCoreValue = floor(10 × GunMultiplier × ProjectileMultiplier × RouteMultiplier)`とする．`INITIAL`
- 独立したDamage Upgrade Treeは作らない．`FIXED`
- Gold収益BuildとCore攻略Buildを分離しない．`FIXED`

例：

```text
Lineage Yield = 100 Gold
Direct Core Value = 30
Core Conversion Rate = 0.5

Core Damage = 30 + floor(100 × 0.5) = 80
```

### 13.5．Material取得

- 完全な独立確率Dropは使わない．`FIXED`
- Material Nodeへの有効命中でGaugeを増やす．`FIXED`
- 初期はGauge 10でMaterial一個．`INITIAL`
- 一LineageからのGauge増加は一回まで．`FIXED`
- Material NodeのBase Goldは1．`INITIAL`
- Material NodeはReady中の命中だけを有効とし，有効命中後4.0秒Rechargeする．`INITIAL`
- Recharge中もColliderは有効で，通常弾と分裂弾を吸収する．PlayerはReadyまでGold Targetを狙う．`INITIAL`
- 支給直後はReadyとし，最短では10発目の36秒で最初のMaterialを得る．以後の理論上限は40秒に一個である．`INITIAL`
- Gauge閾値を下げ，現在Gaugeが新閾値以上になった場合は即時変換し，余剰Gaugeを繰り越す．`FIXED`
- Gauge到達時は必ず取得する．`FIXED`

### 13.6．Gold Upgrade価格

価格は少数所有の三時間作品に合わせ，Levelごとの明示列を使う．式から新Levelを自動生成しない．`FIXED`

| Upgrade | 効果 | 価格列 | 上限 | 状態 |
|---|---|---|---:|---|
| Collector Value | Collector Base Gold＋1 | 10，30，75，180，450，1,100，2,800，7,000，18,000，45,000，110,000，270,000 | 12 | `INITIAL` |
| Gun Calibration | 全Target Gold×2 | 500，6,500，80,000 | 3 | `INITIAL` |
| Reload Mechanism | Reload Duration×0.85 | 400，4,500，50,000 | 3 | `INITIAL` |
| Survey Efficiency | Material Gauge閾値10→8→6 | 6,000，50,000 | 2 | `INITIAL` |

Reload Durationは`0.65→0.5525→0.4696→0.3992秒`となり，0.35秒を下限とする．Gun Calibrationを小数加算にせず二倍へしたのは，最後のfloorによって購入直後の報酬が変わらない状態を防ぐためである．`INITIAL`

Balance基準：

- 最初のUpgradeまで8秒から30秒．`TEST`
- MirrorまたはCollector二個目まで1分30秒から3分．`TEST`
- 一購入で体感収益が原則15％以上変わる．`TEST`
- 次の意味ある選択まで無操作待ち90秒未満．`TEST`
- 価格を上げるだけで完成時間を延ばさない．`FIXED`

### 13.7．返金

- Target配置とRecipe編集は常時無料．`FIXED`
- Gold Upgrade，Target購入，有限解禁は返金しない．購入前に現在値，購入後の値，価格を表示する．`FIXED`
- 有限解放は最終的にすべて取得可能とし，不可逆失敗を作らない．`FIXED`
- Skill Treeは表示上枝分かれしてよいが，排他的選択にしない．`FIXED`

### 13.8．Target購入

- New GameでCollector一個を所有する．`FIXED`
- Collector二個目は150 Gold，三個目は500 Goldとする．追加Collectorは一発の最大報酬ではなく，命中面積と別経路を増やすことを購入前に明記する．`INITIAL`
- MirrorはLifetime Gold 100で表示し，250 Goldで解禁して最初の一個を付与する．二個目は1,500 Gold，三個目は8,000 Goldとする．`INITIAL`
- Material NodeはLifetime Gold 800で予告し，Lifetime Gold 1,300で専用Socketと最初の一個を無償付与する．`INITIAL`
- Amplifierは最初のMaterial取得後に表示し，1,500 Gold＋3 Materialで解禁して最初の一個を付与する．`INITIAL`
- Target Type解禁は必ず最初の一個を含み，同じTypeを再購入させない．`FIXED`
- 同種追加個体はGoldだけを要求し，Materialを繰り返し要求しない．`FIXED`
- 購入Targetは次の空Socketへ仮配置し，直後にBoard Editingを開く．`FIXED`
- 空Socketがない場合は購入できず，必要なSocket解禁を表示する．`FIXED`
- 配置変更とInventoryへの撤去は無料である．`FIXED`

### 13.9．有限解禁

| 順 | Node | 前提 | 価格 | 付与 | 状態 |
|---:|---|---|---:|---|---|
| 0 | Mirror Routing | Lifetime Gold 100 | 250 Gold | Mirror一個，Board Editing | `INITIAL` |
| 1 | Material Survey | Lifetime Gold 1,300 | 無償 | Material Node一個，専用Socket | `INITIAL` |
| 2 | Amplifier Circuit | Materialを一個以上獲得 | 1,500 Gold＋3 Material | Amplifier一個 | `INITIAL` |
| 3 | Recipe Workbench | Amplifier Circuit | 4,000 Gold＋8 Material | Recipe Editor，Capacity 4 | `INITIAL` |
| 4A | Pierce Loading | Recipe Workbench | 9,000 Gold＋12 Material | 貫通弾 | `INITIAL` |
| 4B | Split Loading | Recipe Workbench | 12,000 Gold＋16 Material | 分裂弾 | `INITIAL` |
| 5 | Capacity＋1 | 4Aまたは4B | 25,000 Gold＋20 Material | Capacity 5 | `INITIAL` |
| 6 | Fixed Auto-fire | Capacity＋1 | 50,000 Gold＋25 Material | 固定方向Auto-fire | `INITIAL` |

4Aと4Bは購入順を選べるが，片方を買っても他方を失わない．近い未解禁NodeはSilhouetteと条件を表示する．`FIXED`

---

## 14．CoreとEnding

### 14.1．Shield Ring

Coreは三段階の到達目標を持つ．`FIXED`

1．外輪．累計Gold条件で解除する．  
2．中輪．一Lineageで指定された三種類のTargetへ命中後，Coreへ到達して解除する．  
3．内輪．五発一周期の中で，通常，貫通，分裂をそれぞれCoreへ到達させて解除する．  
4．本体．Lineage Yieldを使ってHPを削る．

必要Gold，Target組合せ，Core HPはPrototype 4後に決める．`TEST`

- 外輪条件を満たした時は自動解除し，Core命中を要求しない．`FIXED`
- 中輪と内輪が有効な間，Core命中は条件評価後にLineageを終端し，本体Damageを与えない．`FIXED`
- 中輪は，Core到達LineageのVisited Target Type集合を評価する．`FIXED`
- 内輪はRecipe Cycle IDごとにCoreへ到達したRoot Projectile Type集合を記録し，通常，貫通，分裂が同一Cycle ID内で揃った時に解除する．`FIXED`
- 内輪の途中記録はCycle ID別に保持する．そのCycleに属する五Root Lineageがすべて完了した時点で成功判定し，条件未達なら破棄する．固定周期数による期限は設けない．`FIXED`
- 最初のCore命中Projectileで，そのLineageに属する全Projectileを終了する．`FIXED`
- Core Damageは終端時点までに確定したLineage Yieldだけを使う．`FIXED`
- GoldはCore Damageへ変換しても消費しない．`FIXED`
- 同一LineageがCoreへ複数回Damageを与えることはない．`FIXED`

### 14.2．Overdrive

- 盤面をResetしない．`FIXED`
- Capacityを8候補へ増やす．`INITIAL`
- Overdrive Burstを解放する．`INITIAL`
- Core Shieldの最終段階を開く．`FIXED`
- 音楽，背景，Projectile表現を一段変える．`INITIAL`
- Prestigeの高速再構築快感を，一盤面内の制約破壊へ変換する．`FIXED`

### 14.3．Ending

- Core HPが0以下になった論理Tickで勝利を確定する．`FIXED`
- Core HPが0以下になったTickでは現在処理中の衝突まで確定し，新規発射を停止し，残Projectileを報酬なしで終了してEndingへ移る．`FIXED`
- Ending演出後，Creditsを表示する．`FIXED`
- Ending後は盤面閲覧，Ending再生，Main Menu，New Gameを提供する．`INITIAL`
- 無限経済や新通貨を追加しない．`FIXED`

---

## 15．自動化

### 15.1．設計目的

自動化は，Playerが最初に手動で行っていた単純操作を委譲する権限成長である．手動倍率を直接付けず，盤面理解による収益差を残す．`FIXED`

### 15.2．Auto-fire

- 最後に成功した手動射撃のAim Directionへ自動発射する．`FIXED`
- Gun Cycle Timeに従う．`FIXED`
- Mouse移動だけではAuto方向を変更しない．成功した手動射撃時だけ上書きする．`FIXED`
- 同一Tickに手動とAutoの発射要求がある場合，手動を優先して一発だけ撃つ．`FIXED`
- 次弾，Target Cooldown，反射経路を判断しない．`FIXED`
- 解放候補は20分から40分．`TEST`

### 15.3．手動割込

- 手動入力を常に優先する．`FIXED`
- 最終手動入力後，候補3秒でAutoへ戻る．`INITIAL`
- UIで現在制御元を表示する．`FIXED`

### 15.4．効率目標

固定方向Autoを100とした仮説：

```text
固定方向Auto = 100
普通の手動 = 130
熟練手動 = 160
```

自動が熟練手動の60％未満なら弱すぎ，85％超なら照準判断を消すRiskがある．同一Save，同一60秒区間を三回測り，中央値で比較する．`TEST`

---

## 16．進行

### 16.1．仮進行表

| 経過 | 主操作 | 解放 | 見た目 | 目標 | 状態 |
|---:|---|---|---|---|---|
| 0から0.5分 | Collectorを約10回狙う | Collector Value | `0/10`，破壊Feedback | 最初の＋1 | `INITIAL` |
| 0.5から3分 | Gold強化と配置購入 | Collector二個目，Mirror | 値上昇，Target追加，初反射線 | 直接射撃と反射経路を比較 | `INITIAL` |
| 2から5分 | Gold経路を育てる | Reload一段，全Gold×2一段 | 発射Tempoと全表示値の跳躍 | 次の質的解放へ備える | `INITIAL` |
| 3.5から7分 | Ready状態を見て照準を切替 | Material Node無償支給 | Material Gauge，Recharge Ring | Gold経路とMaterial経路を切替 | `INITIAL` |
| 6から10分 | Materialを集めつつGoldを稼ぐ | Amplifier | 倍率Token | 反射と倍率を組み合わせる | `INITIAL` |
| 10から16分 | 五発順を編集する | Recipe，Capacity 4 | Recipe Bar | Primerを理解する | `INITIAL` |
| 18から30分 | 弾ごとに照準を変える | 貫通，分裂 | 貫通線，子弾 | 二Build作成 | `INITIAL` |
| 30から50分 | 経路を増やす | Capacity 5，追加Socket，Stone候補 | 盤面横拡張 | Core外輪 | `INITIAL` |
| 45から70分 | 手動とAutoを切替 | Fixed Auto-fire | 固定方向運転 | 手動割込の価値を測る | `INITIAL` |
| 60から120分 | 配置とRecipeを統合 | Capacity 6候補，Core中輪 | 高密度経路 | 三Target連鎖 | `TEST` |
| 120から150分 | Core経路を完成 | Overdrive，Capacity 8，Burst | 音楽と密度変化 | Core内輪 | `INITIAL` |
| 150から180分 | 最終Recipe比較 | 新Systemなし | Core露出 | Yield最大化とCore破壊 | `TEST` |
| 165から180分 | 最終運用 | 新Systemなし | 全盤面がCoreへ収束 | Core破壊 | `TEST` |

### 16.2．進行原則

- 質的解放間隔は原則20分から30分以内．`FIXED`
- 最後の30分は新しい文法を教えず，既存文法を統合する．`FIXED`
- 60分以降に量的Upgradeだけの空白が出る場合，既存のSocket，Stone，Capacityの価格と順序を調整し，新Target Typeを安易に追加しない．0から60分の実収益を得る前に後半価格を指数外挿しない．`FIXED`
- Core撃破予測が20分を超える場合，HPを下げる．`TEST`
- 2時間で密度が尽きた場合，3時間へ価格で引き延ばさない．`FIXED`

---

## 17．UI

### 17.1．基準Layout

Prototype 2の表示PatchではUser指示によりFHD（1920×1080）を基準とする．盤面論理座標は変えず，UI文字を小さめに整理し，整数Pixelで描画する．日本語の行高に十分な枠を確保し，NOW／弾名／PrimerとFooterの上下欠けを防ぐ．`INITIAL`

```text
┌ Gold 12.4K   Material 7/10   Core 42% ───────── [Pause] [設定] ┐
│                                                               │
│                         巨大CORE                              │
│              Target／Socket／弾道がある盤面                   │
│                                                               │
│                         散布CONE                              │
│                           [銃]                                │
├───────────────────────────────────────────────────────────────┤
│ NOW   NEXT→                                                   │
│ [通常] [通常] [分裂] [通常] [貫通]   Primer表示   容量 5/6  │
│ [Recipe編集] [Target配置] [Auto：固定方向] [Upgrade]          │
└───────────────────────────────────────────────────────────────┘
```

### 17.2．Recipe表示

- 現在弾150％Scale．`INITIAL`
- 次弾120％Scale．`INITIAL`
- 残り三弾100％Scale．`INITIAL`
- 特殊弾SlotにはPrimer 0から2を記号で表示する．`FIXED`
- Capacityは`使用量／上限`で常時表示する．`FIXED`

### 17.3．弾種表現

色だけで区別しない．`FIXED`

| 弾 | 形 | 軌跡候補 |
|---|---|---|
| 通常 | 丸 | 実線 |
| 貫通 | 針 | 二重線 |
| 分裂 | 三叉 | 枝線 |

### 17.4．数値表示

- Target本体に基礎Gold．`FIXED`
- 命中時に実Gold．`FIXED`
- 同Target，同0.1秒窓のFloating Numberを合算する．`INITIAL`
- 同時Floating Number上限12．`INITIAL`
- Lineage完了時に`Lineage total`を一回表示する．`FIXED`
- Core命中時はGold由来Damageと直接DamageをTooltipで確認可能にする．`FIXED`

### 17.5．情報開示

- Tooltipで実計算を表示する．`FIXED`
- 例：`基礎10 × Gun 2.0 × 経路2.0 = 40`．
- 強い完成RecipeはTutorialで教えない．`FIXED`
- Primerという文法は隠さない．`FIXED`
- 次の質的解放はSilhouetteまたは短い説明で予告する．`FIXED`
- Active RecipeとPending Recipeを別表示し，Pendingが反映される周期境界を示す．`FIXED`
- Lifetime Gold条件は`現在累積／必要累積`で表示し，Gold消費で減らないことを説明する．`FIXED`

---

## 18．Tutorial

### 18.1．開始

- 起動後5秒以内に発射可能にする．`FIXED`
- 最初に文章Modalを出さない．`FIXED`
- 初期Collectorを大きめにする．`INITIAL`
- 最初のMissが壁反射で戻る配置候補．`TEST`

### 18.2．集中と拡散

- 近Cursorと遠CursorのConeを短いGhost表示で比較する．`INITIAL`
- Textは一文以内にする．
- Playerが距離を変えた時に表示を終了する．`INITIAL`

### 18.3．Target

- Mirror購入前にNPC ShotまたはGhost Shotで一度反射を実演する．`INITIAL`
- Target効果は文章だけでなく，盤面上の一回実演を優先する．`FIXED`
- 開始時はCollector破壊数を`0/10`で示し，10 Gold到達時に最初の`＋1`だけを強調する．`INITIAL`
- Material Node支給時，Ready命中だけがGaugeを増やし，Gauge 10でMaterial一個になることを盤面上で実演する．`INITIAL`
- Material NodeのRecharge中はCollectorを狙う短いGhost Guideを一度だけ表示する．`INITIAL`

### 18.4．Recipe

- 最初の特殊弾取得時に解放する．`FIXED`
- EditorでPrimer線を可視化する．`FIXED`
- `通常→分裂`と`分裂→通常`のPrimer差をPreviewする．`FIXED`
- 完成Buildは提示しない．`FIXED`

---

## 19．Audio，演出，Accessibility

### 19.1．最低演出

- 発射音．
- Reload完了音．
- Target命中音．
- Mirror反射音．
- 分裂音．
- Material取得音．
- Core Shield解除音．
- Targetの短いScaleまたは発光．
- Projectile軌跡．

Prototype 0でも，発射，着弾，Reload完了の三Feedbackは入れる．射撃感を無音Placeholderだけで評価しない．`FIXED`

### 19.2．大量Event

- 同一0.1秒窓の同種音をまとめる．`INITIAL`
- 音量だけでなくPitchとLayerで規模を表現する．`INITIAL`
- 大量命中時に音割れさせない．`FIXED`

### 19.3．設定

- Master，Music，SFX音量．`FIXED`
- Fullscreen，Window Size．`FIXED`
- Screen Shake 0から100％．`INITIAL`
- Flash軽減．`FIXED`
- Projectile透明度または効果密度．`TEST`
- UI Scale．`DEFERRED`だがSteam公開前に再評価する．
- 色覚だけに依存しない形状区別．`FIXED`

---

## 20．Unity実装設計

### 20.1．Environment

- Unity 6000.3.18f1．ProjectVersionと全開発機で完全一致させる．`FIXED`
- C#．`FIXED`
- Windows 10／11 x64．`FIXED`
- 2D Project．`FIXED`
- URPは2D LightまたはShader要件確定後に判断する．`TEST`

### 20.2．責務分離

```text
Pure C# Simulation
├ Game State
├ Recipe／Primer
├ Projectile／Lineage
├ Target Logic
├ Event Queue
├ Economy
├ Core Progress
├ Save DTO
└ Test

Unity Presentation
├ Input
├ Physics2D Query Adapter
├ Sprite／Animation
├ UI
├ Audio
├ Particle
├ Camera
└ Platform／File Access
```

SimulationはMonoBehaviourへ依存させないことを原則とする．Unity固有型を境界に限定し，Pure C# Testを可能にする．`FIXED`

### 20.3．推奨Interface

```csharp
public interface IGameSimulation
{
    void Tick(SimulationInput input);
    SimulationSnapshot GetSnapshot();
}

public interface ICollisionQuery
{
    CollisionHit? CastCircle(
        SimVector2 origin,
        float radius,
        SimVector2 direction,
        float distance);
}

public interface IRandomSource
{
    float NextFloat01();
    RandomState CaptureState();
}

public interface ISaveRepository
{
    SaveData Load();
    void Save(SaveData data);
}
```

実装時の型名は変更可能だが，責務境界は維持する．`FIXED`

### 20.4．Scene

Prototype 0は一Sceneとする．`FIXED`

```text
Prototype0
├ GameRoot
│  ├ SimulationController
│  ├ InputController
│  ├ CollisionQueryAdapter
│  └ TestLogger
├ Board
│  ├ GunView
│  ├ CollectorView
│  └ WallView
├ ProjectileViewPool
├ UI
└ Audio
```

完成版候補：

```text
Boot
MainMenu
Game
```

Scene細分化は必要性が出るまで増やさない．`FIXED`

### 20.5．ScriptableObject

Target，Projectile Type，Upgradeの静的定義に使ってよい．Runtime StateをScriptableObjectへ直接保存しない．`FIXED`

```text
ProjectileDefinition
TargetDefinition
UpgradeDefinition
ProgressionDefinition
AudioDefinition
```

Prototype 0ではData基盤を作りすぎず，Prototype 1で二種類以上の定義が必要になった時に導入する．`FIXED`

### 20.6．Object Pool

- Projectile ViewをPoolする．`INITIAL`
- 論理Projectile StateはPoolの有無に依存しない．`FIXED`
- DOTS，ECS，Job SystemはProfilerで必要性が確認されるまで導入しない．`FIXED`
- Addressablesは初期版へ入れない．`FIXED`

### 20.7．Unity Project設定

```text
Asset Serialization = Force Text
Version Control Mode = Visible Meta Files
```

`.meta`はAssetと同時にVersion管理する．`FIXED`

---

## 21．SaveとVersion

### 21.1．Save形式

JSONまたは同等のVersion付きDataとする．`INITIAL`

```json
{
  "schemaVersion": 1,
  "gameVersion": "0.1.0-prototype0",
  "gold": 120,
  "lifetimeGold": 540,
  "material": 2,
  "materialGauge": 4,
  "activeRecipe": [0, 0, 2, 0, 1],
  "pendingRecipe": null,
  "recipeSlotIndex": 3,
  "recipeCycleId": 14,
  "capacity": 4,
  "core": {
    "shieldStage": 1,
    "hp": 1000
  }
}
```

### 21.2．Save契約

- `schemaVersion`を必須にする．`FIXED`
- v1.0.0前はSave互換を保証しない．`FIXED`
- 非互換時は無言で初期化しない．警告とBackupを行う．`FIXED`
- Upgrade購入，配置確定，Recipe確定，Core進行時に保存候補．`INITIAL`
- 定期保存30秒候補．`INITIAL`
- 終了時保存．`FIXED`
- 前回Save Backupを一世代保持する．`INITIAL`
- Reset Saveを用意する．`FIXED`
- Save Snapshot作成中もLive Simulation Stateを変更しない．`FIXED`
- Load時は飛翔中Projectile，未完了Lineage，未完了Core Cycle記録を破棄し，現在Slotを装填済みとして再開する．`FIXED`

### 21.3．Version表示

```text
v0.1.0-prototype0
commit 7af31c2
schema 1
```

Main Menu，Pause，Playtest Logへ表示する．`FIXED`

文書Version，Game Version，Save Schema Versionを混同しない．本書の`0.3.0`は設計文書の改訂番号，`0.1.0-prototype0`は配布Build，`schema 1`はSave構造である．`FIXED`

---

## 22．Playtest Log

### 22.1．Privacy

- Prototypeは自動Telemetryを送信しない．`FIXED`
- Local JSONまたはCSVへ出力する．`FIXED`
- Playerが任意でGitHub Issueへ添付する．`FIXED`
- User名，PC名，絶対Path，IP Addressを記録しない．`FIXED`

### 22.2．共通記録

```text
Game Version
Commit Hash
Schema Version
Session Seed
Session Duration
First Shot Time
First Hit Time
Shot Count
Hit Count
Miss Count
Reload Click Count
Reflection Hit Count
Recipe Edit Count
Board Edit Count
Gold per Minute
Max Active Projectile
Max Lineage Duration
Max Lineage Yield
Frame Time Summary
Exception Summary
```

### 22.3．Event形式候補

```json
{
  "tick": 184,
  "event": "projectile_hit",
  "projectileId": 14,
  "lineageId": 8,
  "targetId": 3,
  "reward": 50
}
```

Debug Buildだけ詳細Eventを保存し，公開Buildは集約Logへ切り替えられるようにする．`INITIAL`

---

## 23．Prototype計画

### 23.1．Prototype 0．射撃感

実装：

- 単発Gun．
- Aim Directionと距離散布．
- Cone．
- Collector一個．
- 1 Gold．
- Auto Reload．
- 壁一枚と反射．
- Seed付き乱数．
- 最低AudioとHit Feedback．
- Playtest Log．
- Collector破壊表示`0/10`．
- Gold所持とLifetime Gold．
- `Collector Value＋1`の初回10 Gold購入．

入れない：複数Upgrade，Socket編集，Material，Recipe，Core Damage，Save Migration．

検証：

- 一発撃つだけで気持ちよいか．
- 近距離拡散，遠距離集中を理解できるか．
- Miss理由を理解できるか．
- Reloadが期待か退屈か．
- 約10回の命中後に`＋1`を買う循環が理解できるか．

第一波は5人の探索的Testとし，割合の統計判定に使わない．修正後，第二波10人から12人で次を確認する．

- 10人中9人以上が5秒以内に発射．
- 10人中8人以上が三発以内に命中．
- 10人中7人以上が距離と散布の関係を説明．
- Reloadを明確に退屈と答える人が3人未満．
- 10人中8人以上が30秒以内に最初のUpgradeを購入．

### 23.2．Prototype 1．盤面成長

最初の実装はPrototype 1A「自由配置」とする．実装範囲は`AI_PROTOTYPE1A_IMPLEMENTATION_BRIEF_2026-09-05.md`で限定する．自由配置，Mirror回転，Collector二個，Mirror一個，Amplifier一個を使い，置き直してすぐ撃てる試作を作る．

後続候補：Material Node，Target購入，Gold Upgrade，有限解禁．順序と採否はUserの試遊結果から決める．AI駆動で制作を進める現段階では，多人数Playtestを次の内部実装の必須条件としない．動作と当たり判定の自動Testは維持する．正式Release向けの評価条件とは区別する．

成功候補：

- 参加者の大半が二配置以上を試す．
- 配置変更で20％以上の収益差を出せる．
- 高価Targetだけを直接狙う一択にならない．
- Collectorが連鎖起点として残る．
- Materialの入手方法を支給後60秒以内に理解する．
- 最初のMaterial取得が支給後30秒から60秒に収まり，Recharge中にGold Targetへ照準を戻す．
- NodeをRecharge中にも連打した参加者が半数を超える場合，Ready表示または仕組みを再設計する．
- Gold不足とMaterial不足のどちらも一方だけが恒常的な壁にならない．

失敗：全員が同じSocket，同じTargetだけを使う．

### 23.3．Prototype 2．五発Recipe

Userの2026年9月5日の試遊OKと次段階指示により，購入経済より先に本段階へ進む．実装範囲は`AI_PROTOTYPE2_IMPLEMENTATION_BRIEF_2026-09-05.md`．P1A盤面・初期配置を継続し，初期Recipeは通常・通常・通常・通常・分裂，Capacity 4，全弾支給済みのSandbox．旧試作を保持する．`INITIAL`

実装：通常，貫通，分裂，Primer，Capacity 4，Recipe UI，Lineage，Visited集合．

成功候補：

- 参加者が二つ以上の有効Recipeを作る．
- Primerによる結果差を説明できる．
- Recipe変更後にAim先または発射判断が変わる．
- 一種類で全Slotを埋める構成が支配しない．

Go／No-Go：

五発順を変えてもPlayerのAimが変わらず，結果差を説明できない場合，五発Recipeを本作の主軸として正式仕様へ進めない．Recipeを削り，Target配置主軸へ再設計する．`FIXED`

### 23.4．Prototype 3．自動化

実装：固定方向Auto-fire，手動割込，60秒収益計測．

成功候補：

- Autoが熟練手動の60％から80％．
- Auto解放後も手動介入が残る．
- 委譲が権限成長に感じられる．

### 23.5．Prototype 4．進行Slice

実装：30分から60分，Gold，Material，三Target，三弾，Capacity，Auto，Core外輪，仮Ending，Save．

成功候補：

- 無選択区間90秒未満．
- 次目標を大半が説明できる．
- 通常Gold BuildとCore Buildが分離しない．
- 量的Upgradeと質的解放が交互に来る．

Prototype 4後に完成尺，Core HP，最終弾，Overdrive Timingを決める．`FIXED`

---

## 24．Test方針

### 24.1．Pure C# Test

最低限，次を自動Testする．

- Recipe循環Index．
- Primer 0，1，2の計算．
- 五番目から一番目へのPrimer継承．
- Capacity計算．
- Gold式の丸め．
- Collector Value価格列．
- Gold購入時のGoldとLifetime Gold分離．
- Material Gaugeの確定変換と余剰繰越．
- Lineage Yield加算．
- Core Damage変換．
- Visitedによる重複効果防止．
- 子弾のLineage継承．
- Generation Budget．
- Event安定順序．
- Save Schema読込．

### 24.2．Unity Play Mode Test

- CircleCastですり抜けない．
- Mirror法線で反射する．
- Corner接触で無限再衝突しない．
- UI Clickで誤射しない．
- Recipe編集時にSimulationが停止する．
- Window Resizeで論理配置が変わらない．

### 24.3．Performance Test

- 64論理Projectileの一Lineage．
- 250描画Projectile．
- View省略が論理報酬を変更しないこと．
- 大量Floating Number集約．
- Save中のFrame停止．

正式なFrame Budgetは最低対象PC決定後に固定する．`TEST`

---

## 25．GitHub運用

### 25.1．Repository

推奨：

```text
private source repository
public release repository
```

Source公開方針はPrototype 2のGo判定後に決める．`FIXED`

### 25.2．Version

```text
v0.1.0-prototype0
v0.2.0-prototype1
v0.3.0-prototype2
v0.4.0-prototype3
v0.5.0-slice
v0.9.0-beta
v1.0.0
```

公開Tagを移動しない．修正はPatch Versionを増やす．`FIXED`

### 25.3．Branch

- `main`は常に開いてBuildできる状態．`FIXED`
- 短期`feat/`，`fix/`，`test/`，`docs/`Branch．`FIXED`
- 一人開発でもPull Requestで差分を確認する．`INITIAL`
- 常設`develop`Branchは作らない．`FIXED`

### 25.4．Milestone

```text
P0 Shooting Feel
P1 Board Growth
P2 Five-Shot Recipe
P3 Automation
P4 Progression Slice
```

### 25.5．Release

- GitHub ReleasesのPre-releaseとして配布する．`FIXED`
- BuildをGit RepositoryへCommitしない．`FIXED`
- Windows x64 ZIPとSHA-256を添付する．`FIXED`
- Release Notesへ操作，検証目的，変更，既知問題，Save互換，Commitを記載する．`FIXED`

```text
OneBoardPrototype0-v0.1.0-prototype0-Windows-x64.zip
OneBoardPrototype0-v0.1.0-prototype0-Windows-x64.zip.sha256
```

### 25.6．Unity Git設定

GitHub公式Unity `.gitignore`を基準にし，次を除外する．

```text
Library/
Temp/
Obj/
Build/
Builds/
Logs/
UserSettings/
Artifacts/
*.csproj
*.sln
```

次を管理する．

```text
Assets/
Packages/
ProjectSettings/
docs/
.github/
.gitignore
.gitattributes
README.md
```

Source公開時は，公開範囲とAsset再配布条件を確定してからLICENSEを追加する．LICENSE未決定のSource RepositoryをPublicへ変更しない．`FIXED`

### 25.7．Asset License

Assetごとに作者，入手元，License，改変，再配布条件，Credit条件を記録する．有料Assetまたは再配布禁止AssetをPublic SourceへCommitしない．`FIXED`

### 25.8．GitHub Actions

- Prototype 0はRepository構造だけをGitHub Actionsで検査し，Unity Test，Build，Releaseは手元で行う．`FIXED`
- Prototype 1からPure C# Test自動化候補．`INITIAL`
- Prototype 2からUnity Test Runner自動化候補．`INITIAL`
- 進行Slice以降にTagからWindows Build自動生成を検討する．`DEFERRED`

---

## 26．Steam移行

- Prototype段階でSteamworksへ依存させない．`FIXED`
- Platform ServiceはInterfaceで隔離する．`FIXED`
- Steam Achievement，Cloud Save，Rich PresenceはBeta以降．`DEFERRED`
- GitHub版とSteam版でGame Logicを分岐させない．`FIXED`
- Steam Cloud導入時もLocal Backupを残す．`DEFERRED`
- Steam DeckとLinuxはWindows版のProton確認から始める．`DEFERRED`

---

## 27．未確定事項

以下は本書の欠落ではなく，Prototypeでしか決められない事項である．

| 項目 | 現在値 | 決定に必要な結果 | 決定時期 |
|---|---|---|---|
| 完成尺 | 150分から180分 | P4の空白時間と離脱 | P4後 |
| Cycle Time | 0.65秒 | Reload評価と次弾認知 | P0，P2 |
| 散布角 | 2度から14度 | 命中率，距離理解 | P0 |
| 散布分布 | `[-spread,+spread]`の一様角度 | 射撃感比較 | P0 |
| 全Socket数 | 14から16 | 有効配置数，可読性 | P1 |
| Mirror再訪 | 未確定 | 無限反射と理解 | P1 |
| Stone通過減速 | 20％候補 | 直線Buildの読みやすさ | P2 |
| Capacity曲線 | 4→5→6→8 | Recipe分布 | P2，P4 |
| Core条件とHP | 未確定 | 実収益曲線 | P4後 |
| Material Recharge | 4.0秒 | 照準切替，初Material時刻，作業感 | P1，P4 |
| 60分以降の価格 | 未確定 | P4のGold／Material実収益 | P4後 |
| Projectile上限 | 64／250候補 | Profilerと可読性 | P2以降 |
| Source公開 | Private候補 | Project継続とAsset License | P2後 |
| URP | 未確定 | 演出要件 | P1以降 |

未確定事項をAIが独断で正式決定してはならない．Prototypeに必要な場合は，本表の現在値を`INITIAL`として使用する．

---

## 28．Definition of Done

### 28.1．機能Issue

- 仕様参照先がある．
- 受入条件がある．
- Pure C# Logicに必要なTestがある．
- Unity上で手動確認した．
- Debug Logに過剰なErrorがない．
- 既存Save影響を記録した．
- Performance上限へ影響する場合，測定した．
- 本書との不一致がない．

### 28.2．Prototype Release

- Clean環境で起動できる．
- VersionとCommit Hashが表示される．
- 操作説明がある．
- Known Issuesがある．
- Playtest LogをExportできる．
- SHA-256がある．
- Release Assetが正しいTagからBuildされている．
- 次のPrototypeへ進む判断条件が書かれている．

---

## 29．実装開始順

Prototype 0の実装順は次とする．

```text
1．Unity ProjectとGit初期設定
2．固定論理座標とCamera
3．InputとAim Direction
4．Cone表示
5．Seed付き散布
6．Projectile StateとView
7．CircleCast移動
8．Collector衝突と1 Gold
9．Collector破壊表示0/10
10．GoldとLifetime Gold
11．10 GoldのCollector Value＋1
12．壁反射
13．Reload State
14．AudioとHit Feedback
15．Playtest Log
16．Pure C# Test
17．Windows Build
18．GitHub Pre-release
19．第一波Playtest
```

この順序中に複数Upgrade，Material，Recipe，Core Damage，Skill Treeを混ぜない．

---

## 30．変更履歴

### 0.7.2．2026年9月5日

- Clear後の待機が停止に見える問題に対し，中央へCLEAR／次へ／もう一度を表示。最終Stageでは次へを無効化し，全3Stage達成を明示。経済・敵強化の追加は今回行わない。

### 0.7.1．2026年9月5日

- User試遊の「全弾待ちはストレス，最後に滑る弾を待ちたくない」を受け，§33.5で残弾速度300以下なら再射撃できるPatchを定義。旧弾の攻撃・寿命を保持し，重複するマガジンの生成予算を独立管理する。

### 0.7.0．2026年9月5日

- User承認の「空間と命中演出→全弾待機と効果→2銃→GoldとStage進行」を§33の小規模Challenge試作として実装。旧版は保持する。
- 1チャレンジの携行マガジン数を強化し，1マガジン全破壊で特殊効果を開放する。User追記により分裂は子弾にも連鎖し，斉射全体10秒の寿命を共有する。

### 0.6.2．2026年9月5日

- User指示により、FHD横長Window内へ「左ログ・中央9:16ステージ・右弾倉/強化領域」を配置する試作を§32.2へ追加。
- 盤面形状と配置を変更し、旧横長を保持。ダメージ等の数値は変更せず、実効DPS計測を追加する。

### 0.6.1．2026年9月5日

- Userの参考画像と実装指示により、Momentum Labへネオン結晶の2.5D表示比較版を追加。§32の数値、当たり判定、音、Save形式を変更しない。表示仕様は§32.1。

### 0.6.0．2026年9月5日

- User承認の速度資源・抵抗Target・マガジン一括発射・移動強化ゾーン方針を§32へ正本化した．
- 過去Prototypeを保持し，Momentum Labを別Scene／製品として追加する．旧FIXEDの変更範囲はこの新試作に限定する．

### 0.5.1．2026年9月5日

- User提供Screenshotの文字欠け・過大表示を受け，Prototype 2のFHD文字サイズ・行高・Pixel描画基準を追記．Game Logicは変更しない．Game Versionは0.3.1-recipe．

### 0.5.0．2026年9月5日

- Userの次段階指示によりPrototype 2を自由配置版から分離し，五発Recipe・貫通・分裂を実装対象にした．
- 子弾の弾種・継承，深度上限時の終了，貫通残数0の処理をINITIALとして明文化した．
- 反射パズル案は未採用のアイデアメモへ保存し，製品ジャンルは変更していない．

### 0.4.0．2026年9月5日

- User承認により，固定Socketからグリッド吸着付き自由配置へ変更した．
- Mirror回転，形状に一致する重なり判定，配置履歴の保存を定義した．
- 次の試作をPrototype 1Aとして切り出し，旧Prototype 0のSceneとBuildを保持する．
- 趣味制作・AI駆動制作の体験を優先し，多人数の面白さ検証を内部試作の進行条件としない方針を記録した．

### 0.3.0．2026年8月28日

- 18作品の再調査とReference Modelに基づき，旧経済曲線を定量監査した．
- Collector Valueが理想条件で約4分以内にLevel 12へ到達する旧価格列を廃止し，明示価格列へ変更した．
- 最後のfloorにより効果が0になるGun Multiplier＋0.25を廃止し，少数回の全Gold×2へ変更した．
- Material Nodeへ4.0秒のReady／Rechargeを追加し，理論上限を一個40秒へ変更した．
- Mirror，Material Node，Amplifier，Recipe，弾種，Capacity，Autoの初期価格と表示条件を再設定した．
- 0から60分のReference進行を更新し，60分以降の価格はP4実測前に指数外挿しない規則を追加した．
- Unity Editorを6000.3.18f1へ固定した．

### 0.2.0．2026年8月28日

- Gold，Lifetime Gold，Materialの責務と最初の10回循環を統合した．
- Mirror，Material Node，Amplifier，Recipe，弾種，自動化の初期価格と解禁順を固定した．
- Target Type解禁時に最初の一個を付与し，同種追加はGoldだけで購入する規則を追加した．
- ChamberとReload状態機械，AmplifierからSplitへのEvent順，Projectile上限，Core Cycle寿命，Save境界を修正した．
- Gold Upgrade返金，未生成弾の疑似報酬，Sweep Servo，Relay，Prism，追加弾候補を正本から除外した．
- Prototype 0へ最初の10回と一回の`Collector Value＋1`を追加した．
- AI実装時はMasterと対象Prototype Briefだけを参照する規則を追加した．

### 0.1.0．2026年8月28日

- 監査資料，類似作品調査，Unity方針，GitHub運用を統合した．
- 一盤面，Target収益装置，五発Recipe，Lineage，二通貨，Core Endingを正本化した．
- Ascensionを削除し，Overdriveへ置換した．
- 通常弾の命中時Charge案を廃止し，決定論的な装填時Primer方式へ変更した．
- Rigidbody2D Callbackではなく，固定TickとCircleCastによる衝突検索を基準とした．
- AI運用，Save，Test，GitHub Release契約を追加した．

---

## 31．現在の最終判断

以下は五発版時点の判断として保持する．最新実装対象は§32．

現在はPrototype 0とPrototype 1AのUser動作確認を経て，Prototype 2の五発Recipeへ進める．旧試作の実装範囲，復元用Scene，Buildは維持する．正式仕様として最も重要な未検証点は，Prototype 2において五発Recipeの順番がPlayerの照準と結果を実際に変えるかである．

Prototype 0から2が成立するまで，完成時間，Content数，Core HP，後半弾種を確定しない．Prototype 2が失敗した場合，既存仕様へ機能を足して救済せず，五発Recipeを主軸から外す再設計を行う．

## 32．速度資源型マガジン・ピンボール（現在の試作）

Userは，単発五発循環より「仕込んだマガジンを一クリックで撃ち切る」遊び，ランダムな的と障害物，動く効果ゾーン，速度を消費する貫通を承認した．以下を今回のINITIAL正本とする．

- 三Slot通常／貫通，初期通常・貫通・通常．一クリックで0.12秒間隔の三発，クリック時の照準固定，各弾±2度のSeed付き散布．撃ち切り後Reload0.8秒，残弾待ちはしない．編集Pauseは飛行弾・ゾーン・Reloadを保持し停止する．
- 弾速900，最大1800，減速120/s，80以下で消滅，半径8．通常の基礎威力40・抵抗係数1，貫通は26・0.25．
- ダメージは基礎威力×min(命中直前速度/900,2)．HP処理後に速度を抵抗×係数だけ失う．閾値以下なら消滅，それ以上なら通過．アーマーは減速抵抗だけであり弾種による無効化なし．
- 普通Target4個＝HP50／抵抗150／半径34／破壊Gold3．装甲Target3個＝HP90／抵抗420／半径42／破壊Gold7．破壊時に一度だけGold．再訪Hit可，同じ接触の多重Hitは禁止．
- Seed付きランダム初期配置．破壊済みTargetだけ次の斉射開始時に別位置へ補充．生存Targetと障害物の配置は維持する．他の物体・Gun・残弾から離れた候補を選び，候補がなければ次の斉射へ保留．
- 外壁とランダム円形障害物2個（半径26）は反射，速度×0.9．旧反射報酬倍率は使わない．
- 加速ゾーン1個，半径65，中心(940+320sin(0.8t),550)．通過した弾を速度×2（1800上限），同一弾には一回．移動ゾーンとの相対運動も接触検索する．
- 60tick/s，Swept Circleによる最初の接触検索．各弾8接触／Tickで残時間を持越す．同時接触はTarget→障害物→壁→ゾーン，次にID順．
- 論理領域1600×900，Play領域X320〜1560,Y120〜760，Gun(940,725)．文字はFHD基準．Seed表示，Localのみの個別Logと弾倉の日時付き保存履歴．
- ゴールド／カオスゾーン，爆発・分裂・裂傷弾，銃種／弾倉Upgradeは将来候補．今回の追加実装はしない．
- `AI_MOMENTUM_LAB_IMPLEMENTATION_BRIEF_2026-09-05.md`で範囲を限定する．Game Version 0.4.0-momentum，旧版を残した別Scene／Build／製品保存先．

### 32.1．Neon Crystal表示比較（0.4.1-neon）

- 黒い床、緑の通常的、橙の装甲的、紫の反射障害物、水色の加速ゾーン。面取りした立体Meshをほぼ真上（面から87度相当）で表現する。Game Cameraと論理座標の対応は維持し、Mesh側を3度傾ける。
- 発光輪郭、控えめな周辺光、弾の光の尾、命中Flash、消える結晶破片をPresentationだけに追加。既存Audioを維持する。
- 見た目の多角形と円形判定の差を示すため、的には論理半径に一致する細い円を残す。ColliderやGameplay乱数へ干渉しない。
- F2で旧表示と即時比較。切替でSimulation、弾倉、Gold、時間を変更しない。Rの編集Pauseでは軌跡、破片、Zone演出も停止。
- エフェクト破片は最大96、弾の尾は18点。演出上限はGame Logicに影響しない。
- Game Version 0.4.1-neon。同じ製品名・Save schemaで0.4.0と弾倉保存を共有。旧Build・Release・Tagを保持し、出力は新日時Directoryと別Version ZIPへ保存する。

### 32.2．縦長ステージ・三列UI（0.4.2-portrait）

- User承認により、WindowはFHD横長、論理座標は1600×900を保持し、中央StageをX575〜1025/Y50〜850（450×800、9:16）に変更。横長映像の引き伸ばしは行わない。円形Targetと弾の半径は維持する。
- Gun(800,815)。加速Zone中心(800+115sin(0.8t),650)、半径65と倍率/速度上限は維持。反射障害物2個はX685/915、Y560〜610のSeed付き配置。
- Targetの配置候補はX645〜955/Y115〜480。円全体を壁から20以上離し、下端540以内。他の物体・残弾との間隔、補充契機は§32を継承。旧Landscape座標も独立Layoutとして残す。
- 左に実効DPS、Gold、累計実ダメージ、命中/撃破/加速回数、通常/貫通の基礎・上限・直近命中威力、直近7件のEventログ。
- DPSは直近5 Simulation秒の実HP減少合計÷5。Overkillを除外。起動直後も分母5、Pause中は計測窓も停止。直近命中威力はOverkillを含む計算威力として区別する。
- 右に三Slot弾倉とRの編集/再開、Reload状態、将来の強化領域。購入強化は未実装と明記し、効果や価格を先回りして実装しない。
- 盤面にはHPを残し、抵抗や多数の命中数値は左へ移動。短い報酬/加速Popupは最大3件表示。左右のUIクリックを発射判定へ通さない。
- Game Version 0.4.2-portrait。音・発射間隔・速度・威力・抵抗・報酬・弾倉Save schemaは継続。旧Build/Tagは保存。盤面形状変更により反射頻度やDPS自体は旧横長と変わり得る。

## 33．Challenge Arsenal（最新試作 0.5.2-clear）

Userは有限マガジンのChallenge，容量内の全弾効果，恒久開放，2銃，Stage進行の試作を承認した。この章だけを最新実装の正本とし，旧章の一盤面／Stage Resetなし，個別弾Slot，残弾中再射撃，禁止機能と競合する箇所を本試作に限って上書きする。旧Prototypeへ遡及適用しない。以下はすべて試作用INITIALであり，完成版Balanceではない。

### 33.1．空間と表示

- FHD横長Window，1600×900論理座標，中央X500〜1100/Y50〜850の600×800（3:4）。左戦闘ログ／Challenge，右銃／効果／強化。Gun(800,815)，加速Zone(800+170sin(0.8t),660)。Zone半径65と最大速度1800を継続。
- 的12個。半径は通常25.5，装甲31.5（旧版75%）。候補X550〜1050/Y210〜520，物体との間隔22，壁との余白20，下端580まで。最大2000候補を試し，全数配置できなければ不完全なChallengeを開始せずError。
- バンパーはX645/955，Y120〜145の奥寄り。半径26，反射速度×0.9，壁反射も継続。
- 明るい青系の床，淡い結晶面，床影，22度傾いた深い結晶Mesh。Cameraと論理平面は真上を保持し，照準と円形判定を一致させる。真の斜めCamera・屈折・本格Bloomの追加ではない。
- Hitは0.25秒の発光とMesh傾き。破壊は24個，Hit5個等の立体破片，寿命0.8秒，演出上限192個。演出用独立乱数を使い，論理・Damage・Save・射撃可否へ干渉しない。F2比較と既存音を保持。

### 33.2．マガジンと銃

- 一クリック＝一マガジン。照準固定，Seed付き±2度散布。Revolverは6発／0.12秒間隔／初速780／基礎威力48／半径9。UZIは18発／0.05秒間隔／初速1050／基礎威力17／半径5。UZIの18は初期比較値であり，User例の30発をFIXEDとしない。
- 速度減衰120/s，停止80，威力は各銃の基礎値×min(衝突直前速度/900,2)×効果・恒久火力補正。抵抗は通常150／装甲420，Hit後に速度から引く。弾種による完全無効なし。
- 最後の弾を射出した時点からReload0.8秒を飛行と並行。0.5.0は全子弾を含む攻撃解決を待つ。現在の再射撃条件は§33.5へ更新。演出だけの破片や尾を待たない。PauseはすべてのSimulation時間を止める。
- Space／回収Buttonで生存弾と未射出弾をキャンセル。既に使用したマガジンは戻らず，追加報酬はない。射出途中の回収時はその時点から0.8秒Reload，既にReload中ならその残りだけ待つ。
- 最初の射出からSimulation時間10秒で斉射全体が終了。子弾へ同じ絶対終了時刻を継承し，分裂ごとに寿命をResetしない。

### 33.3．共通効果と連鎖

- 容量20pt，同じ効果は1枠，所有した効果は無料で着脱。飛行・射出中は構成変更と購入をUIから禁止。斉射は開始時の銃・効果・火力Snapshotを使用する。
- 初期開放・装備：初速+20%（4pt），抵抗減速1/4（4pt）。Gold購入：威力+25%（4pt，10G），命中二分裂（6pt，20G）。新規購入時は所有のみ，自動装備しない。
- 分裂は親弾が衝突後も生存する時，親を終了して±18度の子弾2個を次Tickから生成。各子威力は親の55%，半径85%（最低2），速度・加速済み・Golden状態・共通効果・10秒の終了時刻を継承。接触中Targetの再Hitを防ぐExiting集合を継承し，異なるTargetの命中履歴は新規。子も次の有効Hitで分裂でき，世代上限を設けない。
- 安全用上限は同時256弾（未射出予定分も予約）・一斉射の親子累計1024生成。到達時は分裂を抑制するだけで親弾を消さず，通常Damageと飛行を継続。上限と抑制回数をUIに表示。これは演出Particle上限とは別の論理ルール。
- Stage1の1マガジンClear：Golden（2pt）。銃の直接射出4発ごと，斉射ごとに番号Reset。命中TargetへGolden印，破壊報酬を一回だけ2倍。子へ継承，印はChallenge開始時にReset。
- Stage2の1マガジンClear：3体目爆発（6pt）。一つの弾が異なるTarget3個へHitすると一回，中心から100＋対象半径内の他の的へ当該Hit威力75%の即時Damage。爆発は分裂や別爆発を直接誘発せず，命中履歴も増やさない。子は新しい履歴と発動権を持つ。
- Stage3の1マガジンClear：共鳴（4pt），威力×1.2。特殊効果は通常のStage開放の必須条件にしない。

### 33.4．Challengeと恒久成長

- 初期3マガジン。残弾・射出予定が全てなくなった時，全的破壊ならClear，生存敵ありかつ携行数を消費済みなら失敗。先に撃ち切っただけで最後の弾を無効化しない。
- 生存敵のHPと位置はChallenge中に保持。倒した的を途中補充しない。次Challengeで全的HPと配置を更新。使用可能数は開始時Snapshotで，購入効果は次Challengeから。
- Stage1：通常10／装甲2，HP30／60，報酬3／7。Stage2：通常6／装甲6，HP50／85，報酬5／9。Stage3：通常8／装甲4，HP70／110，報酬7／11。全Stage抵抗は150／420。Stage Clearで次Stageを恒久開放，Stage3で今回の範囲終了，再訪可。
- 撃破Goldを即時獲得し，失敗・途中の再挑戦でも保持。携行上限3→4は30G，4→5は60G。恒久火力は+10%刻み最大+50%，価格15／30／45／60／75G。UZI開放40G。無料Revolverと初期効果を維持する。
- 1マガジンClearはStageごとのMasteryとして一度だけ恒久記録。再達成の重複報酬なし。特殊効果を装備しなくても次Stageへ進める。
- 構成Preset3個。銃と装備効果を保存・復元。Gold・所有効果・Mastery・到達Stage・恒久強化・現在構成・Presetを新規challenge-v1.jsonに保存。旧magazine.jsonを読み替えたり上書きしない。前Saveを日時履歴へ保持し，置換はAtomic。未知Schema／破損時は元Fileを保護し，一時Playと未保存状態を明示する。
- 飛行状態・途中ChallengeはSaveしない。再起動はStage1の新Challenge，獲得済みGoldと到達権は保持。配置再挑戦はSession内Seed付き乱数を進める。起動Seedは表示する。
- 今回入れない：Ascension，ランダムPerk抽選，大規模Skill Tree，3銃目，外部Asset購入，オンライン機能。旧Prototype・Build・Tagを保持。

### 33.5．低速残弾で次のマガジンを許可（0.5.1-tempo）

- Userの試遊指示で，全弾消滅待ちを撤回。Reload完了・射出予定なし・生存する全弾（分裂子を含む）の最大速度300以下で次のクリックを受ける。300は今回のINITIAL。平均速度ではない。生存弾なしは最大速度0扱い。Pause中や携行数消費済み，Challenge終了後は撃てない。拒否したクリックを予約しない。
- 再射撃しても低速残弾を消さず，命中・Gold・分裂・加速を継続。加速で300を再度超えた場合，その時点の次回発射判定は再び待機になる。既に受理した射出は中断しない。
- 斉射ごとに開始時点から10秒の絶対寿命と1024生成予算を独立管理。後続マガジンで前弾の期限や生成数をResetしない。未射出予定分も生成予算へ予約する。同時256は全マガジン合算で，開始時に銃の全弾分を予約できなければ発射不可。
- 生存敵が0の場合は追加マガジンを消費させない。Clear／失敗判定と構成変更・次Challenge開始は従来通り全攻撃の解決後。即座に確定したければSpaceで残弾回収できる。回収は全マガジンの生存弾・未射出弾が対象。
- HUDにREADY／減速待ち・最大速度・Reload・携行切れ・弾数上限を区別し，再射撃の閾値を表示。Save schema，銃・威力・経済・停止速度80は変えない。旧Build・Tag・記録は保持する。

### 33.6．クリア後の中央案内（0.5.2-clear）

- Clear確定時に中央盤面を暗くし，CLEAR，Stage名，使用マガジン数，今回の獲得Gold，「次へ」「もう一度」を表示する。左右の強化とStage選択は引き続き使用可能。
- 次へは現在Stageの次を開始。Stage3では次へを無効にし「全3ステージ達成」を明示する。もう一度は現在Stageを再挑戦。どちらも既存のChallenge開始処理を使い，全的HP・配置と使用マガジン数を更新する。Gold・所有強化・Masteryは保持する。
- 遷移クリックを新盤面の射撃へ流さない。Clear以外ではこの案内を表示しない。Pause中は隠し，Rで再開すれば再表示。Clearの論理確定条件・報酬・敵HP・Save schema・速度300の再射撃は変更しない。
