using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Requests;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Enrollment.ChatHub
{
    public class AgentInitializer(IServiceScopeFactory scopeFactory) : IAgentInitializer
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private AIAgent? aIAgent;
        public AIAgent AIAgent => aIAgent ?? throw new InvalidOperationException("AIAgent is null.");

        public void Inilitialize(string agentIdentifier)
        {
            using var scope = _scopeFactory.CreateScope();
            IGetAgentFlowHelper getAgentFlowHelper = scope.ServiceProvider.GetRequiredService<IGetAgentFlowHelper>();
            var response = getAgentFlowHelper.RunFlow(new GetAgentRequest(agentIdentifier, "initial"));
            aIAgent = response.AIAgent;
        }
    }
}
