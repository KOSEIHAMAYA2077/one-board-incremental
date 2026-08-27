# インクリメンタル作品再調査・経済曲線監査

作成日：2026年8月28日  
対象：一盤面式能動型インクリメンタルゲーム  
目的：Gold，Material，量的強化，質的解放，盤面購入，3時間進行を，Unity実装前に矛盾のない初期値へする  
位置付け：判断根拠．実装時の正本は`MASTER_GAME_SPECIFICATION_2026-08-28.md`と対象Prototype Briefである

---

## 0．結論

最初の循環は変更しない．Collectorを10回倒し，10 Goldで`Collector Value＋1`を買い，報酬を1から2へ上げる．これはIdle Gumball Machineの観察結果と，Cookie Clickerの最初の15 Click／15 Cookieの購入規模の双方に近く，Prototype 0で検証できる十分に強い開始である．

ただし，旧0.2.0のその後の価格とMaterial速度は採用しない．旧値を理想条件で計算すると，Collector ValueをLevel 12まで約4分で買え，Materialは約6.5秒に一個得られる．設計上の`3から6分でMaterial Node`，`12から20分でRecipe`という進行表と両立していない．

0.3.0では次を採用する．

1．Gold成長を，反復する`＋1`，少数回の`×2`節目，Reload短縮，盤面上の質的解放へ分ける．  
2．Material Nodeは常時連打対象にせず，4秒ごとに発光するReady状態への命中だけでGaugeを得る．発光待ちの間はGold Targetを撃てるため，待機ではなく照準切替になる．  
3．Materialは一個40秒弱の上限速度とし，質的解放の最短間隔をMaterial側から保証する．確率Dropは使わない．  
4．小数倍率を最後に毎回floorする旧Gun Upgradeをやめ，Gunの節目は明確な全Gold`×2`とする．購入しても表示報酬が変わらない状態を作らない．  
5．同種Target追加は，購入直後に空Socketへ仮配置し，配置編集を開く．価格式で無制限に増やさず，少数の固定価格表を使う．  
6．完成尺は価格だけで3時間へ伸ばさない．まず0から60分の機能密度をP4で成立させ，後半は同じ盤面と五発Recipeの統合に使う．

---

## 1．調査方法

情報は次の三層へ分けた．

- `FACT`：公式Store，公式Wiki，公開Source，数値Wiki，実プレイ映像から確認できる事実．
- `REVIEW`：Steam利用者の評価や不満．母集団全体の結論とは扱わない．
- `DESIGN`：本作へ変換した判断．他作品の数値をそのまま複製したものではない．

価格やReview件数は2026年8月28日時点で変動し得る．攻略Wikiの値はVersion差を含み得るため，採用値の根拠ではなく，成長構造の比較へ使った．

---

## 2．作品別に確認した構造

### 2.1．Idle Gumball Machine

`FACT`

- Steamは，GumballをGearへ当てて収益を得て，Upgradeで盤面と数値を成長させる，短く能動的なPlinko型Incrementalとして説明している．後半にAuto-fireがある．  
  https://store.steampowered.com/app/3724490/Idle_Gumball_Machine/
- 公開実プレイ映像では，開始約12秒で4 Gold，約32秒で9 Goldになり，最初の`Bigger Gears`購入後は表示値が1から2へ変化する．所持Gold推移から初回10 Gold，次回15 Goldと推定できる．  
  https://www.youtube.com/watch?v=TFKzi2DgjSk
- 同映像では，数分以内にGear数や数値が盤面上で増え，約4分時点には単なる数値増加ではない高額な挙動Upgradeが提示される．

`DESIGN`

- 最初の約10回で`＋1`を買わせる．
- 購入結果を盤面の実物，表示値，音の三つへ同時に反映する．
- `＋1`だけを並べず，反射，Target追加，倍率経路のような挙動変化を途中へ挟む．
- Target購入時に最初の一個を含め，直後に配置編集へ入る規則を採用する．
- AscensionとResetは借りない．

### 2.2．Cookie Clicker

`FACT`

- 最初のCursorは15 Cookies，基礎生産は0.1 Cookies/sである．同種Building価格は基本的に所有数ごと1.15倍になる．  
  https://cookieclicker.wiki.gg/wiki/Cursor  
  https://cookieclicker.fandom.com/wiki/Building
- Cursor一個の所有を条件に，Cursorと手動Clickを強化する100 CookiesのUpgradeが現れる．後続も所有数や節目で倍率Upgradeが解禁される．  
  https://cookieclicker.fandom.com/wiki/Upgrades?page=2

`DESIGN`

- 1.15という緩い倍率は，多数のBuildingを長時間買う構造だから成立する．Target所有数が最大数個でResetなしの本作へそのまま使わない．
- 借りるべきものは，`反復購入＋所有／累積条件で現れる大きな倍率節目＋次の種類`という三層構造である．
- 手動操作は後半に消さず，Upgradeと経路選択によって一発の価値を上げる．

### 2.3．AdVenture Capitalist

`FACT`

- Business価格は種類によりおおむね1.07から1.15の係数で増える．
- 所有数の節目で速度二倍や利益倍率が発生し，Managerが手動起動を自動化する．  
  https://adventure-capitalist.fandom.com/wiki/Businesses  
  https://adventure-capitalist.fandom.com/wiki/Unlocks_%28Earth%29

`DESIGN`

- 幾何価格だけでは成長感にならない．一定数購入時の明確なPower Spikeと，操作委譲の節目が必要である．
- 本作は大量所有ではなく，少数の`全Gold×2`とAuto-fireで同じ役割を果たす．

### 2.4．Clicker Heroes

`FACT`

- Hero Costは基礎的にLevelごと1.07倍だが，後半には25 Level単位などの大きな倍率節目が重なる．  
  https://clickerheroes.fandom.com/wiki/Formulas

`DESIGN`

- 緩い価格上昇を長く続ける場合，別の倍率段差が必要である．本作は三時間短編なので，細かいLevelを増やすより，`＋1`と少数の`×2`を分ける．

### 2.5．Antimatter Dimensions

`FACT`

- Dimensionは10個購入ごとに生産倍率が二倍になり，より上位のDimensionが下位Dimensionを生産する．価格増加と購入節目が別々に存在する．  
  https://antimatterdimensions.wiki.gg/wiki/Antimatter_Dimension

`DESIGN`

- 数値が上がるだけでなく，どの層がどの層を生むかという構造変化が長期の成長を支える．本作では新通貨階層を増やさず，弾道が複数Targetを通り，Lineage YieldがCore Damageになる構造変化で代替する．

### 2.6．Kittens Game

`FACT`

- 建物のPrice Ratioには1.15前後のものがあり，次価格は現在価格へRatioを掛けて求める．Resourceには上限，特殊Resource，Craft Resourceがある．  
  https://wiki.kittensgame.com/en/general-information/game-mechanics/price-ratio  
  https://wiki.kittensgame.com/en/general-information/resources

`DESIGN`

- Resource上限や多段Craftは，長期計画を生む代わりに三時間作品には重い．本作へは導入しない．
- 新Resourceは，入手源，Gauge，直近の用途を同時に見せるProgressive Disclosureだけを借りる．

### 2.7．A Dark Room

`FACT`

- 最初は小さな操作だけを見せ，進行に応じて資源，建物，探索を段階的に公開する．公開Sourceが存在する．  
  https://github.com/doublespeakgames/adarkroom

`DESIGN`

- 開始時からGold，Material，Recipe，Core条件を同じ密度で見せない．
- Material Nodeを得るまではMaterial欄をSilhouetteにし，最初のMaterial取得後にMaterial価格を持つAmplifierを明示する．

### 2.8．Universal PaperclipsとSPACEPLAN

`FACT`

- Universal Paperclipsは，単純なPaperclip作成から投資，計算資源，世界規模の生産へ段階的に操作と意味を変える．  
  https://www.decisionproblem.com/paperclips/
- SPACEPLANは物語と明確なEndingを持つ短編Clickerとして販売されている．  
  https://store.steampowered.com/app/616110/SPACEPLAN/

`DESIGN`

- 長尺を数字の桁だけで作らず，画面と目的の意味を段階的に変える．
- 本作は盤面をResetせず，Collector稼ぎ，経路構築，五発Recipe，Core攻略へ同じ一盤面の意味を変える．
- 終了条件とEndingを最初から設計対象に含める．

### 2.9．Digseum

`FACT`

- 公式説明では約2.5時間，12以上の発掘地，50以上のRelic，Skill Treeを持つ短編Incrementalである．  
  https://store.steampowered.com/app/3361470/Digseum/
- Wikiの初期価格には，20 GのMarketing，150 GのStamina，700 GのPickaxe Strength，1,000 GのPickaxe Areaがあり，小さな数値購入と大きな機能購入に段差がある．  
  https://digseum.fandom.com/wiki/Upgrades

`DESIGN`

- 安価な反復Upgradeと，高価な質的解放を同じ価格倍率へ押し込まない．
- 3時間作品は明確な地点数とEndingを持ち，終盤の水増しを避ける．

### 2.10．(the) Gnorp Apologue

`FACT`

- Shardの生成と回収が別工程であり，建物と通常UpgradeにはShard，強い有限Upgradeには希少なZybelliumが使われる．ProducerとCollectorのどちらが律速かを組み替える戦略がある．  
  https://store.steampowered.com/app/1473350/the_Gnorp_Apologue/  
  https://gnorp.wiki.gg/wiki/Zybellium

`DESIGN`

- 第二資源は反復購入へばら撒かず，挙動を変える有限解放へ集中させる．
- Gold TargetとMaterial Nodeを別の照準先にすることで，現在の律速をPlayerが操作で切り替えられるようにする．

### 2.11．Magic Archery，Tower Wizard，Xenosensory

`FACT`

- Magic Archeryは短く能動的で，明確なEndingを持つと公式に説明される．  
  https://store.steampowered.com/app/2905170/Magic_Archery/
- Tower Wizardは能動型，複数の固有Mechanic，Prestige，明確なEndingを掲げる．  
  https://store.steampowered.com/app/3372980/Tower_Wizard/
- Xenosensoryは短いRound，Meta Tree，Buttonへ割り当てるAction，装備枠の限られたGadget，複数Sectorを組み合わせる．  
  https://store.steampowered.com/app/4037830/Xenosensory/

`DESIGN`

- 一つの操作を延々と高速化するより，Actionや配置，有限Slotの使い方を追加する方が短編の密度を上げる．
- Prestigeがある作品の表面的なReset周期は借りず，解放時の演出と新しい判断だけを借りる．

### 2.12．Nodebuster，Pincremental，Minutescape

`FACT／REVIEW`

- Nodebusterは短編として非常に高い評価を得ているが，一部の否定的Reviewには，新Mechanicが尽きた後に最後の解放を待つ区間への不満がある．  
  https://store.steampowered.com/app/3107330/Nodebuster/  
  https://steamcommunity.com/app/3107330/negativereviews/
- PincrementalはPinball盤面，複数段Prestige，自動化を持つ．否定的Reviewには，Reset後に自動化を再取得し，盤面自体の変化が乏しいという不満がある．  
  https://store.steampowered.com/app/1369470/Pincremental/  
  https://steamcommunity.com/app/1369470/negativereviews/?browsefilter=toprated
- Minutescapeは五分Run，能力とRule-changing Upgradeを公式の中心に置く．  
  https://store.steampowered.com/app/3327170/Minutescape/

`DESIGN`

- 最後の30分に新しい文法を入れないことと，最後の30分をただの待ち時間にすることは別である．最終区間は，既存要素の組合せを変えてCore Damageを伸ばす実戦にする．
- 盤面成長と自動化をResetで取り上げない．

### 2.13．Loot LoopとMore Sushi!

`FACT`

- Loot Loopは数秒単位のRun，毎回のUpgrade，常時見える成長，深いSkill Treeを公式の売りとしている．  
  https://store.steampowered.com/app/3972320/Loot_Loop/
- More Sushi!は短編，Helper，Sushi解放，Prestigeによる恒久強化を持つ．  
  https://store.steampowered.com/app/3950770/More_Sushi

`DESIGN`

- 本作の一発を数秒Runに相当する小さな結果単位とし，一Lineageの合計結果を読めるようにする．
- `常に何かが変わる`を購入頻度だけで実現せず，Targetの反応，弾道，Core外観を含める．

---

## 3．横断して借りるもの，借りないもの

| 観察した設計 | 本作への変換 | 採否 |
|---|---|---|
| 約10から15回の基本操作で最初の購入 | 10 Collector Hit，10 Gold，報酬1→2 | 採用 |
| 緩い同種価格倍率 | 所有数が少ないため固定価格表へ変換 | そのままは不採用 |
| 所有数節目の×2 | 少数回の全Gold×2 | 採用 |
| Manager／Auto | 固定方向Auto-fire，手動割込 | 採用 |
| 特殊Resource | Materialを有限機能解放だけへ使用 | 採用 |
| Resource Capと多段Craft | 三時間には過密 | 不採用 |
| Prestige | 一盤面の履歴を消す | 不採用 |
| Progressive Disclosure | 次の一層だけSilhouette表示 | 採用 |
| 明確なEnding | 可視Coreの段階解除と破壊 | 採用 |
| 終盤の待ち | Core HPを下げ，価格で延命しない | 禁止 |

---

## 4．0.2.0経済の定量監査

### 4.1．Collector Valueが速すぎる

旧値は次である．

```text
Reward = 1 + Level
Cycle = 0.65秒
Cost = ceil(10 × 1.55 ^ Level)
```

毎発を直接命中させ，買える瞬間にCollector Valueだけを買う理想条件では次になる．飛翔時間0.56秒を初回だけ加えた．

| 購入後Level | 購入時刻 | Lifetime Gold |
|---:|---:|---:|
| 1 | 6.41秒 | 10 |
| 2 | 11.61秒 | 26 |
| 3 | 17.46秒 | 53 |
| 4 | 23.31秒 | 89 |
| 5 | 31.11秒 | 149 |
| 6 | 40.86秒 | 239 |
| 7 | 53.86秒 | 379 |
| 8 | 71.41秒 | 595 |
| 9 | 95.46秒 | 928 |
| 10 | 129.26秒 | 1,448 |
| 11 | 176.71秒 | 2,251 |
| 12 | 243.66秒 | 3,487 |

これは`3から6分でMaterial Node`以前に量的系列の上限へ近づく．初心者のMissやUI時間を加えても，進行表との差が大きい．

### 4.2．Materialが速すぎる

旧値は，Cooldownなし，Gauge 10，Cycle 0.65秒である．理論上は一個約6.5秒，約9.23 Material／分になる．旧有限解放を順番に買う場合，必要Materialの累積84個へ約9.1分で到達できる．Goldが十分なら，Recipe，二弾種，Capacity，Autoが短時間へ圧縮される．

### 4.3．Gun Calibrationに無効購入がある

旧式は次である．

```text
RewardGold = floor(BaseGold × (1 + 0.25 × Level) × otherMultipliers)
```

Base Gold 1，直接命中，Level 0から1では，`floor(1×1.25)=1`で表示報酬が変わらない．Base Gold 2でも`floor(2×1.25)=2`で変わらない．50 Goldを払っても直後の主要行為が強化されないため，`一購入で15％以上変化`という旧基準に違反する．

### 4.4．追加Collectorの価値が曖昧である

通常弾はCollectorに吸収されるため，二個目を置いても一発の最大報酬は増えない．命中面積と経路候補は増えるが，UIが生産設備として見せると誤購入になる．追加Collectorは`命中安定／別経路`として効果を明記し，P1で配置変更が実収益差を作らない場合は，Pierce解放後まで延期する．

---

## 5．0.3.0初期経済

### 5.1．成長の四層

| 層 | Playerが買うもの | 体験上の変化 | 役割 |
|---|---|---|---|
| A．加算 | Collector Value＋1 | Target上の値が増える | 短い購入循環 |
| B．倍率節目 | 全Gold×2，Reload×0.85 | 画面全体とTempoが跳ねる | Power Spike |
| C．規則解放 | Mirror，Node，Amplifier，Recipe，弾種 | 狙う場所と弾道が変わる | 本作の主Content |
| D．統合 | Capacity，Auto，Core Ring，Overdrive | 既存要素の組合せが変わる | 後半のMastery |

同じ種類を三十分以上続けない．AとBはCの間を埋めるが，Cの代用品にはしない．

### 5.2．Gold反復Upgrade

価格はLevel 12まで一つの指数式へ隠さず，明示表を正本とする．

| Upgrade | 効果 | 価格列 | 上限 |
|---|---|---|---:|
| Collector Value | Collector Base Gold＋1 | 10，30，75，180，450，1,100，2,800，7,000，18,000，45,000，110,000，270,000 | 12 |
| Gun Calibration | 全Target Gold×2 | 500，6,500，80,000 | 3 |
| Reload Mechanism | Cycle Time×0.85 | 400，4,500，50,000 | 3 |
| Survey Efficiency | Material Gauge閾値10→8→6 | 6,000，50,000 | 2 |

Reloadは`0.65→0.5525→0.4696→0.3992秒`となる．下限0.35秒を越えない．Gun Calibrationは小数丸めによる無効購入を避けるため，0.25加算から二倍節目へ変更する．

Collector Valueの最初の三購入は，10，30，75 Goldで，基礎値を1→2→3→4へする．Reference条件では約10秒，25秒，50秒に購入できる．その後はMirror，Reload，全Gold×2と競合し，単一路線の連打を避ける．

### 5.3．Target購入

| Target | 解禁／購入 | 追加価格 | 初期上限 | 表示する役割 |
|---|---:|---:|---:|---|
| Collector | New Gameで一個 | 二個目150，三個目500 | 3 | 命中面積と別経路．一発の上限は増えない |
| Mirror | Lifetime Gold 100で表示，250 Goldで一個付与 | 二個目1,500，三個目8,000 | 3 | 難しい反射経路でGold倍率 |
| Material Node | Lifetime Gold 800で予告，1,300で一個無償付与 | なし | 1 | 発光時にMaterial Gauge |
| Amplifier | 最初のMaterial取得後に表示 | 解禁費へ含む | 1 | 次のGold報酬へ×2 Token |

追加Collectorは経済倍率ではないことを購入前に明記する．P1で配置差が20％未満なら，二個目以降をPierce解禁後へ移す．

### 5.4．Material Node

| Parameter | 0.3.0初期値 |
|---|---:|
| Gauge閾値 | 10 |
| Readyでの有効命中 | Gauge＋1，1 Gold |
| 再充電 | 4.0秒 |
| 同一Lineage | Gauge一回まで |
| Gauge繰越 | あり |
| 乱数Drop | なし |

Nodeは支給時にReadyで開始する．Ready中は強い発光と盤面上の`READY`で示す．有効命中後は4秒再充電し，再充電中の命中はGaugeとGoldを与えない．物理的には通常弾と分裂弾を吸収し，貫通弾だけが通過する．したがって，再充電中はCollectorを撃ち，Readyになった時だけNodeへ照準を切り替えるのが基本行動になる．

最短Material時刻は，Ready開始を0秒として，10発目の36秒である．以後一個40秒であり，無操作待ちではない．

| 累積Material | 最短Pulse時間 | 主な用途 |
|---:|---:|---|
| 1 | 0分36秒 | 入手方法の理解，Amplifier表示 |
| 3 | 1分56秒 | Amplifier |
| 11 | 7分16秒 | Amplifier＋Recipe |
| 23 | 15分16秒 | 上記＋Pierce |
| 39 | 25分56秒 | 上記＋Split |
| 59 | 39分16秒 | 上記＋Capacity 5 |
| 84 | 55分56秒 | 上記＋Fixed Auto-fire |

これは一切外さず，Ready直後に必ず命中する理論下限である．実際にはGold購入，盤面編集，照準失敗が入る．Survey Efficiency購入後は閾値が8，6へ下がるため，後半だけMaterial速度を上げられる．

### 5.5．有限解禁

| 順 | Node | 表示条件 | 価格 | 付与 |
|---:|---|---|---:|---|
| 0 | Mirror Routing | Lifetime Gold 100 | 250 Gold | Mirror一個，Board Editing |
| 1 | Material Survey | 800で予告，1,300で支給 | 無償 | Material Node一個，専用Socket |
| 2 | Amplifier Circuit | Materialを初取得 | 1,500 Gold＋3 Material | Amplifier一個 |
| 3 | Recipe Workbench | Amplifier所有 | 4,000 Gold＋8 Material | Recipe Editor，Capacity 4 |
| 4A | Pierce Loading | Recipe Workbench | 9,000 Gold＋12 Material | 貫通弾 |
| 4B | Split Loading | Recipe Workbench | 12,000 Gold＋16 Material | 分裂弾 |
| 5 | Capacity＋1 | 4Aまたは4B | 25,000 Gold＋20 Material | Capacity 5 |
| 6 | Fixed Auto-fire | Capacity 5 | 50,000 Gold＋25 Material | 固定方向Auto-fire |

4Aと4Bは購入順を選べるが，最終的に両方取得できる．Material価格は購入で消費する．上表を順にすべて買う場合の累積Materialは84である．

Gold価格はMaterial理論下限より先に必ず貯まることを保証する値ではない．Gold経路が弱ければGold，Node切替が弱ければMaterialが律速になる．直近60秒の予測ではなく実績を使い，購入画面へ`現在速度なら約N秒`を表示する．

---

## 6．Reference進行

### 6.1．前提

机上の時刻は保証ではなく，比較用のReferenceである．

- Fast：有効Gold命中0.75秒ごと，Mirror経路使用率80％．
- Reference：有効Gold命中1.0秒ごと，Mirror経路使用率65％．
- Novice：有効Gold命中1.3秒ごと，Mirror経路使用率45％．
- Collector Value 10，30，75を早期購入し，Mirror，Reload一段，全Gold×2一段を順次購入する．
- UI閲覧と配置編集の追加時間は含めない．

### 6.2．初期実測モデル

| 出来事 | Fast | Reference | Novice |
|---|---:|---:|---:|
| Collector Value 1 | 7.5秒 | 10秒 | 13秒 |
| Collector Value 2 | 18.8秒 | 25秒 | 32.5秒 |
| Collector Value 3 | 37.5秒 | 50秒 | 65秒 |
| Mirror購入 | 1分25秒 | 1分53秒 | 2分27秒 |
| Reload一段購入 | 2分19秒 | 3分09秒 | 4分14秒 |
| 全Gold×2一段購入 | 3分26秒 | 4分43秒 | 6分26秒 |
| Material Node支給 | 3分28秒 | 4分47秒 | 6分31秒 |

このModelはMirror命中率を単純化しており，実ゲームのProjectile飛翔，Miss，UI滞在を含まない．Node支給の目標は初見3分30秒から7分とする．

### 6.3．質的解放の時間帯

| 経過目標 | 新しい判断 |
|---:|---|
| 0から0.5分 | 10 Hit，最初の＋1 |
| 0.5から3分 | Value，追加Collector，Mirrorへ貯める判断 |
| 2から5分 | 直接射撃と反射経路の比較 |
| 3.5から7分 | Material Node，Ready時の照準切替 |
| 6から10分 | Amplifier経路 |
| 10から16分 | 五発Recipe |
| 18から30分 | PierceとSplitの順序選択 |
| 30から50分 | Capacity 5，追加Socket，Stone候補 |
| 45から70分 | Fixed Auto-fire，手動割込 |
| 60から120分 | 盤面拡張，Core Ring条件，Capacity 6候補 |
| 120から150分 | Overdriveと最終経路統合 |
| 150から180分 | 新文法なし，Core撃破 |

P4で60分までの機能が45分未満に尽きた場合も，価格を上げて60分へ伸ばさない．追加Socket，Stone，Core外輪が異なる判断を作れる場合だけ時間帯を伸ばす．

---

## 7．UIと購入規則

### 7.1．段階表示

1．開始時はGold，10 Hit，Collector Valueだけを完全表示する．  
2．最初の購入後に，次のValue，追加Collector，Mirror Silhouetteを見せる．  
3．Lifetime Gold 100でMirror価格を見せる．  
4．Lifetime Gold 800でMaterial Nodeと支給条件を予告する．Material所持欄はまだ数値化しない．  
5．Node支給時にGaugeを表示する．  
6．最初のMaterial取得時にAmplifier価格を表示する．  
7．Skill Treeは現在Nodeと次の一段だけを完全表示し，その先は名称なしSilhouetteにする．

### 7.2．購入Cardの必須情報

- 現在値→購入後の値．
- 価格と所持額．
- 直近60秒実績から算出した概算回収時間．
- 盤面へ増えるObject数．
- `Gold生産`，`命中安定`，`Material速度`，`新規則`のどれか．
- 返金不可であること．

効果が整数丸めで0になる購入は有効化しない．一回の購入が直近の主要行為を原則15％以上変えない場合，まとめて一段にするか，価格を下げる．

---

## 8．Prototypeで確定する項目

### P0

- 10 Hitまでの中央値と95 Percentile．
- 10 Gold購入を説明なしで見つける率．
- Cycle 0.65秒，Cone 2から14度，飛翔速度900の射撃感．
- 散布偏差が表示Cone全幅`[-spread,+spread]`と一致すること．

### P1

- 追加Collectorが経済投資ではなく命中安定として理解されるか．
- Mirror経路の実収益が直接射撃より15％以上高くなるか．
- Target購入後30秒以内の配置変更率．
- Material Nodeの4秒Pulseが待機ではなく照準切替として機能するか．
- 最初のMaterial取得が支給後30から60秒に収まるか．

### P2

- Recipe順序変更によって照準が変わるか．
- PierceがMaterial Node吸収を越える価値を持つか．
- Splitが広い盤面で別経路を作るか．
- Material閾値10→8の購入が必須一択にならないか．

### P4

- GoldとMaterialのどちらか一方が90秒以上恒常的な壁にならないか．
- 質的解放間隔が20分を超えないか．
- Autoが熟練手動収益の60から80％か．
- 60分時点で新しい判断が尽きていないか．
- 現在収益からCore撃破までの予測が20分を超えないか．

---

## 9．採用判断

次をMaster 0.3.0へ反映する．

- 最初の10 Hit，10 Gold，Collector Value＋1は維持する．
- Collector Value価格を明示列へ変更する．
- Gun Calibrationを`＋0.25`から少数回の`×2`へ変更する．
- Reloadを一段15％短縮へ変更する．
- Material Nodeへ4秒Ready／Rechargeを導入する．
- Material Node予告をLifetime Gold 800，支給を1,300へ変更する．
- Material有限解禁価格を再設定する．
- Target購入時の一個付与，自動仮配置，無料移動を維持する．
- 0から60分の価格をINITIALとして固定し，60分以降の厳密価格とCore HPはP4の実収益から決める．

最も重要な残課題は，Material Pulseが面白い照準切替になるかである．PlayerがReady表示を無視してNodeを連打する，または4秒ごとの作業と感じる場合，Cooldown値だけを短くして救済せず，`一Lineage内でGold Targetを経由してからNodeへ入るとGauge増加`など，経路条件そのものをP1で比較する．
