using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.DependencyInjection;
using Umbraco.Extensions;
using Umbraco.Infrastructure.PublishedCache.DependencyInjection;
using Umbraco.Tests.Integration.Extensions;
using Umbraco.Tests.Integration.Implementations;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;
using Umbraco.Web.Common.DependencyInjection;

namespace Umbraco.Tests.Integration
{

    [TestFixture]
    [UmbracoTest(Boot = true)]
    public class ComponentRuntimeTests : UmbracoIntegrationTest
    {
        // ensure composers are added
        protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddComposers();

        /// <summary>
        /// This will boot up umbraco with components enabled to show they initialize and shutdown
        /// </summary>
        [Test]
        public async Task Start_And_Stop_Umbraco_With_Components_Enabled()
        {
            IRuntime runtime = Services.GetRequiredService<IRuntime>();
            IRuntimeState runtimeState = Services.GetRequiredService<IRuntimeState>();
            IMainDom mainDom = Services.GetRequiredService<IMainDom>();
            ComponentCollection components = Services.GetRequiredService<ComponentCollection>();

            MyComponent myComponent = components.OfType<MyComponent>().First();

            Assert.IsTrue(mainDom.IsMainDom);
            Assert.IsNull(runtimeState.BootFailedException);
            Assert.IsTrue(myComponent.IsInit, "The component was not initialized");

            // force stop now
            await runtime.StopAsync(CancellationToken.None);
            Assert.IsTrue(myComponent.IsTerminated, "The component was not terminated");
        }

        public class MyComposer : IUserComposer
        {
            public void Compose(IUmbracoBuilder builder) => builder.Components().Append<MyComponent>();
        }

        public class MyComponent : IComponent
        {
            public bool IsInit { get; private set; }

            public bool IsTerminated { get; private set; }

            private readonly ILogger<MyComponent> _logger;

            public MyComponent(ILogger<MyComponent> logger) => _logger = logger;

            public void Initialize() => IsInit = true;

            public void Terminate() => IsTerminated = true;

        }
    }
}