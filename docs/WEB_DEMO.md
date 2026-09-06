# ブラウザ試遊版

**[ブラウザで遊ぶ](https://koseihamaya2077.github.io/one-board-incremental/)** / [Web保存版Release](https://github.com/KOSEIHAMAYA2077/one-board-incremental/releases/tag/v0.6.1-web)

PCで「遊ぶ」をクリック。初回通信は約21 MB。Windows版とは別のブラウザ保存です。既存の個人サイトのリポジトリ・設定・コンテンツは変更していません。

ゲームの開発休止は継続したまま、0.6.1-dragの手触りをインストール不要で試せるようWebへ移植する。新しいゲーム要素やBalanceは追加しない。

## 配布方式

- Unity 6000.3.18f1 + Web Build Support。`scripts/momentum-web.ps1`で日時別の`Artifacts/Web/<日時>/site`へ出力。
- 既存の個人サイトは変更せず、この公開RepositoryのGitHub Pages（`https://koseihamaya2077.github.io/one-board-incremental/`）を使う。
- Gzip + Decompression Fallback。GitHub Pages側のContent-Encoding設定に依存しない。
- 生成物はソース履歴へ混ぜず、Release添付ZIPとPagesのDeploy Artifactとして保存する。
- Windowsの旧Build・Tag・Saveはそのまま保持。

## Web限定の互換対応

- OSフォントに依存できないため、Noto Sans CJK JP RegularをSIL OFL 1.1で同梱。元配布：https://github.com/notofonts/noto-cjk 。Unity内の読込はWeb限定。
- 進行はWeb専用PlayerPrefsキー`Momentum.Web.ProgressV1`へ保存し、Unityのブラウザ永続ストレージを利用。Windowsのファイル保存・移行・履歴処理は変更しない。
- サイトデータ削除、プライベートブラウジング、容量制限などで保存できない／失うことがある。Windowsとの自動同期なし。
- 終了確認後は攻撃を停止したままWeb側の終了案内を表示。ブラウザのタブをプログラムから強制終了しない。
- ブラウザ版ではファイルへのSession Log追記を行わない。無制限のログ蓄積を避ける。
- ゲーム画面の開始クリックで読み込み。音声はブラウザのユーザー操作制限に従う。
- PCのマウス／キーボード向け。スマートフォン対応・Mac Safari検証を完了したと扱わない。

## 保存元と検証

- Web Build source：`6c4eabb6c027a251777e4f78cc27891e38e9abf1`。ゲーム内Versionは0.6.1-dragのまま。Web版Tagは`v0.6.1-web`で区別。
- ZIP SHA-256：`ab38767612667bf2c69300cd4b17af3c7c79e2d9b6d85078bc73cfc56a70fe68`。
- 既存EditMode 102/102・PlayMode 35/35、Unity Web Build成功。Edgeで日本語、射撃、Gold増加、銃切替、Help、終了案内、再読込後のGold・銃復元を確認。
- 起動時にWebGLの機能照会Warningあり。検証では描画・操作の停止なし。再読込時、Unityキャッシュが使われた通信の中断はページ例外と区別して記録。
- 未検証：Mac Safari、スマートフォン、長時間プレイ、利用者全環境での保存・音声・描画保証。
- GitHub Pages配布はmainの`.github/workflows/pages-demo.yml`から手動実行。固定ReleaseのZIPとSHA-256を取得・照合してDeployする。ゲームのソースBranchをmainへ統合しない。

## 参考資料

- [Unity Webの配布設定](https://docs.unity3d.com/6000.3/Documentation/Manual/webgl-deploying.html)
- [GitHub Pagesのプロジェクトサイト](https://docs.github.com/en/pages/getting-started-with-github-pages/what-is-github-pages)
- [GitHub Pages無料枠と制限](https://docs.github.com/en/pages/getting-started-with-github-pages/github-pages-limits)
