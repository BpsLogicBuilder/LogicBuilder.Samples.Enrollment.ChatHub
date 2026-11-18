using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Requests;
using LogicBuilder.App.AI.Utils.Responses;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Moq;

namespace Enrollment.ChatHub.Test
{
    public class AgentInitializerTest
    {
        private static (AgentInitializer initializer, Mock<IGetAgentFlowHelper> flowHelperMock) CreateInitializer(GetAgentResponse response)
        {
            var flowHelperMock = new Mock<IGetAgentFlowHelper>();
            flowHelperMock
                .Setup(f => f.RunFlow(It.IsAny<GetAgentRequest>()))
                .Returns(response);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IGetAgentFlowHelper)))
                .Returns(flowHelperMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            var environmentMock = new MockEnvironment();

            var initializer = new AgentInitializer(scopeFactoryMock.Object, environmentMock);
            return (initializer, flowHelperMock);
        }

        [Fact]
        public void AIAgent_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var environmentMock = new MockEnvironment();
            var initializer = new AgentInitializer(scopeFactoryMock.Object, environmentMock);

            Assert.Throws<InvalidOperationException>(() => initializer.AIAgent);
        }

        [Fact]
        public void Inilitialize_SetsAIAgent_FromFlowResponse()
        {
            var agentMock = new Mock<AIAgent>();
            OpenTelemetryAgent telemetryAgent = (OpenTelemetryAgent)agentMock.Object.AsBuilder()
                .UseOpenTelemetry(sourceName: "Contoso.ChatHub", configure: cfg =>
                {
                    cfg.EnableSensitiveData = false;
                })
                .Build();
            var response = new GetAgentResponse { AIAgent = agentMock.Object };
            var (initializer, _) = CreateInitializer(response);

            initializer.Inilitialize("some-agent");

            Assert.Same(telemetryAgent.Id, ((OpenTelemetryAgent)initializer.AIAgent).Id);
        }

        [Fact]
        public void Inilitialize_ThrowsInvalidOperationException_WhenGetAgentResponseAIAgent_IsNull()
        {
            var response = new GetAgentResponse { AIAgent = null };
            var (initializer, _) = CreateInitializer(response);
            Assert.Throws<InvalidOperationException>(() => initializer.Inilitialize("some-agent"));
        }

        [Fact]
        public void Inilitialize_CallsRunFlow_WithExpectedRequest()
        {
            var agentMock = new Mock<AIAgent>();
            var response = new GetAgentResponse { AIAgent = agentMock.Object };
            var (initializer, flowHelperMock) = CreateInitializer(response);

            initializer.Inilitialize("agent-123");

            flowHelperMock.Verify(
                f => f.RunFlow(It.Is<GetAgentRequest>(r => r.AgentIdentifier == "agent-123" && r.FlowName == "initial")),
                Times.Once);
        }

        [Fact]
        public void Inilitialize_CreatesScope_FromScopeFactory()
        {
            var agentMock = new Mock<AIAgent>();
            var response = new GetAgentResponse { AIAgent = agentMock.Object };

            var flowHelperMock = new Mock<IGetAgentFlowHelper>();
            flowHelperMock
                .Setup(f => f.RunFlow(It.IsAny<GetAgentRequest>()))
                .Returns(response);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IGetAgentFlowHelper)))
                .Returns(flowHelperMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            var environmentMock = new MockEnvironment();

            var initializer = new AgentInitializer(scopeFactoryMock.Object, environmentMock);
            initializer.Inilitialize("agent-abc");

            scopeFactoryMock.Verify(f => f.CreateScope(), Times.Once);
        }

        public class MockEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public IFileProvider WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string ContentRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string EnvironmentName { get => "Development"; set => throw new NotImplementedException(); }
        }
    }
}
