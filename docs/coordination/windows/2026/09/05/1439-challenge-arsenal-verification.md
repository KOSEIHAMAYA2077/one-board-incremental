---
handoff_id: windows-20260905-1439-challenge-arsenal-verification
host: windows
created_at: 2026-09-05T14:39:42+09:00
base_commit: 27be90f55280f82d2ed893d1260016def4afe96c
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-1343-buildcraft-brainstorm
  - windows-20260905-1402-challenge-magazine-followup
---

# Challenge Arsenal実装とWindows検証

## 今回の目的

Userが承認した順序「空間と命中演出→全弾待機＋少数効果→2銃と付け替え→GoldとStage進行」を，小さな試作0.5.0-challengeとして一巡させる。追加発言の記録と実装結果を混同しない。

## 読んだ正本

- AGENTS.md，Master §32〜32.2，Momentum Brief，coordination READMEとHandoff Template
- 1343ブレスト，1402追加案。今回の承認範囲をMaster v0.7.0 §33とBriefへ明示的に昇格した。

## 実施内容

- 中央を600×800（3:4）に拡張，的を旧半径75%で12個，奥へバンパーを移動。明るい床・結晶面・厚み，命中傾き，立体破片を追加。
- 一斉射の全攻撃終了待ち，飛行中Reload，Space回収。回収で未発射弾もキャンセルし，使用マガジンと得たGoldは巻き戻さない。
- Revolver6発とUZI18発。共通効果7種，容量20，恒久所有，無料付け替え，Preset3枠。
- 3Stage，初期3マガジン，携行4／5へのGold強化，生存敵HP継続，失敗時Gold保持，Stageごとの1マガジン全破壊による特殊効果開放。
- 新Save challenge-v1.jsonを導入。旧弾倉Saveは変更しない。履歴付きAtomic保存，未知／破損時は元Fileを保護し一時Playを明示。
- 旧8 Release，Tag，Build，中間失敗Logを保持。Stage進行等は今回の新試作に限った仕様変更。

## 途中のUser追記（原文）

> 分裂が無限に増えるなら増えるでいいんだけどね，１０秒以内とかにすればさすがに壊すまでにはいかないでしょ．

この追記を受け，子弾の分裂禁止／世代上限案は採用しなかった。最初の射出から10 Simulation秒の終了時刻を全子弾が共有する方式に変更。指数的増殖は10秒以内でも負荷になり得るため，同時256弾・一斉射累計1024生成の安全上限を表示し，上限時は分裂を抑制して親弾を飛ばし続ける。この安全上限はUIに明示した実装上の提案値であり，Userが具体的に指定した数値ではない。

## 得られた結果

- Build source：`27be90f55280f82d2ed893d1260016def4afe96c`
- Branch：feat/portrait-stage。mainへMergeしない。
- Artifact Root：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-143627-4569252`
- EXE：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-143627-4569252\Windows\OneBoardMomentumLab.exe`
- ZIP：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-143627-4569252\OneBoardMomentumLab-v0.5.0-challenge-Windows-x64.zip`
- SHA-256：`cd8a6e14165f639b1b41c83e335e49a9828b845d3fb642d3144e43576f0bf30b`
- Release予定先：`https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.5.0-challenge`。ZIPのアップロードを含む操作は外部送信の安全確認で拒否され，Release作成は未実行。ローカルBuildは完成。送信先のoriginとGitHub repo metadataがUser指定の非公開KOSEIHAMAYA2077/one-board-incrementalに一致することを再確認した。依頼済みのCode／Handoff Pushと，別途許可が必要なZIP追加を分離する。
- 画像：上記Rootの`VisibleSmoke\02-full-ui.png`。1920×1080の実表示を目視確認。VisibleStressは18発・分裂終了まで成功。テストWindowはUserの明示許可後に表示し，自動終了。診断は永続Save無効。

## 設計判断

### 確定

- 今回採用した範囲・INITIAL数値はMaster §33とMomentum Briefを参照。Handoff単体を正本にしない。
- 「弾倉内の弾数」と「Challengeで使えるマガジン数」は別。後者は3→4→5の恒久強化。
- Masteryは通常Stage進行と分離。未達でもClearできれば次へ進める。
- 分裂子弾は速度・効果・共通終了時刻を継承し，接触を抜ける前の同じ的への再分裂を防ぐ。世代数で打ち切らない。

### 提案

- まず今回のBuildを遊び，Space回収を使いたくなる頻度，分裂装備の楽しさ，携行増加の価値，Mastery達成感を見る。
- 最初から最終成長曲線を確定しない。Userは趣味・AI駆動試作を優先しているので，短いPlay感想から次の変更を選ぶ。

### 棄却または保留

- 子弾を一世代で打ち切る案はUser追記により撤回。
- Ascension，3銃目，大規模Skill Tree，抽選Perk，外部Asset導入は保留。
- Camera自体の傾斜・屈折・Bloomは未実装。今回は厚みのあるMeshを22度傾け，操作平面の対応を維持。

## 検証

- Command：`.\scripts\momentum.ps1 test`，最終`.\scripts\momentum.ps1 verify`，Standalone `-momentum-capture <folder>`／`-momentum-stress`，Git status／fetch／diff --check，GitHub Release既存一覧確認。
- 最終Compile成功，EditMode66/66，PlayMode25/25，Windows x64 Build／ZIP／SHA-256成功。
- Test内容：旧Core Test，100Seed×3Stageの12的配置，残弾待機，回収による未射出取消，最後の攻撃からのClear，失敗後Gold保持，Mastery非必須，容量，Preset／JSON往復，不正Save拒否，再分裂・速度・寿命継承，256弾上限と親弾維持，Golden一回報酬，爆発非再帰。
- VisibleSmoke：FHD，6発，boost4，Pause=True，終了時残弾0，Gold37，Exit0。Playing／Editorの文字Overflow0。
- VisibleStress：FHD，18発，boost15，Peak43弾，Pause=True，終了時残弾0，初期診断Gold1000→1044，Exit0。Playing／Editorの文字Overflow0。Player LogにError／Exception／Warningなし。
- VisibleStressの診断Frame値は平均／最大約4.16ms。ただし短い診断・特定環境・43弾の観測で，256弾でのFPS保証ではない。非表示実行は画面が黒く描画停止することがあり，非表示時のFrame値を性能成功の証拠には使わなかった。
- 合成Play観察：初期Revolver，最初はZone，残りは生存敵へ向ける3斉射でStage1は30/30Seed Clear。育成済みUZI・購入4効果・火力+50%，各Stage30Seedを1斉射でZoneへ向けるとClearは9／2／1件。Peak244弾・13世代を純粋Simulationで確認。人間のPlay評価や最適化探索ではない。

### 途中で発見・修正した問題

1. 連鎖上限の説明欄の高さ不足：実画像とOverflow診断で検出し，欄を拡張。
2. 子弾がすぐ消える：AimCalculator.RotateDegreesは単位方向を返すため，速度継承が抜けていた。角度・威力の調整だけでは直らず，親速度を掛けて根本修正。次Tick生存をTestへ追加。検討中の威力85%・角度28度は採用せず，正本55%・18度を保持。
3. 子弾が小さくなったことで，親の接触入口でExitingが解除され，同じ接触で再分裂する：離れる方向へ抜けるまで継承した除外を保持するよう修正。
4. 速度修正後，一つの旧Testが全敵ClearしてReady=Falseになる：Reload待機だけを測るFixtureのHPを高くし，Challenge終了と待機の検査を分離。Clear判定を変更して迂回していない。
5. 非表示Windowの連鎖診断が実時間をほぼ進めず終了：診断だけ明示Tick進行に変更。User承認後，可視Windowでも別途確認。

### 未実行と既知の制約

- Mac検証，長時間Play，最大密度256弾での可視描画性能，手動の全購入／全Preset操作，実ファイル破損／ディスク満杯を強制した試験は未実行。
- Fire・Hitの既存音を使用。新曲・専用Reload音の作り込みは行っていない。
- 価格・火力・Stage特徴・Mastery難度はINITIAL。Stage3の1マガジンClearは今回の単純照準サンプルでは低頻度。
- Saveするのは恒久進行だけ。途中Challengeは再起動で復元しないが，獲得Gold・購入・到達権は残る。

## 変更File

- AGENTS.md，Master，Momentum Brief，scripts/momentum.ps1
- Core：MomentumBoardLayout／MomentumSimulation，新MomentumChallenge／MomentumProgress
- Presentation：MomentumLabController／MomentumPortraitHud／MomentumNeonView，新MomentumProgressSave
- MomentumのEditMode／PlayMode Test，新CSのUnity生成meta
- docs/CHALLENGE_ARSENAL_RELEASE_2026-09-05.md，docs/RELEASE_ARCHIVE.md
- このHandoffはCodeとは別Commit。新規Path：docs/coordination/windows/2026/09/05/1439-challenge-arsenal-verification.md

## 次のHostへの依頼

1. feat/portrait-stageの最新CodeとこのHandoffを取得。最新仕様はMaster §33。1343・1402の候補案をそのまま復活させない。
2. 旧8版，新版の中間Build，旧Saveを削除しない。Build sourceと検証済みZIPの対応を保つ。
3. 次はUserのPlay感想をもとに調整。最大密度の実機描画と長時間保存を必要に応じて追加確認。

## User判断が必要な点

- 新版Windows ZIP（約34MB）を指定の非公開GitHub RepositoryへRelease添付してよいか追加許可が必要。
- ローカル試作は遊べる。広さ・明るさ・連鎖の勢い・回収の手触り・マガジン強化の価値について感想をもらう。
