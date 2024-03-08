namespace LoodsmanTaskManagingConnectorConsole
{
    internal partial interface ILoodsmanTaskManagingConnectorApp
    {
        internal class ConsoleLoodsmanTaskManagingConnectorAppBuilder : LoodsmanTaskManagingConnectorAppBuilder
        {
            private ConsoleLoodsmanTaskManagingConnectorAppBuilder()
            {
            }

            public static LoodsmanTaskManagingConnectorAppBuilder Instance { get; } =
                new ConsoleLoodsmanTaskManagingConnectorAppBuilder();

            public override ILoodsmanTaskManagingConnectorApp Build()
            {
                return new LoodsmanTaskManagingConnectorConsoleApp(connectorInputSource, connectorBuilder);
            }
        }
    }
}