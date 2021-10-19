import React, { FC, useEffect } from 'react';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Server } from '../../models/server';

type Props = {
    server: Server;
};

const ServerSummaryTab: FC<Props> = (props) => {
    const cancelToken = useCancellationToken();

    useEffect(() => {
        (async () => {})();
    }, [props.server, cancelToken]);

    return <h1>Summary</h1>;
};

export default ServerSummaryTab;
