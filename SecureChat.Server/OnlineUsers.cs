public static class OnlineUsers {
    private static readonly HashSet<int> _onlineUsers = [];
    private static readonly Lock _lock = new();

    public static void Add(int userId) { lock (_lock) _onlineUsers.Add(userId); }
    public static void Remove(int userId) { lock (_lock) _onlineUsers.Remove(userId); }
    public static bool IsOnline(int userId) { lock (_lock) return _onlineUsers.Contains(userId); }
}