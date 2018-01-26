using System.Reflection;

namespace LottieUWP.Tests
{
    sealed partial class App
    {
        protected override void OnInitializeRunner()
        {
            AddTestAssembly(GetType().GetTypeInfo().Assembly);
        }
    }
}
