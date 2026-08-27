# Unity AI開発規則

## 固定環境

- Unityは`6000.3.18f1`だけを使う．別PatchでProjectを保存しない．
- Prototype 0のGame Versionは`0.1.0-prototype0`である．
- 第一対象BuildはWindows 10／11 x64である．

## 参照優先順位

1．`docs/MASTER_GAME_SPECIFICATION_2026-08-28.md`が唯一のGame Logic正本である．  
2．実装範囲は対象Prototype Briefで狭める．  
3．`docs/history/`，Chat Log，過去案から機能を復活させない．

Prototype 0では`docs/AI_PROTOTYPE0_IMPLEMENTATION_BRIEF_2026-08-28.md`だけを併読する．調査や設計監査を明示的に依頼されていない限り，履歴文書を仕様入力にしない．

## 実装規則

- Prototype 0外の機能を先回りして実装しない．
- `Assets/_Project/Core`は`UnityEngine`へ依存させない．
- Game LogicをMonoBehaviour，Animation，Audio，Particleへ置かない．
- SceneとPrefabのYAMLを手書きしない．Editor Toolで生成する．
- Runtime StateをScriptableObjectへ保存しない．
- Rigidbody2D Callbackを論理正本にしない．高速移動は明示Queryで処理する．
- `.meta`をAssetと同時にCommitする．
- Unityが生成する`Library`，`Temp`，`Logs`，`obj`，Build出力をCommitしない．
- 仕様変更時はMaster，Test，変更履歴を同時に更新する．
- `FIXED`を無断変更しない．`INITIAL`は指定値で実装し，計測なしに別値へ変えない．
- Prototype外の空Class，将来用Framework，汎用Serviceを先回りして作らない．

## 検証規則

- C#変更後は少なくともEditMode Testを実行する．
- Presentation，Scene，Input変更後はPlayMode Testも実行する．
- Release候補ではMac検証に加えWindows x64 Buildを実行する．
- Error時は最初の原因を修正し，後続Errorだけを個別対応しない．
- 完了報告に実行Command，成功したTest，未実行項目，既知問題を書く．

## 標準Command

macOSでは`./scripts/unity.sh verify`，Windowsでは`.\scripts\unity.ps1 verify`をRelease候補の最終検証に使う．このCommandはScene存在確認とCompile，EditMode，PlayMode，Windows x64 Build，ZIPとSHA-256生成を順番に実行する．Sceneがない場合だけUnity自身が生成する．結果は`Artifacts/`に出し，Commitしない．Scene生成Codeを変更した場合は`generate`を明示実行し，生成差分を確認する．
