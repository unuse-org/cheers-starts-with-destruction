# タスク管理 - 乾杯の音頭は破壊から

## 現在のフェーズ: Phase 1 - コア実装

---

## Phase 1: コア実装

### 入力システム
- [x] `ISensorInput` インターフェース定義
- [x] `CheersInputData`, `VoiceInputData` 構造体定義
- [x] `MockSensorInput` 実装（キーボード入力でテスト可能に）

### グラスシステム
- [x] `GlassData` ScriptableObject定義
- [x] `PlayerGlass` 実装（耐久値管理）
- [x] ビールジョッキのデータアセット作成（Unity Editor上で作成が必要）

### ゲームフロー
- [x] `GameManager` 実装（状態遷移管理）
- [x] シーン構成（Title, Game, Score）← シングルシーン＋UIパネル切り替え方式を採用
- [x] 基本的な画面遷移

### 仮UI
- [x] `TitleUI` 実装
- [x] `GameUI` 実装（耐久値表示、撃破数表示）
- [x] `ScoreUI` 実装

### 日本語フォント
- [x] Noto Sans JP フォントファイル配置（`Assets/Fonts/`）
- [x] 日本語文字リスト作成（`Assets/Fonts/japanese_characters.txt`）
- [ ] Font Asset 生成（Unity Editor で手動作業）
- [ ] TMP Settings デフォルトフォント変更（Unity Editor で手動作業）

---

## Phase 2: バトル実装

### NPCシステム
- [ ] `NPCData` ScriptableObject定義
- [ ] `NPCController` 実装
- [ ] NPC3種類のデータアセット作成（仮パラメータ）

### タイミングシステム
- [ ] `TimingSystem` 実装
- [ ] タイミングガイドUI実装
- [ ] `TimingGrade` 判定ロジック

### 乾杯判定
- [ ] `BattleManager` 実装
- [ ] 攻撃力計算（タイミング × 声量）
- [ ] 勝敗判定ロジック
- [ ] ダメージ処理

---

## Phase 3: 演出・統合

### 視覚フィードバック
- [ ] `VisualFeedback` 実装
- [ ] ガラス破片パーティクル
- [ ] 画面揺れ演出

### 音声フィードバック
- [ ] `AudioFeedback` 実装
- [ ] SE実装（破壊音、失敗音、歓声など）

### 統合
- [ ] センサー実装統合（センサー担当と連携）
- [ ] デザインアセット統合（デザイン担当と連携）

---

## Phase 4: 調整・ポリッシュ

- [ ] バランス調整（ダメージ値、閾値、タイミング幅）
- [ ] バグ修正
- [ ] 展示対応

---

## 完了タスク

（完了したタスクはここに移動）

---

## レビュー・メモ

（各フェーズ完了時のレビューや気づきを記載）
