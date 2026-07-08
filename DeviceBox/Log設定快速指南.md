# Log 檔案管理 - 快速設定指南

## 🚀 快速設定

在 `DeviceBox\MainForm.cs` 檔案的最上方找到以下設定：

```csharp
// ===== Log 檔案設定 =====
private const bool ENABLE_FILE_LOGGING = true;
private const int LOG_RETENTION_DAYS = 7;
// ======================
```

## 📋 常見設定組合

### 1️⃣ 停用 Log (節省空間)

```csharp
private const bool ENABLE_FILE_LOGGING = false;
private const int LOG_RETENTION_DAYS = 7;  // 這行不會生效
```

✅ 適用於：穩定運作的生產環境
💾 磁碟空間：不占用
⚡ 效能：最佳

---

### 2️⃣ 短期保留 (3天)

```csharp
private const bool ENABLE_FILE_LOGGING = true;
private const int LOG_RETENTION_DAYS = 3;
```

✅ 適用於：開發和測試環境
💾 磁碟空間：少量占用
⚡ 效能：略有影響

---

### 3️⃣ 標準保留 (7天) ⭐ 推薦

```csharp
private const bool ENABLE_FILE_LOGGING = true;
private const int LOG_RETENTION_DAYS = 7;
```

✅ 適用於：一般使用
💾 磁碟空間：中等占用
⚡ 效能：略有影響

---

### 4️⃣ 長期保留 (30天)

```csharp
private const bool ENABLE_FILE_LOGGING = true;
private const int LOG_RETENTION_DAYS = 30;
```

✅ 適用於：需要長期追蹤的環境
💾 磁碟空間：較大占用
⚡ 效能：略有影響

---

### 5️⃣ 保留所有 Log (不清理)

```csharp
private const bool ENABLE_FILE_LOGGING = true;
private const int LOG_RETENTION_DAYS = 0;
```

✅ 適用於：重要系統需要完整記錄
💾 磁碟空間：持續增長
⚠️ 警告：需要手動管理

---

## 🔧 修改步驟

1. 打開 `DeviceBox\MainForm.cs` 檔案
2. 找到最上方的 `// ===== Log 檔案設定 =====` 區塊
3. 修改常數值
4. 儲存檔案
5. **重新建置專案** (F6)
6. 重新啟動程式

⚠️ **重要**：修改後必須重新編譯才會生效！

---

## 📊 磁碟空間估算

假設每個 log 檔案約 1-5 MB：

| 設定天數 | 每天啟動次數 | 預估總大小 |
|---------|------------|----------|
| 3 天    | 5 次       | 15-75 MB |
| 7 天    | 5 次       | 35-175 MB |
| 14 天   | 5 次       | 70-350 MB |
| 30 天   | 5 次       | 150-750 MB |

---

## 🗑️ 手動清理 Log 檔案

如果需要立即清理所有 log：

### Windows 檔案總管
1. 開啟 `DeviceBox\bin\Debug` 資料夾
2. 搜尋 `debug_*.log`
3. 選取並刪除

### PowerShell 指令
```powershell
# 進入專案目錄
cd "DeviceBox\bin\Debug"

# 刪除所有 debug log
Remove-Item "debug_*.log"

# 或者刪除 7 天前的 log
$cutoff = (Get-Date).AddDays(-7)
Get-ChildItem "debug_*.log" | 
	Where-Object { $_.LastWriteTime -lt $cutoff } | 
	Remove-Item
```

---

## ❓ 常見問題

### Q: 修改設定後沒有生效？
A: 請確認有重新建置專案 (F6)。這些是編譯時常數。

### Q: 停用 log 後還會有 Debug 訊息嗎？
A: 在 Visual Studio 的「輸出」視窗仍會看到，但不會寫入檔案。

### Q: 多久清理一次舊 log？
A: 每次程式啟動時會自動執行一次清理。

### Q: 可以在運行時切換開關嗎？
A: 不行，必須修改程式碼並重新編譯。

---

## 📞 需要幫助？

查看完整說明文件：
- `DeviceBox\Log檔案管理說明.md`
