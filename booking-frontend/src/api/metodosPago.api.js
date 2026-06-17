import client from './client';

export const metodosPagoApi = {
  getAll: () => client.get('/metodospago-alojaexpress'),
};
