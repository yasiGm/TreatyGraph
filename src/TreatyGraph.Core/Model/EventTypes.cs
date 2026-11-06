namespace TreatyGraph.Core.Model;

public static class EventTypes
{
    public const string Alliance = "alliance";
    public const string War = "war";
    public const string Coalition = "coalition"; // fought on the same side
    public const string Diplomacy = "diplomacy";
    public const string Support = "support";     // one country helped the other's government in a war
    public const string Proxy = "proxy";         // one country helped someone fighting the other's government
}
