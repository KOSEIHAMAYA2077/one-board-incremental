# 一盤面式インクリメンタルゲーム設計調査報告書

作成日：2026年8月27日  
調査時点：2026年8月27日  
対象：初見約2時間30分から3時間で完結する，無料の能動型インクリメンタルゲーム

## 0．結論

本作は成立する．ただし，独自性の中心を「物理弾が飛ぶこと」だけに置くと，終盤に自動化した時点で観察ゲームへ薄まりやすい．本作固有の最も強い遊びは，**五発レシピを編集し，盤面上の経路と一発ごとの照準によって，その順序効果を回収すること**である．

三要素の役割は，次のように固定するのがよい．

```text
五発レシピ ＝ Buildの設計図
Socket配置 ＝ 設計図が働く物理経路
照準        ＝ 現在弾に応じて経路を選ぶ能動操作
```

最終推奨案は，Ascensionなしの一直線進行である．盤面奥に最初から巨大Coreを見せ，通常Targetから得たGoldと，同じProjectile Lineageが生んだ収益をCore破壊力へ直結させる．150分前後で最終機能を解放し，150分から180分を最終構築と撃破に使う．Playtestで質的変化が120分までに尽きるなら，無理に3時間へ伸ばさず，2時間から2時間30分で完結させる．

## 1．エグゼクティブサマリー

### 1.1．本作の強み

- 数値成長が，Target追加，弾道，反射，分裂，画面密度として見える．
- 一発の照準と長期的な盤面構築を，同じ一盤面上で接続できる．
- 五発という短い循環列は，記憶可能で，現在弾に応じた照準変更も可能である．
- 敗北や技量壁なしでも，「うまく狙れば早い」という能動性を作れる．
- 一つの銃，一つの盤面，二通貨に絞れば，個人制作規模へ収められる．

### 1.2．最大の危険

最大の危険は，終盤の連鎖が派手でも，プレイヤーの判断が「最も高いTargetへ撃つ」だけになることである．五発の順番が結果へ明示的に影響せず，配置も最適解が一つなら，独自要素はすべて見た目へ退化する．

次点の危険は，終盤の自動化後に新しい観察対象がなくなり，最後の価格を待つだけになることである．Nodebusterでは，序盤の約60分から90分は評価される一方，終盤はプレイヤーが不要になり，最後の解放を待つだけという低評価が確認できる．[Steam Communityの否定的レビュー](https://steamcommunity.com/app/3107330/negativereviews/)．

### 1.3．Prototype前に固定する五項目

1．Ascensionなし，一盤面の一直線進行とする．  
2．Build主軸を五発レシピ，配置を補助軸，銃を操作感とテンポの軸とする．  
3．通常弾は次弾用Chargeを作り，特殊弾はChargeを消費して強化される．  
4．最終CoreへのDamageは，独立した攻撃力ではなく，そのLineageが生んだGoldから算出する．  
5．Target配置は，領域解放型Socket方式，編集時Pause，無料移動とする．

### 1.4．Prototype後まで決めない項目

- 五発一周期の実時間．
- 散布角，弾速，Targetの大きさ．
- 特殊装填容量と弾コストの最終値．
- 手動と自動の実収益差．
- 3時間という最終尺．
- 爆発弾，Gold弾，Chaos Loaderの採否．

### 1.5．削除推奨

- Ascension．
- 第二，第三の独立銃．一丁の射撃モード解放で代替する．
- 高度Auto Aim，自動購入，自動配置．
- 不可逆Skill Tree．
- 第三通貨，消費弾薬，Craft，Affix．
- 30発を物理的に並べるUI．

## 2．調査方法と情報の扱い

公式ストア説明を仕様上の事実，Steam User ReviewとCommunity投稿を利用者の意見，そこから本作へ変換した内容を本報告書の設計判断として分離した．レビュー率と件数は2026年8月27日取得値であり，言語別集計しか表示されない作品はその旨を表へ記載した．Play時間は公式値，レビュー記録時間，攻略時間を混同せず，範囲として扱った．

レビューは自己選択された標本であり，Steamの評価率だけから全プレイヤーの満足度を断定しない．また，攻略情報で見える最適解はVersion差の影響を受け得る．

## 3．類似作品比較

| 作品 | 長さ | 主操作 | 盤面成長 | 自動化 | Reset | 終点 | 評価上の長所 | 評価上の短所 | 本作への示唆 |
|---|---:|---|---|---|---|---|---|---|---|
| Idle Gumball Machine | 約2時間 | Gumballを物理盤面へ射出 | Gear，倍率，球数と画面密度が増える | 現行1.1でAuto-fire追加 | Ascensionあり，復帰は速い | ゲームを壊す数値到達 | 物理反応，音，絶えない成長 | 名称に反して能動入力が多い，短い | 物理盤面と短尺は相性がよい．Resetするなら復帰は1分級が必要 |
| Magic Archery | 約1時間 | Target作成，射撃，Quest | 矢と効果が増え，終盤の音と画面が膨らむ | 放置可能部分あり | 実質一直線 | 明確なEnding | 無料，短い，能動と放置の両立 | 面白くなった所で終わるという意見 | 終点は明確にしつつ，最終解放をEnding直前だけに置かない |
| Nodebuster | 約3時間から5時間 | CursorでNodeを破壊，回収 | Skill Treeと同時破壊数が増える | 後半はほぼ自走 | 複数回のRun更新 | Tree完成とEnding | 前半の密な解放，画面の加速 | 最後の約1時間に新しい判断がなく，待ちになる | 自動化後にもレシピ変更や新Targetを残す |
| Digseum | 公式約2.5時間 | Dig，Relic展示，Upgrade | 発掘地とMuseum展示が増える | Visitor収入など | Prestigeあり | 全地域，Museum完成 | 2時間から4時間の密度，意味のあるUpgrade | 終盤にSystemが減り，代替が増えないという意見 | 古い操作を委譲したら，同時に新しい判断を一つ渡す |
| Tower Wizard | 約3時間から5時間，遊び方により長い | Magic収集，Spirit召喚，建築 | Towerが縦に育ち，Systemが追加 | Spiritが旧操作を委譲 | Prestige必須 | 明確なEnding | 建築の視覚成長，Resetごとの新System | 数値が速すぎ選択が無意味，または中盤以降が待ちとの両論 | 数値加速そのものより，盤面の新しい働きを報酬にする |
| Minutescape | 約3時間から4時間 | Bullet回避 | 生存時間，Ability，Challenge | 基本は能動 | 死亡ごとの恒久成長 | 5分生存 | 高いSkill ceiling，終盤Ability | 40分以降のGrind，技量があっても壁，強Upgrade一択 | 能動操作を進行の短縮に使い，進行許可条件にはしない |
| Xenosensory | 約2時間から3時間 | Cursor hoverでXeno破壊，Action使用 | 敵，囚人，Gadget，物語が展開 | Mouse追従攻撃，Meta進行 | 死亡と再出撃 | Story Ending | Accessibility，雰囲気，失敗しても必ず進む | Gadgetが不要でも勝てる，Metaは浅い | 難易度壁なしは本作と相性がよいが，特殊弾を任意の飾りにしない |
| Loot Loop | 約2.5時間から3.5時間 | 数秒Run，Loot回収，Skill使用 | PartyとSkill Tree，Dungeonが進む | Auto battle中心 | 一回のPrestige | Final boss | 即時報酬，一回だけの高速再走 | 一回Reset後すぐ終わる，低入力，終BossのRNG | 一回Resetを入れるより，一盤面の後半変形へ置換する |
| More Sushi! | 約20分から1時間 | Sushi提供，Upgrade，Helper | Sushi種とConveyorが増える | Helper | 約3回のPrestige | 四つのDebt完済 | かわいい，短く満足しやすい | 目標予告不足，下位Sushiが死ぬ，内容不足 | 古いTargetの役割を明示し，遠い目標を最初から見せる |
| Pincremental | 数十時間から数百時間，明確な短編終点なし | Pinball paddle，Upgrade | 初期に少数の盤面要素追加後，ほぼ数値 | Paddle，購入，Prestigeまで多層自動化 | 公式説明で5層以上 | 長期階層進行 | PinballとIncrementalの着想 | Resetで自動化を失う，盤面が早期に変化しなくなる | 物理盤面を序盤の飾りにせず，終盤まで構造変化させる |
| (the) Gnorp Apologue | 約15時間から30時間 | 生産・回収Build構築 | Rock周辺にGnorpsと設備が増える | 生産と回収は自走 | Compressionを複数回 | 明確なEnding | 画面と数値の一致，強いBuild synergy | 不可逆選択，不透明数式，正解Buildを外すと長いRunが無駄 | 深い相互作用は学ぶが，短編では無料Respecと効果予測が必須 |

### 3.1．一次情報とレビュー根拠

- Idle Gumball Machineは，公式にPlinko型の短編Incremental，Ascension，Auto-fireを掲げる．2026年8月27日時点で全248件，90％好評だった．[Steam Store](https://store.steampowered.com/app/3724490/Idle_Gumball_Machine/)．現行Auto-fireは1.1 Updateで追加されたため，発売初期と現行版を分ける必要がある．[Steam Community Update](https://steamcommunity.com/app/3724490)．レビューでは約2時間，Reset後の復帰が約1分で停滞しないという評価が見られる．[Steam Reviews](https://steamcommunity.com/app/3724490/reviews/?browsefilter=toprated)．
- Magic Archeryは，公式に「short and active」「definite ending」を掲げる無料作品で，英語Reviewは6,257件，98％好評だった．[Steam Store](https://store.steampowered.com/app/2905170/Magic_Archery/)．Communityでは約1時間という報告と，終盤の重なった効果音が満足感になるという意見がある．[Steam Community](https://steamcommunity.com/app/2905170/)．
- Nodebusterは，公式に短編のNode破壊とSkill Treeを掲げ，英語Reviewは10,612件，97％好評だった．[Steam Store](https://store.steampowered.com/app/3107330)．一方，否定的Reviewでは，前半後に操作が資源回収だけとなり，最終解放を待つ問題が一致している．[Steam Negative Reviews](https://steamcommunity.com/app/3107330/negativereviews/)．
- Digseumは，公式に12以上の発掘地，50以上のRelic，Skill Tree，約2.5時間を掲げ，英語Reviewは6,389件，97％好評だった．[Steam Store](https://store.steampowered.com/app/3361470/Digseum/)．Reviewでは2時間から4時間の密度と意味のあるUpgradeが評価される一方，終盤にSystemが消えて新しいSystemが補充されないという指摘もある．[Steam Reviews](https://steamcommunity.com/app/3361470/reviews/?browsefilter=toprated)，[個別Review](https://steamcommunity.com/id/kermamrek/recommended/3361470/)．
- Tower Wizardは，公式に能動型，Prestige，固有Mechanic，明確なEndingを掲げ，英語Reviewは7,090件，96％好評だった．[Steam Store](https://store.steampowered.com/app/3372980/Tower_Wizard/)．Resetごとに新Systemが来るという擁護と，数値が速すぎて説明を読まず買える，中盤以降は小幅改善だけという批判が併存する．[Prestige議論](https://steamcommunity.com/app/3372980/discussions/0/599658915298370885/)，[否定的Review](https://steamcommunity.com/app/3372980/negativereviews/?browsefilter=toprated)．
- Minutescapeは，5分連続生存を終点とする能動型で，全541件，74％好評だった．[Steam Store](https://store.steampowered.com/app/3327170)．Reviewでは3時間前後だが，40分後のGrind，少量しか進まないUpgrade，生存Skillがあっても価格壁を越えられないことが低評価へつながる．[Steam Reviews](https://steamcommunity.com/app/3327170/reviews/?browsefilter=toprated)，[完走後の議論](https://steamcommunity.com/app/3327170/discussions/0/563626538148335216/)．
- Xenosensoryは，Cursor hoverで敵を自動攻撃し，ResearchでSkill，Action，Gadgetを解放する短編で，全151件，95％好評だった．[Steam Store](https://store.steampowered.com/app/4037830/Xenosensory/)．ReviewではNormalの100％が約2.5時間，失敗しても進むこと，MouseのみのAccessibilityが評価される一方，Gadgetを使う必要がないという指摘がある．[Steam Reviews](https://steamcommunity.com/app/4037830/reviews/?browsefilter=toprated)．
- Loot Loopは，数秒のDungeon run，Party，Loot，Skill Treeを掲げ，全1,621件，92％好評だった．[Steam Store](https://store.steampowered.com/app/3972320)．Reviewでは一回のPrestigeで高速再走する点が評価される一方，その後約20分で終了する短さ，Final bossのRNG，Cursor回収の残存作業が指摘される．[Steam Reviews](https://steamcommunity.com/app/3972320/reviews/?browsefilter=toprated)．
- More Sushi!は，Debt返済後，PlateをStarへ変えるPrestigeを持ち，全181件，87％好評だった．[Steam Store](https://store.steampowered.com/app/3950770/More_Sushi)．ReviewではUpgradeが約10分で埋まり，Prestigeが目的と手段を兼ねて遠い目標が見えないこと，下位Sushiが実質不要になることが指摘される．[Steam Reviews](https://steamcommunity.com/app/3950770/reviews/?browsefilter=toprated)．
- Pincrementalは，Pinball，60以上のUpgrade，公式説明上5層のPrestigeを持ち，全656件，75％好評だった．[Steam Store](https://store.steampowered.com/app/1369470/Pincremental/)．否定的Reviewでは，盤面変化が初期で止まり，Resetが自動化を奪い，後続Upgradeが新しい遊びでなく不便の再解消になる点が繰り返し問題視される．[Steam Negative Reviews](https://steamcommunity.com/app/1369470/negativereviews/?browsefilter=toprated)．
- (the) Gnorp Apologueは，生産と回収を物理画面へ可視化し，TalentによるBuildを主眼とする．英語Reviewは7,367件，95％好評だった．[Steam Store](https://store.steampowered.com/app/1473350/_/)．攻略上はProgress段階ごとに有力Buildがあり，否定的Reviewでは不可逆選択と不透明な効果により，数時間後に失敗Buildと判明する問題が指摘される．[Steam Negative Reviews](https://steamcommunity.com/app/1473350/negativereviews/?browsefilter=toprated&l=english&snr=1_5_100010_)．

## 4．調査仮説の判定

| 仮説 | 判定 | 根拠と本作への変換 |
|---|---|---|
| 短編は長さより新機能密度が重要 | 支持 | Digseumの2時間から4時間は密度が評価され，MinutescapeとNodebusterは同程度でも空白区間が批判される |
| 視覚成長は単純倍率より強い | 強く支持 | Gnorp，Tower Wizard，Idle Gumball Machineで数値と盤面変化の一致が高評価要因である |
| 能動操作は両立するが技量壁は危険 | 支持 | Magic ArcheryとXenosensoryは軽い能動性が高評価で，Minutescapeは強制生存壁が賛否を分ける |
| 自動化は旧操作の委譲なら成長になる | 支持 | DigseumのPrestige復帰短縮，Idle GumballのAuto-fireは好意的．Pincrementalのように委譲をResetで再没収すると不満になる |
| 倍率だけのResetは短編で水増しに見える | 強く支持 | Pincremental，More Sushi!，Tower Wizardの否定的意見に共通する |
| 不透明かつ不可逆なBuildは攻略強制になる | 強く支持 | Gnorpの否定的Reviewと多数の段階別Guideが典型である |
| 入力不要化後には新しい観察対象が必要 | 強く支持 | Nodebuster終盤とPincremental中盤以降の低評価が直接示す |

## 5．正式採用するゲーム構造

### 5.1．最終目標

盤面奥中央に，開始時から巨大Coreを置く．Coreは三枚のShield Ringを持ち，盤面の成長段階に応じて一枚ずつ機能を変える．

1．外輪はGold総獲得で解除する．経済成長を教える．  
2．中輪は三種類のTargetを一Lineageで通過してCoreへ当てると解除する．連鎖を教える．  
3．内輪は五発一周期のうち，通常，貫通，分裂をそれぞれCoreへ届けると解除する．レシピ理解を教える．  
4．Core本体は，Lineage Yieldに基づくDamageで破壊する．最終Buildを試す．

GoldとDamageを別の成長値にはしない．各Lineageは，命中で得た実Goldを`Lineage Yield`へ累積する．Coreへ到達した時のDamageは次とする．

```text
Core Damage = 直接命中価値 + floor(Lineage Yield × Core変換率)
```

仮に，中盤の弾が木Targetで20 Gold，増幅Target後の石Targetで80 Goldを生み，最後にCoreへ基礎30で命中し，変換率が0.5なら，`30 + floor((20 + 80) × 0.5) = 80 Damage`となる．高収益経路を作ることが，そのままCore攻略になる．

Ending後は「盤面を眺める」「もう一度Ending演出を見る」「New Game」のみ残す．無限進行用の新経済は作らない．

### 5.2．Ascension

採用しない．

一回Reset案は，過去盤面の高速再構築を見せられる利点がある．しかし，本作は同じ一盤面を巨大装置へ変える継続性が強みであり，Resetはその視覚履歴を破壊する．Loot Loopの一回Prestigeは強くなった実感を作るが，直後に終わって短さを強調したというReviewもある．本作では，Resetの代わりに120分前後で**Overdrive**を解放し，盤面を消さずに旧制約を一つ壊す．

Overdriveでは，特殊装填容量を増やし，Core Shieldを開き，描画と音楽を一段変える．これにより「二周目の爆発的成長」を一盤面内で得る．失うものは，序盤を高速で再構築するPrestige固有の快感である．

### 5.3．盤面とSocket

論理盤面を16:9の固定座標系，例えば1600×900とする．実WindowはLetterboxまたは安全領域付きScaleで表示し，物理計算は論理座標から変えない．

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

- 開始時はA1，A2，B1，B2の四Socketを使える．
- 15分で中央Utility Socket U1，U2を解放する．
- 30分で右半分，60分でCore周辺，90分で側壁Socketを解放する．
- Targetは無料で移動でき，編集時はPauseする．
- 同種複数配置はBasic Targetのみ最大3個，Utilityは原則1個とする．
- Socket間隔はTarget直径の1.5倍以上とし，配置画面で予測反射線を表示する．
- 自由配置は採用しない．反射角の微調整が主役になり，最適座標の攻略共有を招くためである．

Socket選択の自由を残しつつ最適配置を一つにしないため，Target価値を位置で変えない．代わりに，現在のレシピと狙うLineageにより，直線，扇形，反射のどのSocket群が有利か変わるようにする．

### 5.4．Targetの最小構成

| Target | 直接報酬 | Projectile処理 | 同一Lineage再起動 | Cooldown | 終盤の役割 |
|---|---|---|---|---|---|
| 木製Collector | 基礎Gold 1 | 通常弾を吸収，貫通弾は通過 | Goldは一回 | なし | 通常弾で次弾Chargeを作る安定起点 |
| Stone Bank | 基礎Gold 5 | 弾速を落として通過 | Goldは一回 | 0.25秒 | Lineage Yieldの主収益源 |
| Mirror | 0 | 反射 | 特殊効果は一回 | なし | Coreへの迂回経路と手動高収益 |
| Amplifier | 0 | 通過，次の報酬倍率×2 | 一回 | なし | 配置と順序の接続点 |
| Prism | 少額Gold | 子弾を生成 | 一回 | なし | 分裂Build．生成Budgetを消費する |
| Material Node | 少額Gold＋素材Gauge | 吸収 | Gaugeは一回 | 2秒 | 有限解放の進行，終盤はOverdrive材料 |
| Relay | 基礎Gold 3 | 通過，1.5秒後に再武装 | Lineage特殊は一回 | 1.5秒 | 自動射撃の安定経路と周期調整 |
| Core | Goldなし | Lineageを終端 | 一回 | なし | 最終目標 |

Prototype 1ではCollector，Mirror，Amplifierだけを実装する．Prismは分裂弾と役割が重なるため，Prototype 2で弾自体の分裂が面白い場合だけ比較する．

### 5.5．銃

初期版は一丁だけにする．主要Build軸はレシピであり，複数銃をBuild軸にすると弾種と競合する．銃は次の三つのParameterに絞る．

- Cycle Time：次弾を撃てるまでの時間．
- Focus Range：最小，最大散布角．
- Impact Multiplier：GoldとCoreへの共通倍率．

初期仮説値は，単発，自動Reload 0.65秒，弾速900 logical px/s，散布角2度から14度である．一発銃は，現在弾を認識し，着弾を見る間を作る利点がある．欠点は外れた時の空白であるため，Reloadは0.5秒から0.8秒の範囲で検証し，1秒を超えさせない．Reload中は次弾Iconの装填，薬莢，短いPitch上昇音を同期させる．

60分で長押し射撃，120分でOverdrive Burstを解放する．これは別銃でなく，同じ五発レシピを一周期まとめて撃つ射撃モードである．

### 5.6．五発レシピと順序効果

五発は維持する．三発ではBuild差が小さく，七発以上では現在位置を忘れやすい．初期0.65秒なら一周3.25秒であり，現在弾を見て狙いを変えられる．

五発レシピは`Cycle Charge`を共有する．

- 通常弾がTargetへ命中するとChargeを1得る．上限2．
- 次に発射した特殊弾がChargeをすべて消費し，効果を強化する．
- 特殊弾が外れた場合もChargeは消費する．結果を予測可能にするためである．
- 五番目から一番目へもChargeを引き継ぐ．五枠を循環列として成立させる．
- ReloadではResetしない．物理Magazineとレシピを分離した設計と矛盾させない．

例：`通常→通常→分裂→通常→貫通`なら，分裂弾はCharge 2で子弾が増え，貫通弾はCharge 1で貫通数が増える．`分裂→貫通→通常→通常→通常`では，最初の二発が未強化となり，同じ弾数でも結果が変わる．

同じ特殊弾だけで埋めるとChargeを作れず弱い．同じ通常弾を複数入れると，安定収益と次の特殊弾強化になる．これが通常弾の終盤まで残る自然な役割である．

Presetは初期版では不要である．編集履歴のUndo一回，Slot入替，弾Iconを全Slotへ配る操作だけを用意する．Playtestで一回のSession中に6回以上，全体構成を往復する参加者が半数を超えた場合だけPresetを二つ追加する．

### 5.7．弾種

| 弾 | 単体効果 | Charge消費効果 | Target相互作用 | 仮Cost | 重複する意味 | 終盤破壊Upgrade |
|---|---|---|---|---:|---|---|
| 通常 | 基礎倍率1.0，命中でCharge＋1 | 消費しない | Collectorで確実にCharge生成 | 0 | 二連で最大Charge | Overdrive時，Charge上限＋1 |
| 貫通 | 2 Targetを通過 | Chargeごとに＋1貫通 | Stone列，Amplifier後の直線に強い | 2 | 別々の列を周期的に掃く | 訪問済みTarget一個ごとに弾幅拡大 |
| 分裂 | 命中後2子弾 | Chargeごとに＋1子弾 | PrismではなくMirror扇形と相性 | 3 | 広い盤面を複数周期で覆う | 子弾の一部を論理Batch化して大量連鎖 |
| 爆発 | 終点で範囲Event | Chargeごとに半径＋25％ | 密集Socketへ強い | 3 | 爆発時刻を周期化 | Lineage Yieldの一部を範囲報酬へ再投資 |
| Gold | 直接効果は弱い | Chargeごとに次の二命中のGold＋50％ | 高基礎値Targetへ狙う | 2 | Buff維持率を上げる | Core変換率も強化 |
| Chaos | 発射時に解放済み特殊弾から抽選 | Chargeを消費し，抽選効果を一段上げる | 配置対応を毎回変える | 1 | 不要．同時1Slot制限 | なし．正式版後半の遊び枠 |

直線貫通，広範囲分裂，反射連鎖はPrototype 2で成立させる．爆発，Gold変換，Chaosは基本三Buildが相互に異なる照準を要求した場合だけ追加する．

### 5.8．特殊装填容量

初期容量4，貫通Cost 2，分裂Cost 3とする．これにより，初解放時は貫通二発または分裂一発を選び，残りを通常弾にする．

容量の仮進行は4→5→6→8とし，120分のOverdriveで8へ跳ねる．同種重複追加Costは採用しない．表示規則を複雑にする割に，Charge構造と容量だけで単色Buildを抑制できるためである．終盤は通常弾のCharge上限を壊すが，通常弾自体を不要にはしない．

容量が面白い選択になっている成功条件は，参加者の70％以上が二つ以上の有効レシピを説明でき，単一レシピの使用率が全体の60％未満であることとする．

### 5.9．経済

通貨はGoldとMaterialの二つだけにする．Materialは一種類のGaugeとして扱い，重要解放を確率Dropにしない．Material Nodeへの有効命中でGauge＋1，10でMaterial一個を確定取得する．後半Upgradeで必要命中数を8，6へ減らす．

一命中のGold仮式は次とする．

```text
Gold = floor(Target基礎値 × 銃倍率 × 弾倍率 × 経路倍率)
銃倍率 = 1 + 0.25 × 銃Upgrade Level
```

具体例：

- 序盤：Collector基礎1，銃Level 0，通常弾1.0，経路1.0なら，`floor(1×1×1×1)=1 Gold`．
- 中盤：Stone基礎10，銃Level 4，貫通弾1.25，Amplifier後2.0なら，`floor(10×2×1.25×2)=50 Gold`．
- 終盤：高価値Target基礎100，銃Level 12，Gold弾1.5，経路2.5なら，`floor(100×4×1.5×2.5)=1500 Gold`．

Target上には基礎値`10`を常時表示し，命中時には実獲得`+50`を表示する．連鎖終了時にはCursor付近で`Lineage total +240`を一回表示する．大量の個別数字は同Target，同Frame内で合算する．

量的Upgrade価格の仮式は`C(n)=ceil(C0×1.75^n)`とする．例えばC0=10なら，Level 0から4の価格は10，18，31，54，94である．これは確定値ではない．一購入で収益が最低15％変わるか，次の購入まで無選択の時間が90秒を超えないかをPrototype 4で測る．

Target配置とレシピは常時無料Respecとする．Gold Upgradeは購入前に増加量を表示し，購入後30秒以内なら全額返金できる．有限解放は取り消し不要で，最終的に全取得可能とする．

### 5.10．自動化

自動化の最終上限はSweep Servoまでとし，高度Auto Aimを削除する．

1．30分前後：最後に指定した方向へのAuto-fire．  
2．90分前後：二方向間を一定周期で往復するSweep Servo．  
3．手動入力時：即座に方向を上書きし，3秒後に自動へ戻る．

Auto-fireは次弾，Target Cooldown，反射経路を理解しない．Sweepも幾何学的な往復だけである．手動に直接倍率は付けない．うまい手動は，現在弾をAmplifier，Mirror，Coreの適切な経路へ通すため高収益になる．

仮説として，固定方向Autoを100とした時，普通の手動を130，熟練手動を160，Sweepを115とする．自動が熟練手動の60％未満なら役に立たず，85％を超えると照準判断が消える．同一Save，同一60秒区間を三回ずつ測り，中央値で比較する．

Offline収益，自動購入，自動配置は入れない．作品全体が一Sessionで終わり，起動中の盤面観察が価値だからである．

## 6．3時間の仮進行

| 経過 | 主操作 | 新しい解放 | 盤面の見た目 | 次の目標 | 停滞Risk |
|---:|---|---|---|---|---|
| 0分 | 一発ずつ照準，距離で散布変更 | Collector，銃倍率 | 銃，木Target，遠方Core | 最初のUpgrade | 外れ続ける |
| 5分 | Mirror経由を狙う | 二つ目のSocket，Mirror | 初の反射線 | 一Lineageで二命中 | 反射理由が読めない |
| 15分 | Target配置を試す | Amplifier，Material Node | 中央領域と素材Gauge | 特殊弾解放 | 配置が作業になる |
| 30分 | 五発の次弾を見て狙う | 通常，貫通，分裂，容量4，Auto-fire | Recipe bar，分裂軌跡 | 二種類のBuildを作る | 説明過多 |
| 60分 | 手動とAutoを切替 | 長押し，Stone Bank，右側Socket | 盤面が左右へ拡張 | Core外輪解除 | 量的Upgradeだけの空白 |
| 90分 | Sweep範囲と反射を調整 | Sweep Servo，Relay，容量6 | 周期運転する装置 | 三Target連鎖 | Autoが強すぎる |
| 120分 | Core経路を組む | Overdrive，容量8，Burst | 音楽，色，発射密度が変化 | Core中輪解除 | 性能と可読性 |
| 150分 | 最終レシピを比較 | Gold弾または爆発弾の一方，Core内輪解除 | Coreが露出 | Lineage Yield最大化 | 最終弾が必須一択 |
| 165から180分 | 手動で高収益経路を通す | 新Systemなし，最終運用 | 盤面全体がCoreへ収束 | Core破壊 | 単なるHP待ち |

60分から90分に，量的Upgradeだけの空白が生じる危険がある．RelayとSweepを同時に90分へ置かず，Playtestで60分時点の新しい判断が薄い場合は，Relayを70分へ前倒しする．150分以降は新Systemを追加せず，既存三軸を統合する最終試験にする．Core撃破の予測時間が20分を超えた場合，HPを下げる．

## 7．UI Wireframe

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
│ [通常] [通常] [分裂] [通常] [貫通]   Charge ●●   容量 5/6   │
│ [Recipe編集] [Target配置] [Auto：固定方向] [Upgrade]          │
└───────────────────────────────────────────────────────────────┘
```

- 現在弾を150％Scale，次弾を120％，残り三発を100％で表示する．
- 弾種は色だけでなく，通常は丸，貫通は針，分裂は三叉，爆発は輪，Goldは六角形とする．軌跡も実線，二重線，枝線，波線で分ける．
- Cone内に中心線と外縁を出し，現在武器の最小散布角へ達したら外縁色を変える．
- Targetには基礎値，命中時に実Gold，連鎖終了時にLineage totalを別階層で表示する．
- 重要な現在弾だけ完全不透明，子弾は世代ごとに90％，70％，50％へ落とす．
- Floating Numberは同Targetの0.1秒窓で合算し，画面上限12個とする．
- Recipe，配置，研究を開くと盤面を即時Pauseし，閉じるButtonとEscの両方で復帰する．

## 8．連鎖処理と性能上限

監査資料のEvent順を採用する．ただし，`訪問済みTarget`を一集合にせず，`効果発動済み`と`報酬取得済み`に分ける．これにより，Mirrorの特殊効果はLineage一回に制限しつつ，Relayの再武装後に別Lineageが報酬を得られる．

```text
1．衝突を確定する
2．LineageとTargetの効果発動済み，報酬取得済みを判定する
3．報酬を計算し，Lineage Yieldへ加算する
4．Target固有効果を予約する
5．Projectile固有効果を予約する
6．子弾へ状態を継承する
7．消滅，反射，貫通を確定する
8．予約Eventを安定したID順で実行する
```

仮上限は，分裂Depth 3，反射6回，貫通8回，一Lineage生成Budget 64，同時描画Projectile 250とする．論理命中は描画と分離する．描画上限を超えた子弾は消すのではなく，同じLineage，同じ方向Bucket，同じ0.1秒窓を`Swarm Event`へ束ねる．画面には太い粒子帯一本と`×24`を表示し，報酬は24発分を一括計算する．

上限到達時に新規効果を無言で抑止してはならない．Budget meterを一瞬表示し，束ねられた軌跡を太くする．これにより「Buildが壊された」ではなく「規模が集約表示された」と理解できる．

## 9．Tutorialと情報開示

- 起動後5秒以内に一発撃てる．文章Modalを先に出さない．
- 一発目はTargetを大きくし，外しても壁で一度反射して戻る配置にする．
- 二発目に，近いCursor位置と遠い位置のConeをGhostで交互表示する．
- Mirrorは購入前に一度，NPC Shotが反射する実演を見せる．
- 五発レシピは30分ではなく，最初の特殊弾入手時に解放し，`通常→貫通`と`貫通→通常`の結果差を短い予測線で見せる．
- Editorでは，各特殊弾のCostと，選択中Slotで消費する予測Chargeを表示する．
- すべての倍率式を常時出さず，Tooltipで`基礎10 × 銃2.0 × 経路2.0 = 40`と実値を出す．
- 強い完成レシピは教えない．ただし，「通常弾は次の特殊弾を強化する」という文法は隠さない．

## 10．意思決定表

| 論点 | 推奨案 | 代替案 | 採用理由 | 主な危険 | 決定時期 | Prototype検証 |
|---|---|---|---|---|---|---|
| 終点 | 可視Coreの段階解除と破壊 | Gold到達，研究完成 | 盤面成長と視覚的終点を統合 | HP待ち | 今すぐ固定 | 最終30分の操作密度 |
| Ascension | なし | 一回 | 一盤面の履歴を守る | 再構築快感を失う | 今すぐ固定 | Prototype 4後にのみ再監査 |
| Build主軸 | 五発レシピ | 銃，Target配置 | 独自性と理解可能な順序判断 | 装備欄化 | 今すぐ固定 | 並替前後の結果説明率 |
| 配置 | 領域解放Socket，無料移動 | 固定，自由配置 | 経路判断と制作規模の均衡 | 最適配置固定 | 推奨初期値で試作 | 配置変更数と収益差 |
| 銃数 | 一丁＋Mode解放 | 三丁切替 | Build軸競合を避ける | 見た目の変化不足 | 正式版後半まで延期 | 射撃感Prototype |
| 初期弾 | 通常，貫通，分裂 | 爆発，Goldも同時 | 三つで順序と空間差を検証可能 | 少なく見える | 推奨初期値で試作 | 混成率，照準差 |
| 通常弾 | Charge生成 | 低Costだけ | 終盤も特殊弾を準備する | 必須枠化 | 今すぐ固定 | 通常弾0枚と2枚の比較 |
| 容量 | 初期4，Cost 2/3 | 弾ごとのCooldown | 一画面で理解可能 | 単なる制限 | Prototype結果後 | 有効レシピ数 |
| 素材 | 確定Gauge | 確率Drop＋天井 | 不運停止を消す | 驚きが弱い | 今すぐ固定 | 素材待ち時間 |
| Damage | Lineage Yield連動 | 独立Attack値 | Gold経済と終点を接続 | 複雑表示 | 今すぐ固定 | 式の説明率 |
| 自動化 | 固定方向，後にSweep | 高度Auto Aim | 成長と手動価値を両立 | 自動が弱い | 推奨初期値で試作 | Auto対手動中央値 |
| Preset | 初期なし | 二Preset | UI規模削減 | 再編集作業 | Prototype結果後 | 往復編集回数 |
| Chaos | 延期 | 初期実装 | 決定論的文法を先に教える | 後半Content不足 | 正式版後半まで延期 | 基本三Build成立後 |
| Offline | なし | 上限付き収益 | 一Session完結 | 放置層の期待不一致 | 削除 | 不要 |
| 自動購入 | なし | 終盤だけ | 選択を残す | 購入作業 | 削除 | 購入頻度のみ観測 |

## 11．Prototype検証計画

### Prototype 0．射撃感

- 最小機能：単発銃，Cone，Collector一個，壁一枚，1 Gold，自動Reload．
- 仮説：照準距離と散布が説明なしで理解でき，外れも納得できる．
- 観測：初発時間，初命中までの発射数，Cursor距離分布，Reload中の追加入力，主観5段階．
- 成功：90％が5秒以内に発射，80％が三発以内に命中，70％が近距離ほど広がると自力説明する．
- 失敗：三発連続Missが20％超，Reloadを「待つだけ」と答える人が半数超．
- 成功時だけ追加：Mirrorと反射予測．

### Prototype 1．盤面成長

- 最小機能：5 Socket，Collector，Mirror，Amplifier，Target追加，無料移動，四つの量的Upgrade．
- 仮説：Target購入と配置変更が，数値Upgradeと異なる成長に感じられる．
- 観測：初Target追加時刻，配置変更回数，変更前後60秒Gold，高価Targetだけを狙う率．
- 成功：70％が少なくとも二配置を試し，変更で20％以上の収益差を出す．古いCollectorが終盤経路に残る．
- 失敗：全員が同一Socket，同一Targetだけを狙う．
- 成功時だけ追加：Material Nodeと領域解放．

### Prototype 2．五発レシピ

- 最小機能：通常，貫通，分裂，Charge，容量4，Recipe UI，Lineage，訪問済み集合．
- 仮説：並び順が結果を変え，参加者が因果を説明できる．
- 観測：変更回数，混成率，五発位置の見落とし，変更前後Gold，説明内容．
- 成功：70％が二つの有効レシピを作り，60％が「通常が次の特殊弾を強化した」と正しく説明する．単一構成が60％未満．
- 失敗：順番を変えても照準が変わらない，または最適列が一つに集中する．
- 成功時だけ追加：Gold弾か爆発弾の一方．両方同時に追加しない．

### Prototype 3．自動射撃

- 最小機能：最後の方向へのAuto-fire，手動割込，60秒収益記録．
- 仮説：委譲が成長に感じられ，手動には盤面理解による優位が残る．
- 観測：Auto継続時間，手動割込回数，Autoと普通手動と熟練手動のGold中央値．
- 成功：Autoが熟練手動の60％から80％，解放後も参加者の70％が一分に一回以上介入する．
- 失敗：Autoを誰も使わない，または解放後に誰も盤面へ触らない．
- 成功時だけ追加：Sweep Servo．

### Prototype 4．30分から60分進行Slice

- 最小機能：二通貨，三Target，三弾，容量，Auto，Core外輪，仮Ending，Save．
- 仮説：量的Upgradeと質的解放が交互に来て，Coreが通常収益と同じBuildで進む．
- 観測：全解放時刻，無選択最長時間，次目標を答えられる率，Core用だけの別Build率，離脱時刻．
- 成功：無選択区間90秒未満，80％が次目標を答え，70％が通常収益BuildをCoreにも使う．
- 失敗：Core前だけ別のDamage Upgradeを待つ，20分以上新しい判断がない．
- 成功時だけ追加：60分以降の領域，Overdrive，最終弾一種．この時点で完成尺を決定する．

## 12．未確定事項

| 未確定 | 理由 | 不足情報 | 必要Prototype | 判断値 |
|---|---|---|---|---|
| 完成尺が2時間か3時間か | Content密度は机上で確定不能 | Sliceの空白時間 | P4 | 質的解放間隔，離脱 |
| 五発一周の時間 | 射撃感と認知負荷に依存 | 0.5秒から0.8秒比較 | P0，P2 | 次弾誤認率 |
| 散布角曲線 | Mouse距離の体感に依存 | 解像度別入力 | P0 | 命中率と説明率 |
| Socket数 | 配置差とUI密度の均衡 | 5，9，13 Socket比較 | P1 | 有効配置数 |
| 特殊容量 | Build多様性に直結 | Cost別の選択分布 | P2 | 単一Recipe占有率 |
| Prism Target | 分裂弾と役割重複 | 分裂だけで盤面成長が足りるか | P2 | 分裂Buildの差 |
| 爆発かGold弾か | 両方は規模超過 | 基本Buildに欠ける空間 | P2，P4 | 未使用Build軸 |
| Sweep Servo | 固定Autoだけで十分か | 介入頻度 | P3 | Auto中の手動回数 |
| Core HP | 収益曲線に依存 | 終盤Buildの実測 | P4以降 | 撃破予測時間 |
| Projectile上限 | 実装環境と演出に依存 | 実機負荷 | P2以降 | Frame time，視認性 |

## 13．最終質問への回答

1．**成立するか**．成立する．一盤面に経済，構築，操作，終点を統合できる．  
2．**最も強い遊びは何か**．五発レシピである．照準は瞬間操作，配置は補助設計として支える．  
3．**五発配列は結果差を生むか**．通常弾のCharge生成と特殊弾の消費を入れれば生む．入れなければ単なる装備欄になる．  
4．**通常弾の終盤役割**．次の特殊弾を強化するCharge生成，安定したGold，Lineageの始動である．  
5．**Target配置自由度**．領域解放型Socket，無料移動，同種個数制限まで．座標自由配置は不要である．  
6．**自動射撃**．30分前後に固定方向，90分前後に単純Sweepまで．Targetや次弾を読むAuto Aimは入れない．  
7．**Ascension**．不要である．盤面を保持したOverdriveで質的跳躍を作る．  
8．**最終Targetとの接続**．Lineage YieldをCore Damageへ変換すれば自然に接続する．  
9．**解放順**．射撃，反射，配置，五発と三弾，自動化，周期Target，Overdrive，最終変換弾，Coreの順とする．  
10．**正式仕様書前の確認**．射撃の納得感，配置による経路差，五発順序の説明可能性，混成Build，自動対手動比，30分から60分Sliceの空白，論理Batch化の可読性を確認する．

最も重要なGo／No-Go条件はPrototype 2である．五発の順番を変えても，参加者が照準を変えず，結果差を説明できない場合，本企画は五発レシピを独自性として正式仕様へ進めるべきではない．その場合は，レシピを削り，Target配置を主軸に再設計する．
