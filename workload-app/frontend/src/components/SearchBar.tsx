import { useState } from 'react';

interface SearchBarProps {
  onSearch: (filters: Record<string, string>) => void;
}

export default function SearchBar({ onSearch }: SearchBarProps) {
  const [city, setCity] = useState('');
  const [minStars, setMinStars] = useState('');
  const [maxPrice, setMaxPrice] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const filters: Record<string, string> = {};
    if (city) filters.city = city;
    if (minStars) filters.minStars = minStars;
    if (maxPrice) filters.maxPrice = maxPrice;
    onSearch(filters);
  };

  return (
    <form onSubmit={handleSubmit} className="bg-white rounded-xl shadow-md p-6 mb-8">
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-600 mb-1">City</label>
          <input
            type="text"
            value={city}
            onChange={e => setCity(e.target.value)}
            placeholder="e.g. Barcelona"
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
          />
        </div>
        <div>
          <label htmlFor="min-stars" className="block text-sm font-medium text-gray-600 mb-1">Min Stars</label>
          <select
            id="min-stars"
            value={minStars}
            onChange={e => setMinStars(e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-500"
          >
            <option value="">Any</option>
            <option value="3">3+ Stars</option>
            <option value="4">4+ Stars</option>
            <option value="5">5 Stars</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-600 mb-1">Max Price (€/night)</label>
          <input
            type="number"
            value={maxPrice}
            onChange={e => setMaxPrice(e.target.value)}
            placeholder="e.g. 300"
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-500"
          />
        </div>
        <div className="flex items-end">
          <button
            type="submit"
            className="w-full bg-indigo-600 text-white rounded-lg px-4 py-2 text-sm font-medium hover:bg-indigo-700 transition"
          >
            🔍 Search Hotels
          </button>
        </div>
      </div>
    </form>
  );
}
