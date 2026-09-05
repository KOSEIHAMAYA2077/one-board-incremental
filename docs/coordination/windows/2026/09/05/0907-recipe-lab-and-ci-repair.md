---
handoff_id: windows-20260905-0907-recipe-lab-and-ci-repair
host: windows
created_at: 2026-09-05T09:07:00+09:00
base_commit: 9a01bca83e8200a0d532bbf7dad141e73a8f9f3e
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-0821-free-placement-route-lab
  - mac-20260905-0457-windows-verification-response
---

# 五発Recipe試作とGitHub検査修正

## 今回の目的

Userは自由配置版を試遊してOKとし，反射パズルへの発展案のメモと次段階を依頼した．途中でGitHub失敗通知が届き「一旦それ修正してから弾5発版」と指示したため，検査修正・成功確認を先に行い，五発Recipeへ進んだ．

## 読んだ正本

- `AGENTS.md`
- `docs/MASTER_GAME_SPECIFICATION_2026-08-28.md`（v0.4.0→v0.5.0）
- `docs/AI_PROTOTYPE1A_IMPLEMENTATION_BRIEF_2026-09-05.md`
- `docs/AI_PROTOTYPE2_IMPLEMENTATION_BRIEF_2026-09-05.md`（今回追加）
- `docs/coordination/README.md`，Handoff Template，相手Hostの最新Handoff

## 実施内容

- 作業開始時の`feat/free-placement`はcleanでremoteと一致．fetch/pullで最新確認し，`feat/recipe-lab`を分岐．mainはP0受入済みのまま変更していない．
- 失敗Run 33929279060は`Confirm canonical specifications`で失敗．Masterは0.4.0なのに検査が0.3.0固定だった．GitHub実行Stepと該当CommitのFileで原因確認．生LogのDownloadは接続Timeoutだったため取得できていない．
- Commit `7d0c96c0db2889bae028e77b89b9a1c9bf93b9d6`でMasterの版番号書式・一意性・対応する変更履歴を確認する検査へ変更．P0のGame Version固定検査は維持．同Commitのみを自由配置Branchにもfast-forwardしPush．
- GitHub成功：Recipe Branch Run https://github.com/KOSEIHAMAYA2077/one-board-incremental/actions/runs/33930985113 ，元の自由配置Branch Run https://github.com/KOSEIHAMAYA2077/one-board-incremental/actions/runs/33931006596 ．旧失敗Runの履歴はそのまま残る．
- 五発循環，通常／貫通／分裂，Capacity 4，Primer，次周期予約，飛行弾を保持する編集Pause，Lineage共有Visitedと報酬合計を実装．
- 新しいRecipeLab SceneはUnity自身で生成．RecipeMode=falseの旧FreePlacement Sceneと旧P0を保持．新製品名で保存先を分離．
- Recipeと配置は日時付き履歴を残す．セッションLogはLocalのみ．Build等は実行ごとの新Directoryで保持．
- 反射パズル案を`docs/ideas/2026-09-05-reflection-puzzle.md`へ未採用メモとして保存．

## 得られた結果

確定ソースCommit `9a01bca83e8200a0d532bbf7dad141e73a8f9f3e`，Game Version `0.3.0-recipe`．

- Compile成功，EditMode 35/35，PlayMode 19/19成功（旧試作Testを含む）．
- 貫通でCollector二個を通過し5＋2 Gold，分裂で二個のCollectorへ4＋4 Gold，子弾間の重複報酬抑止，速度900継承，生成64上限，深度3，保存往復を確認．
- Windows x64 Build・ZIP・SHA-256成功．
- 最終Windows実行Fileの自動診断：exit 0，`applied=True; branches=4; paused=True; gold=4; active=0`．診断時はUserの保存Dataを読み書きしない．Standalone LogにException／Error／Warningなし．
- Boardのoffscreen描画で4本の分裂軌跡を目視確認．非表示Windowの通常Screenshotは表示内容を保証できないため，Recipe UIの実画面の目視確認済みとは扱わない．

### 成果物の絶対Path

```text
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090615-4789771\Windows\OneBoardRecipeLab.exe
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090615-4789771\OneBoardRecipeLab-v0.3.0-recipe-Windows-x64.zip
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090615-4789771\OneBoardRecipeLab-v0.3.0-recipe-Windows-x64.zip.sha256
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090615-4789771\EditMode.xml
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090615-4789771\PlayMode.xml
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090615-4789771\standalone.log
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090615-4789771\Preview\smoke.txt
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090615-4789771\Preview\01-split-board.png
```

ZIP SHA-256：`f0b882bae7289f2394150548ddec348665f0acbf24ec184546cc0a9bc221042a`

### 保持した旧試作と途中記録

```text
C:\Dev\one-board-incremental\Artifacts\Builds\Windows\OneBoardPrototype0.exe
C:\Dev\one-board-incremental\Artifacts\Placement\20260905-081928-4513824\Windows\OneBoardRouteLab.exe
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090017-1694159
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090143-0962360
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090503-7939862
```

最初のRecipe PlayMode失敗は，角度計算関数が正規化済み方向を返すため，子弾速度が900ではなく1になったこと．親速度を掛けて修正し，速度継承のTestを追加．FIXED変更やTest削除で回避していない．失敗XML/Logと途中Buildも保持．途中Buildは未Commit作業状態であり，配布対象は上記090615の確定Commit版のみ．

## 設計判断

### 確定

- 五Slotは連射や物理Magazineではなく，一発ずつ装填する循環規則．0.65秒Reloadを保持．
- Capacity 4，Cost通常0／貫通2／分裂3．初期Recipeは通常4＋分裂1．
- 子弾の詳細継承・上限到達時の終了はMaster v0.5.0のINITIALに明文化．
- Userの趣味制作／AI駆動制作を優先し，多人数評価を今回の条件にしない．

### 提案

- 反射パズルへの発展可能性．未採用であり，MasterのジャンルやStage構造を変更しない．類似作品調査も未実施．

### 棄却または保留

- Capacity 5の比較ボタン，購入待ち，Material，Core，Autoは今回入れない．
- 自由配置版を上書きせず，別Scene／実行Fileとして比較可能な状態を維持する．

## 検証

- 実行Command：検査のcanonical StepをGit Bashで実行，GitHub Run確認，`.\scripts\recipe.ps1 verify`，`.\scripts\recipe.ps1 test`，Standalone `-recipe-capture`診断，ZIP SHA-256再計算．
- 成功したTest：上記54件，Standalone診断，Windows Build，ZIPとHash一致．
- 未実行項目：Macでの新Recipe版検証，Userによる新Recipe UIの見やすさ／入力感の試遊．
- 既知問題・制限：抽象図形のSandboxであり購入進行は未実装．配置中の参考軌道は通常弾の中心射線で，分裂全経路の予測ではない．深度や生成上限時はLIMITとして明示終了する．Unity生成Scene/metaには標準生成の空欄後Whitespaceがあるため`git diff --check`で警告されたが，YAMLを手編集せず保持した．

## 変更File

- `.github/workflows/repository-sanity.yml`
- `Assets/_Project/Core/RecipeCycle.cs`，`RecipeLineage.cs`，`RoutingSimulation.cs`
- `Assets/_Project/Presentation/FreePlacementController.cs`，`FreePlacementController.Recipe.cs`，`BoardPieceView.cs`
- `Assets/_Project/Editor/RecipeLabSceneBuilder.cs`，`Assets/_Project/Scenes/RecipeLab.unity`，新規Assetのmeta
- `Assets/_Project/Tests/EditMode/RecipeTests.cs`，`Assets/_Project/Tests/PlayMode/RecipePlayModeTests.cs`
- `scripts/recipe.ps1`，`README.md`，`AGENTS.md`，Master，新Brief，反射パズル案メモ

## 次のHostへの依頼

1．継続時は`feat/recipe-lab`を取得し，Master v0.5.0とPrototype 2 Briefを読む．旧mainや自由配置Branchへ新機能を勝手に混ぜない．
2．必要ならMacでも新Sceneの検証を行う．Unityは6000.3.18f1固定．
3．Userの新Recipe試遊結果を待ち，次の遊びを決める．記録と旧Artifactは削除しない．

## User判断が必要な点

- 実装完了に必要な未決事項はなし．次の試遊では`R`で並び，`B`で配置，左クリックで射撃．貫通用にCollectorを直列，分裂用にAmplifierの先へ広げて置くと比較できる．
