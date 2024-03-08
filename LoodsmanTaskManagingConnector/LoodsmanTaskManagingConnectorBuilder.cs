using System;
using System.Collections.Generic;
using System.Text;
using DataProvider;
using LoodsmanTaskManagingConnectorObjects.Components.Notifications;
using LoodsmanTaskManagingConnectorObjects.Components.Polling;
using LoodsmanTaskManagingConnectorObjects.Components.Processing;
using LoodsmanTaskManagingConnectorObjects.Services;
using MessagingServicing;
using Serilog;
using UMP.Loodsman.Adapters;
using UMP.Loodsman.Dtos.Names.Attributes;
using UMP.Loodsman.Dtos.Names.Attributes.Documents;

namespace LoodsmanTaskManagingConnectorObjects
{
    public interface ILoodsmanTaskManagingConnectorBuilder
    {
        ILoodsmanTaskManagingConnectorBuilder WithStandardApiAdapter(string loodsmanDatabase,
            string loodsmanCredentials);

        ILoodsmanTaskManagingConnectorBuilder WithApiAdapter(IApiAdapter apiAdapter);

        ILoodsmanTaskManagingConnectorBuilder WithWbsSystemAdapter(IWbsSystemAdapter wbsSystemAdapter);

        ILoodsmanTaskManagingConnectorBuilder
            WithMessagingTaskManagingRequestPoller(IMessagingService messagingService);

        ILoodsmanTaskManagingConnectorBuilder AddNewDocumentBasedTasksCreationRequestProcessor();

        ILoodsmanTaskManagingConnectorBuilder AddDocumentBasedTasksRemovingRequestProcessor();

        ILoodsmanTaskManagingConnectorBuilder AddConsoleLogger();

        ILoodsmanTaskManagingConnectorBuilder AddFileLogger();

        ILoodsmanTaskManagingConnectorBuilder WithConstantLoodsmanObjectsNames();

        ILoodsmanTaskManagingConnectorBuilder WithMessagingTaskManagingReplyNotificator(
            IMessagingService messagingService);

        ILoodsmanTaskManagingConnectorBuilder WithStandardLoodsmanDocumentService();

        ILoodsmanTaskManagingConnectorBuilder WithStandardLoodsmanTaskService();

        ILoodsmanTaskManagingConnector Build();
    }

    public abstract class LoodsmanTaskManagingConnectorBuilder : ILoodsmanTaskManagingConnectorBuilder
    {
        protected IApiAdapter apiAdapter;
        protected IBaseDocumentAttributeNames baseDocumentAttributeNames;
        protected LoggerConfiguration baseLoggerConfiguration;
        protected ILogger logger;
        protected ILoodsmanDocumentService loodsmanDocumentService;
        protected ILoodsmanTaskService loodsmanTaskService;
        protected ITaskManagingReplyNotificator taskManagingReplyNotificator;
        protected ITaskManagingRequestPoller taskManagingRequestPoller;
        protected IDictionary<Type, ITaskManagingRequestProcessor> taskManagingRequestProcessors;
        protected IWbsSystemAdapter wbsSystemAdapter;

        protected LoodsmanTaskManagingConnectorBuilder()
        {
            taskManagingRequestProcessors = new Dictionary<Type, ITaskManagingRequestProcessor>();
            baseLoggerConfiguration = new LoggerConfiguration().MinimumLevel.Information();
        }

        public ILoodsmanTaskManagingConnectorBuilder WithStandardApiAdapter(string loodsmanDatabase,
            string loodsmanCredentials)
        {
            var loodsmanConnection = new LoodsmanConnection();

            loodsmanConnection.API8.UniConnect(loodsmanDatabase, loodsmanCredentials);

            var simpleApi = loodsmanConnection.API8.GetSimpleAPI() as ISimpleAPI2;

            return WithApiAdapter(new ApiAdapter(simpleApi));
        }

        public ILoodsmanTaskManagingConnectorBuilder WithApiAdapter(IApiAdapter apiAdapter)
        {
            this.apiAdapter = apiAdapter;

            return this;
        }

        public ILoodsmanTaskManagingConnectorBuilder WithWbsSystemAdapter(IWbsSystemAdapter wbsSystemAdapter)
        {
            this.wbsSystemAdapter = wbsSystemAdapter;

            return this;
        }

        public ILoodsmanTaskManagingConnectorBuilder WithMessagingTaskManagingRequestPoller(
            IMessagingService messagingService)
        {
            taskManagingRequestPoller = new MessagingTaskManagingRequestPoller(messagingService);

            return this;
        }

        public ILoodsmanTaskManagingConnectorBuilder AddNewDocumentBasedTasksCreationRequestProcessor()
        {
            ThrowIfAnyObjectIsNull(
                ObjectTypePair(loodsmanDocumentService, typeof(ILoodsmanTaskService)),
                ObjectTypePair(loodsmanTaskService, typeof(ILoodsmanTaskService)));

            taskManagingRequestProcessors[typeof(NewDocumentBasedTasksCreationRequestProcessor)] =
                new NewDocumentBasedTasksCreationRequestProcessor(loodsmanDocumentService, loodsmanTaskService);

            return this;
        }

        public ILoodsmanTaskManagingConnectorBuilder AddDocumentBasedTasksRemovingRequestProcessor()
        {
            ThrowIfAnyObjectIsNull(ObjectTypePair(loodsmanTaskService, typeof(ILoodsmanTaskService)));

            taskManagingRequestProcessors[typeof(DocumentBasedTasksRemovingRequestProcessor)] =
                new DocumentBasedTasksRemovingRequestProcessor(loodsmanTaskService);

            return this;
        }

        public ILoodsmanTaskManagingConnectorBuilder AddConsoleLogger()
        {
             baseLoggerConfiguration.WriteTo.Console();

            return this;
        }

        public ILoodsmanTaskManagingConnectorBuilder AddFileLogger()
        {
            baseLoggerConfiguration.Enrich.FromLogContext().WriteTo.File(
                "loodsman_task_managing_connector.log",
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 10_000_000,
                retainedFileCountLimit: 7
            );

            return this;
        }

        public ILoodsmanTaskManagingConnectorBuilder WithConstantLoodsmanObjectsNames()
        {
            baseDocumentAttributeNames = new StandardBaseDocumentAttributeNames(new ConstantBaseAttributeNames());

            return this;
        }

        public ILoodsmanTaskManagingConnectorBuilder WithMessagingTaskManagingReplyNotificator(
            IMessagingService messagingService)
        {
            taskManagingReplyNotificator = new MessagingTaskManagingReplyNotificator(messagingService);

            return this;
        }

        public ILoodsmanTaskManagingConnectorBuilder WithStandardLoodsmanDocumentService()
        {
            ThrowIfAnyObjectIsNull(
                ObjectTypePair(apiAdapter, typeof(IApiAdapter)),
                ObjectTypePair(baseDocumentAttributeNames, typeof(IBaseDocumentAttributeNames)));

            loodsmanDocumentService = new StandardLoodsmanDocumentService(apiAdapter, baseDocumentAttributeNames);

            return this;
        }

        public ILoodsmanTaskManagingConnectorBuilder WithStandardLoodsmanTaskService()
        {
            ThrowIfAnyObjectIsNull(ObjectTypePair(wbsSystemAdapter, typeof(IWbsSystemAdapter)));

            loodsmanTaskService = new StandardLoodsmanTaskService(wbsSystemAdapter);

            return this;
        }

        public abstract ILoodsmanTaskManagingConnector Build();

        private KeyValuePair<object, Type> ObjectTypePair(object key, Type type)
        {
            return new KeyValuePair<object, Type>(key, type);
        }

        private void ThrowIfAnyObjectIsNull(params KeyValuePair<object, Type>[] pairs)
        {
            var stringBuilder = new StringBuilder();

            foreach (var pair in pairs)
            {
                if (pair.Key != null)
                    continue;

                stringBuilder.Append(
                    stringBuilder.Length > 0 ? ", " + pair.Value : pair.Value.ToString());
            }

            if (stringBuilder.Length > 0)
                throw new InvalidOperationException($"Types \"{stringBuilder}\" aren't assigned");
        }
    }
}