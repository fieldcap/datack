import axios from 'axios';
import { curray } from 'curray';
import React, { FC } from 'react';
import { HashRouter, Route, Routes } from 'react-router-dom';
import './App.scss';
import MainLayout from './containers/MainLayout';
import Login from './pages/Login';
import Auth from './services/auth';
import { ErrorHelper } from './services/error';

curray();

const loading = (
  <div className="pt-3 text-center">
    <div className="sk-spinner sk-spinner-pulse"></div>
  </div>
);

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

const App: FC = () => {
  return (
    <HashRouter>
      <React.Suspense fallback={loading}>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="*" element={<MainLayout />} />
        </Routes>
      </React.Suspense>
    </HashRouter>
  );
};

export default App;
