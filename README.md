# One Board Incremental

一つの盤面へTargetを配置し，五発Recipeと一発ごとの照準で収益経路を作り，奥のCoreを破壊する短編能動型インクリメンタルゲームである．現在はPrototype 0として，射撃感と最初の10 Hit→10 Gold→`Collector Value＋1`だけを検証している．

## 固定環境

- Unity 6000.3.18f1．
- Windows 10／11 x64を第一対象とする．
- Prototype 0 Game Versionは`0.1.0-prototype0`．
- Build配布はGitHub ReleasesのPre-releaseを使う．

## 検証

macOS：

```bash
./scripts/unity.sh verify
```

Windows PowerShell：

```powershell
.\scripts\unity.ps1 verify
```

Scene生成，EditMode Test，PlayMode Test，Windows x64 Buildを順番に行い，結果を`Artifacts/`へ出す．詳しくは`docs/DEVELOPMENT_WORKFLOW.md`を参照する．

## AIへ渡すファイル

Prototype 0を実装するAIには，次の二ファイルだけを渡す．

1．`MASTER_GAME_SPECIFICATION_2026-08-28.md`  
2．`AI_PROTOTYPE0_IMPLEMENTATION_BRIEF_2026-08-28.md`

Masterが唯一のGame Logic正本である．Prototype Briefは今回の実装範囲だけを限定する．競合時はMasterを優先する．

## 判断履歴

次は調査と判断理由を保存する履歴文書であり，実装仕様ではない．

- `DESIGN_MEMO_2026-08-28.md`
- `history/economy_bootstrap_research_2026-08-28.md`
- `history/incremental_design_research_report_2026-08-27.md`
- `history/incremental_economy_and_pacing_research_2026-08-28.md`
- `sources/`以下の同期資料

AIは，明示的な調査・設計監査を依頼された場合を除き，これらから機能や数値を実装へ取り込まない．

## 現在の範囲

Prototype 0に入れるもの：

- Mouse照準と距離連動の散布Cone．
- Seed付き単発射撃，0.65秒Reload．
- Collector一個，壁反射，Gold，Lifetime Gold．
- 10 Hit表示と10 Goldの最初の`＋1`．
- Local Log，EditMode／PlayMode Test，Windows x64 Build．

Material，複数Target，配置編集，五発Recipe，特殊弾，Auto，Core DamageはPrototype 0へ先回りして入れない．
