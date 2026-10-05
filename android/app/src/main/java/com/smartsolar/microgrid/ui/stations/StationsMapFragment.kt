package com.smartsolar.microgrid.ui.stations

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.core.os.bundleOf
import androidx.core.view.doOnLayout
import androidx.core.view.isVisible
import androidx.fragment.app.Fragment
import androidx.fragment.app.viewModels
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import androidx.navigation.fragment.findNavController
import com.google.android.gms.maps.CameraUpdateFactory
import com.google.android.gms.maps.GoogleMap
import com.google.android.gms.maps.OnMapReadyCallback
import com.google.android.gms.maps.SupportMapFragment
import com.google.android.gms.maps.model.BitmapDescriptorFactory
import com.google.android.gms.maps.model.LatLng
import com.google.android.gms.maps.model.LatLngBounds
import com.google.android.gms.maps.model.MarkerOptions
import com.smartsolar.microgrid.BuildConfig
import com.smartsolar.microgrid.R
import com.smartsolar.microgrid.SmartSolarApplication
import com.smartsolar.microgrid.core.util.StationMapPresentation
import com.smartsolar.microgrid.core.util.StationMarkerModel
import com.smartsolar.microgrid.databinding.FragmentStationsMapBinding
import com.smartsolar.microgrid.domain.model.StationStatus
import com.smartsolar.microgrid.ui.common.AppViewModelFactory
import kotlinx.coroutines.launch

class StationsMapFragment : Fragment(), OnMapReadyCallback {
    private var binding: FragmentStationsMapBinding? = null
    private var map: GoogleMap? = null
    private var content: StationsMapUiState.Content? = null
    private var cameraPositioned = false
    private val focusStationId by lazy { arguments?.getString(ARG_FOCUS_STATION_ID) }
    private val viewModel: StationsMapViewModel by viewModels {
        val app = (requireActivity().application as SmartSolarApplication).appContainer
        AppViewModelFactory { StationsMapViewModel(app.stationRepository) }
    }

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, state: Bundle?): View =
        FragmentStationsMapBinding.inflate(inflater, container, false).also { binding = it }.root

    override fun onViewCreated(view: View, state: Bundle?) {
        binding?.retryButton?.setOnClickListener { viewModel.load() }
        if (BuildConfig.MAPS_API_KEY_CONFIGURED) initializeMap() else showConfigurationError()
        viewLifecycleOwner.lifecycleScope.launch {
            viewLifecycleOwner.repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.state.collect(::render)
            }
        }
    }

    private fun initializeMap() {
        try {
            val mapFragment = SupportMapFragment.newInstance()
            childFragmentManager.beginTransaction()
                .replace(R.id.map_container, mapFragment)
                .commit()
            mapFragment.getMapAsync(this)
        } catch (_: RuntimeException) {
            showMapInitializationError()
        }
    }

    override fun onMapReady(googleMap: GoogleMap) {
        if (binding == null) return
        map = googleMap.apply {
            uiSettings.isMapToolbarEnabled = false
            uiSettings.isMyLocationButtonEnabled = false
            setOnInfoWindowClickListener { marker ->
                (marker.tag as? String)?.let(::openStation)
            }
        }
        renderMarkers()
    }

    private fun render(state: StationsMapUiState) {
        val current = binding ?: return
        current.progressIndicator.isVisible = state is StationsMapUiState.Loading
        current.errorContainer.isVisible = state is StationsMapUiState.Error
        if (state is StationsMapUiState.Error) {
            current.errorMessage.text = state.error.message
            if (state.error.httpStatus == 401) {
                findNavController().navigate(R.id.action_global_loginSelectionFragment)
            }
        }
        if (state is StationsMapUiState.Content) {
            content = state
            current.noStationsMessage.isVisible = state.markers.isEmpty()
            current.coordinateNotice.isVisible = state.skippedStationCount > 0
            current.coordinateNotice.text = resources.getQuantityString(
                R.plurals.invalid_station_coordinates_notice,
                state.skippedStationCount,
                state.skippedStationCount,
            )
            renderMarkers()
        }
    }

    private fun renderMarkers() {
        val googleMap = map ?: return
        val mapContent = content ?: return
        googleMap.clear()
        mapContent.markers.forEach { markerModel ->
            googleMap.addMarker(
                MarkerOptions()
                    .position(LatLng(markerModel.latitude, markerModel.longitude))
                    .title(markerModel.name)
                    .snippet(
                        getString(
                            R.string.station_marker_snippet,
                            markerModel.locationName,
                            markerModel.status.name,
                            markerModel.totalCapacityKw,
                        ),
                    )
                    .icon(
                        BitmapDescriptorFactory.defaultMarker(
                            if (markerModel.status == StationStatus.ACTIVE) {
                                BitmapDescriptorFactory.HUE_GREEN
                            } else {
                                BitmapDescriptorFactory.HUE_ORANGE
                            },
                        ),
                    ),
            )?.tag = markerModel.stationId
        }
        positionCamera(mapContent.markers)
    }

    private fun positionCamera(markers: List<StationMarkerModel>) {
        if (cameraPositioned) return
        val current = binding ?: return
        current.mapContainer.doOnLayout {
            val googleMap = map ?: return@doOnLayout
            val focus = StationMapPresentation.focusedMarker(markers, focusStationId)
            when {
                focus != null -> googleMap.moveCamera(
                    CameraUpdateFactory.newLatLngZoom(LatLng(focus.latitude, focus.longitude), FOCUSED_ZOOM),
                )
                markers.size == 1 -> googleMap.moveCamera(
                    CameraUpdateFactory.newLatLngZoom(
                        LatLng(markers.first().latitude, markers.first().longitude),
                        SINGLE_MARKER_ZOOM,
                    ),
                )
                markers.size > 1 -> {
                    val bounds = LatLngBounds.builder().apply {
                        markers.forEach { include(LatLng(it.latitude, it.longitude)) }
                    }.build()
                    googleMap.moveCamera(CameraUpdateFactory.newLatLngBounds(bounds, MAP_PADDING_PX))
                }
                else -> googleMap.moveCamera(
                    CameraUpdateFactory.newLatLngZoom(SRI_LANKA_CENTER, SRI_LANKA_ZOOM),
                )
            }
            cameraPositioned = true
        }
    }

    private fun showConfigurationError() {
        val current = binding ?: return
        current.mapContainer.isVisible = false
        current.progressIndicator.isVisible = false
        current.configurationMessage.isVisible = true
        current.configurationMessage.setText(R.string.map_configuration_required)
    }

    private fun showMapInitializationError() {
        val current = binding ?: return
        current.mapContainer.isVisible = false
        current.progressIndicator.isVisible = false
        current.configurationMessage.isVisible = true
        current.configurationMessage.setText(R.string.map_unavailable)
    }

    private fun openStation(stationId: String) {
        if (findNavController().currentDestination?.id != R.id.stationsMapFragment) return
        findNavController().navigate(
            R.id.action_stationsMapFragment_to_stationDetailsFragment,
            bundleOf(StationsFragment.ARG_STATION_ID to stationId),
        )
    }

    override fun onDestroyView() {
        map = null
        content = null
        binding = null
        super.onDestroyView()
    }

    companion object {
        const val ARG_FOCUS_STATION_ID = "focusStationId"
        private val SRI_LANKA_CENTER = LatLng(7.8731, 80.7718)
        private const val SRI_LANKA_ZOOM = 6.7f
        private const val FOCUSED_ZOOM = 14.5f
        private const val SINGLE_MARKER_ZOOM = 12f
        private const val MAP_PADDING_PX = 96
    }
}
