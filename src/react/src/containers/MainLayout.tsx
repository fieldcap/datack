import React, { FC } from 'react';
import { Route, Switch } from 'react-router-dom';
import ServerOverview from '../pages/servers/ServerOverview';
import './MainLayout.scss';
import NavLayout from './NavLayout';

const MainLayout: FC = () => {
    return (
        <div className="wrapper">
            <NavLayout></NavLayout>
            <div className="content">
                <Switch>
                    <Route
                        path="/server/:id"
                        render={(props) => <ServerOverview {...props} />}
                    />
                </Switch>
            </div>
        </div>
    );
};

export default MainLayout;
