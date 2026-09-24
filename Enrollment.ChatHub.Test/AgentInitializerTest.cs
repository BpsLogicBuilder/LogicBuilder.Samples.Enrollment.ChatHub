using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Requests;
using LogicBuilder.App.AI.Utils.Responses;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
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

            var initializer = new AgentInitializer(scopeFactoryMock.Object);
            return (initializer, flowHelperMock);
        }

        [Fact]
        public void AIAgent_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var initializer = new AgentInitializer(scopeFactoryMock.Object);

            Assert.Throws<InvalidOperationException>(() => initializer.AIAgent);
        }

        [Fact]
        public void Inilitialize_SetsAIAgent_FromFlowResponse()
        {
            var agentMock = new Mock<AIAgent>();
            var response = new GetAgentResponse { AIAgent = agentMock.Object };
            var (initializer, _) = CreateInitializer(response);

            initializer.Inilitialize("some-agent");

            Assert.Same(agentMock.Object, initializer.AIAgent);
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

            var initializer = new AgentInitializer(scopeFactoryMock.Object);
            initializer.Inilitialize("agent-abc");

            scopeFactoryMock.Verify(f => f.CreateScope(), Times.Once);
        }
    }
}
