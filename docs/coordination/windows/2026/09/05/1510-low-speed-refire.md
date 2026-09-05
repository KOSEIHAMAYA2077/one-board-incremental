---
handoff_id: windows-20260905-1510-low-speed-refire
host: windows
created_at: 2026-09-05T15:10:08+09:00
base_commit: ad6ede4d52d7847671dade86f0c569385d383e96
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-1449-challenge-release-published
  - mac-20260905-0445-cross-host-sync-and-gumball
---

# 低速の残弾を待たずに次のマガジンを発射

## 今回の目的

User試遊の「もう一回打てないのやっぱストレスだな．最後滑るし．速度が○○いかなら次打てる，ってしたほうがストレスない．」を反映する。

## 読んだ正本

- AGENTS.md，Master §33，Momentum Implementation Brief。
- coordination README，HANDOFF_TEMPLATE，直前Windows記録とMac同期Handoff。旧Mac提案を実装根拠へ昇格していない。

## 実施内容

- Cleanなfeat/portrait-stageで開始。fetch後，main/origin/mainと作業Branch/originがともに差分0。既存のmain非変更方針を守り，切替・Mergeはせず作業Branchを継続。
- Master v0.7.1 §33.5，BriefとAGENTSを更新。Game Version 0.5.1-tempo。
- Reload完了・射出中でない・全生存弾の最大速度300以下で再射撃。子弾も含み，平均速度では判定しない。
- 残弾は攻撃継続。新しい斉射が前の弾の10秒期限・1024生成予算をResetしないよう，生成数をマガジン別に管理。同時256は新旧合算，未射出弾も予約する。
- 全的撃破後は無駄に次マガジンを消費させず，従来通り攻撃解決／回収後にClearを確定。構成変更も従来通り全弾終了後。
- HUDへ閾値とREADY／減速待ち／Reload／最終マガジン／上限の区別を追加。
- 独立Source Commit：34522ebca6e64de9860d0466f42eec09bbcf7808 (`fix: allow refire while slow bullets remain`)。

## 得られた結果

- Compile成功，EditMode 74/74，PlayMode 25/25，Windows x64 Build・ZIP・SHA-256成功。
- 可視診断Exit0，1920×1080。6発，加速4回，Pause成功，終了時残弾0，文字Overflow0。画像で追加説明が読めることを確認。
- 成果物Root：C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-150913-2348824
- EXE：C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-150913-2348824\Windows\OneBoardMomentumLab.exe
- ZIP：C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-150913-2348824\OneBoardMomentumLab-v0.5.1-tempo-Windows-x64.zip
- SHA File：C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-150913-2348824\OneBoardMomentumLab-v0.5.1-tempo-Windows-x64.zip.sha256
- SHA-256：4a048106e45952ab6335eaa577d032cf5f77caefbdaaee809eed6003a93a1693
- 診断画像：C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-150913-2348824\VisibleSmoke\02-full-ui.png
- 旧Build・Tag・Save・過去Handoffは保持。診断は保存無効で実行。
- 今回はLocal CommitとBuildまで。GitHubへの新PatchのPush／Tag／Release追加は未実行。既存v0.5.0-challengeの公開記録を新版の送信済み記録と混同しない。

## 設計判断

### 確定

- 全弾待機を速度条件へ変更することはUser指示。300は今回選定したINITIALで，Userが指定した確定数値ではない。
- 時間減速120/s，消滅80を維持。追加衝突／加速がない場合，速度300から80までの約1.83秒分を待たずに撃てる計算。
- 再加速で速度300を超えれば次回発射判定は再び待機になるが，既に受理した射出は止めない。

### 提案

- まずこの閾値で試遊し，まだ遅ければ300のみを段階調整する。銃の速度や威力を同時に変えない。

### 棄却または保留

- 残弾の強制消滅，残弾の寿命延長，無制限連射，経済の変更は行わない。
- 今回は手動の操作感の合格を推定しない。最大密度の実描画性能も再評価していない。

## 検証

- 実行Command：git status/fetch/diff --check，.\scripts\momentum.ps1 test（2回），.\scripts\momentum.ps1 verify，Windows EXE -momentum-capture，Get-FileHash。
- 追加Test：300境界，1個の高速子弾で待機，Reloadと非予約クリック，旧弾継続と独立期限，マガジン別1024生成境界，Revolver/UZIの256枠予約，再加速，全的破壊後の無駄撃ち防止，Controllerの低速残弾中発射とUI誤射防止。
- 最初のPlayMode追加Testのみ失敗：Pause解除直後の既存誤射防止フレーム中に再発射していた。Test側で2描画Frameを待つ修正後，全Test成功。ゲーム側の誤射防止は変更していない。
- 失敗時Logも保存：C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-150704-1727688
- 未実行項目：Mac，Userによる閾値300の操作感評価，長時間Play，最大256弾の可視性能。新規の分裂ストレス描画は未実行（Coreの上限Testは実行済み）。
- 既知問題：全的撃破後や最後のマガジンの決着はまだ全弾終了を待つ。Space回収で即確定可。効果付け替えも全弾終了まで不可。これらは今回維持した仕様。

## 変更File

- AGENTS.md
- Assets/_Project/Core/MomentumSimulation.cs，MomentumChallenge.cs
- Assets/_Project/Presentation/MomentumPortraitHud.cs，MomentumLabController.cs
- Assets/_Project/Tests/EditMode/MomentumChallengeTests.cs
- Assets/_Project/Tests/PlayMode/MomentumPlayModeTests.cs
- docs/MASTER_GAME_SPECIFICATION_2026-08-28.md，AI_MOMENTUM_LAB_IMPLEMENTATION_BRIEF_2026-09-05.md
- scripts/momentum.ps1
- 本Handoff（専用Commit）

## 次のHostへの依頼

1. GitHub版はまだ0.5.0。Windows側の新Patchの送信後，feat/portrait-stageのSource 34522eb以降を取得する。
2. Master §33.5を優先し，全弾待機へ戻さない。旧Build・記録・Saveとmainを保持する。

## User判断が必要な点

- 閾値300で再射撃のストレスが減るか。数値は再調整可能。
