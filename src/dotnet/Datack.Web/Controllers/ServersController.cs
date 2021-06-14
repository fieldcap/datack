using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Data.Models.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Datack.Service.Services;

namespace Datack.Web.Controllers
{
    [Authorize]
    [Route("Api/Servers")]
    public class ServersController : Controller
    {
        private readonly Servers _servers;

        public ServersController(Servers servers)
        {
            _servers = servers;
        }

        [HttpGet]
        [Route("List")]
        public async Task<ActionResult> List(CancellationToken cancellationToken)
        {
            var result = await _servers.GetAll(cancellationToken);
            return Ok(result);
        }
        
        [HttpGet]
        [Route("GetById/{serverId:guid}")]
        public async Task<ActionResult> GetById(Guid serverId, CancellationToken cancellationToken)
        {
            var server = await _servers.GetById(serverId, cancellationToken);

            if (server == null)
            {
                return NotFound();
            }

            return Ok(server);
        }

        [HttpPut]
        [Route("UpdateDbSettings/{serverId:guid}")]
        public async Task<ActionResult> UpdateDbSettings(Guid serverId, [FromBody] ServerDbSettings serverDbSettings, CancellationToken cancellationToken)
        {
            await _servers.UpdateDbSettings(serverId, serverDbSettings, cancellationToken);
            
            return Ok();
        }
    }
}
