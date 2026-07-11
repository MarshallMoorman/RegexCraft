using Avalonia;
using Avalonia.Markup.Xaml;

namespace RegexCraft.Tests.Headless;

public class TestApp : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Name = "RegexCraft";
    }
}
