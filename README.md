# 🎮 UnityTools - PlayerPrefs Manager

A custom Unity Editor window tool that allows you to easily view, search, edit, and delete PlayerPrefs stored on your machine.

---

## 🚀 Features

- Displays all non-default PlayerPrefs keys stored in the Windows registry  
- Search functionality to filter keys by name  
- Edit PlayerPrefs values supporting `int`, `float`, and `string` types  
- Quickly delete PlayerPrefs keys  
- User-friendly resizable window interface  
- Specifically designed for Windows (uses Windows Registry access)

---

## 📋 How to Use

1. Place the script inside the `Assets/Editor` folder in your Unity project.  
2. Open or reload Unity to compile the editor script.  
3. In Unity Editor, navigate to the menu: `Tools > PlayerPrefs Manager` to open the window.  
4. Within the window, you can:  
   - View all stored PlayerPrefs keys.  
   - Filter keys using the search bar.  
   - Edit or delete keys directly.  
   - Click the Refresh button to reload the keys list.

---

## ⚠️ Important Notes

- This tool is **Windows-only** because it reads PlayerPrefs directly from the Windows Registry.  
- It will not work on macOS or Linux.  
- Always back up important PlayerPrefs data before making changes.  
- Editing or deleting PlayerPrefs can affect your game’s behavior, so use with caution.

---

## 📞 Support & Contribution

- Report bugs or request features via GitHub Issues.  
- Contributions are welcome! Please submit Pull Requests following the repository’s contribution guidelines.

---

## 📝 Technical Overview

The PlayerPrefs Manager uses Unity’s `EditorWindow` class and reads PlayerPrefs data from the Windows Registry path where Unity stores them. Data is kept in a dictionary and displayed in a scrollable window. Users can edit or delete keys, with changes immediately saved back to PlayerPrefs.

---

*UnityTools © 2025*
