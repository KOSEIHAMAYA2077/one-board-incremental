---
handoff_id: windows-20260905-1531-main-readme-navigation
host: windows
created_at: 2026-09-05T15:31:00+09:00
base_commit: f3446164f33d046e374e66a66a2e26a185a3749e
authority: non-authoritative-handoff
status: complete
consumed_handoffs:
  - windows-20260905-1524-clear-panel-and-next-ideas
---

# GitHubトップREADMEの案内を更新

## 今回の目的

Userの「最終更新が古くないか，README更新してよい」に対応。mainはPrototype 0のコードを保持し，開発ブランチと配布先を明示する。

## 読んだ正本

- main／開発ブランチのAGENTS.mdとREADME，Handoff Template。
- 最新実装・配布状況は既存の検証記録とGitHubのread-only確認で照合。Game Logic変更なし。

## 実施内容

- Cleanな開発ブランチと未Push4 Commitを保持。専用Worktree C:\Dev\one-board-readme-20260905 をorigin/mainから作成。
- README冒頭へ開発の紹介，Release／Branch／Archiveリンク，mainと開発版の違い，0.5.1／0.5.2未送信を追記。旧Prototype 0本文は削除せず保存用と明示。
- READMEだけのCommit cdd743fc73e96f5e6b75b6a42e177d4cd1c3216c をmainへ通常Push。mainのゲームコード・正本仕様・旧Tag・非公開設定を変更しない。
- 今回のHandoffは開発ブランチにLocal保存。README以外の未Push差分やZIPは送信していない。

## 得られた結果

- 公開先（非公開Repo）：https://github.com/KOSEIHAMAYA2077/one-board-incremental#readme
- GitHub main HEADと上記Commitが一致。READMEのGitHub blobとLocal blobは e9694fff18a5239602eaf98407ddf651617780d2 で一致。
- 配布確認：v0.5.0-challenge は存在しPrerelease。0.5.1／0.5.2は引き続きLocalのみ。

## 設計判断

### 確定

- README更新はUser承認済み。今回だけmainの文書を更新し，最新ゲームコードの統合は行わない。

### 提案

- 次回試作をPush／Release保存する際，main READMEの保存状況も同時更新する。

### 棄却または保留

- 未送信コードのPush・新Release作成・mainへのゲームコードMergeは今回のREADME更新へ混ぜない。

## 検証

- 実行：status／fetch／diff --check／cat-file（リンク対象確認）／merge-base／push，gh repo view／release view／api。
- Push直前のmain差分がREADME一枚のみであることを検証。GitHubでHEADとREADME blobを照合済み。
- Unity Test／Buildは未実行（文書のみ）。ゲームの新たな動作問題は評価していない。

## 変更File

- main：README.md。
- 開発ブランチ：本Handoff（専用Commit，未Push）。

## 次のHostへの依頼

1. mainは文書Commitが進んだ。次回pull時にゲームコードまで統合済みと誤認しない。
2. 開発ブランチのREADMEには旧説明が残る。将来Merge時はmainに追加した入口案内を失わないこと。
3. 0.5.1／0.5.2は依然未送信。旧Build・Saveを保持。

## User判断が必要な点

- README更新は完了。新しい試作のPush／Release追加は別途。
