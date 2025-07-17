# 🎲 Rolling-Dice Multiplayer Game

Rolling-Dice is a turn-based multiplayer board game developed in Unity using Photon PUN 2. It features synchronized dice rolling, AI or player-versus-player gameplay, camera tracking, and custom tile behaviors like jumps and finish tiles.

---

## 📌 Features

- 🎮 **2-Player Multiplayer Support** (Photon PUN 2)
- 🧠 **AI Match Support** (Single-player vs AI)
- 🎲 **Synchronized Dice Animation**
- 🗺️ **Tile-Based Movement System**
- 🎥 **Dynamic Camera Following**
- 🧩 **Tile Abilities** (Move forward/backward, finish condition, etc.)
- 🗨️ **UI with AI Toggle, Timer, and Feedback**
- 🧠 **Turn Manager** for AI and Online Matches
- 🏁 **End Condition** on reaching final tile

---

## 🛠️ Technologies Used

- **Unity** (Version: 2021.3+ recommended)
- **Photon PUN 2** for networking
- **DOTween** for dice animation effects
- **TextMeshPro** for UI text

---

## 📂 Project Structure
Assets/
├── _Scripts/
│ ├── Dice/
│ │ └── DiceRoller.cs
│ ├── Game/
│ │ ├── GameManager.cs
│ │ └── TileAbility.cs
│ ├── Network/
│ │ ├── PhotonManager.cs
│ │ └── RPCManager.cs
│ └── UI/
│ ├── LobbyUI.cs
│ └── CameraController.cs
├── Scenes/
│ ├── Lobby.unity
│ └── Game.unity
└── Plugins/
└── DOTween

---

## 🚀 How to Run

1. Open the project in **Unity**.
2. Go to `Assets/Scenes/Lobby.unity`.
3. Press **Play** to enter the Lobby.
4. Choose **AI Match** or **Multiplayer Match**.
5. Press **Start** to load the game scene.

> ⚠️ Photon App ID must be set in `PhotonServerSettings`.

---

## 🤖 AI vs Player

- The **AI Toggle** in the lobby enables single-player matches.
- AI rolls the dice and moves based on the same logic as a human player.

---

## 🔄 Multiplayer Mode

- Powered by Photon PUN 2.
- Player turns are synced using actor numbers.
- Dice values and movement are replicated through RPCs.

---

## 🧪 Testing Tips

- Use **Unity Editor** and **build** to simulate multiplayer locally.
- Check Photon Console Logs for actor numbers and connection issues.
- Use `Debug.Log()` in key scripts like `GameManager`, `DiceRoller`, and `PhotonManager`.

---

## 📸 Screenshots

> *(Include gameplay/lobby UI screenshots here if available)*

---

## 📃 License

This project is for educational and prototyping purposes. Modify and extend as needed.

---

## 👤 Author

**Aman Chauhan**  
Unity Developer with experience in multiplayer, AI, Firebase, Web3, and gameplay systems.

