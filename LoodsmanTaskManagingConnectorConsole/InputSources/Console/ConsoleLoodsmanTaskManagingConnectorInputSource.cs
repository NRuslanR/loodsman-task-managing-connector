using System;
using System.Linq;
using AutoMapper;
using CommandLine;

namespace LoodsmanTaskManagingConnectorConsole.InputSources.Console
{
    internal class ConsoleLoodsmanTaskManagingConnectorInputSource : ILoodsmanTaskManagingConnectorInputSource
    {
        private readonly string[] commandLineArgs;
        private readonly IMapper connectorInputMapper;

        public ConsoleLoodsmanTaskManagingConnectorInputSource(string[] commandLineArgs, IMapper connectorInputMapper)
        {
            this.commandLineArgs = commandLineArgs;
            this.connectorInputMapper = connectorInputMapper;
        }

        public LoodsmanTaskManagingConnectorInput GetLoodsmanTaskManagingConnectorInput()
        {
            var connectorInput = ParseConnectorInputFrom(commandLineArgs);

            return MapConnectorInput(connectorInput);
        }

        private ConsoleLoodsmanTaskManagingConnectorInput ParseConnectorInputFrom(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<ConsoleLoodsmanTaskManagingConnectorInput>(commandLineArgs);
            
            if (parseResult.Errors.Any())
                throw new Exception("Input command line args aren't correct");

            return parseResult.Value;
        }

        private LoodsmanTaskManagingConnectorInput MapConnectorInput(
            ConsoleLoodsmanTaskManagingConnectorInput connectorInput)
        {
            return connectorInputMapper
                .Map<ConsoleLoodsmanTaskManagingConnectorInput, LoodsmanTaskManagingConnectorInput>(
                    connectorInput);
        }
    }
}