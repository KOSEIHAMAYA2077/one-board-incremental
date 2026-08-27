# Unity開発・検証・配布手順

## 1．固定環境

| 項目 | 値 |
|---|---|
| Unity | 6000.3.18f1 |
| Prototype 0 Game Version | 0.1.0-prototype0 |
| macOS Unity Path | `/Applications/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity` |
| Windows Unity Path | `C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe` |
| 第一Build | Windows 10／11 x64 |
| 試作配布 | GitHub Releases Pre-release |

別のUnity PatchでProjectを保存しない．既定Path以外へ入れた場合だけ`UNITY_EDITOR`へUnity実行Fileの完全Pathを指定する．

## 2．AIへ渡すもの

実装AIへ渡す仕様は次だけである．

1．`docs/MASTER_GAME_SPECIFICATION_2026-08-28.md`  
2．対象Prototype Brief．Prototype 0では`docs/AI_PROTOTYPE0_IMPLEMENTATION_BRIEF_2026-08-28.md`

`docs/history/`は判断根拠であり，実装入力へ混ぜない．

## 3．一操作検証

macOS：

```bash
./scripts/unity.sh verify
```

Windows PowerShell：

```powershell
.\scripts\unity.ps1 verify
```

順番は固定である．

```text
Scene生成
→ EditMode Test
→ PlayMode Test
→ Windows x64 Build
→ ZIPとSHA-256生成
```

結果は`Artifacts/`へ保存し，GitへCommitしない．途中で失敗した場合は非0で終了する．

個別実行は`generate`，`compile`，`test-edit`，`test-play`，`build-mac`，`build-windows`，`package-windows`を使う．

## 4．SceneとPrefab

Prototype 0 Sceneは`Incremental Game/Generate Prototype 0 Scene`またはHarnessからUnity自身に生成させる．Scene YAMLを手書きしない．Scene生成Code，Core Test，Presentation Testを同じ変更へ含める．

## 5．Git手順

一機能を一Branchにする．

```text
Issue
→ feat／fix／test／docs Branch
→ 実装
→ verify
→ Pull Request
→ main
→ Prototype Tag
→ GitHub Pre-release
```

Commit前に，`Assets`，`Packages`，`ProjectSettings`，`.meta`が含まれ，`Library`，`Temp`，`Logs`，`Artifacts`，`Builds`が含まれないことを確認する．

## 6．Version規則

- 文書Version：Masterの設計改訂．例`0.3.0`．
- Game Version：BuildとTag．Prototype 0は`0.1.0-prototype0`．
- Schema Version：Save Data構造．Prototype 0は`1`．
- Commit Hash：Build元を特定する．

公開済みTagを動かさない．修正版はPatchを上げる．

## 7．GitHub Release

Prototype 0 Release Assetは次とする．

```text
OneBoardPrototype0-v0.1.0-prototype0-Windows-x64.zip
OneBoardPrototype0-v0.1.0-prototype0-Windows-x64.zip.sha256
prototype0-test-summary.md
```

Release Notesには，検証目的，操作，変更，既知問題，Test結果，Save互換，Commit Hashを書く．Source Code Archiveを遊ぶための配布物として案内しない．

## 8．失敗時の読み方

1．`Artifacts/Logs/compile.log`または該当工程Logの最初のErrorを見る．  
2．Compile Errorを直すまで，後続Test Failureを個別修正しない．  
3．Test XMLとConsole Logを照合する．  
4．修正後は失敗工程だけでなく`verify`全体を再実行する．  
5．未実行工程を成功扱いにしない．
