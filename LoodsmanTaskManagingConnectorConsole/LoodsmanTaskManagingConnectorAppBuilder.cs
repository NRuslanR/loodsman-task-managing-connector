using AutoMapper;
using LoodsmanTaskManagingConnectorConsole.InputSources;
using LoodsmanTaskManagingConnectorConsole.InputSources.Console;
using LoodsmanTaskManagingConnectorObjects;

namespace LoodsmanTaskManagingConnectorConsole
{
    internal partial interface ILoodsmanTaskManagingConnectorApp
    {
        internal abstract class LoodsmanTaskManagingConnectorAppBuilder
        {
            protected ILoodsmanTaskManagingConnectorBuilder connectorBuilder;
            protected ILoodsmanTaskManagingConnectorInputSource connectorInputSource;

            public LoodsmanTaskManagingConnectorAppBuilder WithConsoleLoodsmanTaskManagingConnectorInputSource(
                string[] args)
            {
                connectorInputSource =
                    new ConsoleLoodsmanTaskManagingConnectorInputSource(args, CreateConsoleConnectorInputMapper());

                return this;
            }

            private IMapper CreateConsoleConnectorInputMapper()
            {
                var mapperConfig = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<ConsoleLoodsmanTaskManagingConnectorInput, RabbitMQConnectionInput>()
                        .ForMember(
                            dest => dest.Host,
                            opt => opt.MapFrom(src => src.RabbitMQHost)
                        )
                        .ForMember(
                            dest => dest.Port,
                            opt => opt.MapFrom(src => src.RabbitMQPort)
                        )
                        .ForMember(
                            dest => dest.Username,
                            opt => opt.MapFrom(src => src.RabbitMQUsername)
                        )
                        .ForMember(
                            dest => dest.Password,
                            opt => opt.MapFrom(src => src.RabbitMQPassword)
                        );

                    cfg.CreateMap<ConsoleLoodsmanTaskManagingConnectorInput, RabbitMQExchangeInput>()
                        .ForMember(
                            dest => dest.ExchangeName,
                            opt => opt.MapFrom(src => src.ExchangeName)
                        );

                    cfg.CreateMap<ConsoleLoodsmanTaskManagingConnectorInput, RabbitMQInput>()
                        .ForMember(
                            dest => dest.ConnectionInfo,
                            opt => opt.MapFrom(src => src)
                        )
                        .ForMember(
                            dest => dest.ExchangeInfo,
                            opt => opt.MapFrom(src => src)
                        )
                        .ForMember(
                            dest => dest.TaskManagingRequestQueueInfo,
                            opt => opt.MapFrom(src => new RabbitMQQueueInput
                            {
                                QueueName = src.TaskManagingRequestQueueName,
                                RoutingKey = src.TaskManagingRequestRoutingKey
                            })
                        )
                        .ForMember(
                            dest => dest.TaskManagingReplyQueueInfo,
                            opt => opt.MapFrom(src => new RabbitMQQueueInput
                            {
                                QueueName = src.TaskManagingReplyQueueName,
                                RoutingKey = src.TaskManagingReplyRoutingKey
                            })
                        );

                    cfg.CreateMap<ConsoleLoodsmanTaskManagingConnectorInput, LoodsmanInput>()
                    .ForMember(
                        dest => dest.Database,
                        opt => opt.MapFrom(src => src.LoodsmanDatabase)
                    )
                    .ForMember(
                        dest => dest.Credentials,
                        opt => opt.MapFrom(src => src.LoodsmanCredentials)
                    );

                    cfg.CreateMap<ConsoleLoodsmanTaskManagingConnectorInput, LoodsmanTaskManagingConnectorInput>()
                        .ForMember(
                            dest => dest.RabbitMQ,
                            opt => opt.MapFrom(src => src)
                        )
                        .ForMember(
                            dest => dest.Loodsman,
                            opt => opt.MapFrom(src => src)
                        );
                });

                return mapperConfig.CreateMapper();
            }

            public LoodsmanTaskManagingConnectorAppBuilder WithDataFlowLoodsmanTaskManagingConnectorBuilder()
            {
                connectorBuilder = new DataFlowLoodsmanTaskManagingConnectorBuilder();

                return this;
            }

            public abstract ILoodsmanTaskManagingConnectorApp Build();
        }
    }
}