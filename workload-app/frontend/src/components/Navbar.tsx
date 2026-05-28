import { Link } from 'react-router-dom';

export default function Navbar() {
  return (
    <nav className="bg-indigo-700 text-white shadow-lg">
      <div className="max-w-7xl mx-auto px-4 py-3 flex items-center justify-between">
        <Link to="/" className="text-2xl font-bold tracking-tight flex items-center gap-2">
          <span className="text-3xl">🏨</span> StayBright Hotels
        </Link>
        <div className="flex gap-6 text-sm font-medium">
          <Link to="/" className="hover:text-indigo-200 transition">Hotels</Link>
          <Link to="/my-bookings" className="hover:text-indigo-200 transition">My Bookings</Link>
        </div>
      </div>
    </nav>
  );
}
