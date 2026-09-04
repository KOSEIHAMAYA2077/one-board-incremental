---
handoff_id: windows-20260905-0558-prototype0-reflection-fix
host: windows
created_at: 2026-09-05T05:58:00+09:00
base_commit: 3f56d5cc65140ca1e689eb688fdc142cfaba2694
authority: non-authoritative-handoff
status: needs-attention
consumed_handoffs:
  - mac-20260905-0457-windows-verification-response
---

# Prototype 0斜め壁反射の修正とWindows再検証

## 今回の目的

Windows x64 BuildのUser実機確認で，壁接触は記録される一方，反射軌道と反射後のCollector命中を確認できなかった問題を再現し，Prototype 0の範囲内で修正，検証する．

## 読んだ正本

- `AGENTS.md`
- `docs/MASTER_GAME_SPECIFICATION_2026-08-28.md`
- `docs/AI_PROTOTYPE0_IMPLEMENTATION_BRIEF_2026-08-28.md`
- `docs/coordination/README.md`
- `docs/coordination/mac/2026/09/05/0445-cross-host-sync-and-gumball.md`
- `docs/coordination/mac/2026/09/05/0457-windows-verification-response.md`

## 実施内容

- Userの実機報告とLocal Player Logを照合した．Core側は壁接触を反射として記録していたが，反射後のCollector命中は一件もなかった．
- Prototype 0 Sceneと同じGun，斜め壁，Collectorの位置・角度・寸法を使い，壁反射後にCollectorへ到達するPlayMode回帰Testを追加した．修正前は壁反射後にProjectileが消滅し，Testが失敗した．
- 斜め反射後のProjectile中心を反射方向へ押し出していた処理を，衝突面法線方向へ半径とEpsilon分だけ押し出す処理へ変更した．
- Projectileへ短いTrailを追加し，反射後はCyanへ変化させた．Projectile終了後も0.32秒だけTrailを残し，屈曲を目視できるようにした．
- 画面下部の案内を，Cyan壁中央付近を狙いTrailの屈曲を確認する文面へ変更した．
- 修正前のBuild，Package，TestResults，Logsを日時付きArchiveへ複製し，削除していない．

## 得られた結果

- 原因は反射式そのものではなく，斜め壁からの分離方向だった．反射方向への押し出しでは法線方向の距離がProjectile半径未満となり，壁へ重なったまま次Tickへ進む場合があった．
- 次Tickで同じ壁へ再接触し，残り反射回数が0のためProjectileが消滅していた．Userが報告した「壁の下側だけ反射するように見える」症状と整合する．
- 法線方向へ分離する修正後，実際のScene配置を使う反射命中Testを含むPlayMode Test 9件がすべて成功した．
- Game Version，Reload，Spread，Gold，反射倍率，Prototype 0実装範囲は変更していない．
- Code修正Commitは`3f56d5cc65140ca1e689eb688fdc142cfaba2694`である．

## 設計判断

### 確定

- Prototype 0には壁一枚と一回反射，反射後のCollector報酬`floor(baseGold × 1.5)`を維持する．
- Unity `6000.3.18f1`，Game Version `0.1.0-prototype0`，MasterとPrototype Briefの値は変更しない．
- 過去の試作，Handoff，Git Commit，検証記録を削除しない．

### 提案

- なし．Trailは既存の必須反射を観察可能にするPresentation修正であり，新しいGame Logicではない．

### 棄却または保留

- 壁位置，反射回数，Projectile速度を変更して問題を回避する案は採用していない．
- 新しいWindows BuildでTrailの屈曲とUpgrade後の反射命中`＋3`をUser実機で確認する作業は保留中である．

## 検証

- 実行したCommand：`.\scripts\unity.ps1 test-play`，`.\scripts\unity.ps1 verify`
- 成功したTest：EditMode 18/18，PlayMode 9/9．新規PlayMode TestはPrototype 0 Sceneと同じ斜め壁経路で反射後のCollector命中を確認した．
- Windows x64 Build：成功．`C:\Dev\one-board-incremental\Artifacts\Builds\Windows\OneBoardPrototype0.exe`
- ZIP：`C:\Dev\one-board-incremental\Artifacts\Packages\OneBoardPrototype0-v0.1.0-prototype0-Windows-x64.zip`
- SHA-256：`a5b239e56eb6a114142016d514c7bdc47ec76b1f41de1ab1dc9915b3e302f492`
- 修正前成果物Archive：`C:\Dev\one-board-incremental\Artifacts\Archive\20260905-0555-pre-reflection-fix`
- 未実行項目：修正版Windows BuildのUserによる目視Playtest．
- 既知問題：自動Testでは経路と報酬を確認済みだが，Trailの視認性と実際のMouse操作での狙いやすさはUser実機確認が必要である．

## 変更File

- `Assets/_Project/Core/ProjectileSimulation.cs`
- `Assets/_Project/Presentation/Prototype0Controller.cs`
- `Assets/_Project/Tests/PlayMode/Prototype0PlayModeTests.cs`
- `docs/coordination/windows/2026/09/05/0558-prototype0-reflection-fix.md`

## 次のHostへの依頼

1．GitHubからWindowsのCode修正Commitと本Handoff CommitをPullし，斜め反射の分離修正と回帰Test追加を確認する．
2．Userの修正版Windows目視結果が届くまで，壁位置，反射倍率，速度，Prototype 0範囲を変更しない．
3．今後Scene配置を変更する場合も，実配置を使う反射命中Testを維持する．

## User判断が必要な点

- 修正版Windows Buildで，Cyanへ変わるTrailが壁で屈曲することと，Upgrade後の反射命中が`＋3`になることを再確認する．
