---
handoff_id: windows-20260905-0452-prototype0-verification
host: windows
created_at: 2026-09-05T04:52:33+09:00
base_commit: c6898db41d43ee72f524b53ccc0cb3a22ca137ee
authority: non-authoritative-handoff
status: needs-attention
consumed_handoffs:
  - mac-20260905-0445-cross-host-sync-and-gumball
---

# Prototype 0 Windowsネイティブ検証

## 今回の目的

GitHubの`main`をWindowsへ取得し，Unity `6000.3.18f1`でPrototype 0のCompile，EditMode Test，PlayMode Test，Windows x64 Build，ZIP，SHA-256を検証する。

## 読んだ正本

- `AGENTS.md`
- `docs/MASTER_GAME_SPECIFICATION_2026-08-28.md`
- `docs/AI_PROTOTYPE0_IMPLEMENTATION_BRIEF_2026-08-28.md`
- `docs/coordination/README.md`
- `docs/coordination/mac/2026/09/05/0445-cross-host-sync-and-gumball.md`（参考資料）

## 実施内容

- Windowsローカル作業ツリーとしてGitHubの`main`を取得した。
- `git status --short --branch`で未Commit変更がないことを確認し，`git pull --ff-only`を実行した。結果は`Already up to date.`。
- `main`の検証基準Commitは`c6898db41d43ee72f524b53ccc0cb3a22ca137ee`である。
- WindowsネイティブPowerShellで`./scripts/unity.ps1 verify`を実行した。
- Mac側Handoffの提案事項は，MasterまたはPrototype 0 Briefへ昇格していないため実装へ取り込んでいない。

## 得られた結果

- 検証スクリプトは最初のUnity Editor検出で停止した。
- `C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe`が存在しない。
- `UNITY_EDITOR`環境変数は未設定である。
- Unity Hubの標準Editor Directoryには`6000.5.6f1`のみが存在した。別Patchを代替使用していない。
- Compile以前に停止したため，EditMode Test，PlayMode Test，Windows x64 Build，ZIP，SHA-256は未実行である。

## 設計判断

### 確定

- Prototype 0はUnity `6000.3.18f1`とGame Version `0.1.0-prototype0`に固定される。
- MasterとPrototype 0 Briefの範囲，および`FIXED`値は変更していない。

### 提案

- なし。

### 棄却または保留

- インストール済みのUnity `6000.5.6f1`での代替Buildは，別PatchでProjectを保存しない規則に反するため実行しなかった。
- Unity `6000.3.18f1`を利用可能にした後，変更なしの同一Commitで`./scripts/unity.ps1 verify`を再実行する。

## 検証

- 実行したCommand：`git status --short --branch`，`git pull --ff-only`，`./scripts/unity.ps1 verify`
- 成功したTest：Git同期のみ。UnityのCompile／EditMode／PlayMode／Buildは未実行。
- 未実行項目：Compile，EditMode Test，PlayMode Test，Windows x64 Build，ZIP，SHA-256。
- 既知問題：必須のUnity `6000.3.18f1`が標準配置に未インストールであり，`UNITY_EDITOR`も未設定のため，検証を開始できない。

## 生成物

- 生成物なし。Unity検出前に停止したため，`C:\Users\kouha\.codex\.chatgpt-projects\g-p-6a8def0b52588191a95a0f2d37925f19\one-board-incremental\Artifacts`は作成されていない。
- 未生成の想定Build Path：`C:\Users\kouha\.codex\.chatgpt-projects\g-p-6a8def0b52588191a95a0f2d37925f19\one-board-incremental\Artifacts\Builds\Windows\OneBoardPrototype0.exe`
- 未生成の想定ZIP Path：`C:\Users\kouha\.codex\.chatgpt-projects\g-p-6a8def0b52588191a95a0f2d37925f19\one-board-incremental\Artifacts\Packages\OneBoardPrototype0-v0.1.0-prototype0-Windows-x64.zip`
- 未生成の想定SHA-256 Path：`C:\Users\kouha\.codex\.chatgpt-projects\g-p-6a8def0b52588191a95a0f2d37925f19\one-board-incremental\Artifacts\Packages\OneBoardPrototype0-v0.1.0-prototype0-Windows-x64.zip.sha256`

## 変更File

- `docs/coordination/windows/2026/09/05/0452-prototype0-verification.md`

## 次のHostへの依頼

1. Mac側への作業依頼はない。Windows側でUnity `6000.3.18f1`を利用可能にした後，同一Commitの再検証結果を確認する。
2. Master，Prototype 0 Brief，`FIXED`値，およびPrototype 0の実装範囲を変更してこの失敗を回避しない。

## User判断が必要な点

- Windows環境へUnity Editor `6000.3.18f1`（Windows Build Supportを含む）を導入または利用可能な場所を指定した後，再検証する。
