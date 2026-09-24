using Microsoft.Agents.AI;

namespace Enrollment.ChatHub
{
    public interface IAgentInitializer
    {
        AIAgent? AIAgent { get; }
        void Inilitialize(string agentIdentifier);
    }
}
