# One Board Incremental

> **試作してみた結果、思い描いていたものと違ったため、いったん開発を止めています。（2026年9月6日）**
>
> AI駆動でゲームを作ってみることを目的に、射撃・反射・分裂・強化などを試作しました。遊んで面白い部分もありましたが、当初作りたかった遊びとの違いを感じ、この形での開発はいったん区切ることにしました。
> 完成品ではなく、試作コード・アセット・提案や判断の変遷を残すための公開リポジトリです。過去の試作も消さずに保存しています。再開や製品化は未定です。

## インストールせずに遊ぶ

### [▶ ブラウザで試作を遊ぶ](https://koseihamaya2077.github.io/one-board-incremental/)

**PCで開いて「遊ぶ」をクリックするだけ。** マウスで狙い、左クリックで発射。音が出ます。初回は約21 MBを読み込みます。進行はこのブラウザに保存され、Windows版とは別です。スマートフォン向け操作は未対応。重い場合は下のWindows版をどうぞ。

既存の個人サイト本体は変更せず、同じGitHub Pagesドメインのゲーム専用URLで公開しています。

## ダウンロードしてすぐ遊ぶ（Windows）

### [▶ Windows体験版 0.6.1 をダウンロード（ZIP・約35 MB）](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/download/v0.6.1-drag/OneBoardMomentumLab-v0.6.1-drag-Windows-x64.zip)

**ZIPを「すべて展開」→ `OneBoardMomentumLab.exe` をダブルクリック → 盤面を狙って左クリック。** Unityのインストールやビルドは不要です。Windows 10／11・64bit向け。DataフォルダーやDLLも一緒に展開してください。

`R`／ホイールで銃を切替。発射中も狙いを動かせます。左上の`?`で遊び方、`≡`で音量・表示設定、`Esc`で終了確認。[詳しい操作と検証](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.6.1-drag)。未署名の試作なのでWindowsに警告される場合があります。保護機能は無効にせず、配布元を確認してください。

## 開発記録・過去版を見る

- [旧Windows実行版：Challenge Arsenal 0.5.0](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.5.0-challenge)
- **[休止時点の最新コード：0.6.1-drag / feat/portrait-stage](https://github.com/KOSEIHAMAYA2077/one-board-incremental/tree/feat/portrait-stage)**
- [過去の試作も含むダウンロード一覧](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases)
- [試作の保存記録](https://github.com/KOSEIHAMAYA2077/one-board-incremental/blob/feat/portrait-stage/docs/RELEASE_ARCHIVE.md)

Releaseページの `Source code (zip)` は開発用ソースであり、実行版ではありません。遊ぶ場合は上のWindows体験版を選んでください。

## なぜトップの更新日時が古かったのか

トップで表示する `main` のゲームコードはPrototype 0を保持しています。その後の開発は別ブランチで進めており，mainへはまだ統合していません。最新の試作を見る場合は，上の開発ブランチまたはReleaseを開いてください。このREADMEは入口の案内であり，mainに最新ゲームコードが入ったという意味ではありません。

2026-09-06の休止時点の保存状況：

| 場所 | 内容 |
| --- | --- |
| `main` | Prototype 0のコードと，この案内 |
| `feat/portrait-stage` | `0.6.1-drag`までPush済み：4銃・結晶アセット・操作メニュー・B缶・減速調整など |
| GitHub Releases | `0.6.1-drag`のWindows実行版を公開。旧試作も保持 |
| Windowsローカル | 検証済みBuild・ZIP・検証ログを保持 |

0.5.1以降のコードも作業ブランチに保存済みです。過去の試作・Tag・Build・保存記録は保持し、mainへのゲームコード統合は行っていません。

## 配布済み0.5.0の内容：Momentum / Challenge Arsenal

- 明るいネオン結晶の盤面。中央3:4ステージ，左にDPS・戦闘ログ，右に銃と強化。
- 一クリックで一マガジン。Revolverは6発，UZIは18発。
- 弾の速度が威力と貫通の持続を決める。壁・バンパーで反射し，動くゾーンで加速する。
- 容量20ptの中で威力・初速・抵抗軽減・分裂などを組み合わせる。
- 初期3マガジンで12個の的に挑戦。Goldで強化し，全破壊で次Stageへ。全3Stage。
- 1マガジンで全破壊すると特殊効果を恒久開放。獲得Gold・所有強化・構成をローカル保存。

操作：盤面クリックで発射，`Space`で残弾回収，`R`でPause，`F2`で表示比較。配布中の0.5.0は全弾終了後に再射撃し，クリア後は左のStageボタンで進みます。0.5.1／0.5.2の改善とは区別してください。

## 休止時点の最新試作：0.6.1-drag

- Revolver・UZI・Shotgun・Sniperの4銃。マガジン発射中も照準を変更可能。
- 半透明の結晶風アセット、反射・加速・分裂、効果容量20pt、3StageとGold強化。
- B缶を取得した弾でバンパーに当てると換金。子弾へは引き継がない。
- マウス照準とクリック発射、`R`／ホイールで銃切替。`Q`／`E`でキーボード照準、`Space`で発射。左上のヘルプ・メニューで操作説明や設定を確認。
- 狙う意味を取り戻すため、命中時の速度損失と飛行中の減速を強化。Reload完了かつ全弾の速度が300以下になると再射撃可能。

360度の採掘・回収・持ち帰りや、自動生産へ広げる案も話し合いましたが、**未実装の構想メモであり、今後の開発予定ではありません**。[最後の調整とブレインストーミング記録](https://github.com/KOSEIHAMAYA2077/one-board-incremental/blob/feat/portrait-stage/docs/coordination/windows/2026/09/06/1925-stronger-drag-and-extraction-brainstorm.md)に残しています。

## 開発を再開する場合

Unity **6000.3.18f1**，Windows 10／11 x64が対象です。開発ブランチの `AGENTS.md` を読み，そのブランチの[Master仕様](https://github.com/KOSEIHAMAYA2077/one-board-incremental/blob/feat/portrait-stage/docs/MASTER_GAME_SPECIFICATION_2026-08-28.md)と[Momentum Brief](https://github.com/KOSEIHAMAYA2077/one-board-incremental/blob/feat/portrait-stage/docs/AI_MOMENTUM_LAB_IMPLEMENTATION_BRIEF_2026-09-05.md)を参照します。現在の試作はMaster §33。Handoffや過去案は仕様正本を上書きしません。

開発ブランチ上の検証：

```powershell
.\scripts\momentum.ps1 verify
```

Compile，EditMode／PlayMode Test，Windows x64 Build，ZIP，SHA-256を生成します。出力は `Artifacts/Momentum/<日時>/`。mainのPrototype 0にはこのScriptがないため，ブランチを混同しないでください。[開発ブランチのHandoff一覧](https://github.com/KOSEIHAMAYA2077/one-board-incremental/tree/feat/portrait-stage/docs/coordination/windows)も参照できます。

---

## 保存用：mainのPrototype 0説明

以下は初期試作の説明です。現在のChallenge Arsenalの範囲や操作ではありません。Prototype 0を再現する場合に使います。

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
