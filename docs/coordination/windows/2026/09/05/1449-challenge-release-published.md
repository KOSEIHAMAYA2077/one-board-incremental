---
handoff_id: windows-20260905-1449-challenge-release-published
host: windows
created_at: 2026-09-05T14:49:54+09:00
base_commit: b6727fe2eeff9321b83707fcacfc53b7c470a3a9
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-1439-challenge-arsenal-verification
---

# Challenge ArsenalのPush・Release保存完了

## 今回の目的

前回，安全確認で止まっていたGitHub送信をUserの明示許可に従って完了する。

## 読んだ正本

- AGENTS.md，coordination README／Handoff Template
- CHALLENGE_ARSENAL_RELEASE_2026-09-05.md，RELEASE_ARCHIVE.md
- 1439の実装検証記録。Game仕様変更はなく，Master §33を継続する。

## 実施内容

- 「非公開KOSEIHAMAYA2077/one-board-incrementalへコード・仕様・HandoffをPushし，Windows ZIPを新しい試作Releaseへ追加してよいか」という確認に，Userが「pushどうぞ」と承認した。
- Cleanな作業Tree，origin URL，非公開属性，Remote Branchの祖先関係，ローカルZIPのSHA-256を確認。
- feat/portrait-stageへ通常Push。mainへMerge／Pushしない。
- v0.5.0-challengeを独立Prereleaseとして作成し，検証済みZIPとSHAファイルを添付。
- 保存一覧とRelease説明の「許可待ち」を更新。過去Handoffは編集せず，この追記で前回の未送信状態を解消する。

## 得られた結果

- Release：https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.5.0-challenge
- TagのCommit：27be90f55280f82d2ed893d1260016def4afe96c。検証済みBuild sourceと一致。
- ZIP：OneBoardMomentumLab-v0.5.0-challenge-Windows-x64.zip，34,228,348 bytes。
- GitHub ZIP digest：sha256:cd8a6e14165f639b1b41c83e335e49a9828b845d3fb642d3144e43576f0bf30b。ローカル照合値と一致。
- ローカルZIP絶対Path：C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-143627-4569252\OneBoardMomentumLab-v0.5.0-challenge-Windows-x64.zip
- Releaseは計9件。旧8件のTagが残ることを確認。非公開設定を維持。

## 設計判断

### 確定

- 新版の外部送信はUser承認済みで，PushとRelease作成は成功。
- Game Code・Balance・Build内容を変更せず，既存の検証済み成果物をそのまま保存した。

### 提案

- なし。次は新版のPlay感想に基づいて変更範囲を選ぶ。

### 棄却または保留

- mainへの統合，旧Releaseの削除，作り直しBuildは行わない。

## 検証

- 実行Command：Git status／fetch／merge-base／push，Get-FileHash，gh repo view／release create／release view／release list／api git ref。
- 成功：指定先と非公開属性，Push，ZIP uploaded，SHA-256一致，Tag＝Build source，旧8 Release保持。
- Unityの再Compile・再Test・再Buildは未実行。既存の66 EditMode／25 PlayMode成功などは1439 Handoffの結果であり，今回再実行した結果ではない。
- 既知の問題：送信の阻害要因は解消。Gameの未検証範囲は1439 Handoffを継続。

## 変更File

- docs/CHALLENGE_ARSENAL_RELEASE_2026-09-05.md
- docs/RELEASE_ARCHIVE.md
- docs/coordination/windows/2026/09/05/1449-challenge-release-published.md（専用Commit）

## 次のHostへの依頼

1. feat/portrait-stageとv0.5.0-challengeを取得する。mainへ統合済みと仮定しない。
2. 1439 Handoffの「送信許可待ち」はこの追記で解消済みと扱う。
3. 旧版・旧Saveを保持する。

## User判断が必要な点

なし。
