---
handoff_id: windows-20260905-1027-momentum-lab-verification
host: windows
created_at: 2026-09-05T10:27:00+09:00
base_commit: 9347ba9c500936b0a4db777620d21c98b99d36d5
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-0924-fhd-recipe-typography
---

# 速度資源型マガジン・ピンボールの初回試作

## 今回の目的

Userの「その方針でメモ」「その方針でプロトタイプを更新」に従い，速度資源・装甲抵抗・マガジン一括発射・動くゾーンの方針を記録し，旧試作を保持した別版を作った．

## 読んだ正本

- AGENTS.md，Master，Prototype 2 Brief，coordination README，Handoff Template
- 今回正本化したMaster v0.6.0 §32と`docs/AI_MOMENTUM_LAB_IMPLEMENTATION_BRIEF_2026-09-05.md`

## 実施内容

- 開始時の`feat/recipe-lab`はcleanでremoteと一致．pull後`feat/momentum-lab`を分岐．mainと旧Recipe Branchは変更しない．
- `docs/ideas/2026-09-05-speed-resource-direction.md`へUserの意図，採用範囲，将来候補を記録．Master §32を新試作の優先正本にし，旧FIXEDへの変更を新試作だけに限定．
- 新Pure C# MomentumSimulation，新MomentumLabController，新Scene／Editor Builder／Script／Testsを追加．旧C#やSceneは変更していない．
- 一クリック三発（通常・貫通・通常），Rで三Slotを変更．弾倉Snapshotにより発射途中の変更は既発射マガジンへ影響しない．撃ち切り後Reload，押下予約なし，残弾がいても次のクリックは受ける．
- 速度を時間と接触で消費．命中前速度でダメージ後に抵抗減速．通常は威力40／抵抗係数1，貫通は26／0.25．装甲的にも通常弾が効く．
- 普通4個・装甲3個のランダム初期配置．撃破報酬を一回だけ付与．破壊済みの的だけ次の斉射で補充し，生存的・障害物は保持．補充時は残弾の近くも避ける．
- 外壁と円形障害物で速度を失いつつ反射．移動する加速ゾーンは弾ごと一回だけ速度×2，最大1800．Swept Circle検索は移動ゾーンとの相対運動も扱う．
- FHDの整数Pixel文字描画を再利用．的のHPと抵抗，弾速，斉射，残弾，Goldを表示．
- 製品名／保存先を旧版から分離．弾倉はVersion付きJSONと日時履歴，Session Logは個別File．Goldと盤面はSession単位．Seed20260905を表示し再現可能にした．

## 得られた結果

- Game Version `0.4.0-momentum`，確定Code Commit `9347ba9c500936b0a4db777620d21c98b99d36d5`．
- Compile成功，EditMode49/49，PlayMode23/23成功．旧試作のTestを含む計72件．
- Windows x64 Build，ZIP，SHA-256再計算一致．
- 確定Windows版Standalone診断exit0：`screen=1920x1080; fired=3; boosts=3; paused=True; remaining=0; gold=3`．
- 通常画面／弾倉編集の描画時Font寸法によるoverflow診断0．非表示Windowでは完全なGUI Screenshotを確認できないため，実UI目視済みとはしない．Boardのみのoffscreen画像で加速ゾーン・反射軌跡・的配置を目視確認．
- 初回のテスト・Buildも成功．確認中に発射予定時刻のTick丸め累積を解消し，定時刻基準のTestを追加．的の文字を中央配置し，背景とのコントラストも調整した．失敗を規則変更で迂回していない．

### 成果物の絶対Path

```text
C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-102602-8036405\Windows\OneBoardMomentumLab.exe
C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-102602-8036405\OneBoardMomentumLab-v0.4.0-momentum-Windows-x64.zip
C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-102602-8036405\OneBoardMomentumLab-v0.4.0-momentum-Windows-x64.zip.sha256
C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-102602-8036405\EditMode.xml
C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-102602-8036405\PlayMode.xml
C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-102602-8036405\standalone.log
C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-102602-8036405\FHD\smoke.txt
C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-102602-8036405\FHD\01-flight-board.png
```

ZIP SHA-256：`7bbb0e3a08654c214ec303c43c76eda885c1f254d08220c59c55ca1a4d917b4c`

途中記録：`Artifacts/Momentum/20260905-102215-0748466`（未Commit作業版Build）と`20260905-102450-9394722`（Test）を保持．配布対象は上記102602の確定版のみ．旧Recipe0.3.1は`C:\Dev\one-board-incremental\Artifacts\Recipe\20260905-092323-0906230\Windows\OneBoardRecipeLab.exe`に保持．P0とP1Aも削除・上書きしていない．

## 設計判断

### 確定

- 最新試作は速度資源型．旧五発循環・Primer・Capacity・自由配置は復元用として別版に保持．
- 具体的な速度・HP・抵抗・発射間隔・消滅閾値は今回のINITIAL．現実の弾道再現ではない．

### 提案

- ゴールド／カオスゾーン，爆発／裂傷／分裂弾，銃種と弾倉の成長はメモに残した将来候補．

### 棄却または保留

- アーマーが特定弾以外を無効にするHard Gateは採用しない．全弾終了まで射撃禁止もしない．
- 今回は購入経済・Core・Material・追加ゾーンを実装しない．まず弾倉と速度を使う感触を見る．

## 検証

- 実行Command：`.\scripts\momentum.ps1 verify`，`.\scripts\momentum.ps1 test`，Standalone `-momentum-capture`，ZIP Hash再計算．
- 成功したTest：通常／貫通差，減速・消滅，止まった弾のダメージ，加速一回・上限，相対移動ゾーン衝突，高速Hit，接触内多重Hit抑止と再訪Hit，破壊一回報酬と補充，斉射・Reload・Snapshot・Pause，50 Seedの配置，入力再現性，Presentationと旧Test．
- 未実行：Mac検証，Userの新試作試遊，UI全画面の目視評価．
- 既知制限：初回Seed固定で再現性重視．弾倉のみ保存しGold・盤面はSession単位．経済成長はまだない．密な配置で補充候補がなければ次斉射へ保留．非表示Windowの通常Screenshotには依存しない．

## 変更File

- Master，Momentum Brief，方針メモ，AGENTS.md，README.md
- Core/MomentumSimulation.cs，Presentation/MomentumLabController.cs，Editor/MomentumLabSceneBuilder.cs
- Scenes/MomentumLab.unity，Momentum EditMode／PlayMode Tests，各新Assetのmeta
- scripts/momentum.ps1

## 次のHostへの依頼

1．`feat/momentum-lab`を取得しMaster §32とMomentum Briefを読む．旧章だけで現行試作を戻さない．
2．Userの試遊結果から速度・抵抗・マガジンの手応えを判断する．初期数値を完成版Balanceと扱わない．
3．旧Scene・Build・Handoff・途中Artifactを削除しない．

## User判断が必要な点

- 実装完了に必要な未決事項はなし．左クリック一回で三発，Rで弾倉編集．まず通常／貫通と加速ゾーンを使って遊んでもらう．
