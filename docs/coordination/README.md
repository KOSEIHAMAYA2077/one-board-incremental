# Mac・Windows間のAI作業同期

## 目的

このDirectoryは，MacとWindowsで動くChatGPT Work／Codex間に，作業結果と次の依頼をGitHub経由で渡すための追記型Handoff Logである．会話全文のArchiveではなく，別端末のAIが安全に続きを始めるために必要な情報だけを保存する．

公式のChat履歴や端末固有のLocal Contextが完全に同期することを前提にしない．Code，仕様書，検証可能な成果物，HandoffをGitで共有し，各端末の会話はそれらを読む入口として扱う．

## 仕様上の権限

Handoffは参考資料であり，Game Logic正本ではない．優先順位は常に次の通りである．

1．`docs/MASTER_GAME_SPECIFICATION_2026-08-28.md`
2．対象PrototypeのImplementation Brief
3．`AGENTS.md`と検証済みCode／Test
4．`docs/coordination/`のHandoff
5．`docs/history/`と過去Chat

Handoffに新しい案や数値が書かれていても，MasterまたはImplementation Briefへ明示的に昇格するまでは実装しない．

## Directory構造

```text
docs/coordination/
  README.md
  HANDOFF_TEMPLATE.md
  mac/YYYY/MM/DD/HHmm-topic.md
  windows/YYYY/MM/DD/HHmm-topic.md
```

HostごとにDirectoryを分け，同じHandoffをMacとWindowsが編集しない．日付は`YYYY/MM/DD`，File名の時刻は24時間表記の`HHmm`とし，Windowsで無効な`:`などを使わない．

## 作業開始時

1．未Commit変更がないか確認する．
2．`main`を`git pull --ff-only`で最新化する．
3．`AGENTS.md`，Master，対象Prototype Briefを読む．
4．相手Hostの最新Handoffを一件読む．関連する過去Handoffだけ追加で読む．
5．Handoff中の内容を`確定`，`提案`，`観察`，`未検証`に分ける．

## 作業終了時

1．`HANDOFF_TEMPLATE.md`から自Host側へ新しいFileを作る．
2．行った変更，検証結果，判断と確度，未解決点，次のHostへの依頼を書く．
3．実行していないTestを成功扱いしない．
4．HandoffだけのCommitを作り，Codeや無関係な変更と混ぜない．
5．Push前にremoteの更新を確認する．競合や他端末のCode変更があれば，勝手に破棄またはForce Pushしない．

Commit例は次の通りである．

```text
handoff(mac): summarize gumball findings
handoff(windows): record prototype 0 verification
```

## 並行作業の境界

- 読み取りと調査は両端末で並行してよい．
- HandoffはHost別の新規Fileなので並行作成してよい．
- Unity Scene，ProjectSettings，Master，Prototype Brief，同じC# Fileは同時編集しない．
- Codeや正本仕様を並行変更する場合は，BranchまたはWorktreeを分けてPull Requestで統合する．
- Unityの`Library`，`Artifacts`，Build出力，Local LogはGitへ入れない．Handoffには結果の要約と成果物Pathだけを書く．

## ChatGPT WorkとCodexの役割

- ChatGPT Workは調査，分類，設計監査，文書案の作成を担当する．
- CodexはGit操作，C#／Test／Editor Toolの変更，Unity検証，Buildを担当する．
- Work側にGit操作手段がない場合はHandoff Fileまで作成し，同じLocal ProjectのCodexへ内容確認，Commit，Pushを依頼する．
- 会話の一語一句や非公開の内部推論を保存する必要はない．再開に必要な結論，根拠，代案，棄却理由，未解決点を残す．

## 最新Handoffを読むための例

macOS／Git Bashでは次を使える．

```bash
find docs/coordination/windows -type f -name '*.md' | sort | tail -1
```

Windows PowerShellでは次を使える．

```powershell
Get-ChildItem .\docs\coordination\mac -Recurse -Filter *.md |
    Sort-Object FullName |
    Select-Object -Last 1
```
