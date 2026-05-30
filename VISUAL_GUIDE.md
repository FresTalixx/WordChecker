# 🎯 Word Checker WPF - Visual Guide

## Application Layout

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Word Checker Application                         │
├─────────────────────────────────────────────────┬───────────────────┤
│                                                 │   STATISTICS      │
│  CONFIGURATION SECTION                          │  ┌─────────────┐  │
│  ┌─────────────────────────────────────────┐   │  │ Files Found:│  │
│  │ Input File: [___________] [Browse...]   │   │  │     45      │  │
│  │ Output Dir: [___________] [Browse...]   │   │  ├─────────────┤  │
│  │ Log Dir:    [___________] [Browse...]   │   │  │ Words Found:│  │
│  └─────────────────────────────────────────┘   │  │   1,234     │  │
│                                                 │  ├─────────────┤  │
│  SEARCH PROGRESS SECTION                        │  │ Unique:     │  │
│  ┌─────────────────────────────────────────┐   │  │      67     │  │
│  │ C: [████████████░] Searching...        │   │  ├─────────────┤  │
│  │ D: [████████░░░░░] Waiting...          │   │  │ Time: 12.5s │  │
│  │ E: [░░░░░░░░░░░░░] Waiting...          │   │  ├─────────────┤  │
│  └─────────────────────────────────────────┘   │  │ TOP 10      │  │
│                                                 │  │ password: 45│  │
│  CONTROL BUTTONS                                │  │ secret: 34  │  │
│  [Start Search] [Pause] [Resume] [Stop]        │  │ banned: 28  │  │
│                                                 │  │ ...         │  │
│  RESULTS SECTION                                │  └─────────────┘  │
│  ┌─────────────────────────────────────────┐   │                   │
│  │ Search completed successfully!          │   │                   │
│  │ Total files found: 45                   │   │                   │
│  │ Total words found: 1,234                │   │                   │
│  │ Files:                                  │   │                   │
│  │ - C:\Users\Documents\secret.txt         │   │                   │
│  │ - D:\Data\password.md                   │   │                   │
│  │ - E:\Archive\banned_list.txt            │   │                   │
│  │                                         │   │                   │
│  │ [DataGrid showing files with metadata]  │   │                   │
│  └─────────────────────────────────────────┘   │                   │
└─────────────────────────────────────────────────┴───────────────────┘
```

---

## State Transitions

```
                    ┌─────────────────────────┐
                    │   INITIAL STATE         │
                    │ All buttons disabled    │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │  VALIDATION            │
                    │ Check all paths valid  │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │  DIR CREATION          │
                    │ Create missing dirs    │
                    └────────────┬────────────┘
                                 │
        ┌───────────────────────▼────────────────────────┐
        │         SEARCH IN PROGRESS                     │
        │  ┌──────────────┐    ┌──────────────┐         │
        │  │ Start Disabled│    │Pause Enabled │         │
        │  │Resume Enabled│    │Stop Enabled  │         │
        │  └──────────────┘    └──────────────┘         │
        │         │              │         │             │
        │         │   [Pause]    │         │             │
        │         └──────────────┬─────────┘             │
        │                        ▼                        │
        │              ┌─────────────────┐              │
        │              │  PAUSED STATE   │              │
        │              │Resume can now   │              │
        │              │start processing │              │
        │              └──────┬──────┬──┘              │
        │                    │      │                  │
        │            [Resume]│      │[Stop]            │
        │                    │      │                  │
        │         ┌──────────▼──┐   │                  │
        │         │ RESUMED     │   │                  │
        │         │ Processing  │   │                  │
        │         └─────────────┘   │                  │
        │                            ▼                  │
        │                   ┌──────────────┐            │
        │                   │ CANCELLED    │            │
        │                   │ Cleanup      │            │
        │                   └──────┬───────┘            │
        └─────────────────────────┼──────────────────────┘
                                  │
                    ┌─────────────▼────────────┐
                    │   COMPLETION            │
                    │ Display results         │
                    │ Re-enable Start button  │
                    └─────────────────────────┘
```

---

## Data Flow During Search

```
┌──────────────────┐
│ Forbidden Words  │
│   File (*.txt)   │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  HashSet<string> │ ◄─── Parse lines, ignore blanks
└────────┬─────────┘
         │
         ▼
┌──────────────────────────────────────────┐
│      WordsSearcher.SearchWords           │
│  (Running on each drive in parallel)     │
└──────────────┬───────────────────────────┘
               │
        ┌──────┴──────┬──────────┐
        ▼             ▼          ▼
    ┌─────────┐  ┌─────────┐ ┌─────────┐
    │ Drive C │  │ Drive D │ │ Drive E │
    └────┬────┘  └────┬────┘ └────┬────┘
         │             │          │
         ▼             ▼          ▼
    ┌─────────────────────────────────────┐
    │  File Discovery & Content Scanning  │
    │  - ReadAllText                      │
    │  - Regex.Matches                    │
    │  - Count occurrences                │
    └────────┬────────────────────────────┘
             │
        ┌────┴─────────────────┐
        ▼                      ▼
   ┌─────────┐            ┌──────────────────┐
   │ Results │            │ Word Count Dict  │
   │ List    │            │ (aggregated)     │
   └────┬────┘            └──────┬───────────┘
        │                        │
        │                    ┌───┴───────────┐
        │                    ▼               ▼
        │              ┌───────────┐   ┌────────────┐
        │              │ Top 10    │   │ Statistics │
        │              │ Words     │   │ Display    │
        │              └───────────┘   └────────────┘
        │
        ▼
   ┌──────────────────────────────────────┐
   │    Output Directory                  │
   │  ┌────────────────────────────────┐ │
   │  │ Original Files (copied)        │ │
   │  │ ├─ file1.txt                   │ │
   │  │ ├─ file2.md                    │ │
   │  │ └─ file3.txt                   │ │
   │  │                                │ │
   │  │ Modified Files (words replaced)│ │
   │  │ ├─ file1(1).txt                │ │
   │  │ ├─ file2(1).md                 │ │
   │  │ └─ file3(1).txt                │ │
   │  └────────────────────────────────┘ │
   └──────────────────────────────────────┘

   ┌──────────────────────────────────────┐
   │    Log Directory                     │
   │  ├─ search-2024-01-15.log            │
   │  └─ search-2024-01-16.log            │
   └──────────────────────────────────────┘
```

---

## Button States

### Initial State (Before Search)
```
[✓ Start Search]  [✗ Pause]  [✗ Resume]  [✗ Stop]
```

### During Search
```
[✗ Start Search]  [✓ Pause]  [✗ Resume]  [✓ Stop]
```

### Paused
```
[✗ Start Search]  [✗ Pause]  [✓ Resume]  [✓ Stop]
```

### After Completion
```
[✓ Start Search]  [✗ Pause]  [✗ Resume]  [✗ Stop]
```

---

## File Naming Convention

After search completes:

```
Original Files:
  document.txt
  report.md
  data.txt

Modified Files (renamed):
  document(1).txt      ← "document" already exists, so becomes (1)
  report(1).md
  data(1).txt

If you search multiple times:
  document(2).txt      ← Second copy becomes (2)
  document(3).txt      ← Third copy becomes (3)
  ... and so on
```

---

## Progress Bar States

```
Normal state:
┌─────────────────────────────────────┐
│ C: [░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 0%    Waiting...
│ D: [░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 0%    Waiting...
└─────────────────────────────────────┘

During search:
┌─────────────────────────────────────┐
│ C: [████████████░░░░░░░░░░░░░░░░░░] 40%   Searching...
│ D: [░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 0%    Waiting...
└─────────────────────────────────────┘

Completed:
┌─────────────────────────────────────┐
│ C: [████████████████████████████████] 100%  Completed - 23 files found
│ D: [████████████████████████████████] 100%  Completed - 15 files found
└─────────────────────────────────────┘

With cancellation:
┌─────────────────────────────────────┐
│ C: [████████░░░░░░░░░░░░░░░░░░░░░░] 25%   Cancelled
│ D: [░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 0%    Cancelled
└─────────────────────────────────────┘
```

---

## Error States

```
Configuration Error:
┌──────────────────────────────┐
│ ⚠️ Configuration Error        │
│ Could not load config.json   │
│ [OK]                         │
└──────────────────────────────┘

Validation Error:
┌──────────────────────────────┐
│ ⚠️ Validation Error          │
│ Input file does not exist    │
│ [OK]                         │
└──────────────────────────────┘

Search Error:
┌──────────────────────────────┐
│ ⚠️ Search Error              │
│ Error details...             │
│ Stack trace...               │
│ [OK]                         │
└──────────────────────────────┘
```

---

## Timeline Example

```
12:00:00 - User clicks "Start Search"
          └─ Directories created
          └─ Config saved
          └─ Logger initialized

12:00:05 - Progress shows:
          C: Searching... (files discovered)
          D: Waiting...
          E: Waiting...

12:05:30 - Progress shows:
          C: [████████░░░░] 40% Searching...
          D: [████░░░░░░░░] 15% Searching...
          E: [██░░░░░░░░░░] 5%  Searching...

12:15:00 - User clicks "Pause"
          C: [████████░░░░] 42% Paused
          D: [████░░░░░░░░] 18% Paused
          E: [██░░░░░░░░░░] 8%  Paused

12:15:30 - User clicks "Resume"
          C: [████████░░░░] 42% Searching...
          D: [████░░░░░░░░] 18% Searching...
          E: [██░░░░░░░░░░] 8%  Searching...

12:30:00 - All drives complete:
          C: [████████████] 100% Completed - 45 files found
          D: [████████████] 100% Completed - 23 files found
          E: [████████████] 100% Completed - 12 files found

          Results Display:
          Files Found: 80
          Words Found: 1,234
          Unique Words: 67
          Execution Time: 30:00 minutes
          Top 10 Words: [displayed]
```

---

**Use this visual guide to understand the application flow!** 🎯
