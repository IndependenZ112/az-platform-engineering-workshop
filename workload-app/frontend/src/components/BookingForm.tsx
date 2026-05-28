import { useState } from 'react';
import type { Room, CreateBookingRequest, BookingConfirmation } from '../types';
import { api } from '../api/client';

interface BookingFormProps {
  hotelId: string;
  rooms: Room[];
}

export default function BookingForm({ hotelId, rooms }: BookingFormProps) {
  const [selectedRoom, setSelectedRoom] = useState('');
  const [guestName, setGuestName] = useState('');
  const [guestEmail, setGuestEmail] = useState('');
  const [checkIn, setCheckIn] = useState('');
  const [checkOut, setCheckOut] = useState('');
  const [guests, setGuests] = useState(1);
  const [confirmation, setConfirmation] = useState<BookingConfirmation | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const req: CreateBookingRequest = {
        hotelId,
        roomId: selectedRoom,
        guestName,
        guestEmail,
        checkIn,
        checkOut,
        numberOfGuests: guests,
      };
      const result = await api.createBooking(req);
      setConfirmation(result);
    } catch {
      setError('Booking failed. Please check your details and try again.');
    } finally {
      setLoading(false);
    }
  };

  if (confirmation) {
    return (
      <div className="bg-green-50 border border-green-200 rounded-xl p-6">
        <h3 className="text-lg font-bold text-green-800 mb-2">✅ Booking Confirmed!</h3>
        <div className="space-y-1 text-sm text-green-700">
          <p><strong>Booking ID:</strong> {confirmation.bookingId}</p>
          <p><strong>Hotel:</strong> {confirmation.hotelName}</p>
          <p><strong>Room:</strong> {confirmation.roomType}</p>
          <p><strong>Check-in:</strong> {new Date(confirmation.checkIn).toLocaleDateString()}</p>
          <p><strong>Check-out:</strong> {new Date(confirmation.checkOut).toLocaleDateString()}</p>
          <p><strong>Total:</strong> €{confirmation.totalPrice}</p>
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="bg-white rounded-xl shadow-md p-6 space-y-4">
      <h3 className="text-lg font-bold text-gray-800">Book a Room</h3>
      {error && <p className="text-red-600 text-sm">{error}</p>}
      <div>
        <label htmlFor="room-select" className="block text-sm font-medium text-gray-600 mb-1">Select Room</label>
        <select
          id="room-select"
          required
          value={selectedRoom}
          onChange={e => setSelectedRoom(e.target.value)}
          className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm"
        >
          <option value="">Choose a room...</option>
          {rooms.filter(r => r.isAvailable).map(r => (
            <option key={r.id} value={r.id}>
              {r.type} — €{r.pricePerNight}/night (up to {r.maxGuests} guests)
            </option>
          ))}
        </select>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label htmlFor="guest-name" className="block text-sm font-medium text-gray-600 mb-1">Full Name</label>
          <input id="guest-name" required type="text" value={guestName} onChange={e => setGuestName(e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" />
        </div>
        <div>
          <label htmlFor="guest-email" className="block text-sm font-medium text-gray-600 mb-1">Email</label>
          <input id="guest-email" required type="email" value={guestEmail} onChange={e => setGuestEmail(e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" />
        </div>
      </div>
      <div className="grid grid-cols-3 gap-4">
        <div>
          <label htmlFor="check-in" className="block text-sm font-medium text-gray-600 mb-1">Check-in</label>
          <input id="check-in" required type="date" value={checkIn} onChange={e => setCheckIn(e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" />
        </div>
        <div>
          <label htmlFor="check-out" className="block text-sm font-medium text-gray-600 mb-1">Check-out</label>
          <input id="check-out" required type="date" value={checkOut} onChange={e => setCheckOut(e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" />
        </div>
        <div>
          <label htmlFor="num-guests" className="block text-sm font-medium text-gray-600 mb-1">Guests</label>
          <input id="num-guests" required type="number" min={1} max={10} value={guests} onChange={e => setGuests(+e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" />
        </div>
      </div>
      <button
        type="submit"
        disabled={loading}
        className="w-full bg-indigo-600 text-white rounded-lg py-2.5 font-medium hover:bg-indigo-700 transition disabled:opacity-50"
      >
        {loading ? 'Booking...' : '📝 Confirm Booking'}
      </button>
    </form>
  );
}
