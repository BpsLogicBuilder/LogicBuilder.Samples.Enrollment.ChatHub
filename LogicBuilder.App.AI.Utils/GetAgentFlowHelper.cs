using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Requests;
using LogicBuilder.App.AI.Utils.Responses;
using System;

namespace LogicBuilder.App.AI.Utils
{
    public class GetAgentFlowHelper(IFlowManager flowManager) : IGetAgentFlowHelper
    {
        public GetAgentResponse RunFlow(GetAgentRequest selectorFlowRequest)
        {
            flowManager.FlowDataCache.Items[typeof(GetAgentRequest).FullName] = selectorFlowRequest;

            flowManager.Start(selectorFlowRequest.FlowName);

            if (!flowManager.FlowDataCache.Items.TryGetValue(typeof(GetAgentResponse).FullName!, out object? response))
                throw new InvalidOperationException("Agent response is null.");

            return (GetAgentResponse)response;
        }
    }
}
