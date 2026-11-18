using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Requests;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Enrollment.ChatHub
{
    public class AgentInitializer(IServiceScopeFactory scopeFactory, IWebHostEnvironment environment) : IAgentInitializer
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly IWebHostEnvironment environment = environment;
        private AIAgent? aIAgent;
        public AIAgent AIAgent => aIAgent ?? throw new InvalidOperationException("AIAgent is null.");

        public void Inilitialize(string agentIdentifier)
        {
            using var scope = _scopeFactory.CreateScope();
            IGetAgentFlowHelper getAgentFlowHelper = scope.ServiceProvider.GetRequiredService<IGetAgentFlowHelper>();
            var response = getAgentFlowHelper.RunFlow(new GetAgentRequest(agentIdentifier, "initial"));
            aIAgent = (response.AIAgent ?? throw new InvalidOperationException("AIAgent is null."))
                .AsBuilder()
                .UseOpenTelemetry(sourceName: Constants.AgentSourceName, configure: cfg =>
                {
                    cfg.EnableSensitiveData = environment.IsDevelopment();
                })
                .Build();
        }
    }
}
