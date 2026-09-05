---
handoff_id: windows-20260905-1402-challenge-magazine-followup
host: windows
created_at: 2026-09-05T14:02:47+09:00
base_commit: 731ff6f1529ad2f6de16e914271e2b1965e3a4a5
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-1343-buildcraft-brainstorm
---

# チャレンジのマガジン上限・一斉射Masteryと実装着手

## 今回の目的

Userの追加案を追記型で保存し，前回のAI案を訂正する。Userは「空間と命中演出→全弾待機＋少数の効果→銃2種類と付け替え→Goldとステージ進行」の実装着手も依頼した。本記録と実装結果は別Handoffにする。

## 読んだ正本

- AGENTS.md，Master §32〜32.2，Momentum Brief
- coordination README／HANDOFF_TEMPLATE，1343のブレスト

## 実施内容

追加発言と実装前の解釈・提案を記録。ここに記した仮数値を暗黙のFIXEDにしない。採用する実装範囲はMasterとBriefへ別途反映する。

## User追加発言（原文）

> 1. 飛行中にReloadを済ませる。
> 2. 分裂弾も含めて攻撃が終わったら再射撃。
> 3. 見た目の破片や光の余韻は待たない。
> 4. 待ちたくなければ「残弾回収」で、その後の攻撃機会を捨てて終了できる。
>
> これはいいっすね．
>
> 3．敵の全回復は「毎射撃」ではなく「次の遭遇」で
>
> について．
>
> 遭遇中っていうか，一回のチャレンジで打てる回数(マガジン）を増やせるようにしてチャレンジ時間を増やすっていう感じですかね．で，強化でマガジン数増やせるっていうか．それとは別に，各ステージで１マガジンで全部破壊出来たら特殊なスキル開放とか，そういうのでも面白いとは思います．
>
> とかかな．ではメモにこの記述も加えて，君からの提案も加えて新しくｐｕｓｈするとともに，空間と命中演出 → 全弾待機＋少数の効果 → 銃２種類と付け替え → Goldとステージ進行
>
> ここをやってみようか．

## 得られた結果

- Userは全弾待機・飛行中Reload・演出非待機・任意回収の4点を肯定した。
- チャレンジには使用可能なマガジン数があり，強化でそれを増やす案。「1マガジン内の弾数増加」とは別軸である。
- 1マガジン全破壊は通常Clearとは別の達成条件で，特殊Skillを開放する案。
- 前回AIの「通常進行は射撃回数無制限を推す」案より，今回のUserの有限マガジンChallenge案を優先する。

## 設計判断

### 確定

現行実装はまだ0.4.2-portrait。旧版と過去のBuild・Tag・Saveは保持。今回の発言は上記方向で次の試作に着手する承認であり，全候補効果やAscensionの一括承認ではない。

### 提案

- 初期3マガジン，Goldで4／5まで増やす小さな範囲をまず試す。上限・価格はINITIAL。
- チャレンジ内の生存敵HPを保持し，失敗またはClear後の次Challengeで全回復・配置変更。最後のマガジン発射直後ではなく，全攻撃解決後に失敗判定する。
- 残弾回収は撃ったマガジンを返却しない。早く終わる代わりに追加Damage機会を捨てる。射出途中の未発射分もキャンセルし，残弾だけ消して再予約発射することはない。
- 撃破Goldは失敗でも保持。通常Clearで次Stageを開き，1マガジンClearの特殊Skillは必須でない横の遊びにする。
- MasteryはStageごと一回だけ恒久記録。再訪可能，無料付け替え，Presetを残す。ランダム抽選・消耗品・Ascensionは入れない。
- まず3Stage程度と2銃，少数Modifierで最初から最後まで動く小さい循環にする。新旧Balance比較は旧Buildで行う。

### 棄却または保留

- Challenge失敗時のGold／効果喪失，毎マガジン全敵HP Reset，Mastery必須の進行Gateは採らない。
- 銃3種以上，大規模Skill Tree，Ascension，無限再帰する分裂／爆発は後回し。

## 検証

- 実行Command：Git status／fetch／mainとorigin/mainの差分確認，関連File読取。
- 開始時Clean，main差分0／0，作業Branch feat/portrait-stageを保持。
- Game Testはこのメモでは未実行。実装と検証の結果は後続Handoffへ記録する。

## 変更File

- docs/coordination/windows/2026/09/05/1402-challenge-magazine-followup.md

## 次のHostへの依頼

1. 1343メモとこの追記を併読し，マガジン制限について今回のUser発言を優先する。
2. 仕様・Code・Testの変更は実装Commit，Handoffは専用Commitで記録する。
3. 古い試作を消さない。このメモを実装完了報告と取り違えない。

## User判断が必要な点

着手に追加判断不要。試作を遊んでマガジン上限・火力・密度・Mastery難度を調整する。
