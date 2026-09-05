---
handoff_id: windows-20260905-1524-clear-panel-and-next-ideas
host: windows
created_at: 2026-09-05T15:24:15+09:00
base_commit: 8d76a5f
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-1510-low-speed-refire
---

# 中央クリア案内と次段階の候補メモ

## 今回の目的

Userが全的破壊後の待機を停止と感じたため，承認された「CLEAR／次へ／もう一度」を中央に表示する。Userの「敵の硬さ，強化，ステージごとの素材，クリア後の再挑戦で敵が強くなる要素も足したいが，ひとまずプロトタイプとしてはこんなものか」という案と温度感を残す。

## 読んだ正本

- AGENTS.md，Master §33，Momentum Implementation Brief，Handoff Template。
- 過去案は実装根拠にしない。新しい経済要素は今回はMasterへ昇格していない。

## 実施内容

- Clear確定時に中央カードと盤面の暗幕を表示。Stage名・使用マガジン数・今回の獲得Goldも表示。
- 次へは次Stage，再挑戦は同Stage。最終Stageでは次へ無効，全3Stage達成を明示。左右の強化・Stage選択は継続使用可能。
- 既存のChallenge開始とクリック遮断を再利用。切替クリックの誤射と二重遷移を防ぐ。Core／経済／Save schemaは変更なし。
- Master v0.7.2 §33.6，Brief，AGENTSを同期。Version 0.5.2-clear。
- 開始時Clean，既存の未Push 2 Commitを保持。fetch後main/origin差分0，他端末の作業Branch更新なし。mainへ切替・Mergeしない。

## 得られた結果

- Source Commit：62979e2f9bc8a318c820e981b5d2fec75ef372d2 (`feat: show clear result and next-stage actions`)。
- Root：C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-152312-9272260
- EXE：C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-152312-9272260\Windows\OneBoardMomentumLab.exe
- ZIP：C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-152312-9272260\OneBoardMomentumLab-v0.5.2-clear-Windows-x64.zip
- SHA-256：1ac36924c0e3f583fc58e92e61cd7fc7823d07877c223e241ba97c2e673783cd。ZIPと同Pathに.sha256も保存。
- FHD画像：上記RootのClearSmoke\clear-stage-1.png〜clear-stage-3.png。Stage1と3を目視し，文字・ボタンの切れなし。
- Local Commitまで。Push／Tag／Release追加は未実行。旧版・旧Save・過去Handoffを保持。

## 設計判断

### 確定

- 今回は案内の改善のみ。速度300以下の再射撃，クリア確定待ち，既存の敵HPと報酬を維持。

### 提案（未実装・未確定）

- User候補：敵の硬さと強化，Stage固有素材，クリアしたStageを再挑戦する際の敵強化。
- Assistant所感：射撃・反射・速度・分裂・構成変更の基礎的な楽しさを試せる段階なので，現試作は一度区切ってよい。長期成長の面白さまで実証した意味ではない。
- 次に広げるなら「同じStageの難度を自分で一段上げる→追加報酬→構成を強化する」を小さく試す案。再挑戦のたびに強制難化させず，低難度への再訪も残せば，比較や稼ぎ直しができる。
- Stage固有素材は，使い道が単なるGoldの置換にならないよう，そのStageらしい効果や銃の強化とセットで検討する。種類・価格・倍率はまだ決めない。

### 棄却または保留

- 敵HP・素材・周回難化・成長曲線の追加実装は保留。今の変更へ抱き合わせない。

## 検証

- Command：.\scripts\momentum.ps1 test，.\scripts\momentum.ps1 verify，Standalone -momentum-capture <folder> -momentum-clear，Get-FileHash，git diff --check。
- Compile，EditMode74/74，PlayMode26/26，Windows x64 Build，ZIP，SHA-256成功。
- 追加PlayMode：Clear表示条件，Pause，再挑戦，Gold／Mastery保持，12的復元，次Stage，二重遷移不可，最終Stageの次へ不可，遷移直後の射撃遮断。
- 可視診断Exit0，3StageのClear状態一致，文字Overflow0。診断は敵HPを0にする専用Fixtureであり，実プレイの3Stage攻略実績ではない。保存を無効化し自動終了。
- 未実行：Mac，手動マウスによる全ボタン操作，長時間Play。既存の戦闘バランス／最大密度の描画性能は今回再評価していない。
- 既知問題：Clear確定自体は残弾終了／回収後。失敗時は既存の左側案内を継続する。

## 変更File

- AGENTS.md，Master，Momentum Brief，scripts/momentum.ps1。
- MomentumPortraitHud.cs，MomentumLabController.cs，MomentumPlayModeTests.cs。
- 本Handoff（独立Commit）。

## 次のHostへの依頼

1. Windowsの0.5.1と0.5.2はまだLocalのみ。送信後に作業Branchを取得し，main統合済みと仮定しない。
2. 次段階候補は提案に留まる。Userと優先順位を決めてからMasterへ反映する。旧試作を保持。

## User判断が必要な点

- 今回の範囲ではなし。次段階の敵・素材・周回設計は別途相談する。
