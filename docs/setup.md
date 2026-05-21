# セットアップ手順

## 1. Git LFS のインストール

このリポジトリはフォントや `.asset` ファイルを Git LFS で管理しています。
clone 前に以下を実行してください。

```bash
# Homebrew の場合
brew install git-lfs

# 初回のみ
git lfs install
```

その後、通常通り clone します。

```bash
git clone <リポジトリURL>
```

> 既に clone 済みで LFS ファイルが取得できていない場合は `git lfs pull` を実行してください。

## 2. Unity プロジェクトを開く

Unity Hub から Unity **6000.3.9f1** でプロジェクトを開いてください。

## 3. 日本語フォントの設定（初回のみ）

フォントファイル (`Assets/Fonts/NotoSansJP-VariableFont_wght.ttf`) は配置済みです。
SDF Font Asset の生成が必要です。

1. **Window > TextMeshPro > Font Asset Creator** を開く
2. 以下を設定:
   - **Source Font File**: `Assets/Fonts/NotoSansJP-VariableFont_wght.ttf`
   - **Sampling Point Size**: Auto Sizing
   - **Padding**: 5
   - **Packing Method**: Fast
   - **Atlas Resolution**: 4096 x 4096
   - **Character Set**: Characters from File
   - **Character File**: `Assets/Fonts/japanese_characters.txt`
   - **Render Mode**: SDFAA
3. **Generate Font Atlas** → **Save** → `Assets/Fonts/NotoSansJP SDF.asset`
4. **Edit > Project Settings > TextMesh Pro** を開き、**Default Font Asset** を `NotoSansJP SDF` に変更
