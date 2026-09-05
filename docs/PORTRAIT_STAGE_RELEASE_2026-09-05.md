# Momentum Lab — 縦長ステージ・三列UI

Version：0.4.2-portrait  
Commit：c5b138a9c978f06e06058cc85fedd6d94cf005e0  
Unity：6000.3.18f1

## 検証目的

FHD横長Windowの中央に9:16の縦長Stageを置き、左のログと右の弾倉/強化領域を見ながら遊べるかを試す。旧横長版は保存する。

## 操作

- 中央Stageを左クリック：三発マガジン発射。
- Rまたは右のボタン：編集Pause。右の各Slotをクリックして通常/貫通を切替。再開もR。
- F2：ネオン結晶と従来の図形表示を切替（どちらも縦長）。
- AssetsのOneBoardMomentumLab-v0.4.2-portrait-Windows-x64.zipを全体展開して、OneBoardMomentumLab.exeを実行する。隣接するDataとDLLを保持する。

## 今回の変更

- 中央：実際の450×800論理座標のStage（9:16）。引き伸ばしではなく、円形の弾・Targetの形と判定を維持し、壁・Gun・配置候補・Zone軌道を変更。
- 左：実効DPS、Gold、実ダメージ合計、命中/撃破/加速回数、弾別の基礎/上限/直近威力、直近7件のログ。
- DPSは直近5 Simulation秒の実HP減少÷5。Overkill除外。Pause中は計測窓も停止。命中威力はOverkillを含む計算値なので区別する。
- 右：三Slotの弾倉編集、Reload状態、将来の強化領域。購入強化はまだ未実装。
- 盤面のHPを大きく表示し、抵抗・大量の命中数値を左へ移した。
- 音、弾速・威力・抵抗・発射間隔・報酬等のBalance値は維持。旧横長Layout/Test/Build/Tagも保持。

## Test結果

- EditMode：54/54 Passed（100Seedの縦長配置、9:16比率、壁反射、実効DPSの5秒窓/Overkill除外/Pauseを含む）。
- PlayMode：24/24 Passed（左右パネルでの誤射防止、表示切替、三発、編集Pauseを含む）。
- Compile、Windows x64 Build、ZIP、SHA-256：成功。
- FHD Standalone：exit0、fired3、boosts3、pausedTrue、remaining0、gold6。Playing/Editorの文字枠はみ出し0。診断ログにError/Exception/Warningなし。
- Windows画面操作スキルで起動ウィンドウを確認し、三列、HP、実際に更新されるDPS/ログの可読性を確認。
- Macと長時間負荷Testは未実施。開発中のPrerelease。

## 既知問題

- 狭い盤面で壁反射が増えるため、横長版と同じDPSや手応えにはならない。
- 短い報酬/加速Popupは最大3件にしたが、集中時には近接し得る。
- 「強化スペース」は配置だけであり、購入や育成は未実装。
- ヘッドレス/非表示の全画面スクリーンショットは黒くなる環境のため、盤面はRenderTexture、全体は実ウィンドウで確認。

## Save互換

従来と同じ製品名・弾倉schemaで0.4.0/0.4.1と設定を共有。Gold/ランダム盤面/統計は起動ごとに初期化。弾倉の日時付き履歴は維持。

## SHA-256

`67765c29917ded503a1015a00345e3f29dbbdfea8b6f49541e4734cca27b23f0`

