# One Board Incremental

銃から弾を一斉に撃ち出し，反射・加速・貫通・分裂の連鎖と強化を楽しむ，AI駆動のゲーム制作プロジェクト。Unityで小さな試作を積み重ねている段階です。正式製品版ではありません。

## まず遊ぶ・開発を見る

- **[Windows実行版：Challenge Arsenal 0.5.0](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.5.0-challenge)**
- **[開発中のコード：feat/portrait-stage](https://github.com/KOSEIHAMAYA2077/one-board-incremental/tree/feat/portrait-stage)**
- [過去の試作も含むダウンロード一覧](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases)
- [試作の保存記録](https://github.com/KOSEIHAMAYA2077/one-board-incremental/blob/feat/portrait-stage/docs/RELEASE_ARCHIVE.md)

Releaseページの **Assets** から `OneBoardMomentumLab-v0.5.0-challenge-Windows-x64.zip` をダウンロードし，全体を展開して `OneBoardMomentumLab.exe` を起動してください。DataフォルダーとDLLは一緒に置きます。`Source code (zip)` は実行版ではありません。非公開リポジトリのため，アクセス権のあるGitHubアカウントでのログインが必要です。

## なぜトップの更新日時が古かったのか

トップで表示する `main` のゲームコードはPrototype 0を保持しています。その後の開発は別ブランチで進めており，mainへはまだ統合していません。最新の試作を見る場合は，上の開発ブランチまたはReleaseを開いてください。このREADMEは入口の案内であり，mainに最新ゲームコードが入ったという意味ではありません。

2026-09-05時点の保存状況：

| 場所 | 内容 |
| --- | --- |
| `main` | Prototype 0のコードと，この案内 |
| GitHubの開発ブランチ／Release | `0.5.0-challenge`：2銃・効果・3Stage・恒久強化 |
| Windowsローカルのみ・未Push | `0.5.1-tempo`：残弾速度300以下で再射撃。`0.5.2-clear`：中央にCLEAR／次へ／もう一度 |

**0.5.1／0.5.2はまだGitHubから取得できません。** コードとBuildはWindowsに保存済みですが，このREADME更新では送信していません。旧試作・Tag・Build・保存記録は残します。

## 現在のゲーム：Momentum / Challenge Arsenal

- 明るいネオン結晶の盤面。中央3:4ステージ，左にDPS・戦闘ログ，右に銃と強化。
- 一クリックで一マガジン。Revolverは6発，UZIは18発。
- 弾の速度が威力と貫通の持続を決める。壁・バンパーで反射し，動くゾーンで加速する。
- 容量20ptの中で威力・初速・抵抗軽減・分裂などを組み合わせる。
- 初期3マガジンで12個の的に挑戦。Goldで強化し，全破壊で次Stageへ。全3Stage。
- 1マガジンで全破壊すると特殊効果を恒久開放。獲得Gold・所有強化・構成をローカル保存。

操作：盤面クリックで発射，`Space`で残弾回収，`R`でPause，`F2`で表示比較。配布中の0.5.0は全弾終了後に再射撃し，クリア後は左のStageボタンで進みます。0.5.1／0.5.2の改善とは区別してください。

次の候補は敵の特色・強化，Stage固有素材，周回難度と追加報酬。まだ仕様確定・実装済みではありません。

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
