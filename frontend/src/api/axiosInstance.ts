import axios from 'axios';

axios.defaults.withCredentials = true;

const axiosInstance = axios.create({
  baseURL: 'https://localhost:61303/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// NFR: Optimistic Locking — 409 Conflict indicates a concurrent edit collision.
// The user is notified so they can refresh and retry.
axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 409) {
      console.warn(
        '409 Conflict detected — concurrent edit collision',
        error.response.data,
      );
    }
    return Promise.reject(error);
  },
);

export default axiosInstance;
