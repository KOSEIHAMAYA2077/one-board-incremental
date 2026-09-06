# MOMENTUM LAB：速度を使い切る三発マガジン

## 最新Patch：0.6.1-drag

Master v0.8.1 §33.11。Challengeの命中時速度損失を2倍（通常300／装甲840，抵抗軽減時75／210），速度300超の時間減速を180/sへ変更。初速・銃・分裂・壁・加速ゾーン・低速880/s・Saveは保持。HUDで比較値を表示。旧Labと0.6.0のBuildを保存する。新しい360度採掘・持ち帰り案はHandoffのみ。

検証：命中前速度でDamage適用後に強い減速，装甲／通常／抵抗軽減／旧Labの8ケース，300境界の時間分割と停止0.25秒，旧Test全体，旧版と同Seed・同射線の4銃×2装備×8Seed×3射線の観察（楽しさの判定ではない），Windows BuildとFHD表示。

## 最新Patch：0.6.0-arsenal

Master v0.8.0 §33.10。Shotgun8粒同時／Sniper太い高速1発を初期使用可能にし，R・ホイールで次マガジンの銃を切替。Q/E照準・Space発射とMouse自動切替。B缶は取得弾だけに条件付きバンパー換金12Gを付け，子弾へ継承しない。缶破壊で一時ゾーンを作る案は未実装。? Help，設定Menu，Esc終了確認，SE音量，3表示Theme，BGM未実装表示。旧Saveを読むだけの新保存先を用い，全旧版を保持する。新数値・状態・操作・保存仕様は§33.10を参照。

受入条件：同時8粒・1マガジン消費，Sniperの上限／高速Swept Hit，射出中切替の非遡及性，Bの取得／1回換金／壁／回収／子継承なし／子の独立取得，100Seed配置，銃Preset JSON，Mouse・Keyboard・Pauseの境界，Menu裏誤操作防止，SEと表示状態の非干渉，既存全Test，Windows BuildとFHD通常画面・Help・設定・終了確認の描画確認。今段階では外部Asset，BGM作曲，多種缶，Skill Treeや周回経済を追加しない。

SEは10%刻みの増減・消音Button。診断は文字Overflowに加え，Modal四隅の実画素を確認し，全面の描画も目視する。画像Previewで疑った設定画面の欠落は，保存画像のModal領域の画素比較では再現しなかった。試験的なGraphics API／Jobs変更は撤回し，従来のProject設定を継続する。詳細はHandoffに記録する。

## 最新Patch：0.5.5-steer

Master v0.7.5 §33.9。Revolver6発は0.4秒間隔，UZI18発は0.12秒間隔。一クリックで撃ち切り，射出中に盤面内の有効な照準を更新して未射出弾へ反映する。発射済み弾を曲げない。UI／盤面外／銃の近傍では最後の有効方向を保持。Pause・Focus・クリック防止の入力境界を維持。弾速・減速・弾数・親子寿命・Save・旧Labは変更しない。検証は両銃の時刻／弾数／Reload，照準変更の非遡及性，無効入力，Pause，入力列再現性，Controller接続，既存TestとWindows描画。収益ギミックはHandoffにある未採用案のみ。

## 最新Patch：0.5.4-brake

Master v0.7.4 §33.8。Challengeの低速域300以下だけ880/sで減速し，300→停止80を約0.25秒へ短縮。高速域は120/s，境界をまたぐTickは時間を分割する。再加速時は速度に応じて通常減速へ復帰。低速域の弾・尾を収縮し，消滅後は最大0.12秒の非攻撃光のみ。再射撃条件・Save・初速・抵抗・分裂・旧Labと旧Buildは維持。受入Testは複数Tick幅の停止時間／距離，境界，高速域不変，Pause，最後のHit，再加速，表示収縮と余韻の非干渉，既存Test，Windows実描画。

## 最新Patch：0.5.3-crystal / Lucent Kit V1

Master v0.7.3 §33.7。半透明のOriginal 3Dアセット一式と独立Gallery，HP／Goldenの表示変化，浮遊回転を実装。旧Game Logic／Save／Buildを保持。F3は旧Neon Meshとの比較，F2は旧図形表示。Galleryの過充電・凍結は表示見本のみ。検証はMeshの面数・有限値・外向き法線，Prefab再利用と共有Material非破壊，Game状態非干渉／Pause，既存Test，Windows実描画とFHD可読性。

## 最新Patch：0.5.2-clear

正本はMaster v0.7.2 §33.6。Clear確定時の中央案内にCLEAR・結果・次へ・もう一度を表示。最終Stageの次へは無効化し，全3Stage達成を明示。左右の強化操作は継続可能。遷移の誤射防止と再挑戦／次Stage／最終StageをPlayModeで確認し，FHDのClear画面を診断撮影する。敵の追加強化・Stage素材・周回難化は候補メモに留め，今回は実装しない。

## 最新Patch：0.5.1-tempo

正本はMaster v0.7.1 §33.5。以下の0.5.0の全弾待機のみを更新する。

- Reload完了かつ全生存弾の速度300以下で再射撃可。残弾は攻撃継続，旧マガジンの10秒寿命と1024生成予算は独立。全マガジンで同時256，次マガジン全弾分を予約する。
- 300ちょうど可，1発でも超過なら不可。子弾・再加速も判定。予約射撃なし。全敵撃破後の無駄撃ちを防止，決着／構成変更／回収の扱いはMaster参照。
- HUDに再射撃条件とREADYを表示。旧仕様・Build・Saveを保持。Coreで境界・Reload・寿命・生成予算・同時数，PlayModeで低速残弾中の発射とUI誤射を検証し，Windows Buildを作成。

## 最新実装：0.5.0-challenge

現在の正本はMaster v0.7.0 §33。以下の旧版記述は復元用として保持し，競合する現在仕様は§33を優先する。

- 実装範囲：3:4の広い中央Stage，小さい的12個，奥のバンパー，明るい結晶面・命中／破壊演出。
- 全弾待機と飛行中Reload，Space回収，親子全体で10秒寿命。子も分裂可，世代上限なし。256同時／1024生成上限時は親弾継続，UIに制限表示。
- Revolver6発とUZI18発，全弾Modifier7種類，容量20，無料付け替え，Preset3個。
- 3Stage，初期3マガジンChallenge，携行上限と基礎火力のGold強化，1マガジンClearの特殊効果，永続Saveと旧Save保護。
- 数値・効果継承・失敗判定・価格・保存仕様は§33.1〜33.4。大規模な成長曲線やAscensionは含めない。
- 検証：旧Core Test維持，100Seed×3Stageの全数配置，残弾待機，回収の未射出取消，HP継続，最後のマガジン解決，Mastery，容量，Preset／Save validation，再分裂・寿命継承・上限，Golden二重報酬防止，爆発非再帰。PlayModeで表示不変・UI誤射・Pause，Windows Build，FHD診断画像と文字Overflowを確認。

## 旧版の復元用仕様

Master v0.6.0 §32が今回のGame Logic正本．旧章との競合は§32を優先するが，旧Prototypeへ遡及適用しない．Game Version 0.4.0-momentum，Unity 6000.3.18f1，FHD Windows x64．

## 今回実装するもの

- 一クリックで三発を0.12秒間隔で発射し，撃ち切り後0.8秒Reload．照準はクリック時に固定，各弾にSeed付き±2度散布．長押し予約なし．残弾があってもReload後は次のクリックを受ける．
- 三Slotを通常／貫通から選ぶ．初期は通常・貫通・通常．編集中はPauseし，既存弾は保持．発射中のマガジンSnapshotは変わらない．
- 速度900，時間減速120/s，速度80以下で消滅．最大1800．半径8．基礎威力通常40／貫通26，抵抗係数1／0.25．数値はすべて今回のINITIAL．
- 普通の的4個（HP50，抵抗150，破壊Gold3），装甲的3個（HP90，抵抗420，破壊Gold7）．円半径34／42．命中ごとにHPを削り，破壊時一回だけGold付与．
- 初期盤面と破壊済みの的はSeed付き乱数で生成．補充は次の斉射開始時だけ．生存Target・障害物は動かさない．残弾の直近を避けて補充し，配置候補がない時は保留．
- 固定外壁とランダム初期位置の円形障害物2個（半径26）が反射させる．反射時速度×0.9，反射報酬倍率なし．的は反射させず，減速後の速度が閾値を超えれば通過する．
- 移動する加速ゾーン1個（半径65）．中心X=940+320sin(0.8t)，Y=550．速度×2，最大1800，同じ弾には一回のみ．時間停止時はゾーンも止まる．
- ダメージ＝基礎威力×min(命中直前速度/900,2)．その後に速度から抵抗×弾係数を引く．一回の接触中に連続Hitさせず，外へ出て再訪したら再びHitする．
- 1600×900論理盤面．Play領域X320〜1560,Y120〜760，Gun(940,725)．FHD文字基準は旧表示Patchを継続する．
- Pure C#の明示Swept Circle検索（的，障害物，壁，移動ゾーン），60tick/s．一弾一Tick衝突8まで，残時間持越し．同時接触はTarget，障害物，壁，ゾーン，ID順．
- 弾種・速度・HP・抵抗・ダメージ・破壊報酬・弾倉・Reloadを見えるようにする．飛行の軌跡は直近部分だけを描く．
- マガジンをVersion付きLocal JSON＋日時履歴へ保存．セッションLogは個別File．Goldとランダム盤面は起動ごとに初期化し，Seedを表示する．送信なし．
- 新Scene MomentumLab.unity，新実行File OneBoardMomentumLab.exe，製品名One Board Momentum Lab．Artifacts/Momentum/<日時>/に検証・Build・ZIP・SHAを保存．

## 表示比較Patch 0.4.1-neon

User承認のMaster §32.1に従い、ネオン発光と3D結晶Meshを追加する。ゲーム挙動・数値・音は変更しない。F2で従来表示と比較できる。現時点の対象Buildは0.4.1-neonで、上記0.4.0のGame LogicとSave互換を保持する。外部購入Assetは使わず、生成Meshと専用Shaderで試す。検証は旧Test一式、描画切替の論理不変、Mesh奥行き、演出消滅、Windows Build、FHD画像確認。

## 縦長三列Patch 0.4.2-portrait

最新対象はMaster §32.2。FHDの中央に9:16の実盤面、左に実効DPS/威力/直近ログ、右に弾倉編集と未実装の強化領域を置く。旧0.4.1の見た目を維持し、Geometryのみ新Layoutへ変更。DPSは5秒窓の実HP減少÷5でOverkill除外、Pause中は窓も停止。旧横長Layout/Test/Buildを保持する。音やBalance数値、Save形式は変更しない。追加受入条件は100Seedの重なりなし配置、縦長壁反射、UI誤射防止、DPSのOverkill/窓/Pause、FHD全画面の可読性。

## 今回入れない（継続）

ゴールド／カオス等の追加ゾーン，爆発・分裂・裂傷弾，銃の購入／強化，Material，Core，手動配置．これらはメモに残すだけ．

## 完了条件

通常／貫通の減速差，速度倍率と上限，停止弾の最後のダメージ，時間減衰，ゾーン一回と相対移動衝突，壁反射，再訪Hit，Seed再現性，重なり回避，三発SnapshotとReload，編集PauseのTest．旧Testも維持．Windows BuildとStandalone診断を行い，Userへ操作を渡す．
# 配布補足：Web試遊版（2026-09-06）

User承認により、休止中の0.6.1-dragをGitHub Pagesで試遊可能にする。ゲーム仕様・数値の変更ではなく、Web用日本語Font、ブラウザ保存、終了案内、ビルドと配布の対応に限定する。Windowsの既存Build／Saveは保持。範囲と制約は`docs/WEB_DEMO.md`を参照。
