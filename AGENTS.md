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

現在の追加実装はPrototype 1Aであり，`docs/AI_PROTOTYPE1A_IMPLEMENTATION_BRIEF_2026-09-05.md`を併読する．旧Prototype 0のScene・Build・Briefは保持する．Prototype 1Aは`0.2.0-placement`，検証Commandは`.\scripts\placement.ps1 verify`．Userは多人数による面白さ検証より，AI駆動で動く試作を積み上げることを優先している．必要な動作Testは継続する．

## 実装規則

最新試作はMomentum Lab（0.4.0-momentum）．Master §32と`docs/AI_MOMENTUM_LAB_IMPLEMENTATION_BRIEF_2026-09-05.md`を参照．User承認の新方向であり旧PrototypeのGame Logicは保持する．`.\scripts\momentum.ps1 verify`を使う．以下に残るPrototype 1A／2の記述は旧版の復元用．

現在の追加実装はPrototype 2（0.3.0-recipe）．`docs/AI_PROTOTYPE2_IMPLEMENTATION_BRIEF_2026-09-05.md`を併読する．検証は`.\scripts\recipe.ps1 verify`．旧Prototype 0と1AのScene・Build・Brief・履歴を保持する．

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

## Mac・Windows間の作業同期

- GitHubをCode，仕様書，検証結果要約，端末間Handoffの共有経路とする．Chat履歴そのものを仕様正本にしない．
- 作業開始前にWorking Treeを確認し，`main`を`git pull --ff-only`で最新化する．未Commit変更や競合がある場合は破棄せず，内容を確認してから進める．
- 他端末の最新作業を把握するときは，`docs/coordination/README.md`と`docs/coordination/<other-host>/`の最新Handoffを読む．全履歴を毎回読む必要はない．
- Handoffは観察，判断理由，実行結果，未決事項，次の依頼を伝える参考資料であり，MasterやPrototype Briefを上書きしない．Handoffから機能や数値を直接実装しない．
- 自端末のHandoffは`docs/coordination/<host>/YYYY/MM/DD/HHmm-<topic>.md`へ新規作成する．他端末のHandoffや過去Handoffを編集せず，訂正も新しいHandoffで行う．Windowsで無効な文字をFile名に使わない．
- HandoffだけのCommitは，他の変更と混ぜない．Push前にremoteの更新を確認し，`main`をForce Pushしない．Codeや正本仕様を両端末で並行変更する場合は，端末別BranchまたはWorktreeを使う．
- API Key，Token，個人情報，巨大な生Log，内部推論の逐語記録をHandoffへ保存しない．結論，根拠，代案，棄却理由を簡潔に残す．
