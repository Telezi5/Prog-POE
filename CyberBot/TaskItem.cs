using System;

namespace CyberBot
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ReminderDisplay =>
            ReminderDate.HasValue ? ReminderDate.Value.ToString("dd MMM yyyy HH:mm") : "No reminder";

        public string StatusDisplay => IsCompleted ? "✔ Completed" : "⏳ Pending";

        public override string ToString() => Title;
    }
}
