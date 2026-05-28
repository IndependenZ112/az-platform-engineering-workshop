import { useState } from 'react';
import { api } from '../api/client';
import type { Booking } from '../types';
import BookingCard from '../components/BookingCard';

export default function MyBookingsPage() {
  const [email, setEmail] = useState('');
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [searched, setSearched] = useState(false);
  const [loading, setLoading] = useState(false);

  const search = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim()) return;
    setLoading(true);
    try {
      const results = await api.getBookingsByEmail(email);
      setBookings(results);
    } finally {
      setSearched(true);
      setLoading(false);
    }
  };

  const refresh = () => {
    api.getBookingsByEmail(email).then(setBookings);
  };

  return (
    <div className="max-w-2xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">My Bookings</h1>

      <form onSubmit={search} className="bg-white rounded-xl shadow-md p-6 mb-8">
        <label className="block text-sm font-medium text-gray-600 mb-2">Enter your email to find your bookings</label>
        <div className="flex gap-3">
          <input
            type="email"
            required
            value={email}
            onChange={e => setEmail(e.target.value)}
            placeholder="your@email.com"
            className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-500"
          />
          <button
            type="submit"
            disabled={loading}
            className="bg-indigo-600 text-white rounded-lg px-6 py-2 text-sm font-medium hover:bg-indigo-700 transition"
          >
            {loading ? '...' : 'Search'}
          </button>
        </div>
      </form>

      {searched && (
        <div className="space-y-4">
          {bookings.length === 0 ? (
            <p className="text-center text-gray-400 py-8">No bookings found for this email.</p>
          ) : (
            bookings.map(b => <BookingCard key={b.id} booking={b} onCancelled={refresh} />)
          )}
        </div>
      )}
    </div>
  );
}
