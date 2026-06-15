# 專案風格指南

## 語言規範

- 所有回答一律使用**繁體中文**
- 以下內容除外：
  - **專業名詞**：保持英文原詞（如：Component、Prefab、MonoBehaviour、MCP、Unity Editor 等）
  - **程式碼**：包含變數名稱、函數名稱、類別名稱等一律使用英文
  - **檔案路徑**：使用英文路徑
  - **指令與終端機輸出**：保持原本的輸出內容

## 程式碼風格

- 遵循 Unity C# 程式碼規範
- 類別名稱使用 PascalCase
- 類別成員變數使用駝峰式命名（camelCase），私有變數以下底線開頭（_variableName）
- 公開屬性使用 PascalCase
- 函數名稱使用 PascalCase

## 命名約定

| 類型 | 命名方式 | 範例 |
|------|----------|------|
| 類別 | PascalCase | EnemyController、TowerManager |
| 介面 | PascalCase（以 I 開頭） | IAmmoProvider、IDamageable |
| 公開欄位/屬性 | PascalCase | Health、MaxAmmo |
| 私有欄位 | camelCase（以下底線開頭） | _currentHealth、_ammoCount |
| 函數 | PascalCase | Shoot()、TakeDamage() |
| 事件 | PascalCase（以 Event 結尾） | OnEnemyDefeated、OnTowerBuilt |

## Unity 專案結構

- `Assets/Scripts/`：存放所有 C# 腳本
- `Assets/Prefabs/`：存放 Prefab 資源
- `Assets/Scenes/`：存放場景檔案
- `Assets/Sprites/`：存放 2D 圖資
- `Assets/Audio/`：存放音效檔案
- `Assets/Data/`：存放 ScriptableObject 數據

## 提交規範

- Git Commit 使用英文描述
- Commit 格式：`type(scope): description`
  - type：feat（新功能）、fix（修復）、refactor（重構）、docs（文件）、style（格式）、test（測試）
  - scope：模組名稱或檔案範圍
  - description：簡短描述變更內容
