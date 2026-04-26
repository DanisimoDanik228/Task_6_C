using Domain;
using Microsoft.AspNetCore.Connections.Features;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml.Linq;

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
        public class UserInfo
        {
            public string connectionId { get; set; }
            public string name { get; set; }
            public string status { get; set; }
        }

        private static string getRandName()
        {
            return "TempName_" + Guid.NewGuid().ToString();
        }
        // default => senior

        // senior - view + edit existing page + add/remove pages + edit status other
        // widdle - view + edit existing page
        // junior - view
        private const string _defaultStatus = "Senior";

        private readonly ConcurrentDictionary<string, UserInfo> _usersStatus = new();
        private readonly HashSet<string> _existingNames = new([""]);

        public string GetStatus(string connectionId)
        {
            return _usersStatus[connectionId].status;
        }

        public bool SetName(string connectionId, string name)
        {
            lock (_usersStatus)
            {
                if (!_existingNames.Contains(name))
                {
                    _usersStatus[connectionId].name = name;
                    _existingNames.Add(name);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool AddUser(string connectionId)
        {
            RemoveUser(connectionId);

            return _usersStatus.TryAdd(connectionId, new UserInfo() { connectionId= connectionId, status = _defaultStatus, name = getRandName() });
        }

        public void RemoveUser(string connectionId)
        {
            lock (_usersStatus)
            {
                if (_usersStatus.ContainsKey(connectionId))
                { 
                    if (_existingNames.Contains(_usersStatus[connectionId].name))
                    { 
                        _existingNames.Remove(_usersStatus[connectionId].name);
                    }
                }
                
                _usersStatus.Remove(connectionId, out _);
            }
        }

        public bool MayView(string authorConnectionId)
        {
            return (_usersStatus[authorConnectionId].status == "Junior") ||
                   (_usersStatus[authorConnectionId].status == "Middle") ||
                   (_usersStatus[authorConnectionId].status == "Senior");
        }

        public bool MayEditPage(string authorConnectionId)
        {
            return (_usersStatus[authorConnectionId].status == "Middle") || 
                   (_usersStatus[authorConnectionId].status == "Senior");
        }

        public bool MayEditGroup(string authorConnectionId)
        {
            return (_usersStatus[authorConnectionId].status == "Senior");
        }
        public bool MayEditUser(string authorConnectionId)
        {
            return (_usersStatus[authorConnectionId].status == "Senior");
        }

        public void SetStatus(string[] connectionid, string status)
        {
            foreach (var item in connectionid)
            {
                _usersStatus[item].status = status;
            }
        }

        public UserInfo[] GetAllUsers()
        {
            return _usersStatus.Values.ToArray();
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
