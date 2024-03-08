using CommandLine;

namespace LoodsmanTaskManagingConnectorConsole.InputSources.Console
{
    internal class ConsoleLoodsmanTaskManagingConnectorInput
    {
        [Option("rmq_host", HelpText = "RabbitMQ host name", Required = true)]
        public string RabbitMQHost { get; set; }

        [Option("rmq_port", HelpText = "RabbitMQ host port", Required = false, Default = default(int))]
        public int RabbitMQPort { get; set; }

        [Option("rmq_user", HelpText = "RabbitMQ user name", Required = true)]
        public string RabbitMQUsername { get; set; }

        [Option("rmq_pass", HelpText = "RabbitMQ user password", Required = true)]
        public string RabbitMQPassword { get; set; }

        [Option("rmq_exchange", HelpText = "RabbitMQ exchange name", Required = true)]
        public string ExchangeName { get; set; }

        [Option("rmq_req_queue", HelpText = "Task managing request RabbitMQ queue name", Required = true)]
        public string TaskManagingRequestQueueName { get; set; }

        [Option("rmq_req_rkey", HelpText = "Task managing request RabbitMQ routing key", Required = true)]
        public string TaskManagingRequestRoutingKey { get; set; }

        [Option("rmq_rep_queue", HelpText = "Task managing reply RabbitMQ queue name", Required = true)]
        public string TaskManagingReplyQueueName { get; set; }

        [Option("rmq_rep_rkey", HelpText = "Task managing reply RabbitMQ routing key", Required = true)]
        public string TaskManagingReplyRoutingKey { get; set; }

        [Option("lood_db", HelpText = "Loodsman database name", Required = true)]
        public string LoodsmanDatabase { get; set; }

        [Option("lood_auth", HelpText = "Loodsman authentication credentials", Required = false,
            Default = default(string))]
        public string LoodsmanCredentials { get; set; }
    }
}