using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CyberBot
{
    public partial class MainWindow : Window
    {
        // ══════════════════════════════════════════════════════════════════════
        //  FIELDS
        // ══════════════════════════════════════════════════════════════════════
        private string _userName = "";
        private string _lastTopic = "";
        private string _favouriteTopic = "";
        private bool _chatStarted = false;
        private readonly Random _random = new Random();
        private readonly HashSet<int> _usedIndexes = new HashSet<int>();
        private readonly SpeechSynthesizer _speech = new SpeechSynthesizer();

        // ── Activity log ─────────────────────────────────────────────────────
        private readonly List<string> _activityLog = new List<string>();
        private int _logDisplayCount = 5;

        // ── Pending task reminder state (NLP multi-turn) ─────────────────────
        private bool _awaitingReminder = false;
        private string _pendingTaskTitle = "";
        private string _pendingTaskDesc = "";

        // ══════════════════════════════════════════════════════════════════════
        //  QUIZ DATA
        // ══════════════════════════════════════════════════════════════════════
        private List<QuizQuestion> _questions;
        private int _quizIndex = 0;
        private int _quizScore = 0;
        private bool _quizActive = false;

        private class QuizQuestion
        {
            public string Question { get; set; }
            public string[] Options { get; set; }   // null = True/False
            public int CorrectIndex { get; set; }
            public string Explanation { get; set; }
            public bool IsTrueFalse { get; set; }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CYBERSECURITY RESPONSES  (Part 1 & 2 preserved in full)
        // ══════════════════════════════════════════════════════════════════════
        private readonly Dictionary<string, List<string>> _responses =
            new Dictionary<string, List<string>>
            {
                ["password"] = new List<string>
            {
                "Use at least 12 characters with uppercase, lowercase, numbers and symbols.",
                "Never reuse passwords across accounts — use a unique password for every site.",
                "Avoid using personal info like birthdays or pet names in passwords.",
                "Consider using a password manager like Bitwarden to store them safely.",
                "A passphrase like 'PurpleCat$Runs@Night' is strong and easier to remember.",
                "Change your passwords immediately if you suspect a breach.",
                "Never share your password with anyone, even someone claiming to be IT support."
            },
                ["phishing"] = new List<string>
            {
                "Be cautious of emails asking for personal info — legitimate companies rarely do this.",
                "Always check the sender's full email address, not just the display name.",
                "If an email feels urgent or threatening, slow down — that is a manipulation tactic.",
                "Hover over links before clicking to see where they actually lead.",
                "Look out for spelling mistakes and poor grammar — common signs of phishing.",
                "Never download attachments from unknown senders.",
                "Phishing can also happen via SMS (smishing) or phone calls (vishing)."
            },
                ["privacy"] = new List<string>
            {
                "Review your privacy settings on all apps and social media regularly.",
                "Limit the personal information you share publicly online.",
                "Use private browsing mode when on shared or public computers.",
                "Read app permissions carefully — does a flashlight app really need your contacts?",
                "Turn off location tracking on apps that do not actually need it.",
                "Use a private search engine like DuckDuckGo to reduce data tracking.",
                "Regularly audit which third-party apps have access to your accounts."
            },
                ["scam"] = new List<string>
            {
                "If something sounds too good to be true, it almost always is.",
                "Never send money or gift cards to someone you have only met online.",
                "Report scams to your country's cybercrime authority immediately.",
                "Be wary of unsolicited calls claiming to be from your bank or Microsoft.",
                "Online shopping scams often use fake websites that look like real stores.",
                "Always verify charity organisations before donating, especially after disasters."
            },
                ["vpn"] = new List<string>
            {
                "A VPN encrypts your internet connection, keeping your data private.",
                "Always use a VPN when connecting to public Wi-Fi in cafes or airports.",
                "Choose a reputable paid VPN — free ones often sell your data to advertisers.",
                "VPNs hide your IP address, making it harder for sites to track your location.",
                "A VPN does not make you fully anonymous — combine it with safe browsing habits.",
                "Check if your VPN has a no-logs policy, meaning they do not store your activity."
            },
                ["antivirus"] = new List<string>
            {
                "Keep your antivirus software updated to detect the latest threats.",
                "Run full system scans regularly, not just the quick scan.",
                "Antivirus alone is not enough — pair it with a firewall and safe habits.",
                "Be careful with free antivirus tools — some are actually malware themselves.",
                "Real-time protection is a key feature to look for in antivirus software.",
                "Antivirus cannot protect you from clicking on a phishing link — stay alert."
            },
                ["public wifi"] = new List<string>
            {
                "Avoid logging into sensitive accounts like banking on public Wi-Fi.",
                "Use a VPN on public Wi-Fi to encrypt your traffic.",
                "Turn off auto-connect to open Wi-Fi networks on your device.",
                "Hackers can set up fake hotspots like 'Free Airport WiFi' to steal your data.",
                "Stick to HTTPS websites when using public Wi-Fi for extra protection.",
                "Log out of accounts when done, especially on shared or public networks."
            },
                ["two factor"] = new List<string>
            {
                "Two-factor authentication adds a second verification step beyond your password.",
                "Even if a hacker gets your password, 2FA stops them from logging in.",
                "Use an authenticator app like Google Authenticator instead of SMS when possible.",
                "Enable 2FA on your email first — it is the key to all your other accounts.",
                "Never share your 2FA code with anyone, even someone claiming to be support."
            },
                ["backup"] = new List<string>
            {
                "Follow the 3-2-1 rule: 3 copies, 2 different media types, 1 offsite backup.",
                "Back up your data regularly — ransomware can encrypt all your files instantly.",
                "Test your backups occasionally to make sure they actually restore correctly.",
                "Keep at least one backup disconnected from the internet to protect from ransomware.",
                "Automate your backups so you never forget to do them manually."
            },
                ["safe browsing"] = new List<string>
            {
                "Always look for HTTPS in the address bar before entering any information.",
                "Avoid clicking on pop-up ads — they can lead to malicious sites.",
                "Keep your browser and extensions updated to patch security vulnerabilities.",
                "Use a browser extension like uBlock Origin to block malicious ads.",
                "Clear your cookies and cache regularly to reduce tracking.",
                "Be cautious of browser extensions — some can read everything you type."
            },
                ["hack"] = new List<string>
            {
                "If you think you have been hacked, change all your passwords immediately.",
                "Enable two-factor authentication on all accounts after a suspected hack.",
                "Check haveibeenpwned.com to see if your email appeared in a data breach.",
                "Review your accounts for suspicious activity like unknown logins.",
                "Inform your bank immediately if you suspect financial accounts are compromised.",
                "Run a full antivirus scan to check for malware left behind by attackers."
            }
            };

        private readonly Dictionary<string, string> _sentiments =
            new Dictionary<string, string>
            {
                ["worried"] = "It is completely understandable to feel that way. Let me help ease your concerns. ",
                ["scared"] = "Do not worry — knowledge is your best defence. Here is what you need to know: ",
                ["frustrated"] = "I hear you. Cybersecurity can feel overwhelming. Let us break it down simply. ",
                ["curious"] = "Great curiosity — that is the first step to staying safe online. ",
                ["confused"] = "No problem at all — let me explain that more clearly. ",
                ["angry"] = "I understand your frustration. Let us work through this together. ",
                ["nervous"] = "It is okay to feel nervous — being cautious online is actually a good thing. ",
                ["unsure"] = "No worries — that is exactly what I am here for. Let me clarify. "
            };

        private readonly List<string> _followUpPhrases = new List<string>
        {
            "tell me more","explain more","another tip","more info",
            "give me another","what else","keep going","go on",
            "more please","continue","elaborate","anything else",
            "more details","give me more","and then"
        };

        // NLP intent keyword groups
        private readonly List<string> _addTaskKeywords = new List<string>
            { "add task","create task","new task","add a task","set task","log task" };
        private readonly List<string> _reminderKeywords = new List<string>
            { "remind me","set reminder","add reminder","set a reminder","reminder for","remind me to" };
        private readonly List<string> _viewTaskKeywords = new List<string>
            { "show tasks","view tasks","my tasks","list tasks","show my tasks","what tasks" };
        private readonly List<string> _quizKeywords = new List<string>
            { "start quiz","play quiz","begin quiz","quiz me","take quiz","quiz","open quiz" };
        private readonly List<string> _logKeywords = new List<string>
            { "activity log","show log","what have you done","recent actions","show activity","view log" };
        private readonly List<string> _completeKeywords = new List<string>
            { "complete task","mark complete","finish task","done task","task done","mark as complete" };

        // ══════════════════════════════════════════════════════════════════════
        //  CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════════════
        public MainWindow()
        {
            InitializeComponent();
            InitialiseQuestions();

            try
            {
                Database.Initialise();
                LogAction("System: Database connected successfully.");
            }
            catch (Exception ex)
            {
                LogAction($"System: DB connection failed — {ex.Message}");
                MessageBox.Show(
                    $"Could not connect to MySQL.\n\nMake sure XAMPP MySQL is running.\n\nDetail: {ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            ShowWelcomeBanner();
            _speech.Rate = 0;
            _speech.Volume = 100;
            _speech.SpeakAsync("Welcome to CyberBot, your personal cybersecurity assistant. Please enter your name to get started.");
        }

        // ══════════════════════════════════════════════════════════════════════
        //  WELCOME BANNER
        // ══════════════════════════════════════════════════════════════════════
        private void ShowWelcomeBanner()
        {
            string banner = @"
╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║    ██████╗      ██████╗  ██████╗ ████████╗                   ║
║   ██╔════╝     ██╔══██╗██╔═══██╗╚══██╔══╝                    ║
║   ██║          ██████╔╝██║   ██║   ██║                       ║
║   ██║          ██╔══██╗██║   ██║   ██║                       ║
║   ╚██████╗     ██████╔╝╚██████╔╝   ██║                       ║
║    ╚═════╝     ╚═════╝  ╚═════╝    ╚═╝                       ║
║                                                              ║
╠══════════════════════════════════════════════════════════════╣
║            CyberBot - Your Cybersecurity Assistant           ║
╠══════════════════════════════════════════════════════════════╣
║  Features: Chat · Tasks · Quiz · Activity Log               ║
║  Enter your name above and click 'Start Chat' to begin      ║
╚══════════════════════════════════════════════════════════════╝";
            AddBubble(banner, isUser: false);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  START CHAT
        // ══════════════════════════════════════════════════════════════════════
        private void StartChat_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                AddBubble("Please enter your name to begin.", isUser: false);
                return;
            }
            if (_chatStarted)
            {
                AddBubble("A session is already active. Type 'bye' to end it first.", isUser: false);
                return;
            }

            _userName = name;
            _chatStarted = true;
            NameBox.IsEnabled = false;

            _speech.SpeakAsync($"Hello {_userName}, welcome to CyberBot!");
            LogAction($"Session started for user: {_userName}");

            AddBubble(
                $"Hello, {_userName}! Welcome to CyberBot.\n" +
                "I am here to help you stay safe online.\n\n" +
                "💬 Ask me about: Passwords, Phishing, Privacy, Scams, VPN,\n" +
                "   Antivirus, Public WiFi, Two-Factor Auth, Backups,\n" +
                "   Safe Browsing, or Hacking.\n\n" +
                "📋 Task commands: 'add task [title]', 'show tasks', 'remind me to [action]'\n" +
                "🎮 Quiz: type 'start quiz' or use the Quiz tab\n" +
                "📜 Log:  type 'show activity log'\n\n" +
                "Type 'help' to see this again.",
                isUser: false);

            RefreshTaskList();
            UserInput.Focus();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CHAT — SEND / PROCESS MESSAGE
        // ══════════════════════════════════════════════════════════════════════
        private void SendMessage_Click(object sender, RoutedEventArgs e) => ProcessMessage();

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ProcessMessage();
        }

        private void ProcessMessage()
        {
            if (!_chatStarted)
            {
                AddBubble("Please enter your name and click 'Start Chat' first.", isUser: false);
                return;
            }

            string msg = UserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(msg)) return;

            AddBubble(msg, isUser: true);
            string response = GetResponse(msg);
            AddBubble(response, isUser: false);

            string lower = msg.ToLower();
            if (lower.Contains("bye") || lower.Contains("exit") || lower.Contains("quit"))
                EndSession();

            UserInput.Text = "";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CORE RESPONSE LOGIC  (NLP + keyword matching)
        // ══════════════════════════════════════════════════════════════════════
        private string GetResponse(string input)
        {
            string lower = input.ToLower().Trim();
            string prefix = DetectSentiment(lower);

            // ── Multi-turn: waiting for reminder answer ───────────────────────
            if (_awaitingReminder)
            {
                _awaitingReminder = false;
                return HandleReminderReply(lower, _pendingTaskTitle, _pendingTaskDesc);
            }

            // ── Greetings ─────────────────────────────────────────────────────
            if (ContainsWord(lower, "hello") || ContainsWord(lower, "hi") || ContainsWord(lower, "hey"))
                return $"{prefix}Hello {_userName}! How can I help you stay safe online today?";
            if (lower.Contains("how are you") || lower.Contains("how r u"))
                return $"I am running at full security, thanks for asking, {_userName}! How can I help?";
            if (lower.Contains("good morning"))
                return $"Good morning, {_userName}! Stay cyber-safe today. What would you like to know?";
            if (lower.Contains("good evening") || lower.Contains("good night"))
                return $"Good evening, {_userName}! Always lock your accounts before bed. Anything I can help with?";

            // ── NLP: Activity log ─────────────────────────────────────────────
            if (_logKeywords.Any(k => lower.Contains(k)))
                return GetActivityLogSummary();

            // ── NLP: Quiz triggers ────────────────────────────────────────────
            if (_quizKeywords.Any(k => lower.Contains(k)))
            {
                SwitchToQuizTab();
                return "Opening the Quiz tab! Press 'Start Quiz' to begin your cybersecurity challenge. 🎮";
            }

            // ── NLP: View tasks ───────────────────────────────────────────────
            if (_viewTaskKeywords.Any(k => lower.Contains(k)))
            {
                SwitchToTaskTab();
                return GetTaskSummaryForChat();
            }

            // ── NLP: Add task (full NLP — extracts title from message) ────────
            if (_addTaskKeywords.Any(k => lower.Contains(k)))
                return HandleNlpAddTask(input, lower);

            // ── NLP: Reminder shortcut ("remind me to X") ─────────────────────
            if (_reminderKeywords.Any(k => lower.Contains(k)))
                return HandleNlpReminder(input, lower);

            // ── NLP: Mark complete via chat ───────────────────────────────────
            if (_completeKeywords.Any(k => lower.Contains(k)))
                return HandleNlpComplete(lower);

            // ── Follow-up tips ────────────────────────────────────────────────
            foreach (var phrase in _followUpPhrases)
            {
                if (lower.Contains(phrase))
                {
                    if (!string.IsNullOrEmpty(_lastTopic) && _responses.ContainsKey(_lastTopic))
                    {
                        string tip = GetFreshTip(_lastTopic);
                        return $"{prefix}Here is another tip on {_lastTopic}:\n{tip}\n\nSay 'tell me more' for another tip!";
                    }
                    return "What topic would you like more info on? Try asking about passwords, phishing, or scams!";
                }
            }

            // ── Memory: user expresses interest ──────────────────────────────
            if (lower.Contains("i'm interested in") || lower.Contains("i am interested in")
                || lower.Contains("i want to know about"))
            {
                foreach (var key in _responses.Keys)
                {
                    if (lower.Contains(key))
                    {
                        _favouriteTopic = key;
                        _lastTopic = key;
                        _usedIndexes.Clear();
                        LogAction($"NLP: User expressed interest in '{key}'.");
                        return $"Great! I will remember that you are interested in {key}.\n\n{GetFreshTip(key)}\n\nSay 'tell me more' for another {key} tip anytime!";
                    }
                }
            }

            // ── Keyword matching ──────────────────────────────────────────────
            foreach (var key in _responses.Keys)
            {
                if (lower.Contains(key))
                {
                    if (_lastTopic != key) { _usedIndexes.Clear(); _lastTopic = key; }
                    string tip = GetFreshTip(key);
                    LogAction($"NLP: Keyword '{key}' matched. Tip provided.");

                    if (!string.IsNullOrEmpty(_favouriteTopic) && key == _favouriteTopic)
                        return $"{prefix}As someone interested in {_favouriteTopic}, here is a tip:\n{tip}\n\nSay 'tell me more' for another tip!";

                    return $"{prefix}{tip}\n\nSay 'tell me more' for another tip on {key}!";
                }
            }

            // ── Help ──────────────────────────────────────────────────────────
            if (lower.Contains("what can i ask") || ContainsWord(lower, "help")
                || lower.Contains("topics") || lower.Contains("menu"))
                return "You can ask me about:\n" +
                       "- Passwords     - Phishing      - Privacy\n" +
                       "- Scams         - VPN           - Antivirus\n" +
                       "- Public WiFi   - Two-Factor    - Backups\n" +
                       "- Safe Browsing - Hacking\n\n" +
                       "Task commands: 'add task [title]', 'show tasks', 'remind me to [action]'\n" +
                       "Quiz:          'start quiz'\n" +
                       "Log:           'show activity log'";

            if (lower.Contains("thank"))
                return $"You are welcome, {_userName}! Stay safe and stay smart online.";

            if (ContainsWord(lower, "bye") || lower.Contains("exit") || lower.Contains("quit"))
                return $"Goodbye {_userName}! Remember — stay alert, stay secure. See you next time!";

            if (!string.IsNullOrEmpty(_favouriteTopic) && (lower.Contains("remind") || lower.Contains("my interest")))
                return $"You told me earlier that you are interested in {_favouriteTopic}. Would you like another tip on that?";

            // ── 2FA alias ─────────────────────────────────────────────────────
            if (lower.Contains("2fa") || lower.Contains("two-factor") || lower.Contains("two factor"))
            {
                _lastTopic = "two factor";
                return $"{prefix}{GetFreshTip("two factor")}\n\nSay 'tell me more' for another tip on two-factor authentication!";
            }

            LogAction($"NLP: Input not matched — fallback triggered.");
            return $"I am not sure I understand that, {_userName}. Could you try rephrasing?\nType 'help' to see all topics I can assist with!";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  NLP TASK HANDLERS
        // ══════════════════════════════════════════════════════════════════════
        private string HandleNlpAddTask(string original, string lower)
        {
            // Extract title: everything after the matched keyword
            string title = original;
            foreach (var kw in _addTaskKeywords)
            {
                int idx = lower.IndexOf(kw);
                if (idx >= 0)
                {
                    string after = original.Substring(idx + kw.Length).Trim();
                    if (after.Length > 2) { title = after; break; }
                }
            }
            // Remove leading punctuation/conjunctions
            foreach (string filler in new[] { "to ", "- ", ": ", "for " })
                if (title.ToLower().StartsWith(filler)) title = title.Substring(filler.Length).Trim();

            if (title.Length < 3 || title.ToLower() == lower)
                title = "Cybersecurity task";

            _pendingTaskTitle = title;
            _pendingTaskDesc = $"Task related to: {title}";
            _awaitingReminder = true;

            return $"Task noted: \"{title}\"\n\nWould you like a reminder? If yes, say something like 'yes, 3 days' or just 'yes' for a 7-day reminder. Say 'no' to skip.";
        }

        private string HandleNlpReminder(string original, string lower)
        {
            string action = original;
            foreach (var kw in _reminderKeywords)
            {
                int idx = lower.IndexOf(kw);
                if (idx >= 0)
                {
                    string after = original.Substring(idx + kw.Length).Trim();
                    if (after.Length > 2) { action = after; break; }
                }
            }
            foreach (string filler in new[] { "to ", "that ", "about " })
                if (action.ToLower().StartsWith(filler)) action = action.Substring(filler.Length).Trim();

            // Try to extract a day count from the message
            int days = 7;
            var words = lower.Split(' ');
            for (int i = 0; i < words.Length; i++)
                if (int.TryParse(words[i], out int d)) { days = d; break; }

            DateTime reminder = DateTime.Now.AddDays(days);

            try
            {
                int id = Database.AddTask(action, $"Reminder task: {action}", reminder);
                RefreshTaskList();
                LogAction($"Reminder set via NLP: '{action}' in {days} days (ID {id}).");
                return $"Reminder set! I will remind you to \"{action}\" on {reminder:dd MMM yyyy}.\nTask saved to your task list. ✔";
            }
            catch (Exception ex)
            {
                return $"Could not save the reminder to the database: {ex.Message}";
            }
        }

        private string HandleReminderReply(string lower, string title, string desc)
        {
            bool wantsReminder = lower.Contains("yes") || lower.Contains("sure")
                              || lower.Contains("please") || lower.Contains("ok")
                              || lower.Contains("yep") || lower.Contains("yeah");

            int days = 7;
            var words = lower.Split(' ');
            for (int i = 0; i < words.Length; i++)
                if (int.TryParse(words[i], out int d)) { days = d; break; }

            DateTime? reminder = wantsReminder ? (DateTime?)DateTime.Now.AddDays(days) : null;

            try
            {
                int id = Database.AddTask(title, desc, reminder);
                RefreshTaskList();

                if (wantsReminder)
                {
                    LogAction($"Task added with reminder: '{title}' in {days} days (ID {id}).");
                    return $"Task added: \"{title}\"\nReminder set for {reminder:dd MMM yyyy}. ✔\n\nI've saved it to your task list!";
                }
                else
                {
                    LogAction($"Task added without reminder: '{title}' (ID {id}).");
                    return $"Task added: \"{title}\" (no reminder). ✔\n\nI've saved it to your task list!";
                }
            }
            catch (Exception ex)
            {
                return $"Could not save the task: {ex.Message}";
            }
        }

        private string HandleNlpComplete(string lower)
        {
            try
            {
                var tasks = Database.GetAllTasks().Where(t => !t.IsCompleted).ToList();
                if (!tasks.Any())
                    return "You have no pending tasks to mark as complete.";

                // Find best match by title keyword
                TaskItem best = null;
                foreach (var t in tasks)
                    if (lower.Contains(t.Title.ToLower()))
                    { best = t; break; }

                if (best == null)
                    return $"I found {tasks.Count} pending task(s). Please use the Tasks tab to select and mark one as complete, or be more specific (e.g. 'complete task enable 2FA').";

                Database.MarkCompleted(best.Id);
                RefreshTaskList();
                LogAction($"Task marked complete via NLP: '{best.Title}'.");
                return $"✔ Task \"{best.Title}\" marked as completed!";
            }
            catch (Exception ex)
            {
                return $"Could not update the task: {ex.Message}";
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TASK TAB — GUI HANDLERS
        // ══════════════════════════════════════════════════════════════════════
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.", "Required Field",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string desc = TaskDescBox.Text.Trim();
            DateTime? reminder = null;

            if (SetReminderCheck.IsChecked == true)
            {
                if (!int.TryParse(ReminderDaysBox.Text.Trim(), out int days) || days < 1)
                {
                    MessageBox.Show("Please enter a valid number of days for the reminder.", "Invalid Input",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                reminder = DateTime.Now.AddDays(days);
            }

            try
            {
                int id = Database.AddTask(title, desc, reminder);
                RefreshTaskList();
                TaskTitleBox.Text = "";
                TaskDescBox.Text = "";
                SetReminderCheck.IsChecked = false;

                string logMsg = reminder.HasValue
                    ? $"Task added via GUI: '{title}' (Reminder: {reminder:dd MMM yyyy}, ID {id})."
                    : $"Task added via GUI: '{title}' (no reminder, ID {id}).";
                LogAction(logMsg);

                // Reflect in chat
                AddBubble(
                    reminder.HasValue
                        ? $"✔ Task added: \"{title}\"\nReminder set for {reminder:dd MMM yyyy}."
                        : $"✔ Task added: \"{title}\" (no reminder).",
                    isUser: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save task: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaskListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool selected = TaskListBox.SelectedItem is TaskItem t && !t.IsCompleted;
            CompleteTaskBtn.IsEnabled = selected;
            DeleteTaskBtn.IsEnabled = TaskListBox.SelectedItem != null;
        }

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is not TaskItem task) return;
            try
            {
                Database.MarkCompleted(task.Id);
                LogAction($"Task marked complete via GUI: '{task.Title}'.");
                RefreshTaskList();
                AddBubble($"✔ Task \"{task.Title}\" marked as completed!", isUser: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not update task: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is not TaskItem task) return;
            var result = MessageBox.Show(
                $"Delete task \"{task.Title}\"?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                Database.DeleteTask(task.Id);
                LogAction($"Task deleted via GUI: '{task.Title}'.");
                RefreshTaskList();
                CompleteTaskBtn.IsEnabled = false;
                DeleteTaskBtn.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not delete task: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshTaskList()
        {
            try
            {
                var tasks = Database.GetAllTasks();
                TaskListBox.ItemsSource = null;
                TaskListBox.ItemsSource = tasks;
                CheckDueReminders(tasks);
            }
            catch { /* DB not available — silently skip */ }
        }

        private void CheckDueReminders(List<TaskItem> tasks)
        {
            var due = tasks.Where(t => t.ReminderDate.HasValue
                                    && t.ReminderDate.Value <= DateTime.Now
                                    && !t.IsCompleted).ToList();
            if (!due.Any()) return;

            string msg = $"⏰ Reminder: You have {due.Count} task(s) due:\n" +
                         string.Join("\n", due.Select(t => $"  • {t.Title}"));
            AddBubble(msg, isUser: false);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  QUIZ
        // ══════════════════════════════════════════════════════════════════════
        private void InitialiseQuestions()
        {
            _questions = new List<QuizQuestion>
            {
                new QuizQuestion {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options  = new[]{"Reply with your password","Delete the email","Report it as phishing","Ignore it"},
                    CorrectIndex = 2,
                    Explanation  = "Reporting phishing emails helps prevent scams and protects others. Legitimate organisations never ask for your password via email." },

                new QuizQuestion {
                    Question = "True or False: Using the same password on multiple sites is safe if the password is very long.",
                    IsTrueFalse  = true,
                    CorrectIndex = 1, // False
                    Explanation  = "False! If one site is breached, attackers try the same credentials everywhere — called 'credential stuffing'. Always use unique passwords." },

                new QuizQuestion {
                    Question = "Which of the following makes the strongest password?",
                    Options  = new[]{"password123","P@ssw0rd","Tr0ub4dor&3Horse!","MyName2024"},
                    CorrectIndex = 2,
                    Explanation  = "A long, random passphrase mixing words and symbols is the strongest. Length beats complexity." },

                new QuizQuestion {
                    Question = "True or False: A VPN makes you completely anonymous online.",
                    IsTrueFalse  = true,
                    CorrectIndex = 1, // False
                    Explanation  = "False! A VPN hides your IP and encrypts traffic but does not make you fully anonymous. Websites can still track you via cookies and login data." },

                new QuizQuestion {
                    Question = "What does HTTPS in a website URL indicate?",
                    Options  = new[]{"The site is safe to buy from","Data is encrypted between browser and server","The site is government approved","The site has no malware"},
                    CorrectIndex = 1,
                    Explanation  = "HTTPS means your connection is encrypted, but it does NOT guarantee the site itself is trustworthy or free of scams." },

                new QuizQuestion {
                    Question = "Which is the safest way to store your passwords?",
                    Options  = new[]{"Write them in a notebook","Save them in a browser","Use a reputable password manager","Use the same memorable password everywhere"},
                    CorrectIndex = 2,
                    Explanation  = "A reputable password manager generates, stores, and fills strong unique passwords securely — far safer than alternatives." },

                new QuizQuestion {
                    Question = "True or False: Public Wi-Fi is safe as long as you are only browsing, not banking.",
                    IsTrueFalse  = true,
                    CorrectIndex = 1, // False
                    Explanation  = "False! Even casual browsing on public Wi-Fi can expose session cookies and login tokens. Always use a VPN on public networks." },

                new QuizQuestion {
                    Question = "What is 'smishing'?",
                    Options  = new[]{"Hacking via social media","Phishing attacks sent via SMS","A type of malware","Password cracking technique"},
                    CorrectIndex = 1,
                    Explanation  = "Smishing = SMS + phishing. Attackers send fake text messages with malicious links to steal credentials or install malware." },

                new QuizQuestion {
                    Question = "True or False: Antivirus software alone is enough to protect your computer.",
                    IsTrueFalse  = true,
                    CorrectIndex = 1, // False
                    Explanation  = "False! Antivirus is one layer. You also need safe browsing habits, updated software, a firewall, and strong passwords." },

                new QuizQuestion {
                    Question = "What is two-factor authentication (2FA)?",
                    Options  = new[]{"Using two different passwords","A second verification step beyond your password","Logging in from two devices","Encrypting data twice"},
                    CorrectIndex = 1,
                    Explanation  = "2FA requires something you know (password) AND something you have (code/app/device), making accounts far harder to compromise." },

                new QuizQuestion {
                    Question = "What is the 3-2-1 backup rule?",
                    Options  = new[]{"3 passwords, 2 devices, 1 cloud","3 copies, 2 media types, 1 offsite","3 backups daily, 2 weekly, 1 monthly","3 files, 2 folders, 1 drive"},
                    CorrectIndex = 1,
                    Explanation  = "3 copies of data, on 2 different storage types, with 1 stored offsite — ensuring resilience against hardware failure, theft, and ransomware." },

                new QuizQuestion {
                    Question = "True or False: It is safe to click a link in an email if the sender's display name looks familiar.",
                    IsTrueFalse  = true,
                    CorrectIndex = 1, // False
                    Explanation  = "False! Display names are trivially spoofed. Always check the actual email address and hover over links to see the real destination URL." }
            };
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            _quizIndex = 0;
            _quizScore = 0;
            _quizActive = true;
            FeedbackBorder.Visibility = Visibility.Collapsed;
            StartQuizBtn.Content = "↺  Restart";
            LogAction("Quiz started.");
            ShowQuestion();
        }

        private void ShowQuestion()
        {
            if (_quizIndex >= _questions.Count)
            {
                ShowQuizResults();
                return;
            }

            var q = _questions[_quizIndex];
            QuizProgressText.Text = $"Question {_quizIndex + 1} of {_questions.Count}";
            QuizScoreText.Text = $"Score: {_quizScore} / {_quizIndex}";

            QuizPanel.Children.Clear();

            // Question number badge
            var badge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(233, 69, 96)),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(14, 4, 14, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10),
                Child = new TextBlock
                {
                    Text = $"Question {_quizIndex + 1}",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold
                }
            };
            QuizPanel.Children.Add(badge);

            // Question text
            QuizPanel.Children.Add(new TextBlock
            {
                Text = q.Question,
                Foreground = Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // Options
            string[] options = q.IsTrueFalse ? new[] { "True", "False" } : q.Options;
            string[] letters = { "A", "B", "C", "D" };

            for (int i = 0; i < options.Length; i++)
            {
                int captured = i;
                var optBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(14, 10, 14, 10),
                    Margin = new Thickness(0, 0, 0, 8),
                    Cursor = Cursors.Hand
                };

                var optContent = new StackPanel { Orientation = Orientation.Horizontal };
                optContent.Children.Add(new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(233, 69, 96)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6, 2, 6, 2),
                    Margin = new Thickness(0, 0, 10, 0),
                    Child = new TextBlock
                    {
                        Text = letters[i],
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold
                    }
                });
                optContent.Children.Add(new TextBlock
                {
                    Text = options[i],
                    Foreground = Brushes.White,
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });

                optBorder.Child = optContent;
                optBorder.MouseLeftButtonUp += (s, e) => HandleAnswer(captured, q);
                QuizPanel.Children.Add(optBorder);
            }
        }

        private void HandleAnswer(int selectedIndex, QuizQuestion q)
        {
            bool correct = selectedIndex == q.CorrectIndex;
            if (correct) _quizScore++;
            _quizIndex++;

            string[] options = q.IsTrueFalse ? new[] { "True", "False" } : q.Options;
            string correctAns = options[q.CorrectIndex];

            FeedbackBorder.Background = correct
                ? new SolidColorBrush(Color.FromRgb(39, 174, 96))
                : new SolidColorBrush(Color.FromRgb(192, 57, 43));

            FeedbackText.Text = correct
                ? $"✔ Correct! {q.Explanation}"
                : $"✘ Incorrect. The correct answer was: {correctAns}\n{q.Explanation}";

            FeedbackBorder.Visibility = Visibility.Visible;
            LogAction($"Quiz Q{_quizIndex}: {(correct ? "Correct" : "Incorrect")}.");

            // Short delay then load next question
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2.5)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                FeedbackBorder.Visibility = Visibility.Collapsed;
                ShowQuestion();
            };
            timer.Start();
        }

        private void ShowQuizResults()
        {
            _quizActive = false;
            int total = _questions.Count;
            double pct = (double)_quizScore / total * 100;

            string grade = pct >= 90 ? "🏆 Outstanding! You are a cybersecurity pro!"
                         : pct >= 70 ? "👍 Great job! You have solid cybersecurity knowledge."
                         : pct >= 50 ? "📚 Not bad! Keep learning to stay safe online."
                                     : "💡 Keep practising — cybersecurity knowledge saves you from real threats!";

            QuizProgressText.Text = "Quiz Complete!";
            QuizScoreText.Text = $"Score: {_quizScore} / {total}";

            QuizPanel.Children.Clear();
            QuizPanel.Children.Add(new TextBlock
            {
                Text = "Quiz Complete! 🎉",
                Foreground = new SolidColorBrush(Color.FromRgb(233, 69, 96)),
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            });
            QuizPanel.Children.Add(new TextBlock
            {
                Text = $"You scored {_quizScore} out of {total}  ({pct:0}%)",
                Foreground = Brushes.White,
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12)
            });
            QuizPanel.Children.Add(new TextBlock
            {
                Text = grade,
                Foreground = Brushes.White,
                FontSize = 15,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            });

            LogAction($"Quiz completed: {_quizScore}/{total} ({pct:0}%).");
            AddBubble($"Quiz finished! You scored {_quizScore}/{total} ({pct:0}%). {grade}", isUser: false);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ACTIVITY LOG
        // ══════════════════════════════════════════════════════════════════════
        private void LogAction(string message)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}]  {message}";
            _activityLog.Insert(0, entry); // newest first
            RefreshLogPanel();
        }

        private void RefreshLogPanel()
        {
            LogPanel.Children.Clear();
            int display = Math.Min(_logDisplayCount, _activityLog.Count);

            for (int i = 0; i < display; i++)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = new Thickness(0, 0, 0, 5),
                    Child = new TextBlock
                    {
                        Text = $"{i + 1}. {_activityLog[i]}",
                        Foreground = Brushes.White,
                        FontFamily = new FontFamily("Consolas"),
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap
                    }
                };
                LogPanel.Children.Add(border);
            }

            ShowMoreBtn.Visibility = _activityLog.Count > _logDisplayCount
                ? Visibility.Visible : Visibility.Collapsed;

            LogCountText.Text = $"Showing {display} of {_activityLog.Count} actions";
            LogScroller.ScrollToTop();
        }

        private void ShowMoreLog_Click(object sender, RoutedEventArgs e)
        {
            _logDisplayCount += 5;
            RefreshLogPanel();
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Clear all activity log entries?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            _activityLog.Clear();
            _logDisplayCount = 5;
            RefreshLogPanel();
        }

        private string GetActivityLogSummary()
        {
            if (!_activityLog.Any())
                return "No actions have been logged yet this session.";

            int show = Math.Min(5, _activityLog.Count);
            string list = string.Join("\n", _activityLog.Take(show).Select((a, i) => $"{i + 1}. {a}"));
            LogAction("User viewed activity log via chat.");
            return $"Here is a summary of recent actions:\n\n{list}\n\n(Switch to the Activity Log tab to see the full history.)";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TAB SWITCHING HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private void SwitchToTaskTab()
        {
            if (this.FindName("TaskListBox") is ListBox lb)
                (lb.Parent as FrameworkElement)?.FindAncestor<TabItem>()?.Focus();

            var tc = FindTabControl();
            if (tc != null) tc.SelectedIndex = 1;
        }

        private void SwitchToQuizTab()
        {
            var tc = FindTabControl();
            if (tc != null) tc.SelectedIndex = 2;
        }

        private TabControl FindTabControl()
        {
            return FindVisualChild<TabControl>(this);
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match) return match;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CHAT BUBBLE
        // ══════════════════════════════════════════════════════════════════════
        private void AddBubble(string message, bool isUser)
        {
            var color = isUser
                ? new SolidColorBrush(Color.FromRgb(233, 69, 96))
                : new SolidColorBrush(Color.FromRgb(15, 52, 96));
            var alignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            var bubble = new Border
            {
                Background = color,
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(5, 4, 5, 4),
                HorizontalAlignment = alignment,
                Child = new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    FontSize = 13,
                    FontFamily = new FontFamily("Consolas"),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 560,
                    Padding = new Thickness(10, 8, 10, 8)
                }
            };

            ChatPanel.Children.Add(bubble);
            ChatScroller.ScrollToEnd();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TASK LIST SUMMARY FOR CHAT
        // ══════════════════════════════════════════════════════════════════════
        private string GetTaskSummaryForChat()
        {
            try
            {
                var tasks = Database.GetAllTasks();
                if (!tasks.Any())
                    return "You have no tasks yet. Add one using the Tasks tab or type 'add task [title]'.";

                int pending = tasks.Count(t => !t.IsCompleted);
                int completed = tasks.Count(t => t.IsCompleted);
                string summary = $"You have {pending} pending and {completed} completed task(s):\n\n";
                foreach (var t in tasks.Take(5))
                    summary += $"  {(t.IsCompleted ? "✔" : "⏳")} {t.Title} — {t.ReminderDisplay}\n";
                if (tasks.Count > 5)
                    summary += $"\n  ...and {tasks.Count - 5} more. See the Tasks tab for the full list.";
                return summary;
            }
            catch
            {
                return "Could not retrieve tasks — make sure the database is running.";
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPER METHODS  (Part 1 & 2 preserved)
        // ══════════════════════════════════════════════════════════════════════
        private string GetFreshTip(string topic)
        {
            var list = _responses[topic];
            if (_usedIndexes.Count >= list.Count) _usedIndexes.Clear();
            int index;
            do { index = _random.Next(list.Count); }
            while (_usedIndexes.Contains(index));
            _usedIndexes.Add(index);
            return list[index];
        }

        private string DetectSentiment(string input)
        {
            foreach (var s in _sentiments)
                if (input.Contains(s.Key)) return s.Value;
            return "";
        }

        private bool ContainsWord(string input, string word)
        {
            int index = input.IndexOf(word);
            while (index >= 0)
            {
                bool beforeOk = index == 0 || !char.IsLetter(input[index - 1]);
                bool afterOk = index + word.Length == input.Length || !char.IsLetter(input[index + word.Length]);
                if (beforeOk && afterOk) return true;
                index = input.IndexOf(word, index + 1);
            }
            return false;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  END SESSION
        // ══════════════════════════════════════════════════════════════════════
        private void EndSession()
        {
            _chatStarted = false;
            UserInput.IsEnabled = false;
            LogAction($"Session ended for user: {_userName}.");

            if (!string.IsNullOrEmpty(_favouriteTopic))
                AddBubble($"Your favourite topic this session was: {_favouriteTopic}.\nKeep learning about it to stay protected.", isUser: false);

            AddBubble($"Session ended. Stay safe online, {_userName}.", isUser: false);
            _speech.SpeakAsync($"Goodbye {_userName}. Stay safe online. See you next time!");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  EXTENSION  — walk visual tree upward
    // ══════════════════════════════════════════════════════════════════════════
    public static class VisualTreeExtensions
    {
        public static T FindAncestor<T>(this DependencyObject obj) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T match) return match;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}
