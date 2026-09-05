# Challenge Arsenal 0.5.0-challenge

開発中のWindows試作。旧8版を保持し，mainにはまだ統合しない。

## 遊び方

- 盤面クリック：1マガジンを撃ち切る。飛行中にReloadし，全弾が終了したら次を撃てる。
- Space／右の残弾回収：未発射分と残弾を終了。使用したマガジンは戻らない。
- 初期はRevolver6発。GoldでUZI18発を開放。右の効果を容量20pt内で無料付け替え。
- 初速と抵抗軽減は最初から装備。分裂と威力を購入可能。購入後はもう一度押して装備する。
- 子弾も分裂可能。斉射全体10秒，同時256弾・累計1024生成を上限とし，上限時は親弾の飛行を継続。
- 1Challengeは初期3マガジン。全敵破壊で次Stage開放，失敗でもGoldを保持。左のStageボタンで再挑戦・再訪。
- 携行数はGoldで4／5へ強化可能。次Challengeから適用。銃の弾数とは別。
- 各Stageの1マガジン全破壊でGolden／3体目爆発／共鳴を恒久開放。通常進行にMasteryは必須ではない。
- 構成Preset3枠。RでPause，F2で旧図形表示と比較。飛行中の構成変更は不可。

## 保存

新規challenge-v1.jsonにGold・購入・到達Stage・Mastery・構成を保存。旧magazine.jsonを変更しない。前のSaveはchallenge-historyへ保持。途中Challenge・飛行状態は保存せず，再起動時はStage1の新Challenge。保存エラー時は画面に警告し，未知／破損した元Fileを上書きしない。

## 生成物と検証

- Source：`27be90f55280f82d2ed893d1260016def4afe96c`
- Unity：6000.3.18f1
- Command：`.\scripts\momentum.ps1 verify`
- Compile成功，EditMode66/66，PlayMode25/25，Windows x64 Build／ZIP／SHA-256成功。
- Artifact Root：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-143627-4569252`
- EXE：`Windows\OneBoardMomentumLab.exe`
- ZIP：`OneBoardMomentumLab-v0.5.0-challenge-Windows-x64.zip`
- SHA-256：`cd8a6e14165f639b1b41c83e335e49a9828b845d3fb642d3144e43576f0bf30b`
- 可視ウィンドウ診断：1920×1080，通常6発・分裂18発ともExit0，Pause成功，終了時の弾・演出0。文字Overflow0。取得画像で明るい3:4盤面と12個の的，左右UIを確認。
- 分裂の可視診断はPeak43弾。純粋Simulationの別Seed検査でPeak244弾・13世代まで確認。256弾の論理上限と親弾維持は専用Testで確認。非表示実行のFrame値は描画性能の証拠にしない。
- 未検証：Mac，長時間Play，最大密度256弾での可視描画性能，手動の全購入／全Preset操作，実ファイル破損・ディスク満杯の強制試験。JSONの往復と不正値拒否は自動Test済み。

## 初期Balanceの観察

決めた照準手順による合成Playで，初期Revolverは30SeedすべてでStage1を3マガジン以内にClear。育成済みUZI＋購入可能4効果で1マガジンを加速Zoneへ狙った30Seed比較は，Stage1が9/30，Stage2が2/30，Stage3が1/30。人間のPlay評価や最適構成探索ではない。火力・価格・Mastery難度はINITIALで，今後のUser感想で調整する。

## 既知の制約

- 立体感はMeshの厚みと傾き・床影によるもの。Cameraは論理平面の真上であり，本格的な屈折／Bloomは未導入。
- 演出用破片は192個，軌跡は18点。3銃目・Ascension・ランダムPerkは入れていない。
- 全中間Buildを削除していないが，GitHub Releaseへ添付するのはこの最終成功Buildだけ。
