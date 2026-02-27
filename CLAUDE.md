# CLAUDE.md - 乾杯の音頭は破壊から

## プロジェクト概要

Unity製の体感型アクションゲーム「乾杯の音頭は破壊から」の開発プロジェクト。
実物のビールジョッキ（M5系センサー内蔵）を振ってNPCと乾杯バトルを行う。

**詳細仕様**: `docs/仕様書_乾杯の音頭は破壊から.md`

---

## ワークフロー設計

### 1. Planモードを基本とする
- 3ステップ以上 or アーキテクチャに関わるタスクは必ずPlanモードで開始する
- 途中でうまくいかなくなったら、無理に進めずすぐに立ち止まって再計画する
- 構築だけでなく、検証ステップにもPlanモードを使う
- 曖昧さを減らすため、実装前に詳細な仕様を書く

### 2. サブエージェント戦略
- メインのコンテキストウィンドウをクリーンに保つためにサブエージェントを積極的に活用する
- リサーチ・調査・並列分析はサブエージェントに任せる
- 複雑な問題には、サブエージェントを使ってより多くの計算リソースを投入する
- 集中して実行するために、サブエージェント1つにつき1タスクを割り当てる

### 3. 自己改善ループ
- ユーザーから修正を受けたら必ず `tasks/lessons.md` にそのパターンを記録する
- 同じミスを繰り返さないように、自分へのルールを書く
- ミス率が下がるまで、ルールを徹底的に改善し続ける
- セッション開始時に、そのプロジェクトに関連するlessonsをレビューする

### 4. 完了前に必ず検証する
- 動作を証明できるまで、タスクを完了とマークしない
- 必要に応じてmainブランチと自分の変更の差分を確認する
- 「スタッフエンジニアはこれを承認するか？」と自問する
- テストを実行し、ログを確認し、正しく動作することを示す

### 5. エレガントさを追求する（バランスよく）
- 重要な変更をする前に「もっとエレガントな方法はないか？」と一度立ち止まる
- ハック的な修正に感じたら「今知っていることをすべて踏まえて、エレガントな解決策を実装する」
- シンプルで明白な修正にはこのプロセスをスキップする（過剰設計しない）
- 提示する前に自分の作業に自問自答する

### 6. 自律的なバグ修正
- バグレポートを受けたら、手取り足取り教えてもらわずにそのまま修正する
- ログ・エラー・失敗しているテストを見て、自分で解決する
- ユーザーのコンテキスト切り替えをゼロにする
- 言われなくても、失敗しているCIテストを修正しに行く

---

## タスク管理

1. **まず計画を立てる**：チェック可能な項目として `tasks/todo.md` に計画を書く
2. **計画を確認する**：実装を開始する前に確認する
3. **進捗を記録する**：完了した項目を随時マークしていく
4. **変更を説明する**：各ステップで高レベルのサマリーを提供する
5. **結果をドキュメント化する**：`tasks/todo.md` にレビューセクションを追加する
6. **学びを記録する**：修正を受けた後に `tasks/lessons.md` を更新する

---

## コア原則

- **シンプル第一**：すべての変更をできる限りシンプルにする。影響するコードを最小限にする。
- **手を抜かない**：根本原因を見つける。一時的な修正は避ける。シニアエンジニアの水準を保つ。
- **影響を最小化する**：変更は必要な箇所のみにとどめる。バグを新たに引き込まない。

---

## プロジェクト固有ルール

### Unity固有
- スクリプトは `Assets/Scripts/` 以下に仕様書のコンポーネント構成に従って配置
- ScriptableObjectアセットは `Assets/Data/` に配置
- Prefabは `Assets/Prefabs/` に配置
- センサー入力は必ず `ISensorInput` インターフェース経由でアクセス
- 新規スクリプト作成時は対応する名前空間を使用

```
namespace CheersGame.Input { }
namespace CheersGame.Data { }
namespace CheersGame.Game { }
namespace CheersGame.UI { }
namespace CheersGame.Feedback { }
```

### フォルダ構成

```
Assets/
├── Scripts/
│   ├── Input/
│   │   ├── ISensorInput.cs
│   │   ├── MockSensorInput.cs
│   │   └── RealSensorInput.cs      # センサー担当が実装
│   ├── Data/
│   │   ├── GlassData.cs
│   │   └── NPCData.cs
│   ├── Game/
│   │   ├── GameManager.cs
│   │   ├── BattleManager.cs
│   │   ├── TimingSystem.cs
│   │   ├── PlayerGlass.cs
│   │   └── NPCController.cs
│   ├── UI/
│   │   ├── TitleUI.cs
│   │   ├── GameUI.cs
│   │   └── ScoreUI.cs
│   └── Feedback/
│       ├── VisualFeedback.cs
│       └── AudioFeedback.cs
├── Data/                           # ScriptableObjectアセット
├── Prefabs/
├── Scenes/
└── Art/                            # デザイン担当領域
```

### チーム連携・責任範囲

| 担当 | 責任範囲 | 触れてよいファイル |
|------|----------|-------------------|
| システム担当 | ゲームロジック、UI、フロー | `Scripts/`（RealSensorInput.cs以外）、`Scenes/`、`Prefabs/` |
| センサー担当 | センサーハードウェア、通信 | `Scripts/Input/RealSensorInput.cs` のみ |
| デザイン担当 | キャラクター、3Dモデル、UI素材 | `Assets/Art/`、ScriptableObjectアセットの設定値 |

**重要**: 他担当の領域を変更する必要がある場合は、事前に相談すること。

### インターフェース規約

センサー担当との連携のため、以下のインターフェースは変更しない：

```csharp
public interface ISensorInput
{
    event Action<CheersInputData> OnCheersDetected;
    event Action<VoiceInputData> OnVoiceDetected;
}

public struct CheersInputData
{
    public float Velocity;
    public float Angle;
    public float Timestamp;
}

public struct VoiceInputData
{
    public float Volume;
    public float Timestamp;
}
```

インターフェースの変更が必要な場合は、センサー担当と合意の上で `docs/仕様書_乾杯の音頭は破壊から.md` を先に更新する。

### 開発時の注意

- センサーなしでテスト可能な状態を常に維持する（MockSensorInput使用）
- ScriptableObjectでデータを管理し、ハードコードを避ける
- マジックナンバーは定数化または設定ファイルに外出しする
- 拡張性を意識する（グラス種類、NPC種類の追加を想定）

### プラットフォーム

- ターゲット: macOS
- Unity バージョン: （プロジェクト作成時に記載）
- センサー通信: Bluetooth（センサー担当が実装）

---

## 参照ドキュメント

- **ゲーム仕様**: `docs/仕様書_乾杯の音頭は破壊から.md`
- **タスク管理**: `tasks/todo.md`
- **学び記録**: `tasks/lessons.md`
