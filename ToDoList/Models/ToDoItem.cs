public class ToDoItem
{
    public Guid ID { get; set; }
    public string ItemText { get; set; }
    public bool IsDone { get; set; }
    public DateTime? DoneDate { get; set; }  // Nullable DateTime for completion date
    public TaskPriority Priority { get; set; }


    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }
    // Override ToString to handle display of both pending and completed tasks
    public override string ToString()
    {
        if (IsDone && DoneDate.HasValue)
        {
            // If the task is done, show the "done at" time
            return $"{ItemText} - Priority: {Priority} - Done at: {DoneDate.Value.ToString("g")}";
        }
        else
        {
            // If the task is pending, just show the task name and priority (no "done at")
            return $"{ItemText} - Priority: {Priority}";
        }
    }
}
