using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoodsmanTaskManagingConnectorObjects.Components.Notifications;
using LoodsmanTaskManagingConnectorObjects.Components.Polling;
using LoodsmanTaskManagingConnectorObjects.Components.Processing;
using Serilog;

namespace LoodsmanTaskManagingConnectorObjects
{
    public abstract class LoodsmanTaskManagingConnector : ILoodsmanTaskManagingConnector
    {
        public ILogger Logger { get; set; }

        public ITaskManagingRequestPoller RequestPoller { get; set; }

        public ICollection<ITaskManagingRequestProcessor> RequestsProcessors { get; set; }

        public ITaskManagingReplyNotificator ReplyNotificator { get; set; }

        public void Run()
        {
            StartNecessaryServices();

            InternalRun();
        }

        protected virtual void StartNecessaryServices()
        {
            StartNecessaryServicesAsync().Wait();
        }

        protected virtual void InternalRun()
        {
            InternalRunAsync().Wait();
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            await StartNecessaryServicesAsync(cancellationToken);

            await InternalRunAsync(cancellationToken);
        }

        private Task StartNecessaryServicesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected abstract Task InternalRunAsync(CancellationToken cancellationToken = default);
    }
}