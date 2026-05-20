import { MapPin, Phone, Mail } from 'lucide-react';
import type { VenueConfig } from '@/types/api';

interface VenueHeaderProps {
  config: VenueConfig;
}

export function VenueHeader({ config }: VenueHeaderProps) {
  return (
    <div className="bg-white border-b border-gray-200">
      <div className="max-w-4xl mx-auto px-4 py-6">
        <div className="flex items-start gap-4">
          {config.logoUrl && (
            <img
              src={config.logoUrl}
              alt={config.venueName}
              className="w-16 h-16 rounded-lg object-cover"
            />
          )}
          <div className="flex-1">
            <h1 className="text-2xl font-bold text-gray-900">
              {config.venueName}
            </h1>
            {config.description && (
              <p className="text-sm text-gray-600 mt-1">
                {config.description}
              </p>
            )}
            <div className="flex flex-wrap gap-4 mt-3 text-sm text-gray-500">
              {config.address && (
                <div className="flex items-center gap-1">
                  <MapPin className="w-4 h-4" />
                  <span>{config.address}</span>
                </div>
              )}
              {config.phone && (
                <div className="flex items-center gap-1">
                  <Phone className="w-4 h-4" />
                  <span>{config.phone}</span>
                </div>
              )}
              {config.email && (
                <div className="flex items-center gap-1">
                  <Mail className="w-4 h-4" />
                  <span>{config.email}</span>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
