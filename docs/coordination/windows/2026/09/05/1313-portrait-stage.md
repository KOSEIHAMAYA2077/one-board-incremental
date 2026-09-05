---
handoff_id: windows-20260905-1313-portrait-stage
host: windows
created_at: 2026-09-05T13:13:06+09:00
base_commit: c5b138a9c978f06e06058cc85fedd6d94cf005e0
authority: non-authoritative-handoff
status: complete
consumed_handoffs: []
---

# 縦長ステージと左右パネル

## 今回の目的

User指定の「ログ(DPS・威力) | 縦9:16ステージ | 強化要素」の配置を実装する。旧横長版は保存する。

## 読んだ正本

- AGENTS.md
- docs/MASTER_GAME_SPECIFICATION_2026-08-28.md §32〜32.2
- docs/AI_MOMENTUM_LAB_IMPLEMENTATION_BRIEF_2026-09-05.md
- 継続作業のcoordination README / HANDOFF_TEMPLATE規則

## 実施内容

- Master0.6.2 §32.2を追加、Game0.4.2-portrait。
- 中央の論理Stageを450×800（9:16）、左右にログと弾倉UIを配置。
- MomentumBoardLayoutを追加。旧LandscapeをSimulation既定値として保持し、新ControllerだけPortraitを使用。
- 壁、Gun、Zone軌道、Target配置候補をPortrait対応。弾やTargetの円形半径・Balance値・音は変更なし。
- 実HP減少の5秒DPSと累計、弾ごとの直近威力、直近7件のEventを追加。OverkillをDPSへ含めない。
- 右側に既存弾倉編集と「未実装」と明示した強化領域。購入/強化Logicは追加していない。
- 新HUDはPartial Classへ分離し、不要になった横長HUDを置換。過去のHUDは旧Tag/Buildで復元可能。
- feat/portrait-stageへPush。mainへはMergeしていない。

## 得られた結果

- SourceとTag: c5b138a9c978f06e06058cc85fedd6d94cf005e0
- Release: https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.4.2-portrait
- 成果物Root: C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-131128-7227709
- EXE: C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-131128-7227709\Windows\OneBoardMomentumLab.exe
- ZIP: C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-131128-7227709\OneBoardMomentumLab-v0.4.2-portrait-Windows-x64.zip
- SHA-256: 67765c29917ded503a1015a00345e3f29dbbdfea8b6f49541e4734cca27b23f0。GitHubのAsset digestと一致、TagもBuild source一致。
- 前回までの7 Releaseとローカルの旧試作を保持。
- computer-useスキルで実ウィンドウを確認。中央9:16、左右パネル、更新中のDPS/ログ/HPを観察した。Userが遊び始めている画面だったので追加の入力操作をせず、開いたまま渡した。

## 設計判断

### 確定

- User承認の縦長盤面。単なる描画伸縮ではなくGeometryを変更。
- DPSは直近5秒の実HP減少÷5。起動直後も分母5、Pause中の窓は停止。
- F2はネオン/旧図形の切替であり、縦長/横長の切替ではない。横長は旧Releaseから取得する。

### 提案

- 右の購入強化は次のゲーム設計判断後に追加する。

### 棄却または保留

- 新経済、価格やUpgrade効果、銃種は今回の配置変更へ混ぜない。

## 検証

- 実行Command: .\\scripts\\momentum.ps1 verify、Standalone -momentum-capture、Get-FileHash、gh release view、git ls-remote。
- Compile/Windows x64 Build/ZIP成功。EditMode54/54、PlayMode24/24。
- 100Seedの全7Target初期配置・重なりなし・再現性、縦長の壁反射、左右UI発射拒否、DPSのOverkill除外/5秒窓/編集停止を追加確認。
- FHD Standalone exit0、fired3、boosts3、pausedTrue、remaining0、gold6。Playing/Editor文字枠はみ出し0。診断Player LogにError/Exception/Warningなし。
- 盤面RenderTextureはSmoke/01-flight-board.png。全体は実ウィンドウ画像で確認。
- 未実行: Mac、長時間負荷、右Slotの手動切替（Pause/編集と誤射防止は自動Test）。
- 既知問題: 縦長化で反射頻度/DPSが旧横長と変わる。集中時の短いPopupの近接は残る。強化購入は未実装。

## 変更File

- AGENTS.md、Master、Momentum Brief、scripts/momentum.ps1
- Core: MomentumBoardLayout、MomentumCombatStats、MomentumSimulation
- Presentation: MomentumLabController、MomentumPortraitHud、MomentumNeonView
- MomentumのEditMode/PlayMode Test、新規CSのmeta
- docs/PORTRAIT_STAGE_RELEASE_2026-09-05.md、RELEASE_ARCHIVE.md
- このHandoff（専用Commit）

## 次のHostへの依頼

1．最新はv0.4.2-portrait。旧横長と比べる場合は0.4.1-neonを別起動する。
2．旧試作・Tag・Buildを削除せず、UIだけの変更と盤面Geometry変更を区別する。

## User判断が必要な点

- なし。遊んだ感想から次の強化要素を検討する。

