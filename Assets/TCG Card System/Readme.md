# TextMesh Pro - Required Dependency #1
Installation
The TextMesh Pro UPM package is already included with the Unity Editor and as such does not require installation. TextMesh Pro "TMP" does however require adding resources to your project which are essential for using TextMesh Pro.

To import the "TMP Essential Resources", please use the `Window -> TextMeshPro -> Import TMP Essential Resources` menu option. These resources will be added at the root of your project in the "TextMesh Pro" folder.

# UniTask - Required Dependency #2
### Install via git URL

Requires a version of unity that supports path query parameter for git packages (Unity >= 2019.3.4f1, Unity >= 2020.1a21). You can add `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask` to Package Manager

![image](https://user-images.githubusercontent.com/46207/79450714-3aadd100-8020-11ea-8aae-b8d87fc4d7be.png)

![image](https://user-images.githubusercontent.com/46207/83702872-e0f17c80-a648-11ea-8183-7469dcd4f810.png)

or add `"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"` to `Packages/manifest.json`.

If you want to set a target version, UniTask uses the `*.*.*` release tag so you can specify a version like `#2.1.0`. For example `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.1.0`.

You can find more informations here `https://github.com/Cysharp/UniTask`.

# 🎴 Adaptive Card System - Demo Scene

The **Demo Scene** provides a lightweight preview of the package, showcasing how the system handles card distribution, placement, and interactions. It includes a **BattleController** that simulates drawing cards, opponent plays, and attacks, giving you a clear starting point for your own game logic.

The core systems are **modular and easily extendable**, making it simple to customize or add new functionality as needed.

## ✨ Features

- **📜 Card Serving System** – Automatically draws and distributes cards from the deck.
- **🎴 Dynamic Card Fan (Hand) Adaptation** – Adjusts card layout based on hand size.
- **📍 Customizable Pointer System** – Easily modify for different interactions and targeting.
- **⚔️ Melee & Ranged Attack System** – Supports various attack types with flexible integration.
- **🎞️ Replaceable Attack Animations** – Swap out the default animations effortlessly.
- **🛠️ Card Placement System** – Manages smooth positioning and interactions.
- **🕹️ BattleController** – Simulates card actions like drawing, playing, and attacking.
- **🎨 Customizable Card Assets** – Includes ready-to-use card face and back templates.
- **🌄 Dynamic Background & Character Images** – Auto-adapts to fit the card face.

## 🏗️ Extending the System

The system is designed with **flexibility in mind**. You can easily:

- Add new card effects or interactions.
- Modify the **BattleController** to fit your game’s mechanics.
- Customize the card appearance and animations.
- Expand the attack system with new mechanics.

## 🚀 Getting Started

1. Open the **Demo Scene** in Unity.
2. Run the scene to see the card system in action.
3. Explore the **BattleController** and card scripts to understand how they work.
4. Modify and extend as needed to fit your game!

This demo scene is just the beginning—**customize it to create your own unique card game experience!** 🎮  
