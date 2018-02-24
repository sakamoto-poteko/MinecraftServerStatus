using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MCServerStatus.Models;

namespace MCServerStatus
{
    public interface IMinecraftPinger
    {
        Task<Status> PingAsync();
        Task<Status> RequestAsync();
    }
}
