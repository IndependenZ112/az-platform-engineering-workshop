import { Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import HomePage from './pages/HomePage';
import HotelDetailPage from './pages/HotelDetailPage';
import MyBookingsPage from './pages/MyBookingsPage';

export default function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/hotels/:id" element={<HotelDetailPage />} />
        <Route path="/my-bookings" element={<MyBookingsPage />} />
      </Routes>
    </Layout>
  );
}
