import { importLibrary, setOptions } from '@googlemaps/js-api-loader'
import { useEffect, useRef, useState } from 'react'
import { Alert } from '@/components/feedback/Alert'
import { Spinner } from '@/components/feedback/Spinner'

interface StationLocationPickerProps {
  latitude: string
  longitude: string
  disabled?: boolean
  onSelect: (latitude: number, longitude: number) => void
}

const SRI_LANKA_CENTER = { lat: 7.8731, lng: 80.7718 }
let loaderConfigured = false

function selectedCoordinate(latitude: string, longitude: string): google.maps.LatLngLiteral | null {
  const trimmedLat = latitude?.trim()
  const trimmedLng = longitude?.trim()
  if (!trimmedLat || !trimmedLng) return null

  const lat = Number(trimmedLat)
  const lng = Number(trimmedLng)
  return Number.isFinite(lat) && lat >= -90 && lat <= 90 && Number.isFinite(lng) && lng >= -180 && lng <= 180
    ? { lat, lng }
    : null
}

export function StationLocationPicker({ latitude, longitude, disabled = false, onSelect }: StationLocationPickerProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const mapRef = useRef<google.maps.Map | null>(null)
  const markerRef = useRef<google.maps.Marker | null>(null)
  const markerConstructorRef = useRef<typeof google.maps.Marker | null>(null)
  const onSelectRef = useRef(onSelect)
  const disabledRef = useRef(disabled)
  const [status, setStatus] = useState<'loading' | 'ready' | 'error'>('loading')
  const apiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY?.trim()

  useEffect(() => { onSelectRef.current = onSelect }, [onSelect])
  useEffect(() => { disabledRef.current = disabled }, [disabled])

  useEffect(() => {
    if (!apiKey || !containerRef.current) return
    let disposed = false

    async function initialize() {
      try {
        if (!loaderConfigured) {
          setOptions({ key: apiKey, v: 'weekly' })
          loaderConfigured = true
        }
        const [{ Map }, { Marker }] = await Promise.all([importLibrary('maps'), importLibrary('marker')])
        if (disposed || !containerRef.current) return
        const initial = selectedCoordinate(latitude, longitude)
        const map = new Map(containerRef.current, {
          center: initial ?? SRI_LANKA_CENTER,
          zoom: initial ? 14 : 7,
          streetViewControl: false,
          mapTypeControl: false,
          fullscreenControl: true,
        })
        mapRef.current = map
        markerConstructorRef.current = Marker

        const placeMarker = (position: google.maps.LatLngLiteral) => {
          if (!markerRef.current) {
            markerRef.current = new Marker({ map, position, draggable: true, title: 'Selected station location' })
            markerRef.current.addListener('dragend', () => {
              const moved = markerRef.current?.getPosition()
              if (moved) onSelectRef.current(moved.lat(), moved.lng())
            })
          } else {
            markerRef.current.setPosition(position)
          }
        }

        if (initial) placeMarker(initial)
        map.addListener('click', (event: google.maps.MapMouseEvent) => {
          if (disabledRef.current || !event.latLng) return
          const position = { lat: event.latLng.lat(), lng: event.latLng.lng() }
          placeMarker(position)
          onSelectRef.current(position.lat, position.lng)
        })
        setStatus('ready')
      } catch {
        if (!disposed) setStatus('error')
      }
    }

    void initialize()
    return () => {
      disposed = true
      markerRef.current?.setMap(null)
      markerRef.current = null
      mapRef.current = null
      markerConstructorRef.current = null
    }
    // The map is initialized once; controlled-coordinate updates are handled below.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [apiKey])

  useEffect(() => {
    if (status !== 'ready' || !mapRef.current || !markerConstructorRef.current) return
    const coordinate = selectedCoordinate(latitude, longitude)
    if (!coordinate) {
      markerRef.current?.setMap(null)
      markerRef.current = null
      return
    }
    if (!markerRef.current) {
      markerRef.current = new markerConstructorRef.current({ map: mapRef.current, position: coordinate, draggable: !disabled, title: 'Selected station location' })
      markerRef.current.addListener('dragend', () => {
        const moved = markerRef.current?.getPosition()
        if (moved) onSelectRef.current(moved.lat(), moved.lng())
      })
    } else {
      markerRef.current.setPosition(coordinate)
      markerRef.current.setDraggable(!disabled)
    }
  }, [disabled, latitude, longitude, status])

  if (!apiKey) {
    return <Alert variant="info" title="Map selection unavailable">Enter latitude and longitude manually. Configure VITE_GOOGLE_MAPS_API_KEY to enable map selection.</Alert>
  }

  return (
    <section className="station-location-picker" aria-label="Station location map picker">
      <div ref={containerRef} className="station-location-picker__map" aria-label="Select station coordinates on the map" />
      {status === 'loading' && <div className="station-location-picker__overlay"><Spinner label="Loading location map" /></div>}
      {status === 'error' && <div className="station-location-picker__overlay"><Alert variant="warning" title="Map selection unavailable">Enter latitude and longitude manually.</Alert></div>}
      {status === 'ready' && <p className="station-location-picker__help">Click the map or drag the marker to update the coordinates. Location name is entered separately.</p>}
    </section>
  )
}
