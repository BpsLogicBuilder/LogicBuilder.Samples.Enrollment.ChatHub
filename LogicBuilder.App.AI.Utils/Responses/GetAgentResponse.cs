using Microsoft.Agents.AI;
using System.Diagnostics.CodeAnalysis;

namespace LogicBuilder.App.AI.Utils.Responses
{
    [ExcludeFromCodeCoverage]
    public class GetAgentResponse(AIAgent aIAgent) : IResponse
    {
        public AIAgent AIAgent { get; set; } = aIAgent;
        public bool Success { get; set; }
    }
}
