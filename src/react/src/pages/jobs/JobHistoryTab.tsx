import { Button, Skeleton } from '@chakra-ui/react';
import axios from 'axios';
import React, { FC, useEffect, useState } from 'react';
import { Job } from '../../models/job';
import { Server } from '../../models/server';
import Jobs from '../../services/jobs';
import Servers from '../../services/servers';

type JobHistoryTabTabProps = {
    job: Job | null;
};

const JobHistoryTab: FC<JobHistoryTabTabProps> = (props) => {
    let [isLoaded, setIsLoaded] = useState<boolean>(false);
    let [servers, setServers] = useState<Server[]>([]);

    useEffect(() => {
        if (props.job == null) {
            return;
        }

        const getByIdCancelToken = axios.CancelToken.source();

        (async () => {
            const result = await Servers.getList(getByIdCancelToken);
            setServers(result);
            setIsLoaded(true);
        })();

        return () => {
            getByIdCancelToken.cancel();
        };
    }, [props.job]);

    const run = async (type: 'Full' | 'Diff' | 'Log') => {
        if (props.job == null) {
            return;
        }
        if (servers.length === 0) {
            return;
        }

        const serverId = servers[0].serverId;

        await Jobs.run(serverId, props.job.jobId, type);
    };

    return (
        <Skeleton isLoaded={isLoaded}>
            <Button onClick={() => run('Full')}>Run Full Backup</Button>
            <Button marginLeft="12px" onClick={() => run('Diff')}>
                Run Diff Backup
            </Button>
            <Button marginLeft="12px" onClick={() => run('Log')}>
                Run Transaction Log Backup
            </Button>
        </Skeleton>
    );
};

export default JobHistoryTab;
