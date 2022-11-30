const { createProxyMiddleware } = require('http-proxy-middleware');

// eslint-disable-next-line func-names, no-undef
module.exports = function (app) {
  app.use(
    '/api',
    createProxyMiddleware({
      target: 'http://localhost:3001',
      changeOrigin: true,
    })
  );
  app.use(
    createProxyMiddleware('/hubs/web', {
      target: 'http://localhost:3001',
      ws: true,
      changeOrigin: true,
    })
  );
};
