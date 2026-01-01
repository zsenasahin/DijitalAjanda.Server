namespace DijitalAjanda.Server.Models.Chat
{
    public class IntentCommand
    {
        public string Intent { get; set; }       // navigate, create_goal, ...
        public string Route { get; set; }        // navigate
        public string Title { get; set; }        // create_goal, create_habit, create_note
        public string Description { get; set; }  // create_goal, create_note
        public string Frequency { get; set; }    // create_habit
        public string Period { get; set; }       // summarize_notes, analyze_habits, analyze_week
    }
}


