import React, { FC, useEffect } from 'react';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Agent } from '../../models/agent';

type Props = {
    agent: Agent;
};

const AgentSummaryTab: FC<Props> = (props) => {
    const cancelToken = useCancellationToken();

    useEffect(() => {
        (async () => {})();
    }, [props.agent, cancelToken]);

    return <h1>Summary</h1>;
};

export default AgentSummaryTab;
