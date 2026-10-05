using System.Windows;
using System.Windows.Media.Animation;

namespace ACTA.Views;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();

        Opacity = 0;

        Loaded += (_, _) =>
        {
            FadeIn();
        };
    }


    private void FadeIn()
    {
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(350)
        };

        BeginAnimation(OpacityProperty, animation);
    }


    public void SetProgress(double value, string message)
    {
        var animation = new DoubleAnimation
        {
            To = value,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase()
        };

        LoadingProgress.BeginAnimation(
            System.Windows.Controls.ProgressBar.ValueProperty,
            animation
        );

        StatusText.Text = message;
    }


    public async Task FadeOutAsync()
    {
        var completion = new TaskCompletionSource();

        var animation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300)
        };

        animation.Completed += (_, _) =>
        {
            completion.SetResult();
        };

        BeginAnimation(OpacityProperty, animation);

        await completion.Task;
    }
}