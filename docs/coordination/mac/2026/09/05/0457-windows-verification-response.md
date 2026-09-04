---
handoff_id: mac-20260905-0457-windows-verification-response
host: mac
created_at: 2026-09-05T04:57:21+09:00
base_commit: 5dafdcbef51fa163c57c0bc2baf89989e7f6e138
authority: non-authoritative-handoff
status: needs-attention
consumed_handoffs:
  - windows-20260905-0452-prototype0-verification
---

# Windows検証結果の受領と作業場所の是正提案

## 今回の目的

Windows側のPrototype 0検証HandoffをMac側で受領し，現在の阻害要因と次の安全な手順をWindowsへ返す．

## 読んだ正本とHandoff

- `AGENTS.md`
- `docs/coordination/README.md`
- `docs/coordination/windows/2026/09/05/0452-prototype0-verification.md`

## 受領結果

- Mac側はWindowsのCommit `5dafdcbef51fa163c57c0bc2baf89989e7f6e138`を取得した．GitHubを介したHandoff同期は正常に機能している．
- Windows側のGit同期とPowerShell検証Script起動までは成功している．
- 必須のUnity Editor `6000.3.18f1`が標準Pathに存在せず，`UNITY_EDITOR`も未設定だったため，Unity起動前に停止した．
- Windowsには`6000.5.6f1`のみ存在した．別Patchを代替利用しなかった判断は，`AGENTS.md`の固定環境規則に従っており正しい．
- Compile，EditMode Test，PlayMode Test，Windows x64 Build，ZIP，SHA-256は未実行である．現時点ではGame CodeまたはHarnessの不具合は観測されていない．
- Prototype 0，Master，Prototype Brief，`FIXED`値に変更はない．

## 追加で確認した運用上の注意

Windows側の現在のCloneは次のChatGPT管理領域内にある．

```text
C:\Users\kouha\.codex\.chatgpt-projects\g-p-6a8def0b52588191a95a0f2d37925f19\one-board-incremental
```

GitHubへPush済みの追跡Fileはremoteに保存されるが，この管理領域を恒久的なUnity作業場所として扱うことは推奨しない．再同期やProject管理処理の影響を受けた場合，Git管理外の`Library`，`Artifacts`，Local Logなどを失い，再Importや再Buildが必要になる可能性がある．

Unityを本格的に開く前に，次のようなWindows Local Drive上の通常Directoryへ新しくCloneする．

```text
C:\Dev\one-board-incremental
```

新しいCloneでGit状態とUnity検証を確認するまでは，既存Cloneを削除しない．ChatGPT Desktop側では新しいDirectoryをLocal Projectへ追加し，主Folderとして設定する．

## Windows側の次の手順

1．`C:\Dev`などの恒久Directoryへprivate repositoryをCloneする．
2．新しいCloneで`git status --short --branch`を実行し，`main`と`origin/main`が一致していることを確認する．
3．Unity HubでEditor `6000.3.18f1`と必要なWindows Build Supportを追加する．`6000.5.6f1`は削除せず共存させてよい．
4．新しいCloneのPowerShellで`.\scripts\unity.ps1 verify`を実行する．
5．Unityを標準Path以外へ入れた場合だけ，現在のPowerShell Sessionで次のように指定してから再実行する．

```powershell
$env:UNITY_EDITOR = "D:\Unity\6000.3.18f1\Editor\Unity.exe"
.\scripts\unity.ps1 verify
```

6．成功時も失敗時も，新しいWindows Handoffへ実行結果，最初の失敗原因，生成物Pathを記録し，Commit，Pushする．

想定Handoff Pathは次である．

```text
docs/coordination/windows/2026/09/05/HHmm-prototype0-reverification.md
```

## 設計判断

### 確定

- Unity `6000.3.18f1`固定とPrototype 0境界は変更しない．
- 現在の失敗を回避するためにMaster，Prototype Brief，`FIXED`値を変更しない．

### 提案

- Windowsの恒久的なUnity作業Cloneを`C:\Dev\one-board-incremental`へ置く．
- ChatGPT管理領域内のCloneは，新しいCloneが正常に動くまで保持し，確認後に別途整理する．

### 保留

- Active Lineage上限を含むGumball由来の設計提案は，Prototype 0のWindows実機検証が完了するまで変更しない．

## 検証

- 実行したCommand：Mac側`git pull --ff-only origin main`，Git log，Git status，Windows Handoff読取．
- 成功したTest：GitHub経由のMac・Windows Handoff同期．
- 未実行項目：Unity Compile，EditMode，PlayMode，Windows x64 Build．
- 既知問題：WindowsへUnity `6000.3.18f1`が未導入である．WindowsのUnity作業CloneがChatGPT管理領域内にある．

## 変更File

- `docs/coordination/mac/2026/09/05/0457-windows-verification-response.md`

## Windowsへの依頼

1．恒久DirectoryへのCloneとUnity `6000.3.18f1`導入を行う．
2．新しいCloneから検証Scriptを再実行する．
3．結果を新しいWindows Handoffへ記録してPushする．
4．Prototype 0の仕様またはCodeは，環境検証が終わるまで変更しない．

## User判断が必要な点

- なし．現在は環境を正本指定へ合わせる通常の検証準備として進められる．
