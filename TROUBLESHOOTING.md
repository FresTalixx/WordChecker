# Troubleshooting Guide - Word Checker WPF Application

## Issues and Solutions

### Issue 1: "I can't start the search"

#### Possible Causes and Solutions:

**1. Input File Not Selected**
   - Click "Browse..." next to "Input File (Forbidden Words)"
   - Select a text file containing one forbidden word per line
   - The file MUST exist and be readable

**2. Output Directory Not Selected**
   - Click "Browse..." next to "Output Directory"
   - Choose an existing directory or create a new one
   - The application will create it automatically if it doesn't exist

**3. Log Directory Not Selected**
   - Click "Browse..." next to "Log Directory"
   - Choose an existing directory or create a new one
   - The application will create it automatically if it doesn't exist

**4. Missing config.json**
   - The application looks for `config.json` in the working directory
   - If not found, it shows a configuration error but still works
   - Simply configure the paths using the Browse buttons

**5. No Ready Drives**
   - Ensure at least one drive is properly mounted and accessible
   - The application skips drives that are not ready
   - Check that USB drives/external drives are connected

---

## Step-by-Step Guide to Get Started

### 1. Prepare Forbidden Words File
   - Create a text file (e.g., `forbidden_words.txt`)
   - Add one forbidden word per line
   - Example content:
     ```
     password
     secret
     confidential
     classified
     ```

### 2. Create Output Directory
   - Create a folder where results will be saved
   - Example: `C:\SearchResults` or `D:\WordCheckerOutput`

### 3. Create Log Directory
   - Create a folder for log files
   - Example: `C:\Logs` or `D:\WordCheckerLogs`

### 4. Start the Application
   - Run `WordCheckerWPF.exe`
   - The UI should appear

### 5. Configure Paths
   - Click "Browse..." for Input File → Select your forbidden words file
   - Click "Browse..." for Output Directory → Select your output folder
   - Click "Browse..." for Log Directory → Select your logs folder

### 6. Start Search
   - Click the green "Start Search" button
   - Progress bars will appear for each drive
   - Wait for the search to complete

### 7. View Results
   - Results will appear in the main area
   - Statistics will display on the right sidebar
   - Top 10 words will be shown in the list

---

## Debug Information

If the application doesn't start or crashes:

1. **Check the Log File**
   - Navigate to your Log Directory
   - Look for `search-*.log` files
   - Open the latest log file in a text editor
   - Look for error messages

2. **Check Windows Event Viewer**
   - Press `Win + R` → type `eventvwr.msc`
   - Look for Application errors related to WordCheckerWPF

3. **Run from Command Line**
   - Open PowerShell
   - Navigate to the application directory
   - Run: `.\WordCheckerWPF.exe`
   - Any errors will display in the console

---

## Performance Tips

1. **Large Drives Take Time**
   - Searching entire drives (C:, D:, etc.) can take hours
   - Consider testing with a single folder first
   - The application doesn't support folder filtering yet

2. **Monitor Resources**
   - Open Task Manager (Ctrl + Shift + Esc)
   - Check CPU and Memory usage
   - The application uses parallel processing

3. **Stop Search Anytime**
   - Click the red "Stop" button if needed
   - Search will terminate gracefully
   - Partial results will be displayed

---

## Common Error Messages

### "Input file does not exist"
- **Solution**: Check the file path is correct and file exists

### "Please select an output directory"
- **Solution**: Click Browse and select or create a folder

### "Please select a log directory"
- **Solution**: Click Browse and select or create a folder

### "No ready drives found"
- **Solution**: Check that at least one drive is accessible

### "Failed to create directories"
- **Solution**: Check you have write permissions in the parent directory

---

## Feature Controls

### During Search:
- **Pause**: Temporarily suspends processing without losing progress
- **Resume**: Continues from where it was paused
- **Stop**: Cancels the entire search

### Result Display:
- Top 10 most frequent forbidden words
- Complete list of files containing forbidden words
- File paths and sizes
- Total statistics

---

## Sample Configuration

Create a `config.json` file in the same directory as the application:

```json
{
  "InputDirectory": "C:\\Users\\YourName\\forbidden_words.txt",
  "OutputDirectory": "C:\\Users\\YourName\\WordCheckerResults",
  "LogDirectory": "C:\\Users\\YourName\\WordCheckerLogs"
}
```

---

## Support Information

If you encounter issues:

1. Check the logs in your Log Directory
2. Ensure all paths are valid and accessible
3. Try with a fresh forbidden words file
4. Verify the input file encoding is UTF-8
5. Check available disk space in output directory
