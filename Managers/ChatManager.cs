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
        public bool IsInitialized => throw new NotImplementedException();


        private void OnFactionMessageReceived()
        {
        }


        public void Run()
        {
            if (!IsInitialized)
            {
                MySession session = (MyAPIGateway.Session as MySession);

                //session.SessionSimSpeedServer;
                //session.ChatSystem.FactionMessageReceived
            }

            throw new NotImplementedException();
        }
    }
}
