# 🧩 UnityTools – Sprite Atlas Builder

A custom Unity Editor utility that lets you quickly generate Sprite Atlases from selected GameObjects or Sprites, with full control over packing, texture settings, and platform-specific compression overrides.

---

## 🚀 Features

Create Sprite Atlases from:

✅ A selected GameObject
✅ All SpriteRenderers inside its children
✅ All UI Images inside its children
✅ A single selected Sprite

Atlas configuration options:

* Tight Packing toggle
* Allow Rotation toggle
* Padding control
* Block Offset control

Texture configuration options:

* Read/Write toggle
* Generate MipMaps toggle
* sRGB Color Space toggle
* Filter Mode selection

Platform Overrides:

* Add any number of platform override entries
* Choose platform (BuildTargetGroup)
* Enable/disable override
* Set Max Texture Size
* Set Texture Format
* Set Compression Quality
* Remove individual overrides
* Clear all overrides
* Collapsible section for organized UI

Asset handling:

* Set custom atlas name
* Set save folder path
* Folder picker support
* Auto-create default folder if the path is invalid
* Creates or replaces .spriteatlas assets
* Automatically adds collected sprites to the atlas

Clean, structured Editor UI with collapsible groups and validation.

---

## 📋 How to Use

1. Place the script inside your **Assets/Editor** directory.
2. Open or reload Unity to compile it.
3. Integrate the tool into an EditorWindow and call `DrawUI()` (or use your existing menu integration).
4. In the tool window, you can:

   * Select a GameObject or Sprite
   * Configure packing settings
   * Configure texture settings
   * Add or remove platform overrides
   * Choose atlas name and save path
   * Click **Create Sprite Atlas** to generate the asset

The tool will create a new `.spriteatlas` file and apply all specified settings.

---

## ⚠️ Important Notes

* Only sprites collected from the selected object will be added.
* The tool will create the **SpriteAtlases** folder automatically if needed.
* If an atlas with the same name exists, it will be replaced.
* No runtime behavior is affected, and all operations run strictly inside the Unity Editor.

---

## 📁 Output

The tool generates:

`YourPath/YourAtlasName.spriteatlas`

The atlas includes all collected sprites and the full configuration defined in the UI.

---

## 🧠 Behind the Scenes

The tool uses:

* `SpriteAtlas`, `SpriteAtlasPackingSettings`, `SpriteAtlasTextureSettings`
* `TextureImporterPlatformSettings`
* Unity’s `AssetDatabase` for asset creation and updating
* Automatic sprite collection from `SpriteRenderer` and `UI.Image` components

All operations run entirely within the Unity Editor and do not affect runtime performance.

---

## 🤝 Contributions & Support

Contributions, issues, and feature requests are welcome.
Feel free to fork the tool and enhance it for your pipeline.

---

## 🧰 Example Use Cases

* Building optimized UI atlases
* Organizing sprites for complex UI screens
* Preparing platform-specific atlases for mobile
* Automating the atlas creation workflow for designers
