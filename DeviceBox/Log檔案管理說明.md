# Log 檔案管理功能說明

## 功能概述

新增了兩個重要的 log 檔案管理功能：

1. **Log 開關** - 可以啟用或停用 log 檔案記錄
2. **自動清理** - 自動刪除超過保留天數的舊 log 檔案

## 設定方式

在 `MainForm.cs` 檔案中，有兩個常數可以調整：

```csharp
// ===== Log 檔案設定 =====
// 是否啟用 log 檔案記錄 (true=啟用, false=停用)
private const bool ENABLE_FILE_LOGGING = true;

// 自動清理舊 log 檔案的天數 (0 = 不自動清理)
private const int LOG_RETENTION_DAYS = 7;  // 保留最近 7 天的 log
// ======================
```

### 設定 1：ENABLE_FILE_LOGGING

**用途**：控制是否產生 log 檔案

**設定值**：
- `true` - 啟用 log (預設)
- `false` - 停用 log

**使用時機**：
- 開發或除錯時：設為 `true`
- 正式運作且不需要記錄時：設為 `false`

**範例**：
```csharp
// 停用 log 檔案
private const bool ENABLE_FILE_LOGGING = false;
```

### 設定 2：LOG_RETENTION_DAYS

**用途**：設定保留 log 檔案的天數

**設定值**：
- `0` - 不自動清理 (保留所有 log)
- `1-365` - 保留指定天數的 log

**建議值**：
- 測試環境：`3-7` 天
- 生產環境：`7-14` 天
- 長期監控：`30` 天

**範例**：
```csharp
// 保留最近 3 天的 log
private const int LOG_RETENTION_DAYS = 3;

// 保留最近 30 天的 log
private const int LOG_RETENTION_DAYS = 30;

// 不自動清理
private const int LOG_RETENTION_DAYS = 0;
```

## 運作方式

### Log 開關流程

```
程式啟動
  ↓
EnableFileLogging() 被調用
  ↓
檢查 ENABLE_FILE_LOGGING
  ↓
├─ true  → 繼續建立 log 檔案
└─ false → 停止，不建立 log
```

**當停用時**：
- 不會建立任何 log 檔案
- 節省磁碟空間
- 提升些許效能
- 仍會輸出到 Debug 視窗 (Visual Studio)

### 自動清理流程

```
程式啟動 (且 ENABLE_FILE_LOGGING = true)
  ↓
EnableFileLogging() 被調用
  ↓
CleanOldLogFiles() 被調用
  ↓
掃描 debug_*.log 檔案
  ↓
檢查每個檔案的最後修改時間
  ↓
刪除超過 LOG_RETENTION_DAYS 的檔案
  ↓
記錄刪除的檔案數量
  ↓
繼續建立新的 log 檔案
```

**清理規則**：
- 只清理 `debug_*.log` 格式的檔案
- 根據檔案的「最後修改時間」判斷
- 無法刪除的檔案會被忽略 (例如正在使用中)
- 清理動作在每次程式啟動時執行

## 使用範例

### 範例 1：正常使用 (預設設定)

```csharp
private const bool ENABLE_FILE_LOGGING = true;
private const int LOG_RETENTION_DAYS = 7;
```

**效果**：
- ✅ 建立 log 檔案
- ✅ 每次啟動時清理 7 天前的舊 log
- ✅ 最近 7 天的 log 會保留

### 範例 2：停用 log (節省空間)

```csharp
private const bool ENABLE_FILE_LOGGING = false;
private const int LOG_RETENTION_DAYS = 7;  // 這個設定不會被使用
```

**效果**：
- ❌ 不建立 log 檔案
- ❌ 不執行清理動作
- ✅ Debug 輸出仍然有效 (Visual Studio)

### 範例 3：保留所有 log (不自動清理)

```csharp
private const bool ENABLE_FILE_LOGGING = true;
private const int LOG_RETENTION_DAYS = 0;
```

**效果**：
- ✅ 建立 log 檔案
- ❌ 不自動清理
- ⚠️ 需要手動管理舊 log 檔案

### 範例 4：短期保留 (測試環境)

```csharp
private const bool ENABLE_FILE_LOGGING = true;
private const int LOG_RETENTION_DAYS = 3;
```

**效果**：
- ✅ 建立 log 檔案
- ✅ 只保留最近 3 天
- ✅ 快速釋放空間

## Log 檔案格式

**檔案名稱格式**：
```
debug_{ProcessID}_{DateTime}.log
```

**範例**：
```
debug_12345_20260703_143020.log
debug_67890_20260703_143025.log
```

**說明**：
- `12345` - 程序 ID (Process ID)
- `20260703` - 日期 (年月日)
- `143020` - 時間 (時分秒)

## 日誌訊息

### 啟用 log 時

```
[MainForm] Log file created: C:\...\debug_12345_20260703_143020.log
[MainForm] Cleaned up 5 old log file(s) older than 7 days
```

### 停用 log 時

```
[MainForm] File logging is disabled
```

### 清理失敗時

```
[MainForm] Failed to clean old log files: Access denied
```

## 故障排除

### 問題 1：log 檔案沒有被清理

**可能原因**：
1. `LOG_RETENTION_DAYS` 設為 `0`
2. 舊 log 檔案還不到保留天數
3. 檔案被其他程序使用中

**檢查方式**：
```powershell
# 檢查 log 檔案及修改時間
Get-ChildItem ".\bin\Debug\debug_*.log" | Select-Object Name, LastWriteTime | Sort-Object LastWriteTime -Descending
```

### 問題 2：log 檔案沒有產生

**可能原因**：
1. `ENABLE_FILE_LOGGING` 設為 `false`
2. 沒有寫入權限

**檢查方式**：
1. 確認設定：檢查 `ENABLE_FILE_LOGGING` 的值
2. 檢查 Debug 輸出：應該會看到 "File logging is disabled"
3. 檢查權限：確認程式對執行目錄有寫入權限

### 問題 3：占用空間太大

**解決方式**：
1. 減少 `LOG_RETENTION_DAYS`：
   ```csharp
   private const int LOG_RETENTION_DAYS = 3;  // 改為 3 天
   ```
2. 或者停用 log：
   ```csharp
   private const bool ENABLE_FILE_LOGGING = false;
   ```
3. 手動清理：
   ```powershell
   # 刪除所有 debug log
   Remove-Item ".\bin\Debug\debug_*.log"
   ```

### 問題 4：Debug 訊息看不到

**說明**：
- 停用 `ENABLE_FILE_LOGGING` 後，log 不會寫入檔案
- 但 Debug 訊息仍會輸出到 Visual Studio 的「輸出」視窗
- 如果在沒有 VS 的環境執行，Debug 訊息會消失

**解決方式**：
- 開發時：保持 `ENABLE_FILE_LOGGING = true`
- 除錯時：使用 Visual Studio 的「輸出」視窗
- 生產環境：根據需求決定是否啟用

## 效能影響

### 啟用 log 時

- **寫入效能**：微小影響 (異步寫入)
- **磁碟空間**：根據使用頻率和保留天數
- **啟動時間**：額外 10-50ms (清理舊檔案)

### 停用 log 時

- **寫入效能**：無影響
- **磁碟空間**：不占用
- **啟動時間**：無額外開銷

## 建議設定

### 開發環境

```csharp
private const bool ENABLE_FILE_LOGGING = true;
private const int LOG_RETENTION_DAYS = 3;
```

### 測試環境

```csharp
private const bool ENABLE_FILE_LOGGING = true;
private const int LOG_RETENTION_DAYS = 7;
```

### 生產環境 (需要監控)

```csharp
private const bool ENABLE_FILE_LOGGING = true;
private const int LOG_RETENTION_DAYS = 14;
```

### 生產環境 (穩定運作)

```csharp
private const bool ENABLE_FILE_LOGGING = false;
private const int LOG_RETENTION_DAYS = 0;
```

## 手動清理腳本

如果需要手動管理 log 檔案，可以使用以下 PowerShell 腳本：

```powershell
# 清理 7 天前的 log
$days = 7
$path = ".\bin\Debug"
$cutoff = (Get-Date).AddDays(-$days)
Get-ChildItem "$path\debug_*.log" | 
	Where-Object { $_.LastWriteTime -lt $cutoff } | 
	Remove-Item -Force
```

## 重要提示

1. **修改設定後需要重新編譯**
   - 這些是 `const` 常數，在編譯時決定
   - 修改後必須重新建置專案

2. **正在執行的 log 檔案不會被刪除**
   - 自動清理會跳過無法刪除的檔案
   - 關閉所有實例後再清理會更徹底

3. **建議定期檢查磁碟空間**
   - 即使啟用自動清理，仍應定期檢查
   - 特別是在長時間運作的環境

4. **備份重要的 log**
   - 如果需要長期保存某些 log
   - 在自動清理前先備份到其他位置
