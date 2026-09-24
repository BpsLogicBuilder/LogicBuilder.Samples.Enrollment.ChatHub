using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Structures;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Enrollment.ChatHub.Test
{
    public class AgentAIChatHubTest
    {
        private readonly Mock<IAgentHandler> agentHandlerMock = new();
        private readonly Mock<IAgentInitializer> agentInitializerMock = new();
        private readonly Mock<ISingleClientProxy> callerProxyMock = new();
        private readonly Mock<IHubCallerClients> hubCallerClientsMock = new();
        private readonly Mock<HubCallerContext> hubCallerContextMock = new();

        private AgentAIChatHub CreateHub()
        {
            hubCallerClientsMock.Setup(c => c.Caller).Returns(callerProxyMock.Object);
            hubCallerContextMock.Setup(c => c.ConnectionAborted).Returns(CancellationToken.None);

            var hub = new AgentAIChatHub(agentHandlerMock.Object, agentInitializerMock.Object)
            {
                Clients = hubCallerClientsMock.Object,
                Context = hubCallerContextMock.Object
            };
            return hub;
        }

        [Fact]
        public async Task SendMessageToAgent_WhenThreadIdIsNull_InitializesSessionAndNotifiesCaller()
        {
            var agentMock = new Mock<AIAgent>();
            agentInitializerMock.Setup(i => i.AIAgent).Returns(agentMock.Object);
            agentHandlerMock.Setup(h => h.InitializeSession(agentMock.Object)).ReturnsAsync("new-thread-id");
            agentHandlerMock
                .Setup(h => h.SendMessageToAgent(agentMock.Object, "new-thread-id", "hello", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AgentStreamResult("chunk", "new-thread-id"));

            var hub = CreateHub();

            await hub.SendMessageToAgent(null, "hello", "agent-1");

            agentInitializerMock.Verify(i => i.Inilitialize("agent-1"), Times.Once);
            agentHandlerMock.Verify(h => h.InitializeSession(agentMock.Object), Times.Once);
            callerProxyMock.Verify(c => c.SendCoreAsync("SessionInitialized", It.Is<object?[]>(a => (string)a[0]! == "new-thread-id"), It.IsAny<CancellationToken>()), Times.Once);
            callerProxyMock.Verify(c => c.SendCoreAsync("ReceiveAgentChunk", It.Is<object?[]>(a => (string)a[0]! == "new-thread-id" && (string)a[1]! == "chunk"), It.IsAny<CancellationToken>()), Times.Once);
            callerProxyMock.Verify(c => c.SendCoreAsync("ReceiveAgentResponseComplete", It.Is<object?[]>(a => (string)a[0]! == "new-thread-id"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendMessageToAgent_WhenThreadIdProvided_DoesNotReinitializeSession()
        {
            var agentMock = new Mock<AIAgent>();
            agentInitializerMock.Setup(i => i.AIAgent).Returns(agentMock.Object);
            agentHandlerMock
                .Setup(h => h.SendMessageToAgent(agentMock.Object, "existing-thread-id", "hello", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AgentStreamResult("chunk", "existing-thread-id"));

            var hub = CreateHub();

            await hub.SendMessageToAgent("existing-thread-id", "hello", "agent-1");

            agentInitializerMock.Verify(i => i.Inilitialize(It.IsAny<string>()), Times.Never);
            agentHandlerMock.Verify(h => h.InitializeSession(It.IsAny<AIAgent>()), Times.Never);
            callerProxyMock.Verify(c => c.SendCoreAsync("SessionInitialized", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SendMessageToAgent_WhenOperationCanceledExceptionThrown_DoesNotSendError()
        {
            var agentMock = new Mock<AIAgent>();
            agentInitializerMock.Setup(i => i.AIAgent).Returns(agentMock.Object);
            agentHandlerMock
                .Setup(h => h.SendMessageToAgent(agentMock.Object, "existing-thread-id", "hello", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var hub = CreateHub();

            await hub.SendMessageToAgent("existing-thread-id", "hello", "agent-1");

            callerProxyMock.Verify(c => c.SendCoreAsync("ReceiveAgentError", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SendMessageToAgent_WhenExceptionThrown_SendsErrorToCaller()
        {
            var agentMock = new Mock<AIAgent>();
            agentInitializerMock.Setup(i => i.AIAgent).Returns(agentMock.Object);
            agentHandlerMock
                .Setup(h => h.SendMessageToAgent(agentMock.Object, "existing-thread-id", "hello", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var hub = CreateHub();

            await hub.SendMessageToAgent("existing-thread-id", "hello", "agent-1");

            callerProxyMock.Verify(
                c => c.SendCoreAsync(
                    "ReceiveAgentError",
                    It.Is<object?[]>(a => (string)a[0]! == "existing-thread-id" && ((string)a[1]!).Contains("boom")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
