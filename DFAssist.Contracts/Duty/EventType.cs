namespace DFAssist.Contracts.Duty
{
    public enum EventType
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Global
        NONE,
        INSTANCE_ENTER, // [0] = instance code
        INSTANCE_EXIT,  // [0] = instance code
        MATCH_BEGIN,    // [0] = match type(0,1), [1] = roulette code or instance count, [...] = instance
        MATCH_PROGRESS, // [0] = instance code, [1] = status, [2] = tank, [3] = healer, [4] = dps
        MATCH_ALERT,    // [0] = roulette code, [1] = instance code
        MATCH_END,      // [0] = end reason <MatchEndType>
        // ReSharper restore UnusedMember.Global
        // ReSharper restore InconsistentNaming
    }
}