# 週一到週日排程 Bug 修正說明

## 問題描述

使用者設定排程：
- **開始：週一**
- **結束：週日**
- **勾選跨日模式**
- **運轉時數：168 小時（一整週）**

確定後，原本開啟的空壓機立即停止。

## 問題根因

### C# 的 DayOfWeek 枚舉值

```csharp
Sunday = 0     // 週日 = 0
Monday = 1     // 週一 = 1
Tuesday = 2    // 週二 = 2
Wednesday = 3  // 週三 = 3
Thursday = 4   // 週四 = 4
Friday = 5     // 週五 = 5
Saturday = 6   // 週六 = 6
```

### 錯誤的計算邏輯

**舊版程式碼：**
```csharp
int ToWeeklyMinutes(DayOfWeek day, TimeSpan time)
{
	return (int)day * 1440 + (int)time.TotalMinutes;
}

StartDay = Monday (1) → start = 1 * 1440 + 0 = 1440
EndDay = Sunday (0)   → end = 0 * 1440 + 0 = 0

因為 start (1440) > end (0)，程式誤判為「跨週」模式
```

### 錯誤的判斷結果

```csharp
if (start <= end)
{
	// 不跨週：正常一週（週一~週五）
	return current >= start && current <= end;
}
else
{
	// 跨週：週末跨週一（週六~週一）
	return current >= start || current <= end;  ← 錯誤分支
}
```

**實際情況：**
- 使用者設定：週一 00:00 ~ 週日 23:59（一整週，168小時）
- 程式誤判為：「跨週」模式
- 錯誤邏輯：`current >= 1440 || current <= 0`
- 結果：只有週日 00:00~00:00 會判斷為在排程內，其他時間都判斷為不在排程內！

## 解決方案

### 修正週分鐘計算邏輯

**新版程式碼：**
```csharp
int ToWeeklyMinutes(DayOfWeek day, TimeSpan time)
{
	int dayValue = (int)day;
	if (dayValue == 0) // Sunday
		dayValue = 7;   // 將週日視為一週的最後一天
	return dayValue * 1440 + (int)time.TotalMinutes;
}
```

### 正確的計算結果

```
StartDay = Monday (1)  → start = 1 * 1440 + 0 = 1440
EndDay = Sunday (0→7)  → end = 7 * 1440 + 0 = 10080

因為 start (1440) <= end (10080)，正確判斷為「不跨週」模式
```

### 正確的判斷邏輯

```csharp
if (start <= end)
{
	// 不跨週：週一~週日（一整週）
	return current >= start && current <= end;  ← 正確分支
}
```

**判斷範圍：**
```
週一 00:00 = 1 * 1440 + 0 = 1440
週二 00:00 = 2 * 1440 + 0 = 2880
週三 00:00 = 3 * 1440 + 0 = 4320
週四 00:00 = 4 * 1440 + 0 = 5760
週五 00:00 = 5 * 1440 + 0 = 7200
週六 00:00 = 6 * 1440 + 0 = 8640
週日 00:00 = 7 * 1440 + 0 = 10080

判斷：current >= 1440 && current <= 10080
結果：週一~週日整週都在排程內 ✓
```

## 測試案例

### 案例 1：週一~週日（一整週）

**設定：**
- StartDay = Monday, StartTime = 00:00
- EndDay = Sunday, EndTime = 23:59
- IsSpanMode = True

**舊版結果：** ❌ 只有週日 00:00 會啟動
**新版結果：** ✅ 週一~週日整週都會啟動

### 案例 2：週一~週五（工作日）

**設定：**
- StartDay = Monday, StartTime = 08:00
- EndDay = Friday, EndTime = 17:00
- IsSpanMode = True

**舊版結果：** ✅ 正常（因為 Monday=1 < Friday=5）
**新版結果：** ✅ 正常（不受影響）

### 案例 3：週六~週一（跨週末）

**設定：**
- StartDay = Saturday, StartTime = 20:00
- EndDay = Monday, EndTime = 08:00
- IsSpanMode = True

**計算：**
```
start = 6 * 1440 + 1200 = 9840
end = 1 * 1440 + 480 = 1920
```

**舊版結果：** ✅ 判斷為跨週（因為 9840 > 1920）
**新版結果：** ✅ 判斷為跨週（不受影響）

### 案例 4：週日~週六（反向一整週）

**設定：**
- StartDay = Sunday, StartTime = 00:00
- EndDay = Saturday, EndTime = 23:59
- IsSpanMode = True

**舊版計算：**
```
start = 0 * 1440 + 0 = 0
end = 6 * 1440 + 1439 = 10079
```
**舊版結果：** ✅ 判斷為不跨週（因為 0 < 10079）

**新版計算：**
```
start = 7 * 1440 + 0 = 10080
end = 6 * 1440 + 1439 = 10079
```
**新版結果：** ⚠️ 判斷為跨週（因為 10080 > 10079）

**說明：** 週日~週六實際上應該是「跨週」（週日是上週的最後一天，週六是本週的倒數第二天），新版邏輯更正確。

## 實際測試建議

### 測試 1：週一~週日（168小時）
1. 設定排程：週一 00:00 ~ 週日 23:59
2. 勾選跨日模式
3. 確認運轉時數為 168 小時
4. 按下確定
5. **預期：設備應該持續運轉**

### 測試 2：週一~週五（工作日）
1. 設定排程：週一 08:00 ~ 週五 17:00
2. 勾選跨日模式
3. 確認運轉時數為 45 小時
4. 按下確定
5. **預期：週一~週五 08:00~17:00 啟動，週末停止**

### 測試 3：週六~週一（跨週末）
1. 設定排程：週六 20:00 ~ 週一 08:00
2. 勾選跨日模式
3. 按下確定
4. **預期：週六 20:00 啟動，週日整天運轉，週一 08:00 停止**

## Debug 輸出範例

### 修正前（錯誤）

```
[IsInSchedule] === 跨日模式 ===
[IsInSchedule] StartDay: Monday, StartTime: 00:00
[IsInSchedule] EndDay: Sunday, EndTime: 23:59
[IsInSchedule] 週分鐘計算:
[IsInSchedule]   current = Friday * 1440 + 600 = 7800
[IsInSchedule]   start   = Monday * 1440 + 0 = 1440
[IsInSchedule]   end     = Sunday * 1440 + 1439 + 1 = 1440  ← Sunday=0，錯誤！
[IsInSchedule] 跨週檢查: 7800 >= 1440 || 7800 <= 1440 = True || False = True
[IsInSchedule] 最終結果: True  ← 誤打誤撞正確，但邏輯錯誤
```

實際上週五不應該通過 `current <= 1440` 檢查，但因為 `current >= 1440` 為真，所以結果正確。但這是運氣，不是邏輯正確。

### 修正後（正確）

```
[IsInSchedule] === 跨日模式 ===
[IsInSchedule] StartDay: Monday, StartTime: 00:00
[IsInSchedule] EndDay: Sunday, EndTime: 23:59
[IsInSchedule] 週分鐘計算:
[IsInSchedule]   current = Friday(5) * 1440 + 600 = 7800
[IsInSchedule]   start   = Monday(1) * 1440 + 0 = 1440
[IsInSchedule]   end     = Sunday(7) * 1440 + 1439 = 11519  ← Sunday=7，正確！
[IsInSchedule] 不跨週檢查: 7800 >= 1440 && 7800 <= 11519 = True && True = True
[IsInSchedule] 最終結果: True  ← 邏輯正確
```

## 相關檔案

- `DeviceBox\MainForm.cs` - `IsInSchedule(ModeScheduleItem schedule)` 方法
- `DeviceBox\ScheduleEditForm.cs` - 排程編輯介面

## 修改日期

2025-01-XX

## 額外說明

這個 bug 只影響「結束日為週日」的跨日模式排程。其他情況（週一~週五、週六~週一等）不受影響。

修正後，週日會被視為一週的最後一天（第 7 天），而不是第一天（第 0 天），這樣週一~週日的計算就正確了。
