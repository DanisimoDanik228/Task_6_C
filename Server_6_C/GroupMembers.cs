namespace Server_6_C
{
    using System.Collections.Concurrent;
    using System.Text;

    public class GroupMembers
    {
        private static readonly Dictionary<string, List<string>> _groupMembers = new();

        public bool AddUser(string connectionId, string groupId)
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
                return _groupMembers.TryAdd(groupId,new());
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
