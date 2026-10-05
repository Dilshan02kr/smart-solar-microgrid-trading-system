package com.smartsolar.microgrid

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import androidx.core.view.isVisible
import androidx.core.view.updatePadding
import androidx.navigation.fragment.NavHostFragment
import com.smartsolar.microgrid.databinding.ActivityMainBinding

class MainActivity : AppCompatActivity() {
    private lateinit var binding: ActivityMainBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        ViewCompat.setOnApplyWindowInsetsListener(binding.root) { view, windowInsets ->
            val insets = windowInsets.getInsets(
                WindowInsetsCompat.Type.systemBars() or WindowInsetsCompat.Type.displayCutout()
            )
            view.updatePadding(top = insets.top)
            windowInsets
        }
        ViewCompat.requestApplyInsets(binding.root)

        val navHost = supportFragmentManager.findFragmentById(R.id.nav_host_fragment) as NavHostFragment
        val navController = navHost.navController
        val rootDestinations = setOf(
            R.id.launchFragment,
            R.id.loginSelectionFragment,
            R.id.prosumerHomeFragment,
            R.id.operatorDashboardFragment,
        )
        binding.topAppBar.navigationContentDescription = getString(R.string.navigate_back)
        binding.topAppBar.setNavigationOnClickListener { navController.navigateUp() }
        navController.addOnDestinationChangedListener { _, destination, _ ->
            binding.topAppBar.isVisible = destination.id !in rootDestinations
            binding.topAppBar.title = destination.label
        }
    }
}

