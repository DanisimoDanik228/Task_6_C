using Domain;
using System.Collections.Concurrent;
using System.Text;

namespace Server_6_C
{
    public enum UserStatus
    {
        Senior = 0,
        Middle,
        Junior
    }

    public class PermintationManager
    {
        private static UserStatus parseStatus(string status)
        {
            if (status == "Senior")
            {
                return UserStatus.Senior;
            }
            else
            {
                if (status == "Middle")
                {
                    return UserStatus.Middle;
                }
                else
                {
                    return UserStatus.Junior;
                }
            }
        }
        private static string parseStatus(UserStatus status)
        {
            if (status == UserStatus.Senior)
            {
                return "Senior";
            }
            else
            {
                if (status == UserStatus.Middle)
                {
                    return "Middle";
                }
                else
                {
                    return "Junior";
                }
            }
        }

        // default => senior

        // senior - view + edit existing page + add/remove pages + edit status other
        // widdle - view + edit existing page
        // junior - view
        private const UserStatus _defaultStatus = UserStatus.Senior;

        private readonly ConcurrentDictionary<string, UserStatus> _usersStatus = new();

        public UserStatus GetStatus(string connectionId)
        {
            return _usersStatus[connectionId];
        }

        public bool AddUser(string connectionId)
        {
            RemoveUser(connectionId);

            return _usersStatus.TryAdd(connectionId, _defaultStatus);
        }

        public void RemoveUser(string connectionId)
        {
            lock (_usersStatus)
            {
                _usersStatus.Remove(connectionId, out _);
            }
        }

        public bool MayView(string authorConnectionId)
        {
            return (_usersStatus[authorConnectionId] == UserStatus.Junior) ||
                   (_usersStatus[authorConnectionId] == UserStatus.Middle) ||
                   (_usersStatus[authorConnectionId] == UserStatus.Senior);
        }

        public bool MayEditPage(string authorConnectionId)
        {
            return (_usersStatus[authorConnectionId] == UserStatus.Middle) || 
                   (_usersStatus[authorConnectionId] == UserStatus.Senior);
        }

        public bool MayEditGroup(string authorConnectionId)
        {
            return (_usersStatus[authorConnectionId] == UserStatus.Senior);
        }
        public bool MayEditUser(string authorConnectionId)
        {
            return (_usersStatus[authorConnectionId] == UserStatus.Senior);
        }

        public void SetStatus(string[] connectionid, string status)
        {
            foreach (var item in connectionid)
            {
                _usersStatus[item] = parseStatus(status);
            }
        }

        public UserData[] GetAllUsers()
        {
            return _usersStatus.Select(s => new UserData()
            {
                User = s.Key,
                Status = parseStatus(s.Value)
            }).ToArray();
        }

        public int GetCountUsers()
        {
            return _usersStatus.Count;
        }

        public override string ToString()
        {
            var str = new StringBuilder();

            foreach (var item in _usersStatus)
            {
                str.AppendLine((item.Key + " : " + item.Value));
            }

            return str.ToString();
        }
    }
}
