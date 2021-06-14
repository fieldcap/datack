import axios from 'axios';
import React, { FC } from 'react';
import { RouteComponentProps, useHistory } from 'react-router-dom';
import { Server } from '../../models/server';
import Servers from '../../services/servers';
import './ServerOverview.scss';

type RouteParams = {
    id: string;
};

const ServerOverview: FC<RouteComponentProps<RouteParams>> = (props) => {
    let [server, setServer] = React.useState<Server | null>(null);

    const history = useHistory();

    const getByIdCancelToken = axios.CancelToken.source();

    React.useEffect(() => {
        const fetchData = async () => {
            try {
                const result = await Servers.getById(
                    props.match.params.id,
                    getByIdCancelToken
                );
                setServer(result);
            } catch {
                history.push('/');
            }
        };
        fetchData();

        return () => {
            getByIdCancelToken.cancel();
        };
    }, [props.match.params.id]);

    if (!server) {
        return <></>;
    }

    return (
        <>
            <h1>{server.name}</h1>

            {/* <Tabs defaultActiveKey="home" transition={false}>
                <Tab eventKey="home" title="Jobs">
                    <ServerJobsTab server={server}></ServerJobsTab>
                </Tab>
                <Tab eventKey="dbSettings" title="Database Settings">
                    <ServerDbSettingsTab server={server}></ServerDbSettingsTab>
                </Tab>
                <Tab eventKey="settings" title="Settings"></Tab>
            </Tabs> */}
        </>
    );
};

export default ServerOverview;
