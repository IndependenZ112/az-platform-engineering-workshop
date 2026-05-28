import type { Booking } from '../types';
import { api } from '../api/client';

interface BookingCardProps {
  booking: Booking;
  onCancelled: () => void;
}

export default function BookingCard({ booking, onCancelled }: BookingCardProps) {
  const handleCancel = async () => {
    if (!confirm('Are you sure you want to cancel this booking?')) return;
    await api.cancelBooking(booking.id);
    onCancelled();
  };

  const statusColor = {
    Confirmed: 'bg-green-100 text-green-800',
    Cancelled: 'bg-red-100 text-red-800',
    Completed: 'bg-blue-100 text-blue-800',
  }[booking.status] ?? 'bg-gray-100 text-gray-800';

  return (
    <div className="bg-white rounded-xl shadow-md p-5 flex justify-between items-start">
      <div className="space-y-1">
        <div className="flex items-center gap-3">
          <span className="font-mono text-sm text-gray-500">{booking.id}</span>
          <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${statusColor}`}>{booking.status}</span>
        </div>
        <p className="text-sm text-gray-700">
          📅 {new Date(booking.checkIn).toLocaleDateString()} → {new Date(booking.checkOut).toLocaleDateString()}
        </p>
        <p className="text-sm text-gray-700">👤 {booking.guestName} · {booking.numberOfGuests} guest(s)</p>
        <p className="text-lg font-bold text-indigo-700">€{booking.totalPrice}</p>
      </div>
      {booking.status === 'Confirmed' && (
        <button
          onClick={handleCancel}
          className="text-sm text-red-600 hover:text-red-800 font-medium"
        >
          Cancel
        </button>
      )}
    </div>
  );
}
