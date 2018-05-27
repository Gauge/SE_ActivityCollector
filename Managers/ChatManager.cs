using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityCollectorPlugin.Managers
{
    class ChatManager : IManager
    {
        public bool IsInitialized { get; set; } = false;

        private MySession session;

        private void OnFactionMessageReceived(long faction_id)
        {
            return;
        }

        private void OnGlobalMessageRecived()
        {
            return;
        }

        private void OnPlayerMessageRecived(long player)
        {
            return;
        }


        public void Run()
        {
            if (!IsInitialized)
            {
                session = (MyAPIGateway.Session as MySession);
                session.ChatSystem.GlobalMessageReceived += OnGlobalMessageRecived;
                session.ChatSystem.FactionMessageReceived += OnFactionMessageReceived;
                session.ChatSystem.PlayerMessageReceived += OnPlayerMessageRecived;

                IsInitialized = true;
            }
        }
    }
}
