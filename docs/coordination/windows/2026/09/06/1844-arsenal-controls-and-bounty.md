---
handoff_id: windows-20260906-1844-arsenal-controls-and-bounty
host: windows
created_at: 2026-09-06T18:44:00+09:00
base_commit: 892aa7a639ac852ae0fe82403eee2e2cdb80e8a7
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260906-1757-pickup-ideas-and-playtest-feedback
  - mac-20260905-0457-windows-verification-response
---

# Arsenal 0.6.0：新銃・B缶・入力と基本Menu

## 今回の目的

Shotgun／Sniperと取得型アイテムの小さな試作，Mouse・Keyboard操作切替，Help・設定・終了Menuを実装。既存試作・Build・Save・記録を残す。

## 会話からの開発記録

### Userの指示・発想（要約）

- 盤面へ広く弾が散るため，「拾ったら直接Gold」よりも「取得状態で敵を倒す／バンパーに当てる／壁へ届ける／既に3体貫通していれば成立」など，取得後の条件で稼ぐ案。
- 当初は効果を子弾に引き継ぐ案も検討。銃の弾数・速さで効果を届けやすさに違いがあってもよい。
- Shotgunは1回に多方向へ出し，Sniperは大きく非常に速い弾。まずこの2銃と，E缶のような形・英語1文字・色で分かるアイテムを実装したい。
- 銃外観は今回は保留。R／ホイールで銃切替。通常はMouse照準，キーを押したらKeyboard操作へ切替え，Q/E照準・Space発射。
- 左上?に操作とゲーム説明，Menuに終了・SE／BGM音量・Theme。BGMは未実装でも可。
- 追加回答の原意：子弾には引き継がない。代案として「缶を破壊するとその位置に一定時間ゾーンができ，通過弾へ効果が付く」ならよさそう。
- 関連する未実装メモ：前方の加速ゾーンと組み合わせる連射速度アップSkill。基礎強化・装填効果のどちらに置くか未決定。

### Assistantの提案

- まずB（Bumper）缶1種類で，取得弾を奥の紫バンパーへ届けて換金する小さな比較版。+12G・2個・位置などは今回のINITIALであり，Userが指定した確定Balanceではない。
- 新銃2種を初期使用可能にし，UZIの従来40G開放は維持。BGMは今回は未実装表示，SEとThemeを用意。
- 終了の誤操作を避ける確認画面。旧版へ戻って遊べるよう新Save名へ移行し，旧Fileは読取のみ。

### 採用・保留・変更

- 採用：§33.10へ記載した新銃，B缶，操作・Menu。User追加指示によりBは子弾へ継承しない。親の分裂消滅時は未換金Bも失われるが，子自身が別の缶を拾うことは可能。
- 撤回：分裂家系で報酬を共有して1回換金するAssistant初期案。Userの非継承指定を優先。
- 保留：缶破壊による一時ゾーン，複数種缶，連射速度Skill，条件付き撃破／壁／貫通履歴の報酬。今回のBは破壊対象ではなく接触取得物。
- 銃Rendering・敵HP・Stage経済・低速急減速値・旧Prototypeは今回の変更対象外。

## 読んだ正本

- AGENTS，Master §33（特に§33.8〜33.10），Momentum Brief，coordination README，Handoff Template。
- 最新Mac Handoffの当時の未導入Unity問題は過去状況。現在はC:\DevのCloneと固定Editorで検証できている。

## 実施内容・得られた結果

- Source：0.6.0-arsenal，Master：0.8.0。Shotgun8粒同時／Sniper1発，選択切替は次マガジンのみへ適用。
- B缶は黄緑の缶Mesh＋Bロゴ。取得ではGoldなし，取得弾のバンパー接触で1回+12G，その後解除。壁・回収・消滅で換金なし。次Challengeで2缶復活。
- Q/E 120度/s・Space発射，盤面内Mouse移動／クリックでMouse操作復帰。R／ホイールは未開放銃を飛ばす。Help／Menu停止・背景入力防止・終了確認，SE10%刻み増減／消音，Crystal／Neon／Diagram。
- 新保存先challenge-arsenal-v1.json。存在しない場合だけ旧challenge-v1.jsonから妥当な進行を読込み，新Saveへ書く。旧challenge-historyも保持。
- 作業開始時mainをff-onlyで確認し更新なし，feat/portrait-stageへ復帰。今回はLocal Commitのみ，Push／PR／Release追加は行っていない。

## 設計判断

### 確定

- 現在の実装と数値はMaster §33.10が正本。メモの未実装案を先回りで追加しない。
- 既存256同時／1024生成／10秒寿命と速度300の再射撃条件を維持。

### 提案・保留

- Bを分裂前に換金する構成と，分裂を使わず確実に届ける構成を試遊で比較したい。銃間の強さ・缶報酬は未調整。
- 非継承が厳しすぎる場合の比較候補が「缶破壊→一時効果ゾーン」。まだ実装承認済みの仕様として扱わない。

## 検証

- Command：`.\scripts\momentum.ps1 verify`（Unity 6000.3.18f1，Windows native PowerShell）。Compile成功，EditMode 93/93，PlayMode 35/35，Windows x64 Build／ZIP／SHA-256成功。
- 追加Test：Shotgun同時8粒，Sniper上限・高速Swept Hit，銃切替の非遡及性，所有・Preset JSON，B取得／1回換金／壁／回収／消滅／子非継承／子の独立取得，100Seed缶配置，入力Mode／Menu停止／誤射防止／SE値／Theme非干渉。
- Standalone診断：`-momentum-capture <folder> -momentum-stress -momentum-arsenal`を2回，各Exit 0。Shotgun8／Sniper1，Help・設定・終了確認のPause，文字Overflow 0，Modal四隅の画素存在を全て確認。通常診断もExit 0，Revolver6発，boost2，残弾0，Pause成功，文字Overflow 0。
- 各Player診断はPersistence無効。旧Saveの最終書込は2026-09-06 17:54:04で作業前のまま，新Arsenal Saveは診断で作成していない。
- Repository sanityの生成物非追跡・Master版／変更履歴整合をLocalで確認。GitHub Actionsは今回未実行（未Push）。
- 未実行：Mac Test／Build，実ユーザー保存先での移行を伴う起動，長時間試遊，物理キー／ホイールの人手操作。Controller入力経路は自動Testで確認。
- 既知制限：BGM未実装，Bは1種類のみ，缶は固定2位置，Sniperは初速強化時1800上限へ達するため前方加速ゾーンの恩恵が小さい。Balance調整は未完了。

### 描画調査の訂正記録

画像Previewで設定画面が欠けたように見えたため，D3D11指定・Graphics Jobs無効を試験した。しかし保存PNGのModal領域を2px間隔で比較するとD3D12対D3D11，Jobs有無とも差分0で，描画欠落を裏付けられなかった。Assistantの描画原因説明をUserへ訂正し，API／Jobs変更は撤回した。元のSceneBuilderとProjectSettingsへ復帰済み。試験Commit・Buildは削除せず保持。最終版は元の描画設定を使用し，実画素検査も2回成功。内部Engine／Driver不具合を確認した記録として引用しない。

設定APIの調査時参照：[Unity 6.3 SetGraphicsAPIs](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PlayerSettings.SetGraphicsAPIs.html)。これは設定方法の資料であり，今回の不具合原因を支持する資料ではない。

## 生成物の絶対Path

- 検証Root：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260906-184213-5181607`
- 起動：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260906-184213-5181607\Windows\OneBoardMomentumLab.exe`
- ZIP：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260906-184213-5181607\OneBoardMomentumLab-v0.6.0-arsenal-Windows-x64.zip`
- SHA File：上記ZIPの末尾に`.sha256`。
- SHA-256：`21210dda5a52b59c858813b5a24b637d62ad9aeeb7b6c06f20935008df1cacdf`
- Root内：compile.log／EditMode.xml／PlayMode.xml／build.log／summary.txt，ArsenalCapture／ArsenalRepeat／NormalCaptureに診断PNGと結果。Build Sourceはbase_commitと一致。

## 変更File

- Core：MomentumProgress，MomentumChallenge，MomentumSimulation，MomentumPickups（新規）。
- Presentation：MomentumLabController，MomentumPortraitHud，MomentumNeonView，MomentumProgressSave，MomentumUtilities／MomentumPickupView／MomentumArsenalCapture（新規）。
- Tests：MomentumArsenalTests，MomentumArsenalPlayTests（新規，meta同梱）。
- AGENTS，Master，Momentum Brief，scripts/momentum.ps1，本Handoff。Scene／旧Assets／ProjectSettingsの最終差分なし。

## 次のHostへの依頼

1. UserにShotgun／Sniper，Q/E/SpaceとMouse復帰，Bを奥へ届ける操作を試してもらう。
2. Push指示が出たらRemote更新を確認し，このBranchのLocal履歴を同期。mainへ無断統合・Force Pushしない。
3. 今後のBゾーン案・連射Skillを検討する場合は，Masterへ採用範囲を反映してから実装。旧Build／Save／Kit／メモを保持。

## User判断が必要な点

今回の実装を試すための追加判断はなし。次のBalance／ゾーン比較とGitHubへの公開操作は別途指示を待つ。
