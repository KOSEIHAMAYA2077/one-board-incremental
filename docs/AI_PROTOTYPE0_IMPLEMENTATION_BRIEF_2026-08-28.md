# AI向けPrototype 0実装Brief

## 0．参照規則

実装時に参照してよい仕様は，次の二ファイルだけである．

1．`MASTER_GAME_SPECIFICATION_2026-08-28.md`  
2．本書

競合時はMasterを優先する．その他のMarkdown，`sources/`，Chat Log，旧調査資料は読まず，機能を取り込まない．不明点は推測で拡張せず，Masterの`INITIAL`値で最小実装する．

## 1．目的

Prototype 0は，次の二点だけを検証するWindows x64版である．

使用Unityは6000.3.18f1，Game Versionは`0.1.0-prototype0`へ固定する．別PatchでAssetを保存しない．

1．照準，発射，命中，Reloadの一発循環が気持ちよいか．  
2．Collectorへ約10回命中し，10 Goldで最初の`Collector Value＋1`を買う循環が理解できるか．

完成版の縦Sliceではない．将来機能の空Class，仮UI，汎用Frameworkを先回りして作らない．

## 2．実装範囲

### 2.1．必須

- 1600×900の論理座標と16:9 Camera．
- 画面下中央の一丁のGun．
- Mouse位置によるAim Direction．
- Cursor距離による2度から14度の散布Cone．
- Seed付き一様角度偏差．`spread`を片側最大角として`[-spread,+spread]`から選ぶ．
- Left Click一回につき一発．
- Reload Duration 0.65秒．Reload中のClickは無視する．
- Pure C#のProjectile Stateと60 tick/s Simulation．
- CircleCast Adapterによる高速衝突検索．
- Collector一個．Base Gold 1．
- Collector有効命中時の短い破壊Feedbackと0.35秒の表示復帰．
- Collector破壊数`0/10`表示．
- GoldとLifetime Gold．
- `Collector Value＋1`一Levelだけ．価格10 Gold．購入後Base Gold 2．
- 壁一枚と反射．
- 発射，命中，Reload完了の仮Audio．
- Version，Commit Hash，Session Seedの表示．
- Local Playtest Logと，保存PathをClipboardへCopyするButton．
- Windows x64 Build．

### 2.2．実装しない

- MaterialとMaterial Node．
- Mirror Target．Prototype 0の壁反射だけを使う．
- Amplifier，Stone，複数Collector．
- Socket編集．
- Recipe，Primer，Capacity．
- 貫通弾，分裂弾．
- Auto-fire．
- Core Damage，Shield，Ending．
- Save Migration．
- Steamworks，Achievement，Cloud Save．
- URP，Addressables，DOTS，ECS，Job System．
- Online Telemetry．

## 3．初期値

| Parameter | 値 |
|---|---:|
| Logical Resolution | 1600×900 |
| Gun Position | 800，820 |
| Reload Duration | 0.65秒 |
| Projectile Speed | 900 logical px/s |
| Projectile Radius | 8 logical px |
| Min Aim Distance | 100 |
| Max Aim Distance | 600 |
| Min Spread | 2度 |
| Max Spread | 14度 |
| Simulation Rate | 60 tick/s |
| Collector Base Gold | 1 |
| First Upgrade Cost | 10 Gold |
| Collector復帰表示 | 0.35秒 |

## 4．状態遷移

```text
Boot
↓
Playing
├ PausedByPlayer
└ ViewingFirstUpgrade
```

`ViewingFirstUpgrade`中はSimulationを停止する．飛翔中Projectileは保持し，再開後に同じSnapshot倍率で進行する．Upgrade購入UIを閉じたClickを発射へ伝播させない．

発射Stateは次のとおりである．

```text
Ready
  successful fire
Reloading
  reloadRemaining <= 0
Ready
```

Reload中のClickはQueueしない．同一Frameに複数Clickがあっても一発だけ生成する．

## 5．最小Data

```text
GameRuntimeState
  Gold: double
  LifetimeGold: double
  CollectorValueLevel: int
  HitCount: int
  ChamberReady: bool
  ReloadRemaining: double
  Tick: long
  SessionSeed: uint

ProjectileState
  ProjectileId: long
  Position: SimVector2
  Velocity: SimVector2
  Radius: double
  RemainingReflections: int
  DistinctReflectionSurfaceCount: int
  Alive: bool
  SpawnSequence: long
  GunMultiplierSnapshot: double
```

Unityの`Vector2`をPure C# Domainへ持ち込まず，`SimVector2`または同等の独自値型を使う．Runtime StateをScriptableObjectへ保存しない．

## 6．責務境界

推奨Folderは次とする．名前の微調整は許可するが，依存方向を逆転させない．

```text
Assets/
  Game/
    Domain/
      Simulation/
      Economy/
      Math/
    Application/
      GameSession.cs
    Infrastructure/
      UnityCollisionQuery.cs
      LocalPlaytestLog.cs
    Presentation/
      Input/
      Views/
      UI/
      Audio/
    Tests/
      EditMode/
      PlayMode/
```

`Domain`は`UnityEngine`へ依存しない．PresentationはSnapshotとView Eventを読み，Game Stateを直接書き換えない．Collider Callbackを論理正本にしない．

## 7．Gold処理

Collector命中時，次を同じ論理Tick内で行う．

```text
baseGold = 1 + CollectorValueLevel
reflectionMultiplier = 1.0 if no reflection else 1.5
reward = floor(baseGold × reflectionMultiplier)
Gold += reward
LifetimeGold += reward
HitCount += 1
```

Prototype 0には壁一枚しかないため，Reflection Multiplierは最大1.5である．同じ壁へ複数回当たっても倍率を重ねない．

Prototype 0の`Collector Value＋1`は一回だけ購入可能である．

```text
if Gold >= 10 and level == 0:
    Gold -= 10
    level = 1
```

Lifetime Goldは購入で減らさない．購入後の次命中から2 Goldを与える．すでに飛んでいるProjectileについても，Target所有値であるCollector Base Goldは命中時のLevelを使う．

## 8．必須Test

### 8.1．Edit Mode

- Cursor距離100以下でSpread 14度になる．
- Cursor距離600以上でSpread 2度になる．
- 同じSeedとShot列で同じ偏差列になる．
- 生成偏差が常に`[-spread,+spread]`内にあり，Cone表示の両端と一致する．
- Reload中は発射に失敗し，Projectile数が増えない．
- 10回のLevel 0命中でGold 10，Lifetime Gold 10になる．
- 10 Gold購入後，Gold 0，Lifetime Gold 10，Level 1になる．
- 次命中でGold 2，Lifetime Gold 12になる．
- Level 1のProjectileが壁反射後に命中すると`floor(2 × 1.5) = 3 Gold`になる．
- Gold不足または購入済みの場合，二重購入できない．
- Collector表示復帰がGoldを追加しない．

### 8.2．Play Mode

- UI Clickで発射しない．
- 高速ProjectileがCollectorをすり抜けない．
- 壁法線で正しい方向へ反射する．
- Corner接触で無限再衝突しない．
- Window Resizeで論理Target位置が変わらない．
- Upgrade画面中はSimulationが停止する．

## 9．受入条件

- Unity EditorでErrorなしにPlayできる．
- Clean CheckoutからWindows x64 Buildを作れる．
- 起動後5秒以内に発射できる．
- 10回の命中で10 Goldになり，最初のUpgradeを購入できる．
- 購入後，Collector表示が1から2へ変わり，次命中で`＋2`を表示する．
- Reload中のClickで予約発射しない．
- Version，Commit，Seedを画面とLogで確認できる．
- Testがすべて成功する．
- Consoleへ継続的なWarningまたはErrorを出さない．

## 10．実装順

1．Unity Project設定とGit設定．  
2．論理座標とCamera．  
3．Pure C#のAim，Spread，Seed RNG．  
4．InputとCone View．  
5．Projectile SimulationとCircleCast Adapter．  
6．Collector衝突とGold．  
7．Reload State．  
8．`0/10`と最初のUpgrade．  
9．壁反射．  
10．AudioとHit Feedback．  
11．TestとLocal Log．  
12．Windows BuildとGitHub Pre-release．

各段階でPlay可能な状態を維持し，機能ごとに小さくCommitする．
