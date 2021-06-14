import React from 'react';
import { HashRouter, Route, Switch } from 'react-router-dom';
import './App.scss';

const loading = (
    <div className="pt-3 text-center">
        <div className="sk-spinner sk-spinner-pulse"></div>
    </div>
);

const MainLayout = React.lazy(() => import('./containers/MainLayout'));
const Login = React.lazy(() => import('./pages/Login'));

function App() {
    return (
        <HashRouter>
            <React.Suspense fallback={loading}>
                <Switch>
                    <Route
                        exact
                        path="/login"
                        render={(props) => <Login {...props} />}
                    />
                    <Route
                        path="/"
                        render={(props) => <MainLayout {...props} />}
                    />
                </Switch>
            </React.Suspense>
        </HashRouter>
    );
}

export default App;
