using Microsoft.Agents.AI;

namespace LogicBuilder.Samples.Enrollment.ChatHub
{
    public interface IAgentInitializer
    {
        AIAgent? AIAgent { get; }
        void Inilitialize(string agentIdentifier);
    }
}
