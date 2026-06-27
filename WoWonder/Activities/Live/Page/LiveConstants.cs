using System;

namespace WoWonder.Activities.Live.Page
{
    public class LiveConstants
    {
        public static readonly string PrefName = "Demo_Live";
        public static readonly int DefaultProfileIdx = 4;

        // LiveKit Client Roles
        public static readonly string KeyClientRole = "key_client_role";
        public static readonly int ClientRoleBroadcaster = 1;  // Host / Publisher
        public static readonly int ClientRoleAudience = 2;     // Viewer / Subscriber
    }
}
