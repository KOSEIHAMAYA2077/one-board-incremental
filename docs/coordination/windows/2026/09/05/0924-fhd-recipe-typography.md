---
handoff_id: windows-20260905-0924-fhd-recipe-typography
host: windows
created_at: 2026-09-05T09:24:00+09:00
base_commit: e7edfbadea320ec1a6d4d9e099ae1bbd6bf8ceec
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-0907-recipe-lab-and-ci-repair
---

# FHD基準の文字サイズ・上下欠け修正

## 今回の目的

User提供ScreenshotでNOW／弾名／Footerが上下に切れていた．「文字がでかすぎ，FHD前提で整えて」と依頼された．Game Logicは変更せず表示だけを修正した．

## 読んだ正本

- AGENTS.md，Master §17と変更履歴，Prototype 2 Brief
- coordination README，Mac最新Handoff，Handoff Template

## 実施内容

- Game Version 0.3.1-recipe．Master表示基準を0.5.1へ追記．
- Recipe UIのみ本文21→16，補助18→13，見出し28→21，Gold56→38，Button22→16（論理単位）．FHDでは本文19px，補助16px，見出し25pxになる．
- 日本語の行高に対する枠不足を解消．Labelの継承Paddingを除き，Headerを余裕のある三行へ整理，Footerを52論理px高に拡大．
- 盤面の1600×900論理座標は維持．文字・Buttonは画面Pixelの矩形・整数Font Sizeへ変換し，GUI.matrixの拡大だけで文字を伸ばさない．入力判定も同じPixel矩形を使う．
- Windows初期Windowサイズを1920×1080に設定．Build終了時は元のProject画面設定へ戻す．P0とP1Aの文字Styleは変更しない．
- Capacity表示を使用量／4へ整理．五発の規則，配置，当たり判定，報酬，Save Schemaは変更なし．
- User報告画像と途中Buildも残した．旧版の実行Fileを上書きしていない．

## 得られた結果

- `.\scripts\recipe.ps1 verify`：Compile，EditMode 35/35，PlayMode 22/22，Windows x64 Build，ZIP，SHA-256成功．
- 日本語Header／FooterのFont計測TestはScale 1.0，1.2（FHD），1.525で成功．
- 確定Commit版を1920×1080でStandalone診断：exit 0．通常画面とRecipe編集画面で実際のGUIStyle.CalcHeightと枠高さを照合し，overflow 0．4分裂・編集Pause・4 Gold・全弾終了も成功．
- 非表示WindowのScreenCaptureは黒画像だったため，実画面の目視QAとして使っていない．今回の表示確認はUserの問題画像，実Font計測Test，Standaloneの描画時メトリクス診断による．新しい見た目のUser確認はこれから．

### 配布成果物

```text
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-092323-0906230\Windows\OneBoardRecipeLab.exe
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-092323-0906230\OneBoardRecipeLab-v0.3.1-recipe-Windows-x64.zip
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-092323-0906230\OneBoardRecipeLab-v0.3.1-recipe-Windows-x64.zip.sha256
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-092323-0906230\FHD\smoke.txt
C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-092323-0906230\fhd.log
```

ZIP SHA-256：`b0bf38f15e553e115b8ef6e65c3e10f71833d6ec34d33dabbc49e11c05b56a3b`

途中Buildと報告画像：`C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-092145-2111268`（`user-reported-before.png`含む）．旧0.3.0は`Artifacts/Recipe/20260905-090615-4789771`に保持．

## 設計判断

### 確定

- User指示によるFHD文字サイズ・行高調整のみ．ゲーム進行や物理の数値は変更しない．

### 提案

- なし．

### 棄却または保留

- 全UIのFramework置換は今回行わない．文字の見切れと過大表示へ限定．

## 検証

- 実行Command：`.\scripts\recipe.ps1 verify`，Standalone `-screen-width 1920 -screen-height 1080 -recipe-capture ...`，`git diff --check`．
- 成功Test：57件とFHD Standalone診断．
- 未実行：Mac，新UIのUser試遊，実画面Screenshotの目視確認．
- 既知制限：非表示WindowのScreenshotは黒くなる．表示枠診断は描画時のFont寸法を使うが，主観的な読みやすさの評価を代替するものではない．

## 変更File

- Presentation/FreePlacementController.cs，FreePlacementController.Recipe.cs，RecipeUiText.csとmeta
- Editor/RecipeLabSceneBuilder.cs，Tests/PlayMode/RecipeUiTextTests.csとmeta
- scripts/recipe.ps1，Master，Prototype 2 Brief

## 次のHostへの依頼

1．feat/recipe-labの最新を取得し，0.3.1の表示修正を保持する．
2．Userが読みにくい箇所を追加報告したら対象枠とFont寸法を照合する．旧ArtifactやHandoffは削除しない．

## User判断が必要な点

- 実装に必要な未決事項はなし．新しい文字サイズの好みはUserの次の試遊で確認する．
