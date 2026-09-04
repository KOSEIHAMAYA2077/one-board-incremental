# Prototype 1A：自由配置 / ROUTE LAB

唯一のGame Logic正本はMaster v0.4.0．このBriefはUserが承認した次の試作範囲を限定する．Game Versionは0.2.0-placement，Unity 6000.3.18f1，Windows x64．

## 目的

装置を置き直し，Mirrorを回転させ，撃って経路と報酬を確認できる一つの試作を作る．Userの趣味制作・AI駆動制作・将来のポートフォリオという目的を優先する．多人数の評価や厳密な経済実験を今回の完了条件にしない．

## 今回の範囲

- 1600×900論理座標，16:9等比表示，銃(800,820)，速度900，半径8，Reload 0.65秒，距離散布2〜14度，Seed 20260828．
- 支給済みCollector二個（円，直径100，初期中心800,220と1200,360），Mirror一個（18×220，460,580，論理角-5度），Amplifier一個（96×56，800,480）．寸法・配置は今回のINITIAL値．
- Collector Base Goldは初回Upgrade購入済み相当の2．直撃2，反射3，増幅4，反射＋増幅6．通貨はGoldのみ．
- 通常弾はCollectorで吸収．Collectorの命中FeedbackはColliderや論理報酬を停止させない．
- Mirrorは反射回数6の範囲で反射．反射報酬は初回1.5倍，異なる面二個以上で上限2倍．今回はMirror一個のみ．
- Amplifierは通過させ，その弾の次の報酬を2倍にする．同じ一発で同じAmplifierの効果は一回だけ．
- 配置領域X300〜1560，Y140〜750，20px吸着，重なり判定の余白2px．盤面・UI・銃への侵入禁止．
- B / Tabと画面ボタンで編集・再開．ドラッグ移動．MirrorはQ/Eまたはホイールで5度回転．編集時のみ中心射線の予測とMirror法線を表示．実弾には散布があり予測は保証ではない．
- 編集時はSimulation停止，既発射弾は終了．再開操作を射撃へ伝播させない．Escはドラッグ取消，または編集終了．
- 配置をLocal JSONへ保存し，保存ごとに日時付き履歴も残す．再起動で有効な配置だけ復元する．GoldはSessionごとに0へ戻る．
- 各実行のLogを別Fileへ保存．Build，Test，ZIP，SHAも実行ごとの新規Directoryへ保存．

## 入れない

Material，購入経済，Recipe，貫通・分裂，Core，Auto，装置同士の物理的な押し合い．装置は固定設備．

## 保存と完了条件

Prototype0.unity，旧Windows実行Fileと過去Artifactを保持．FreePlacement.unityとOneBoardRouteLab.exeを追加する．

配置の境界・円矩形と回転矩形の重なり，実Colliderと表示形状，反射と増幅，連続Collector命中をTestする．EditMode / PlayModeとWindows Buildを実行し，Userが起動できる成果物と操作説明を渡す．
