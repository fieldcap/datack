using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datack.Web.Web.Controllers
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

        [HttpPost]
        [Route("Add")]
        public async Task<ActionResult<Guid>> Add([FromBody] Server server, CancellationToken cancellationToken)
        {
            var result = await _servers.Add(server, cancellationToken);

            return Ok(result);
        }
        
        [HttpPut]
        [Route("Update")]
        public async Task<ActionResult> Update([FromBody] Server server, CancellationToken cancellationToken)
        {
            await _servers.Update(server, cancellationToken);

            return Ok();
        }
        
        [HttpPost]
        [Route("TestSqlServerConnection")]
        public async Task<ActionResult<String>> TestSqlServerConnection([FromBody] Server server, CancellationToken cancellationToken)
        {
            var result = await _servers.TestSqlServerConnection(server, cancellationToken);

            return Ok(result);
        }
    }
}
