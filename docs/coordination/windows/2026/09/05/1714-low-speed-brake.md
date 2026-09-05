---
handoff_id: windows-20260905-1714-low-speed-brake
host: windows
created_at: 2026-09-05T17:14:00+09:00
base_commit: 52386751ce8934649799eddb6d3a94acfa4db6df
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-1630-lucent-crystal-kit
---

# 低速弾の滑走を短くする0.5.4-brake

## 今回の目的

勢いのある飛行を保ち，終盤の低速滑走だけを短くする試遊用Patch。

## 会話からの開発記録

### Userの指示・発想

- 要約：見た目は良いが弾が滑って止まらない感じがする。現在の減速を知りたい。少なくとも最後はメリハリよく終わってほしい。
- Assistantの調整案へ「やってみていいっすよ」と実装を承認。

### Assistantの提案

- 旧時間減速は全速度域で120/s。接触・加速なしなら再射撃閾値300から停止80まで約1.83秒残ることを原因候補として説明。
- 高速域は維持，300以下のみ急減速して約0.25秒で終える。弾と尾を収縮させ，最後に短い非攻撃光を残す。

### 採用・保留・変更

- 採用：低速減速880/s。再加速して300を超えれば120/sへ戻す。固定の死期Timerは追加しない。
- 採用：低速追加Hitの機会が減ることを意図したテンポ調整。威力の式やGold価格を同時に補正しない。
- 保留：現実の弾道再現，高速域の変更，銃・効果・経済の追加。感触の採否はUser試遊後に判断する。

## 読んだ正本

- AGENTS，Master §33，Momentum Brief，coordination README／Template。Masterは0.7.4／§33.8，Briefは0.5.4-brakeを追加。

## 実施内容

- `MomentumRules.TimeSpeed`で300をまたぐTickの時間を高速域と低速域へ分割。Challengeのみ新減速を適用し，Progressなしの旧Momentum Labは元の120/sを保持。
- 弾とAuraを速度に応じて100→35%に収縮し，尾の長さ・幅・明るさを低下。Collider半径は不変。Crystal／Neonでは停止後0.12秒の光を最大64個まで表示し，Simulation時間で消す。Pauseで停止，攻撃・報酬・再射撃に非干渉。
- Game VersionとBuild出力名を0.5.4-brakeへ更新。旧0.5.3，Crystal Kit V1，Gallery，過去Build／Saveを削除していない。

## 得られた結果

- Source Commit：`52386751ce8934649799eddb6d3a94acfa4db6df`，Branch `feat/portrait-stage`。SourceとHandoffを別Local Commitで保存。今回Push／Tag／Release／main Mergeは行っていない。
- ゲーム：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-171221-7555051\Windows\OneBoardMomentumLab.exe`
- ZIP：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-171221-7555051\OneBoardMomentumLab-v0.5.4-brake-Windows-x64.zip`
- ZIP SHA-256：`170c37b190a31bde473037f9dff4c97b3e9c9fd39aee7858f4fa57101d56f461`
- Test XML／Build Log／summary：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-171221-7555051\`
- 実機診断画像とPlayer Log：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-171221-7555051\Smoke\`
- 比較用旧版：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-162337-1513972\Windows\OneBoardMomentumLab.exe`

## 設計判断

### 確定

- User承認のMaster §33.8のみ。高速域120/s，初速，抵抗，反射損失，加速倍率，停止80，再射撃300，10秒寿命，Save形式を維持。

### 提案

- 試遊では終盤の待ち／滑りが軽くなったか，加速ゾーンへ向かう期待を早く切りすぎていないかを確認する。

### 棄却または保留

- 300到達時の即消去はせず，0.25秒の減速区間を残す。見た目の縮小に合わせた判定縮小や，威力補償は行わない。

## 検証

- 実行：`.\scripts\momentum.ps1 test`，続いてSource Commit後に`.\scripts\momentum.ps1 verify`。Unity 6000.3.18f1。
- Compile成功。EditMode79/79，PlayMode31/31，計110/110成功。Windows x64 Build，ZIP，SHA-256生成と再計算一致を確認。
- 新規Test：0.01／1/60／0.05秒Tickで300から0.25秒以内に消滅，移動30〜50論理pixel，高速域不変，閾値またぎの分割，旧Lab減速保持，Pause，停止時の最後のHit，表示収縮，余韻の非攻撃性とPause／寿命。既存の再加速・子弾・再射撃Testも成功。
- 実機診断：`-momentum-capture <Smoke> -momentum-stress`，Exit0，FHD，18発，15加速，最大同時43弾，分裂抑制0，終了時残弾0。UI overflowなし，Player Logに確認対象のException／Shader errorなし。全画面画像を目視し描画と文字配置を確認。
- 診断区間の平均4.17ms／最大5.04msは短時間の記録に限る。長時間性能や最大256弾の保証ではない。
- `git diff --check`成功。新規Unity Assetなし，既存metaを保持。
- 未実行：Mac検証，Userによる止まり際の最終評価，長時間Play，最大弾数の性能評価。
- 既知の制約：低速中の到達距離・追加Hitは旧版より減る。途中の加速・衝突があれば単純な0.25秒経過とは異なる。縮小するのは見た目で，消えるまでは元の判定半径で攻撃する。

## 変更File

- `Assets/_Project/Core/MomentumSimulation.cs`
- `Assets/_Project/Presentation/MomentumNeonView.cs`，`MomentumLabController.cs`
- `Assets/_Project/Tests/EditMode/MomentumChallengeTests.cs`
- `Assets/_Project/Tests/PlayMode/MomentumPlayModeTests.cs`
- `scripts/momentum.ps1`，`AGENTS.md`，Master，Momentum Brief，本Handoff。

## 次のHostへの依頼

1. 0.5.3と0.5.4を残したまま止まり際を比較する。新旧Saveは共通形式なので通常の試遊では恒久進行が保存される。自動診断は保存を無効にして実行済み。
2. Userの感触に応じて低速閾値／時間を次Patchで調整する。今回のHandoffから未採用案をGame Logicへ輸入しない。
3. Push時は他の未送信Commitを含むことを確認する。main READMEだけ先に更新されているため古い説明で上書きしない。

## User判断が必要な点

- 止まり際の感触の採否。今回は試遊版完成までで，GitHub公開操作は未承認。
