using System.Collections.Generic;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IGameService
    {
        public IList<GameInfo> GetGameList();
    }
}
