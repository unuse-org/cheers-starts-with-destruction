# 画像ベース数字表示のセットアップ手順

このドキュメントは、Unity Editorで画像ベース数字表示を完成させるための手順を説明します。

## 完了した作業

- ✅ `NumberDisplaySettings.cs` (ScriptableObject) を作成
- ✅ `ImageNumberDisplay.cs` (コアコンポーネント) を作成
- ✅ `CompoundNumberDisplay.cs` (複合表示用) を作成
- ✅ すべての数字画像 (0-9.png) を Sprite モードに変換
- ✅ `GameUI.cs` の6箇所の数字表示を置き換え
- ✅ `ScoreUI.cs` のスコア表示を置き換え

## Unity Editorでの必須作業

### 1. NumberDisplaySettings アセットの作成

1. Unity Editorを開く
2. Project ウィンドウで `Assets/Data/UI/` フォルダを作成（存在しない場合）
3. `Assets/Data/UI/` を右クリック → Create → CheersGame → NumberDisplaySettings
4. 作成されたアセット名を `DefaultNumberDisplaySettings` に変更
5. Inspectorで以下を設定：
   - **Digit Sprites (0-9)**:
     - Element 0: `Assets/Art/UI/Numbers/0.png`
     - Element 1: `Assets/Art/UI/Numbers/1.png`
     - Element 2: `Assets/Art/UI/Numbers/2.png`
     - Element 3: `Assets/Art/UI/Numbers/3.png`
     - Element 4: `Assets/Art/UI/Numbers/4.png`
     - Element 5: `Assets/Art/UI/Numbers/5.png`
     - Element 6: `Assets/Art/UI/Numbers/6.png`
     - Element 7: `Assets/Art/UI/Numbers/7.png`
     - Element 8: `Assets/Art/UI/Numbers/8.png`
     - Element 9: `Assets/Art/UI/Numbers/9.png`
   - **Spacing**: 10
   - **Default Scale**: 1

### 2. GameSceneの更新

#### カウントダウン表示

1. `GameScene.unity` を開く
2. Hierarchy で `CountdownText` GameObject を探す
3. 新しい GameObject を作成（名前: `CountdownDisplay`）
4. `CountdownDisplay` に `ImageNumberDisplay` コンポーネントを追加
5. Inspector で設定：
   - Settings: `DefaultNumberDisplaySettings` をドラッグ＆ドロップ
   - RectTransform: `CountdownText` と同じ位置・アンカー設定
6. `GameUI` の Inspector で：
   - `Countdown Display` フィールドに `CountdownDisplay` をドラッグ
7. 元の `CountdownText` を削除または非アクティブ化

#### マイルストーン表示

1. Hierarchy で `MilestoneText` GameObject を探す（`_milestoneImage` の子）
2. 新しい GameObject を作成（名前: `MilestoneDisplay`）
3. `MilestoneDisplay` に `ImageNumberDisplay` コンポーネントを追加
4. Inspector で設定：
   - Settings: `DefaultNumberDisplaySettings` をドラッグ＆ドロップ
   - RectTransform: `MilestoneText` と同じ位置・アンカー設定
5. `GameUI` の Inspector で：
   - `Milestone Display` フィールドに `MilestoneDisplay` をドラッグ
6. 元の `MilestoneText` を削除または非アクティブ化

#### ゲーム中スコア表示

1. Hierarchy で `ScoreNumberText` GameObject を探す（GameScreen内）
2. 新しい GameObject を作成（名前: `ScoreNumberDisplay`）
3. `ScoreNumberDisplay` に `ImageNumberDisplay` コンポーネントを追加
4. Inspector で設定：
   - Settings: `DefaultNumberDisplaySettings` をドラッグ＆ドロップ
   - RectTransform: `ScoreNumberText` と同じ位置・アンカー設定
5. `GameUI` の Inspector で：
   - `Score Number Display` フィールドに `ScoreNumberDisplay` をドラッグ
6. 元の `ScoreNumberText` を削除または非アクティブ化

#### 撃破数表示

1. Hierarchy で `DefeatCountText` GameObject を探す
2. 新しい GameObject を作成（名前: `DefeatCountDisplay`）
3. `DefeatCountDisplay` に `CompoundNumberDisplay` コンポーネントを追加
4. `DefeatCountDisplay` の子として以下を作成：
   - `PrefixText` (TextMeshProUGUI)
   - `NumberDisplay` (ImageNumberDisplay)
5. `PrefixText` の設定：
   - Text: "撃破: "
   - Font: NotoSansJP SDF
   - 位置調整
6. `NumberDisplay` の設定：
   - Settings: `DefaultNumberDisplaySettings`
7. `CompoundNumberDisplay` の Inspector で：
   - Prefix Text: `PrefixText` をドラッグ
   - Number Display: `NumberDisplay` をドラッグ
8. `GameUI` の Inspector で：
   - `Defeat Count Display` フィールドに `DefeatCountDisplay` をドラッグ
9. 元の `DefeatCountText` を削除または非アクティブ化

#### 耐久値表示

1. Hierarchy で `DurabilityText` GameObject を探す
2. 新しい GameObject を作成（名前: `DurabilityDisplay`）
3. `DurabilityDisplay` に `CompoundNumberDisplay` コンポーネントを追加
4. `DurabilityDisplay` の子として以下を作成：
   - `CurrentNumberDisplay` (ImageNumberDisplay)
   - `MiddleText` (TextMeshProUGUI)
   - `MaxNumberDisplay` (ImageNumberDisplay)
5. `CurrentNumberDisplay` の設定：
   - Settings: `DefaultNumberDisplaySettings`
6. `MiddleText` の設定：
   - Text: " / "
   - Font: NotoSansJP SDF
7. `MaxNumberDisplay` の設定：
   - Settings: `DefaultNumberDisplaySettings`
8. `CompoundNumberDisplay` の Inspector で：
   - Number Display: `CurrentNumberDisplay` をドラッグ
   - Middle Text: `MiddleText` をドラッグ
   - Second Number Display: `MaxNumberDisplay` をドラッグ
9. `GameUI` の Inspector で：
   - `Durability Display` フィールドに `DurabilityDisplay` をドラッグ
10. 元の `DurabilityText` を削除または非アクティブ化

### 3. ScoreSceneの更新

#### リザルトスコア表示

1. `GameScene.unity` を開く（ScoreScreen部分）
2. Hierarchy で `ScoreNumberText` GameObject を探す（ScoreScreen内）
3. 新しい GameObject を作成（名前: `ScoreNumberDisplay`）
4. `ScoreNumberDisplay` に `ImageNumberDisplay` コンポーネントを追加
5. Inspector で設定：
   - Settings: `DefaultNumberDisplaySettings` をドラッグ＆ドロップ
   - RectTransform: `ScoreNumberText` と同じ位置・アンカー設定
6. `ScoreUI` の Inspector で：
   - `Score Number Display` フィールドに `ScoreNumberDisplay` をドラッグ
7. 元の `ScoreNumberText` を削除または非アクティブ化

## レイアウト調整のヒント

### HorizontalLayoutGroup の推奨設定

各 `ImageNumberDisplay` には自動的に `HorizontalLayoutGroup` が追加されます。
推奨設定（InspectorでデフォルトでOK）：

- **Padding**: 0
- **Spacing**: 10 (NumberDisplaySettingsから自動設定)
- **Child Alignment**: Middle Center
- **Child Control Size**: Width ✅, Height ✅
- **Child Force Expand**: Width ❌, Height ❌

### 位置調整

数字画像は元のTextMeshProと同じ位置・アンカーに配置してください：
- Anchor: 元のテキストと同じ
- Pivot: Center (0.5, 0.5)
- Position: 元のテキストと同じ

### スケール調整

数字が大きすぎる/小さすぎる場合：
1. RectTransformのScaleを調整
2. または `DefaultNumberDisplaySettings` の `Default Scale` を変更

## 検証項目

シーンを再生して以下を確認：

- [ ] カウントダウンが正しく表示される（3, 2, 1）
- [ ] マイルストーンバナーが右からスライドイン
- [ ] スコアがバネ振動で出現
- [ ] リザルト画面のスコアがバネポップイン
- [ ] 撃破数が"撃破: X"形式で表示
- [ ] 耐久値が"X / Y"形式で表示
- [ ] 桁数の変化（1桁→2桁→3桁）が正しく動作
- [ ] アルファ・スケールアニメーションが滑らか
- [ ] コンパイルエラーがない

## トラブルシューティング

### 数字が表示されない

1. `DefaultNumberDisplaySettings` が正しく設定されているか確認
2. 各 `ImageNumberDisplay` の `Settings` フィールドにアセットが設定されているか確認
3. 数字画像 (0-9.png) が Sprite モードになっているか確認

### レイアウトが崩れる

1. `HorizontalLayoutGroup` の設定を確認
2. 親GameObjectのRectTransformサイズを調整
3. Anchorとpivotが正しく設定されているか確認

### アニメーションが動作しない

1. `GameUI` / `ScoreUI` のInspectorで参照が正しく設定されているか確認
2. RectTransformが正しくアタッチされているか確認

## 完了後

すべての検証項目がクリアされたら：

```bash
git add .
git commit -m "feat: 画像ベース数字表示システムを実装"
```

以上でセットアップは完了です！
