import React, { FC } from 'react';
import { Server } from '../models/server';
import Servers from '../services/servers';
import './NavLayout.scss';

type Props = {};

const NavLayout: FC<Props> = () => {
    let [servers, setServers] = React.useState<Server[]>([]);

    React.useEffect(() => {
        const fetchData = async () => {
            try {
                const result = await Servers.getList();

                setServers(result);
            } catch {
                console.log('err');
            }
        };
        fetchData();
    }, []);

    return (
        <nav className="sidebar">
            <div className="branding">Datack SQL</div>

            <ul className="list-unstyled nav-items">
                <li className="active nav-item">
                    <a href="/#/">Home</a>
                </li>
                <li className="nav-item">
                    <a href="/#/Settings">Settings</a>
                </li>
                {servers.map((server) => (
                    <li className="nav-item" key={server.serverId}>
                        <a href={'/#/server/' + server.serverId}>
                            {server.name}
                        </a>
                    </li>
                ))}
            </ul>
        </nav>
    );
};

export default NavLayout;
