using LogicBuilder.App.AI.Utils.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Enrollment.ChatHub
{
    public class AgentAIChatHub(IAgentHandler agentHandler, IAgentInitializer agentInitializer) : Hub
    {
        private readonly IAgentHandler agentHandler = agentHandler;
        private readonly IAgentInitializer agentInitializer = agentInitializer;

        public async Task SendMessageToAgent(string? threadId, string userMessage, string agentIdentifier)
        {
            if (string.IsNullOrEmpty(threadId))
            {
                this.agentInitializer.Inilitialize(agentIdentifier);
                threadId = await agentHandler.InitializeSession(this.agentInitializer.AIAgent ?? throw new System.InvalidOperationException("AI Agent cannot be null."));
                await Clients.Caller.SendAsync("SessionInitialized", threadId);
            }

            try
            {
                var result = await agentHandler.SendMessageToAgent
                (
                    this.agentInitializer.AIAgent ?? throw new System.InvalidOperationException("AI Agent cannot be null."),
                    threadId, 
                    userMessage, 
                    Context.ConnectionAborted
                );
                await Clients.Caller.SendAsync("ReceiveAgentChunk", threadId, result.ContentChunk);
                await Clients.Caller.SendAsync("ReceiveAgentResponseComplete", result.UpdatedThreadId);
            }
            catch (OperationCanceledException)
            {
                // Safe handling for when a client disconnects or aborts mid-stream
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveAgentError", threadId, $"Execution failed: {ex.Message}");
            }
        }
    }
}
