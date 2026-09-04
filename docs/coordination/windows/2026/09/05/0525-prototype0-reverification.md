---
handoff_id: windows-20260905-0525-prototype0-reverification
host: windows
created_at: 2026-09-05T05:25:43+09:00
base_commit: 03e02f77ca72a0d5ffacec89ffa8767c9394352f
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - mac-20260905-0457-windows-verification-response
---

# Prototype 0 Windowsネイティブ再検証

## 今回の目的

恒久的なWindows Local Drive上のCloneと正本指定のUnity環境を用意し，Prototype 0のCompile，EditMode Test，PlayMode Test，Windows x64 Build，ZIP，SHA-256を再検証する。

## 読んだ正本

- `AGENTS.md`
- `docs/MASTER_GAME_SPECIFICATION_2026-08-28.md`
- `docs/AI_PROTOTYPE0_IMPLEMENTATION_BRIEF_2026-08-28.md`
- `docs/coordination/README.md`
- `docs/coordination/mac/2026/09/05/0457-windows-verification-response.md`（参考資料）

## 実施内容

- GitHubの最新`main`を`C:\Dev\one-board-incremental`へ新規Cloneした。
- Unity Editor `6000.3.18f1`（changeset `5ebeb53e4c07`）とWindows Build Support（IL2CPP）をUnity Hub経由で導入した。
- `git status --short --branch`で新しいCloneがcleanであり，`main`と`origin/main`が一致することを確認した。
- WindowsネイティブPowerShellで`./scripts/unity.ps1 verify`を実行した。
- 最初の実行で，UnityのCompile自体は終了コード0で完了したが，PowerShellがGUI形式の`Unity.exe`を待たず，`$LASTEXITCODE`が空値のままHarnessだけが失敗する問題を確認した。
- `scripts/unity.ps1`を，`Start-Process -Wait -PassThru`でUnity終了を待ち，Processの終了コードを評価する実装へ最小修正した。
- Harness修正Commit `03e02f77ca72a0d5ffacec89ffa8767c9394352f`で`./scripts/unity.ps1 verify`を再実行した。
- Master，Prototype 0 Brief，Game Code，`FIXED`値，Prototype 0の実装範囲は変更していない。

## 得られた結果

- Unity：`6000.3.18f1 (5ebeb53e4c07)`
- Compile：成功。
- EditMode Test：成功，18/18 passed，0 failed。
- PlayMode Test：成功，8/8 passed，0 failed。
- Windows x64 Build：成功，Executableは667136 bytes。
- ZIP：成功，34177953 bytes。
- SHA-256 File：生成成功。
- ZIP再計算SHA-256：`0952ae219b548ff4a444f36ac49bd3361d97cc73cea23f48f2a7af1495d3ad18`。生成されたSHA-256 Fileと一致した。

## 設計判断

### 確定

- Prototype 0はUnity `6000.3.18f1`とGame Version `0.1.0-prototype0`に固定する。
- Mac側Handoffの設計提案はPrototype 0へ取り込まない。

### 提案

- なし。

### 棄却または保留

- Unity `6000.5.6f1`による代替検証は行っていない。
- Active Lineage上限などGumball由来の設計提案は，Prototype 0の実装入力へ昇格していないため保留した。

## 検証

- 実行したCommand：`git status --short --branch`，Unity Hub Editor／Module導入，`./scripts/unity.ps1 verify`，`Get-FileHash -Algorithm SHA256`。
- 成功したTest：Compile，EditMode 18/18，PlayMode 8/8，Windows x64 Build，ZIP，SHA-256一致確認。
- 未実行項目：生成したWindows Buildの手動Playtest。今回の標準`verify`範囲外である。
- 既知問題：Game Code，Test，Buildには今回の検証で既知問題なし。元のHarnessはWindows上でUnity Processを待たなかったが，Commit `03e02f7`で修正済み。

## 生成物

- Windows x64 Build：`C:\Dev\one-board-incremental\Artifacts\Builds\Windows\OneBoardPrototype0.exe`
- ZIP：`C:\Dev\one-board-incremental\Artifacts\Packages\OneBoardPrototype0-v0.1.0-prototype0-Windows-x64.zip`
- SHA-256：`C:\Dev\one-board-incremental\Artifacts\Packages\OneBoardPrototype0-v0.1.0-prototype0-Windows-x64.zip.sha256`
- Test Summary：`C:\Dev\one-board-incremental\Artifacts\Packages\prototype0-test-summary.md`
- EditMode Result：`C:\Dev\one-board-incremental\Artifacts\TestResults\editmode.xml`
- PlayMode Result：`C:\Dev\one-board-incremental\Artifacts\TestResults\playmode.xml`
- Unity Logs：`C:\Dev\one-board-incremental\Artifacts\Logs`

## 変更File

- `scripts/unity.ps1`
- `docs/coordination/windows/2026/09/05/0525-prototype0-reverification.md`

## 次のHostへの依頼

1. Mac側でHarness修正CommitとこのHandoffをPullし，Windows検証完了を受領する。
2. `Artifacts/`はGit管理外のWindowsローカル生成物として扱い，RepositoryへCommitしない。
3. Prototype 0の仕様変更またはGumball由来提案の実装は，Userの次の明示判断まで行わない。

## User判断が必要な点

- なし。Prototype 0のWindows標準検証は完了した。
