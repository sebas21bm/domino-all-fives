using System;
using System.Collections.Generic;
using System.Linq;
using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates lobby and room management operations.
    /// </summary>
    internal sealed class LobbyController
    {
        internal sealed class Change
        {
            internal Lobby Lobby { get; set; }

            internal List<string> Recipients { get; set; }

            internal bool Closed { get; set; }

            internal int MatchId { get; set; }
        }

        private sealed class Room
        {
            internal string Code { get; set; }

            internal int MaximumPlayers { get; set; }

            internal string HostConnectionId { get; set; }

            internal bool Started { get; set; }

            internal readonly Dictionary<string, string> Members =
                new Dictionary<string, string>();
        }

        private readonly Dictionary<string, Room> _rooms =
            new Dictionary<string, Room>();

        private readonly Dictionary<string, string> _membership =
            new Dictionary<string, string>();

        private int _nextProvisionalMatchId;

        internal Change Create(string connectionId, LobbySettings settings)
        {
            if (settings == null || settings.MaximumPlayers < 2 ||
                settings.MaximumPlayers > 4 ||
                string.IsNullOrWhiteSpace(settings.HostDisplayName))
            {
                throw new InvalidOperationException("Invalid lobby settings.");
            }

            EnsureNotMember(connectionId);
            string code;

            do
            {
                code = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
            }
            while (_rooms.ContainsKey(code));

            Room room = new Room
            {
                Code = code,
                MaximumPlayers = settings.MaximumPlayers,
                HostConnectionId = connectionId
            };

            room.Members.Add(connectionId, settings.HostDisplayName.Trim());
            _rooms.Add(code, room);
            _membership.Add(connectionId, code);

            return Changed(room);
        }

        internal Change Join(string connectionId, string code)
        {
            EnsureNotMember(connectionId);
            Room room = FindRoom(code);

            if (room.Started || room.Members.Count >= room.MaximumPlayers)
            {
                throw new InvalidOperationException("Lobby is unavailable or full.");
            }

            room.Members.Add(connectionId, "Guest " + (room.Members.Count + 1));
            _membership.Add(connectionId, room.Code);

            return Changed(room);
        }

        internal Lobby Get(string code)
        {
            return Snapshot(FindRoom(code));
        }

        internal Change Leave(string connectionId)
        {
            if (!_membership.TryGetValue(connectionId, out string code))
            {
                return null;
            }

            Room room = _rooms[code];

            if (room.HostConnectionId == connectionId)
            {
                Change closed = new Change
                {
                    Closed = true,
                    Recipients = room.Members.Keys.Where(id => id != connectionId).ToList()
                };

                RemoveRoom(room);
                return closed;
            }

            room.Members.Remove(connectionId);
            _membership.Remove(connectionId);

            return Changed(room);
        }

        internal Change Close(string connectionId)
        {
            Room room = FindMembership(connectionId);
            EnsureHost(room, connectionId);

            Change closed = new Change
            {
                Closed = true,
                Recipients = room.Members.Keys.Where(id => id != connectionId).ToList()
            };

            RemoveRoom(room);
            return closed;
        }

        internal Change Start(string connectionId)
        {
            Room room = FindMembership(connectionId);
            EnsureHost(room, connectionId);

            if (room.Started || room.Members.Count < 2)
            {
                throw new InvalidOperationException("Lobby cannot start.");
            }

            room.Started = true;
            Change change = Changed(room);
            change.MatchId = ++_nextProvisionalMatchId;

            return change;
        }

        private Room FindRoom(string code)
        {
            if (string.IsNullOrWhiteSpace(code) ||
                !_rooms.TryGetValue(code.Trim(), out Room room))
            {
                throw new InvalidOperationException("Lobby not found.");
            }

            return room;
        }

        private Room FindMembership(string connectionId)
        {
            if (!_membership.TryGetValue(connectionId, out string code))
            {
                throw new InvalidOperationException("Connection is not in a lobby.");
            }

            return _rooms[code];
        }

        private void EnsureNotMember(string connectionId)
        {
            if (_membership.ContainsKey(connectionId))
            {
                throw new InvalidOperationException("Connection is already in a lobby.");
            }
        }

        private static void EnsureHost(Room room, string connectionId)
        {
            if (room.HostConnectionId != connectionId)
            {
                throw new InvalidOperationException("Only the host may perform this operation.");
            }
        }

        private void RemoveRoom(Room room)
        {
            foreach (string id in room.Members.Keys)
            {
                _membership.Remove(id);
            }

            _rooms.Remove(room.Code);
        }

        private static Change Changed(Room room)
        {
            return new Change
            {
                Lobby = Snapshot(room),
                Recipients = room.Members.Keys.ToList()
            };
        }

        private static Lobby Snapshot(Room room)
        {
            return new Lobby
            {
                LobbyCode = room.Code,
                MaximumPlayers = room.MaximumPlayers,
                HasStarted = room.Started,
                Participants = room.Members.Select(member => new LobbyParticipant
                {
                    ConnectionId = member.Key,
                    DisplayName = member.Value,
                    IsHost = member.Key == room.HostConnectionId
                }).ToList()
            };
        }
    }
}