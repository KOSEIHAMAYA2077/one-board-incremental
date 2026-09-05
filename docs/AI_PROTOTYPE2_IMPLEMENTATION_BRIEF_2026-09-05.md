# Prototype 2：五発Recipe / RECIPE LAB

Master v0.5.0の§7〜12を正本とし，Userの「次の段階へgo」「弾5発版」指示を対象範囲とする．Unity 6000.3.18f1，Game Version 0.3.0-recipe．P0とP1AのScene・Build・履歴は保持する．

## 今回の範囲

表示修正Patch `0.3.1-recipe`：FHD 1920×1080基準，本文約19px・補助約16px・見出し約25px．論理盤面1600×900は維持．文字は画面の整数Pixelサイズで描き，日本語行高を表示枠内へ収める．旧Build・Save互換・Game Logicは変更しない．

- P1Aの自由配置・回転・当たり判定・射撃感を継続する．Collector二個，Mirror一個，Amplifier一個．Base Gold 2．初期配置もP1Aと同じ．
- 通常／貫通／分裂の循環五Slot．初期Capacity 4，Cost 0/2/3，Primer 0〜2．初期Recipeは通常・通常・通常・通常・分裂．全弾支給済みで購入待ちはない．Capacity比較用追加ボタンは入れない．
- Rで編集．Slotクリックで弾種を循環変更，全Slot一括設定，一回Undo．超過時は確定不可．飛行弾を保持して時間停止し，閉じる入力は射撃へ伝播させない．
- NOW／NEXT／残り三発，Primer，装填進行，次周期適用を表示．初回発射前のみ即時適用．
- 貫通は倍率1.25，通過回数2＋Primer．Collector通過のみ回数消費．残り0で次のCollectorへ当たると報酬後に吸収．
- 分裂は2＋Primer子弾，合計30度，子も分裂弾，Primerと残反射・倍率を継承．深度3以降は子を生成せず親を終了し，抑止数をLIMIT表示．未詳細部分はMasterに今回のINITIALとして明文化する．
- 子はLineageのReward／Effect Visitedを共有．Mirrorは各弾の反射履歴と残回数を継承．AmplifierのToken付与後に分裂．報酬未取得Targetの命中だけToken消費．
- 同じColliderから抜けるまでの一時的な再接触抑止と，一Lineage一回の報酬制限は別物．訪問済みCollectorでも物理処理は行う．
- Root込み生成64，反射6，深度3，一Tick各弾衝突8・残距離持越し．同Tickに生じた子は親の残移動距離だけ進む．Spawn順で安定処理．
- 命中ごとのGoldと，全子弾終了時のLineage合計を分けて表示する．配置編集による終了時も一回だけ合計確定．
- 配置とRecipeは別のVersion付きJSONとして日時付き履歴を保存．RecipeのActive／Pending／Index／Cycleと発射済み状態を保存するが，飛行弾は復元しない．GoldはSession単位．
- 新Scene RecipeLab.unity，新実行File OneBoardRecipeLab.exe，新製品名で旧版の保存先と分離する．Artifacts/Recipe/<日時>/にBuildと検証結果を追加保存する．

## 範囲外

Material，購入経済，Core，Auto，新弾種，反射パズルへの転換，オンライン送信．

## 完了条件

Recipeの循環／境界適用／Primer／Capacity，貫通，分裂，Visited，上限，編集停止を自動Test．旧試作のTestも継続実行．Windows Build・ZIP・SHA-256を作り操作説明を渡す．多人数の面白さ調査を条件としない．
