# 開発の共通ルール

## 仕様駆動開発

要求の整理、サブSpecへの分解、設計、実装、仕様との照合には、
[spec-driven-developmentスキル](.github/skills/spec-driven-development/SKILL.md)を参照する。
小さな修正でも原則は適用するが、不要な文書や承認手続きは増やさない。

### 仕様の正本

- 全体の要求は親Issue、サブSpecは関連するサブIssueで管理する。
- 小さく完結する変更は、単一のIssueをSpec兼実装単位として扱う。
- 原則としてIssue本文に記載する。本文では読みにくくなる内容だけをIssue Artifactへ分け、本文からリンクする。
- 同じ内容を本文、Artifact、リポジトリ内の仕様書へコピーしない。既存の正本を更新する。
- Issueコメントの提案・質問・決定を区別する。明示的に合意された内容だけを仕様へ反映する。
- 動作、権限、互換性に影響する未決事項は推測で確定しない。該当部分の実装を止めて確認する。
- 合意済み仕様を変更する場合は、差分、理由、影響を示して合意を得る。コードに合わせて黙って変更しない。

### 人が理解できる単位

- サブSpecは、人が目的・振る舞い・完了条件をまとめて理解できる単位にする。
- ファイル、クラス、API・DB・画面などのレイヤーだけを基準に分解しない。
- 親要求への対応、依存関係、共通契約を明確にする。共通の仕様やADRはリンクで参照する。
- 一度の確認では、対象サブSpecの要点、判断が必要な差分、影響を提示する。全仕様を繰り返し読ませない。
- サブSpecの完了と全体の完了を区別する。最後に組み合わせた動作と親Specの受け入れ条件を確認する。

### 設計と技術検証

- 実装前に既存コード、テスト、適用範囲内のAGENTS.md、関連する採用済みADRを確認する。
- 詳細設計は行うが、詳細設計書は必須にしない。実装詳細はコードとテストを参照する。
- 長期的・横断的な判断はADRで扱う。既存配置を優先し、なければ`docs/adr/`に置く。
- AGENTS.mdにはADRの配置と確認手順だけを記載し、個別ADRへのリンク、一覧、決定内容の転記は追加しない。ADRの追加・置き換えに伴うAGENTS.mdの更新を不要に保つ。
- ADRの採用には明示的な合意が必要。採用済みの決定を変える場合は新しいADRで置き換え、旧ADRに置き換え先を示す。
- 設計を左右する技術的不確実性は、必要に応じて本体から独立した小さな検証用コードで確認する。
- 技術検証は問いと判定基準を先に決め、結論に影響する実行条件を本体と揃える。
- 検証のために本体のコード・依存関係・設定を変更しない。実データ、秘密情報、本番環境は使用しない。
- 再実行・レビューに必要な検証コードは独立したプロジェクトとして保存する。一時コードは結果と必要な再現情報を記録してから、今回作成したものだけを整理する。
- 技術検証コードをそのまま本体へ移さない。知見を本体の実装とテストへ反映し、統合・受け入れ確認も行う。

### 検証とGitHub操作

- 実際の構成と既存手順を確認し、変更範囲を覆う最小限のビルド・テストを実行する。未確認のコマンドを作らない。
- 受け入れ条件ごとに確認方法と結果を示す。Actions成功だけで要求達成と判断しない。
- 未検証、失敗、残る制約を明示する。確認できた範囲を超えて完了としない。
- PRには関連Issue、基準にした仕様・ADR、受け入れ条件との対応、検証結果、未検証事項を記載する。
- Artifactは更新可能なので、版が取得できれば記録する。取得できなければ、確認日時と基準にした受け入れ条件をPRに残す。
- PR作成、承認、マージ、Issueのクローズは、それぞれ明示された依頼・権限の範囲で行う。技術的に操作できることを許可と解釈しない。
- 文書でルールを書くことと、GitHubのrulesetやブランチ保護で強制することは別である。設定を無断で変更しない。

## リポジトリの現状

.NET 10 の初期構成がある。SDK は`global.json`で10.0.403以降の.NET 10 feature bandに限定する。ソリューション形式は`.slnx`。

- `src/EmployeeManagement.Api`: ASP.NET Core Web API。`/health`の準備状態と`/api/health`のJSON応答を公開し、CommonのApplicationとInfrastructureを参照する。
- `src/EmployeeManagement.Web`: APIとは別ホストのBlazor Web App。Aspireのサービス検出でAPIに接続し、初期画面からヘルス状態を取得する。
- `src/Modules/Common`: CommonモジュールのDomain、Application、Infrastructure各プロジェクト。業務機能やDBスキーマはまだない。
- `tests/EmployeeManagement.Common.Tests`: プロジェクト参照とパッケージ参照の許可リストを検査するテスト。
- `tests/EmployeeManagement.IntegrationTests`: `WebApplicationFactory`によるAPIヘルスエンドポイントの結合テスト。
- `EmployeeManagement.AppHost`: PostgreSQL、pgAdmin、API、Blazor Webのローカル開発オーケストレーション。
- `EmployeeManagement.ServiceDefaults`: API/Webのヘルスチェック、サービス検出、HTTP回復性、OpenTelemetry既定値。
- `.agents/skills/aspire*`および`.github/skills/aspire*`: Aspire CLIで追加したAspire開発用エージェントスキル。
- `.editorconfig`、`Directory.Build.props`、`Directory.Packages.props`: 書式、解析、警告、ターゲットフレームワーク、NuGetバージョンを共通管理する。

リポジトリルートで次を実行する。これらのコマンドは.NET SDK 10.0.403で確認済み。

```powershell
dotnet build EmployeeManagement.slnx
dotnet test EmployeeManagement.slnx
aspire run --apphost EmployeeManagement.AppHost\EmployeeManagement.AppHost.csproj
```

Aspire AppHostはAPIとWebを別ホストとして起動し、Web→API参照、PostgreSQL→API接続、pgAdminを管理する。PostgreSQLとpgAdminコンテナー初期化用の資格情報はAspire管理の開発用シークレットとし、ソースに記録しない。pgAdminはログイン画面を出さないDesktop modeで起動し、ホスト側ではループバックインターフェースにのみ公開する。`aspire run --apphost EmployeeManagement.AppHost\EmployeeManagement.AppHost.csproj`で起動し、ダッシュボードから各リソースへアクセスする。PostgreSQLデータはAspire管理の名前付きボリュームへ永続化する。旧Composeのコンテナは停止し、旧データボリュームは削除せず保持する。Aspire CLI 13.4.6を使い、`aspire init --non-interactive --language csharp`、`aspire add postgresql --apphost EmployeeManagement.AppHost\EmployeeManagement.AppHost.csproj --non-interactive`、`aspire agent init --workspace-root . --skill-locations github --skills aspire,aspire-init,aspire-orchestration,aspire-monitoring,aspire-deployment,aspireify --non-interactive`を実行している。APIへのNpgsql接続、DBスキーマ、およびDBを使う結合テストはまだ追加していない。

起動後、別のターミナルで以下を実行して各リソースの準備状態を確認する。pgAdminのURLはAspireダッシュボードから開き、`/misc/ping`が`PING`を返すことを確認する。APIは`/api/health`がJSONで`Healthy`を返し、BlazorのトップページにAPI状態が表示される。

```powershell
aspire wait postgres --apphost EmployeeManagement.AppHost\EmployeeManagement.AppHost.csproj
aspire wait pgadmin --apphost EmployeeManagement.AppHost\EmployeeManagement.AppHost.csproj
aspire wait api --apphost EmployeeManagement.AppHost\EmployeeManagement.AppHost.csproj
aspire wait web --apphost EmployeeManagement.AppHost\EmployeeManagement.AppHost.csproj
```
