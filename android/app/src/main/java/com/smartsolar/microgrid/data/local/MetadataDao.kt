package com.smartsolar.microgrid.data.local

import android.content.ContentValues
import android.database.sqlite.SQLiteDatabase

class MetadataDao(
    private val databaseHelper: AppDatabaseHelper,
) {
    fun put(key: String, value: String, updatedAtEpochMillis: Long = System.currentTimeMillis()) {
        require(key.isNotBlank()) { "Metadata key must not be blank." }
        val values = ContentValues().apply {
            put(DatabaseContract.AppMetadata.COLUMN_KEY, key)
            put(DatabaseContract.AppMetadata.COLUMN_VALUE, value)
            put(DatabaseContract.AppMetadata.COLUMN_UPDATED_AT, updatedAtEpochMillis)
        }
        databaseHelper.writableDatabase.insertWithOnConflict(
            DatabaseContract.AppMetadata.TABLE_NAME,
            null,
            values,
            SQLiteDatabase.CONFLICT_REPLACE,
        )
    }

    fun get(key: String): MetadataEntry? {
        databaseHelper.readableDatabase.query(
            DatabaseContract.AppMetadata.TABLE_NAME,
            arrayOf(
                DatabaseContract.AppMetadata.COLUMN_VALUE,
                DatabaseContract.AppMetadata.COLUMN_UPDATED_AT,
            ),
            "${DatabaseContract.AppMetadata.COLUMN_KEY} = ?",
            arrayOf(key),
            null,
            null,
            null,
        ).use { cursor ->
            if (!cursor.moveToFirst()) return null
            return MetadataEntry(
                key = key,
                value = cursor.getString(
                    cursor.getColumnIndexOrThrow(DatabaseContract.AppMetadata.COLUMN_VALUE),
                ),
                updatedAtEpochMillis = cursor.getLong(
                    cursor.getColumnIndexOrThrow(DatabaseContract.AppMetadata.COLUMN_UPDATED_AT),
                ),
            )
        }
    }
}

data class MetadataEntry(
    val key: String,
    val value: String,
    val updatedAtEpochMillis: Long,
)

