---
handoff_id: windows-20260905-0821-free-placement-route-lab
host: windows
created_at: 2026-09-05T08:21:00+09:00
base_commit: 08d857cc716221bfbb854a736674c4a2e6195b61
authority: non-authoritative-handoff
status: needs-attention
consumed_handoffs:
  - windows-20260905-0642-prototype0-reflection-playtest-pass
---

# 自由配置の次試作 ROUTE LAB / Prototype 1A

## 今回の目的

Userが承認した「盤面内の自由配置・グリッド吸着・重なり禁止・Mirror回転」を実装し，置き直してすぐ撃てる次のWindows試作を作る．

Userの目的は，好きなジャンルをAI駆動で制作する体験と，将来的なポートフォリオである．正式Releaseを想定した多人数による面白さ検証を内部試作の必須条件にしない．動作確認と必要な自動Testは続ける．

## 読んだ正本

- AGENTS.md，Master v0.3.0（承認内容を反映してv0.4.0へ更新）
- Prototype 0 Brief，DEVELOPMENT_WORKFLOW.md
- docs/coordination/README.md，前回Windows Handoff

## 実施内容

- 専用Branch `feat/free-placement`で作業．mainは確認済みPrototype 0の`f344616`に保持した．
- Master §5.4を自由配置へ改訂し，次の実装範囲をPrototype 1A Briefに定義した．旧Socket個数・価格などは後続で整理する旧候補であり，今回へ実装しない．
- FreePlacement.unityと専用Controller，純粋C#の配置判定・射撃経路処理を追加．Prototype0.unityと旧Controllerは変更していない．
- Collector二個，Mirror一個，Amplifier一個を支給した試作開始状態．Collector基礎値は初回Upgrade済み相当の2．
- 20pxグリッド吸着，円・回転矩形の重なり判定，盤面・UI・銃領域への侵入禁止，Mirrorの5度回転，不正配置の取消を実装．
- 編集中は停止し，既発射弾を終了．再開操作による誤射を防ぐ．編集時の中心射線予測，Mirror法線，軌跡，命中Feedbackを追加．
- Collectorの表示FeedbackではColliderも命中受付も停止させない．旧版の0.35秒中の命中拒否を新しい試作へ持ち込んでいない．
- 配置はLocalに保存・復元．毎回日時付き配置履歴を保存し，Session Logも実行別Fileとした．
- 新しい検証Scriptは実行ごとに固有Artifact Directoryを作成．旧Prototype 0のBuild・ZIP・Logsを上書きしていない．

## 得られた結果

- Compile成功，EditMode 26/26，PlayMode 14/14．旧Prototype 0のTestを含む．
- 実Colliderを通して初期配置のAmplifier→Collectorで4 Gold，斜め反射＋増幅で6 Goldを確認．
- 円と回転矩形の重なり，回転時の盤面外はみ出し，編集Pause，再開時の誤射防止を確認．
- 最終Windows実行Fileを自動起動し，Gold=4，有効な配置変更成功，不正な重なり拒否，編集状態を確認して正常終了した．
- OffscreenのCamera描画を画像で確認した．UIを含む通常Windowの見た目・実Mouse操作の手動確認は未実施．

## 設計判断

### 確定

- 固定Socketから自由配置への変更はUser承認済みで，Masterへ反映した．
- Prototype 0を保持し，新しい試作は0.2.0-placementとして別Scene・別実行Fileへ追加する．
- 過去の試作，失敗した検証，Build，Handoffを消さず積み上げる．

### 提案

- Userは自由に配置して遊び，気になった点を伝える．数値評価や多人数評価を今回の試遊条件にしない．

### 棄却または保留

- Material，Recipe，貫通，分裂，Core，Auto，購入経済は今回へ入れていない．
- 製品としての初期値や解禁価格は後続で検討する．今回の支給状態は実験用．

## 検証

- 実行Command：`.\scripts\placement.ps1 test`，`.\scripts\placement.ps1 verify`
- Windows自動起動：`OneBoardRouteLab.exe -force-d3d11 -placement-capture <新規Preview Directory>`．診断起動ではUserの配置保存・Session Logへ書き込まない．
- 最終検証Directory：`C:\Dev\one-board-incremental\Artifacts\Placement\20260905-081928-4513824`
- 実行File：`C:\Dev\one-board-incremental\Artifacts\Placement\20260905-081928-4513824\Windows\OneBoardRouteLab.exe`
- ZIP：`C:\Dev\one-board-incremental\Artifacts\Placement\20260905-081928-4513824\OneBoardRouteLab-v0.2.0-placement-Windows-x64.zip`
- SHA-256：`0bc60052709221e6f2d915af2040db086a09d5d60d8585b874dc1a148a8781cd`
- 成功Test：EditMode 26/26，PlayMode 14/14，Windows Build，自動起動診断．
- 未実行：Mac上のUnity検証，通常WindowのUI目視確認と実Mouseによる手動試遊．
- 既知制約：非表示WindowのScreenCaptureはD3D12では失敗し，D3D11でも黒画像となった．盤面はCameraから直接RenderTextureへ描画して確認した．通常起動の画面が黒いと判定したわけではない．UIを含む見た目を確認済みとはしない．
- 実装中の失敗：画面保存Module未登録によるCompile Errorを，Unity同梱Screen Capture Module追加で解消．旧Start-Process -Wait式が補助Processを待ち続けたため，新ScriptはUnity本体をProcess.WaitForExitで待つ．その実行記録もArtifacts/Placementの別Directoryへ保持．

## 変更File

- Core：FreePlacementBoard.cs，RoutingSimulation.cs
- Presentation：BoardPieceView.cs，FreePlacementController.cs
- Editor：FreePlacementSceneBuilder.cs，Prototype0Build.cs（Build Metadata書込を再利用可能にした）
- FreePlacement.unity，各.meta，EditMode/PlayModeの新規Test
- scripts/placement.ps1，Packagesの同梱画面保存Module
- Master，Prototype 1A Brief，AGENTS.md，README.md，本Handoff

## 次のHostへの依頼

1．`feat/free-placement`を取得して本HandoffとPrototype 1A Briefを読む．mainにはまだ統合していない．
2．Userの試遊を受けて調整する．過去の試作，検証成果物，Handoffを削除しない．
3．Macで同じSourceを並行変更する場合はBranchを分ける．

## User判断が必要な点

- 起動して自由に配置・回転・射撃を試し，触りにくい点や次に作りたいことを伝える．
