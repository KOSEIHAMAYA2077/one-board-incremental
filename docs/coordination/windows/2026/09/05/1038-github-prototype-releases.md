---
handoff_id: windows-20260905-1038-github-prototype-releases
host: windows
created_at: 2026-09-05T10:38:11+09:00
base_commit: b2542bd6df9016eaad4e0618fecbe062c5a059fc
authority: non-authoritative-handoff
status: complete
consumed_handoffs: []
---

# GitHubへの主要試作Release保存

## 今回の目的

User承認により、主要試作を版ごとにGitHubから取得できるようにし、過去の試作を保持する。

## 読んだ正本

- AGENTS.md
- docs/coordination/README.md
- docs/coordination/HANDOFF_TEMPLATE.md
- ゲーム仕様の変更なし。各版のBuild成果物と検証記録を保存元として確認。

## 実施内容

- 新規タグ付きPrerelease 5件を作成。ZIP、SHA-256ファイル、説明・検証要約を添付。
- ドラフト段階でGitHubのZIP digestとローカルSHA-256、target commitを照合してから確定。
- 既存v0.1.0-prototype0のRelease・タグは変更していない。
- docs/RELEASE_ARCHIVE.mdにリンクとCommit、照合値を整理。

## 得られた結果

- 全6件がdraft=false、prerelease=true。private設定は維持。
- 新規5タグが各ZIPのBuild sourceに一致。
- ローカル成果物の絶対Path：
  - `C:\Dev\one-board-incremental\Artifacts\Packages\OneBoardPrototype0-v0.1.0-prototype0-Windows-x64.zip`
  - `C:\Dev\one-board-incremental\Artifacts\Placement\20260905-081928-4513824\OneBoardRouteLab-v0.2.0-placement-Windows-x64.zip`
  - `C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-090615-4789771\OneBoardRecipeLab-v0.3.0-recipe-Windows-x64.zip`
  - `C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-092323-0906230\OneBoardRecipeLab-v0.3.1-recipe-Windows-x64.zip`
  - `C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-102602-8036405\OneBoardMomentumLab-v0.4.0-momentum-Windows-x64.zip`
- リンク・SHA-256はdocs/RELEASE_ARCHIVE.mdを参照。

## 設計判断

### 確定

- ゲームのGame Logicは変更していない。
- Userの「旧試作を消さない」希望に従い、既存成果物は保持。

### 提案

- 今後の主要な遊べる版も新しいタグとReleaseで追加し、過去Assetを上書きしない。

### 棄却または保留

- mainへの統合、PR作成、一般公開化は今回行わない。
- 旧P0のバージョン書換えや再Buildはせず、日付付き保存タグで区別。

## 検証

- 実行したCommand：git status、git fetch origin、git ls-remote --tags origin、Get-FileHash、gh release create/view/edit/list、gh repo view。
- 成功したTest：今回はファイル同一性とタグ・Release状態の確認。Unity Testの再実行ではない。
- 当時の成功記録：P0 Edit18/Play9、Placement Edit26/Play14、Recipe0.3.0 Edit35/Play19、Recipe0.3.1 Edit35/Play22、Momentum Edit49/Play23。いずれも当時のCompileとWindows Build成功。
- 未実行項目：今回のUnity再検証、Mac検証、新たな手動Playtest。
- 既知問題：Recipe0.3.0の文字切れを保存。Recipe0.3.1/Momentumの画面全体の目視確認は未完了。正式製品の検証完了を意味しない。

## 変更File

- docs/RELEASE_ARCHIVE.md（別Commit）
- docs/coordination/windows/2026/09/05/1038-github-prototype-releases.md

## 次のHostへの依頼

1．保存版の取得にはRelease一覧を利用する。古いPrototypeの仕様を最新試作へ復活させない。
2．既存タグ・Release・ローカルBuildを削除または上書きしない。

## User判断が必要な点

- なし

