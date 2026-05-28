import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../api/client';
import type { Hotel } from '../types';
import BookingForm from '../components/BookingForm';

export default function HotelDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [hotel, setHotel] = useState<Hotel | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (id) api.getHotel(id).then(setHotel).finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="text-center py-20 text-gray-400">Loading...</div>;
  if (!hotel) return <div className="text-center py-20 text-gray-400">Hotel not found</div>;

  return (
    <div>
      <Link to="/" className="text-indigo-600 text-sm hover:underline mb-4 inline-block">← Back to Hotels</Link>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Hotel Info */}
        <div className="lg:col-span-2 space-y-6">
          <div className="rounded-xl overflow-hidden h-80">
            <img src={hotel.imageUrl} alt={hotel.name} className="w-full h-full object-cover" />
          </div>

          <div>
            <div className="flex items-center gap-3 mb-2">
              <h1 className="text-3xl font-bold text-gray-900">{hotel.name}</h1>
              <span className="text-yellow-400 text-xl">{'★'.repeat(hotel.starRating)}</span>
            </div>
            <p className="text-gray-500 mb-4">📍 {hotel.address}, {hotel.city}, {hotel.country}</p>
            <p className="text-gray-700 leading-relaxed">{hotel.description}</p>
          </div>

          <div>
            <h2 className="text-xl font-semibold text-gray-800 mb-3">Amenities</h2>
            <div className="flex flex-wrap gap-2">
              {hotel.amenities.map(a => (
                <span key={a} className="px-3 py-1 bg-indigo-50 text-indigo-700 text-sm rounded-full">{a}</span>
              ))}
            </div>
          </div>

          <div>
            <h2 className="text-xl font-semibold text-gray-800 mb-3">Available Rooms</h2>
            <div className="space-y-3">
              {hotel.rooms.filter(r => r.isAvailable).map(room => (
                <div key={room.id} className="bg-white rounded-lg border border-gray-200 p-4 flex justify-between items-center">
                  <div>
                    <h3 className="font-semibold text-gray-800">{room.type}</h3>
                    <p className="text-sm text-gray-500">{room.description}</p>
                    <p className="text-xs text-gray-400 mt-1">Up to {room.maxGuests} guests</p>
                  </div>
                  <span className="text-xl font-bold text-indigo-700">€{room.pricePerNight}<span className="text-xs text-gray-400 font-normal">/night</span></span>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Booking Sidebar */}
        <div>
          <BookingForm hotelId={hotel.id} rooms={hotel.rooms} />
        </div>
      </div>
    </div>
  );
}
