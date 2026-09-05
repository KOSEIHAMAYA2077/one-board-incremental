# Momentum Lab — Neon Crystal表示比較版

Version：0.4.1-neon  
Commit：b17fed20df60d3820536ba35d6cc387d1631b71e  
Unity：6000.3.18f1

## 検証目的

面白さを確認できた0.4.0の挙動と音を維持し、参考画像の暗い盤面・ネオン輪郭に、ほぼ真上から見る3D結晶を組み合わせる内部試作。

## 操作

- 左クリック：三発マガジンを発射。
- R：弾倉編集・停止／再開。
- F2：ネオン結晶表示／旧表示を即時切替。
- AssetsのOneBoardMomentumLab-v0.4.1-neon-Windows-x64.zipを全体展開し、OneBoardMomentumLab.exeを実行。DataフォルダーとDLLも保持する。

## 今回の変更

- 生成した立体Meshによる面取り結晶。通常的は緑、装甲的は橙、反射障害物は紫。
- 水色の移動加速リング、弾の光の尾と周辺光、命中Flash、消える結晶破片。
- 論理座標とカメラを維持し、結晶Meshを3度傾けた、盤面から87度相当の表示。
- Collider追加なし。Core・既存Audioコードは変更なし。追加の購入Assetなし。
- 旧試作、Build、Tag、Release、失敗時の検証記録は保持。

## Test結果

- EditMode：49/49 Passed。
- PlayMode：24/24 Passed。表示切替の時間・Gold不変、3D Mesh奥行き、描画と論理弾数の対応、破片終了を追加確認。
- Windows x64 Build：成功。
- FHD Standalone：exit 0。fired=3、boosts=3、paused=True、remaining=0、gold=3。Playing／Editorの文字枠はみ出し0。
- RenderTexture画像で盤面を確認。Windows画面操作スキルによる起動中のウィンドウ画像で、UI、結晶、光の尾も確認。
- Mac検証は未実施。正式製品版の検証完了を意味しない。

## 既知問題

- 連続命中時のFloating Numberが重なる既存問題がある。
- 3度の傾きなので側面は控えめ。透明屈折や本格的なBloomではなく、面の明暗と専用発光Shaderによる表現。
- 非表示起動の通常スクリーンキャプチャは黒くなるため、盤面はRenderTexture、全体は起動ウィンドウで確認した。
- 色覚対応・演出軽減の詳細設定は未整備。F2で旧表示へ切替可能。

## Save互換

0.4.0-momentumと同じ製品名・schemaで弾倉設定を共有。配置とGoldは従来どおりセッションごとに初期化。F2は一時的な表示設定で、弾倉や進行を書き換えない。

## SHA-256

`2b31f82eeefde898c47b378712a7688f398bfbdf9c42beeefb9e0ddf1e8cbfd3`

