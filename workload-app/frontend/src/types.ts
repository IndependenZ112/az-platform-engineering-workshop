export interface Hotel {
  id: string;
  name: string;
  description: string;
  city: string;
  country: string;
  address: string;
  latitude: number;
  longitude: number;
  starRating: number;
  imageUrl: string;
  amenities: string[];
  rooms: Room[];
}

export interface Room {
  id: string;
  hotelId: string;
  type: string;
  description: string;
  maxGuests: number;
  pricePerNight: number;
  isAvailable: boolean;
}

export interface HotelSummary {
  id: string;
  name: string;
  city: string;
  country: string;
  starRating: number;
  imageUrl: string;
  startingPrice: number;
  amenities: string[];
}

export interface Booking {
  id: string;
  hotelId: string;
  roomId: string;
  guestName: string;
  guestEmail: string;
  checkIn: string;
  checkOut: string;
  numberOfGuests: number;
  totalPrice: number;
  status: string;
  createdAt: string;
}

export interface BookingConfirmation {
  bookingId: string;
  hotelName: string;
  roomType: string;
  checkIn: string;
  checkOut: string;
  totalPrice: number;
  status: string;
}

export interface CreateBookingRequest {
  hotelId: string;
  roomId: string;
  guestName: string;
  guestEmail: string;
  checkIn: string;
  checkOut: string;
  numberOfGuests: number;
}
