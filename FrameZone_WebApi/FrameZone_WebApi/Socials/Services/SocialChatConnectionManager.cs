using System.Collections.Concurrent;

namespace FrameZone_WebApi.Socials.Services
{
    /// <summary>
    /// 紀錄使用者的 SignalR 連線清單，方便後端廣播時排除/鎖定特定使用者。
    /// </summary>
    public class SocialChatConnectionManager
    {
        private readonly ConcurrentDictionary<long, HashSet<string>> _userConnections = new();

        public void Add(long userId, string connectionId)
        {
            var connections = _userConnections.GetOrAdd(userId, _ => new HashSet<string>());
            lock (connections)
            {
                connections.Add(connectionId);
            }
        }

        public void Remove(long userId, string connectionId)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                    }
                }
            }
        }

        public IReadOnlyList<string> GetConnections(long userId)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    return connections.ToList();
                }
            }
            return Array.Empty<string>();
        }
    }
}
