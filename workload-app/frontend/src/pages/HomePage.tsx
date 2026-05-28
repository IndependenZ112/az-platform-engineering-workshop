import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { HotelSummary } from '../types';
import HotelCard from '../components/HotelCard';
import SearchBar from '../components/SearchBar';

export default function HomePage() {
  const [hotels, setHotels] = useState<HotelSummary[]>([]);
  const [loading, setLoading] = useState(true);

  const loadHotels = (filters?: Record<string, string>) => {
    setLoading(true);
    api.searchHotels(filters).then(setHotels).finally(() => setLoading(false));
  };

  useEffect(() => { loadHotels(); }, []);

  return (
    <div>
      <div className="text-center mb-8">
        <h1 className="text-4xl font-bold text-gray-900 mb-2">Find Your Perfect Stay</h1>
        <p className="text-gray-500">Discover handpicked hotels around the world</p>
      </div>

      <SearchBar onSearch={loadHotels} />

      {loading ? (
        <div className="text-center py-20 text-gray-400">Loading hotels...</div>
      ) : hotels.length === 0 ? (
        <div className="text-center py-20 text-gray-400">No hotels found. Try different filters.</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {hotels.map(h => <HotelCard key={h.id} hotel={h} />)}
        </div>
      )}
    </div>
  );
}
