# LUCENT / Crystal Studies — Asset Kit V1

2026-09-05。半透明の幾何結晶を使う2.5D表示試作。全形状はコードで生成したOriginal Mesh。購入Assetや参考作品の抽出素材は含まない。

## まず触る

- `Gallery/LucentCrystalGallery.exe`：Saveを持たない独立展示室。名前ボタンで拡大。Tabで2ページを切替。
- 右側でHP・不透明度・傾き・状態色を調整。Spaceで浮遊回転停止，Hで命中Flash，Bで破片散布（約1.2秒後に復帰），Escで一覧。
- Game版は `0.5.3-crystal`。F3で新Kit／旧Neon Mesh，F2で旧図形へ切替。HPの色変化とGoldenは実Game Stateに連動。
- Galleryの過充電・凍結は表示見本。ゲームへ新たな能力を実装したものではない。

## 11 Prefab

| Prefab | 形状・用途 |
| --- | --- |
| Tetra | 正四面体・4三角面 |
| Octa | 正八面体・8三角面 |
| Urchin | 正二十面体の各面を尖らせた20尖端・60三角面 |
| Hexadeca | 八角形を基底とする双角錐・16三角面 |
| Triaconta | 十六角形を基底とする双角錐・32三角面（正多面体という意味ではない） |
| FacetedSphere | 八面体を2回細分化し球面へ投影・128三角面，Flat Normal |
| SmoothSphere | 八面体を3回細分化し球面へ投影・512三角面，Smooth Normal |
| Bumper | 六角断面リング，結晶歯，内部コア |
| Launcher | Pivot筐体，結晶弾倉，二本の加速レール，銃口 |
| Cabinet | 6×8単位の盤面用外枠，金属Spine，半透明Rail，発光Corner |
| Projectile | 八面体の結晶弾頭 |

Unity内：`Assets/Resources/CrystalKitV1/Prefabs/`。Gallery Scene：`Assets/_Project/Scenes/CrystalGallery.unity`。

## 使い回しと色の制御

`LucentCrystalKit-v1.unitypackage`をUnityへImportすると，Prefab・Mesh・Materialと表示Componentが入る。展示室／ゲーム本体は含めない。対象はUnity 6000.3.18f1のBuilt-in Render Pipeline。URP／HDRPでは材質変換が必要で，未検証。

各Prefabの`CrystalAppearance`に外殻・稜線・コアのRendererを設定済み。`Apply(health01, status, flash01, opacity01)`で表示を更新する。健康状態は外殻，状態効果は内部CoreとRingに分離。共有Materialを複製・書換せずMaterialPropertyBlockを使うため，個体ごとに違う色を出せる。

```csharp
appearance.BaseColor = new Color(0.2f, 0.86f, 0.9f);
appearance.Apply(0.3f, CrystalStatus.Golden, 0f, 0.38f);
```

- HPを直接読取・変更しない表示Component。Game側が必要な値を渡す。
- Colliderは含めない。半径やゲーム上の能力を形状から自動決定しない。
- 単体形状は原点中心・概ね半径1。Prefabの子Transformを含めて用途に合わせ拡縮する。
- 基本形状のOBJ8個は `ArtExports/CrystalKitV1/`。Shape0〜7は順に四面体／八面体／星形／面取りPrism／16面体／32面体／多面球／滑らかな球。OBJには材質と動作Scriptを含めない。銃・筐体の組立状態はPrefabに保存。
- 新しい制作方向を試すときはV2など別Folderにし，V1・過去Build・旧Saveを残す。

## 見た目の仕様と限界

- 両面の透過，面ごとの明暗，視線角による縁光，白い反射Highlight，内部Core。物理的な屈折・反射像・透過光は計算していない。
- 低解像度Bloomは別の`LucentBloom`表示Component。Game／Galleryで使用し，UIは後描画。Prefab単体のImportには不要なのでAsset Packageには含めない。
- 多面体を回すと外形と円形判定は一致しない。Gameは元の判定位置・半径と足元輪を維持し，浮遊は表示のみに限定。
- 大量の透明物体の重なりには描画順の限界がある。完全なOrder-independent Transparencyではない。
- 材質の色以外に内部Ringでも状態を区別。GameのHP数字は引き続き別表示。色覚特性ごとの本格検証は未実施。

## 再生成・検証

```powershell
.\scripts\crystal-kit.ps1 generate
.\scripts\crystal-kit.ps1 verify
```

GenerateはUnity自身でMesh・Prefab・Gallery Sceneを保存する。既存V1を再生成するとそのV1の形状を更新するので，別案は別Versionを作る。VerifyはゲームのCompile／EditMode／PlayMode／Windows Buildに続けて，Gallery Build・Unity Package・OBJ・ZIP・SHA-256を新しい日時Directoryへ出す。

## 参考と独自解釈

- [Rez Infinite](https://store.steampowered.com/app/636450/Rez_Infinite/)：光・音・動きがまとまる方向を参照。本Kitは同期音楽システムの再現ではない。
- [REVOLVER360 RE:ACTOR](https://store.steampowered.com/app/313400/REVOLVER360_REACTOR/)：奥行きのあるシューティング表現を参照。視点回転のゲームルールは輸入しない。
- [Geometry Wars: Retro Evolved](https://store.steampowered.com/app/8400/Geometry_Wars_Retro_Evolved/)：幾何形状と明瞭な発光輪郭の方向を参照。

公式Store説明と利用可能な画像，Userが提示した参考画像からの独自の見た目の解釈。作品を実プレイしたという記録ではない。形状・材質・コードは本試作用に新規作成。
