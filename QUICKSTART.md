# Word Checker WPF - Quick Start Guide

## ⚡ Quick Start (5 Minutes)

### Step 1: Prepare Your Files
```
1. Create a text file with forbidden words (one per line)
   Example file: forbidden_words.txt

   banned
   restricted
   private
   secret
```

### Step 2: Create Directories
```
Create two directories:
- Output: Where results will be saved
- Logs: Where search logs will be saved

Example:
C:\WordChecker\Output
C:\WordChecker\Logs
```

### Step 3: Run the Application
```
1. Launch WordCheckerWPF.exe
2. You'll see the configuration dialog
```

### Step 4: Configure Paths
```
In the "Configuration" section:

Input File:    Browse → Select your forbidden_words.txt
Output Dir:    Browse → Select your Output folder
Log Dir:       Browse → Select your Logs folder
```

### Step 5: Start Search
```
Click the green "Start Search" button
Wait for it to complete
```

### Step 6: View Results
```
Results appear in three places:
1. Left panel: Detailed list and grid
2. Right panel: Statistics (Files, Words, Top 10)
3. Status: Each drive shows its progress
```

---

## 🎮 Control Buttons

| Button | What it Does | When to Use |
|--------|-------------|-----------|
| **Start Search** | Begins searching all ready drives | Click first |
| **Pause** | Temporarily stops the search | When you need to pause |
| **Resume** | Continues from pause | To resume paused search |
| **Stop** | Completely cancels the search | To stop immediately |

---

## 📊 Understanding the Results

### Left Side (Main Results):
- **Progress Section**: Shows each drive's search status
- **Results Section**: 
  - Text summary of findings
  - DataGrid with all files found

### Right Side (Statistics):
- **Files Found**: How many files contain forbidden words
- **Total Forbidden Words**: Sum of all matches
- **Unique Words**: Number of different forbidden words found
- **Execution Time**: How long the search took
- **Top 10 Words**: Most frequently found words

---

## ⚙️ Configuration File (Optional)

If you want to save your settings, create `config.json`:

```json
{
  "InputDirectory": "C:\\path\\to\\forbidden_words.txt",
  "OutputDirectory": "C:\\path\\to\\output",
  "LogDirectory": "C:\\path\\to\\logs"
}
```

The app automatically loads and saves this file.

---

## 📝 Output Files

After a successful search, you'll find in your Output Directory:

1. **Original Files** (copied with original names)
   ```
   document.txt
   report.md
   ```

2. **Modified Files** (forbidden words replaced with ***)
   ```
   document(1).txt     ← Modified version
   report(1).md        ← Modified version
   ```

3. **Log Files** (in Log Directory)
   ```
   search-2024-01-15.log
   ```

---

## ⏱️ Search Time Expectations

| Scenario | Time |
|----------|------|
| Small folder (< 100 files) | Seconds |
| Single drive (1-2 TB) | 30 min - 2 hours |
| Multiple large drives | Several hours |

**Tip**: The first search scan is slower (file discovery). Subsequent searches are faster.

---

## 🆘 Common Issues

### "Start Search button won't work"
- ✅ Did you select an Input File?
- ✅ Did you select an Output Directory?
- ✅ Did you select a Log Directory?
- ✅ Does the Input File exist?

### "Search started but no progress showing"
- This is normal for the first few minutes
- File discovery takes time on large drives
- Check the drive status in Progress section

### "Search stopped unexpectedly"
- Check the logs in your Log Directory
- Look for error messages
- Ensure output directory has enough space

### "Can't find my results"
- Check the Output Directory you selected
- Look for files matching the ones that contained forbidden words
- Look in subdirectories if files had similar names

---

## 💡 Tips & Tricks

1. **Test First**
   - Create a test folder with a few files
   - Modify the Input to point to that folder
   - Test with a small set of words

2. **Use Meaningful Names**
   - Create folders like: `WordChecker-2024-01-15`
   - Easy to identify which search is which

3. **Save Logs**
   - Keep logs for documentation
   - Useful if you need to report issues

4. **Forbidden Words Format**
   - One word per line
   - No special characters
   - Blank lines are ignored

5. **Large Searches**
   - Don't close the application mid-search
   - If you need to stop, use the Stop button
   - Then wait for graceful shutdown

---

## 🔗 Application Files

After running, you'll have:

```
WordCheckerWPF/
├── config.json              ← Settings (auto-created)
├── search-*.log             ← Log files (in Log Directory)
└── [Output Directory]/      ← Your results
    ├── original_file1.txt
    ├── original_file1(1).txt ← Modified version
    └── ...
```

---

## 🚀 Next Steps

1. **Run a Test Search**
   - Create a small test folder
   - Add a few words to your forbidden list
   - See the app in action

2. **Full Drive Search**
   - Once comfortable, search your main drive
   - Monitor progress
   - Review results

3. **Automate** (Future Feature)
   - Schedule regular searches
   - Process multiple word lists
   - Generate reports

---

**Happy searching!** 🔍
