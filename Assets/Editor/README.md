# 🧹 UnityTools – Missing Scripts Cleaner

A custom Unity Editor utility that helps you find and remove **missing script references** from scenes and prefabs, keeping your project clean and error-free.

---

## 🚀 Features

* Clean missing scripts from:

  * ✅ Selected or all **scenes**
  * ✅ Prefabs in **specific folders** (optionally including subfolders)
* Filter prefabs and scenes using a **search query**
* **Bulk-select** prefabs and scenes for cleaning
* Automatically generates a detailed **log file**
* Clean and modern **EditorWindow UI**
* Safe to use: changes are only made to selected items

---

## 📋 How to Use

1. Place the script inside the `Assets/Editor` folder in your Unity project.
2. Open or reload Unity to compile the script.
3. In Unity Editor, go to: `Tools > Missing Scripts Cleaner`.
4. In the window, you can:

   * Select scenes or folders you want to scan.
   * Use the toggle to **include subfolders** for prefabs.
   * **Search** for specific prefabs or scenes.
   * Review and select individual items before cleaning.
   * Click `Clean` to remove all missing scripts from selected objects.

---

## ⚠️ Important Notes

* The tool does **not automatically clean all assets** — you must choose scenes/prefabs manually, unless you enable full project cleaning.
* Always ensure you’ve committed or backed up changes before running bulk cleanup.
* Cleaning scenes will open and save them via `EditorSceneManager`, so **unsaved changes may be lost**.

---

## 📁 Log Output

* Every cleaning operation is logged to:

  ```
  Assets/MissingScriptsLog.txt
  ```
* You can review the log inside the window itself or open it in any text editor.

---

## 🧠 Behind the Scenes

* Uses Unity’s `GameObjectUtility.RemoveMonoBehavioursWithMissingScript` to safely remove missing scripts.
* Integrates with Unity’s `AssetDatabase` and `EditorSceneManager` for scene and prefab manipulation.
* All operations are performed in the Editor — no runtime impact.

---

## 🤝 Contributions & Support

* Issues and feature requests are welcome on GitHub.
* Feel free to fork and improve the tool — PRs are appreciated!

---

## 🧰 Example Use Cases

* Cleaning corrupted prefabs imported from external tools
* Preparing scenes for production by removing broken references
* Keeping legacy projects tidy as scripts are deprecated

---

*UnityTools © 2025 – Clean Code, Clean Projects*