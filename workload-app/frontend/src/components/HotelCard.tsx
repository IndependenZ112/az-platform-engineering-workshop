import { Link } from 'react-router-dom';
import type { HotelSummary } from '../types';

function Stars({ count }: { count: number }) {
  return <span className="text-yellow-400">{'★'.repeat(count)}{'☆'.repeat(5 - count)}</span>;
}

export default function HotelCard({ hotel }: { hotel: HotelSummary }) {
  return (
    <Link to={`/hotels/${hotel.id}`} className="group bg-white rounded-xl shadow-md overflow-hidden hover:shadow-xl transition-shadow">
      <div className="h-48 overflow-hidden">
        <img
          src={hotel.imageUrl}
          alt={hotel.name}
          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
        />
      </div>
      <div className="p-4">
        <div className="flex items-center justify-between mb-1">
          <h3 className="text-lg font-semibold text-gray-900">{hotel.name}</h3>
          <Stars count={hotel.starRating} />
        </div>
        <p className="text-sm text-gray-500 mb-3">📍 {hotel.city}, {hotel.country}</p>
        <div className="flex flex-wrap gap-1 mb-3">
          {hotel.amenities.slice(0, 4).map(a => (
            <span key={a} className="px-2 py-0.5 bg-indigo-50 text-indigo-700 text-xs rounded-full">{a}</span>
          ))}
          {hotel.amenities.length > 4 && (
            <span className="px-2 py-0.5 bg-gray-100 text-gray-500 text-xs rounded-full">+{hotel.amenities.length - 4}</span>
          )}
        </div>
        <div className="flex items-center justify-between">
          <span className="text-xl font-bold text-indigo-700">€{hotel.startingPrice}</span>
          <span className="text-xs text-gray-400">per night</span>
        </div>
      </div>
    </Link>
  );
}
