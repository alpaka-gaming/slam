using System.Collections.Generic;
using Core.Entities;
using Core.Interfaces;

namespace Core.Services
{
    public class GameService : IGameService
    {
        public IList<GameInfo> GetGameList()
        {
            var result = new List<GameInfo>();

            if (Steamworks.SteamClient.IsLoggedOn)
            {
                
            }

            return result;
        }
    }
}
