---
handoff_id: windows-20260905-1630-lucent-crystal-kit
host: windows
created_at: 2026-09-05T16:30:00+09:00
base_commit: e184877e6c0a452d83d6c1687238e54c5a700c57
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-1531-main-readme-navigation
  - mac-20260905-0457-windows-verification-response
---

# Lucent Crystal Kit V1：半透明3D素材と独立展示室

## 今回の目的

既存のMomentum試作を保持し，半透明の幾何結晶を敵・バンパー・銃身・筐体へ展開する。完成ゲームの大規模な設計変更ではなく，取り外せる表示試作と再利用可能なAssetを制作する。

## 会話からの開発記録

### Userの指示・発想

- 要約：Rez Infinite，REVOLVER360 RE:ACTOR，Geometry Warsのような光る幾何形状と，3Dを2D的に眺める表現を試したい。角度は30〜40度程度でもよい。あとで採用しなくてもよく，旧試作は消さない。
- 要約：半透明のクリスタル感。敵，バンパー，銃身，筐体を制作し，ダメージやスキル影響によって色を変えたい。
- 追加指示：四面体・八面体・いがぐりに加えて，16面体・32面体・球体など形状を増やしてよい。
- 最新の動きに関する入力には変換の乱れがあった。Assistantは「ふよふよ浮遊して，ゆっくり回転」と解釈したことを伝え，その表示を実装した。Userによる文言の再確認済みとは扱わない。
- 今後のHandoffにはUserとAssistantの指示・提案・発想を簡潔に分けて残し，開発方針を辿れる記録にしてほしい。

### Assistantの提案

- 透明な外殻，発光する内部Core，輪郭線を組み合わせる。HPは外殻，状態効果はCoreとRingに分け，両方を同時に表示する。
- Asset単体を確認できる独立Galleryを作り，不透明度・HP・状態色・傾きを操作可能にする。ゲームのSaveには触れない。
- 見た目だけを既存のSimulationへ接続し，旧描画も比較用に残す。購入素材を流用せず，MeshとShaderを新規生成する。

### 採用・保留・変更

- 採用：11 Prefab，敵の表示上の形状バリエーション，緩やかな浮遊・回転，HP／Golden連動，Gallery，Unity Package，OBJ8個。
- 採用：Galleryは35度を初期値に調整可能。Gameは判定とAimを保つためCameraを傾けず，個体Meshを35度傾ける。盤面全体の斜め視点化とは区別する。
- 採用：今後の開発記録欄をHandoff Template・coordination README・AGENTSへ追加。
- 保留：物理的屈折，URP／HDRP対応，過充電・凍結の新Game Logic，経済やStageの追加。過充電・凍結はGalleryの表示見本のみ。

## 読んだ正本

- `AGENTS.md`
- `docs/MASTER_GAME_SPECIFICATION_2026-08-28.md`（今回0.7.3，§33.7を追加）
- `docs/AI_MOMENTUM_LAB_IMPLEMENTATION_BRIEF_2026-09-05.md`
- `docs/coordination/README.md`，`docs/coordination/HANDOFF_TEMPLATE.md`

## 実施内容

- CrystalMeshFactoryで閉じた立体Meshを生成。四面体，八面体，20尖端のいがぐり，16／32三角面の双角錐，多面球，滑らかな球，Ringを制作。
- 11 Prefab：Tetra，Octa，Urchin，Hexadeca，Triaconta（32面），FacetedSphere，SmoothSphere，Bumper，Launcher，Cabinet，Projectile。
- Unity自身でMesh／Material／Prefab／Sceneを保存し，Assetとmetaを一緒にCommit。基本Mesh8個をOBJでも保存。
- 両面透過・面の明暗・縁光・反射Highlightを持つGlassと，金属／発光Shaderを制作。MaterialPropertyBlockで個体の外観を変える。
- GalleryにHP・不透明度・傾き・状態色の操作，浮遊回転停止，命中Flash，短時間の破片散布，拡大表示を追加。
- Gameを0.5.3-crystalへ更新。F3で新Kit／旧Neon Mesh，F2で旧図形を比較。判定円，Aim，速度，ダメージ，報酬，SaveなどGame Logicは変更しない。
- Gameの小さな球体で細線が点々に見えたため，多面球のWireをゲーム表示だけ省略し，線の太さと外殻の不透明度を調整。Galleryは細部確認用のWireを保持。
- `scripts/crystal-kit.ps1 verify`でゲーム検証，Gallery Build，Unity Package，OBJ，案内書，ZIP，SHA-256をまとめて生成。

## 得られた結果

- Source Commit：`e184877e6c0a452d83d6c1687238e54c5a700c57`。制作Commitは`2a00fa1f1f202f159416ba23313b8fd6e877000a`，その後Gallery筐体の傾き適用を修正。
- 作業Branch：`feat/portrait-stage`。今回のSourceと本HandoffはLocal Commitのみ。Push，Tag，GitHub Release，mainへのゲームMergeはしていない。
- 旧Scene，Asset，Build，Tag，Saveを削除していない。新しい成果物は日時別Directoryへ保存。

### 生成物の絶対Path

- 展示室：`C:\Dev\one-board-incremental\Artifacts\CrystalKit\20260905-162337-1350284\Gallery\LucentCrystalGallery.exe`
- 再利用Package：`C:\Dev\one-board-incremental\Artifacts\CrystalKit\20260905-162337-1350284\LucentCrystalKit-v1.unitypackage`
- 展示室＋Asset一式：`C:\Dev\one-board-incremental\Artifacts\CrystalKit\20260905-162337-1350284\LucentCrystalKit-v1-Gallery-and-Assets.zip`
- 上記ZIP SHA-256：`0408e73261aaad76f0348c14c9a8d3376816112b1b8f361751d45dd84db9f648`
- ゲーム：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-162337-1513972\Windows\OneBoardMomentumLab.exe`
- ゲームZIP：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-162337-1513972\OneBoardMomentumLab-v0.5.3-crystal-Windows-x64.zip`
- 上記ZIP SHA-256：`a1b50396fe448e342d813a8cf96b3b151acb8f74377d75887d7d4d579865f056`
- Gallery画像5枚：`C:\Dev\one-board-incremental\Artifacts\CrystalKit\20260905-162337-1350284\Capture\`
- Game画像・自動動作結果：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-162337-1513972\CrystalSmoke\`
- 分裂連鎖確認：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260905-162337-1513972\CrystalStress\`
- 空ProjectへのImport検証Log：`C:\Dev\one-board-incremental\Artifacts\CrystalKit\20260905-162337-1350284\import-verify.log`
- 使用案内：`C:\Dev\one-board-incremental\docs\LUCENT_CRYSTAL_KIT_V1.md`

## 設計判断

### 確定

- Master §33.7の表示試作範囲のみ。CoreへUnity依存を追加せず，形状から能力や判定を変更しない。
- HPと既存Goldenは実Game Stateへ連動。浮遊・回転は表示だけ。Handoffは引き続き参考資料であり正本ではない。

### 提案

- Userの見た目の評価後，明るさ，線の密度，筐体の存在感を調整する。別方向ならKit V2としてV1を保存する。
- 次にPush／Releaseする場合は，既存の未送信0.5.1／0.5.2も含むBranch差分を確認し，main READMEの配布案内を更新する。

### 棄却または保留

- 本Kitは軽量な透明表現で，物理的屈折や透明物体の完全な描画順保証はしない。URP／HDRPは未検証。
- 新しい状態色をそのまま新能力の実装と扱わない。Game Loop，Balance，Stage，Saveの変更を今回へ混ぜない。

## 検証

- 実行Command：`.\scripts\crystal-kit.ps1 verify`（内部で`.\scripts\momentum.ps1 verify`），Unity 6000.3.18f1。
- Compile成功，EditMode 74/74，PlayMode 30/30，計104/104。Windows x64 Game／Gallery Build，ZIP，SHA-256生成成功。最終ZIPのHashを再計算して一致確認。
- Gallery自動Capture：Exit 0，Assets=11，Overflows=0，NoSaveSystem=True。5画像生成。基本セット・追加形状・筐体の画像を目視し，透過，奥行き，文字の配置，画面の正常描画を確認。
- Game自動Capture：Exit 0，FHD，6発，4加速，最終Gold44，UI overflowなし。フルUI画像を目視。診断起動はSaveを変更しない。
- Game分裂Stress：Exit 0，18発，15加速，最大同時43弾，分裂抑制0。診断区間平均4.17ms／最大4.90ms。これは当該短時間の診断値であり，最大256弾の性能保証や長時間Play結果ではない。
- 空のUnity 6000.3.18f1 ProjectをArtifacts内に作成し，PackageをImport後，11 Prefabを生成してMesh／Material／表示ComponentとGolden適用を確認。Exit 0。ゲームCore／Galleryなしで利用可能。
- `git diff --check`，追跡対象の生成Directory不在，Assetのmeta存在を確認。
- 開発中の最初のCompile失敗はVector2とVector3の加算の型曖昧性。明示的な型変換で修正。
- その後の実画像でBloom追加後の盤面が灰色になる不具合を発見。Shaderの入力TextureがProperties未登録だったことを修正し，灰色一色のCaptureを失敗扱いにする検査を追加。修正後の最終画像は正常。失敗した中間Buildも保存している。
- 未実行：Mac検証，長時間Play，全ボタンの手操作による網羅確認，色覚特性の本格評価，URP／HDRP，256弾最大負荷の性能検証。
- 既知の制約：透明物体の重なり順，真の屈折なし，小さい表示で形状細部が見えにくい場合がある。正16／32面体という分類ではなく，指定面数の双角錐を使用。

## 変更File

- `Assets/_Project/Presentation/Crystal*.cs`，`MomentumCrystalKitView.cs`，`LucentBloom.cs`
- `Assets/_Project/Presentation/MomentumNeonView.cs`，`MomentumLabController.cs`，`MomentumPortraitHud.cs`
- `Assets/_Project/Editor/CrystalAssetBuilder.cs`
- `Assets/_Project/Tests/PlayMode/CrystalKitTests.cs`
- `Assets/_Project/Scenes/CrystalGallery.unity`，`Assets/Resources/CrystalKitV1/`，`Assets/Resources/Lucent*.shader`（それぞれmetaを含む）
- `ArtExports/CrystalKitV1/Shape0.obj`〜`Shape7.obj`
- `scripts/crystal-kit.ps1`，`scripts/momentum.ps1`
- `AGENTS.md`，Master，Momentum Brief，`docs/LUCENT_CRYSTAL_KIT_V1.md`，coordination README／Template
- 本HandoffはSource変更とは別Commit。

## 次のHostへの依頼

1. UserにGalleryのHP・状態色・不透明度・回転を触ってもらい，採用する見た目を確認する。旧Asset／Buildを消さない。
2. PackageはBuilt-in向け。別Pipelineへ持ち出す場合はShader互換を別途検証する。
3. 今後のHandoffでも指示・提案・採用／保留を分ける。曖昧な入力の解釈は確定したUser発言へ書き換えない。
4. GitHubへ共有する場合はPushの対象Branchと未送信Commitを確認する。mainのREADMEだけは既に進んでいるため，古いBranch側READMEで上書きしない。

## User判断が必要な点

- 制作・Local検証は完了。見た目の採否と，今回のKit／ゲームをGitHubへPush・Release保存するかは未決定。
