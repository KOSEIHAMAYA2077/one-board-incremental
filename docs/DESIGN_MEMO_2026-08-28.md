# 設計メモ 2026-08-28

## 1．現在地

本作は，一盤面を最後まで育てる能動型Incrementalである．一丁のGunから狙って撃ち，盤面上のTargetを収益装置として利用し，五発Recipe，Target配置，照準を組み合わせて巨大Coreを破壊する．初見150分から180分で完結し，Ascension，Prestige，Stage Resetは使わない．

現在は，UnityでPrototype 0を開始できる段階にある．ただし，完成版の面白さは未証明であり，次の順で検証する．

1．撃つ，命中する，Reloadを待つ行為が気持ちよいか．  
2．約10回命中して最初の`＋1`を買う循環が理解できるか．  
3．MirrorとTarget配置で，直接射撃とは違う収益経路が生まれるか．  
4．GoldとMaterialの二経路が，作業ではなく配置と照準の判断になるか．  
5．五発Recipeの順番が，実際に次の照準を変えるか．

五発Recipeが照準を変えない場合は，追加機能で延命せず，Recipeを主軸から外して再設計する．

---

## 2．今回成熟させた判断

### 2.1．最初の10回

New GameではCollector一個を撃つ．一命中で1 Goldを得て，短い破壊と再生を見せる．約10回で10 Goldになり，最初の`Collector Value＋1`を購入できる．購入後は表示値が1から2へ変わる．

この導入は，Idle Gumball Machineの序盤で，約10 Goldの最初のGear価値上昇を購入し，盤面表示が1から2へ変わる構造を参照した．重要なのは正確な価格模倣ではなく，最初の反復回数，購入直後の倍化，盤面上の即時変化である．

### 2.2．Gold

Goldは現在の生産力を伸ばす通貨である．

- Collector Value．
- Gun Calibration．
- Reload Mechanism．
- Material Nodeの少額Gold．
- 同種Targetの追加個体．
- 有限解禁のGold部分．

再調査後，反復Upgradeを一つの指数式へ隠す案は廃止した．Collector Valueは10，30，75，180，450，1,100と明示列で増え，一回目は収益を二倍，その後は加算による逓減を作る．Gun Calibrationは最後のfloorで効果が消える`＋0.25`ではなく，500，6,500，80,000 Goldの少数回`×2`とする．Reloadは一段15％短縮とし，一購入の変化を読める大きさへした．

### 2.3．Material

Materialは数値を繰り返し上げる通貨ではなく，新しい盤面文法を開く有限Tokenである．

Material NodeをMaterialで買わせる循環は廃止した．Lifetime Gold 1,300へ到達した時，Material Nodeと専用Socketを無償支給する．Lifetime Gold 800で事前予告する．以後，NodeがReady発光している時に命中するとGauge＋1となり，10回で必ず1 Materialを得る．有効命中後は4秒Rechargeし，その間はCollectorを狙う．確率Dropは使わない．

有限解禁は原則としてGold＋Materialを要求する．Goldは現在の生産力，MaterialはMaterial経路を実際に扱ったことを示す．Materialだけを撃ち続けてもGold不足になり，Collectorだけを撃ち続けてもMaterial不足になるため，経路を切り替える理由が生まれる．旧仕様のCooldownなしでは約6.5秒に一個となり，解放が数分へ圧縮されたため，4秒Pulseによって理論上限を一個40秒へ直した．

### 2.4．Target購入

Target Typeを解禁した時，最初の一個を必ず付与する．「解禁権を買った後，実物をもう一度買う」という二重支払いは行わない．

同種追加個体はGoldだけで購入し，Materialを繰り返し要求しない．購入後は次の空Socketへ仮配置し，Board Editingを開く．配置変更，撤去，Recipe編集は無料である．

最初のMirrorはMaterial導入前に必要なため，Lifetime Gold 100で表示し，250 Goldだけで解禁する．Mirror経路は初回反射でRoute Multiplier 1.5を得る．これにより，Mirrorは見た目だけの配置物ではなく，難しい狙いへ報酬を返す最初の盤面投資になる．

追加Collectorは一発の最大Goldを増やさず，命中面積と別経路を増やす配置物である．購入Cardへこの役割を明記し，P1で20％以上の実収益差が出ない場合はPierce解放後まで販売を延期する．

### 2.5．Skill Tree

Skill Treeは枝分かれして見せるが，排他的選択にはしない．PierceとSplitは購入順を選べるが，最終的に両方取得できる．本作は短編であり，取り返しのつかないBuild失敗を作らない．

---

## 3．実装矛盾の解消

### 3.1．Reload

発射時にChamberを進める記述と，Reload完了時に次弾を送る記述が競合していた．現在は，発射時に次Indexだけを予約し，Reload完了時にChamberへ装填する状態機械へ統一した．Pending RecipeはSlot 4から0を装填する瞬間だけ反映する．

### 3.2．AmplifierとSplit

旧Event順では，子弾継承後にAmplifier効果を実行していたため，倍率を子弾へ渡せなかった．現在はTarget効果，Projectile効果，子弾継承の順に固定した．Amplifier倍率はProjectileごとのTokenであり，Split後は各子弾が継承して，次のGold報酬で個別に消費する．

### 3.3．Projectile上限

生成しなかった子弾の将来報酬を集約計算する案は廃止した．異なる方向へ飛ぶ未生成弾の命中先は予測できないためである．論理上限64は明示されたBalance制限とし，超過分は`LIMIT`表示して生成しない．描画上限はViewだけを省略し，Game Logicへ影響させない．

### 3.4．Save

Autosave時に飛翔中Projectileを消す解釈を廃止した．Save SnapshotはLive Runtimeを変更しない．Projectileと未完了Lineageを保存しないのはLoad境界だけであり，Load後はChamberを装填済みの安全状態へ戻す．

### 3.5．返金

30秒以内のGold Upgrade全額返金は廃止した．強化効果で稼いだ後に返金できるためである．購入前に現在値，購入後，価格を表示し，購入後は返金しない．

---

## 4．正本から外した案

次は現在の実装候補から除外した．過去資料に記述があっても，AIは実装しない．

- 爆発弾．
- Gold弾．
- Chaos Loader．
- Prism．
- Relay．
- Sweep Servo．
- Run内Perk．
- Ascension，Prestige，盤面Reset．
- Offline収益．
- 自動購入，自動配置，高度なAuto Aim．

通常，貫通，分裂，Collector，Mirror，Material Node，Amplifier，StoneだけでBuild差を検証する．不足が実測された場合だけ，正本を改訂して追加を検討する．

---

## 5．文書の役割

- `MASTER_GAME_SPECIFICATION_2026-08-28.md`：唯一の製品・Game Logic正本．
- `AI_PROTOTYPE0_IMPLEMENTATION_BRIEF_2026-08-28.md`：Prototype 0だけの実装範囲と受入条件．
- 本書：判断理由と変更要約．実装根拠にはしない．
- `economy_bootstrap_research_2026-08-28.md`：経済調査履歴．実装根拠にはしない．
- `incremental_design_research_report_2026-08-27.md`：類似作調査履歴．実装根拠にはしない．
- `incremental_economy_and_pacing_research_2026-08-28.md`：再調査，旧数値監査，0.3.0判断根拠．実装根拠にはしない．

AIへPrototype 0を依頼する時は，MasterとPrototype 0 Briefだけを渡す．調査報告，設計メモ，Chat Logを追加で渡さない．

---

## 6．次の作業

1．Unity 6000.3.18f1で2D Projectを作る．  
2．Git Repositoryを初期化し，Unity用`.gitignore`とText Serializationを設定する．  
3．Prototype 0 Briefの順でPure C# SimulationとUnity Viewを実装する．  
4．Windows x64 BuildをGitHub ReleasesへPre-releaseとして配布する．  
5．最初の5人で探索的Testを行い，射撃感と最初の10回循環を修正する．  
6．修正後，10人から12人でPrototype 0の判定を行う．

Prototype 0ではMaterial，Recipe，複数Target，Core Damageを実装しない．最初の射撃と最初の`＋1`が成立してから，Prototype 1へ進む．
