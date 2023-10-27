public struct SaveQuest
{
    public int id;
    public float timeStarted;
    public QuestState state;

    public SaveQuest(int id, QuestState state, float timeStarted)
    {
        this.id = id;
        this.state = state;
        this.timeStarted = timeStarted;
    }
}