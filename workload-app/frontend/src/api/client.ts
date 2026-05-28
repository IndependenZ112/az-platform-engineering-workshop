import type { HotelSummary, Hotel, Room, Booking, BookingConfirmation, CreateBookingRequest } from '../types';

const BASE = '/api';

async function fetchJson<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, init);
  if (!res.ok) throw new Error(`API error: ${res.status}`);
  return res.json();
}

export const api = {
  searchHotels: (params?: Record<string, string>) => {
    const qs = params ? '?' + new URLSearchParams(params).toString() : '';
    return fetchJson<HotelSummary[]>(`${BASE}/hotels${qs}`);
  },

  getHotel: (id: string) => fetchJson<Hotel>(`${BASE}/hotels/${id}`),

  getAvailableRooms: (hotelId: string, guests?: number) => {
    const qs = guests ? `?guests=${guests}` : '';
    return fetchJson<Room[]>(`${BASE}/hotels/${hotelId}/rooms${qs}`);
  },

  createBooking: (req: CreateBookingRequest) =>
    fetchJson<BookingConfirmation>(`${BASE}/bookings`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(req),
    }),

  getBooking: (id: string) => fetchJson<Booking>(`${BASE}/bookings/${id}`),

  getBookingsByEmail: (email: string) =>
    fetchJson<Booking[]>(`${BASE}/bookings/by-email/${encodeURIComponent(email)}`),

  cancelBooking: (id: string) =>
    fetch(`${BASE}/bookings/${id}`, { method: 'DELETE' }),
};
