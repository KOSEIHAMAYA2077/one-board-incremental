---
handoff_id: windows-20260905-1758-slow-cadence-and-live-aim
host: windows
created_at: 2026-09-05T17:58:00+09:00
base_commit: 25758d201b529737c0b6945a6fbfc9bd0d66c5bc
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-1744-active-shooting-and-profit-gimmicks
---

# 発射を遅くし，撃ちながら狙い分ける0.5.5-steer

## 今回の目的

ブレストメモを先にGitHubへ保存し，続いて発射レートと射撃中の照準変更を実装する。

## 会話からの開発記録

### Userの指示・発想

- 要約：終盤へ向け，弾を増やして眺めるだけでなく，工夫がGold稼ぎになる仕掛けが欲しい。低速でバンパーへ当てると報酬などの例。先にメモをPushし，その後レート／射撃中照準を実装するよう依頼。

### Assistantの提案

- 一マガジンを約2秒かけて発射し，未射出弾をその瞬間のマウス方向へ振り分ける。発射済み弾は操縦しない。
- ワープ・蓄積・変換・バンパー収益・スキルツリーは別の設計候補として保持し，操作改善へ混ぜない。

### 採用・保留・変更

- 採用：Revolver6発／0.4秒間隔，UZI18発／0.12秒間隔，射撃中の有効照準更新。
- 採用：カーソルがUI／盤面外／銃の近傍へ出た場合は最後の有効方向を保持。Pause・Focus・Pause閉じ直後の入力防止を維持。
- 保留：ギミック，報酬数値，Skill Tree。+2k Goldを確定値と扱わない。詳細ブレストは1744メモ。

## 読んだ正本

- AGENTS，Master §33，Momentum Brief，coordination README／Template。今回Master0.7.5／§33.9とBriefを更新。

## 実施内容

- メモCommit `e84a4d8ab5a663fd7bc8855b21d5f4a6ca16bdbe` を先に `origin/feat/portrait-stage` へPush。未送信だった0.5.1〜0.5.4のSource・素材・Handoffも含む。remote HEAD一致とRepository sanity成功を確認。
- CI：https://github.com/KOSEIHAMAYA2077/one-board-incremental/actions/runs/33956135030
- mainは作業前にff-onlyでcdd743fへ更新済み。ゲームのmain Merge，Tag，Release，ZIPアップロードは実施していない。
- CoreにUpdateBurstAimを追加。射出中のみ有限・盤面内・Gunから30以上の入力を受け，次弾へ適用。既存Ballの速度や方向は更新しない。各弾のSeed付きSpreadは維持。
- 発射間隔をCoreの一つの定義へまとめ，HUDも同じ値を表示。銃身と次弾の向きを一致させ，「マウスで照準」を表示。
- 自動診断は実カーソルの入力を受けないようにし，専用の左右照準変更シーケンスを追加。通常PlayではMouseAim→SteerBurst→Coreへ接続する。
- 旧Labは三発0.12秒・固定照準を維持。弾速・急減速・弾数・10秒の斉射寿命・Save形式・旧AssetとBuildは変更しない。

## 得られた結果

- 操作改善Source：`25758d201b529737c0b6945a6fbfc9bd0d66c5bc`，Game `0.5.5-steer`。このSourceと本HandoffはLocal Commitのみ。依頼された先行メモのPush後に実装したため，0.5.5はまだ送信していない。
- ゲーム：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-175539-8757400\Windows\OneBoardMomentumLab.exe`
- ZIP：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-175539-8757400\OneBoardMomentumLab-v0.5.5-steer-Windows-x64.zip`
- SHA-256：`a38d945cd790c4a5ac261e29eb764ae4e6e022fca9d6d3ea2c1dd2fdb9741cf8`
- Test XML・Build Log・summary：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-175539-8757400\`
- 実機画像とLog：同Directoryの`Normal\`と`Steering\`。
- 比較用旧版：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-171221-7555051\Windows\OneBoardMomentumLab.exe`

## 設計判断

### 確定

- Master §33.9の操作変更のみ。クリック一回で一マガジン，途中クリックを次射撃として予約しない。Reloadは最後の射出から0.8秒。
- 最初→最後は2.0秒／2.04秒。ただし60tick/sのUZI最終発射は直後のTick（約2.05秒）となる。親子寿命は最初から10秒で，後発弾の寿命を延長しない。

### 提案・保留

- 試遊ではマガジンを左右へ振り分ける時間が足りるかを見る。追加の報酬ギミックは今後別に検討し，今回のGoldや価格は補正しない。

## 検証

- `.\scripts\momentum.ps1 test`，Source Commit後に`.\scripts\momentum.ps1 verify`。Unity6000.3.18f1，Compile成功，EditMode83/83，PlayMode32/32，計115/115。Windows x64 Build，ZIP，SHA生成・再計算一致。
- 新規Test：両銃の発射時刻・全弾数・Reload開始・10秒寿命維持，照準変更の未射出限定，盤面外・NaN・Infinity・近傍・Pause入力拒否，旧Lab不変，同じSeed＋入力列の再現性，Controller経由のUI／Focus／Pause閉じ防止と次弾の左右方向。
- 既存の再射撃Testは旧短時間の前提を新しい撃ち切り時間へ更新。最初のPlayMode失敗は長い射撃中に全敵が倒れ，正常な「敵0なら追加射撃禁止」に当たったこと。再射撃用Fixtureだけを生存敵あり・明示入力にし，ゲームのHPや報酬は変えず再検証で成功。
- 通常診断：Exit0，6発，2加速，Pause成功，最終残弾0，Gold37，UI overflowなし。
- 左右照準診断（`-momentum-stress -momentum-steer`）：Exit0，照準更新2回，18発，14加速，最大同時44弾，分裂抑制0，最終残弾0。FHD画像で表示・HUD可読性を目視し，Player Logに確認対象Exception／Shader errorなし。
- 診断の照準はAPIによる入力であり，人間がマウスを動かした最終試遊とは区別する。Mac・長時間Play・最大弾数の性能評価は未実行。
- `git diff --check`成功。新規Unity Assetなし。既知の制約：遅い射出によって命中順・加速Zoneとの遭遇・DPSは変わり得る。これは発射レート変更の結果であり同一戦果を保証しない。

## 変更File

- `Assets/_Project/Core/MomentumSimulation.cs`，`MomentumChallenge.cs`
- `Assets/_Project/Presentation/MomentumLabController.cs`，`MomentumPortraitHud.cs`
- EditMode MomentumChallengeTests，PlayMode MomentumPlayModeTests
- AGENTS，Master，Momentum Brief，`scripts/momentum.ps1`，本Handoff。

## 次のHostへの依頼

1. クリック後，約2秒間でマウスを左右へ動かして次弾が振り分けられるか試遊。旧版やSave履歴を消さない。
2. 1744メモと0.5.4までのSourceはGitHubへ保存済み。0.5.5とこの検証記録はLocalのみなので，次回Pushで区別する。
3. 条件付きGoldギミックはまだ未設計。Masterへ正式に反映するまで例示数値を実装しない。

## User判断が必要な点

- 操作感の採否。0.5.5のPush／Release保存は今回の先行メモPushとは別の未実施操作。
