namespace Server_6_C
{
    using System.Collections.Concurrent;

    public class GroupMembers
    {
        private static readonly Dictionary<string, List<string>> _groupMembers = new();

        public void AddUser(string connectionId, string groupId)
        {
            lock (_groupMembers)
            { 
                RemoveUser(connectionId);

                if (!_groupMembers.ContainsKey(groupId))
                {
                    AddGroup(groupId);
                }

                _groupMembers[groupId].Add(connectionId);
            }
        }

        public void AddGroup(string groupId)
        {
            lock (_groupMembers)
            { 
                _groupMembers.Add(groupId,new());
            }
        }

        public void RemoveUser(string connectionId)
        {
            lock (_groupMembers)
            { 
                foreach (var item in _groupMembers)
                {
                    if (item.Value.Contains(connectionId))
                    {
                        item.Value.Remove(connectionId);
                    }
                }
            }
        }

        public IEnumerable<string> GetAllGroups()
        {
            lock (_groupMembers)
            { 
                return _groupMembers.Keys;
            }
        }
    }
}
