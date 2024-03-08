namespace LoodsmanTaskManagingConnectorObjects
{
    public class DataFlowLoodsmanTaskManagingConnectorBuilder : LoodsmanTaskManagingConnectorBuilder
    {
        public override ILoodsmanTaskManagingConnector Build()
        {
            return new DataFlowLoodsmanTaskManagingConnector
            {
                ReplyNotificator = taskManagingReplyNotificator,
                RequestPoller = taskManagingRequestPoller,
                RequestsProcessors = taskManagingRequestProcessors.Values,
                Logger = baseLoggerConfiguration.CreateLogger()
            };
        }
    }
}