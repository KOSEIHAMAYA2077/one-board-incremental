# One Board Incremental

> **試作してみた結果、思い描いていたものと違ったため、いったん開発を止めています。（2026年9月6日）**
>
> AI駆動でゲームを作ってみることを目的に、射撃・反射・分裂・強化などを試作しました。遊んで面白い部分もありましたが、当初作りたかった遊びとの違いを感じ、この形での開発はいったん区切ることにしました。
> 完成品ではなく、試作コード・アセット・提案や判断の変遷を残すための公開リポジトリです。過去の試作も消さずに保存しています。再開や製品化は未定です。

## 休止時点の最新試作：MOMENTUM LAB / 0.6.1-drag

中央3:4の結晶風盤面で、一マガジンの射撃・反射・加速・分裂を試すUnityプロトタイプです。Revolver・UZI・Shotgun・Sniperの4銃、容量20ptの付け替え効果、3StageとGold強化、B缶を取得した弾でバンパーに当てる換金を実装しています。子弾はB缶の効果を継承しません。

マウス照準とクリックで一マガジンを発射。発射中も照準を変更できます。`R`／ホイールで銃切替、`Q`／`E`でキーボード照準、`Space`で発射。左上のヘルプ・メニューに操作説明や設定があります。Reload完了かつ全弾の速度が300以下になると再射撃可能。0.6.1では狙う意味を取り戻すため、命中時と飛行中の減速を強化しました。

**このブランチには0.6.1までのコードを保存済みですが、[配布済みWindows実行版](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases)は0.5.0までです。** 最新版のBuild・ZIPはWindowsローカルに保存しており、Releaseには未掲載です。mainは初期Prototype 0と案内を保持しています。

Scene：`Assets/_Project/Scenes/MomentumLab.unity`。Unity **6000.3.18f1**で `.\scripts\momentum.ps1 verify` を実行すると検証・Windows Build・ZIP・SHA-256を生成します。成果物は `Artifacts/Momentum/<日時>/`。現行試作の仕様はMaster §33（最新調整は§33.11）とMomentum Briefを参照してください。

360度の採掘・回収・持ち帰りや自動生産への展開は、**未実装の構想メモであり、今後の開発予定ではありません**。[最後の調整とブレインストーミング記録](docs/coordination/windows/2026/09/06/1925-stronger-drag-and-extraction-brainstorm.md)と[試作保存記録](docs/RELEASE_ARCHIVE.md)も残しています。以下は過去の試作説明です。

## 保存版：RECIPE LAB / Prototype 2

五発Recipe（通常・貫通・分裂）と自由配置．`R`で弾の並び，`B`で配置，左クリックで一発．Cost合計4以内で編集する．直前の通常弾が特殊弾を最大2段階強化する．変更は次周期から（初回発射前だけ即時）．貫通はCollectorを直列に，分裂はAmplifierの先へ扇状に置くと違いを試せる．

Scene `Assets/_Project/Scenes/RecipeLab.unity`．検証・Build `.\scripts\recipe.ps1 verify`．出力は`Artifacts/Recipe/<日時>/Windows/OneBoardRecipeLab.exe`．旧版のBuildと保存データには触れない．対象範囲は`docs/AI_PROTOTYPE2_IMPLEMENTATION_BRIEF_2026-09-05.md`．反射パズル案は`docs/ideas/2026-09-05-reflection-puzzle.md`へ未採用メモとして保持．

## 保存版：ROUTE LAB / Prototype 1A

自由配置，Mirror回転，Collector二個とAmplifierを試せるWindows版．`B`で配置編集，ドラッグで移動，Mirror選択後`Q / E`またはホイールで回転，再び`B`で再開し左クリックで発射する．配置は自動保存され，日時付き履歴も残る．

Unity Scene：`Assets/_Project/Scenes/FreePlacement.unity`．検証・Build：`.\scripts\placement.ps1 verify`．成果物は`Artifacts/Placement/<実行日時>/`へ毎回新規保存する．今回の範囲は`docs/AI_PROTOTYPE1A_IMPLEMENTATION_BRIEF_2026-09-05.md`を参照．

旧Prototype 0のScene，Build手順，過去成果物は引き続き保持する．以下はPrototype 0の説明．

一つの盤面へTargetを配置し，五発Recipeと一発ごとの照準で収益経路を作り，奥のCoreを破壊する短編能動型インクリメンタルゲームである．現在はPrototype 0として，射撃感と最初の10 Hit→10 Gold→`Collector Value＋1`だけを検証している．

## 固定環境

- Unity 6000.3.18f1．
- Windows 10／11 x64を第一対象とする．
- Prototype 0 Game Versionは`0.1.0-prototype0`．
- Build配布はGitHub ReleasesのPre-releaseを使う．

## 検証

macOS：

```bash
./scripts/unity.sh verify
```

Windows PowerShell：

```powershell
.\scripts\unity.ps1 verify
```

Scene存在確認とCompile，EditMode Test，PlayMode Test，Windows x64 Buildを順番に行い，結果を`Artifacts/`へ出す．Sceneがない場合だけUnity自身が生成する．詳しくは`docs/DEVELOPMENT_WORKFLOW.md`を参照する．

## AIへ渡すファイル

Prototype 0を実装するAIには，次の二ファイルだけを渡す．

1．`MASTER_GAME_SPECIFICATION_2026-08-28.md`  
2．`AI_PROTOTYPE0_IMPLEMENTATION_BRIEF_2026-08-28.md`

Masterが唯一のGame Logic正本である．Prototype Briefは今回の実装範囲だけを限定する．競合時はMasterを優先する．

## 判断履歴

次は調査と判断理由を保存する履歴文書であり，実装仕様ではない．

- `DESIGN_MEMO_2026-08-28.md`
- `history/economy_bootstrap_research_2026-08-28.md`
- `history/incremental_design_research_report_2026-08-27.md`
- `history/incremental_economy_and_pacing_research_2026-08-28.md`
- `sources/`以下の同期資料

AIは，明示的な調査・設計監査を依頼された場合を除き，これらから機能や数値を実装へ取り込まない．

## Mac・Windows間の引継ぎ

端末間の作業結果は`docs/coordination/`のHost別Handoffで共有する．作業開始前にGitHubから最新化し，相手Hostの最新Handoffを読む．Handoffは参考資料であり，MasterやPrototype Briefを上書きしない．運用方法は`docs/coordination/README.md`を参照する．

## 現在の範囲

Prototype 0に入れるもの：

- Mouse照準と距離連動の散布Cone．
- Seed付き単発射撃，0.65秒Reload．
- Collector一個，壁反射，Gold，Lifetime Gold．
- 10 Hit表示と10 Goldの最初の`＋1`．
- Local Log，EditMode／PlayMode Test，Windows x64 Build．

Material，複数Target，配置編集，五発Recipe，特殊弾，Auto，Core DamageはPrototype 0へ先回りして入れない．
