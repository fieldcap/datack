using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Internal;
using Datack.Common.Models.RPC;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.SignalR;

namespace Datack.Web.Service.Hubs
{
    public class DatackHub : Hub
    {
        private readonly Servers _servers;
        private readonly JobRunner _jobRunner;

        public DatackHub(Servers servers, JobRunner jobRunner)
        {
            _servers = servers;
            _jobRunner = jobRunner;
        }

        public static readonly ConcurrentDictionary<String, String> Users = new();
        public static readonly ConcurrentDictionary<Guid, RpcResult> Transactions = new();

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var (key, value) in Users)
            {
                if (value == Context.ConnectionId)
                {
                    Users.TryRemove(key, out _);
                    break;
                }
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Connect(String key)
        {
            var server = await _servers.GetByKey(key, CancellationToken.None);

            if (server == null)
            {
                throw new Exception($"Server with key {key} was not found");
            }

            Users.TryAdd(key, Context.ConnectionId);
        }

        public void Response(RpcResult rpcResult)
        {
            Transactions.TryAdd(rpcResult.TransactionId, rpcResult);
        }

        public async Task<RpcUpdate> RpcUpdate(String key)
        {
            var server = await _servers.GetByKey(key, CancellationToken.None);

            return new RpcUpdate
            {
                Server = server
            };
        }

        public async Task TaskProgress(RpcProgressEvent progressEvent)
        {
            await _jobRunner.ProgressTask(progressEvent.JobRunTaskId, progressEvent.Message, progressEvent.IsError, CancellationToken.None);
        }

        public async Task TaskComplete(RpcCompleteEvent completeEvent)
        {
            await _jobRunner.ProgressTask(completeEvent.JobRunTaskId, completeEvent.Message, completeEvent.IsError, CancellationToken.None);
            await _jobRunner.CompleteTask(completeEvent.JobRunTaskId, completeEvent.Message, completeEvent.ResultArtifact, completeEvent.IsError, CancellationToken.None);
        }
    }
}
