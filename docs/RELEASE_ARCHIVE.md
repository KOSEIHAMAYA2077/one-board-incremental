# 試作Release保存一覧

## 2026-09-06追記：休止時点の体験版

ブラウザ版も[GitHub Pages](https://koseihamaya2077.github.io/one-board-incremental/)で試遊可能。[v0.6.1-web](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.6.1-web)はWeb配置用ZIPの保存版。Source `6c4eabb6c027a251777e4f78cc27891e38e9abf1`、SHA-256 `ab38767612667bf2c69300cd4b17af3c7c79e2d9b6d85078bc73cfc56a70fe68`。[Web制約と検証](WEB_DEMO.md)。Windows用ZIPとは異なる。

リポジトリは公開済み。**[0.6.1-drag Windows版](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.6.1-drag)**を追加保存。ZIPを展開してexeを起動するだけで試遊可能。Build source `56bfa5d4d3805113bef6e89719a7067bf17da375`、SHA-256 `7236cb91f26cfa8d3df4ba6b48e9e389bc927caaf7768292f006651ecb6b72cb`。[起動・操作・検証](MOMENTUM_DRAG_RELEASE_2026-09-06.md)。旧版はすべて保持。以下は公開前を含む当時の保存記録。

2026-09-05時点。すべて開発中のPrereleaseであり、正式製品版ではない。リポジトリは非公開のまま。

[全Release](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases) / [最新の保存済みRelease：Challenge Arsenal](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.5.0-challenge)

追加保存：0.5.0-challenge（3:4盤面・連鎖分裂・2銃・有限マガジンChallenge・恒久成長）。Build source `27be90f55280f82d2ed893d1260016def4afe96c`。SHA-256 `cd8a6e14165f639b1b41c83e335e49a9828b845d3fb642d3144e43576f0bf30b`。[操作・検証記録](CHALLENGE_ARSENAL_RELEASE_2026-09-05.md)。Userの明示承認後，2026-09-05にZIPとSHAファイルを追加し，GitHubのZIP digestとTagのSource一致を確認した。旧8版と旧Saveを保持する。

追加保存：0.4.2-portrait（三列UI・中央9:16）、Build source `c5b138a9c978f06e06058cc85fedd6d94cf005e0`。ZIPは`OneBoardMomentumLab-v0.4.2-portrait-Windows-x64.zip`、SHA-256は`67765c29917ded503a1015a00345e3f29dbbdfea8b6f49541e4734cca27b23f0`。[操作・検証記録](PORTRAIT_STAGE_RELEASE_2026-09-05.md)。旧横長版も保持する。

追加保存：0.4.1-neon（表示比較版）、Build source `b17fed20df60d3820536ba35d6cc387d1631b71e`。ZIPは`OneBoardMomentumLab-v0.4.1-neon-Windows-x64.zip`、SHA-256は`2b31f82eeefde898c47b378712a7688f398bfbdf9c42beeefb9e0ddf1e8cbfd3`。操作・検証は[新版記録](NEON_CRYSTAL_RELEASE_2026-09-05.md)。以下の6版も引き続き保持する。

| 試作 | 保存タグ | Build source |
| --- | --- | --- |
| 初期Prototype 0（既存・変更なし） | [v0.1.0-prototype0](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.1.0-prototype0) | `5d832eb1e4b1497e8b8cf022e662b7f6f0b0df77` |
| Prototype 0 — Windows反射修正版（保存版） | [prototype0-windows-reflection-20260905](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/prototype0-windows-reflection-20260905) | `3f56d5cc65140ca1e689eb688fdc142cfaba2694` |
| Route Lab — 自由配置版 | [v0.2.0-placement](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.2.0-placement) | `08d857cc716221bfbb854a736674c4a2e6195b61` |
| Recipe Lab — 五発Recipe初版 | [v0.3.0-recipe](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.3.0-recipe) | `9a01bca83e8200a0d532bbf7dad141e73a8f9f3e` |
| Recipe Lab — FHD文字修正版 | [v0.3.1-recipe](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.3.1-recipe) | `e7edfbadea320ec1a6d4d9e099ae1bbd6bf8ceec` |
| Momentum Lab — 速度・弾倉版 | [v0.4.0-momentum](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.4.0-momentum) | `9347ba9c500936b0a4db777620d21c98b99d36d5` |

## 取得方法

各ReleaseのAssetsにあるOneBoardで始まるWindows-x64.zipを全体展開し、exeを実行する。DataフォルダーとDLLを同じ場所に保持する。GitHubが自動表示するSource codeのZIPはUnityソースであり実行版ではない。

各版にZIP、SHA-256ファイル、説明・検証要約を添付した。今回追加した5件は既存ZIPを再ビルドせず保存し、GitHub側のZIP digestとローカルのSHA-256一致、およびタグとBuild source一致を確認した。

## 保存時の注意

- Prototype 0反射修正版はゲーム内番号が0.1.0-prototype0のまま。元のReleaseを上書きせず、日付付きの別タグで区別した。
- 0.3.0-recipeには文字が大きく切れる既知問題がある。文字修正版は0.3.1-recipe。
- 0.3.0と0.3.1は保存領域を共有する。Momentum Labは別製品名・別保存領域。
- 旧版ソース・ローカル成果物・失敗時ログは削除していない。今回のRelease対象は主要な成功Buildであり、全中間Buildのアップロードではない。
- 今回はWindows試作の履歴保存。Unity再検証・Mac検証・正式リリース候補の認定は実施していない。
- ReleaseはPRではない。開発Branchをmainへ統合する操作は行っていない。

## ZIP照合値

### prototype0-windows-reflection-20260905

- File: `OneBoardPrototype0-v0.1.0-prototype0-Windows-x64.zip`
- SHA-256: `a5b239e56eb6a114142016d514c7bdc47ec76b1f41de1ab1dc9915b3e302f492`

### v0.2.0-placement

- File: `OneBoardRouteLab-v0.2.0-placement-Windows-x64.zip`
- SHA-256: `0bc60052709221e6f2d915af2040db086a09d5d60d8585b874dc1a148a8781cd`

### v0.3.0-recipe

- File: `OneBoardRecipeLab-v0.3.0-recipe-Windows-x64.zip`
- SHA-256: `f0b882bae7289f2394150548ddec348665f0acbf24ec184546cc0a9bc221042a`

### v0.3.1-recipe

- File: `OneBoardRecipeLab-v0.3.1-recipe-Windows-x64.zip`
- SHA-256: `b0bf38f15e553e115b8ef6e65c3e10f71833d6ec34d33dabbc49e11c05b56a3b`

### v0.4.0-momentum

- File: `OneBoardMomentumLab-v0.4.0-momentum-Windows-x64.zip`
- SHA-256: `7bbb0e3a08654c214ec303c43c76eda885c1f254d08220c59c55ca1a4d917b4c`
