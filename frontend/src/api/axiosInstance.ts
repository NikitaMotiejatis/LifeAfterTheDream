import axios from 'axios';

const axiosInstance = axios.create({
  baseURL: 'https://localhost:61303/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// 409 Conflict handling placeholder
axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 409) {
      // TODO: trigger concurrency conflict modal (e.g., dispatch custom event, show toast)
      console.warn('409 Conflict detected', error.response.data);
    }
    return Promise.reject(error);
  },
);

export default axiosInstance;
