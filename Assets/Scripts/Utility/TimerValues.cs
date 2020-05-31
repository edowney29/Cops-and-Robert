public static class TimerValues
{
    public static float ExportTime = 60f;
    public static float CooldownTime(ActionType action)
    {
        if (action == ActionType.UseJail4) return 30f;
        if (action == ActionType.UseJail5) return 60f;
        if (action == ActionType.UseJail6) return 90f;
        if (action == ActionType.CreateEvidence) return 180f;
        if (action == ActionType.DestroyEvidence) return 210f;
        return 0f;
    }
}
