# CyberBot 🛡️

# Project Name

**CyberBot** — A Cybersecurity Awareness Chatbot (WPF Desktop Application)

# Description

CyberBot is a desktop chatbot built in C# (WPF) that helps users learn good cybersecurity habits through natural conversation. It started as a simple keyword-based chatbot in Parts 1 and 2, and has been extended in Part 3 with four major features:

- A **Task Assistant** for managing cybersecurity to-do items with reminders, backed by a MySQL database
- A **Cybersecurity Quiz** mini-game with 12 questions covering passwords, phishing, VPNs, and more
- A **Natural Language Processing (NLP) simulation** that recognises user requests phrased in different ways
- An **Activity Log** that records and displays the bot's recent actions

The chatbot still supports its original features from Parts 1 and 2: keyword-based topic responses, sentiment detection, memory of a user's favourite topic, and spoken responses via text-to-speech.

# How to Open and Run the Project

1. Open **Visual Studio** (2022 or later recommended).
2. Open the solution file `CyberBot.sln`.
3. Wait for NuGet packages to restore (this happens automatically on first open — see *Software Required* below if it doesn't).
4. Make sure your MySQL server is running first (see *Database Setup Instructions* below) — the app will show a warning on startup if it can't connect.
5. Press `Ctrl+Shift+B` to build the solution, then `F5` (or click **Start**) to run.
6. Enter your name in the field at the top and click **Start Chat** to begin.

## Software Required

| Tool | Purpose |
|---|---|
| **Visual Studio 2022+** (Windows) | IDE used to build and run the WPF project |
| **.NET Framework / .NET Desktop Development workload** | Required for WPF projects — install via Visual Studio Installer if missing |
| **XAMPP** | Provides the local MySQL/MariaDB server and phpMyAdmin |
| **MySqlConnector** (NuGet package) | C# driver used to connect to MySQL — restores automatically with the project |

## Database Setup Instructions

CyberBot stores tasks in a local MySQL database. Follow these steps on a fresh machine:

1. **Install XAMPP** from [https://www.apachefriends.org](https://www.apachefriends.org) and run the installer with default settings.
2. **Open the XAMPP Control Panel** and click **Start** next to both **Apache** and **MySQL**. Both rows should turn green and show a port number.
3. **Open phpMyAdmin** by visiting `http://localhost/phpmyadmin/` in your browser.
4. **Create the database**: click **New** in the left sidebar, name it `cyberbot`, and click **Create**.
5. **Create the table**: select the `cyberbot` database, open the **SQL** tab, paste the following, and click **Go**:

```sql
CREATE TABLE tasks (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    reminder_date DATETIME NULL,
    is_completed BOOLEAN NOT NULL DEFAULT FALSE,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

> **Note:** The app also runs this same `CREATE TABLE IF NOT EXISTS` statement automatically on startup via `Database.cs`, so even if you skip step 5, the table will be created the first time you run the app — as long as the `cyberbot` database itself exists (step 4) and MySQL is running.

6. **Install the NuGet package** (if not already restored): in Visual Studio, right-click the project → **Manage NuGet Packages** → **Browse** tab → search `MySqlConnector` → **Install**. Make sure the **Package source** dropdown is set to **nuget.org**, not "Microsoft Visual Studio Offline Packages."

## How to Use the Task Assistant

**Via the Tasks tab:**
1. Click the **Tasks** tab.
2. Enter a task title (required) and an optional description.
3. Tick **Set a reminder** and enter a number of days if you want a reminder date attached.
4. Click **➕ Add Task**.
5. Select any task in the list to enable the **✔ Mark as Completed** or **🗑 Delete Task** buttons.

**Via the Chat tab (NLP):** type things like:
- `add task enable 2FA`
- `remind me to update my password in 3 days`
- `show tasks`
- `complete task enable 2FA`

The bot will confirm the action in the chat, and the Tasks tab updates immediately.

## How to Access the Quiz / Mini-Game

1. Click the **Quiz** tab.
2. Click **▶ Start Quiz**.
3. Answer each of the 12 questions (multiple-choice or true/false) by clicking an option.
4. The bot gives instant feedback and a short explanation, then automatically moves to the next question.
5. After the final question, your score and a performance message are shown, and also posted to the Chat tab.

You can also type `start quiz` in the Chat tab to jump straight to the Quiz tab.

## How to Test the NLP Simulation

The NLP layer recognises differently worded requests for the same intent. Try typing any of these into the Chat tab to see it in action:

| Try typing... | Recognised as... |
|---|---|
| `add a task to enable 2FA` | Add task |
| `can you remind me to update my password tomorrow` | Add reminder |
| `show my tasks` / `what tasks do I have` | View tasks |
| `quiz me` / `play quiz` | Start quiz |
| `show activity log` / `what have you done for me` | View activity log |
| `mark complete enable 2FA` | Complete a task |

The bot detects these using keyword-group matching (`string.Contains()`-based) rather than requiring an exact command, so slightly different phrasing of the same request should still work.

## How to View the Activity Log

- Click the **Activity Log** tab to see a running list of the bot's recent actions (tasks added, reminders set, quiz attempts, NLP commands recognised, etc.), newest first.
- By default the last 5 actions are shown — click **Show More** to reveal 5 more at a time.
- Click **🗑 Clear Log** to reset it.
- Alternatively, type `show activity log` or `what have you done for me` in the Chat tab for a quick text summary without switching tabs.

## Login Details / Important Notes

- **No login or authentication is required** — simply enter any name to start a chat session.
- **Default database credentials**: the app connects to MySQL using `User ID=root` with **no password**, which is the default XAMPP configuration. If your MySQL setup uses different credentials, update the connection string at the top of `Database.cs`.
- The app expects MySQL to be running on `localhost:3306` — if XAMPP's MySQL is not started, the app will still launch but will show a warning and the Task Assistant will not be able to save data until the database is reachable.
- Text-to-speech requires Windows' built-in speech engine (`System.Speech`) — no extra setup needed on a standard Windows install.

## Video Presentation

📺 [Insert YouTube link to presentation video here]

## Project Structure (for reference)

```
CyberBot/
├── MainWindow.xaml        # GUI layout — Chat, Tasks, Quiz, Activity Log tabs
├── MainWindow.xaml.cs     # All event handling, chat logic, NLP, quiz logic
├── Database.cs            # MySQL connection and CRUD operations
├── TaskItem.cs             # Task model class
└── README.md              # This file
```
