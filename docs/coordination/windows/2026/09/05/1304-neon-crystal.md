---
handoff_id: windows-20260905-1304-neon-crystal
host: windows
created_at: 2026-09-05T13:04:01+09:00
base_commit: b17fed20df60d3820536ba35d6cc387d1631b71e
authority: non-authoritative-handoff
status: complete
consumed_handoffs: [mac-20260905-0445-cross-host-sync-and-gumball]
---

# ネオン結晶表示比較版

## 今回の目的

UserがMomentumのゾーンと音を評価し、暗い盤面・ネオン輪郭・ほぼ真上からの3D結晶表示を試すよう依頼。既存挙動と音を維持し、旧版も残す。

## 読んだ正本

- AGENTS.md
- docs/MASTER_GAME_SPECIFICATION_2026-08-28.md
- docs/AI_MOMENTUM_LAB_IMPLEMENTATION_BRIEF_2026-09-05.md
- docs/DEVELOPMENT_WORKFLOW.md
- docs/coordination/README.md / HANDOFF_TEMPLATE.md

## 実施内容

- Master文書0.6.1 §32.1とBriefへ表示仕様を追加。Game Version 0.4.1-neon。
- 新規の生成Mesh、頂点色Shader、発光Shaderを追加。購入Assetや画像Assetは不使用。
- 結晶の面取り、光の縁、弾の尾、加速リング、命中Flashと最大96破片。
- F2で旧表示と切替可能。Core、Audio、Scene YAML、Save schemaは変更なし。
- feat/neon-crystalへCommitとPush。mainへの統合やPR作成は行っていない。
- GitHubへv0.4.1-neonをPrereleaseとして保存。旧6版は保持。

## 得られた結果

- Release: https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.4.1-neon
- Source/Tag: b17fed20df60d3820536ba35d6cc387d1631b71e
- 成果物Root: C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-130142-4734274
- EXE: C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-130142-4734274\Windows\OneBoardMomentumLab.exe
- ZIP: C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-130142-4734274\OneBoardMomentumLab-v0.4.1-neon-Windows-x64.zip
- SHA-256: 2b31f82eeefde898c47b378712a7688f398bfbdf9c42beeefb9e0ddf1e8cbfd3。GitHubのAsset digestと一致、タグがBuild sourceに一致。
- 盤面画像: 成果物Root下Smoke/01-flight-board.png。
- computer-useスキルで実行版を起動し、起動ウィンドウ画像から文字・結晶・飛行中の尾を確認。Userの操作が検出されたため入力を追加せず、ゲームを開いたまま渡した。

## 設計判断

### 確定

- User承認の表示比較であり、Game Logicは従来どおり。
- 視点の整合を守るためCameraは変更せず、Meshを3度傾ける。
- 装飾多角形と当たり判定の差を細い円形輪郭で示す。

### 提案

- 次の見た目調整はUserが実際に遊んだ感触を受けて決める。

### 棄却または保留

- 現実的な銃モデル、外部Asset購入、屈折や本格Bloom、大規模Render Pipeline変更は今回不要。

## 検証

- 実行したCommand: .\\scripts\\momentum.ps1 verify、Standalone -momentum-capture、git diff、Get-FileHash、gh release view、git ls-remote。
- EditMode 49/49、PlayMode 24/24、Compile、Windows x64 Build成功。
- 新規Testは3D奥行き、Colliderなし、F2の時間/Gold不変、表示弾数対応、全弾/破片終了。
- Standalone FHD exit0、fired3、boosts3、pausedTrue、remaining0、gold3。Playing/Editor文字枠はみ出し0。
- 初回失敗: MaterialPropertyBlockをMonoBehaviourフィールド初期化で作ったためUnity初期化例外。Initialize内へ移し、全verifyを再実行して成功。
- 初回失敗LogはArtifacts/Momentum/20260905-130054-4900565へ保持。
- 未実行: Mac検証、長時間負荷検証、音の新たな聴感比較（Audioコードは不変）。
- 既知問題: 多重命中のFloating Number重なり。非表示起動の全画面Captureは黒（盤面RenderTextureと実ウィンドウで確認）。側面は浅く、透明屈折なし。色覚/演出軽減の詳細設定未整備。

## 変更File

- AGENTS.md / Master / Momentum Brief
- Assets/_Project/Presentation/MomentumLabController.cs
- Assets/_Project/Presentation/MomentumNeonView.cs とmeta
- Assets/Resources/MomentumCrystal.shader / MomentumGlow.shader とmeta
- Assets/_Project/Tests/PlayMode/MomentumPlayModeTests.cs
- scripts/momentum.ps1
- docs/NEON_CRYSTAL_RELEASE_2026-09-05.md / RELEASE_ARCHIVE.md
- このHandoff（専用Commit）

## 次のHostへの依頼

1．最新表示はv0.4.1-neon。比較はF2、過去版はReleaseから取得する。
2．旧Build・失敗Log・Tagを削除／上書きせず、ゲーム挙動と見た目の変更を区別する。

## User判断が必要な点

- 見た目の方向性の感想。作業を進めるための必須判断はなし。

