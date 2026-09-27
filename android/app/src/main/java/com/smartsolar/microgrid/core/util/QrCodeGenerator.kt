package com.smartsolar.microgrid.core.util

import android.graphics.Bitmap
import com.google.zxing.BarcodeFormat
import com.google.zxing.EncodeHintType
import com.google.zxing.MultiFormatWriter

object QrCodeGenerator {
    fun create(
        payload: String,
        sizePixels: Int,
        foregroundColor: Int,
        backgroundColor: Int,
    ): Bitmap {
        require(payload.isNotEmpty())
        require(sizePixels > 0)
        val matrix = MultiFormatWriter().encode(
            payload,
            BarcodeFormat.QR_CODE,
            sizePixels,
            sizePixels,
            mapOf(EncodeHintType.MARGIN to 4),
        )
        val pixels = IntArray(sizePixels * sizePixels)
        for (y in 0 until sizePixels) {
            for (x in 0 until sizePixels) {
                pixels[y * sizePixels + x] = if (matrix[x, y]) foregroundColor else backgroundColor
            }
        }
        return Bitmap.createBitmap(pixels, sizePixels, sizePixels, Bitmap.Config.ARGB_8888)
    }
}
