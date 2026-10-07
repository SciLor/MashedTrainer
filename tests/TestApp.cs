using Avalonia;
using Avalonia.Headless;
using SciLors_Mashed_Trainer;
using SciLors_Mashed_Trainer.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace SciLors_Mashed_Trainer.Tests {
    public class TestAppBuilder {
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UseSkia()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions {
                    UseHeadlessDrawing = false
                });
    }
}
