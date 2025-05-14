// See https://aka.ms/new-console-template for more information

using ContosoAds.Cli;
using Oakton;

var executor = CommandExecutor.For(configure =>
{
    configure.RegisterCommand<CreateAdCommand>();
});
return await executor.ExecuteAsync(args);
