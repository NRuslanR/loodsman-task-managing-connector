using DataProvider;
using LoodsmanTaskManagingConnectorConsole.InputSources;
using LoodsmanTaskManagingConnectorObjects;
using MessagingServicing;
using SimpleRabbitMQMessagingServicing;
using UMP.Loodsman.Adapters;

namespace LoodsmanTaskManagingConnectorConsole
{
    internal class LoodsmanTaskManagingConnectorConsoleApp : ILoodsmanTaskManagingConnectorApp
    {
        private readonly ILoodsmanTaskManagingConnectorBuilder connectorBuilder;
        private readonly ILoodsmanTaskManagingConnectorInputSource connectorInputSource;

        public LoodsmanTaskManagingConnectorConsoleApp(ILoodsmanTaskManagingConnectorInputSource connectorInputSource,
            ILoodsmanTaskManagingConnectorBuilder connectorBuilder)
        {
            this.connectorInputSource = connectorInputSource;
            this.connectorBuilder = connectorBuilder;
        }

        public void Run()
        {
            var connectorInput = connectorInputSource.GetLoodsmanTaskManagingConnectorInput();

            var connector = BuildConnector(connectorInput);

            connector.Run();
        }

        private ILoodsmanTaskManagingConnector BuildConnector(LoodsmanTaskManagingConnectorInput connectorInput)
        {
            var requestPollerMessagingService =
                CreateRabbitMQMessagingService(
                    connectorInput.RabbitMQ.ConnectionInfo,
                    connectorInput.RabbitMQ.ExchangeInfo,
                    connectorInput.RabbitMQ.TaskManagingRequestQueueInfo
                );

            var replyNotificatorMessagingService =
                CreateRabbitMQMessagingService(
                    connectorInput.RabbitMQ.ConnectionInfo,
                    connectorInput.RabbitMQ.ExchangeInfo,
                    connectorInput.RabbitMQ.TaskManagingReplyQueueInfo
                );

            var loodsmanApi = GetLoodsmanApi(connectorInput.Loodsman);

            return connectorBuilder
                .WithApiAdapter(new ApiAdapter(loodsmanApi))
                .WithWbsSystemAdapter(new WbsSystemAdapter(loodsmanApi))
                .WithMessagingTaskManagingRequestPoller(requestPollerMessagingService)
                .WithMessagingTaskManagingReplyNotificator(replyNotificatorMessagingService)
                .WithConstantLoodsmanObjectsNames()
                .WithStandardLoodsmanDocumentService()
                .WithStandardLoodsmanTaskService()
                .AddDocumentBasedTasksRemovingRequestProcessor()
                .AddNewDocumentBasedTasksCreationRequestProcessor()
                .AddConsoleLogger()
                .AddFileLogger()
                .Build();
        }

        private IMessagingService CreateRabbitMQMessagingService(
            RabbitMQConnectionInput connectionInfo, 
            RabbitMQExchangeInput exchangeInfo, 
            RabbitMQQueueInput queueInfo
            )
        {
            var connectionOptions = new SimpleRabbitMQConnectionOptions
            {
                HostName = connectionInfo.Host,
                Port = connectionInfo.Port,
                UserName = connectionInfo.Username,
                Password = connectionInfo.Password,
            };

            var exchangeOptions = SimpleRabbitMQExchangeOptions.CreateDefault();

            exchangeOptions.Name = exchangeInfo.ExchangeName;

            var queueOptions = SimpleRabbitMQQueueOptions.CreateDefault();

            queueOptions.Name = queueInfo.QueueName;
            queueOptions.RoutingKey = queueOptions.RoutingKey;

            return new SimpleRabbitMQMessagingService(connectionOptions, queueOptions, exchangeOptions);
        }

        private ISimpleAPI2 GetLoodsmanApi(LoodsmanInput loodsmanInput)
        {
            var loodsmanConnection = new LoodsmanConnection();

            loodsmanConnection.API8.UniConnect(loodsmanInput.Database, loodsmanInput.Credentials);

            return loodsmanConnection.API8.GetSimpleAPI() as ISimpleAPI2;
        }
    }
}