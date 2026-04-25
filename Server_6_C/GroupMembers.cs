namespace Server_6_C
{
    using System.Collections.Concurrent;
    using System.Text;

    public class GroupMembers
    {
        private readonly Dictionary<string, List<string>> _groupMembers = new();
        public bool AddUserToGroup(string connectionId, string groupId)
        {
            lock (_groupMembers)
            { 
                RemoveUser(connectionId);

                if (!_groupMembers.ContainsKey(groupId))
                {
                    return false;
                }

                _groupMembers[groupId].Add(connectionId);
            }

            return true;
        }

        public bool AddGroup(string groupId)
        {
            lock (_groupMembers)
            {
                if (_groupMembers.ContainsKey(groupId))
                {
                    return false;
                }
                else
                {
                    return _groupMembers.TryAdd(groupId, new());
                }
            }
        }

        public bool RemoveGroup(string groupId)
        {
            lock (_groupMembers)
            {
                if (_groupMembers.ContainsKey(groupId))
                {
                    _groupMembers.Remove(groupId);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool RemoveUser(string connectionId)
        {
            lock (_groupMembers)
            {
                foreach (var item in _groupMembers)
                {
                    if (item.Value.Contains(connectionId))
                    {
                        item.Value.Remove(connectionId);
                        return true;
                    }
                }
            }

            return false;
        }

        public IEnumerable<string> GetAllGroups()
        {
            lock (_groupMembers)
            { 
                return _groupMembers.Keys;
            }
        }

        public override string ToString() 
        {
            var str = new StringBuilder();

            foreach (var item in _groupMembers)
            {
                str.Append(item.Key + ": ");

                foreach (var item1 in item.Value)
                {
                    str.Append(item1 + " ");
                }

                str.AppendLine();

            }

            return str.ToString();
        }
    }
}
