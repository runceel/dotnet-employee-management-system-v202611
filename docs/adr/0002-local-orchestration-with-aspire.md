# Aspireによるローカル開発リソースのオーケストレーション

## 状態

提案中。

- 提案日: 2026-11-03
- 関連するユーザーの明示的な要求: 「aspire を使って」
- 技術的な採用確認: 未実施

## 背景

APIとBlazor Webを別ホストで実行し、PostgreSQLとpgAdminを含むローカル開発環境を再現可能に起動する必要がある。ComposeでDB管理を別々に行うより、アプリと依存リソースの起動状態を一つの開発用ホストで確認できることが望ましい。

ユーザーは.NET Aspireの導入を明示的に指定した。一方、AppHost構成、サービス既定値、パッケージの版、永続化の扱いといった具体的な技術選択は個別に採用合意されていないため、このADR全体は提案状態とする。

## 決定案

- 開発時のアプリ起動とローカル依存リソース管理に.NET Aspire AppHostを使用する。
- AppHostにAPI、Blazor Web、PostgreSQL、pgAdminを登録し、WebからAPIへの通信はAspireのサービス検出で行う。
- APIとWebに共通のServiceDefaultsを参照させ、サービス検出、ヘルスチェック、HTTP回復性、OpenTelemetryの開発用設定を提供する。
- AppHostは起動・構成に限定し、Domain/Application/Infrastructureの業務実装やデータアクセス実装を持たない。
- AppHostはデータベースリソースをAPIへ接続文字列として提供する。Npgsql、EF Core、スキーマ、マイグレーション方式は別途合意・設計する。
- PostgreSQLのパスワードはAspireの生成シークレット、pgAdminコンテナーの初期化用パスワードはAspireのsecret parameterとして管理する。パスワード等の機密値をソース管理へ含めない。
- pgAdminはログイン画面を省略するDesktop modeで起動し、ホストのループバックインターフェースにのみ公開する。
- Docker ComposeのPostgreSQL/pgAdmin定義は削除し、Composeで既に作成されたデータボリュームは削除しない。

## 重要な代替案と不採用理由

- 個別の`dotnet run`と手書きComposeを引き続き併用する: 起動構成とサービス間接続が分散する。
- APIとWebを単一ホストに統合する: 別ホストからAPI経由で通信する要求と異なる。

## 影響・トレードオフ

- ローカル開発ではDockerとAspire CLIが必要となる。
- Aspire AppHostとServiceDefaultsはホストの構成・運用コードであり、業務モジュールのアーキテクチャ境界を拡張しない。
- Aspireのパッケージ版やコンテナーイメージ版は実装上の初期値であり、長期運用に向けた更新方針は別途確認する。
- Compose環境に既存DBデータがあってもAspire側のDBへ自動移行しない。既存のComposeデータボリュームは削除せず、必要なデータ移行は別作業とする。
- 開発用のデータベース接続環境は定義するが、APIはまだDB接続を実装していない。

## 根拠・関連リンク

- [Aspire PostgreSQL hosting integration](https://aspire.dev/integrations/databases/postgres/postgres-host.md)
- [レイヤー・モジュール境界を決定論的に検査する](0001-enforce-architecture-boundaries.md)
