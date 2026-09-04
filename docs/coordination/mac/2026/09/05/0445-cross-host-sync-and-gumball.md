---
handoff_id: mac-20260905-0445-cross-host-sync-and-gumball
host: mac
created_at: 2026-09-05T04:45:07+09:00
base_commit: 5d832eb1e4b1497e8b8cf022e662b7f6f0b0df77
authority: non-authoritative-handoff
status: complete
consumed_handoffs: []
---

# Mac・Windows同期基盤とGumball追加観察

## 今回の目的

Windows側ChatGPT Work／CodexとMac側の作業を，GitHubを介して安全に引き継ぐ方式を定める．併せて，GitHub上のDesign Memoへまだ十分に入っていないIdle Gumball Machineの追加プレイ観察をWindowsへ渡す．

## 読んだ正本

- `AGENTS.md`
- `README.md`
- `docs/MASTER_GAME_SPECIFICATION_2026-08-28.md`
- `docs/AI_PROTOTYPE0_IMPLEMENTATION_BRIEF_2026-08-28.md`
- `docs/DEVELOPMENT_WORKFLOW.md`

## 正本とWindows側復元内容の照合

- `main`の照合時点は`5d832eb1e4b1497e8b8cf022e662b7f6f0b0df77`である．
- Prototype 0の目的，実装範囲，Unity `6000.3.18f1`，Master `0.3.0`というWindows側の復元は概ね正しい．
- `docs/DESIGN_MEMO_2026-08-28.md`には，Gumball序盤の約10回反復，価値`1`から`2`への変化，購入直後の倍化を参照する判断が保存されている．
- ただし，2026-09-05にUserが報告した反射，弾の一発制限，Ascensionごとの価値変化，盤面滞在報酬，終盤の無限反射と意図的崩壊に関する詳細観察は，この時点のDesign Memoには十分に保存されていない．したがって「Gumball観察が全て保存済み」とは扱わない．

## Gumball追加プレイ観察

以下はUserによる一次プレイ観察であり，外部資料で再検証した確定仕様ではない．

- 通常の発射レートは大きく変わらず，成長は一発が生む反射，貫通，追加Hit，別Projectileへ移っていく．
- 序盤のTarget値はHitごとに半減し，全消去後に再配置されるように見えた．
- Gold UpgradeにはTarget値加算，現在値の倍化，Target追加，反射報酬，反射回数依存効果，Projectile消滅地点から逆向きに出る風船などがあった．
- Gumという別資源とAscension Roomがあり，新Roomごとに複数弾，貫通，法外な報酬など，収益を生む条件自体が変わった．
- 5 Hitごとの遠隔Hitや，弾が盤面に存在するだけでGoldを得る効果により，正確に当てること以外へ価値が広がった．
- 一つの盤面に通常弾は一個までで，消滅するまで次弾を撃てないように見えた．別種のKnifeなどは存在した．
- Ascension費用は単調増加ではなく，劇的な解放までのPhase尺に合わせている可能性がある．これは観察からの推測である．
- 後半はBumperによる長時間または無限反射へ至り，Ascension 9前後ではUpgradeをほぼ即時に買える状態まで意図的に崩壊したように感じられた．

## 設計上の読み取り

### 現行設計と整合する点

- 成長の中心を発射レートの暴騰ではなく，一発の経路，Lineage，反射，Recipe，相互作用へ置く．
- 報酬契機を`OnFire`，`OnHit`，`OnNthHit`，`OnReflect`，`WhileAlive`，`OnExpire`，`OnLineageComplete`のように考えると，Targetと五発Recipeの設計語彙を増やせる．これは設計用の分類であり，Prototype 0へ汎用Event Frameworkを実装する指示ではない．
- Overdriveを最後の5〜10分程度の制御されたゲーム崩壊として使い，制限解放と爆発的成長の直後にCore破壊とEndingへ接続する案は，Prestigeなしの現行方針と相性がよい．
- Phaseごとに価格を一つの指数式で決めるのではなく，開始資産，最初の購入，買い切り総額，解放後収益，次解放までの時間から逆算する．

### Prototypeで比較する提案

- 同時Active Lineage数について，現行のReloadのみで再射撃可能な案，前Lineage消滅まで撃てない案，序盤上限1から強化で増やす案を比較する．第一候補は上限1から段階的に増やす案だが，五発RecipeのTempoと競合するため未確定である．
- 各主要解放は単なる倍率ではなく，報酬発生条件，経路判断，操作対象，自動化範囲のいずれかを変える．
- 通常Reloadは現行想定の約`0.65`秒から`0.40`秒程度の範囲に抑え，成長を一発当たりの結果へ寄せる．これはMasterとの照合後にPrototypeで検証する提案である．

### 直接輸入しないもの

- Target全消去後のRandom再配置は，PlayerがSocket配置と反射経路を育てる本作の中核を弱めるため，そのまま導入しない．
- Ascension／Prestigeは復活させない．Roomごとにゲームが変わる感覚だけを，同一盤面上の永続的なPhase解放とOverdriveで再構成する．

## 今回確定した運用判断

- `docs/coordination/mac/`と`docs/coordination/windows/`へHost別の追記型Handoffを置く．
- HandoffはMasterより下位であり，実装入力へ自動昇格しない．
- 各Hostは相手HostのFileを編集せず，自Host側へ新規Fileを追加する．
- 会話全文ではなく，結論，根拠，代案，検証結果，未決事項，次の依頼を保存する．

## 検証

- 実行したCommand：Git status，local／remote `main` commit照合，Repo文書確認．
- 成功したTest：なし．今回はUnity Codeを変更していない．
- 未実行項目：Windows上の`.\scripts\unity.ps1 verify`．
- 既知問題：Windows側のUnity実機検証結果はまだGitHubへ戻っていない．

## 変更File

- `AGENTS.md`
- `docs/coordination/README.md`
- `docs/coordination/HANDOFF_TEMPLATE.md`
- `docs/coordination/mac/2026/09/05/0445-cross-host-sync-and-gumball.md`

## Windowsへの依頼

1．`main`をPullし，`AGENTS.md`と`docs/coordination/README.md`を読む．
2．Unity `6000.3.18f1`で`.\scripts\unity.ps1 verify`を実行する．
3．Compile，EditMode，PlayMode，Windows x64 Build，ZIP，SHA-256の各結果を記録する．
4．結果を`docs/coordination/windows/2026/09/05/`以下の新規Handoffへ保存し，Commit，Pushする．
5．失敗時は最初の原因を記録し，Prototype 0境界や`FIXED`値を変更して迂回しない．

## User判断が必要な点

- Active Lineage上限の三案は，Prototype 0のWindows実機確認後に比較検証へ進めるか判断する．
