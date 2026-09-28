package com.smartsolar.microgrid.core.util

import com.google.zxing.BarcodeFormat
import com.google.zxing.BinaryBitmap
import com.google.zxing.MultiFormatReader
import com.google.zxing.MultiFormatWriter
import com.google.zxing.RGBLuminanceSource
import com.google.zxing.common.HybridBinarizer
import org.junit.Assert.assertEquals
import org.junit.Test

class QrPayloadRoundTripTest {
    @Test
    fun `encoded QR decodes to exact server transaction reference`() {
        val serverReference = "A1b2C3-server-reference"
        val size = 240
        val matrix = MultiFormatWriter().encode(serverReference, BarcodeFormat.QR_CODE, size, size)
        val pixels = IntArray(size * size) { index ->
            if (matrix[index % size, index / size]) 0xFF000000.toInt() else 0xFFFFFFFF.toInt()
        }
        val decoded = MultiFormatReader().decode(
            BinaryBitmap(HybridBinarizer(RGBLuminanceSource(size, size, pixels))),
        )

        assertEquals(serverReference, decoded.text)
        assertEquals(serverReference, OperatorPresentation.normalizeReference(decoded.text))
    }
}
