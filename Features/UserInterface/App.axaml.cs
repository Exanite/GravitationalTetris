using Avalonia;
using Avalonia.Markup.Xaml;

namespace Exanite.GravitationalTetris.Features.UserInterface;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
