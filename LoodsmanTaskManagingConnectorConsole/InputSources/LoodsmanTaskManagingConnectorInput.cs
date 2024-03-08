namespace LoodsmanTaskManagingConnectorConsole.InputSources
{
    internal class LoodsmanTaskManagingConnectorInput
    {
        public RabbitMQInput RabbitMQ { get; set; }

        public LoodsmanInput Loodsman { get; set; }
    }

    internal class RabbitMQInput
    {
        public RabbitMQConnectionInput ConnectionInfo { get; set; }

        public RabbitMQExchangeInput ExchangeInfo { get; set; }

        public RabbitMQQueueInput TaskManagingRequestQueueInfo { get; set; }

        public RabbitMQQueueInput TaskManagingReplyQueueInfo { get; set; }
    }

    internal class RabbitMQConnectionInput
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }

    internal class RabbitMQExchangeInput
    {
        public string ExchangeName { get; set; }
    }

    internal class RabbitMQQueueInput
    {
        public string QueueName { get; set; }

        public string RoutingKey { get; set; }
    }

    internal class LoodsmanInput
    {
        public string Database { get; set; }

        public string Credentials { get; set; }
    }
}