import axios from 'axios';
import React, { FC } from 'react';
import { OmitNative } from 'react-router';
import { HashRouter, Redirect, Route, RouteProps, Switch } from 'react-router-dom';
import './App.scss';
import MainLayout from './containers/MainLayout';
import Login from './pages/Login';
import Auth from './services/auth';
import { ErrorHelper } from './services/error';

const loading = (
    <div className="pt-3 text-center">
        <div className="sk-spinner sk-spinner-pulse"></div>
    </div>
);

type PrivateRouteProps = RouteProps & OmitNative<{}, keyof RouteProps> & {};

axios.interceptors.response.use(
    (request) => {
        return request;
    },
    (error) => {
        if (error && error.response && error.response.status === 401) {
            Auth.logout();
        }
        const formattedError = ErrorHelper.getError(error);
        throw formattedError;
    }
);

const PrivateRoute: FC<PrivateRouteProps> = (props) => {
    const { children, ...rest } = props;

    return (
        <Route
            {...rest}
            render={({ location }) => {
                return Auth.hasAuthToken() ? (
                    children
                ) : (
                    <Redirect
                        to={{
                            pathname: '/login',
                            state: { from: location },
                        }}
                    />
                );
            }}
        />
    );
};

const App: FC = () => {
    return (
        <HashRouter>
            <React.Suspense fallback={loading}>
                <Switch>
                    <Route exact path="/login" render={(props) => <Login {...props} />} />

                    <PrivateRoute path="/">
                        <MainLayout />
                    </PrivateRoute>
                </Switch>
            </React.Suspense>
        </HashRouter>
    );
};

export default App;
