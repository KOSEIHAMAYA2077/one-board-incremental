---
handoff_id: windows-20260906-2154-playable-download-and-web-demo
host: windows
created_at: 2026-09-06T21:54:14+09:00
base_commit: d93b5408de0c2be4c5b7dbf6f141cfc1209e64fc
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260906-1957-public-prototype-pause
  - windows-20260906-1925-stronger-drag-and-extraction-brainstorm
---

# ダウンロード版とGitHub Pages試遊版の公開

## 今回の目的

READMEからすぐ触れる入口を用意し、開発記録だけでなく実際の手触りを伝える。

## 会話からの開発記録

### Userの指示・発想

- DLすれば起動して手触りを試せるようREADMEを整えてほしい。
- Webアプリにできるなら公開し、自分のGitHubドメインのサイトを使いたい。
- Webビルド用モジュールは必要なら入れてよい。有料なのか質問。

### Assistantの提案

- 既存の検証済みWindows ZIPを公開し、ブラウザ版も並行して用意。
- 既存個人サイトは改変せず、ゲームRepositoryのProject Pagesを同じgithub.ioドメインの下に作る。
- Webモジュール追加・公開RepositoryのPagesは追加購入なしで実施できる。GitHub Pagesには無料枠の利用制限がある。

### 採用・保留・変更

- モジュール追加、Web互換対応、Windows／Web配布、README更新を採用。
- 開発休止は継続。Balance、新Stage、採掘／持ち帰り案などは追加しない。
- Sitesの別ホスティングではなく、User指定のGitHub Pagesを使用。

## 読んだ正本・資料

- AGENTS、README、coordination README／Template、前回検証Handoff、Release Archive。
- Masterの最新節、Momentum Briefの最新節、対象Controller／Save／Editor Buildコード。
- Unity公式Web配布資料、GitHub公式Pages概要・無料枠、ローカルUnity Hub CLI help。

## 実施内容

- 既存0.6.1 Windows ZIPのSHAと内容一覧を再確認。`v0.6.1-drag`をBuild source `56bfa5d4d3805113bef6e89719a7067bf17da375`で新規公開。旧Releaseは保持。
- Unity Hub CLIで6000.3.18f1へwebglモジュールを追加。Exit0・installed successfully。Editor本体の版は変更なし。課金手続きなし。
- Web限定の日本語Font、PlayerPrefs進行保存、終了案内、Session Log抑制を追加。Windowsの既存Save処理は条件コンパイルで維持。
- Noto Sans CJK JP Regularを元配布から同梱しOFLライセンスを掲載。ゲームLogicは変更なし。
- Gzip＋Decompression Fallback、ユーザー操作で開始するWebテンプレート、全画面・Windows DL・説明リンクを用意。
- `v0.6.1-web`でWeb ZIPを保存。mainの手動Pages WorkflowでSHA検証後Deploy。
- mainと作業BranchのREADMEに「ブラウザで遊ぶ」とWindows直DLを掲載。mainにはゲームコードを統合していない。

## 得られた結果

- 試遊URL：https://koseihamaya2077.github.io/one-board-incremental/
- Windows配布：https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.6.1-drag
- Web保存版：https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.6.1-web
- Pages実行：https://github.com/KOSEIHAMAYA2077/one-board-incremental/actions/runs/34034405543 （success）
- 個人サイト本体のRepository・コンテンツ・Pages設定は変更なし。休止の説明も保持。

## 設計判断

### 確定

- 0.6.1のGame Logicはそのまま。Web互換対応の範囲はBrief補足と`docs/WEB_DEMO.md`に明記。

### 提案

- 新しいゲーム実装の提案なし。

### 保留

- スマートフォン対応、Mac Safari検証、他環境での描画／音声／保存保証は未実施。

## 検証

- `scripts/momentum.ps1 test`：Compile、EditMode102/102、PlayMode35/35成功。
- `scripts/momentum-web.ps1`：Web Build成功。最終Source `6c4eabb6c027a251777e4f78cc27891e38e9abf1`、Tagも一致。
- ローカルHTTPと公開HTTPSをEdgeの隔離ブラウザで試遊。射撃、撃破とGold増加、R／SpaceでShotgun発射、Helpの日本語、終了案内、再読込を確認。
- 公開先再読込後のスクリーンショットでGold22とShotgun選択を確認。途中Challengeはリセットされ、永続値だけ復元。
- 最終試遊のページ例外・Console Errorは0。初回テストのfavicon404は明示アイコン宣言で修正。Unityキャッシュの通信中断は例外とは分けて記録。
- 起動時にWebGL機能照会のINVALID_ENUM Warningあり。試遊では画面停止・操作失敗なし。Warningがあること自体を隠さず既知制約に記載。
- Windowsは今回再Buildせず、以前の検証済みZIPを配布。Webでは人の聴覚によるSE確認・長時間試遊は未実行。
- Windows／WebともGitHub asset digestとローカルSHAが一致。Pages Deploy成功後に公開URLを直接再検証。

## 成果物の絶対パス

- Windows：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260906-192233-9062126\OneBoardMomentumLab-v0.6.1-drag-Windows-x64.zip`
- 既存Test再実行：`C:\Dev\one-board-incremental\Artifacts\Momentum\20260906-214718-5635505`
- Web：`C:\Dev\one-board-incremental\Artifacts\Web\20260906-215112\site`
- Web ZIP：`C:\Dev\one-board-incremental\Artifacts\Web\20260906-215112\OneBoardMomentumLab-v0.6.1-Web.zip`
- Web SHA：`ab38767612667bf2c69300cd4b17af3c7c79e2d9b6d85078bc73cfc56a70fe68`
- 画面・Console記録：同Web実行フォルダー内の`browser-local`／`browser-public`。Playwrightによる隔離環境の保存はユーザーの既存セーブに触れない。
- main文書編集用Worktree：`C:\Dev\one-board-readme-main`。本体Checkoutはfeat/portrait-stageのまま。

## 変更File

- README（main／作業Branch）、Release Archive、Windows配布説明、Web配布説明、Momentum Brief配布補足。
- MomentumLabSceneBuilder、MomentumLabController、MomentumProgressSave、MomentumUtilities。
- Web Fontとライセンス、WebGLテンプレート、Web終了jslibとUnity生成meta。
- scripts/momentum-web.ps1、mainの.github/workflows/pages-demo.yml、本Handoff。

## 次のHostへの依頼

1. 開発休止は継続。試遊URLを案内するだけで新機能を自動追加しない。
2. 既存Release／Build／Save／過去Handoffを保存する。
3. Pages更新が必要な場合は新しい保存版を作って検証し、旧ZIPを上書きしない。

## User判断が必要な点

なし。今後の公開先・試作再開の変更は別途User指示を待つ。
