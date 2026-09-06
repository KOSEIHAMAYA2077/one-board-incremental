# Momentum Lab 0.6.1-drag — 遊べる試作保存版

試作して遊んだ結果、当初思い描いた方向との違いを感じたため、開発はいったん停止しています。これは完成品ではなく、手触りを体験できる保存版です。

## ダウンロードして遊ぶ

**[Windows版をダウンロード（ZIP・約35 MB）](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/download/v0.6.1-drag/OneBoardMomentumLab-v0.6.1-drag-Windows-x64.zip)**

1. ZIPをダウンロードし、右クリック →「すべて展開」。
2. 展開先の `OneBoardMomentumLab.exe` をダブルクリック。
3. マウスで盤面を狙い、左クリックで一マガジン発射。発射中も狙いを変えられます。

Windows 10／11・64bit向け。Unityのインストール、ソースコードの取得、ビルド操作は不要です。ZIPの中から直接起動せず、DataフォルダーとDLLを含めて展開してください。Mac・スマートフォン・ブラウザではこのexeを実行できません。

未署名の試作のためWindowsに警告される場合があります。保護機能は無効にせず、配布元とファイル名を確認してください。起動できなければ警告内容を添えて報告してください。

## 最初の1分

- まず緑の的を狙ってクリック。青いゾーンを通すと加速し、紫のバンパーで反射します。
- `R`／ホイールで銃を切替。Shotgunは散弾、Sniperは太く速い一発。UZIは40 Goldで開放します。
- 右側で効果を付け替え、狙いや銃による違いを試せます。B缶を取得した弾でバンパーに当てると換金できます。
- 全破壊したら画面の案内から次のStageへ。全3Stage。残弾待ちは右側の回収ボタンで切り上げられます。
- `Q`／`E`で照準、`Space`で発射も可能。左上の`?`は説明、`≡`は音量・テーマなどの設定。`Esc`で終了確認。

進行は端末へ自動保存されます。新しい試遊用セーブを同梱しているわけではないため、以前遊んだ端末では既存進行が復元されます。セーブを削除する必要はありません。BGMは未実装です。

## 保存元と検証

- Build source：`56bfa5d4d3805113bef6e89719a7067bf17da375`。タグ `v0.6.1-drag` はこのソースを指します。
- Unity：6000.3.18f1。既存の検証済みZIPを再ビルドせず配布。
- 作成時の検証：Compile、EditMode 102/102、PlayMode 35/35、Windows x64 Build、通常／Arsenalの自動起動診断に成功。今回の公開に際してZIPのSHA-256を再確認。
- SHA-256：`7236cb91f26cfa8d3df4ba6b48e9e389bc927caaf7768292f006651ecb6b72cb`
- 未検証：Mac、Web移植、他のPCでの動作。正式製品版の品質保証ではありません。
- [調整・検証記録](https://github.com/KOSEIHAMAYA2077/one-board-incremental/blob/feat/portrait-stage/docs/coordination/windows/2026/09/06/1925-stronger-drag-and-extraction-brainstorm.md)

GitHubが自動表示する `Source code (zip)` は開発用ソースです。遊ぶ場合は上のWindows版ZIPを選んでください。旧Releaseは削除・上書きしません。
