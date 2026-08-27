# Gold・Material初期経済調査とPrototype設定

作成日：2026-08-28  
対象：`MASTER_GAME_SPECIFICATION_2026-08-28.md`のGold，Material，Target購入，有限解禁  
状態：Prototype用仮確定案

---

## 1．結論

初期経済は，次の三段階で構成する．

1．最初のCollectorを約10回破壊し，10 Goldで`Collector Value＋1`を初購入する．  
2．Goldだけを使って基礎値，銃倍率，Reload，最初のMirrorを購入する．  
3．累積250 GoldでMaterial Nodeを無償支給し，以後の新Target，新弾種，自動化をGold＋Materialで有限解禁する．

Goldは何度でも購入する量的強化と，同種Targetの追加購入へ使う．Materialは新しい遊び方を開く有限解禁にだけ使う．Materialだけで解禁させず，Goldも同時に要求することで，Material Nodeだけを撃ち続ける進行を防ぐ．

最初のMirrorとMaterial NodeはMaterialを要求しない．これにより，Material入手源をMaterialで解禁する循環を排除する．

---

## 2．類似作から確認できた事実

### 2.1．Idle Gumball Machine

数値を網羅したWikiは確認できなかったため，Steamの説明と公開実プレイ映像を根拠にした．

- Steamは，本作を「ガムボールをGearへ当て，Upgradeを買い，数値を拡大する短編Incremental」と説明している．Auto-fireは後半Upgradeであり，ゲーム自体もIdleよりIncrementalとして説明されている．  
  https://store.steampowered.com/app/3724490/Idle_Gumball_Machine/
- 公開実プレイでは，開始約12秒で4 Gold，約32秒で9 Goldになり，最初の`Bigger Gears`購入後はGearの開始値が1から2へ上がっている．購入済みLevelは1/60，次価格は15 Goldである．所持Goldの推移から，初回価格は10 Goldと推定できる．  
  https://www.youtube.com/watch?v=TFKzi2DgjSk
- 同映像の約3分時点では，盤面に複数Gearが存在し，店には`＋1`，`＋10`，`×2`のように効果を即読できる項目が並ぶ．購入結果が盤面上のGear数と表示値へ直結する．
- 約4分11秒には，`Psychic Beam`が0/4，2.00k Goldで表示され，「Ballが5回BounceするたびRandom Gearへ命中する」という質的効果が提示される．量的強化の途中へ，挙動を変える高額有限Upgradeが差し込まれている．

本作へ借りるのは，次の部分である．

- 約10回の基本行為で最初の`＋1`を買わせる．
- Upgrade名，効果，盤面表示を一対一で対応させる．
- 量的な`＋1`と，挙動を変える高額有限Upgradeを交互に提示する．
- Target購入直後に盤面へ実物を出し，配置編集へ誘導する．

AscensionとResetは本作へ借りない．

### 2.2．Cookie Clicker

Cookie Clicker Wikiでは，最初のCursorは15 Cookiesで，0.1 Cookies/sを生産する．同種Buildingの価格は購入ごとに1.15倍となる．また，Cursorを一つ所有すると，Cursorと手動Clickを2倍にするUpgradeが100 Cookiesで解禁される．  
https://cookieclicker.wiki.gg/wiki/Cursor  
https://cookieclicker.wiki.gg/wiki/Buildings

この構造の重要点は，単一の価格式ではなく，次の三種類を併用していることである．

- 同種設備を増やす緩い指数価格．
- 所有数を条件に現れる固定価格の倍率Upgrade．
- 次の設備種へ貯めるための大きな価格段差．

本作では同種Target数が少なく，3時間でResetしないため，Cookie Clickerの1.15倍をそのまま使うと価格がほとんど上がらない．Goldの反復Upgradeには1.55から1.90倍，同種Target追加には約3倍の強い段差を使う．一方，「所有または累積値で次の有限Upgradeを表示する」という構造は採用する．

Cookie ClickerのHeavenly ChipsはReset後も残る恒久通貨である．本作のMaterialはReset通貨ではないため，役割を模倣しない．Materialは同一Playthrough内の有限解禁Tokenに限定する．  
https://cookieclicker.wiki.gg/wiki/Heavenly_Chips

### 2.3．Digseumと(the) Gnorp Apologue

Digseum Wikiの初期Upgradeには，20 GのMarketing，150 GのStamina，700 GのPickaxe Strength，1,000 GのPickaxe Areaがあり，小さい量的購入と，進行を変える大きな購入に明確な価格差がある．  
https://digseum.fandom.com/wiki/Upgrades

(the) Gnorp Apologueでは，Shardが建物と通常Upgradeの両方へ使われ，特別資源Zybelliumが強い有限強化を担当する．また，最初のExpress解禁は10 Shardsである．通常資源を循環の中心に置き，特別資源を質的強化へ限定する点を参考にする．  
https://gnorp.dev/  
https://gnorp.wiki.gg/wiki/The_Express

---

## 3．本作の通貨責務

| 通貨・値 | 入手 | 用途 | 禁止する用途 |
|---|---|---|---|
| Gold | Target破壊・有効命中 | 基礎値，銃倍率，Reload，同種Target追加，有限解禁のGold部分 | 発射消費，Recipe変更料金 |
| Material | Material Nodeの確定Gauge | 新Target Type，新弾種，Socket領域，自動化，Capacity | 反復する数値Upgrade |
| Lifetime Gold | 累積獲得Gold．消費しない | 店項目の表示，Material Node支給，Tutorial進行 | 購入費用 |

有限解禁は，原則として`Gold＋Material`を同時に要求する．Goldは現在の生産力，MaterialはMaterial経路を実際に扱ったことを証明する．

---

## 4．最初のCollector

### 4.1．破壊と再生

- New Game時にCollectorを一個所有し，中央の初期Socketへ配置する．
- CollectorはHP 1の破壊表現を持つ．有効命中で破壊され，1 Goldを出す．
- 論理上は同一Targetを維持し，0.35秒後に同一Socketで再生する．Target IDは変えない．
- Reload初期値0.65秒のため，再生待ちは通常の連続射撃を妨げない．
- 最初の10回は破壊回数を`0/10`で表示する．

これは既存の「盤面に残るTarget」と両立する．破壊はFeedback表現であり，購入済みTargetを消費しない．

### 4.2．最初の購入

| 項目 | 初期値 |
|---|---:|
| Collector Base Gold | 1 |
| 最初の`Collector Value＋1`価格 | 10 Gold |
| 購入後Base Gold | 2 |
| 次価格 | 16 Gold |
| 最大Level | 12 |

価格式は次とする．

```text
CollectorValueCost(level) = ceil(10 × 1.55 ^ level)
```

Level 0から7の価格は，10，16，25，38，58，90，139，215となる．一回目は収益を2倍にし，以後は`＋1`なので体感上昇率が自然に小さくなる．

---

## 5．Gold反復Upgrade

| Upgrade | 効果 | Base Cost | 価格倍率 | 上限 |
|---|---|---:|---:|---:|
| Collector Value | Collector Base Gold＋1 | 10 | 1.55 | 12 |
| Gun Calibration | 全GoldへGun Multiplier＋0.25 | 50 | 1.75 | 12 |
| Reload Mechanism | Reload Duration×0.925 | 75 | 1.90 | 8 |
| Material Residue | Material Node Base Gold＋1 | 120 | 1.80 | 6 |

共通式は次とする．

```text
Cost(level) = ceil(BaseCost × Growth ^ level)
```

Reloadは0.35秒を下限とする．一購入が約7.5％短縮に留まるため，購入単体の体感は発射音とReload Indicatorで補強する．

Gold Upgradeに返金は設けない．購入前に`現在値 → 購入後`と，直近60秒実績から計算した概算Gold/min変化を表示する．

---

## 6．Target購入規則

### 6.1．共通規則

1．Target Typeの有限解禁は，最初の一個を必ず含む．解禁後にもう一度購入させない．  
2．購入したTargetは次の空Socketへ仮配置し，直後にBoard Editingを開く．  
3．配置変更と撤去は無料である．撤去はInventoryへ戻すだけで，売却や返金ではない．  
4．同種追加個体はGoldだけで購入する．Materialを繰り返し要求しない．  
5．空Socketがない場合は購入できず，必要なSocket解禁を表示する．  
6．価格，所有数上限，購入後に盤面へ増える実物を常時表示する．

### 6.2．初期価格

| Target | 一個目 | 二個目 | 三個目 | 初期上限 |
|---|---:|---:|---:|---:|
| Collector | New Gameで所持 | 100 Gold | 350 Gold | 3 |
| Mirror | 125 Goldで解禁し一個付与 | 購入不可 | 購入不可 | 1 |
| Material Node | Lifetime Gold 250で一個無償付与 | 購入不可 | 購入不可 | 1 |
| Amplifier | 200 Gold＋2 Materialで解禁し一個付与 | 購入不可 | 購入不可 | 1 |

Collector追加価格は，所有数が少ないため汎用指数式へ隠さず，100，350の固定表を使用する．Prototypeでは，Collector追加が単なる命中面積の増加にしか感じられない可能性を検証する．弱い場合は，所有Collector一個ごとにCollector Base Goldへ`＋10％`ではなく，Pierce・Split解禁を前倒しして複数配置の経路価値を上げる．所有だけで無条件収益を増やす補正は最初から入れない．

Mirror命中後にCollectorへ到達した場合，初期Route Multiplierを1.5とする．これにより，Mirror購入は見た目だけでなく，狙いを難しくする代わりに収益を増やす最初の盤面投資になる．

---

## 7．Material導入

### 7.1．導入手順

- Lifetime Gold 200でMaterial Nodeの存在と次の支給条件を表示する．
- Lifetime Gold 250でMaterial Node一個と専用Socket一個を無償支給する．
- 支給直後にBoard Editingを開き，配置場所を選ばせる．
- Material Node自体はMaterialを要求しない．

### 7.2．取得規則

| Parameter | 初期値 |
|---|---:|
| 一Materialに必要なGauge | 10 |
| 一有効命中のGauge | 1 |
| Material Node Base Gold | 1 |
| 同一LineageからのGauge増加 | 一回まで |
| Gauge繰越 | あり |

Material Nodeを10回有効命中させると，乱数なしで1 Materialを得る．Material Nodeにも1 Goldを与えるが，同時期のCollectorよりGold効率を低くする．これにより，Goldを稼ぐ経路とMaterialを進める経路をプレイヤーが明示的に選ぶ．

Gauge閾値を下げた結果，現在Gaugeが新閾値以上なら，直ちにMaterialへ変換して余剰Gaugeを繰り越す．

---

## 8．初期Skill Tree

Skill Treeは見た目上枝分かれしてよいが，排他的選択にはしない．最終的にはすべて取得できる．

| 順 | Node | 前提 | 価格 | 付与 |
|---:|---|---|---:|---|
| 0 | Mirror Routing | Lifetime Gold 60 | 125 Gold | Mirror一個，Board Editing |
| 1 | Material Survey | Lifetime Gold 250 | 無償 | Material Node一個，専用Socket |
| 2 | Amplifier Circuit | Material Survey | 200 Gold＋2 Material | Amplifier一個 |
| 3 | Recipe Workbench | Amplifier Circuit | 350 Gold＋4 Material | 五発Recipe Editor，Capacity 4 |
| 4A | Pierce Loading | Recipe Workbench | 500 Gold＋5 Material | 貫通弾 |
| 4B | Split Loading | Recipe Workbench | 650 Gold＋7 Material | 分裂弾 |
| 5 | Capacity＋1 | 4Aまたは4B | 900 Gold＋6 Material | Capacity 5 |
| 6 | Fixed Auto-fire | Capacity＋1 | 1,500 Gold＋8 Material | 最終手動射撃方向へのAuto-fire |

4Aと4Bは購入順を選べるが，片方を買っても他方を失わない．最初のBuild選択を作りつつ，不可逆な失敗を作らない．

Skill Nodeは前提を満たすまで完全に隠さず，近いNodeをSilhouetteと条件で表示する．現在Goldを使い切ってもLifetime Goldによる表示進行は戻らない．

---

## 9．Prototypeで狙う時間

以下は，初見の有効命中間隔を平均1.0秒から1.5秒とした目標範囲であり，保証時間ではない．

| 経過目標 | 出来事 |
|---:|---|
| 0から20秒 | Collectorを約10回破壊し，最初の10 Goldを得る |
| 15から30秒 | `Collector Value＋1`を購入する |
| 30から90秒 | 次のValue，Gun，Reloadから選ぶ |
| 1分30秒から3分 | MirrorまたはCollector二個目を購入する |
| 3分から6分 | Lifetime Gold 250へ到達し，Material Nodeを得る |
| 5分から8分 | 最初のMaterialを得る |
| 7分から12分 | AmplifierをGold＋Materialで解禁する |
| 12分から20分 | Recipe Workbenchを解禁する |
| 18分から30分 | PierceまたはSplitを選んで解禁する |

Prototypeの収益がこの範囲から外れた場合，価格だけでなく，命中率，飛翔時間，Reload待ち，UI滞在時間を分けて調整する．

---

## 10．Prototype計測項目

- 最初の10回破壊までの実時間．
- 最初のUpgrade購入までの実時間．
- 最初に選ばれたGold Upgrade．
- MirrorとCollector二個目のどちらを先に買ったか．
- Material Node支給から最初のMaterialまでの時間．
- Gold不足とMaterial不足のどちらがSkill解禁を止めたか．
- Materialを得るためにCollector経路から切り替えた回数．
- 購入前後60秒のGold/min．
- 次の意味ある購入まで何も選べなかった最長時間．
- Target購入後30秒以内に配置を変更したか．

合格基準は次とする．

- 5人中4人以上が説明なしで最初のUpgradeを30秒以内に購入する．
- 5人中4人以上がMaterialの入手方法を支給後60秒以内に理解する．
- Mirror購入者の過半数が，直接Collectorを撃つ場合と反射経路の報酬差を説明できる．
- 最初の10分に，90秒を超える無選択待ちがない．
- Goldだけ，またはMaterialだけを集め続けることが最適にならない．

---

## 11．Master仕様書へ反映する決定候補

次を`INITIAL`としてMasterへ反映する．

- 最初の10回破壊と，10 Goldの`Collector Value＋1`．
- Gold反復Upgradeの個別Base CostとGrowth．
- Mirrorは125 Goldで最初に解禁し，一個を付与する．
- Material NodeはLifetime Gold 250で無償支給する．
- 有限解禁は原則Gold＋Materialとし，解禁時に最初の一個を付与する．
- 同種Targetの追加購入はGoldだけを使う．
- 購入Targetは仮配置後，Board Editingを開く．
- Lifetime Goldを表示解禁専用の非消費値として保存する．
- Skill Treeは非排他的であり，最終的に全Nodeを取得できる．

数値はPrototypeのTelemetryで変更してよい．通貨責務，循環の開始方法，Target Type解禁時に一個を付与する規則は，数値調整と分離して維持する．
