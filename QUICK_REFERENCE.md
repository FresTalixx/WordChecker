# 📱 Word Checker WPF - Quick Reference Card

## 🚀 START HERE (3 Steps)

```
Step 1: Open WordCheckerWPF.exe
Step 2: Click Browse buttons and select your paths
Step 3: Click "Start Search" (green button)
```

---

## 📍 Paths to Know

| Item | Path |
|------|------|
| **App Location** | `C:\Users\frest\source\repos\WordChecker\WordCheckerWPF\bin\Debug\net10.0-windows\WordCheckerWPF.exe` |
| **Config Auto-Saves** | `C:\Users\frest\source\repos\WordChecker\config.json` |
| **Your Forbidden Words** | `C:\forbidden_words.txt` (your choice) |
| **Output Results** | `C:\WordChecker\Output` (your choice) |
| **Log Files** | `C:\WordChecker\Logs` (auto-created) |

---

## 🎯 UI Button Guide

```
┌─────────────────────────────────────────┐
│ INPUT FILE: [      Browse...     ]      │
│ OUTPUT DIR: [      Browse...     ]      │
│ LOG DIR:    [      Browse...     ]      │
└─────────────────────────────────────────┘

[Start Search] [Pause] [Resume] [Stop]
     🟢         🟡      🔵      🔴
     Before    During  While   Anytime
     Search    Search  Paused

┌─────────────────────────────────────────┐
│ RESULTS                    │ STATISTICS │
│ ........................   │ Files: 0   │
│ ........................   │ Words: 0   │
│ ........................   │ Unique: 0  │
│ Top 10 Words Found:        │ Time: 0.0s │
│ - word1: 45 matches        │            │
│ - word2: 32 matches        │ ▀▀▀▀▀▀▀▀  │
│ - word3: 28 matches        │ Top 10:    │
└─────────────────────────────────────────┘
```

---

## 📝 What to Put in Forbidden Words File

**File: `C:\forbidden_words.txt`**

```
password
secret
confidential
classified
banned
restricted
private
internal
proprietary
```

**Rules:**
- One word per line
- Any case (Password, PASSWORD, password all match)
- Spaces are trimmed (auto-handled)
- Empty lines ignored
- UTF-8 encoding recommended

---

## ⚙️ Configuration (Auto-Saved)

**File: `config.json`**

```json
{
  "InputDirectory": "C:\\forbidden_words.txt",
  "OutputDirectory": "C:\\WordChecker\\Output",
  "LogDirectory": "C:\\WordChecker\\Logs"
}
```

After first run, these paths auto-load next time!

---

## 📊 Result Files

**In Output Directory, You'll Find:**

```
C:\WordChecker\Output\
├── file1.txt              (Original file, unchanged)
├── file1_MASKED.txt       (Copy with ******* instead of words)
├── file2.txt              (Original)
├── file2_MASKED.txt       (Masked copy)
└── search_results.csv     (Summary - if enabled)
```

---

## 🔍 Search Progress Display

```
Drive C:\
████████████████████████████ 100%
Status: Completed - 145 files found

Drive D:\
██████░░░░░░░░░░░░░░░░░░░░░ 23%
Status: Searching...

Drive E:\
░░░░░░░░░░░░░░░░░░░░░░░░░░░ 0%
Status: Waiting...
```

---

## 📋 Results Summary

After search completes:

```
┌─────────────────────────────────────┐
│ SEARCH COMPLETED SUCCESSFULLY!      │
│                                     │
│ Total Files Found:           145    │
│ Total Forbidden Words:     2,345    │
│ Unique Forbidden Words:       23    │
│ Execution Time:           12.5 sec  │
│                                     │
│ FILES FOUND:                        │
│ - C:\Users\admin\Documents\...      │
│ - C:\Users\admin\Pictures\...       │
│ - D:\Projects\CodeRepo\...          │
│ (143 more files)                    │
└─────────────────────────────────────┘
```

---

## ⏱️ Timing Guide

| Item | Time |
|------|------|
| Startup | <1 second |
| Small folder (100 files) | 5-10 seconds |
| Medium search (10K files) | 2-5 minutes |
| Large drive (100K files) | 10-30 minutes |
| Very large drive (1M files) | 1-3 hours |

**💡 Start with small tests!**

---

## 🔧 Controls During Search

| Button | Does What |
|--------|-----------|
| **Pause** | Stops temporarily, saves progress |
| **Resume** | Continues from where paused |
| **Stop** | Cancels entire search gracefully |

**Note:** During pause, don't close the app!

---

## 📊 Top 10 Words Display

Example right-side panel:

```
FILES FOUND
45

TOTAL WORDS
1,234

UNIQUE WORDS
67

TIME ELAPSED
12.5 sec

TOP 10 WORDS
┌──────────────┐
│ password: 245│
│ secret: 189  │
│ banned: 156  │
│ admin: 134   │
│ test: 98     │
│ debug: 87    │
│ temp: 76     │
│ delete: 65   │
│ backup: 54   │
│ archive: 43  │
└──────────────┘
```

---

## 🐛 If Something Goes Wrong

### Can't Click Browse Button?
```
Check: Is input field empty?
Fix: Click in the field first, then Browse
```

### Can't Select Directory?
```
Check: Do you have write permissions?
Fix: Choose a folder in your Documents/Downloads
```

### Search Doesn't Start?
```
Check: Did you select all 3 items (Input, Output, Log)?
Fix: Click Browse for each one
```

### Results Are Empty?
```
Check: Did your forbidden words file have content?
Fix: Verify at least one word in the file
```

### Search Seems Frozen?
```
Check: How many files? (Very large drives take hours)
Fix: Wait longer, or check logs for progress
```

### No Log Files?
```
Check: Can you write to the log directory?
Fix: Try a different folder with write permissions
```

---

## 💡 Pro Tips

1. **Test First**
   ```
   Before searching entire drive:
   1. Create small test folder
   2. Put 5 files in it
   3. Run search
   4. Verify results
   ```

2. **Save Good Configs**
   ```
   config.json is auto-saved
   Next run loads same paths
   Change Browse paths as needed
   ```

3. **Monitor Logs**
   ```
   Check logs while searching:
   - dir C:\WordChecker\Logs\
   - Opens search-*.log
   - Look for ERROR for issues
   ```

4. **Batch Searches**
   ```
   Run multiple times with different word files:
   - Create forbidden1.txt, forbidden2.txt
   - Run search with first file
   - Run again with second file
   - Compare results
   ```

5. **Archive Results**
   ```
   After each search:
   1. Copy output folder
   2. Rename with date (Output_2024-01-15)
   3. Keep for records
   ```

---

## 🎓 Understanding Results

**Original Files:** Keep unchanged versions for reference  
**Masked Files:** Have ******* instead of forbidden words  
**Top 10 Words:** Most frequently found words  
**Statistics:** Total count, unique count, time taken  
**File List:** Complete list of all matching files  

---

## 🔐 Safety Notes

- ✅ Original files never modified
- ✅ All results in separate directory
- ✅ Can safely cancel anytime
- ✅ No data deleted
- ✅ Results can be deleted after review

---

## 📞 Quick Help

| Issue | Solution |
|-------|----------|
| Won't start | Select all 3 paths, click Start |
| Too slow | Test with smaller folder first |
| No results | Check forbidden words file content |
| Can't find output | Look in Output Directory you selected |
| Crash on startup | Check config.json for valid paths |

---

## 🎯 Success Checklist

- [ ] Forbidden words file created
- [ ] Output directory exists (or let app create)
- [ ] Log directory exists (or let app create)
- [ ] App launches without error
- [ ] Can select Input File
- [ ] Can select Output Directory
- [ ] Can select Log Directory
- [ ] Start Search button works
- [ ] Progress bars appear
- [ ] Search completes
- [ ] Results display
- [ ] Files saved correctly
- [ ] Logs created successfully

✅ All checked? **You're ready to search!**

---

**Version:** .NET 10.0 WPF  
**Status:** ✅ Ready to Use  
**Last Updated:** Today
