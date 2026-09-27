using Bindito.Core;
using Timberborn.TemplateInstantiation;

namespace DirectionalPowerShafts;

[Context("Game")]
public sealed class DirectionalShaftConfigurator : Configurator
{
    protected override void Configure()
    {
        Bind<DirectionalShaft>().AsTransient();
        MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }

    private static TemplateModule ProvideTemplateModule()
    {
        var builder = new TemplateModule.Builder();
        builder.AddDecorator<DirectionalShaftSpec, DirectionalShaft>();
        return builder.Build();
    }
}
